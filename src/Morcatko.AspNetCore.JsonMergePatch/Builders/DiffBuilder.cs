using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Morcatko.AspNetCore.JsonMergePatch.Builders
{
	public static class DiffBuilder
	{
		public static JObject Build<TModel>(TModel original, TModel patched) where TModel : class
			=> Build(JObject.FromObject(original), JObject.FromObject(patched));

		public static JObject Build(JObject original, JObject patched)
			=> (JObject)BuildDiff(original, patched);

		private static JToken BuildDiff(JToken original, JToken patched)
		{
			var originalIsNull = (original == null) || (original.Type == JTokenType.Null);
			var patchedIsNull = (patched == null) || (patched.Type == JTokenType.Null);

			if (originalIsNull && patchedIsNull)
				return null;
			else if (originalIsNull)
				return patched.DeepClone();
			else if (patchedIsNull)
				return JValue.CreateNull();
			else if ((original is JArray) || (patched is JArray))
				return BuildArrayDiff(original as JArray, patched as JArray);
			else
			{
				switch (original)
				{
					case JValue originalValue:
						return BuildValueDiff(originalValue, patched as JValue);
					case JObject originalObject:
						return BuildObjectDiff(originalObject, patched as JObject);
					default:
						throw new NotImplementedException();
				}
			}
		}

		private static JToken BuildObjectDiff(JObject original, JObject patched)
		{
			JObject result = new JObject();
			var properties = original?.Properties() ?? patched.Properties();
			foreach (var property in properties)
			{
				var propertyName = property.Name;
				var originalJToken = original?.GetValue(propertyName);
				var patchedJToken = patched?.GetValue(propertyName);

				var patchToken = BuildDiff(originalJToken, patchedJToken);
				if (patchToken != null)
					result.Add(propertyName, patchToken);
			}

			if (result.Properties().Any())
				return result;
			return null;
		}

		private static JValue BuildValueDiff(JValue original, JValue patched)
		{
			if (((original.Value != null) && !original.Value.Equals(patched.Value))
				|| (patched.Value != null) && !patched.Value.Equals(original?.Value))
				return patched.DeepClone() as JValue;

			return null;
		}

		private static JToken BuildArrayDiff(JArray original, JArray patched)
		{
			bool JArrayEquals(JArray left, JArray right)
			{
				if (left.Count != right.Count)
					return false;
				for (int i = 0; i < original.Count; i++)
				{
					//Hack.
					//Array can consist of values, objects or arrays so we reuse logic to calculate diff for each item
					//if there is any patch operation (aka any diff) we return false and replace whole array
					var diff = BuildDiff(left[i], right[i]);
					if (diff != null)
						return false;
				}
				return true;
			}

			if (JArrayEquals(original, patched))
				return null;
			else
				return patched.DeepClone();
		}
	}
}
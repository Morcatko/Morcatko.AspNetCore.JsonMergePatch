using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;

namespace Morcatko.AspNetCore.JsonMergePatch.Builder
{
	internal class PatchBuilder<TModel> where TModel : class
	{
		public JsonMergePatchDocument<TModel> Build(TModel original, TModel patched)
		{
			var patchDocument = new JsonMergePatchDocument<TModel>();
			BuildObjectDiff(
				patchDocument,
				string.Empty,
				JObject.FromObject(original),
				JObject.FromObject(patched));
			return patchDocument;
		}

		public JsonMergePatchDocument<TModel> Build(JObject jsonObjectPatch)
		{
			var patchDocument = new JsonMergePatchDocument<TModel>(jsonObjectPatch.ToObject<TModel>());
			BuildObjectDiff(
				patchDocument,
				string.Empty,
				null,
				jsonObjectPatch);
			return patchDocument;
		}

		public JsonMergePatchDocument<TModel> Build(string jsonObject)
			=> Build(JObject.Parse(jsonObject));



		private static void BuildDiff(JsonMergePatchDocument<TModel> patchDocument, string propertyPath, JToken originalJToken, JToken patchedJToken)
		{
			if ((originalJToken is JArray) || (patchedJToken is JArray))
				BuildArrayDiff(patchDocument, propertyPath, originalJToken as JArray, patchedJToken as JArray);
			else if (originalJToken != null)
			{
				switch (originalJToken)
				{
					case JValue originalValue:
						BuildValueDiff(patchDocument, propertyPath, originalValue, patchedJToken as JValue);
						break;
					case JObject originalObject:
						BuildObjectDiff(patchDocument, propertyPath, originalObject, patchedJToken as JObject);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				switch (patchedJToken)
				{
					case JValue patchedValue:
						BuildValueDiff(patchDocument, propertyPath, JValue.CreateString(patchedValue.Value + "_dummy"), patchedValue);	//workaround when patchedValue is null
						break;
					case JObject patchedObject:
						BuildObjectDiff(patchDocument, propertyPath, originalJToken as JObject, patchedObject);
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}

		private static void BuildObjectDiff(JsonMergePatchDocument<TModel> patchDocument, string path, JObject original, JObject patched)
		{
			if (patched == null)
			{
				patchDocument.AddPatch(path, null);
				return;
			}

			var properties = original?.Properties() ?? patched.Properties();

			foreach (var property in properties)
			{
				var propertyName = property.Name;
				var propertyPath = path + "/" + propertyName;
				var originalJToken = original?.GetValue(propertyName);
				var patchedJToken = patched.GetValue(propertyName);

				BuildDiff(patchDocument, propertyPath, originalJToken, patchedJToken);
			}
		}

		private static void BuildValueDiff(JsonMergePatchDocument<TModel> patchDocument, string path, JValue original, JValue patched)
		{
			if (((original?.Value != null) && !original.Value.Equals(patched.Value))
				|| (patched.Value != null) && !patched.Value.Equals(original?.Value))
				patchDocument.AddPatch(path, patched.Value);
		}

		private static void BuildArrayDiff(JsonMergePatchDocument<TModel> patchDocument, string path, JArray original, JArray patched)
		{
			bool JArrayEquals(JArray left, JArray right)
			{
				if ((right == null) || (left.Count != right.Count))
					return false;
				for (int i = 0; i < original.Count; i++)
				{
					//Hack.
					//Array can consist of values, objects or arrays so we reuse logic to calculate diff for each item
					//if there is any patch operation (aka any diff) we return false and replace whole array
					var _patchDocument = new JsonMergePatchDocument<TModel>();
					BuildDiff(_patchDocument, string.Empty, left[i], right[i]);
					if (_patchDocument.JsonPatchDocument.Operations.Any())
						return false;
				}
				return true;
			}

			if (patched == null)
				patchDocument.AddPatch(path, null);
			else if (((original != null) && !JArrayEquals(original, patched))
				|| (patched != null) && !JArrayEquals(patched, original))
				patchDocument.AddPatch(path, patched.ToObject<ArrayList>());
		}
	}
}
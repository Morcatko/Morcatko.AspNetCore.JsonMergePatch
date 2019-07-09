using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	static class ReflectionHelper
	{
		private static char[] pathSplitter = new[] { '/' };

		private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
			=> type.GetProperties().Single(property => property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

		internal static Type GetPropertyTypeFromPath(Type type, string path, IContractResolver contractResolver)
		{
			var currentType = type;
			foreach (var propertyName in path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries))
			{
				var jsonContract = contractResolver.ResolveContract(currentType);
				if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
				{
					currentType = jsonDictionaryContract.DictionaryValueType;
					continue;
				}
				var currentProperty = GetPropertyInfo(currentType, propertyName);
				currentType = currentProperty.PropertyType;
			}
			return currentType;
		}

		private static bool Exist(object value, IEnumerable<string> paths, IContractResolver contractResolver)
		{
			if (value == null)
				return false;

			var currentPath = paths.FirstOrDefault();
			if (currentPath == null)
				return value != null;

			object currentValue;

			var jsonContract = contractResolver.ResolveContract(value.GetType());
			if (jsonContract is JsonDictionaryContract)
			{
				try
				{
					currentValue = value
							.GetType()
							.GetProperty("Item")
							.GetValue(value, new[] { currentPath });
				}
				catch (Exception)
				{
					return false;
				}

			}
			else
			{
				currentValue = GetPropertyInfo(value.GetType(), currentPath).GetValue(value);
			}


			return Exist(currentValue, paths.Skip(1), contractResolver);
		}

		internal static bool Exist(object value, string path, IContractResolver contractResolver)
			=> Exist(value, path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries), contractResolver);
	}
}

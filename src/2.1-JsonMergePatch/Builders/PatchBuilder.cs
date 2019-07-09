
using Morcatko.AspNetCore.JsonMergePatch.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Morcatko.AspNetCore.JsonMergePatch.Builder
{
	public class PatchBuilder<TModel> where TModel : class
	{
		public JsonMergePatchDocument<TModel> Build(TModel original, TModel patched) => PatchBuilder.Build<TModel>(original, patched);
		public JsonMergePatchDocument<TModel> Build(string jsonObjectPatch) => PatchBuilder.Build<TModel>(jsonObjectPatch);
		public JsonMergePatchDocument<TModel> Build(object jsonObjectPatch) => PatchBuilder.Build<TModel>(jsonObjectPatch);
		public JsonMergePatchDocument<TModel> Build(JObject jsonObjectPatch) => PatchBuilder.Build<TModel>(jsonObjectPatch);
	}

	public static class PatchBuilder
	{
		private static readonly JsonSerializer defaultSerializer = JsonSerializer.CreateDefault();

		#region Static methods
		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched, JsonMergePatchOptions options = null) where TModel : class
			=> Build<TModel>(DiffBuilder.Build(original, patched) ?? new JObject(), options);

		public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObjectPatch, JsonMergePatchOptions options = null) where TModel : class
			=> Build<TModel>(JObject.Parse(jsonObjectPatch), options);

		public static JsonMergePatchDocument<TModel> Build<TModel>(object jsonObjectPatch, JsonMergePatchOptions options = null) where TModel : class
			=> Build<TModel>(JObject.FromObject(jsonObjectPatch), options);

		public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObjectPatch, JsonMergePatchOptions options = null) where TModel : class
			=> CreatePatchDocument<TModel>(jsonObjectPatch, defaultSerializer, options ?? new JsonMergePatchOptions());
		#endregion

		#region PatchCreation methods
		private static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JObject patchObject, JsonSerializer jsonSerializer, JsonMergePatchOptions options) where TModel : class
			=> CreatePatchDocument(typeof(JsonMergePatchDocument<TModel>), typeof(TModel), patchObject, jsonSerializer, options) as JsonMergePatchDocument<TModel>;

		private static void AddOperation(JsonMergePatchDocument jsonMergePatchDocument, string pathPrefix, JObject patchObject, JsonMergePatchOptions options)
		{
			foreach (var jProperty in patchObject)
			{
				var path = pathPrefix + jProperty.Key;
				if (jProperty.Value is JValue jValue)
				{
					if (options.EnableDelete && jValue.Value == null)
						jsonMergePatchDocument.AddOperation_Remove(path);
					else 
						jsonMergePatchDocument.AddOperation_Replace(path, jValue.Value);
				}
				else if (jProperty.Value is JArray jArray)
					jsonMergePatchDocument.AddOperation_Replace(path, jArray);
				else if (jProperty.Value is JObject jObject)
				{
					jsonMergePatchDocument.AddOperation_Add(path);
					AddOperation(jsonMergePatchDocument, path + "/", jObject, options);
				}
			}
		}

		internal static JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject patchObject, JsonSerializer jsonSerializer, JsonMergePatchOptions options)
		{
			var model = patchObject.ToObject(modelType, jsonSerializer);
			var jsonMergePatchDocument = (JsonMergePatchDocument)Activator.CreateInstance(jsonMergePatchType, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { model }, null);
			AddOperation(jsonMergePatchDocument, "/", patchObject, options);
			return jsonMergePatchDocument;
		}
		#endregion

	}
}
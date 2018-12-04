
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
        public JsonMergePatchDocument<TModel> Build(JObject jsonObjectPatch) => PatchBuilder.Build<TModel>(jsonObjectPatch);
    }

    public static class PatchBuilder
    {
        private static readonly JsonSerializer defaultSerializer = JsonSerializer.CreateDefault();

        #region Static methods
        public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
            => Build<TModel>(DiffBuilder.Build(original, patched));

        public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObjectPatch) where TModel : class
            => Build<TModel>(JObject.Parse(jsonObjectPatch));

        public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObjectPatch) where TModel : class
            => CreatePatchDocument<TModel>(jsonObjectPatch, defaultSerializer);
        #endregion

        #region PatchCreation methods
        private static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JObject jObject, JsonSerializer jsonSerializer) where TModel : class
            => CreatePatchDocument(typeof(JsonMergePatchDocument<TModel>), typeof(TModel), jObject, jsonSerializer) as JsonMergePatchDocument<TModel>;

        private static void AddOperation(JsonMergePatchDocument jsonMergePatchDocument, string pathPrefix, JObject jObject)
        {
            foreach (var jProperty in jObject)
            {
                var path = pathPrefix + jProperty.Key;
                if (jProperty.Value is JValue)
                    jsonMergePatchDocument.AddPatch(path, ((JValue)jProperty.Value).Value);
                else if (jProperty.Value is JArray)
                    jsonMergePatchDocument.AddPatch(path, ((JArray)jProperty.Value));
                else if (jProperty.Value is JObject)
                {
                    jsonMergePatchDocument.AddObject(path);
                    AddOperation(jsonMergePatchDocument, path + "/", (jProperty.Value as JObject));
                }
            }
        }

        internal static JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject jObject, JsonSerializer jsonSerializer)
        {
            var model = jObject.ToObject(modelType, jsonSerializer);
            var jsonMergePatchDocument = (JsonMergePatchDocument)Activator.CreateInstance(jsonMergePatchType, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { model }, null);
            AddOperation(jsonMergePatchDocument, "/", jObject);
            return jsonMergePatchDocument;
        }
        #endregion

    }
}
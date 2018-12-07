
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
        public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched, JsonMergePatchOptions options = null) where TModel : class
            => Build<TModel>(DiffBuilder.Build(original, patched), options);

        public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObjectPatch, JsonMergePatchOptions options = null) where TModel : class
            => Build<TModel>(JObject.Parse(jsonObjectPatch), options);

        public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObjectPatch, JsonMergePatchOptions options = null) where TModel : class
            => CreatePatchDocument<TModel>(jsonObjectPatch, defaultSerializer, options ?? new JsonMergePatchOptions());
        #endregion

        #region PatchCreation methods
        private static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JObject jObject, JsonSerializer jsonSerializer, JsonMergePatchOptions options) where TModel : class
            => CreatePatchDocument(typeof(JsonMergePatchDocument<TModel>), typeof(TModel), jObject, jsonSerializer, options) as JsonMergePatchDocument<TModel>;

        private static void AddOperation(JsonMergePatchDocument jsonMergePatchDocument, string pathPrefix, JObject jObject, JsonMergePatchOptions options)
        {
            foreach (var jProperty in jObject)
            {
                var path = pathPrefix + jProperty.Key;
                if (jProperty.Value is JValue jValue)
                {
                    if (options.EnableDelete && jValue.Value ==  null)
                    {
                        jsonMergePatchDocument.Delete(path);
                        continue;
                    }
                    jsonMergePatchDocument.AddPatch(path, jValue.Value);
                }
                else if (jProperty.Value is JArray jArray)
                    jsonMergePatchDocument.AddPatch(path, jArray);
                else if (jProperty.Value is JObject @object)
                {

                    jsonMergePatchDocument.AddObject(path);
                    AddOperation(jsonMergePatchDocument, path + "/", @object, options);
                }
            }
        }

        internal static JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject jObject, JsonSerializer jsonSerializer, JsonMergePatchOptions options)
        {
            var model = jObject.ToObject(modelType, jsonSerializer);
            var jsonMergePatchDocument = (JsonMergePatchDocument)Activator.CreateInstance(jsonMergePatchType, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { model }, null);
            AddOperation(jsonMergePatchDocument, "/", jObject, options);
            return jsonMergePatchDocument;
        }
        #endregion

    }
}
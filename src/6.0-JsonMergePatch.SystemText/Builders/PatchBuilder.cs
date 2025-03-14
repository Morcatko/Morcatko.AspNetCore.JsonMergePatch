using Morcatko.AspNetCore.JsonMergePatch.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText.Builders
{
    public static class PatchBuilder<TModel> where TModel : class
    {
        public static JsonMergePatchDocument<TModel> Build(TModel original, TModel patched, JsonMergePatchOptions options = null)
        {
            var diff = DiffBuilder.Build(original, patched);
            var jsonElement = diff != null ? diff.RootElement.Clone() : JsonDocument.Parse("{}").RootElement;
            return PatchBuilder.CreatePatchDocument<TModel>(jsonElement, options);
        }

        public static JsonMergePatchDocument<TModel> Build(string jsonObjectPatch, JsonSerializerOptions jsonOptions = null, JsonMergePatchOptions options = null)
        {
            var jsonElement = JsonDocument.Parse(jsonObjectPatch).RootElement;
            return PatchBuilder.CreatePatchDocument<TModel>(jsonElement, jsonOptions ?? new JsonSerializerOptions(), options ?? new JsonMergePatchOptions());
        }

        public static JsonMergePatchDocument<TModel> Build(object jsonObjectPatch, JsonMergePatchOptions options = null)
        {
            var json = JsonSerializer.Serialize(jsonObjectPatch);
            var jsonElement = JsonDocument.Parse(json).RootElement;
            return PatchBuilder.CreatePatchDocument<TModel>(jsonElement, options);
        }

        public static JsonMergePatchDocument<TModel> Build(JsonElement jsonObjectPatch, JsonMergePatchOptions options = null)
        {
            return PatchBuilder.CreatePatchDocument<TModel>(jsonObjectPatch, options);
        }
    }

    public static class PatchBuilder
    {
        private static Type GetEnumeratedType(this Type type)
        {
            if (type is null)
            {
                return null;
            }

            var elType = type.GetElementType();
            if (elType is not null)
            {
                return elType;
            }

            var elTypes = type.GetGenericArguments();
            if (elTypes.Length > 0)
            {
                return elTypes[0];
            }

            return null;
        }

        private static object ToObject(
            this JsonElement jsonElement,
            Type propertyType,
            JsonSerializerOptions jsonOptions)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Null: return null;
                case JsonValueKind.String: return jsonElement.GetString();
                case JsonValueKind.Number: return jsonElement.GetGenericNumber();
                case JsonValueKind.Array:
                    var elementType = propertyType?.GetEnumeratedType();
                    return jsonElement.EnumerateArray().Select(j => j.ToObject(elementType, jsonOptions)).ToArray();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Object:
                    return propertyType is null ? jsonElement : jsonElement.Deserialize(propertyType, jsonOptions);
                case JsonValueKind.Undefined:
                default:
                    throw new NotSupportedException($"Unsupported ValueKind - {jsonElement.ValueKind}");
            }
        }

        private static object GetGenericNumber(this JsonElement jsonElement)
        {

            // Attempt to parse the JSON Element as an Int32 first
            if (jsonElement.TryGetInt32(out int int32)) return int32;

            // Failing that, parse it as a Decimal instead
            return jsonElement.GetDecimal();

        }

        private static bool IsValue(this JsonValueKind valueKind)
            => (valueKind == JsonValueKind.False)
            || (valueKind == JsonValueKind.True)
            || (valueKind == JsonValueKind.Number)
            || (valueKind == JsonValueKind.String);

        private static void AddOperation(
            IInternalJsonMergePatchDocument jsonMergePatchDocument,
            string pathPrefix,
            JsonElement jsonElement,
            JsonMergePatchOptions options,
            Type modelType,
            JsonSerializerOptions jsonOptions)
        {
            var enumerator = jsonElement.EnumerateObject();
            while (enumerator.MoveNext())
            {
                var jsonProp = enumerator.Current;
                var propertyType = modelType?.GetProperty(jsonProp.Name)?.PropertyType;
                var jsonValue = jsonProp.Value;
                var path = pathPrefix + jsonProp.Name;
                if (jsonValue.ValueKind.IsValue() || (jsonValue.ValueKind == JsonValueKind.Null))
                {
                    if (options.EnableDelete && (jsonValue.ValueKind == JsonValueKind.Null))
                        jsonMergePatchDocument.AddOperation_Remove(path);
                    else
                        jsonMergePatchDocument.AddOperation_Replace(path, jsonValue.ToObject(propertyType, jsonOptions));
                }
                else if (jsonValue.ValueKind == JsonValueKind.Array)
                    jsonMergePatchDocument.AddOperation_Replace(path, jsonValue.ToObject(propertyType, jsonOptions));
                else if (jsonValue.ValueKind == JsonValueKind.Object)
                {
                    jsonMergePatchDocument.AddOperation_Add(path);
                    AddOperation(jsonMergePatchDocument, path + "/", jsonValue, options, propertyType, jsonOptions);
                }
            }
        }

        static readonly Type internalJsonMergePatchDocumentType = typeof(InternalJsonMergePatchDocument<>);

        internal static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JsonElement patchObject, JsonMergePatchOptions options = null) where TModel : class
        {
            return CreatePatchDocument(typeof(TModel), patchObject, new JsonSerializerOptions(), options ?? new JsonMergePatchOptions()) as JsonMergePatchDocument<TModel>;
        }

        internal static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JsonElement patchObject, JsonSerializerOptions jsonOptions, JsonMergePatchOptions mergePatchOptions) where TModel : class
        {
            return CreatePatchDocument(typeof(TModel), patchObject, jsonOptions, mergePatchOptions) as JsonMergePatchDocument<TModel>;
        }

        internal static IInternalJsonMergePatchDocument CreatePatchDocument(Type modelType, JsonElement jsonElement, JsonSerializerOptions jsonOptions, JsonMergePatchOptions mergePatchOptions)
        {
            var jsonMergePatchType = internalJsonMergePatchDocumentType.MakeGenericType(modelType);
            var json = jsonElement.GetRawText();
            var model = JsonSerializer.Deserialize(json, modelType, jsonOptions);

            var jsonMergePatchDocument = (IInternalJsonMergePatchDocument)Activator.CreateInstance(jsonMergePatchType, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { model }, null);
            AddOperation(jsonMergePatchDocument, "/", jsonElement, mergePatchOptions, modelType, jsonOptions);
            return jsonMergePatchDocument;
        }
    }
}
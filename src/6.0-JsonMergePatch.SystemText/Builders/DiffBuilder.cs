using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText.Builders
{
    public static class DiffBuilder
    {
        public static JsonDocument Build<TModel>(TModel original, TModel patched) where TModel : class
        {
            var originalJson = JsonSerializer.SerializeToElement(original);
            var patchedJson = JsonSerializer.SerializeToElement(patched);
            return BuildDiff(originalJson, patchedJson);
        }

        private static JsonDocument BuildDiff(JsonElement original, JsonElement patched)
        {
            if (original.ValueKind == JsonValueKind.Null && patched.ValueKind == JsonValueKind.Null)
                return JsonDocument.Parse("{}");

            if (original.ValueKind == JsonValueKind.Null)
                return JsonDocument.Parse(patched.GetRawText());

            if (patched.ValueKind == JsonValueKind.Null)
                return JsonDocument.Parse("null");

            if (original.ValueKind == JsonValueKind.Array || patched.ValueKind == JsonValueKind.Array)
                return BuildArrayDiff(original, patched);

            return original.ValueKind == JsonValueKind.Object ?
                BuildObjectDiff(original, patched) : BuildValueDiff(original, patched);
        }

        private static JsonDocument BuildObjectDiff(JsonElement original, JsonElement patched)
        {
            var result = new Dictionary<string, JsonElement>();

            var propertyNames = original.EnumerateObject()
                .Select(p => p.Name)
                .Union(patched.EnumerateObject().Select(p => p.Name))
                .Distinct();

            foreach (var propertyName in propertyNames)
            {
                var originalPropExists = original.TryGetProperty(propertyName, out var originalValue);
                var patchedPropExists = patched.TryGetProperty(propertyName, out var patchedValue);

                // If the property exists in both and is unchanged, skip it
                if (originalPropExists &&
                    patchedPropExists &&
                    originalValue.ValueKind == patchedValue.ValueKind &&
                    originalValue.ToString() == patchedValue.ToString())
                {
                    continue;
                }

                var patchToken = BuildDiff(
                    originalPropExists ? originalValue : default,
                    patchedPropExists ? patchedValue : default
                );

                if (patchToken != null && patchToken.RootElement.ValueKind != JsonValueKind.Undefined)
                    result[propertyName] = patchToken.RootElement.Clone();
            }

            if (!result.Any()) {
                return JsonDocument.Parse("{}");
            }

            var serializedResult = JsonSerializer.Serialize(result);
            return JsonDocument.Parse(serializedResult);

        }

        private static JsonDocument BuildValueDiff(JsonElement original, JsonElement patched)
        {
            return JsonDocument.Parse(!original.Equals(patched) ? patched.GetRawText() : "{}");
        }

        private static JsonDocument BuildArrayDiff(JsonElement original, JsonElement patched)
        {
            return JsonDocument.Parse(JsonArrayEquals(original, patched) ? "{}" : patched.GetRawText());

            bool JsonArrayEquals(JsonElement left, JsonElement right)
            {
                if (left.GetArrayLength() != right.GetArrayLength())
                    return false;

                for (int i = 0; i < left.GetArrayLength(); i++)
                {
                    var diff = BuildDiff(left[i], right[i]);
                    if (diff.RootElement.ValueKind != JsonValueKind.Undefined)
                        return false;
                }
                return true;
            }
        }
    }
}
using Morcatko.AspNetCore.JsonMergePatch;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace testApp
{
    public class JsonMergePatchDocumentOperationFilter : IOperationFilter
    {
        private static bool IsJsonMergePatchDocumentType(Type t) => (t != null) && t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>));

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if ((context.ApiDescription.ParameterDescriptions.Count > 0) && (context.ApiDescription.ParameterDescriptions.Count != operation.Parameters.Count))
                throw new NotSupportedException("Something went wrong. Parameter counts do not match");

            for (int i = 0; i < context.ApiDescription.ParameterDescriptions.Count; i++)
            {
                var parameter = context.ApiDescription.ParameterDescriptions[i];
                if (IsJsonMergePatchDocumentType(parameter.Type))
                {
                    (operation.Parameters[i] as BodyParameter).Schema = context.SchemaRegistry.GetOrRegister(parameter.Type.GenericTypeArguments[0]);
                }
                else if ((parameter.Type != null) && parameter.Type.IsGenericType && (parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    var jsonMergeType = parameter.Type.GenericTypeArguments[0];
                    if (IsJsonMergePatchDocumentType(jsonMergeType))
                    {
                        var enumerableType = typeof(IEnumerable<>);
                        var genericEnumerableType = enumerableType.MakeGenericType(jsonMergeType.GenericTypeArguments[0]);
                        (operation.Parameters[i] as BodyParameter).Schema = context.SchemaRegistry.GetOrRegister(genericEnumerableType);
                    }
                }
            }
        }
    }
}

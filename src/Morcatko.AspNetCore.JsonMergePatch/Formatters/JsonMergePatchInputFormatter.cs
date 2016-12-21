using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Formatters
{
	internal class JsonMergePatchInputFormatter : JsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly IArrayPool<char> _charPool;

		public JsonMergePatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider)
			: base(logger, serializerSettings, charPool, objectPoolProvider)
		{
			this._charPool = new JsonArrayPool<char>(charPool);

			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
		}

		private void AddOperation(JsonMergePatchDocument jsonMergePatchDocument, string pathPrefix, JObject jObject)
		{
			foreach (var jProperty in jObject)
			{
				if (jProperty.Value is JValue)
					jsonMergePatchDocument.AddOperation(OperationType.Replace, pathPrefix + jProperty.Key, ((JValue)jProperty.Value).Value);
				else if (jProperty.Value is JArray)
					jsonMergePatchDocument.AddOperation(OperationType.Replace, pathPrefix + jProperty.Key, ((JArray)jProperty.Value));
				else if (jProperty.Value is JObject)
					AddOperation(jsonMergePatchDocument, pathPrefix + jProperty.Key + "/", (jProperty.Value as JObject));
			}
		}

		public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
		{
			var request = context.HttpContext.Request;
			using (var streamReader = context.ReaderFactory(request.Body, encoding))
			{
				using (var jsonReader = new JsonTextReader(streamReader))
				{
					jsonReader.ArrayPool = _charPool;
					jsonReader.CloseInput = false;

					var jsonMergePatchDocumentModelType = context.ModelType;
					var modelType = jsonMergePatchDocumentModelType.GenericTypeArguments.Single();

					JsonSerializer jsonSerializer = CreateJsonSerializer();
					try
					{
						var jModel = JObject.Load(jsonReader);

						var model = jModel.ToObject(modelType, jsonSerializer);
						var jsonMergePatchDocument = (JsonMergePatchDocument)Activator.CreateInstance(jsonMergePatchDocumentModelType, model);

						AddOperation(jsonMergePatchDocument, "/", jModel);

						return InputFormatterResult.SuccessAsync(jsonMergePatchDocument);
					}
					catch (Exception ex)
					{
						context.ModelState.TryAddModelError(context.ModelName, ex.Message);
						return InputFormatterResult.FailureAsync();
					}
					finally
					{
						ReleaseJsonSerializer(jsonSerializer);
					}
				}
			}
		}


		public override bool CanRead(InputFormatterContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

#warning Add support for non-generic type
			return (context.ModelType.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>));
		}
	}
}

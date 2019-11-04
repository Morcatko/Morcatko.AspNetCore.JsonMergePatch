using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Morcatko.AspNetCore.JsonMergePatch.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Formatters
{
	internal class JsonMergePatchInputFormatter : JsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();
		private readonly Lazy<ModelMetadata> _modelMetadata;
		private readonly JsonMergePatchOptions _options;

		public JsonMergePatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			Lazy<IModelMetadataProvider> lazyModelMetadataProvider,
			JsonMergePatchOptions options)
			: base(logger, serializerSettings, charPool, objectPoolProvider)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
			_modelMetadata = new Lazy<ModelMetadata>(() => lazyModelMetadataProvider.Value.GetMetadataForType(typeof(JToken)));
			_options = options;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context)
			=> context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

		private JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject jObject, JsonSerializer jsonSerializer)
		{
			var jsonMergePatchDocument = PatchBuilder.CreatePatchDocument(jsonMergePatchType, modelType, jObject, jsonSerializer, this._options);
			jsonMergePatchDocument.ContractResolver = SerializerSettings.ContractResolver;
			return jsonMergePatchDocument;
		}

		public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
		{
			var jsonMergePatchType = context.ModelType;
			var container = (IList)null;

			if (ContainerIsIEnumerable(context))
			{
				jsonMergePatchType = context.ModelType.GenericTypeArguments[0];
				var listType = typeof(List<>);
				var constructedListType = listType.MakeGenericType(jsonMergePatchType);
				container = (IList)Activator.CreateInstance(constructedListType);
			}
			var modelType = jsonMergePatchType.GenericTypeArguments[0];


			var patchContext = new InputFormatterContext(
				context.HttpContext,
				context.ModelName,
				context.ModelState,
				_modelMetadata.Value,
				context.ReaderFactory,
				context.TreatEmptyInputAsDefaultValue);

			var jTokenResult = await base.ReadRequestBodyAsync(patchContext, encoding);

			if (jTokenResult.HasError)
				return jTokenResult;

			var serializer = base.CreateJsonSerializer();
			try
			{

				switch (jTokenResult.Model)
				{
					case JObject jObject:
						if (container != null)
							throw new ArgumentException("Received object when array was expected"); //This could be handled by returning list with single item

						var jsonMergePatchDocument = CreatePatchDocument(jsonMergePatchType, modelType, jObject, serializer);
						return await InputFormatterResult.SuccessAsync(jsonMergePatchDocument);
					case JArray jArray:
						if (container == null)
							throw new ArgumentException("Received array when object was expected");

						foreach (var jObject in jArray.OfType<JObject>())
						{
							container.Add(CreatePatchDocument(jsonMergePatchType, modelType, jObject, serializer));
						}
						return await InputFormatterResult.SuccessAsync(container);
				}
			}
			catch (Exception e)
			{
				throw;
			}
			finally
			{
				base.ReleaseJsonSerializer(serializer);
			}

			return InputFormatterResult.Failure();
		}


		public override bool CanRead(InputFormatterContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var jsonMergePatchType = context.ModelType;

			if (ContainerIsIEnumerable(context))
				jsonMergePatchType = context.ModelType.GenericTypeArguments[0];

			return (jsonMergePatchType.IsGenericType && (jsonMergePatchType.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>)));
		}
	}
}

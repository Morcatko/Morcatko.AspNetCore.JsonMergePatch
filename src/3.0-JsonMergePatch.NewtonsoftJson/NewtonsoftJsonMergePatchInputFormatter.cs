using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Morcatko.AspNetCore.JsonMergePatch.Internal;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson
{
	internal class NewtonsoftJsonMergePatchInputFormatter : NewtonsoftJsonInputFormatter
	{
		private static readonly Type _listType = typeof(List<>);
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();
		private readonly Lazy<ModelMetadata> _modelMetadata;
		private readonly JsonMergePatchOptions _jsonMergePatchOptions;

		public NewtonsoftJsonMergePatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			MvcOptions mvcOptions,
			MvcNewtonsoftJsonOptions jsonOptions,
			Lazy<IModelMetadataProvider> lazyModelMetadataProvider,
			JsonMergePatchOptions jsonMergePatchOptions)
			: base(logger, serializerSettings, charPool, objectPoolProvider, mvcOptions, jsonOptions)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
			_modelMetadata = new Lazy<ModelMetadata>(() => lazyModelMetadataProvider.Value.GetMetadataForType(typeof(JObject)));
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context)
			=> context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

		private IInternalJsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject jObject, JsonSerializer jsonSerializer)
		{
			var jsonMergePatchDocument = PatchBuilder.CreatePatchDocument(modelType, jObject, jsonSerializer, this._jsonMergePatchOptions);
			jsonMergePatchDocument.ContractResolver = SerializerSettings.ContractResolver;
			return jsonMergePatchDocument;
		}

		private object ConvertToPatch(object o, IList container, Type jsonMergePatchType, Type modelType, JsonSerializer serializer)
		{
			switch (o)
			{
				case JObject jObject:
					if (container != null)
						throw new ArgumentException("Received object when array was expected"); //This could be handled by returning list with single item

					return CreatePatchDocument(jsonMergePatchType, modelType, jObject, serializer);
				case JArray jArray:
					if (container == null)
						throw new ArgumentException("Received array when object was expected");

					foreach (var jObject in jArray.OfType<JObject>())
					{
						container.Add(CreatePatchDocument(jsonMergePatchType, modelType, jObject, serializer));
					}
					return container;
				default:
					throw new NotSupportedException($"Unsupported type '{o?.GetType()}'");
			}
		}

		public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
		{
			var patchContext = new InputFormatterContext(
				context.HttpContext,
				context.ModelName,
				context.ModelState,
				_modelMetadata.Value,
				context.ReaderFactory,
				context.TreatEmptyInputAsDefaultValue);

			var jsonResult = await base.ReadRequestBodyAsync(patchContext);

			if (jsonResult.HasError)
				return jsonResult;

			var serializer = base.CreateJsonSerializer();
			try
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

				var result = ConvertToPatch(jsonResult.Model, container, jsonMergePatchType, modelType, serializer);
				return InputFormatterResult.Success(result);
			}
			catch (Exception ex)
			{
				context.ModelState.TryAddModelError(context.ModelName, ex.Message);
				return InputFormatterResult.Failure();
			}
			finally
			{
				base.ReleaseJsonSerializer(serializer);
			}
		}


		public override bool CanRead(InputFormatterContext context)

		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var jsonMergePatchType = context.ModelType;

			if (ContainerIsIEnumerable(context))
				jsonMergePatchType = context.ModelType.GenericTypeArguments[0];

			return (jsonMergePatchType.IsGenericType && (jsonMergePatchType.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>)));
		}

	}
}

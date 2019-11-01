using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Morcatko.AspNetCore.JsonMergePatch.external;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson
{
	internal class NewtonsoftJsonMergePatchInputFormatter : NewtonsoftJsonInputFormatter
	{
		private const int DefaultMemoryThreshold = 1024 * 30;

		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly JsonMergePatchOptions _jsonMergePatchOptions;
		private readonly MvcNewtonsoftJsonOptions _jsonOptions;
		private readonly Lazy<ModelMetadata> _modelMetadata;
		private readonly MvcOptions _mvcOptions;
		private readonly ILogger _logger;
		private readonly IArrayPool<char> _charPool;

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

			_logger = logger;
			_charPool = new JsonArrayPool<char>(charPool);
			_mvcOptions = mvcOptions;
			_jsonOptions = jsonOptions;
			_modelMetadata = new Lazy<ModelMetadata>(() => lazyModelMetadataProvider.Value.GetMetadataForType(typeof(JObject)));
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context)
			=> context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

#warning TODO - Pooling (was not implemented in v2 either)
		private new NewtonsoftJsonMergePatchSerializer CreateJsonSerializer(InputFormatterContext context)
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

			return new NewtonsoftJsonMergePatchSerializer(
				container,
				modelType,
				base.CreateJsonSerializer(),
				SerializerSettings,
				_jsonMergePatchOptions);
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

		public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
		{
			var patchContext = new InputFormatterContext(
				context.HttpContext,
				context.ModelName,
				context.ModelState,
				_modelMetadata.Value,
				context.ReaderFactory,
				context.TreatEmptyInputAsDefaultValue);

			var jtokenResult = await base.ReadRequestBodyAsync(patchContext);

			if (jtokenResult.HasError)
				return jtokenResult;


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

			try
			{
				var p = PatchBuilder.CreatePatchDocument(
					modelType,
					jtokenResult.Model as JObject,
					base.CreateJsonSerializer(),
					_jsonMergePatchOptions);

				return InputFormatterResult.Success(p);
			}
			catch (Exception e)
			{
				throw;
			}
		}

	}
}

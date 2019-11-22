using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Morcatko.AspNetCore.JsonMergePatch.Internal;
using Morcatko.AspNetCore.JsonMergePatch.SystemText.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
	internal class SystemTextJsonMergePatchInputFormatter : SystemTextJsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly Lazy<ModelMetadata> _modelMetadata;
		private readonly JsonMergePatchOptions _jsonMergePatchOptions;

		public SystemTextJsonMergePatchInputFormatter(
			JsonOptions options,
			ILogger<SystemTextJsonInputFormatter> logger,
			Lazy<IModelMetadataProvider> lazyModelMetadataProvider,
			JsonMergePatchOptions jsonMergePatchOptions)
			: base(options, logger)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
			_modelMetadata = new Lazy<ModelMetadata>(() => lazyModelMetadataProvider.Value.GetMetadataForType(typeof(JsonElement)));
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context)
			=> context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

		private IInternalJsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JsonElement jsonElement)
		{
			var jsonMergePatchDocument = PatchBuilder.CreatePatchDocument(modelType, jsonElement, this._jsonMergePatchOptions);
			return jsonMergePatchDocument;
		}

		private object ConvertToPatch(object o, IList container, Type jsonMergePatchType, Type modelType)
		{
			var e = (JsonElement)o;
			switch (e.ValueKind)
			{
				case JsonValueKind.Object:
					if (container != null)
						throw new ArgumentException("Received object when array was expected"); //This could be handled by returning list with single item

					return CreatePatchDocument(jsonMergePatchType, modelType, e);
				case JsonValueKind.Array:
					if (container == null)
						throw new ArgumentException("Received array when object was expected");

					var enumerator = e.EnumerateArray();
					while (enumerator.MoveNext())
					{
						container.Add(CreatePatchDocument(jsonMergePatchType, modelType, enumerator.Current));
					}
					return container;
				default:
					throw new NotSupportedException($"Unsupported ValueKing '{e.ValueKind}'");
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

				var x = jsonResult.Model;

				var result = ConvertToPatch(jsonResult.Model, container, jsonMergePatchType, modelType);
				return InputFormatterResult.Success(result);
			}
			catch (Exception ex)
			{
				context.ModelState.TryAddModelError(context.ModelName, ex.Message);
				return InputFormatterResult.Failure();
			}
		}

	}
}

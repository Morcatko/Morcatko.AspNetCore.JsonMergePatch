using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Formatters
{
	internal class NewtonsoftJsonMergePatchInputFormatter : NewtonsoftJsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly JsonMergePatchOptions _jsonMergePatchOptions;

		public NewtonsoftJsonMergePatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			MvcOptions mvcOptions,
			MvcNewtonsoftJsonOptions jsonOptions,
			JsonMergePatchOptions jsonMergePatchOptions)
			: base(logger, serializerSettings, charPool, objectPoolProvider, mvcOptions, jsonOptions)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context) => context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

		protected override JsonSerializer CreateJsonSerializer(InputFormatterContext context)
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
				jsonMergePatchType,
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
	}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Morcatko.AspNetCore.JsonMergePatch.external;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	internal class NewtonsoftJsonMergePatchInputFormatter : NewtonsoftJsonInputFormatter
	{
		private const int DefaultMemoryThreshold = 1024 * 30;

		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly JsonMergePatchOptions _jsonMergePatchOptions;
		private readonly MvcNewtonsoftJsonOptions _jsonOptions;
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
			JsonMergePatchOptions jsonMergePatchOptions)
			: base(logger, serializerSettings, charPool, objectPoolProvider, mvcOptions, jsonOptions)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);

			_logger = logger;
			_charPool = new JsonArrayPool<char>(charPool);
			_mvcOptions = mvcOptions;
			_jsonOptions = jsonOptions;
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context) => context.ModelType.IsGenericType && (context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

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
				jsonMergePatchType,
				modelType,
				base.CreateJsonSerializer(),
				SerializerSettings,
				_jsonMergePatchOptions);
		}

		protected override void ReleaseJsonSerializer(JsonSerializer serializer)
		{
			base.ReleaseJsonSerializer(serializer);
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

		#region Copied 1:1 from AspNetCore/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonInputFormatter.cs
		/*
		The reason for copy&paste is that jsonSerializer used in
		  model = jsonSerializer.Deserialize(jsonReader, type);
		does not have virtual Deserialize method and so my custom JsonSerializer.Deserialize is not called
		*/

		public override async Task<InputFormatterResult> ReadRequestBodyAsync(
			InputFormatterContext context,
			Encoding encoding)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			var request = context.HttpContext.Request;

			var suppressInputFormatterBuffering = _mvcOptions.SuppressInputFormatterBuffering;

			var readStream = request.Body;
			if (!request.Body.CanSeek && !suppressInputFormatterBuffering)
			{
				// JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously
				// read everything into a buffer, and then seek back to the beginning.
				var memoryThreshold = DefaultMemoryThreshold;
				var contentLength = request.ContentLength.GetValueOrDefault();
				if (contentLength > 0 && contentLength < memoryThreshold)
				{
					// If the Content-Length is known and is smaller than the default buffer size, use it.
					memoryThreshold = (int)contentLength;
				}

				readStream = new FileBufferingReadStream(request.Body, memoryThreshold);

				await readStream.DrainAsync(CancellationToken.None);
				readStream.Seek(0L, SeekOrigin.Begin);
			}

			var successful = true;
			Exception exception = null;
			object model;

			using (var streamReader = context.ReaderFactory(readStream, encoding))
			{
				using var jsonReader = new JsonTextReader(streamReader);
				jsonReader.ArrayPool = _charPool;
				jsonReader.CloseInput = false;

				var type = context.ModelType;
				var jsonSerializer = CreateJsonSerializer(context);
				jsonSerializer.Error += ErrorHandler;
				try
				{
					model = jsonSerializer.Deserialize(jsonReader, type);
				}
				finally
				{
					// Clean up the error handler since CreateJsonSerializer() pools instances.
					jsonSerializer.Error -= ErrorHandler;
					ReleaseJsonSerializer(jsonSerializer);

					if (readStream is FileBufferingReadStream fileBufferingReadStream)
					{
						await fileBufferingReadStream.DisposeAsync();
					}
				}
			}

			if (successful)
			{
				if (model == null && !context.TreatEmptyInputAsDefaultValue)
				{
					// Some nonempty inputs might deserialize as null, for example whitespace,
					// or the JSON-encoded value "null". The upstream BodyModelBinder needs to
					// be notified that we don't regard this as a real input so it can register
					// a model binding error.
					return InputFormatterResult.NoValue();
				}
				else
				{
					return InputFormatterResult.Success(model);
				}
			}

			if (!(exception is JsonException || exception is OverflowException))
			{
				var exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
				exceptionDispatchInfo.Throw();
			}

			return InputFormatterResult.Failure();

			void ErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs eventArgs)
			{
				successful = false;

				// When ErrorContext.Path does not include ErrorContext.Member, add Member to form full path.
				var path = eventArgs.ErrorContext.Path;
				var member = eventArgs.ErrorContext.Member?.ToString();
				var addMember = !string.IsNullOrEmpty(member);
				if (addMember)
				{
					// Path.Member case (path.Length < member.Length) needs no further checks.
					if (path.Length == member.Length)
					{
						// Add Member in Path.Memb case but not for Path.Path.
						addMember = !string.Equals(path, member, StringComparison.Ordinal);
					}
					else if (path.Length > member.Length)
					{
						// Finally, check whether Path already ends with Member.
						if (member[0] == '[')
						{
							addMember = !path.EndsWith(member, StringComparison.Ordinal);
						}
						else
						{
							addMember = !path.EndsWith("." + member, StringComparison.Ordinal)
								&& !path.EndsWith("['" + member + "']", StringComparison.Ordinal)
								&& !path.EndsWith("[" + member + "]", StringComparison.Ordinal);
						}
					}
				}

				if (addMember)
				{
					path = ModelNames.CreatePropertyModelName(path, member);
				}

				// Handle path combinations such as ""+"Property", "Parent"+"Property", or "Parent"+"[12]".
				var key = ModelNames.CreatePropertyModelName(context.ModelName, path);

				exception = eventArgs.ErrorContext.Error;

				var metadata = GetPathMetadata(context.Metadata, path);
				var modelStateException = WrapExceptionForModelState(exception);
				context.ModelState.TryAddModelError(key, modelStateException, metadata);

				_logger.JsonInputException(exception);

				// Error must always be marked as handled
				// Failure to do so can cause the exception to be rethrown at every recursive level and
				// overflow the stack for x64 CLR processes
				eventArgs.ErrorContext.Handled = true;
			}
		}

		private ModelMetadata GetPathMetadata(ModelMetadata metadata, string path)
		{
			var index = 0;
			while (index >= 0 && index < path.Length)
			{
				if (path[index] == '[')
				{
					// At start of "[0]".
					if (metadata.ElementMetadata == null)
					{
						// Odd case but don't throw just because ErrorContext had an odd-looking path.
						break;
					}

					metadata = metadata.ElementMetadata;
					index = path.IndexOf(']', index);
				}
				else if (path[index] == '.' || path[index] == ']')
				{
					// Skip '.' in "prefix.property" or "[0].property" or ']' in "[0]".
					index++;
				}
				else
				{
					// At start of "property", "property." or "property[0]".
					var endIndex = path.IndexOfAny(new[] { '.', '[' }, index);
					if (endIndex == -1)
					{
						endIndex = path.Length;
					}

					var propertyName = path.Substring(index, endIndex - index);
					if (metadata.Properties[propertyName] == null)
					{
						// Odd case but don't throw just because ErrorContext had an odd-looking path.
						break;
					}

					metadata = metadata.Properties[propertyName];
					index = endIndex;
				}
			}

			return metadata;
		}

		private Exception WrapExceptionForModelState(Exception exception)
		{
			// In 2.0 and earlier we always gave a generic error message for errors that come from JSON.NET
			// We only allow it in 2.1 and newer if the app opts-in.
			if (!_jsonOptions.AllowInputFormatterExceptionMessages)
			{
				// This app is not opted-in to JSON.NET messages, return the original exception.
				return exception;
			}

			// It's not known that Json.NET currently ever raises error events with exceptions
			// other than these two types, but we're being conservative and limiting which ones
			// we regard as having safe messages to expose to clients
			if (exception is JsonReaderException || exception is JsonSerializationException)
			{
				// InputFormatterException specifies that the message is safe to return to a client, it will
				// be added to model state.
				return new InputFormatterException(exception.Message, exception);
			}

			// Not a known exception type, so we're not going to assume that it's safe.
			return exception;
		}
		#endregion
	}
}

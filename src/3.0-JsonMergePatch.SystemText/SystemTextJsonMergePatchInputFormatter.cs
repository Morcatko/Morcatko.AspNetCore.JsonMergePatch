using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
	internal class SystemTextJsonMergePatchInputFormatter : SystemTextJsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType = MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly JsonMergePatchOptions _jsonMergePatchOptions;
		private readonly ILogger _logger;

		public SystemTextJsonMergePatchInputFormatter(
			JsonOptions options,
			ILogger<SystemTextJsonInputFormatter> logger,
			JsonMergePatchOptions jsonMergePatchOptions)
			: base(options, logger)
		{
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);

			_logger = logger;
			_jsonMergePatchOptions = jsonMergePatchOptions;
		}

		public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
		{
			return base.ReadRequestBodyAsync(context);
		}

	}
}

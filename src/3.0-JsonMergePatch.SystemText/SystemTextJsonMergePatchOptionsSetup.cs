using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
	class SystemTextJsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly IOptions<JsonOptions> _jsonOptions;
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly IOptions<JsonMergePatchOptions> _jsonMergePatchOptions;

		public SystemTextJsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			IOptions<JsonOptions> jsonOptions,
			Lazy<IModelMetadataProvider> modelMetadataProvider,
			IOptions<JsonMergePatchOptions> jsonMergePatchOptions)
		{

			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
			_jsonMergePatchOptions = jsonMergePatchOptions ?? throw new ArgumentNullException(nameof(jsonMergePatchOptions));
		}

		public void Configure(MvcOptions options)
		{
			var jsonMergePatchLogger = _loggerFactory.CreateLogger<SystemTextJsonInputFormatter>();
			options.InputFormatters.Insert(0, new SystemTextJsonMergePatchInputFormatter(
				_jsonOptions.Value,
				jsonMergePatchLogger,
				_modelMetadataProvider,
				_jsonMergePatchOptions.Value));
		}
	}
}

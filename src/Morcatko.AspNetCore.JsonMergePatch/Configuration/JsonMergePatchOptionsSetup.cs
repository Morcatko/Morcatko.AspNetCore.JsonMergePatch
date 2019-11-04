using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.Formatters;
using System;
using System.Buffers;

namespace Morcatko.AspNetCore.JsonMergePatch.Configuration
{
	class JsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly IOptions<MvcJsonOptions> _jsonOptions;
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<JsonMergePatchOptions> _options;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			IOptions<MvcJsonOptions> jsonOptions,
			Lazy<IModelMetadataProvider> modelMetadataProvider,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			IOptions<JsonMergePatchOptions> options)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(loggerFactory));
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
			_objectPoolProvider = objectPoolProvider ?? throw new ArgumentNullException(nameof(objectPoolProvider));
			_options = options ?? throw new ArgumentNullException(nameof(options));
		}

		public void Configure(MvcOptions options)
		{
			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				_loggerFactory.CreateLogger<JsonMergePatchInputFormatter>(),
				_jsonOptions.Value.SerializerSettings,
				_charPool,
				_objectPoolProvider,
				_modelMetadataProvider,
				_options.Value));
		}
	}
}

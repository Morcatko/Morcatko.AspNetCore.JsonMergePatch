using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;

namespace Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson
{
	class NewtonsoftJsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly IOptions<JsonMergePatchOptions> _jsonMergePatchOptions;

		public NewtonsoftJsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
			Lazy<IModelMetadataProvider> modelMetadataProvider,
			IOptions<JsonMergePatchOptions> jsonMergePatchOptions)
		{

			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
			_objectPoolProvider = objectPoolProvider ?? throw new ArgumentNullException(nameof(objectPoolProvider));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
			_jsonMergePatchOptions = jsonMergePatchOptions ?? throw new ArgumentNullException(nameof(jsonMergePatchOptions));
		}

		public void Configure(MvcOptions options)
		{
			var jsonMergePatchLogger = _loggerFactory.CreateLogger<NewtonsoftJsonMergePatchInputFormatter>();
			options.InputFormatters.Insert(0, new NewtonsoftJsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonOptions.Value.SerializerSettings,
				_charPool,
				_objectPoolProvider,
				options,
				_jsonOptions.Value,
				_modelMetadataProvider,
				_jsonMergePatchOptions.Value));
		}
	}
}

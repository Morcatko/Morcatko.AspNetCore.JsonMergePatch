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
		private readonly IOptions<MvcJsonOptions> _mvcJsonOptions;
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<JsonMergePatchOptions> _jsonMergePatchOptions;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			IOptions<MvcJsonOptions> mvcJsonOptions,
			Lazy<IModelMetadataProvider> modelMetadataProvider,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			IOptions<JsonMergePatchOptions> jsonMergePatchOptions)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_mvcJsonOptions = mvcJsonOptions ?? throw new ArgumentNullException(nameof(mvcJsonOptions));
			_modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(loggerFactory));
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
			_objectPoolProvider = objectPoolProvider ?? throw new ArgumentNullException(nameof(objectPoolProvider));
			_jsonMergePatchOptions = jsonMergePatchOptions ?? throw new ArgumentNullException(nameof(jsonMergePatchOptions));
		}

		public void Configure(MvcOptions mvcOption)
		{
			mvcOption.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				_loggerFactory.CreateLogger<JsonMergePatchInputFormatter>(),
				_mvcJsonOptions.Value.SerializerSettings,
				_charPool,
				_objectPoolProvider,
				mvcOption,
				_mvcJsonOptions.Value,
				_modelMetadataProvider,
				_jsonMergePatchOptions.Value));
		}
	}
}

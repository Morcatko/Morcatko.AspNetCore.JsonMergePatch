using Microsoft.AspNetCore.Mvc;
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
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<MvcJsonOptions> _jsonOptions;
		private readonly IOptions<JsonMergePatchOptions> _jsonMergePatchOptions;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			IOptions<MvcJsonOptions> jsonOptions,
			IOptions<JsonMergePatchOptions> jsonMergePatchOptions)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
			_objectPoolProvider = objectPoolProvider ?? throw new ArgumentNullException(nameof(objectPoolProvider));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_jsonMergePatchOptions = jsonMergePatchOptions ?? throw new ArgumentNullException(nameof(jsonMergePatchOptions));
		}

		public void Configure(MvcOptions options)
		{
			var jsonMergePatchLogger = _loggerFactory.CreateLogger<JsonMergePatchInputFormatter>();
			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonOptions.Value.SerializerSettings,
				_charPool,
				_objectPoolProvider,
				options,
				_jsonOptions.Value,
				_jsonMergePatchOptions.Value));
		}
	}
}

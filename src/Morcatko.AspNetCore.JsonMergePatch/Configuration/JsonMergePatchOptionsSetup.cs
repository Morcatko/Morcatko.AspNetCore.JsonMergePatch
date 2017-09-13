using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.Formatters;
using Newtonsoft.Json;
using System;
using System.Buffers;

namespace Morcatko.AspNetCore.JsonMergePatch.Configuration
{
	class JsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			IOptions<MvcJsonOptions> jsonOptions,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider)
		{
			if (loggerFactory == null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}

			if (jsonOptions == null)
			{
				throw new ArgumentNullException(nameof(jsonOptions));
			}

			if (charPool == null)
			{
				throw new ArgumentNullException(nameof(charPool));
			}

			if (objectPoolProvider == null)
			{
				throw new ArgumentNullException(nameof(objectPoolProvider));
			}

			_loggerFactory = loggerFactory;
			_jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
			_charPool = charPool;
			_objectPoolProvider = objectPoolProvider;
		}

		public void Configure(MvcOptions options)
		{
			var jsonMergePatchLogger = _loggerFactory.CreateLogger<JsonMergePatchInputFormatter>();
			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonSerializerSettings,
				_charPool,
				_objectPoolProvider));
		}
	}
}

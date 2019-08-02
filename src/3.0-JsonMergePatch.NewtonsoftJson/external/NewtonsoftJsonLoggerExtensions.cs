using Microsoft.Extensions.Logging;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch.external
{
	//Copied from AspNetCore/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonLoggerExtensions.cs
	internal static class NewtonsoftJsonLoggerExtensions
	{
		private static readonly Action<ILogger, Exception> _jsonInputFormatterException;


		static NewtonsoftJsonLoggerExtensions()
		{
			_jsonInputFormatterException = LoggerMessage.Define(
				LogLevel.Debug,
				new EventId(1, "JsonInputException"),
				"JSON input formatter threw an exception.");
		}

		public static void JsonInputException(this ILogger logger, Exception exception)
		{
			_jsonInputFormatterException(logger, exception);
		}
	}
}

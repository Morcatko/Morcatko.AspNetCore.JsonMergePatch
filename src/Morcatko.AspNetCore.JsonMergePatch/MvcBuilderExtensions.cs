using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.Configuration;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public static class MvcBuilderExtensions
	{
		private static void AddJsonMergePatch(this IServiceCollection services, Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
		}

		public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddJsonMergePatch(configure);
			return builder;
		}

		public static IMvcCoreBuilder AddJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.AddJsonFormatters();
			builder.Services.AddJsonMergePatch(configure);
			return builder;
		}
	}

	public class JsonMergePatchOptions
	{
		/// <summary>
		/// Allow to delete property when setting null on a dictionary type
		/// </summary>
		public bool EnableDelete { get; set; }
	}
}

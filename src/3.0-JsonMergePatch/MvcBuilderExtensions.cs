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
		private static void AddNewtonsoftJsonMergePatch(this IServiceCollection services, Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, NewtonsoftJsonMergePatchOptionsSetup>());
		}

		public static IMvcBuilder AddNewtonsoftJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddNewtonsoftJsonMergePatch(configure);
			return builder;
		}

		public static IMvcCoreBuilder AddNewtonsoftJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.AddNewtonsoftJson();
			builder.Services.AddNewtonsoftJsonMergePatch(configure);
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

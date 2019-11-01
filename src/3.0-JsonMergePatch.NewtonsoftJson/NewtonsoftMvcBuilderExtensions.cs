using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public static class NewtonsoftMvcBuilderExtensions
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
			builder.Services.AddNewtonsoftJsonMergePatch(configure);
			return builder;
		}
	}
}

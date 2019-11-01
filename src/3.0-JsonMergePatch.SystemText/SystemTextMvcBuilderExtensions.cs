using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
	public static class SystemTextMvxBuilderExtensions
	{
		private static void AddSystemText(this IServiceCollection services, Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SystemTextJsonMergePatchOptionsSetup>());
		}

		public static IMvcBuilder AddSystemText(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddSystemText(configure);
			return builder;
		}

		public static IMvcCoreBuilder AddSystemTextJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddSystemText(configure);
			return builder;
		}
	}
}

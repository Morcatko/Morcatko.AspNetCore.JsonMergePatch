using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using Morcatko.AspNetCore.JsonMergePatch.SystemText;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public static class SystemTextMvxBuilderExtensions
	{
		private static void AddSystemTextJsonMergePatch(this IServiceCollection services, Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SystemTextJsonMergePatchOptionsSetup>());
			services.AddSingleton<Lazy<IModelMetadataProvider>>(sp => new Lazy<IModelMetadataProvider>(() => sp.GetRequiredService<IModelMetadataProvider>()));
		}

		public static IMvcBuilder AddSystemTextJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddSystemTextJsonMergePatch(configure);
			return builder;
		}

		public static IMvcCoreBuilder AddSystemTextJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
		{
			builder.Services.AddSystemTextJsonMergePatch(configure);
			return builder;
		}
	}
}

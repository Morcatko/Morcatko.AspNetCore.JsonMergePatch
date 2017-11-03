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
        [Obsolete("User AddJsonMergePatch(this IMvcBuilder builder)")]
		public static void AddJsonMergePatch(this IServiceCollection services)
		{
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
		}

        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
            return builder;
        }
    }
}

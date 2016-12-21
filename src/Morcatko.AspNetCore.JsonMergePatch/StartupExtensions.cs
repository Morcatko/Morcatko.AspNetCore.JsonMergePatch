using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.Configuration;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public static class StartupExtensions
	{
		public static void AddJsonMergePatch(this IServiceCollection services)
		{
			services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
		}
	}
}

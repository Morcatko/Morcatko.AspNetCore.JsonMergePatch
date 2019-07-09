using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	class MvcStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.AddNewtonsoftJsonMergePatch();

			services.AddSingleton<IRepository, Repository>();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseEndpoints(e => e.MapControllers());
		}
	}

	class MvcCoreStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvcCore()
				.AddNewtonsoftJsonMergePatch();
			services.AddSingleton<IRepository, Repository>();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseEndpoints(e => e.MapControllers());
		}
	}
}

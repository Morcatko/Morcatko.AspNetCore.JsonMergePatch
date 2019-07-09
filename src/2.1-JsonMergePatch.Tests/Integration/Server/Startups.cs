using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	class MvcStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.AddJsonMergePatch();
			services.AddSingleton<IRepository, Repository>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseMvc();
		}
	}

	class MvcCoreStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvcCore()
				.AddJsonMergePatch();
			services.AddSingleton<IRepository, Repository>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseMvc();
		}
	}
}

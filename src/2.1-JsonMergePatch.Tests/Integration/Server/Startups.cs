using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IRepository, Repository>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseMvc();
		}
	}
}

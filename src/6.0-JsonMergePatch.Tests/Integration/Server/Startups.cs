using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IRepository<NewtonsoftTestModel>, Repository<NewtonsoftTestModel>>();
			services.AddSingleton<IRepository<SystemTextTestModel>, Repository<SystemTextTestModel>>();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseEndpoints(e => e.MapControllers());
		}
	}
}

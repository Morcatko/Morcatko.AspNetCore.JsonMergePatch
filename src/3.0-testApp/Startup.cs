using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch;
using Morcatko.AspNetCore.JsonMergePatch.Tests;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;

namespace testApp
{
	abstract class StartupBase
	{
		public virtual void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IRepository<NewtonsoftTestModel>, Repository<NewtonsoftTestModel>>();
			services.AddSingleton<IRepository<SystemTextTestModel>, Repository<SystemTextTestModel>>();

			/*services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
			});*/
		}

		public void Configure(IApplicationBuilder app)
		{
			/*app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});*/

			app.UseRouting();
			app.UseEndpoints(e => e.MapControllers());
		}
	}

	class Startup_Newtonsoft : StartupBase
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddControllers()
				.AddNewtonsoftJsonMergePatch();
		}
	}

	class Startup_SystemText: StartupBase
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddControllers();
		}
	}
}

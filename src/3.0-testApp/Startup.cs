using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;

namespace testApp
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddControllers()
				.AddNewtonsoftJsonMergePatch();

			services.AddSingleton<IRepository, Repository>();

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
}

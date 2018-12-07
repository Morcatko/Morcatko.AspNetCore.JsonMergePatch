using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;
using Swashbuckle.AspNetCore.Swagger;

namespace testApp
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.AddJsonMergePatch();

			services.AddSingleton<IRepository, Repository>();

			services.AddSwaggerGen(c =>
			{
				c.OperationFilter<JsonMergePatchDocumentOperationFilter>();
				c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});
			app.UseMvc();
		}
	}
}

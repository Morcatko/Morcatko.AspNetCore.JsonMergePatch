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
                .AddJsonOptions(settings => {
                    settings.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    settings.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset;
                    settings.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                })
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

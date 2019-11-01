using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Integration.Server
{
	class MvcStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.AddNewtonsoftJson(o =>
				{
					o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
					o.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
					o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
				})
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
				.AddNewtonsoftJson(o =>
				{
					o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
					o.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
					o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
				})
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

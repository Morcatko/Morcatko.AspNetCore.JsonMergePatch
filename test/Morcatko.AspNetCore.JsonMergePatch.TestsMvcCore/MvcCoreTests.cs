using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.TestsMvcCore
{
    public class MvcCoreStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors().AddMvcCore().AddApplicationPart(typeof(DataController).Assembly)
                .AddJsonFormatters(settings =>
                {
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                    settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                })
                .AddJsonMergePatch()
                ;
            services.AddSingleton<IRepository, Repository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowCredentials());
            app.UseMvc();
        }
    }

    public class MvcCoreTests
    {
        public static TestServer CreateServer() => new TestServer(new WebHostBuilder().UseStartup<MvcCoreStartup>());

        private TestModel GetTestModel() => new TestModel { Integer = 5 };

        [Fact]
        public async Task PatchIntegers()
        {
            using (var server = CreateServer())
            {
                var response = await server.CreateClient().ClientPost("api/data/0", GetTestModel());
                response.EnsureSuccessStatusCode();
                await server.MergePatchAsync("api/data/0", new { id = 0, integer = 7 });

                var patchedModel = await server.GetAsync<TestModel>("api/data/0");
                var expected = GetTestModel();
                expected.Integer = 7;
                Assert.Equal(expected, patchedModel);
            }
        }
    }
}
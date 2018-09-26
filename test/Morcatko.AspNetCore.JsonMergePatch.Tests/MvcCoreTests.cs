using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Server;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests
{
    public class MvcCoreStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore() // Do not add the full Mvc
                .AddJsonFormatters() // required to have json input in [FromBody]
                .AddJsonMergePatch() // Services to be tested ;)
                ;
            services.AddSingleton<IRepository, Repository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
                var response = await server.PostAsync("api/dataCore/0", GetTestModel());
                response.EnsureSuccessStatusCode();
                await server.MergePatchAsync("api/dataCore/0", new { id = 0, integer = 7 });

                var patchedModel = await server.GetAsync<TestModel>("api/dataCore/0");
                var expected = GetTestModel();
                expected.Integer = 7;
                Assert.Equal(expected, patchedModel);
            }
        }
    }
}

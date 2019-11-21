using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch.SystemText;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	static class Helper
	{
		public static TestServer CreateServer(bool core, bool newtonsoft)
		{
			void ConfigureNewtonsoft(MvcNewtonsoftJsonOptions o)
			{
				o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
				o.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
				o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			}


			return new TestServer(new WebHostBuilder()
				.ConfigureServices(services =>
				{
					if (core)
					{
						var builder = services.AddMvcCore();
						if (newtonsoft)
							builder
								.AddNewtonsoftJson(ConfigureNewtonsoft)
								.AddNewtonsoftJsonMergePatch();
						else
							builder.AddSystemTextJsonMergePatch();
					}
					else
					{
						var builder = services.AddMvc();
						if (newtonsoft)
							builder
								.AddNewtonsoftJson(ConfigureNewtonsoft)
								.AddNewtonsoftJsonMergePatch();
						else
							builder.AddSystemTextJsonMergePatch();
					}
				})
				.UseStartup<Startup>());
		}

		public static HttpContent HttpContent(object data, string contentType)
		{
			var stringContent = JsonConvert.SerializeObject(data);
			return new StringContent(stringContent, Encoding.UTF8, contentType);
		}

		public static HttpContent JsonContent(object data) => HttpContent(data, "application/json");
		public static HttpContent MergePatchContent(object data) => HttpContent(data, JsonMergePatchDocument.ContentType);
		public static HttpContent JsonPatchContent(object data) => HttpContent(data, "application/json-patch+json");

		private static async Task<T> Parse<T>(Task<HttpResponseMessage> response) => JsonConvert.DeserializeObject<T>(await (await response).Content.ReadAsStringAsync());

		public static Task<HttpResponseMessage> JsonPatchAsync(this TestServer server, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = JsonPatchContent(model)).SendAsync("PATCH");
		public static Task<HttpResponseMessage> MergePatchAsync(this TestServer server, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = MergePatchContent(model)).SendAsync("PATCH");
		public static Task<HttpResponseMessage> PostAsync(this TestServer server, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = JsonContent(model)).SendAsync("POST");

		public static Task<T> MergePatchAsync<T>(this TestServer server, string uri, object model) => Parse<T>(MergePatchAsync(server, uri, model));
		public static Task<T> GetAsync<T>(this TestServer server, string uri) => Parse<T>(server.CreateRequest(uri).SendAsync("GET"));
	}
}

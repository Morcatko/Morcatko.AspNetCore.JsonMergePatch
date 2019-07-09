using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	static class Helper
	{
		public static TestServer CreateServer<TStartup>(Action<JsonMergePatchOptions> configure = null) where TStartup : class
			=> new TestServer(new WebHostBuilder()
				.ConfigureServices(services => services.Configure(configure ?? (_ => { })))
				.UseStartup<TStartup>());

		public static TestServer CreateMvcServer(Action<JsonMergePatchOptions> configure = null)
			=> CreateServer<MvcStartup>(configure);
		public static TestServer CreateMvcCoreServer(Action<JsonMergePatchOptions> configure = null)
			=> CreateServer<MvcCoreStartup>(configure);

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
		public static Task<T> MergePatchAsync<T>(this TestServer server, string uri, object model) => Parse<T>(MergePatchAsync(server, uri, model));
		public static Task<HttpResponseMessage> PostAsync(this TestServer server, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = JsonContent(model)).SendAsync("POST");
		public static Task<T> GetAsync<T>(this TestServer server, string uri) => Parse<T>(server.CreateRequest(uri).SendAsync("GET"));
	}
}

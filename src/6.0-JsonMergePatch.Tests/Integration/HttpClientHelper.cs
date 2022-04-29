using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	static class HttpClientHelper
	{
		public static HttpContent HttpContent(ISerializer serializer, object data, string contentType)
		{
			var stringContent = serializer.Serialize(data);
			return new StringContent(stringContent, Encoding.UTF8, contentType);
		}

		public static HttpContent JsonContent(ISerializer serializer, object data) => HttpContent(serializer, data, "application/json");
		public static HttpContent MergePatchContent(ISerializer serializer, object data) => HttpContent(serializer, data, JsonMergePatchDocument.ContentType);
		public static HttpContent JsonPatchContent(ISerializer serializer, object data) => HttpContent(serializer, data, "application/json-patch+json");

		private static async Task<T> Parse<T>(ISerializer serializer, Task<HttpResponseMessage> response)
			=> serializer.Deserialize<T>(await (await response).Content.ReadAsStringAsync());

		public static Task<HttpResponseMessage> JsonPatchAsync(this TestServer server, ISerializer serializer, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = JsonPatchContent(serializer, model)).SendAsync("PATCH");
		public static Task<HttpResponseMessage> MergePatchAsync(this TestServer server, ISerializer serializer, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = MergePatchContent(serializer, model)).SendAsync("PATCH");
		public static Task<HttpResponseMessage> PostAsync(this TestServer server, ISerializer serializer, string uri, object model) => server.CreateRequest(uri).And(r => r.Content = JsonContent(serializer, model)).SendAsync("POST");

		public static Task<T> MergePatchAsync<T>(this TestServer server, ISerializer serializer, string uri, object model) => Parse<T>(serializer, server.MergePatchAsync(serializer, uri, model));
		public static Task<T> GetAsync<T>(this TestServer server, ISerializer serializer, string uri) => Parse<T>(serializer, server.CreateRequest(uri).SendAsync("GET"));
	}
}

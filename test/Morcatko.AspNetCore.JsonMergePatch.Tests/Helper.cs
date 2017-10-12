using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Server.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Server
{
    static class Helper
    {
        public static TestServer CreateServer() => new TestServer(new WebHostBuilder().UseStartup<Startup>());

        public static HttpContent HttpContent(object data, string contentType)
        {
            var stringContent = JsonConvert.SerializeObject(data);
            return new StringContent(stringContent, Encoding.UTF8, contentType);
        }

        public static HttpContent JsonContent(object data) => HttpContent(data, "application/json");
        public static HttpContent PatchContent(object data) => HttpContent(data, JsonMergePatchDocument.ContentType);

        public static Task<HttpResponseMessage> PatchAsync(this TestServer server, string uri, object content)
        {
            var requestBuilder = server.CreateRequest(uri);
            requestBuilder.And(m => m.Content = Helper.PatchContent(content));
            return requestBuilder.SendAsync("PATCH");
        }

        public static Task PostAsync(this HttpClient client, string uri, object model) => client.PostAsync(uri, JsonContent(model));
        public static async Task<T> GetAsync<T>(this HttpClient client, string uri)
        {
            var result = await client.GetStringAsync(uri);
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

namespace Morcatko.AspNetCore.JsonMergePatch.TestsMvcCore
{
    public static class Helper
    {
        public static HttpContent HttpContent(object data, string contentType)
        {
            string stringContent = JsonConvert.SerializeObject(data);
            return new StringContent(stringContent, Encoding.UTF8, contentType);
        }

        public static HttpContent MergePatchContent(object data) => HttpContent(data, JsonMergePatchDocument.ContentType);

        public static Task<HttpResponseMessage> MergePatchAsync(this TestServer server, string uri, object model) =>
            server.CreateRequest(uri).And(r => r.Content = MergePatchContent(model)).SendAsync("PATCH");

        public static Task<T> GetAsync<T>(this TestServer server, string uri) => Parse<T>(server.CreateRequest(uri).SendAsync("GET"));

        public static Task<HttpResponseMessage> ClientPost<TEntity>(this HttpClient client, string url, TEntity entity)
        {
            string dataAsString = JsonConvert.SerializeObject(entity);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return client.PostAsync(url, content);
        }

        private static async Task<T> Parse<T>(Task<HttpResponseMessage> response) =>
            JsonConvert.DeserializeObject<T>(await (await response).Content.ReadAsStringAsync());
    }
}
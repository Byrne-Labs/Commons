using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ByrneLabs.Commons.Serialization;

namespace ByrneLabs.Commons.WebApi.Client
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient httpClient, string requestUri, T content, string contentMediaType = null)
        {
            var realContentMediaType = string.IsNullOrWhiteSpace(contentMediaType) ? httpClient.DefaultRequestHeaders.Accept.FirstOrDefault()?.MediaType : contentMediaType;
            var serializerAdapter = SerializerAdapterFactory.GetAdapter(realContentMediaType);
            var stream = new MemoryStream();
            serializerAdapter.Write(stream, content);
            var httpContent = new StreamContent(stream);
            httpContent.Headers.ContentType.MediaType = contentMediaType;

            return httpClient.PostAsync(requestUri, httpContent);
        }

        public static T ReadAs<T>(this HttpResponseMessage response)
        {
            var serializerAdapter = SerializerAdapterFactory.GetAdapter(response.Content.Headers.ContentType.MediaType);
            using (var stream = response.Content.ReadAsStreamAsync().Result)
            {
                return serializerAdapter.ReadAs<T>(stream);
            }
        }
    }
}

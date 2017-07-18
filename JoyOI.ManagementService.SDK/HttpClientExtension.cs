using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace JoyOI.ManagementService.SDK
{
    public static class HttpClientExtension
    {
        private static Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var method = new HttpMethod("PATCH");
            var message = new HttpRequestMessage(method, requestUri) { Content = content };
            return client.SendAsync(message, cancellationToken);
        }

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string endpoint, HttpContent content, CancellationToken cancellationToken = default(CancellationToken)) => client.PatchAsync(new Uri(client.BaseAddress.AbsoluteUri + requestUri), content, cancellationToken);
    }
}

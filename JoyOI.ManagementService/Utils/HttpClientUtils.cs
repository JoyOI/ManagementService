using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Utils
{
    public static class HttpClientUtils
    {
        private class JsonContent : HttpContent
        {
            public byte[] JsonBytes { get; set; }

            public JsonContent(object obj)
            {
                Headers.Add("Content-Type", "application/json; charset=utf-8");
                JsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return stream.WriteAsync(JsonBytes, 0, JsonBytes.Length);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = JsonBytes.Length;
                return true;
            }
        }

        /// <summary>
        /// 提交内容到远程服务器, 并返回回应的内容
        /// </summary>
        public static async Task<string> HttpInvokeAsync(
            string host, HttpMethod method, string endpoint, object body)
        {
            var handler = new HttpClientHandler();
            using (var client = new HttpClient(handler))
            {
                var message = new HttpRequestMessage()
                {
                    Method = method,
                    RequestUri = new Uri(host + endpoint),
                    Content = new JsonContent(body),
                };
                var result = await client.SendAsync(message);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await result.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new InvalidOperationException(await result.Content.ReadAsStringAsync());
                }
            }
        }
    }
}

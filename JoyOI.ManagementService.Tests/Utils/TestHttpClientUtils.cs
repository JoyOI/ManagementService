using JoyOI.ManagementService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestHttpClientUtils
    {
        [Fact]
        public async Task HttpInvokeAsync()
        {
            var postResult = await HttpClientUtils.HttpInvokeAsync(
                "http://httpbin.org", HttpMethod.Post, "/post", new { post = 123 });
            var postResultObj = JsonConvert.DeserializeObject<dynamic>(postResult);
            Assert.Equal("{\"post\":123}", (string)postResultObj.data);
            Assert.Equal("application/json; charset=utf-8", (string)postResultObj.headers["Content-Type"]);
            Assert.Equal("http://httpbin.org/post", (string)postResultObj.url);
        }
    }
}

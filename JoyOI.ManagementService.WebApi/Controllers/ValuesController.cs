using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JoyOI.ManagementService.DbContexts;
using System.Security.Cryptography.X509Certificates;
using Docker.DotNet.X509;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace JoyOI.ManagementService.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        public ValuesController(JoyOIManagementContext context)
        {

        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var credentials = new CertificateCredentials(new X509Certificate2("ClientCerts/docker-1.pfx", "123456"));
            var config = new DockerClientConfiguration(new Uri("http://docker-1:2376"), credentials);
            DockerClient client = config.CreateClient();
            var images = await client.Images.ListImagesAsync(new ImagesListParameters());
            return images.Select(x => x.ID + " " + string.Join(" ", x.RepoTags)).ToArray();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using JoyOI.ManagementService.Model.Dtos;

namespace JoyOI.ManagementService.SDK
{
    public class ManagementServiceClient : IDisposable
    {
        private const string _apiVersion = "/api/v1";
        private HttpClient _client;

        public ManagementServiceClient(IConfiguration config)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate(File.ReadAllBytes(config["ManagementService:Certification"]), config["ManagementService:Password"]));
            _client = new HttpClient(handler) { BaseAddress = new Uri(config["ManagementService:Url"]) };
        }

        #region Blobs
        public async Task<Guid> PutBlobAsync(string name, byte[] body, CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { Remark = name, Body = Convert.ToBase64String(body) }), Encoding.UTF8, "application/json");
            var result = await _client.PutAsync(_apiVersion + "/blob", stringContent, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Guid.Parse(JsonConvert.DeserializeObject<dynamic>(response).data.id);
            }
            throw new ManagementServiceException(response);
        }

        public async Task<IEnumerable<BlobOutputDto>> GetAllBlobsAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/blob/all", token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<JObject>(response)["data"].Value<IEnumerable<BlobOutputDto>>();
            }
            throw new ManagementServiceException(response);
        }

        public async Task<(string Name, byte[] Body)> GetBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/blob/" + id, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var json = JsonConvert.DeserializeObject<dynamic>(response);
                return (json.data.remark, Convert.FromBase64String(json.data.body));
            }
            throw new ManagementServiceException(response);
        }

        public async Task DeleteBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.DeleteAsync(_apiVersion + "/blob/" + id, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }
        #endregion

        #region StateMachineDefinition
        public async Task<IEnumerable<StateMachineOutputDto>> GetAllStateMachineDefinitions(CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/statemachine/all", token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<JObject>(response)["data"].Value<IEnumerable<StateMachineOutputDto>>();
            }
            throw new ManagementServiceException(response);
        }

        public async Task<StateMachineOutputDto> GetStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/statemachine/" + name, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<JObject>(response)["data"].Value<StateMachineOutputDto>();
            }
            throw new ManagementServiceException(response);
        }

        public async Task DeleteStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.DeleteAsync(_apiVersion + "/statemachine/" + name, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }

        public async Task PutStateMachineDefinitionAsync(string name, string code, ContainerLimitation limitation = null, CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(new
            {
                Name = name,
                Body = code,
                ContainerLimitation = limitation
            }), 
            Encoding.UTF8, 
            "application/json");
            var result = await _client.PutAsync(_apiVersion + "/statemachine", stringContent, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }

        public Task PutStateMachineDefinitionAsync(string name, string code, CancellationToken token = default(CancellationToken)) 
            => PutStateMachineDefinitionAsync(name, code, null, token);

        public async Task PatchStateMachineDefinitionAsync(string name, string code, ContainerLimitation limitation = null, CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(new
            {
                Name = name,
                Body = code,
                ContainerLimitation = limitation
            }),
           Encoding.UTF8,
           "application/json");
            var result = await _client.PatchAsync(_apiVersion + "/statemachine", stringContent, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }

        public Task PatchStateMachineDefinitionAsync(string name, string code, CancellationToken token = default(CancellationToken))
            => PatchStateMachineDefinitionAsync(name, code, null, token);
        #endregion

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}

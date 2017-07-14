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

        public void Dispose()
        {
            _client.Dispose();
        }

        #region Base
        private async Task<IEnumerable<T>> GetAllBaseAsync<T>(string controller, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/" + controller + "/all", token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<JObject>(response)["data"].Value<IEnumerable<T>>();
            }
            throw new ManagementServiceException(response);
        }

        private async Task<T> PutBaseAsync<T>(string controller, object body, string idFieldName = "id", CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var result = await _client.PutAsync(_apiVersion + "/" + controller, stringContent, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var obj = JsonConvert.DeserializeObject<JObject>(response);
                return obj["data"][idFieldName].Value<T>();
            }
            throw new ManagementServiceException(response);
        }

        private async Task PutBaseAsync(string controller, object body, CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var result = await _client.PutAsync(_apiVersion + "/" + controller, stringContent, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            throw new ManagementServiceException(response);
        }

        private async Task<TReturn> GetBaseAsync<TResponse, TReturn>(string controller, object id, Func<TResponse, TReturn> convert, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/" + controller + "/" + id, token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var obj = JsonConvert.DeserializeObject<JObject>(response);
                var val = obj["data"].Value<TResponse>();
                return convert(val);
            }
            throw new ManagementServiceException(response);
        }

        private Task<T> GetBaseAsync<T>(string controller, object id, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<T, T>(controller, id, x => x, token);

        private async Task DeleteBaseAsync(string controller, object id, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.DeleteAsync(_apiVersion + "/" + controller + "/" + id, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }

        private async Task PatchBaseAsync(string controller, object id, object body, CancellationToken token = default(CancellationToken))
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(body),
           Encoding.UTF8,
           "application/json");
            var result = await _client.PatchAsync(_apiVersion + "/" + controller + "/" + id, stringContent, token);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            var response = await result.Content.ReadAsStringAsync();
            throw new ManagementServiceException(response);
        }
        #endregion

        #region Blobs
        public Task<IEnumerable<BlobOutputDto>> GetAllBlobsAsync(CancellationToken token = default(CancellationToken))
            => GetAllBaseAsync<BlobOutputDto>("blob", token);

        public Task<Guid> PutBlobAsync(string name, byte[] body, CancellationToken token = default(CancellationToken))
            => PutBaseAsync<Guid>("blob", new { Remark = name, Body = Convert.ToBase64String(body) }, "id", token);

        public Task<(string Name, byte[] Body, long Timestamp)> GetBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<BlobOutputDto, (string Name, byte[] Body, long Timestamp)>("blob", id, x => (x.Remark, Convert.FromBase64String(x.Body), x.TimeStamp), token);

        public Task DeleteBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync("blob", id, token);
        #endregion

        #region StateMachineDefinition
        public Task<IEnumerable<StateMachineOutputDto>> GetAllStateMachineDefinitions(CancellationToken token = default(CancellationToken))
            => GetAllBaseAsync<StateMachineOutputDto>("statemachine", token);

        public Task<StateMachineOutputDto> GetStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<StateMachineOutputDto>("statemachine", name, token);

        public Task DeleteStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync("statemachine", name, token);

        public Task PutStateMachineDefinitionAsync(string name, string code, ContainerLimitation limitation = null, CancellationToken token = default(CancellationToken))
            => PutBaseAsync(
                "statemachine",
                new
                {
                    Name = name,
                    Body = code,
                    ContainerLimitation = limitation
                },
                token);

        public Task PutStateMachineDefinitionAsync(string name, string code, CancellationToken token = default(CancellationToken))
            => PutStateMachineDefinitionAsync(name, code, null, token);

        public Task PatchStateMachineDefinitionAsync(string name, string code, ContainerLimitation limitation = null, CancellationToken token = default(CancellationToken))
            => PatchBaseAsync(
                "statemachine", 
                name, 
                new
                {
                    Name = name,
                    Body = code,
                    ContainerLimitation = limitation
                }, token);

        public Task PatchStateMachineDefinitionAsync(string name, string code, CancellationToken token = default(CancellationToken))
            => PatchStateMachineDefinitionAsync(name, code, null, token);
        #endregion

        #region Actor
        public Task<IEnumerable<ActorOutputDto>> GetAllActorsAsync(CancellationToken token = default(CancellationToken))
            => GetAllBaseAsync<ActorOutputDto>("actor", token);

        public Task<ActorOutputDto> GetActorAsync(string name, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<ActorOutputDto>("actor", name, token);

        public Task DeleteActorAsync(string name, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync("actor", name, token);

        public Task PutActorAsync(string name, string code, CancellationToken token)
            => PutBaseAsync("actor", new { Name = name, Body = code }, token);

        public Task PatchActorAsync(string name, string code, CancellationToken token)
            => PatchBaseAsync("actor", name, new { Body = code }, token);
        #endregion

        #region StateMachineInstance

        #endregion
    }
}

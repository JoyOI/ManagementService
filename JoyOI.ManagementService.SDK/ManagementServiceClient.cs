using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Model.Dtos;

namespace JoyOI.ManagementService.SDK
{
    public class ManagementServiceClient : IDisposable
    {
        private const string _apiVersion = "/api/v1";
        private const string _blobController = "blob";
        private const string _stateMachineDefinitionController = "statemachine";
        private const string _stateMachineInstanceController = "statemachineinstance";
        private const string _actorController = "actor";
        private HttpClient _client;

        public ManagementServiceClient(IConfiguration config)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(File.ReadAllBytes(config["ManagementService:Certification"]), config["ManagementService:Password"]));
            _client = new HttpClient(handler) { BaseAddress = new Uri(config["ManagementService:Url"]) };
        }

        public ManagementServiceClient(string url, string certPath, string password)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(File.ReadAllBytes(certPath), password));
            _client = new HttpClient(handler) { BaseAddress = new Uri(url) };
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        #region Base
        private async Task<IEnumerable<T>> GetAllBaseAsync<T>(string controller, string queryString = null, CancellationToken token = default(CancellationToken))
        {
            var result = await _client.GetAsync(_apiVersion + "/" + controller + "/all" + (string.IsNullOrEmpty(queryString) ? "" : queryString), token);
            var response = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<JObject>(response)["data"].Values<T>();
            }
            throw new ManagementServiceException(response);
        }

        private Task<IEnumerable<T>> GetAllBaseAsync<T>(string controller, CancellationToken token = default(CancellationToken))
            => GetAllBaseAsync<T>(controller, null, token);

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
            => GetAllBaseAsync<BlobOutputDto>(_blobController, token);

        public Task<Guid> PutBlobAsync(string name, byte[] body, CancellationToken token = default(CancellationToken))
            => PutBaseAsync<Guid>(_blobController, new { Remark = name, Body = Convert.ToBase64String(body) }, "id", token);

        public Task<(string Name, byte[] Body, long Timestamp)> GetBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<BlobOutputDto, (string Name, byte[] Body, long Timestamp)>(_blobController, id, x => (x.Remark, Convert.FromBase64String(x.Body), x.TimeStamp), token);

        public Task DeleteBlobAsync(Guid id, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync(_blobController, id, token);
        #endregion

        #region StateMachineDefinition
        public Task<IEnumerable<StateMachineOutputDto>> GetAllStateMachineDefinitionsAsync(CancellationToken token = default(CancellationToken))
            => GetAllBaseAsync<StateMachineOutputDto>("statemachine", token);

        public Task<StateMachineOutputDto> GetStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<StateMachineOutputDto>("statemachine", name, token);

        public Task DeleteStateMachineDefinitionAsync(string name, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync("statemachine", name, token);

        public Task PutStateMachineDefinitionAsync(string name, string code, ContainerLimitation limitation = null, CancellationToken token = default(CancellationToken))
            => PutBaseAsync(
               _stateMachineDefinitionController,
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
               _stateMachineDefinitionController,
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

        public Task PutActorAsync(string name, string code, CancellationToken token = default(CancellationToken))
            => PutBaseAsync("actor", new { Name = name, Body = code }, token);

        public Task PatchActorAsync(string name, string code, CancellationToken token = default(CancellationToken))
            => PatchBaseAsync("actor", name, new { Body = code }, token);
        #endregion

        #region StateMachineInstance
        public Task<IEnumerable<StateMachineInstanceOutputDto>> GetAllStateMachineInstancesAsync(string stateMachineName, string stage = null, StateMachineStatus? status = null, DateTime? begin = null, DateTime? end = null, CancellationToken token = default(CancellationToken))
        {
            var queryString = new QueryString();
            if (!string.IsNullOrWhiteSpace(stateMachineName))
                queryString = queryString.Add("name", stateMachineName);
            if (!string.IsNullOrWhiteSpace(stage))
                queryString = queryString.Add("stage", stage);
            if (status.HasValue)
                queryString = queryString.Add("status", status.Value.ToString());
            if (begin.HasValue)
                queryString = queryString.Add("begin", begin.Value.ToString());
            if (end.HasValue)
                queryString = queryString.Add("end", end.Value.ToString());

            return GetAllBaseAsync<StateMachineInstanceOutputDto>("statemachineinstance", queryString.Value, token);
        }

        public Task<StateMachineInstanceOutputDto> GetStateMachineInstanceAsync(Guid id, CancellationToken token = default(CancellationToken))
            => GetBaseAsync<StateMachineInstanceOutputDto>("statemachineinstance", id, token);

        public Task DeleteStateMachineInstanceAsync(Guid id, CancellationToken token = default(CancellationToken))
            => DeleteBaseAsync(_stateMachineInstanceController, id, token);

        public Task<Guid> PutStateMachineInstanceAsync(string stateMachineName,string host = null, IEnumerable<BlobInfo> blobs = null, CancellationToken token = default(CancellationToken))
            => PutBaseAsync<Guid>(_stateMachineInstanceController, new { Name = stateMachineName, InitialBlobs = blobs, Parameters = new { Host = host } });

        public Task PatchStateMachineInstanceAsync(Guid id, string stage, CancellationToken token = default(CancellationToken))
            => PatchBaseAsync(_stateMachineInstanceController, id, new { Stage = stage }, token);
        #endregion
    }
}

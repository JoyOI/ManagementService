using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services.Impl;
using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public class TestStateMachineInstanceStore : TestServiceBase
    {
        private StateMachineInstanceStore _store;

        public TestStateMachineInstanceStore()
        {
            _store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration),
                new DynamicCompileService());
            _store.Initialize(
                () => new EmptyDisposable(),
                _ => new DummyRepository<BlobEntity, Guid>(_storage),
                _ => new DummyRepository<ActorEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineInstanceEntity, Guid>(_storage));
        }

        private async Task<(StateMachineEntity, StateMachineInstanceEntity)> PutTestInstance()
        {
            var putDto = await PutSimpleDataSet();
            var stateMachineRepository = new DummyRepository<StateMachineEntity, Guid>(_storage);
            var stateMachineInstanceRepository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            var stateMachineEntity = await stateMachineRepository.QueryNoTrackingAsync(q =>
                q.FirstOrDefaultAsyncTestable(x => x.Name == putDto.Name));
            var stateMachineInstanceEntity = new StateMachineInstanceEntity()
            {
                Id = PrimaryKeyUtils.Generate<Guid>(),
                Name = putDto.Name,
                Status = StateMachineStatus.Running,
                Stage = StateMachineBase.InitialStage,
                InitialBlobs = putDto.InitialBlobs,
                Limitation = _configuration.Limitation,
                Parameters = new Dictionary<string, string>() { { "Host", "http://localhost:8888" } },
                Priority = 123,
                FromManagementService = _configuration.Name,
                ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString(),
                StartTime = DateTime.UtcNow
            };
            await stateMachineInstanceRepository.AddAsync(stateMachineInstanceEntity);
            await stateMachineInstanceRepository.SaveChangesAsync();
            return (stateMachineEntity, stateMachineInstanceEntity);
        }

        [Fact]
        public async Task ContinueRunningInstances()
        {
            var (_, entity) = await PutTestInstance();
            var store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration),
                new DynamicCompileService());
            store.Initialize(
                () => new EmptyDisposable(),
                _ => new DummyRepository<BlobEntity, Guid>(_storage),
                _ => new DummyRepository<ActorEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineInstanceEntity, Guid>(_storage));
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            while (true)
            {
                var stateMachine = await repository.QueryNoTrackingAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Id == entity.Id));
                if (stateMachine.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachine.Exception);
                if (stateMachine.Status == StateMachineStatus.Succeeded)
                {
                    Assert.Equal(1, stateMachine.ReRunTimes);
                    break;
                }
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task CreateInstance()
        {
            var (stateMachineEntity, stateMachineInstanceEntity) = await PutTestInstance();
            var instance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            Assert.Equal(stateMachineInstanceEntity.Id, instance.Id);
            Assert.Equal(stateMachineInstanceEntity.ExecutionKey, instance.ExecutionKey);
            Assert.Equal(stateMachineInstanceEntity.Status, instance.Status);
            Assert.Equal(stateMachineInstanceEntity.Stage, instance.Stage);
            Assert.Equal(
                JsonConvert.SerializeObject(stateMachineInstanceEntity.StartedActors),
                JsonConvert.SerializeObject(instance.StartedActors));
            Assert.Equal(
                JsonConvert.SerializeObject(stateMachineInstanceEntity.InitialBlobs),
                JsonConvert.SerializeObject(instance.InitialBlobs));
            Assert.Equal(
                JsonConvert.SerializeObject(stateMachineInstanceEntity.Parameters),
                JsonConvert.SerializeObject(instance.Parameters));
            Assert.Equal(stateMachineInstanceEntity.Priority, instance.Priority);
            var otherInstance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            Assert.True(!object.ReferenceEquals(instance, otherInstance));
        }

        [Fact]
        public async Task RunInstance()
        {
            var (stateMachineEntity, stateMachineInstanceEntity) = await PutTestInstance();
            var instance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            await _store.RunInstance(instance);
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            while (true)
            {
                var stateMachine = await repository.QueryNoTrackingAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Id == stateMachineInstanceEntity.Id));
                if (stateMachine.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachine.Exception);
                if (stateMachine.Status == StateMachineStatus.Succeeded)
                {
                    Assert.Equal("SimpleStateMachine", stateMachine.Name);
                    Assert.Equal(StateMachineStatus.Succeeded, stateMachine.Status);
                    Assert.Equal(StateMachineBase.FinalStage, stateMachine.Stage);
                    Assert.Equal(2, stateMachine.StartedActors.Count);

                    Assert.Equal("CompileUserCodeActor", stateMachine.StartedActors[0].Name);
                    Assert.True(stateMachine.StartedActors[0].StartTime != DateTime.MinValue);
                    Assert.True(stateMachine.StartedActors[0].EndTime != null);
                    Assert.Equal(1, stateMachine.StartedActors[0].Inputs.Count());
                    Assert.Equal(stateMachine.InitialBlobs[0].Id, stateMachine.StartedActors[0].Inputs.First().Id);
                    Assert.Equal(stateMachine.InitialBlobs[0].Name, stateMachine.StartedActors[0].Inputs.First().Name);
                    Assert.Equal(stateMachine.InitialBlobs[0].Tag, stateMachine.StartedActors[0].Inputs.First().Tag);
                    Assert.Equal(4, stateMachine.StartedActors[0].Outputs.Count());
                    Assert.True(stateMachine.StartedActors[0].Outputs.Any(x => x.Name == "runner.json"));
                    Assert.True(stateMachine.StartedActors[0].Outputs.Any(x => x.Name == "Main.out"));
                    Assert.True(stateMachine.StartedActors[0].Outputs.Any(x => x.Name == "stdout.txt"));
                    Assert.True(stateMachine.StartedActors[0].Outputs.Any(x => x.Name == "stderr.txt"));
                    Assert.Equal(0, stateMachine.StartedActors[0].Exceptions.Length);
                    Assert.Equal(ActorStatus.Succeeded, stateMachine.StartedActors[0].Status);
                    Assert.Equal("CompileUserCode", stateMachine.StartedActors[0].Stage);
                    Assert.True(string.IsNullOrEmpty(stateMachine.StartedActors[0].Tag));
                    Assert.True(!string.IsNullOrEmpty(stateMachine.StartedActors[0].UsedNode));
                    Assert.True(!string.IsNullOrEmpty(stateMachine.StartedActors[0].UsedContainer));

                    Assert.Equal("RunUserCodeActor", stateMachine.StartedActors[1].Name);
                    Assert.True(stateMachine.StartedActors[1].StartTime != DateTime.MinValue);
                    Assert.True(stateMachine.StartedActors[1].EndTime != null);
                    Assert.Equal(
                        JsonConvert.SerializeObject(stateMachine.StartedActors[0].Outputs),
                        JsonConvert.SerializeObject(stateMachine.StartedActors[1].Inputs));
                    Assert.Equal(3, stateMachine.StartedActors[1].Outputs.Count());
                    Assert.True(stateMachine.StartedActors[1].Outputs.Any(x => x.Name == "runner.json"));
                    Assert.True(stateMachine.StartedActors[1].Outputs.Any(x => x.Name == "stdout.txt"));
                    Assert.True(stateMachine.StartedActors[1].Outputs.Any(x => x.Name == "stderr.txt"));
                    var blob = stateMachine.StartedActors[1].Outputs.FindBlob("stdout.txt");
                    var stdout = (await _store.ReadBlobs(new[] { blob })).Last().Item2;
                    Assert.Equal("simple state machine is ok\r\n", Encoding.UTF8.GetString(stdout));
                    Assert.Equal(0, stateMachine.StartedActors[1].Exceptions.Length);
                    Assert.Equal(ActorStatus.Succeeded, stateMachine.StartedActors[1].Status);
                    Assert.Equal("RunUserCode", stateMachine.StartedActors[1].Stage);
                    Assert.True(string.IsNullOrEmpty(stateMachine.StartedActors[1].Tag));
                    Assert.True(!string.IsNullOrEmpty(stateMachine.StartedActors[1].UsedNode));
                    Assert.True(!string.IsNullOrEmpty(stateMachine.StartedActors[1].UsedContainer));

                    Assert.Equal(
                        JsonConvert.SerializeObject(_configuration.Limitation),
                        JsonConvert.SerializeObject(stateMachine.Limitation));
                    Assert.Equal(_configuration.Name, stateMachine.FromManagementService);
                    Assert.Equal(0, stateMachine.ReRunTimes);
                    Assert.True(string.IsNullOrEmpty(stateMachine.Exception));
                    Assert.True(!string.IsNullOrEmpty(stateMachine.ExecutionKey));
                    Assert.True(stateMachine.StartTime != DateTime.MinValue);
                    Assert.True(stateMachine.EndTime != null);
                    break;
                }
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task SetInstanceStage()
        {
            var (stateMachineEntity, stateMachineInstanceEntity) = await PutTestInstance();
            var instance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            await _store.SetInstanceStage(instance, StateMachineBase.FinalStage);
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            stateMachineInstanceEntity = await repository.QueryNoTrackingAsync(q =>
                q.FirstOrDefaultAsyncTestable(x => x.Id == stateMachineInstanceEntity.Id));
            Assert.Equal(StateMachineBase.FinalStage, stateMachineInstanceEntity.Stage);
        }

        [Fact]
        public async Task SetInstanceStage_ObsoleteExecutionKey()
        {
            var (stateMachineEntity, stateMachineInstanceEntity) = await PutTestInstance();
            var instance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            stateMachineInstanceEntity.ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString();
            repository.Update(stateMachineInstanceEntity);
            await repository.SaveChangesAsync();
            await Assert.ThrowsAsync<StateMachineInterpretedException>(
                () => _store.SetInstanceStage(instance, StateMachineBase.FinalStage));
            repository.Remove(stateMachineInstanceEntity);
            await repository.SaveChangesAsync();
            await Assert.ThrowsAsync<StateMachineInterpretedException>(
                () => _store.SetInstanceStage(instance, StateMachineBase.FinalStage));
        }

        [Fact]
        public async Task RunActorsAsync()
        {
            var (stateMachineEntity, stateMachineInstanceEntity) = await PutTestInstance();
            var instance = await _store.CreateInstance(stateMachineEntity, stateMachineInstanceEntity);
            var actorInfos = new List<ActorInfo>();
            var startTime = DateTime.UtcNow;
            for (var x = 0; x < 3; ++x)
            {
                actorInfos.Add(new ActorInfo()
                {
                    Name = "CompileUserCodeActor",
                    StartTime = startTime,
                    Inputs = stateMachineInstanceEntity.InitialBlobs,
                    Outputs = new BlobInfo[0],
                    Exceptions = new string[0],
                    Status = ActorStatus.Running,
                    Stage = "CompileUserCode"
                });
            }
            await _store.RunActors(instance, actorInfos);
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            stateMachineInstanceEntity = await repository.QueryNoTrackingAsync(q =>
                q.FirstOrDefaultAsyncTestable(x => x.Id == stateMachineInstanceEntity.Id));
            Assert.Equal(3, stateMachineInstanceEntity.StartedActors.Count);
            foreach (var actorInfo in stateMachineInstanceEntity.StartedActors)
            {
                Assert.Equal("CompileUserCodeActor", actorInfo.Name);
                Assert.Equal(startTime, actorInfo.StartTime);
                Assert.True(actorInfo.EndTime != null);
                Assert.Equal(
                    JsonConvert.SerializeObject(stateMachineInstanceEntity.InitialBlobs),
                    JsonConvert.SerializeObject(actorInfo.Inputs));
                Assert.Equal(4, actorInfo.Outputs.Count());
                Assert.True(actorInfo.Outputs.Any(x => x.Name == "runner.json"));
                Assert.True(actorInfo.Outputs.Any(x => x.Name == "Main.out"));
                Assert.True(actorInfo.Outputs.Any(x => x.Name == "stdout.txt"));
                Assert.True(actorInfo.Outputs.Any(x => x.Name == "stderr.txt"));
                Assert.Equal(0, actorInfo.Exceptions.Length);
                Assert.Equal(ActorStatus.Succeeded, actorInfo.Status);
                Assert.Equal("CompileUserCode", actorInfo.Stage);
                Assert.True(actorInfo.Tag == null);
                Assert.True(actorInfo.UsedNode != null);
                Assert.True(actorInfo.UsedContainer != null);
            }
        }

        [Fact]
        public async Task PutBlob_ReadBlobs()
        {
            var now = DateTime.UtcNow;
            var blobIdA = await _store.PutBlob("a.txt", Encoding.UTF8.GetBytes("content of a.txt"), now);
            var blobIdB = await _store.PutBlob("b.txt", Encoding.UTF8.GetBytes("content of b.txt"), now);
            var readResult = (await _store.ReadBlobs(new[]
            {
                new BlobInfo(blobIdA, "read_a.txt", "tag_a"),
                new BlobInfo(blobIdB, "read_b.txt", "tag_b"),
            })).ToList();
            Assert.Equal(2, readResult.Count);
            Assert.Equal(blobIdA, readResult[0].Item1.Id);
            Assert.Equal("read_a.txt", readResult[0].Item1.Name);
            Assert.Equal("tag_a", readResult[0].Item1.Tag);
            Assert.Equal("content of a.txt", Encoding.UTF8.GetString(readResult[0].Item2));
            Assert.Equal(blobIdB, readResult[1].Item1.Id);
            Assert.Equal("read_b.txt", readResult[1].Item1.Name);
            Assert.Equal("tag_b", readResult[1].Item1.Tag);
            Assert.Equal("content of b.txt", Encoding.UTF8.GetString(readResult[1].Item2));
        }

        [Fact]
        public async Task ReadActorCode()
        {
            await PutSimpleDataSet();
            var code = await _store.ReadActorCode("CompileUserCodeActor");
            Assert.True(!string.IsNullOrEmpty(code));
            var notExistCode = await _store.ReadActorCode("NotExistActor");
            Assert.True(string.IsNullOrEmpty(notExistCode));
        }
    }
}

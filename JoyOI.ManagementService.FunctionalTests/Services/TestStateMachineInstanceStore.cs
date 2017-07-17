using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services.Impl;
using JoyOI.ManagementService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public async Task ContinueRunningInstances()
        {
            var putDto = await PutSimpleDataSet();
            var repository = new DummyRepository<StateMachineInstanceEntity, Guid>(_storage);
            var entity = new StateMachineInstanceEntity()
            {
                Id = PrimaryKeyUtils.Generate<Guid>(),
                Name = putDto.Name,
                Status = StateMachineStatus.Running,
                Stage = StateMachineBase.InitialStage,
                InitialBlobs = putDto.InitialBlobs,
                Limitation = _configuration.Limitation,
                FromManagementService = _configuration.Name,
                ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString(),
                StartTime = DateTime.UtcNow
            };
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
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

        [Fact(Skip = "TODO")]
        public void CreateInstance()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void RunInstance()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void SetInstanceStage()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void RunActors()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void PutBlob()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void ReadBlobs()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void ReadActorCode()
        {
            // TODO
        }
    }
}

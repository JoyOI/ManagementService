using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services.Impl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public class TestStateMachineInstanceService : TestServiceBase
    {
        private StateMachineInstanceStore _store;
        private StateMachineInstanceService _service;
        private ITestOutputHelper _outputHelper;

        public TestStateMachineInstanceService(ITestOutputHelper outputHelper)
        {
            _store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration, new NotificationService()),
                new DynamicCompileService());
            _store.Initialize(
                () => new EmptyDisposable(),
                _ => new DummyRepository<BlobEntity, Guid>(_storage),
                _ => new DummyRepository<ActorEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineInstanceEntity, Guid>(_storage));
            _service = new StateMachineInstanceService(
                _configuration,
                _store,
                new DummyRepository<StateMachineInstanceEntity, Guid>(_storage),
                new DummyRepository<StateMachineEntity, Guid>(_storage));
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Search()
        {
            var putDto = await PutSimpleDataSet();
            var putResultA = await _service.Put(putDto);
            var putResultB = await _service.Put(putDto);
            Assert.Equal(200, putResultA.Code);
            Assert.Equal(200, putResultB.Code);
            var stateMachines = await _service.Search(
                "SimpleStateMachine", null, null, null, null);
            Assert.Equal(2, stateMachines.Count);
            Assert.True(stateMachines.All(x => x.Name == "SimpleStateMachine"));
            while (true)
            {
                stateMachines = await _service.Search(
                    "SimpleStateMachine", null, null, null, null);
                foreach (var stateMachine in stateMachines)
                {
                    if (stateMachine.Status == StateMachineStatus.Failed)
                        throw new InvalidOperationException(stateMachine.Exception);
                }
                if (stateMachines.All(x => x.Status == StateMachineStatus.Succeeded))
                {
                    stateMachines = await _service.Search(
                        "SimpleStateMachine", StateMachineBase.FinalStage, null, null, null);
                    Assert.Equal(2, stateMachines.Count);
                    break;
                }
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task Get()
        {
            var putDto = await PutSimpleDataSet();
            var putResultA = await _service.Put(putDto);
            var putResultB = await _service.Put(putDto);
            Assert.Equal(200, putResultA.Code);
            Assert.Equal(200, putResultB.Code);
            while (true)
            {
                var stateMachineA = await _service.Get(putResultA.Instance.Id);
                var stateMachineB = await _service.Get(putResultB.Instance.Id);
                Assert.True(stateMachineA != null);
                Assert.True(stateMachineB != null);
                if (stateMachineA.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachineA.Exception);
                if (stateMachineB.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachineB.Exception);
                if (stateMachineA.Status == StateMachineStatus.Succeeded &&
                    stateMachineB.Status == StateMachineStatus.Succeeded)
                    break;
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task Put()
        {
            var putDto = await PutSimpleDataSet();
            var stateMachineIds = new List<Guid>();
            for (var x = 0; x < 30; ++x)
            {
                stateMachineIds.Add((await _service.Put(putDto)).Instance.Id);
            }
            while (true)
            {
                var stateMachines = await _service.Search(null, null, null, null, null);
                foreach (var stateMachine in stateMachines)
                {
                    if (stateMachine.Status == StateMachineStatus.Failed)
                        throw new InvalidOperationException(stateMachine.Exception);
                }
                if (stateMachines.All(x => x.Status == StateMachineStatus.Succeeded))
                {
                    stateMachines = await _service.Search(null, null, null, null, null);
                    Assert.Equal(stateMachineIds.Count, stateMachines.Count);
                    foreach (var stateMachine in stateMachines)
                    {
                        Assert.True(stateMachineIds.Contains(stateMachine.Id));
                        Assert.Equal("SimpleStateMachine", stateMachine.Name);
                        Assert.Equal(StateMachineStatus.Succeeded, stateMachine.Status);
                        Assert.Equal(StateMachineBase.FinalStage, stateMachine.Stage);
                        Assert.Equal(2, stateMachine.StartedActors.Count);

                        Assert.Equal("CompileUserCodeActor", stateMachine.StartedActors[0].Name);
                        Assert.True(stateMachine.StartedActors[0].StartTime != DateTime.MinValue);
                        Assert.True(stateMachine.StartedActors[0].EndTime != null);
                        Assert.Equal(1, stateMachine.StartedActors[0].Inputs.Count());
                        Assert.Equal(putDto.InitialBlobs[0].Id, stateMachine.StartedActors[0].Inputs.First().Id);
                        Assert.Equal(putDto.InitialBlobs[0].Name, stateMachine.StartedActors[0].Inputs.First().Name);
                        Assert.Equal(putDto.InitialBlobs[0].Tag, stateMachine.StartedActors[0].Inputs.First().Tag);
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
                            JsonConvert.SerializeObject(putDto.InitialBlobs),
                            JsonConvert.SerializeObject(stateMachine.InitialBlobs));
                        Assert.Equal(
                            JsonConvert.SerializeObject(_configuration.Limitation),
                            JsonConvert.SerializeObject(stateMachine.Limitation));
                        Assert.Equal(_configuration.Name, stateMachine.FromManagementService);
                        Assert.Equal(0, stateMachine.ReRunTimes);
                        Assert.True(string.IsNullOrEmpty(stateMachine.Exception));
                        Assert.True(!string.IsNullOrEmpty(stateMachine.ExecutionKey));
                        Assert.True(stateMachine.StartTime != DateTime.MinValue);
                        Assert.True(stateMachine.EndTime != null);
                    }
                    break;
                }
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task PutError()
        {
            var putDto = await PutSimpleDataSet();
            putDto.InitialBlobs[0].Id = await PutTestBlob("incorrect Main.c",
                Encoding.UTF8.GetBytes("#include <stdiox.h>\r\nint main() { }"));
            var stateMachineId = (await _service.Put(putDto)).Instance.Id;
            while (true)
            {
                var stateMachine = await _service.Get(stateMachineId);
                if (stateMachine.Status == StateMachineStatus.Failed)
                {
                    Assert.True(
                        stateMachine.Exception.Contains("stdiox.h: No such file or directory"),
                        stateMachine.Exception);
                    break;
                }
                Assert.False(stateMachine.Status == StateMachineStatus.Succeeded);
                await Task.Delay(1);
            }
        }

        [Fact]
        public async Task Patch()
        {
            var putDto = await PutSimpleDataSet();
            var stateMachineId = (await _service.Put(putDto)).Instance.Id;
            StateMachineInstanceOutputDto oldStateMachine;
            StateMachineInstanceOutputDto patchToRunStateMachine;
            StateMachineInstanceOutputDto patchToStartStateMachine;
            while (true)
            {
                var stateMachine = await _service.Get(stateMachineId);
                if (stateMachine.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachine.Exception);
                if (stateMachine.Status == StateMachineStatus.Succeeded)
                {
                    oldStateMachine = stateMachine;
                    break;
                }
                await Task.Delay(1);
            }
            _outputHelper.WriteLine(JsonConvert.SerializeObject(oldStateMachine, Formatting.Indented));
            // 修改运行状态到执行代码
            var patchResult = await _service.Patch(stateMachineId,
                new StateMachineInstancePatchDto() { Stage = "RunUserCode" });
            Assert.Equal(200, patchResult.Code);
            while (true)
            {
                var stateMachine = await _service.Get(stateMachineId);
                if (stateMachine.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachine.Exception);
                if (stateMachine.Status == StateMachineStatus.Succeeded)
                {
                    patchToRunStateMachine = stateMachine;
                    break;
                }
                await Task.Delay(1);
            }
            _outputHelper.WriteLine(JsonConvert.SerializeObject(patchToRunStateMachine, Formatting.Indented));
            // 修改运行状态到开始
            patchResult = await _service.Patch(stateMachineId,
                new StateMachineInstancePatchDto() { Stage = StateMachineBase.InitialStage });
            Assert.Equal(200, patchResult.Code);
            while (true)
            {
                var stateMachine = await _service.Get(stateMachineId);
                if (stateMachine.Status == StateMachineStatus.Failed)
                    throw new InvalidOperationException(stateMachine.Exception);
                if (stateMachine.Status == StateMachineStatus.Succeeded)
                {
                    patchToStartStateMachine = stateMachine;
                    break;
                }
                await Task.Delay(1);
            }
            _outputHelper.WriteLine(JsonConvert.SerializeObject(patchToStartStateMachine, Formatting.Indented));
            // 检查修改运行状态到执行代码, 是否只有RunUserCodeActor不同
            Assert.Equal(2, oldStateMachine.StartedActors.Count);
            Assert.Equal(2, patchToRunStateMachine.StartedActors.Count);
            Assert.Equal(
                oldStateMachine.StartedActors[0].UsedContainer,
                patchToRunStateMachine.StartedActors[0].UsedContainer);
            Assert.NotEqual(
                oldStateMachine.StartedActors[1].UsedContainer,
                patchToRunStateMachine.StartedActors[1].UsedContainer);
            // 检查修改运行状态到开始, 是否全部Actor都不同
            Assert.Equal(2, patchToStartStateMachine.StartedActors.Count);
            Assert.NotEqual(
                oldStateMachine.StartedActors[0].UsedContainer,
                patchToStartStateMachine.StartedActors[0].UsedContainer);
            Assert.NotEqual(
                oldStateMachine.StartedActors[1].UsedContainer,
                patchToStartStateMachine.StartedActors[1].UsedContainer);
        }

        [Fact]
        public async Task Delete()
        {
            var putDto = await PutSimpleDataSet();
            var putResultA = await _service.Put(putDto);
            var putResultB = await _service.Put(putDto);
            Assert.Equal(200, putResultA.Code);
            Assert.Equal(200, putResultB.Code);
            var deleteResultA = await _service.Delete(putResultA.Instance.Id);
            var deleteResultB = await _service.Delete(putResultB.Instance.Id);
            Assert.Equal(1, deleteResultA);
            Assert.Equal(1, deleteResultB);
            var getResultA = await _service.Get(putResultA.Instance.Id);
            var getResultB = await _service.Get(putResultB.Instance.Id);
            Assert.True(getResultA == null);
            Assert.True(getResultB == null);
        }
    }
}

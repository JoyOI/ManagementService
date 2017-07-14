using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Services.Impl;
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
            _context = _contextFactory();
            _store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration),
                new DynamicCompileService());
            _store.Initialize(_contextFactory);
            _service = new StateMachineInstanceService(
                _configuration,
                _context,
                _store);
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
            var stateMachines = await _service.Search("SimpleStateMachine", null);
            Assert.Equal(2, stateMachines.Count);
            Assert.True(stateMachines.All(x => x.Name == "SimpleStateMachine"));
            while (true)
            {
                stateMachines = await _service.Search("SimpleStateMachine", null);
                foreach (var stateMachine in stateMachines)
                {
                    if (stateMachine.Status == StateMachineStatus.Failed)
                        throw new InvalidOperationException(stateMachine.Exception);
                }
                if (stateMachines.All(x => x.Status == StateMachineStatus.Succeeded))
                {
                    stateMachines = await _service.Search("SimpleStateMachine", StateMachineBase.FinalStage);
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
                var stateMachines = await _service.Search(null, null);
                foreach (var stateMachine in stateMachines)
                {
                    if (stateMachine.Status == StateMachineStatus.Failed)
                        throw new InvalidOperationException(stateMachine.Exception);
                }
                if (stateMachines.All(x => x.Status == StateMachineStatus.Succeeded))
                {
                    stateMachines = await _service.Search(null, null);
                    Assert.Equal(stateMachineIds.Count, stateMachines.Count);
                    foreach (var stateMachine in stateMachines)
                    {
                        Assert.True(stateMachineIds.Contains(stateMachine.Id));
                        Assert.True(stateMachine.EndTime != null);
                        Assert.Equal(_configuration.Name, stateMachine.FromManagementService);
                        Assert.Equal(StateMachineBase.FinalStage, stateMachine.Stage);
                        var blob = stateMachine.StartedActors.Last().Outputs.FindBlob("stdout.txt");
                        var stdout = (await _store.ReadBlobs(new[] { blob })).Last().Item2;
                        Assert.Equal("simple state machine is ok\r\n", Encoding.UTF8.GetString(stdout));
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
            putDto.InitialBlobs[0].Id = await PutBlob("incorrect Main.c",
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

        [Fact(Skip = "TODO")]
        public void Patch()
        {
            // TODO
        }

        [Fact(Skip = "TODO")]
        public void Delete()
        {
            // TODO
        }
    }
}

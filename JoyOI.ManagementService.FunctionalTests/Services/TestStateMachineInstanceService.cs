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

        [Fact(Skip = "TODO")]
        public void Put()
        {
            // TODO
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

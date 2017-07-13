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
                    // _outputHelper.WriteLine(stateMachine.Status.ToString());
                    // _outputHelper.WriteLine(stateMachine.Stage.ToString());
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

        [Fact(Skip = "TODO")]
        public void Get()
        {
            // TODO
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

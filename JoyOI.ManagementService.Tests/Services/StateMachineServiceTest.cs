using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.Tests.Services
{
    public class StateMachineServiceTest : ServiceTestBase
    {
        private IStateMachineService _service;

        public StateMachineServiceTest()
        {

            _service = new StateMachineService(_context);
        }

        [Fact]
        public async Task GetAll()
        {
            var firstId = await _service.Put(
                new StateMachineInputDto() { Name = "first name", Body = "first body" });
            var secondId = await _service.Put(
                new StateMachineInputDto() { Name = "second name", Body = "second body" });
            var all = await _service.GetAll(null);
            Assert.Equal(2, all.Count);
            Assert.True(all.Any(x => x.Id == firstId && x.Name == "first name" && x.Body == "first body"));
            Assert.True(all.Any(x => x.Id == secondId && x.Name == "second name" && x.Body == "second body"));
        }

        [Fact]
        public async Task Get()
        {
            var firstId = await _service.Put(
                new StateMachineInputDto() { Name = "first name", Body = "first body" });
            var secondId = await _service.Put(
                new StateMachineInputDto() { Name = "second name", Body = "second body" });
            var first = await _service.Get("first name");
            var second = await _service.Get("second name");
            var third = await _service.Get("third name");
            Assert.True(first != null);
            Assert.Equal(firstId, first.Id);
            Assert.Equal("first name", first.Name);
            Assert.Equal("first body", first.Body);
            Assert.True(second != null);
            Assert.Equal(secondId, second.Id);
            Assert.Equal("second name", second.Name);
            Assert.Equal("second body", second.Body);
            Assert.True(third == null);
        }

        [Fact]
        public async Task Put()
        {
            var putId = await _service.Put(
                new StateMachineInputDto() { Name = "put name", Body = "put body" });
            var put = await _service.Get("put name");
            Assert.True(put != null);
            Assert.Equal(putId, put.Id);
            Assert.Equal("put name", put.Name);
            Assert.Equal("put body", put.Body);
        }

        [Fact]
        public async Task Patch()
        {
            var firstId = await _service.Put(
                new StateMachineInputDto() { Name = "first name", Body = "first body" });
            var secondId = await _service.Put(
                new StateMachineInputDto() { Name = "second name", Body = "second body" });
            var firstPatch = await _service.Patch("first name", new StateMachineInputDto() { Name = "first name updated" });
            var secondPatch = await _service.Patch("second name", new StateMachineInputDto() { Body = "second body updated" });
            var thirdPatch = await _service.Patch("third name", new StateMachineInputDto() { Name = "no exist" });
            Assert.Equal(1, firstPatch);
            Assert.Equal(1, secondPatch);
            Assert.Equal(0, thirdPatch);

            var first = await _service.Get("first name updated");
            var second = await _service.Get("second name");
            Assert.True(first != null);
            Assert.Equal(firstId, first.Id);
            Assert.Equal("first name updated", first.Name);
            Assert.Equal("first body", first.Body);
            Assert.True(second != null);
            Assert.Equal(secondId, second.Id);
            Assert.Equal("second name", second.Name);
            Assert.Equal("second body updated", second.Body);
        }

        [Fact]
        public async Task Delete()
        {
            var firstId = await _service.Put(
                new StateMachineInputDto() { Name = "first name", Body = "first body" });
            var secondId = await _service.Put(
                new StateMachineInputDto() { Name = "second name", Body = "second body" });
            var firstDelete = await _service.Delete("first name");
            var thirdDelete = await _service.Delete("third name");
            Assert.Equal(1, firstDelete);
            Assert.Equal(0, thirdDelete);

            var first = await _service.Get("first name");
            var second = await _service.Get("second name");
            Assert.True(first == null);
            Assert.True(second != null);
        }
    }
}

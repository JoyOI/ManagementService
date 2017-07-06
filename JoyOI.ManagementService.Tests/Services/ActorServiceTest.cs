using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.Services.Impl;
using JoyOI.ManagementService.Tests.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.Tests.Services
{
    public class ActorServiceTest : ServiceTestBase
    {
        private IActorService _service;

        public ActorServiceTest()
        {
            
            _service = new ActorService(_context);
        }

        [Fact]
        public async Task GetAll()
        {
            var firstId = await _service.Put(
                new ActorInputDto() { Name = "first name", Body = "first body" });
            var secondId = await _service.Put(
                new ActorInputDto() { Name = "second name", Body = "second body" });
            var all = await _service.GetAll(null);
            Assert.Equal(2, all.Count);
            Assert.True(all.Any(x => x.Id == firstId && x.Name == "first name" && x.Body == "first body"));
            Assert.True(all.Any(x => x.Id == secondId && x.Name == "second name" && x.Body == "second body"));
        }

        /* [Fact]
        public async Task GetByExpression()
        {
        }

        [Fact]
        public async Task GetById()
        {

        }

        [Fact]
        public async Task Put()
        {

        }

        [Fact]
        public async Task Patch()
        {

        }

        [Fact]
        public async Task Delete()
        {

        } */
    }
}

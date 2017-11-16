using JoyOI.ManagementService.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public class TestDockerNodeService : TestServiceBase
    {
        [Fact]
        public void GetNodes()
        {
            var store = new DockerNodeStore(_configuration,
                new TestDockerNodeStore.TestNotificationService());
            var service = new DockerNodeService(store);
            var nodes = service.GetNodes();
            Assert.Equal(2, nodes.Count());
            Assert.Contains(nodes, x => x.Name == "docker-1");
            Assert.Contains(nodes, x => x.Name == "docker-2");
        }

        [Fact]
        public void GetWaitingTasks()
        {
            Assert.True(false, "TODO");
        }
    }
}

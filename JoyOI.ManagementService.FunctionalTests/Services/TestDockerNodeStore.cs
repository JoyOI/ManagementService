using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.Services.Impl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public class TestDockerNodeStore : TestServiceBase
    {
        internal class TestNotificationService : INotificationService
        {
            public IList<(string, string)> Sent { get; } = new List<(string, string)>();

            public Task Send(string title, string message)
            {
                Sent.Add((title, message));
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task StartKeepaliveLoop()
        {
            var configure = JsonConvert.DeserializeObject<JoyOIManagementConfiguration>(
                JsonConvert.SerializeObject(_configuration));
            configure.Nodes["docker-1"].Address = "http://not-exist:2376";
            var notificationService = new TestNotificationService();
            var store = new DockerNodeStore(configure, notificationService);
#pragma warning disable CS4014
            store.StartKeepaliveLoop();
#pragma warning restore CS4014
            while (notificationService.Sent.Count == 0)
            {
                await Task.Delay(1);
            }
            Assert.Equal(1, notificationService.Sent.Count);
            Assert.Equal("Docker Node Failure: docker-1", notificationService.Sent[0].Item1);
        }

        [Fact]
        public void GetNode()
        {
            var store = new DockerNodeStore(_configuration, new NotificationService());
            var node = store.GetNode("docker-1");
            Assert.True(node != null);
            node = store.GetNode("docker-2");
            Assert.True(node != null);
        }

        [Fact]
        public void GetNodes()
        {
            var store = new DockerNodeStore(_configuration, new NotificationService());
            var nodes = store.GetNodes();
            Assert.Equal(2, nodes.Count());
            Assert.Contains(nodes, x => x.Name == "docker-1");
            Assert.Contains(nodes, x => x.Name == "docker-2");
        }

        [Fact]
        public void GetWaitingTasks()
        {
            var store = new DockerNodeStore(_configuration, new NotificationService());
            var result = store.GetWaitingTasks();
            Assert.NotNull(result);
        }

        [Fact]
        public void AcquireAndReleaseNode()
        {
            var store = new DockerNodeStore(_configuration, new NotificationService());
            var tasks = new List<Task<DockerNode>>();
            // 获取节点直到达到上限值
            for (var x = 0; x < _configuration.Container.MaxRunningJobs * _configuration.Nodes.Count; ++x)
            {
                tasks.Add(store.AcquireNode(0));
            }
            Task.WaitAll(tasks.ToArray());
            // 之后再获取需要等待
            var waitTask = store.AcquireNode(0);
            Assert.True(!waitTask.Wait(TimeSpan.FromMilliseconds(100)));
            // 判断节点的运行任务数量
            foreach (var node in store.GetNodes())
            {
                Assert.Equal(_configuration.Container.MaxRunningJobs, node.RunningJobs);
            }
            // 释放获取到的节点
            foreach (var task in tasks)
            {
                store.ReleaseNode(task.Result);
            }
            // 处理waitTask
            Assert.True(waitTask.Wait(TimeSpan.FromMilliseconds(100)));
            store.ReleaseNode(waitTask.Result);
            // 判断节点的运行任务数量
            foreach (var node in store.GetNodes())
            {
                Assert.Equal(0, node.RunningJobs);
            }
        }
    }
}

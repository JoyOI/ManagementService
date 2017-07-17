using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.Services.Impl;
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
        [Fact]
        public void GetNode()
        {
            var store = new DockerNodeStore(_configuration);
            var node = store.GetNode("docker-1");
            Assert.True(node != null);
            node = store.GetNode("docker-2");
            Assert.True(node != null);
        }

        [Fact]
        public void GetNodes()
        {
            var store = new DockerNodeStore(_configuration);
            var nodes = store.GetNodes();
            Assert.Equal(2, nodes.Count());
            Assert.True(nodes.Any(x => x.Name == "docker-1"));
            Assert.True(nodes.Any(x => x.Name == "docker-2"));
        }

        [Fact]
        public void AcquireAndReleaseNode()
        {
            var store = new DockerNodeStore(_configuration);
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

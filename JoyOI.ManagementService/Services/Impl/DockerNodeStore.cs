﻿using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理Docker节点的仓库, 应该为单例
    /// </summary>
    internal class DockerNodeStore : IDockerNodeStore
    {
        private JoyOIManagementConfiguration _configuration;
        private List<DockerNode> _nodes;
        private Dictionary<string, DockerNode> _nodesMap;
        private SortedDictionary<int, Queue<TaskCompletionSource<DockerNode>>> _waitReleaseQueue;
        private object _nodesLock;

        public DockerNodeStore(JoyOIManagementConfiguration configuration)
        {
            _configuration = configuration;
            _nodes = new List<DockerNode>();
            _nodesMap = new Dictionary<string, DockerNode>();
            _waitReleaseQueue = new SortedDictionary<int, Queue<TaskCompletionSource<DockerNode>>>();
            _nodesLock = new object();
            foreach (var nodeInfo in _configuration.Nodes)
            {
                var node = new DockerNode(nodeInfo.Key, nodeInfo.Value);
                _nodes.Add(node);
                _nodesMap.Add(node.Name, node);
            }
        }

        public DockerNode GetNode(string nodeName)
        {
            DockerNode node;
            _nodesMap.TryGetValue(nodeName, out node);
            return node;
        }

        public IEnumerable<DockerNode> GetNodes()
        {
            return _nodesMap.Select(x => x.Value);
        }

        public async Task<DockerNode> AcquireNode(int priority)
        {
            Task<DockerNode> waitRelease;
            lock (_nodesLock)
            {
                // 使用任务最少的节点
                foreach (var node in _nodes)
                {
                    if (node.RunningJobs < node.NodeInfo.Container.MaxRunningJobs)
                    {
                        ++node.RunningJobs;
                        _nodes.Sort(new DockerNodeComparer());
                        return node;
                    }
                }
                // 需要等待其他节点完成, 添加到等待队列
                // https://stackoverflow.com/questions/27891253/how-to-create-a-task-i-can-complete-manually
                var source = new TaskCompletionSource<DockerNode>();
                waitRelease = source.Task;
                if (!_waitReleaseQueue.TryGetValue(priority, out var childQueue))
                    _waitReleaseQueue[priority] = childQueue = new Queue<TaskCompletionSource<DockerNode>>();
                childQueue.Enqueue(source);
            }
            var released = await waitRelease;
            return released;
        }

        public void ReleaseNode(DockerNode node)
        {
            TaskCompletionSource<DockerNode> source = null;
            lock (_nodesLock)
            {
                // 判断等待队列是否为空
                foreach (var childQueue in _waitReleaseQueue)
                {
                    // 不为空时需要把节点分配给正在等待的任务
                    // 正在运行的任务数量不变
                    if (childQueue.Value.Count > 0)
                    {
                        source = childQueue.Value.Dequeue();
                        break;
                    }
                }
                if (source == null)
                {
                    // 减少节点的正在运行任务数量
                    --node.RunningJobs;
                    _nodes.Sort(new DockerNodeComparer());
                }
            }
            if (source != null)
            {
                source.SetResult(node);
            }
        }
    }
}

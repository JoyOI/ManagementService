using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
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
        private INotificationService _notificationService;
        private List<DockerNode> _nodes;
        private Dictionary<string, DockerNode> _nodesMap;
        private SortedDictionary<int, Queue<TaskCompletionSource<DockerNode>>> _waitReleaseQueue;
        private object _nodesLock;
        private TimeSpan _keepaliveReportInterval;
        private TimeSpan _keepaliveScanInterval;
        private int _keepaliveMaxErrorCount;

        public DockerNodeStore(
            JoyOIManagementConfiguration configuration,
            INotificationService notificationService)
        {
            _configuration = configuration;
            _notificationService = notificationService;
            _nodes = new List<DockerNode>();
            _nodesMap = new Dictionary<string, DockerNode>();
            _waitReleaseQueue = new SortedDictionary<int, Queue<TaskCompletionSource<DockerNode>>>();
            _nodesLock = new object();
            _keepaliveReportInterval = TimeSpan.FromHours(1);
            _keepaliveScanInterval = TimeSpan.FromSeconds(5);
            _keepaliveMaxErrorCount = 3;
            foreach (var nodeInfo in _configuration.Nodes)
            {
                var node = new DockerNode(nodeInfo.Key, nodeInfo.Value);
                _nodes.Add(node);
                _nodesMap.Add(node.Name, node);
            }
        }

        public async Task StartKeepaliveLoop()
        {
            // 循环确认Docker节点是否存活
            var errorCountMap = new Dictionary<DockerNode, int>();
            var lastReportedMap = new Dictionary<DockerNode, DateTime>();
            while (true)
            {
                foreach (var pair in _nodesMap)
                {
                    try
                    {
                        // 检查连接是否正常
                        var client = pair.Value.Client;
                        await client.System.GetVersionAsync();
                        // 重置错误标记
                        pair.Value.ErrorFlags = false;
                        // 重置出错次数
                        errorCountMap[pair.Value] = 0;
                    }
                    catch (Exception ex)
                    {
                        // 设置错误标记
                        pair.Value.ErrorFlags = true;
                        // 判断出错次数是否超过最大次数
                        errorCountMap.TryGetValue(pair.Value, out var errorCount);
                        ++errorCount;
                        if (errorCount >= _keepaliveMaxErrorCount)
                        {
                            // 判断是否需要报告
                            lastReportedMap.TryGetValue(pair.Value, out var lastReported);
                            var now = DateTime.UtcNow;
                            if (now - lastReported > _keepaliveReportInterval)
                            {
                                await _notificationService.Send(
                                    $"Docker Node Failure: {pair.Value.Name}", ex.ToString());
                                lastReportedMap[pair.Value] = now;
                                errorCount = 0;
                            }
                        }
                        errorCountMap[pair.Value] = errorCount;
                    }
                }
                await Task.Delay(_keepaliveScanInterval);
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
                // 第一次使用任务数最少且上次未出错的节点 
                // 第二次使用任务数最少的节点
                for (var x = 0; x < 2; ++x)
                {
                    foreach (var node in _nodes)
                    {
                        if (x == 0 && node.ErrorFlags)
                            continue;
                        if (node.RunningJobs < node.NodeInfo.Container.MaxRunningJobs)
                        {
                            ++node.RunningJobs;
                            _nodes.Sort(new DockerNodeComparer());
                            return node;
                        }
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

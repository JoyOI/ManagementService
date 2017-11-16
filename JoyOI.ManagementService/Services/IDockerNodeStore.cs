using JoyOI.ManagementService.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理Docker节点的仓库, 应该为单例
    /// </summary>
    internal interface IDockerNodeStore
    {
        /// <summary>
        /// 循环确认Docker节点是否存活
        /// </summary>
        Task StartKeepaliveLoop();

        /// <summary>
        /// 获取指定名称的节点, 不存在时返回null
        /// </summary>
        DockerNode GetNode(string nodeName);

        /// <summary>
        /// 获取所有节点
        /// </summary>
        IEnumerable<DockerNode> GetNodes();

        /// <summary>
        /// 获取等待中的任务数量, 返回{ 优先度: 任务数量 }
        /// </summary>
        IDictionary<int, int> GetWaitingTasks();

        /// <summary>
        /// 获取可以用于执行任务的Docker节点
        /// </summary>
        Task<DockerNode> AcquireNode(int priority);

        /// <summary>
        /// 释放可以用于执行任务的Docker节点
        /// </summary>
        void ReleaseNode(DockerNode node);
    }
}

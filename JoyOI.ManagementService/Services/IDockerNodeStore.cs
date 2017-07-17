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
        /// 获取指定名称的节点, 不存在时返回null
        /// </summary>
        DockerNode GetNode(string nodeName);

        /// <summary>
        /// 获取所有节点
        /// </summary>
        IEnumerable<DockerNode> GetNodes();

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

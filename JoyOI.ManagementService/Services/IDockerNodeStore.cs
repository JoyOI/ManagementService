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
        /// 获取可以用于执行任务的Docker节点
        /// </summary>
        Task<DockerNode> AcquireNode();

        /// <summary>
        /// 释放可以用于执行任务的Docker节点
        /// </summary>
        void ReleaseNode(DockerNode node);
    }
}

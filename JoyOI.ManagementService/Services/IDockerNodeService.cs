using JoyOI.ManagementService.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 获取Docker节点的服务, 对外提供
    /// </summary>
    public interface IDockerNodeService
    {
        /// <summary>
        /// 获取所有节点
        /// </summary>
        IEnumerable<DockerNodeOutputDto> GetNodes();

        /// <summary>
        /// 获取等待中的任务数量, 返回{ 优先度: 任务数量 }
        /// </summary>
        IDictionary<int, int> GetWaitingTasks();

        /// <summary>
        /// 获取超时错误的历史记录
        /// </summary>
        IList<string> GetTimeoutErrors();
    }
}

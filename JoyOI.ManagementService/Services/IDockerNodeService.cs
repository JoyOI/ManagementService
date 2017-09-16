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
    }
}

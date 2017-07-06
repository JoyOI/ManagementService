using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理Docker节点的仓库, 应该为单例
    /// </summary>
    public class DockerNodeStore : IDockerNodeStore
    {
        private JoyOIManagementConfiguration _configuration;

        public DockerNodeStore(JoyOIManagementConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}

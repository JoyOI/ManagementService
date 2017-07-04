using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JoyOIManagementServiceCollectionExtensions
    {
        public static void AddJoyOIManagement(
            this IServiceCollection services, JoyOIManagementConfiguration configuration)
        {
            // 检查配置
            if (string.IsNullOrEmpty(configuration.DockerImage))
            {
                throw new ArgumentNullException("Please provide DockerImage");
            }
            if ((configuration.Nodes?.Count ?? 0) <= 0)
            {
                throw new ArgumentNullException("Please provide atleast 1 docker nodes");
            }
            // 注册服务
            // TODO
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Configuration
{
    /// <summary>
    /// 管理服务的配置
    /// 一般写在appsettings.json中
    /// </summary>
    public class JoyOIManagementConfiguration
    {
        /// <summary>
        /// 管理服务的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// { 节点名称: 节点配置 }
        /// </summary>
        public IDictionary<string, Node> Nodes { get; set; }

        public JoyOIManagementConfiguration()
        {
            Name = "Default";
            Nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        /// 节点配置
        /// </summary>
        public class Node
        {
            /// <summary>
            /// Docker镜像名称
            /// </summary>
            public string Image { get; set; }
            /// <summary>
            /// 节点地址
            /// 例如: http://docker-1:2376
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// 客户端证书路径
            /// 例如: ClientCerts/docker-1.pfx
            /// </summary>
            public string ClientCertificatePath { get; set; }
            /// <summary>
            /// 客户端证书密码
            /// 例如: 123456
            /// </summary>
            public string ClientCertificatePassword { get; set; }
        }
    }
}

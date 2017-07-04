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
        /// Docker镜像名称
        /// 默认: joyoi
        /// </summary>
        public string DockerImage { get; set; }
        /// <summary>
        /// { 节点名称: 节点配置 }
        /// </summary>
        public IDictionary<string, Node> Nodes { get; set; }

        public JoyOIManagementConfiguration()
        {
            DockerImage = "joyoi";
            Nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        /// 节点配置
        /// </summary>
        public class Node
        {
            /// <summary>
            /// 节点地址
            /// 例如: http://docker-1:2376
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// 证书路径
            /// 例如: ClientCerts/docker-1
            /// 要求文件夹下有以下的文件
            /// ClientCerts/docker-1/ca.pem
            /// ClientCerts/docker-1/cert.pem
            /// ClientCerts/docker-1/key.pem
            /// </summary>
            public string ClientCertificateDirectory { get; set; }
        }
    }
}

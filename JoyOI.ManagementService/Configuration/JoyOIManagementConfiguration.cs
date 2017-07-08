using Microsoft.EntityFrameworkCore.Migrations;
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
        /// 管理服务的名称, 如果要配置多个管理服务必须使用不同的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 容器相关的配置
        /// </summary>
        public ContainerConfiguration Container { get; set; }
        /// <summary>
        /// 运行任务时对容器的限制
        /// </summary>
        public ContainerLimitation Limitation { get; set; }
        /// <summary>
        /// { 节点名称: 节点配置 }
        /// </summary>
        public IDictionary<string, Node> Nodes { get; set; }

        public JoyOIManagementConfiguration()
        {
            Name = "Default";
            Container = new ContainerConfiguration();
            Limitation = new ContainerLimitation();
            Nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        /// 节点配置
        /// </summary>
        public class Node
        {
            /// <summary>
            /// docker镜像的名称
            /// </summary>
            public string Image { get; set; }
            /// <summary>
            /// 节点的地址
            /// 例如: http://docker-1:2376
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// 客户端证书的路径
            /// 例如: ClientCerts/docker-1.pfx
            /// </summary>
            public string ClientCertificatePath { get; set; }
            /// <summary>
            /// 客户端证书的密码
            /// 例如: 123456
            /// </summary>
            public string ClientCertificatePassword { get; set; }
        }

        /// <summary>
        /// 容器相关的配置
        /// </summary>
        public class ContainerConfiguration
        {
            /// <summary>
            /// 单个节点可以同时运行的任务数量
            /// </summary>
            public int MaxRunningJobs { get; set; }
            /// <summary>
            /// 容器中的工作目录路径, 需要以"/"结尾
            /// </summary>
            public string WorkDir { get; set; }
            /// <summary>
            /// 任务代码的路径, 相对于工作目录
            /// 例如: actor/Program.cs
            /// </summary>
            public string ActorCodePath { get; set; }
            /// <summary>
            /// 执行任务的命令
            /// 例如: sh run-actor.sh
            /// </summary>
            public string ActorExecuteCommand { get; set; }

            public ContainerConfiguration()
            {
                MaxRunningJobs = 32;
                WorkDir = "/workdir/";
                ActorCodePath = "actor/Program.cs";
                ActorExecuteCommand = "sh run-actor.sh";
            }
        }
    }
}

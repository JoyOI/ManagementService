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
        /// 是否测试模式
        /// </summary>
        public bool TestMode { get; set; }
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
            TestMode = false;
            Container = new ContainerConfiguration()
            {
                DevicePath = "/dev/sda",
                MaxRunningJobs = 32,
                WorkDir = "/workdir/",
                ActorExecutablePath = "actor/bin/Debug/netcoreapp2.0/actor.dll",
                ActorExecuteCommand = "sh run-actor.sh &> run-actor.log",
                ActorExecuteLogPath = "run-actor.log",
                ResultPath = "return.json"
            };
            Limitation = new ContainerLimitation();
            Nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        /// 加载后的处理
        /// </summary>
        public void AfterLoaded()
        {
            // 检查配置
            if ((Nodes?.Count ?? 0) <= 0)
            {
                throw new ArgumentNullException("Please provide atleast 1 docker nodes");
            }
            // 整合各个node的容器配置
            foreach (var node in Nodes)
            {
                node.Value.Container = (node.Value.Container ?? new ContainerConfiguration())
                    .WithDefaults(Container);
            }
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
            /// <summary>
            /// 阶段单独的容器配置, 可以等于null也可以只设置部分属性, 不设置的属性会使用上面的值
            /// </summary>
            public ContainerConfiguration Container { get; set; }
        }

        /// <summary>
        /// 容器相关的配置
        /// </summary>
        public class ContainerConfiguration
        {
            /// <summary>
            /// 主设备路径
            /// 例如: /dev/sda
            /// </summary>
            public string DevicePath { get; set; }
            /// <summary>
            /// 单个节点可以同时运行的任务数量
            /// 例如: 32
            /// </summary>
            public int MaxRunningJobs { get; set; }
            /// <summary>
            /// 容器中的工作目录路径, 需要以"/"结尾
            /// 例如 /workdir/
            /// </summary>
            public string WorkDir { get; set; }
            /// <summary>
            /// 任务可执行文件的路径, 相对于工作目录
            /// 例如: actor/bin/Debug/netcoreapp2.0/actor.dll
            /// </summary>
            public string ActorExecutablePath { get; set; }
            /// <summary>
            /// 执行任务的命令
            /// 例如: sh run-actor.sh &> run-actor.log
            /// </summary>
            public string ActorExecuteCommand { get; set; }
            /// <summary>
            /// 执行任务的记录文件
            /// 例如: run-actor.log
            /// </summary>
            public string ActorExecuteLogPath { get; set; }
            /// <summary>
            /// 执行任务的结果文件
            /// 例如: return.json
            /// </summary>
            public string ResultPath { get; set; }

            public ContainerConfiguration()
            {
            }

            public ContainerConfiguration WithDefaults(ContainerConfiguration configuration)
            {
                DevicePath = DevicePath ?? configuration.DevicePath;
                MaxRunningJobs = MaxRunningJobs > 0 ? MaxRunningJobs : configuration.MaxRunningJobs;
                WorkDir = WorkDir ?? configuration.WorkDir;
                ActorExecutablePath = ActorExecutablePath ?? configuration.ActorExecutablePath;
                ActorExecuteCommand = ActorExecuteCommand ?? configuration.ActorExecuteCommand;
                ActorExecuteLogPath = ActorExecuteLogPath ?? configuration.ActorExecuteLogPath;
                ResultPath = ResultPath ?? configuration.ResultPath;
                return this;
            }
        }
    }
}

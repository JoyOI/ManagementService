using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    /// <summary>
    /// Docker节点的输出信息
    /// </summary>
    public class DockerNodeOutputDto : IOutputDto
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get;  set; }
        /// <summary>
        /// 节点信息
        /// </summary>
        public NodeOutputDto NodeInfo { get; set; }
        /// <summary>
        /// 正在运行的任务数量
        /// </summary>
        public int RunningJobs { get; set; }
        /// <summary>
        /// 正在运行的任务描述
        /// </summary>
        public List<string> RunningJobDescriptions { get; set; }
        /// <summary>
        /// 上次使用此节点执行任务是否出错
        /// </summary>
        public bool ErrorFlags { get; set; }

        /// <summary>
        /// 节点的配置信息
        /// </summary>
        public class NodeOutputDto
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
        }
    }
}

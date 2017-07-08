using JoyOI.ManagementService.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 任务的执行信息
    /// </summary>
    public class ActorInfo
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 输入文件
        /// </summary>
        public IEnumerable<BlobInfo> Inputs { get; set; }
        /// <summary>
        /// 输出文件
        /// </summary>
        public IEnumerable<BlobInfo> Outputs { get; set; }
        /// <summary>
        /// 发生的错误列表
        /// </summary>
        public string[] Exceptions { get; set; }
        /// <summary>
        /// 任务的当前状态
        /// </summary>
        public ActorStatus Status { get; set; }
        /// <summary>
        /// 任务的所属阶段
        /// </summary>
        public string Stage { get; set; }
        /// <summary>
        /// 附加信息, 可以是空值
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 执行任务的节点名称
        /// </summary>
        public string RunningNode { get; set; }
        /// <summary>
        /// 执行任务的容器ID
        /// </summary>
        public string RunningContainer { get; set; }
    }
}

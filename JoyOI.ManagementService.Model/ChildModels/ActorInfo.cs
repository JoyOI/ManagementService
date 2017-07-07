using JoyOI.ManagementService.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 任务的执行信息
    /// </summary>
    public struct ActorInfo
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid Id { get; set; }

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
        /// 任务所属的状态
        /// </summary>
        public string State { get; set; }
    }
}

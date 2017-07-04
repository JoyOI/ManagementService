using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 任务的历史记录
    /// 所有添加或修改的任务都会保存到这个表
    /// </summary>
    public class ActorHistoryEntity : IEntity<Guid>
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 可编译并独立执行的C# Console App代码
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 修订号
        /// </summary>
        public long Revision { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}

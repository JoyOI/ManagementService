using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 任务
    /// </summary>
    public class ActorEntity :
        IEntity<Guid>,
        IEntityWithCreateTime,
        IEntityWithUpdateTime
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
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}

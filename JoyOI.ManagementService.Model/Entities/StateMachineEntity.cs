using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 状态机
    /// </summary>
    public class StateMachineEntity :
        IEntity<Guid>,
        IEntityWithCreateTime,
        IEntityWithUpdateTime
    {
        /// <summary>
        /// 状态机Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 状态机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 继承了StateMachineBase类的代码
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

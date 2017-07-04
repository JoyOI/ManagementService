using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 状态机的历史记录
    /// 所有添加或修改的状态机都会保存到这个表
    /// </summary>
    public class StateMachineHistoryEntity : IEntity<Guid>
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
        /// 修订号
        /// </summary>
        public long Revision { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}

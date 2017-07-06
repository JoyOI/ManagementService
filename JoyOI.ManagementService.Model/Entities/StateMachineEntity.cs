using JoyOI.ManagementService.Model.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
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
        /// 状态机使用的限制设置, 不设置时使用默认值
        /// </summary>
        public string _Limitation { get; set; }
        public ContainerLimitation Limitation
        {
            get => string.IsNullOrEmpty(_Limitation) ?
                new ContainerLimitation() :
                JsonConvert.DeserializeObject<ContainerLimitation>(_Limitation);
            set => _Limitation = JsonConvert.SerializeObject(value);
        }
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

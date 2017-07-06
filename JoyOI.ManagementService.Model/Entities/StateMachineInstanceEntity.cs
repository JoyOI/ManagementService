using JoyOI.ManagementService.Model.Entities.Interfaces;
using JoyOI.ManagementService.Model.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 状态机实例
    /// </summary>
    public class StateMachineInstanceEntity : IEntity<Guid>
    {
        /// <summary>
        /// 最多重新运行的次数
        /// </summary>
        public const int MaxReRunTimes = 3;

        /// <summary>
        /// 状态机实例Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 状态机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 状态机的当前状态
        /// </summary>
        public StateMachineStatus Status { get; set; }
        /// <summary>
        /// 已执行的任务列表
        /// </summary>
        public string _FinishedActors { get; set; }
        public ActorInfo[] FinishedActors
        {
            get => JsonConvert.DeserializeObject<ActorInfo[]>(_FinishedActors);
            set => _FinishedActors = JsonConvert.SerializeObject(value);
        }
        /// <summary>
        /// 当前执行的任务
        /// </summary>
        public string _CurrentActor { get; set; }
        public ActorInfo CurrentActor
        {
            get => JsonConvert.DeserializeObject<ActorInfo>(_CurrentActor);
            set => _FinishedActors = JsonConvert.SerializeObject(value);
        }
        /// <summary>
        /// 当前执行的Docker节点名称 (不是地址而是名称)
        /// </summary>
        public string CurrentNode { get; set; }
        /// <summary>
        /// 当前执行的Docker容器Id (例如b9a51f0805de)
        /// </summary>
        public string CurrentContainer { get; set; }
        /// <summary>
        /// 创建此实例的管理服务,各个管理服务只对自己创建的实例负责
        /// </summary>
        public string FromManagementService { get; set; }
        /// <summary>
        /// 重新运行的次数
        /// 超过MaxReRunTimes则会标记为Failed
        /// </summary>
        public int ReRunTimes { get; set; }
        /// <summary>
        /// 第一个任务的开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 最后一个任务的结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}

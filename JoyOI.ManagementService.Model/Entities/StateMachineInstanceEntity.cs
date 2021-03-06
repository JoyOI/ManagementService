﻿using JoyOI.ManagementService.Model.Entities.Interfaces;
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
        /// 状态机的当前阶段
        /// </summary>
        public string Stage { get; set; }
        /// <summary>
        /// 已经开始的任务列表
        /// </summary>
        public string _StartedActors { get; set; }
        public IList<ActorInfo> StartedActors
        {
            get => string.IsNullOrEmpty(_StartedActors) ?
                new List<ActorInfo>() :
                JsonConvert.DeserializeObject<IList<ActorInfo>>(_StartedActors);
            set => _StartedActors = JsonConvert.SerializeObject(value ?? new List<ActorInfo>());
        }
        /// <summary>
        /// 初始的文件列表
        /// 创建状态机实例后不应该修改这个字段
        /// 需要获取前一个阶段创建的文件请使用StartedActors中的Outputs
        /// </summary>
        public string _InitialBlobs { get; set; }
        public IList<BlobInfo> InitialBlobs
        {
            get => string.IsNullOrEmpty(_InitialBlobs) ?
                new List<BlobInfo>() :
                JsonConvert.DeserializeObject<IList<BlobInfo>>(_InitialBlobs);
            set => _InitialBlobs = JsonConvert.SerializeObject(value ?? new List<BlobInfo>());
        }
        /// <summary>
        /// 使用的限制参数
        /// 用于限制容器的运行环境
        /// 优先度:
        /// StateMachineInstanceEntity > StateMachineEntity > JoyOIManagementConfiguration
        /// </summary>
        public string _Limitation { get; set; }
        public ContainerLimitation Limitation
        {
            get => string.IsNullOrEmpty(_Limitation) ?
                null :
                JsonConvert.DeserializeObject<ContainerLimitation>(_Limitation);
            set => _Limitation = JsonConvert.SerializeObject(value ?? new ContainerLimitation());
        }
        /// <summary>
        /// 自定义参数
        /// Host等等可以通过这里传入
        /// </summary>
        public string _Parameters { get; set; }
        public IDictionary<string, string> Parameters
        {
            get => string.IsNullOrEmpty(_Parameters) ?
                new Dictionary<string, string>() :
                JsonConvert.DeserializeObject<IDictionary<string, string>>(_Parameters);
            set => _Parameters = JsonConvert.SerializeObject(value ?? new Dictionary<string, string>());
        }
        /// <summary>
        /// 运行优先级
        /// 默认为0, 越低的值越优先
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 创建此实例的管理服务,各个管理服务只对自己创建的实例负责
        /// </summary>
        public string FromManagementService { get; set; }
        /// <summary>
        /// 重新运行的次数
        /// 超过MaxReRunTimes则会标记Status为Failed
        /// </summary>
        public int ReRunTimes { get; set; }
        /// <summary>
        /// 执行过程中发生的错误
        /// 可能是状态机本身的错误, 也可能是任务执行过程中发生的错误
        /// </summary>
        public string Exception { get; set; }
        /// <summary>
        /// 执行时使用的并发键
        /// 用于防止多个管理服务同时修改一个状态机实例
        /// </summary>
        public string ExecutionKey { get; set; }
        /// <summary>
        /// 第一个任务的开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 最后一个任务的结束时间
        /// 未完成或失败时等于null
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}

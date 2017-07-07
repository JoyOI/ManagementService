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
        public string _Actors { get; set; }
        
        public IList<ActorInfo> Actors
        {
            get => string.IsNullOrEmpty(_Actors) ?
                new List<ActorInfo>() :
                JsonConvert.DeserializeObject<IList<ActorInfo>>(_Actors);
            set => _Actors = JsonConvert.SerializeObject(value);
        }

        /// 使用的限制参数
        /// 优先度:
        /// StateMachineInstanceEntity > StateMachineEntity > JoyOIManagementConfiguration
        /// </summary>
        public string _Limitation { get; set; }

        public ContainerLimitation Limitation
        {
            get => null;
            //get => string.IsNullOrEmpty(_CurrentActor) ?
            //    null :
            //    JsonConvert.DeserializeObject<ContainerLimitation>(_Limitation);
            set => _Limitation = JsonConvert.SerializeObject(value);
        }

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
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 当前状态机状态
        /// </summary>
        public string State { get; set; }

        public string _Blobs { get; set; }

        public IEnumerable<BlobInfo> Blobs
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<BlobInfo>>(_Blobs ?? "[]");
            }
        }
    }
}

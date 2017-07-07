using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Services;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 状态机实例的基础类
    /// </summary>
    public abstract class StateMachineBase : IDisposable
    {
        public Guid Id { get; set; }
        public StateMachineStatus Status { get; set; }
        public IList<ActorInfo> FinishedActors { get; set; }
        public ActorInfo CurrentActor { get; set; }
        internal IStateMachineInstanceStore Store { get; set; }
        internal ContainerLimitation Limitation { get; set; }

        public virtual void Dispose()
        {
        }

        protected async Task DeployAndRunActorAsync(string actor, BlobInfo[] inputs)
        {
            // 更新FinishedActors
            if (CurrentActor.Name != null)
            {
                FinishedActors.Add(CurrentActor);
            }
            // 创建新的CurrentActor
            CurrentActor = new ActorInfo()
            {
                Name = actor,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                Inputs = inputs,
                Outputs = new BlobInfo[0],
                Exceptions = new string[0],
                Status = ActorStatus.Running
            };
            // 传到docker上执行
            await Store.RunActor(this);
        }

        public abstract Task RunAsync(string actor, BlobInfo[] blobs);
    }
}

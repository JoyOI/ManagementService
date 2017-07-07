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

        protected Task DeployAndRunActorAsync(string actor, BlobInfo[] inputs)
        {
            throw new NotImplementedException();
        }

        public abstract Task RunAsync(string actor, BlobInfo[] blobs);
    }
}

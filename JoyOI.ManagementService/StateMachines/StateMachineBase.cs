using JoyOI.ManagementService.Model.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.StateMachines
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

        public void Dispose()
        {
        }

        protected async Task DeployAndRunActorAsync(string actor, BlobInfo[] inputs)
        {
            throw new NotImplementedException();
        }

        public abstract Task RunAsync(string actorName, BlobInfo[] blobs);
    }
}

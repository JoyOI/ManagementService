using JoyOI.ManagementService.Model.Dtos.Interfaces;
using JoyOI.ManagementService.Model.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstanceOutputDto : IOutputDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public StateMachineStatus Status { get; set; }
        public string Stage { get; set; }
        public IList<ActorInfo> StartedActors { get; set; }
        public IList<BlobInfo> InitialBlobs { get; set; }
        public ContainerLimitation Limitation { get; set; }
        public string FromManagementService { get; set; }
        public int ReRunTimes { get; set; }
        public string Exception { get; set; }
        public string ExecutionKey { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}

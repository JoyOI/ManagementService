using JoyOI.ManagementService.Model.Dtos.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstancePutDto : IInputDto
    {
        public string Name { get; set; }
        public IList<BlobInfo> InitialBlobs { get; set; }
        public ContainerLimitation Limitation { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public int Priority { get; set; }
    }
}

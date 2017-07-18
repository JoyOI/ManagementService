using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstancePatchDto : IInputDto
    {
        public string Stage { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
    }
}

using JoyOI.ManagementService.Model.Dtos.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineOutputDto : IOutputDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public ContainerLimitation Limitation { get; set; }
    }
}

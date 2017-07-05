using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class ActorOutputDto : IOutputDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
    }
}

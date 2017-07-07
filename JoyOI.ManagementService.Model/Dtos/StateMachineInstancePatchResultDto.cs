using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstancePatchResultDto : IOutputDto
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public StateMachineInstancePatchResultDto()
        {
        }

        public StateMachineInstancePatchResultDto(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public static StateMachineInstancePatchResultDto Success()
        {
            return new StateMachineInstancePatchResultDto(200, null);
        }

        public static StateMachineInstancePatchResultDto NotFound(string msg)
        {
            return new StateMachineInstancePatchResultDto(404, msg);
        }

        public static StateMachineInstancePatchResultDto Failed(string msg)
        {
            return new StateMachineInstancePatchResultDto(500, msg);
        }
    }
}

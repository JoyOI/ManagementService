using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstancePutResultDto : IOutputDto
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public StateMachineInstanceOutputDto Instance { get; set; }

        public StateMachineInstancePutResultDto()
        {
        }

        public StateMachineInstancePutResultDto(int code, string message, StateMachineInstanceOutputDto instance)
        {
            Code = code;
            Message = message;
            Instance = instance;
        }

        public static StateMachineInstancePutResultDto Success(StateMachineInstanceOutputDto instance)
        {
            return new StateMachineInstancePutResultDto(200, null, instance);
        }

        public static StateMachineInstancePutResultDto NotFound(string msg)
        {
            return new StateMachineInstancePutResultDto(404, msg, null);
        }

        public static StateMachineInstancePutResultDto Failed(string msg)
        {
            return new StateMachineInstancePutResultDto(500, msg, null);
        }
    }
}

using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class StateMachineInstancePutResultDto : IOutputDto
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public StateMachineInstanceOutputDto Instance { get; set; }

        public static StateMachineInstancePutResultDto Success(StateMachineInstanceOutputDto instance)
        {
            return new StateMachineInstancePutResultDto()
            {
                Code = 200,
                Message = null,
                Instance = instance
            };
        }

        public static StateMachineInstancePutResultDto NotFound(string msg)
        {
            return new StateMachineInstancePutResultDto()
            {
                Code = 404,
                Message = msg,
                Instance = null
            };
        }

        public static StateMachineInstancePutResultDto Failed(string msg)
        {
            return new StateMachineInstancePutResultDto()
            {
                Code = 500,
                Message = msg,
                Instance = null
            };
        }
    }
}

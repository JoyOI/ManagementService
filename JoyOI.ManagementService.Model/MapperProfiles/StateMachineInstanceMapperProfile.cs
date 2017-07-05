using AutoMapper;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.MapperProfiles
{
    public class StateMachineInstanceMapperProfile : Profile
    {
        public StateMachineInstanceMapperProfile()
        {
            // 状态机实例需要特殊处理, TODO
        }
    }
}

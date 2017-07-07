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
            // 只转换输出的, 输入的需要特殊处理
            CreateMap<StateMachineInstanceEntity, StateMachineInstanceOutputDto>();
        }
    }
}

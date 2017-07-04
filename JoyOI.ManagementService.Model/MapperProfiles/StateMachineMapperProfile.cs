using AutoMapper;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.MapperProfiles
{
    public class StateMachineMapperProfile : Profile
    {
        public StateMachineMapperProfile()
        {
            CreateMap<StateMachineInputDto, StateMachineEntity>();
            CreateMap<StateMachineEntity, StateMachineOutputDto>();
        }
    }
}

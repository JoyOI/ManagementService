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
            CreateMap<StateMachineInputDto, StateMachineEntity>()
                .ForMember(model => model.Name, model => model.Ignore())
                .ForMember(model => model.Body, model => model.Ignore())
                .ForMember(model => model.Limitation, model => model.Ignore())
                .AfterMap((src, dst) =>
                {
                    if (src.Name != null)
                        dst.Name = src.Name;
                    if (src.Body != null)
                        dst.Body = src.Body;
                    if (src.Limitation != null && !src.Limitation.IsAllDefault())
                        dst.Limitation = src.Limitation;
                });
            CreateMap<StateMachineEntity, StateMachineOutputDto>();
        }
    }
}

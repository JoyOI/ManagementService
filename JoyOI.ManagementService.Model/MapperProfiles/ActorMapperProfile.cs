using AutoMapper;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.MapperProfiles
{
    public class ActorMapperProfile : Profile
    {
        public ActorMapperProfile()
        {
            CreateMap<ActorInputDto, ActorEntity>()
                .ForMember(model => model.Name, model => model.Ignore())
                .ForMember(model => model.Body, model => model.Ignore())
                .AfterMap((src, dst) =>
                {
                    if (src.Name != null)
                        dst.Name = src.Name;
                    if (src.Body != null)
                        dst.Body = src.Body;
                });
            CreateMap<ActorEntity, ActorOutputDto>();
        }
    }
}

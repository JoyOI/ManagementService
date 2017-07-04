using AutoMapper;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.MapperProfiles
{
    public class BlobMapperProfile : Profile
    {
        public BlobMapperProfile()
        {
            CreateMap<BlobInputDto, BlobEntity>();
            CreateMap<BlobEntity, BlobOutputDto>();
        }
    }
}

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
            // blob需要分割大小, 目前不能通过AutoMapper自动转换
        }
    }
}

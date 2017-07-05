using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.ManagementService.Services.Impl
{
    internal class BlobService :
        EntityOperationServiceBase<BlobEntity, Guid, BlobInputDto, BlobOutputDto>,
        IBlobService
    {
        public BlobService(DbContext dbContext) : base(dbContext)
        {
        }
    }
}

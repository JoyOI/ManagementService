using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.ManagementService.Services.Impl
{
    internal class ActorService :
        EntityOperationServiceBase<ActorEntity, Guid, ActorInputDto, ActorOutputDto>,
        IActorService
    {
        public ActorService(DbContext dbContext) : base(dbContext)
        {
        }
    }
}

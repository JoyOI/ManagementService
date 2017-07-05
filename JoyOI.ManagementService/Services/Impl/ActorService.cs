using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using JoyOI.ManagementService.DbContexts;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理任务的服务
    /// </summary>
    internal class ActorService :
        EntityOperationServiceBase<ActorEntity, Guid, ActorInputDto, ActorOutputDto>,
        IActorService
    {
        public ActorService(JoyOIManagementContext dbContext) : base(dbContext)
        {
        }
    }
}

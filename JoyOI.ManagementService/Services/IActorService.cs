using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理任务的服务
    /// </summary>
    public interface IActorService :
        IEntityOperationService<ActorEntity, Guid, ActorInputDto, ActorOutputDto>
    {
    }
}

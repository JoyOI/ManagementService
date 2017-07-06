using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理任务的服务接口
    /// </summary>
    public interface IActorService :
        IEntityOperationService<ActorEntity, Guid, ActorInputDto, ActorOutputDto>,
        IEntityOperationByKeyService<ActorEntity, string, ActorInputDto, ActorOutputDto>
    {
    }
}

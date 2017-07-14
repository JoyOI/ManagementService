using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using JoyOI.ManagementService.DbContexts;
using System.Threading.Tasks;
using JoyOI.ManagementService.Repositories;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理任务的服务
    /// </summary>
    internal class ActorService :
        EntityOperationServiceBase<ActorEntity, Guid, ActorInputDto, ActorOutputDto>,
        IActorService
    {
        public ActorService(IRepository<ActorEntity, Guid> repository) : base(repository)
        {
        }

        public Task<long> Delete(string key)
        {
            return Delete(x => x.Name == key);
        }

        public Task<ActorOutputDto> Get(string key)
        {
            return Get(x => x.Name == key);
        }

        public Task<long> Patch(string key, ActorInputDto dto)
        {
            return Patch(x => x.Name == key, dto);
        }
    }
}

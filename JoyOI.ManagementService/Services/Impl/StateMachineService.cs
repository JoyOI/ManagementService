using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JoyOI.ManagementService.Repositories;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机的服务
    /// </summary>
    internal class StateMachineService :
        EntityOperationServiceBase<StateMachineEntity, Guid, StateMachineInputDto, StateMachineOutputDto>,
        IStateMachineService
    {
        public StateMachineService(IRepository<StateMachineEntity, Guid> repository) : base(repository)
        {
        }

        public Task<long> Delete(string key)
        {
            return Delete(x => x.Name == key);
        }

        public Task<StateMachineOutputDto> Get(string key)
        {
            return Get(x => x.Name == key);
        }

        public Task<long> Patch(string key, StateMachineInputDto dto)
        {
            return Patch(x => x.Name == key, dto);
        }
    }
}

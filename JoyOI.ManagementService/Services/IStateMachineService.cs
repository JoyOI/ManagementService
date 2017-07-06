using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理状态机的服务接口
    /// </summary>
    public interface IStateMachineService :
        IEntityOperationService<StateMachineEntity, Guid, StateMachineInputDto, StateMachineOutputDto>,
        IEntityOperationByKeyService<StateMachineEntity, string, StateMachineInputDto, StateMachineOutputDto>
    {
    }
}

using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using JoyOI.ManagementService.DbContexts;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机的服务
    /// </summary>
    internal class StateMachineService :
        EntityOperationServiceBase<StateMachineEntity, Guid, StateMachineInputDto, StateMachineOutputDto>,
        IStateMachineService
    {
        public StateMachineService(JoyOIManagementContext dbContext) : base(dbContext)
        {
        }
    }
}

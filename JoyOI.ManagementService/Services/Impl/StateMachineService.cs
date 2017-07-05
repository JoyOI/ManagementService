using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.ManagementService.Services.Impl
{
    internal class StateMachineService :
        EntityOperationServiceBase<StateMachineEntity, Guid, StateMachineInputDto, StateMachineOutputDto>,
        IStateMachineService
    {
        public StateMachineService(DbContext dbContext) : base(dbContext)
        {
        }
    }
}

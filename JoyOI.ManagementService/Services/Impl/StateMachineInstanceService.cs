using AutoMapper;
using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机实例的服务
    /// </summary>
    internal class StateMachineInstanceService : IStateMachineInstanceService
    {
        private JoyOIManagementConfiguration _configuration;
        private JoyOIManagementContext _dbContext;
        private DbSet<StateMachineInstanceEntity> _dbSet;
        private IStateMachineInstanceStore _stateMachineInstanceStore;

        public StateMachineInstanceService(
            JoyOIManagementConfiguration configuration,
            JoyOIManagementContext dbContext,
            IStateMachineInstanceStore stateMachineInstanceStore)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _dbSet = dbContext.Set<StateMachineInstanceEntity>();
            _stateMachineInstanceStore = stateMachineInstanceStore;
        }

        public async Task<StateMachineInstanceOutputDto> Get(Guid id)
        {
            var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                var dto = Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(entity);
                return dto;
            }
            return null;
        }

        public async Task<StateMachineInstancePutResultDto> Put(StateMachineInstancePutDto dto)
        {
            // 获取name对应的状态机代码
            var stateMachine = await _dbContext.Set<StateMachineEntity>()
                .FirstOrDefaultAsync(x => x.Name == dto.Name);
            if (stateMachine == null)
            {
                return StateMachineInstancePutResultDto.NotFound("state machine not found");
            }
            // 使用roslyn编译状态机代码
            var stateMachineInstance = await _stateMachineInstanceStore
                .CreateInstance(stateMachine.Name, stateMachine.Body);
            // 添加状态机实例到数据库
            var stateMachineInstanceEntity = new StateMachineInstanceEntity()
            {
                Id = PrimaryKeyUtils.Generate<Guid>(),
                Name = stateMachine.Name,
                Status = StateMachineStatus.Running,
                FinishedActors = new ActorInfo[0],
                CurrentActor = new ActorInfo()
                {
                    Name = null,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.MaxValue,
                    Inputs = dto.Inputs?.ToArray() ?? new BlobInfo[0],
                    Outputs = new BlobInfo[0],
                    Exceptions = new string[0],
                    Status = ActorStatus.Running
                },
                CurrentNode = null,
                CurrentContainer = null,
                FromManagementService = _configuration.Name,
                ReRunTimes = 0,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.MaxValue
            };
            _dbSet.Add(stateMachineInstanceEntity);
            await _dbContext.SaveChangesAsync();
            // 更新状态机实例的数据
            stateMachineInstance.Id = stateMachineInstanceEntity.Id;
            stateMachineInstance.Status = stateMachineInstanceEntity.Status;
            stateMachineInstance.FinishedActors = stateMachineInstanceEntity.FinishedActors;
            stateMachineInstance.CurrentActor = stateMachineInstanceEntity.CurrentActor;
            // 调用RunAsync(null, blobs), 从这里开始会在后台运行
#pragma warning disable CS4014
            _stateMachineInstanceStore.RunInstance(stateMachineInstance);
#pragma warning restore CS4014
            return StateMachineInstancePutResultDto.Success(
                Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(stateMachineInstanceEntity));
        }

        public Task Patch(StateMachineInstancePatchDto dto)
        {
            throw new NotImplementedException();
        }
    }
}

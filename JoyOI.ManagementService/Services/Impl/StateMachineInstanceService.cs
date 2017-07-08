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
using Newtonsoft.Json;
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

        public async Task<IList<StateMachineInstanceOutputDto>> Search(string name, string stage)
        {
            var query =  _dbSet.AsNoTracking();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name == name);
            }
            if (!string.IsNullOrEmpty(stage))
            {
                query = query.Where(x => x.Stage == stage);
            }
            var entities = await query.ToListAsync();
            var dtos = new List<StateMachineInstanceOutputDto>(entities.Count);
            foreach (var entity in entities)
            {
                dtos.Add(Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(entity));
            }
            return dtos;
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
            var stateMachineEntity = await _dbContext.Set<StateMachineEntity>()
                .FirstOrDefaultAsync(x => x.Name == dto.Name);
            if (stateMachineEntity == null)
            {
                return StateMachineInstancePutResultDto.NotFound("state machine not found");
            }
            // 创建状态机实例
            var stateMachineInstanceEntity = new StateMachineInstanceEntity()
            {
                Id = PrimaryKeyUtils.Generate<Guid>(),
                Name = stateMachineEntity.Name,
                Status = StateMachineStatus.Running,
                Stage = StateMachineBase.InitialStage,
                StartedActors = new List<ActorInfo>(),
                InitialBlobs = dto.InitialBlobs ?? new BlobInfo[0],
                Limitation = ContainerLimitation.Default
                    .WithDefaults(dto.Limitation)
                    .WithDefaults(stateMachineEntity.Limitation)
                    .WithDefaults(_configuration.Limitation),
                FromManagementService = _configuration.Name,
                ReRunTimes = 0,
                StartTime = DateTime.UtcNow,
                EndTime = null
            };
            var stateMachineInstance = await _stateMachineInstanceStore.CreateInstance(
                stateMachineEntity, stateMachineInstanceEntity);
            // 添加状态机实例到数据库
            await _dbSet.AddAsync(stateMachineInstanceEntity);
            await _dbContext.SaveChangesAsync();
            // 运行状态机, 从这里开始会在后台运行
#pragma warning disable CS4014
            _stateMachineInstanceStore.RunInstance(stateMachineInstance);
#pragma warning restore CS4014
            return StateMachineInstancePutResultDto.Success(
                Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(stateMachineInstanceEntity));
        }

        public Task<StateMachineInstancePatchResultDto> Patch(Guid id, StateMachineInstancePatchDto dto)
        {
            throw new NotImplementedException();
        }
    }
}

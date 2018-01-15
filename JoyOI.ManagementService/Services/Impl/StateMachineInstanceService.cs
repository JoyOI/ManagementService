using AutoMapper;
using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Repositories;
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
        private const int ConcurrencyErrorMaxRetryTimes = 100;
        private JoyOIManagementConfiguration _configuration;
        private IStateMachineInstanceStore _stateMachineInstanceStore;
        private IRepository<StateMachineInstanceEntity, Guid> _repository;
        private IRepository<StateMachineEntity, Guid> _stateMachineRepository;

        public StateMachineInstanceService(
            JoyOIManagementConfiguration configuration,
            IStateMachineInstanceStore stateMachineInstanceStore,
            IRepository<StateMachineInstanceEntity, Guid> repository,
            IRepository<StateMachineEntity, Guid> stateMachineRepository)
        {
            _configuration = configuration;
            _stateMachineInstanceStore = stateMachineInstanceStore;
            _repository = repository;
            _stateMachineRepository = stateMachineRepository;
        }

        public async Task<IList<StateMachineInstanceOutputDto>> Search(
            string name, string stage, string status, string begin_time, string finish_time)
        {
            var entities = await _repository.QueryNoTrackingAsync(q =>
            {
                if (!string.IsNullOrEmpty(name))
                {
                    q = q.Where(x => x.Name == name);
                }
                if (!string.IsNullOrEmpty(stage))
                {
                    q = q.Where(x => x.Stage == stage);
                }
                if (!string.IsNullOrEmpty(status))
                {
                    var statusEnum = (StateMachineStatus)Enum.Parse(typeof(StateMachineStatus), status);
                    q = q.Where(x => x.Status == statusEnum);
                }
                if (!string.IsNullOrEmpty(begin_time))
                {
                    var time = DateTime.Parse(begin_time).ToUniversalTime();
                    q = q.Where(x => x.StartTime >= time);
                }
                if (!string.IsNullOrEmpty(finish_time))
                {
                    var time = DateTime.Parse(finish_time).ToUniversalTime();
                    q = q.Where(x => (x.StartTime < time) || (x.EndTime != null && x.EndTime < time));
                }
                return q.ToListAsyncTestable();
            });
            var dtos = new List<StateMachineInstanceOutputDto>(entities.Count);
            foreach (var entity in entities)
            {
                dtos.Add(Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(entity));
            }
            return dtos;
        }

        public async Task<StateMachineInstanceOutputDto> Get(Guid id)
        {
            var entity = await _repository.QueryNoTrackingAsync(q =>
                q.FirstOrDefaultAsyncTestable(x => x.Id == id));
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
            var stateMachineEntity = await _stateMachineRepository.QueryNoTrackingAsync(q =>
                q.FirstOrDefaultAsyncTestable(x => x.Name == dto.Name));
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
                Parameters = dto.Parameters,
                Priority = dto.Priority,
                FromManagementService = _configuration.Name,
                ReRunTimes = 0,
                Exception = null,
                ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString(),
                StartTime = DateTime.UtcNow,
                EndTime = null
            };
            var stateMachineInstance = await _stateMachineInstanceStore.CreateInstance(
                stateMachineEntity, stateMachineInstanceEntity);
            // 添加状态机实例到数据库
            await _repository.AddAsync(stateMachineInstanceEntity);
            await _repository.SaveChangesAsync();
            // 运行状态机, 从这里开始会在后台运行
#pragma warning disable CS4014
            _stateMachineInstanceStore.RunInstance(stateMachineInstance);
#pragma warning restore CS4014
            return StateMachineInstancePutResultDto.Success(
                Mapper.Map<StateMachineInstanceEntity, StateMachineInstanceOutputDto>(stateMachineInstanceEntity));
        }

        public async Task<StateMachineInstancePatchResultDto> Patch(Guid id, StateMachineInstancePatchDto dto)
        {
            // 更新阶段, 参数和并发键
            StateMachineInstanceEntity stateMachineInstanceEntity = null;
            StateMachineEntity stateMachineEntity = null;
            for (int from = 0; from <= ConcurrencyErrorMaxRetryTimes; ++from)
            {
                stateMachineInstanceEntity = await _repository.QueryAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Id == id));
                if (stateMachineInstanceEntity == null)
                {
                    return StateMachineInstancePatchResultDto.NotFound("state machine instance not found");
                }
                stateMachineEntity = await _stateMachineRepository.QueryNoTrackingAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Name == stateMachineInstanceEntity.Name));
                if (stateMachineEntity == null)
                {
                    return StateMachineInstancePatchResultDto.NotFound("state machine not found");
                }
                stateMachineInstanceEntity.Status = StateMachineStatus.Running;
                stateMachineInstanceEntity.Stage = dto.Stage ?? StateMachineBase.InitialStage;
                stateMachineInstanceEntity.ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString();
                stateMachineInstanceEntity.FromManagementService = _configuration.Name;
                stateMachineInstanceEntity.StartTime = DateTime.UtcNow; // 柚子大姐要求
                if (dto.Parameters != null)
                {
                    var parameters = stateMachineInstanceEntity.Parameters;
                    foreach (var pair in dto.Parameters)
                    {
                        parameters[pair.Key] = pair.Value;
                    }
                    stateMachineInstanceEntity.Parameters = parameters;
                }
                try
                {
                    await _repository.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (from == ConcurrencyErrorMaxRetryTimes)
                    {
                        throw;
                    }
                    await Task.Delay(1);
                }
            }
            // 创建状态机实例
            var stateMachineInstance = await _stateMachineInstanceStore.CreateInstance(
                stateMachineEntity, stateMachineInstanceEntity);
            // 运行状态机, 从这里开始会在后台运行
#pragma warning disable CS4014
            _stateMachineInstanceStore.RunInstance(stateMachineInstance);
#pragma warning restore CS4014
            return StateMachineInstancePatchResultDto.Success();
        }

        public async Task<long> Delete(Guid id)
        {
            // 删除指定id的状态机实例, 因为有并发键需要重试多次
            StateMachineInstanceEntity stateMachineInstanceEntity = null;
            for (int from = 0; from <= ConcurrencyErrorMaxRetryTimes; ++from)
            {
                stateMachineInstanceEntity = await _repository.QueryAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Id == id));
                if (stateMachineInstanceEntity == null)
                {
                    return 0;
                }
                _repository.Remove(stateMachineInstanceEntity);
                try
                {
                    await _repository.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (from == ConcurrencyErrorMaxRetryTimes)
                    {
                        throw;
                    }
                    await Task.Delay(1);
                }
            }
            return 1;
        }
    }
}

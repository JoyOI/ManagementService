using JoyOI.ManagementService.Model.Dtos.Interfaces;
using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq;
using JoyOI.ManagementService.Utils;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Core;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 实体操作服务的通用基类
    /// 如果逻辑特殊可以重写部分函数或不使用此基类
    /// </summary>
    internal abstract class EntityOperationServiceBase<TEntity, TPrimaryKey, TInputDto, TOutputDto> :
        IEntityOperationService<TEntity, TPrimaryKey, TInputDto, TOutputDto>
        where TEntity : class, IEntity<TPrimaryKey>, new()
        where TInputDto : IInputDto
        where TOutputDto : IOutputDto
    {
        private IRepository<TEntity, TPrimaryKey> _repository;

        public EntityOperationServiceBase(IRepository<TEntity, TPrimaryKey> repository)
        {
            _repository = repository;
        }

        public async Task<long> Delete(Expression<Func<TEntity, bool>> expression)
        {
            var entity = await _repository.QueryAsync(q => q
                .Where(expression)
                .Select(x => new TEntity() { Id = x.Id })
                .FirstOrDefaultAsyncTestable());
            if (entity != null)
            {
                _repository.Remove(entity);
                await _repository.SaveChangesAsync();
                return 1;
            }
            return 0;
        }

        public async Task<TOutputDto> Get(Expression<Func<TEntity, bool>> expression)
        {
            var entity = await _repository.QueryNoTrackingAsync(q =>
            {
                if (expression != null)
                    q = q.Where(expression);
                return q.FirstOrDefaultAsyncTestable();
            });
            if (entity != null)
            {
                var dto = Mapper.Map<TEntity, TOutputDto>(entity);
                return dto;
            }
            return default(TOutputDto);
        }

        public async Task<IList<TOutputDto>> GetAll(Expression<Func<TEntity, bool>> expression)
        {
            var entities = await _repository.QueryNoTrackingAsync(q =>
            {
                if (expression != null)
                    q = q.Where(expression);
                return q.ToListAsyncTestable();
            });
            var dtos = new List<TOutputDto>(entities.Count);
            foreach (var entity in entities)
            {
                dtos.Add(Mapper.Map<TEntity, TOutputDto>(entity));
            }
            return dtos;
        }

        public async Task<long> Patch(Expression<Func<TEntity, bool>> expression, TInputDto dto)
        {
            var entity = await _repository.QueryAsync(q =>
                q.FirstOrDefaultAsyncTestable(expression));
            if (entity != null)
            {
                Mapper.Map<TInputDto, TEntity>(dto, entity);
                if (entity is IEntityWithUpdateTime updateTimeEntity)
                {
                    updateTimeEntity.UpdateTime = DateTime.UtcNow;
                }
                _repository.Update(entity); // 设置所有字段为updated, 以防万一检测不出
                await _repository.SaveChangesAsync();
                return 1;
            }
            return 0;
        }

        public async Task<TPrimaryKey> Put(TInputDto dto)
        {
            var entity = new TEntity();
            entity.Id = PrimaryKeyUtils.Generate<TPrimaryKey>();
            Mapper.Map<TInputDto, TEntity>(dto, entity);
            if (entity is IEntityWithCreateTime createTimeEntity)
            {
                createTimeEntity.CreateTime = DateTime.UtcNow;
            }
            if (entity is IEntityWithUpdateTime updateTimeEntity)
            {
                updateTimeEntity.UpdateTime = DateTime.UtcNow;
            }
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return entity.Id;
        }
    }
}

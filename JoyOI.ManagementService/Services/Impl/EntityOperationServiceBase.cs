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
        private DbContext _dbContext;
        private DbSet<TEntity> _dbSet;

        public EntityOperationServiceBase(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public virtual async Task<bool> Delete(TPrimaryKey id)
        {
            var entity = await _dbSet
                .Where(x => x.Id.Equals(id))
                .Select(x => new TEntity() { Id = x.Id })
                .FirstOrDefaultAsync();
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public virtual Task<IList<TOutputDto>> Get()
        {
            return Get(null);
        }

        public virtual async Task<IList<TOutputDto>> Get(Expression<Func<TEntity, bool>> expression)
        {
            var queryable = _dbSet.AsNoTracking();
            if (expression != null)
            {
                queryable = queryable.Where(expression);
            }
            var entities = await queryable.ToListAsync();
            var dtos = new List<TOutputDto>(entities.Count);
            foreach (var entity in entities)
            {
                dtos.Add(Mapper.Map<TEntity, TOutputDto>(entity));
            }
            return dtos;
        }

        public virtual async Task<TOutputDto> Get(TPrimaryKey id)
        {
            var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity != null)
            {
                var dto = Mapper.Map<TEntity, TOutputDto>(entity);
                return dto;
            }
            return default(TOutputDto);
        }

        public virtual async Task<bool> Patch(TPrimaryKey id, TInputDto dto)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity != null)
            {
                Mapper.Map<TInputDto, TEntity>(dto, entity);
                if (entity is IEntityWithUpdateTime updateTimeEntity)
                {
                    updateTimeEntity.UpdateTime = DateTime.UtcNow;
                }
                _dbSet.Update(entity); // 设置所有字段为updated, 以防万一检测不出
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public virtual async Task<TPrimaryKey> Put(TInputDto dto)
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
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }
    }
}

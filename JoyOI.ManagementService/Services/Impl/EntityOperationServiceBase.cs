using JoyOI.ManagementService.Model.Dtos.Interfaces;
using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 实体操作服务的通用基类, 可以满足大部分但不是全部情况
    /// </summary>
    internal abstract class EntityOperationServiceBase<TEntity, TPrimaryKey, TInputDto, TOutputDto> :
        IEntityOperationService<TEntity, TPrimaryKey, TInputDto, TOutputDto>
        where TEntity : class, IEntity<TPrimaryKey>
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

        public Task<bool> Delete(TPrimaryKey id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TOutputDto>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<IList<TOutputDto>> Get(Expression<Func<TEntity, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<TOutputDto> Get(TPrimaryKey id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Patch(TPrimaryKey id, TInputDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<TPrimaryKey> Put(TInputDto dto)
        {
            throw new NotImplementedException();
        }
    }
}

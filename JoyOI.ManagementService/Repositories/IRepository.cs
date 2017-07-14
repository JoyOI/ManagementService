using JoyOI.ManagementService.Model.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Repositories
{
    /// <summary>
    /// 仓储的接口
    /// 因EF Core无法参与单元测试(InMemory不能用于多线程环境), 这个接口用于在测试中模拟数据层
    /// </summary>
    public interface IRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        /// <summary>
        /// 查询实体
        /// </summary>
        Task<T> QueryAsync<T>(Func<IQueryable<TEntity>, Task<T>> func);

        /// <summary>
        /// 查询实体, 不跟踪
        /// </summary>
        Task<T> QueryNoTrackingAsync<T>(Func<IQueryable<TEntity>, Task<T>> func);

        /// <summary>
        /// 添加实体
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// 添加多个实体
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        void Remove(TEntity entity);

        /// <summary>
        /// 删除多个实体
        /// </summary>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// 保存所有更改
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// 开始事务
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}

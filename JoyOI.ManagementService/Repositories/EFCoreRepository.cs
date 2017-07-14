using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Threading.Tasks;
using JoyOI.ManagementService.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.ManagementService.Repositories
{
    public class EFCoreRepository<TEntity, TPrimaryKey>
        : IRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        private JoyOIManagementContext _context;

        public EFCoreRepository(JoyOIManagementContext context)
        {
            _context = context;
        }

        public Task AddAsync(TEntity entity)
        {
            return _context.AddAsync(entity);
        }

        public Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            return _context.AddRangeAsync(entities);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }

        public Task<T> QueryAsync<T>(Func<IQueryable<TEntity>, Task<T>> func)
        {
            return func(_context.Set<TEntity>());
        }

        public Task<T> QueryNoTrackingAsync<T>(Func<IQueryable<TEntity>, Task<T>> func)
        {
            return func(_context.Set<TEntity>().AsNoTracking());
        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Update(TEntity entity)
        {
            _context.Update(entity);
        }
    }
}

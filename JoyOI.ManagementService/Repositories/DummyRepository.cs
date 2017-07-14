using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

/**
 * 到底发生了什么?
 * 
 * EF Core的InMemory在多线程环境下会出现数据不同步的问题, 故无法用于单元测试
 * 于是我创建了IRepository接口
 * 然后, 很奇葩的是遇到了这个问题 https://github.com/aspnet/EntityFramework/issues/9179
 * 我尝试过自己实现IAsyncQueryProvider (https://msdn.microsoft.com/en-us/data/dn314429#async), 但错误依旧
 * 目前只能把所有Async函数都另外包装一层, 例如 ToListAsync => ToListAsyncTestable
 */

namespace JoyOI.ManagementService.Repositories
{
    internal class DummyTransaction : IDbContextTransaction
    {
        public Guid TransactionId => Guid.Empty;

        public void Commit()
        {
        }

        public void Dispose()
        {
        }

        public void Rollback()
        {
        }
    }

    public class DummyStorage
    {
        internal SemaphoreSlim TableLock { get; set; }
        internal IDictionary<Type, object> Tables { get; set; }

        public DummyStorage()
        {
            TableLock = new SemaphoreSlim(1);
            Tables = new Dictionary<Type, object>();
        }

        internal IDictionary<TPrimaryKey, TEntity> GetTableThreadUnsafe<TEntity, TPrimaryKey>()
        {
            if (!Tables.TryGetValue(typeof(TEntity), out var table))
                table = Tables[typeof(TEntity)] = new Dictionary<TPrimaryKey, TEntity>();
            return (IDictionary<TPrimaryKey, TEntity>)table;
        }
    }

    public class DummyRepository<TEntity, TPrimaryKey>
        : IRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        private DummyStorage _storage { get; set; }
        private Func<TEntity, TEntity> _clone { get; set; }

        public DummyRepository(DummyStorage storage)
        {
            _storage = storage;
            _clone = e => JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(e));
        }

        public async Task<T> QueryAsync<T>(Func<IQueryable<TEntity>, Task<T>> func)
        {
            await _storage.TableLock.WaitAsync();
            try
            {
                var query = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>()
                    .Select(x => x.Value).AsQueryable();
                return await func(query);
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public async Task<T> QueryNoTrackingAsync<T>(Func<IQueryable<TEntity>, Task<T>> func)
        {
            await _storage.TableLock.WaitAsync();
            try
            {
                var query = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>()
                    .Select(x => _clone(x.Value)).AsQueryable();
                return await func(query);
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public async Task AddAsync(TEntity entity)
        {
            await _storage.TableLock.WaitAsync();
            try
            {
                var table = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>();
                table[entity.Id] = entity;
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _storage.TableLock.WaitAsync();
            try
            {
                var table = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>();
                foreach (var entity in entities)
                {
                    table[entity.Id] = entity;
                }
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public async void Update(TEntity entity)
        {
            await _storage.TableLock.WaitAsync();
            try
            {
                var table = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>();
                table[entity.Id] = entity;
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public void Remove(TEntity entity)
        {
            _storage.TableLock.Wait();
            try
            {
                var table = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>();
                table.Remove(entity.Id);
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _storage.TableLock.Wait();
            try
            {
                var table = _storage.GetTableThreadUnsafe<TEntity, TPrimaryKey>();
                foreach (var entity in entities)
                {
                    table.Remove(entity.Id);
                }
            }
            finally
            {
                _storage.TableLock.Release();
            }
        }

        public Task SaveChangesAsync()
        {
            return Task.FromResult(0);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return Task.FromResult<IDbContextTransaction>(new DummyTransaction());
        }
    }
}

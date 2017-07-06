using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 数据库上下文的工具函数
    /// </summary>
    public static class DbContextUtils
    {
        /// <summary>
        /// 返回是否内存数据库
        /// </summary>
        public static bool IsMemoryDb(DbContext context)
        {
            return context.Database.ProviderName.EndsWith(".InMemory");
        }

        /// <summary>
        /// 开始事务, 内存数据库时返回假事务
        /// </summary>
        public static Task<IDbContextTransaction> BeginTransactionAsync(DbContext context)
        {
            return BeginTransactionAsync(context, IsMemoryDb(context));
        }

        /// <summary>
        /// 开始事务, 内存数据库时返回假事务
        /// </summary>
        public static Task<IDbContextTransaction> BeginTransactionAsync(DbContext context, IsolationLevel isolationLevel)
        {
            return BeginTransactionAsync(context, isolationLevel, IsMemoryDb(context));
        }

        /// <summary>
        /// 开始事务, 内存数据库时返回假事务
        /// </summary>
        public static Task<IDbContextTransaction> BeginTransactionAsync(DbContext context, bool isMemoryDb)
        {
            if (isMemoryDb)
                return Task.FromResult<IDbContextTransaction>(new FakeDbContextTransaction());
            return context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 开始事务, 内存数据库时返回假事务
        /// </summary>
        public static Task<IDbContextTransaction> BeginTransactionAsync(DbContext context, IsolationLevel isolationLevel, bool isMemoryDb)
        {
            if (isMemoryDb)
                return Task.FromResult<IDbContextTransaction>(new FakeDbContextTransaction());
            return context.Database.BeginTransactionAsync(isolationLevel);
        }

        /// <summary>
        /// 假事务
        /// </summary>
        private class FakeDbContextTransaction : IDbContextTransaction
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
    }
}

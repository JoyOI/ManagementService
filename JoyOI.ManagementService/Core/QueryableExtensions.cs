using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 参考: https://github.com/aspnet/EntityFramework/issues/9179
    /// </summary>
    public static class QueryableExtensions
    {
        public static Task<TSource> FirstOrDefaultAsyncTestable<TSource>(
            this IQueryable<TSource> source)
        {
            if (source.Provider is IAsyncQueryProvider)
                return source.FirstOrDefaultAsync();
            return Task.FromResult(source.FirstOrDefault());
        }

        public static Task<TSource> FirstOrDefaultAsyncTestable<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source.Provider is IAsyncQueryProvider)
                return source.FirstOrDefaultAsync(predicate);
            return Task.FromResult(source.FirstOrDefault(predicate));
        }

        public static Task<List<TSource>> ToListAsyncTestable<TSource>(this IQueryable<TSource> source)
        {
            if (source.Provider is IAsyncQueryProvider)
                return source.ToListAsync();
            return Task.FromResult(source.ToList());
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsyncTestable<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> selectKey)
        {
            if (source.Provider is IAsyncQueryProvider)
                return source.ToDictionaryAsync(selectKey);
            return Task.FromResult(source.ToDictionary(selectKey));
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsyncTestable<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> selectKey,
            Func<TSource, TElement> selectElement)
        {
            if (source.Provider is IAsyncQueryProvider)
                return source.ToDictionaryAsync(selectKey, selectElement);
            return Task.FromResult(source.ToDictionary(selectKey, selectElement));
        }
    }
}

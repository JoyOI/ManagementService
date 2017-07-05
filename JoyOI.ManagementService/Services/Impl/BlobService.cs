using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyOI.ManagementService.DbContexts;
using System.Linq;
using AutoMapper;
using JoyOI.ManagementService.Utils;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理文件的服务
    /// 文件需要分库储存, 不使用通用的基类
    /// </summary>
    internal class BlobService : IBlobService
    {
        private DbContext _dbContext;
        private DbSet<BlobEntity> _dbSet;

        public BlobService(JoyOIManagementContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<BlobEntity>();
        }

        public BlobOutputDto MergeChunks(IList<BlobEntity> entities)
        {
            if (entities.Count == 0)
            {
                return null;
            }
            var dto = new BlobOutputDto();
            dto.Id = entities[0].BlobId;
            dto.Name = entities[0].Name;
            dto.TimeStamp = Mapper.Map<DateTime, long>(entities[0].UpdateTime);
            if (entities.Count == 1)
            {
                // 不需要合并
                dto.Body = Mapper.Map<byte[], string>(entities[0].Body);
            }
            else
            {
                // 需要合并
                var bodyBytes = new byte[entities.Sum(e => e.Body.Length)];
                var bodyBytesStart = 0;
                foreach (var entity in entities)
                {
                    Array.Copy(entity.Body, 0, bodyBytes, bodyBytesStart, entity.Body.Length);
                    bodyBytesStart += entity.Body.Length;
                }
                dto.Body = Mapper.Map<byte[], string>(bodyBytes);
            }
            return dto;
        }

        public IEnumerable<BlobEntity> SplitChunks(Guid blobId, BlobInputDto dto)
        {
            var bodyBytes = Mapper.Map<string, byte[]>(dto.Body);
            var bodyBytesStart = 0;
            var chunkIndex = 0;
            do
            {
                var entityBodySize = Math.Min(bodyBytes.Length - bodyBytesStart, BlobEntity.BlobChunkSize);
                var entity = new BlobEntity();
                entity.BlobId = blobId;
                entity.ChunkIndex = chunkIndex++;
                entity.Name = dto.Name;
                if (bodyBytesStart == 0 && entityBodySize == bodyBytes.Length)
                {
                    // 不需要分块
                    entity.Body = bodyBytes;
                }
                else
                {
                    entity.Body = new byte[entityBodySize];
                    Array.Copy(bodyBytes, bodyBytesStart, entity.Body, 0, entityBodySize);
                }
                bodyBytesStart += entityBodySize;
                entity.TimeStamp = Mapper.Map<long, DateTime>(dto.TimeStamp);
                entity.CreateTime = DateTime.UtcNow;
                entity.UpdateTime = DateTime.UtcNow;
                yield return entity;
            } while (bodyBytesStart < bodyBytes.Length);
        }

        public async Task<bool> Delete(Guid id)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                var entities = await _dbSet
                    .Where(x => x.BlobId == id)
                    .Select(x => new BlobEntity() {
                        Id = x.Id,
                        BlobId = x.BlobId,
                        ChunkIndex = x.ChunkIndex
                    })
                    .ToListAsync();
                if (entities.Count > 0)
                {
                    _dbSet.RemoveRange(entities);
                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                return false;
            }
        }

        public Task<IList<BlobOutputDto>> Get()
        {
            return Get(null);
        }

        public async Task<IList<BlobOutputDto>> Get(Expression<Func<BlobEntity, bool>> expression)
        {
            IList<BlobEntity> entities;
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                var queryable = _dbSet.AsNoTracking();
                if (expression != null)
                {
                    queryable = queryable.Where(expression);
                }
                entities = await queryable.ToListAsync();
            }
            var dtos = new List<BlobOutputDto>(32);
            foreach (var group in entities.GroupBy(e => e.BlobId))
            {
                dtos.Add(MergeChunks(group.ToList()));
            }
            return dtos;
        }

        public async Task<BlobOutputDto> Get(Guid id)
        {
            IList<BlobEntity> entities;
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                entities = _dbSet.AsNoTracking()
                    .Where(x => x.BlobId == id)
                    .ToList();
            }
            return MergeChunks(entities);
        }

        public async Task<bool> Patch(Guid id, BlobInputDto dto)
        {
            if (dto.Body == null)
            {
                // 不更新内容
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    var entities = await _dbSet.Where(x => x.BlobId == id).ToListAsync();
                    if (entities.Count > 0)
                    {
                        foreach (var entity in entities)
                        {
                            if (dto.Name != null)
                                entity.Name = dto.Name;
                            if (dto.TimeStamp > 0)
                                entity.TimeStamp = Mapper.Map<long, DateTime>(dto.TimeStamp);
                        }
                        await _dbContext.SaveChangesAsync();
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // 更新内容, 需要先删除再创建
                BlobEntity existEntity;
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    var entities = await _dbSet
                        .Where(x => x.BlobId == id)
                        .Select(x => new BlobEntity()
                        {
                            Id = x.Id,
                            BlobId = x.BlobId,
                            ChunkIndex = x.ChunkIndex,
                            Name = x.Name,
                            TimeStamp = x.TimeStamp,
                            CreateTime = x.CreateTime
                        })
                        .ToListAsync();
                    if (entities.Count > 0)
                    {
                        _dbSet.RemoveRange(entities);
                        await _dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    existEntity = entities.FirstOrDefault();
                }
                if (existEntity != null)
                {
                    var chunks = new List<BlobEntity>(SplitChunks(id, dto));
                    foreach (var chunk in chunks)
                    {
                        // 名称无更新时使用原值
                        if (dto.Name == null)
                            chunk.Name = existEntity.Name;
                        // 时间戳无更新时使用原值
                        if (dto.TimeStamp <= 0)
                            chunk.TimeStamp = existEntity.TimeStamp;
                        // 创建时间使用原值
                        chunk.CreateTime = existEntity.CreateTime;
                    }
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        await _dbSet.AddRangeAsync(chunks);
                        await _dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<Guid> Put(BlobInputDto dto)
        {
            var blobId = PrimaryKeyUtils.Generate<Guid>();
            var chunks = new List<BlobEntity>(SplitChunks(blobId, dto));
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                await _dbSet.AddRangeAsync(chunks);
                await _dbContext.SaveChangesAsync();
                transaction.Commit();
            }
            return blobId;
        }
    }
}

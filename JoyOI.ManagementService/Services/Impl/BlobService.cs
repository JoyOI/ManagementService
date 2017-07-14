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
    /// 注意: 相同内容的文件会合并, 删除和修改文件时必须确认文件未被使用
    /// </summary>
    internal class BlobService : IBlobService
    {
        private JoyOIManagementContext _dbContext;
        private DbSet<BlobEntity> _dbSet;
        private bool _isInMemory;

        public BlobService(JoyOIManagementContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Blobs;
            _isInMemory = DbContextUtils.IsMemoryDb(dbContext);
        }

        public BlobOutputDto MergeChunks(IList<BlobEntity> entities)
        {
            if (entities.Count == 0)
            {
                return null;
            }
            var dto = new BlobOutputDto();
            dto.Id = entities[0].BlobId;
            dto.TimeStamp = Mapper.Map<DateTime, long>(entities[0].TimeStamp);
            dto.Remark = entities[0].Remark;
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
            var bodyHash = HashUtils.GetSHA256Hash(bodyBytes);
            var chunkIndex = 0;
            do
            {
                var entityBodySize = Math.Min(bodyBytes.Length - bodyBytesStart, BlobEntity.BlobChunkSize);
                var entity = new BlobEntity();
                entity.BlobId = blobId;
                entity.ChunkIndex = chunkIndex++;
                entity.Remark = dto.Remark;
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
                entity.TimeStamp = dto.TimeStamp == 0 ?
                    DateTime.UtcNow :
                    Mapper.Map<long, DateTime>(dto.TimeStamp);
                entity.BodyHash = bodyHash;
                entity.CreateTime = DateTime.UtcNow;
                yield return entity;
            } while (bodyBytesStart < bodyBytes.Length);
        }

        public async Task<long> Delete(Expression<Func<BlobEntity, bool>> expression)
        {
            IList<BlobEntity> entities = null;
            using (var transaction = await DbContextUtils.BeginTransactionAsync(_dbContext, _isInMemory))
            {
                var query = _dbSet.Where(expression);
                if (!_isInMemory)
                {
                    query = query.Select(x => new BlobEntity()
                    {
                        Id = x.Id,
                        BlobId = x.BlobId,
                        ChunkIndex = x.ChunkIndex
                    });
                }
                entities = await query.ToListAsync();
                if (entities.Count > 0)
                {
                    _dbSet.RemoveRange(entities);
                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
            }
            return entities?.Select(x => x.BlobId).Distinct().LongCount() ?? 0;
        }

        public Task<long> Delete(Guid key)
        {
            return Delete(x => x.BlobId == key);
        }

        public async Task<BlobOutputDto> Get(Expression<Func<BlobEntity, bool>> expression)
        {
            IList<BlobEntity> entities;
            using (var transaction = await DbContextUtils.BeginTransactionAsync(_dbContext, _isInMemory))
            {
                entities = _dbSet.AsNoTracking()
                    .Where(expression)
                    .OrderBy(x => x.ChunkIndex)
                    .ToList();
            }
            return MergeChunks(entities);
        }

        public Task<BlobOutputDto> Get(Guid key)
        {
            return Get(x => x.BlobId == key);
        }

        public async Task<IList<BlobOutputDto>> GetAll(Expression<Func<BlobEntity, bool>> expression)
        {
            IList<BlobEntity> entities;
            using (var transaction = await DbContextUtils.BeginTransactionAsync(_dbContext, _isInMemory))
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
                dtos.Add(MergeChunks(group.OrderBy(x => x.ChunkIndex).ToList()));
            }
            return dtos;
        }

        public Task<long> Patch(Expression<Func<BlobEntity, bool>> expression, BlobInputDto dto)
        {
            throw new NotSupportedException("modify an exist blob without changing it's blob id is dangerous");
        }

        public Task<long> Patch(Guid key, BlobInputDto dto)
        {
            return Patch(x => x.BlobId == key, dto);
        }

        public async Task<Guid> Put(BlobInputDto dto)
        {
            var blobId = PrimaryKeyUtils.Generate<Guid>();
            var chunks = new List<BlobEntity>(SplitChunks(blobId, dto));
            using (var transaction = await DbContextUtils.BeginTransactionAsync(_dbContext, _isInMemory))
            {
                // 如果有相同内容的blob时, 返回原blob的id
                var existBlobId = await _dbSet
                    .Where(x => x.BodyHash == chunks[0].BodyHash)
                    .Select(x => x.BlobId)
                    .FirstOrDefaultAsync();
                if (existBlobId != Guid.Empty)
                {
                    return existBlobId;
                }
                // 添加新的blob
                // 注意并发添加时可能会添加相同内容的blob
                await _dbSet.AddRangeAsync(chunks);
                await _dbContext.SaveChangesAsync();
                transaction.Commit();
            }
            return blobId;
        }
    }
}

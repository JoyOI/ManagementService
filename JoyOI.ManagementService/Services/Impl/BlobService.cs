using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using JoyOI.ManagementService.Utils;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Core;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理文件的服务
    /// 文件需要分库储存, 不使用通用的基类
    /// 注意: 相同内容的文件会合并, 删除和修改文件时必须确认文件未被使用
    /// </summary>
    internal class BlobService : IBlobService
    {
        private IRepository<BlobEntity, Guid> _repository;

        public BlobService(IRepository<BlobEntity, Guid> repository)
        {
            _repository = repository;
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
            // 合并各个分块的内容
            byte[] bodyBytes;
            if (entities.Count == 1)
            {
                bodyBytes = entities[0].Body;
            }
            else
            {
                bodyBytes = new byte[entities.Sum(e => e.Body.Length)];
                var bodyBytesStart = 0;
                foreach (var entity in entities)
                {
                    Array.Copy(entity.Body, 0, bodyBytes, bodyBytesStart, entity.Body.Length);
                    bodyBytesStart += entity.Body.Length;
                }
            }
            // 解压缩合并后的内容
            bodyBytes = ArchiveUtils.DecompressFromGZip(bodyBytes);
            dto.Body = Mapper.Map<byte[], string>(bodyBytes);
            return dto;
        }

        public IEnumerable<BlobEntity> SplitChunks(
            Guid blobId, byte[] originalBytes, string hash, string remark, long timeStamp)
        {
            // 压缩分块之前的内容 
            var bodyBytes = ArchiveUtils.CompressToGZip(originalBytes);
            var chunkIndex = 0;
            var bodyBytesStart = 0;
            // 对内容进行分块
            do
            {
                var entityBodySize = Math.Min(bodyBytes.Length - bodyBytesStart, BlobEntity.BlobChunkSize);
                var entity = new BlobEntity();
                entity.Id = PrimaryKeyUtils.Generate<Guid>();
                entity.BlobId = blobId;
                entity.ChunkIndex = chunkIndex++;
                entity.Remark = remark;
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
                entity.TimeStamp = timeStamp == 0 ?
                    DateTime.UtcNow :
                    Mapper.Map<long, DateTime>(timeStamp);
                entity.BodyHash = hash;
                entity.CreateTime = DateTime.UtcNow;
                yield return entity;
            } while (bodyBytesStart < bodyBytes.Length);
        }

        public async Task<long> Delete(Expression<Func<BlobEntity, bool>> expression)
        {
            IList<BlobEntity> entities = null;
            using (var transaction = await _repository.BeginTransactionAsync())
            {
                entities = await _repository.QueryAsync(q =>
                {
                    return q.Where(expression).Select(x => new BlobEntity()
                    {
                        Id = x.Id,
                        BlobId = x.BlobId,
                        ChunkIndex = x.ChunkIndex
                    }).ToListAsyncTestable();
                });
                if (entities.Count > 0)
                {
                    _repository.RemoveRange(entities);
                    await _repository.SaveChangesAsync();
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
            using (var transaction = await _repository.BeginTransactionAsync())
            {
                entities = await _repository.QueryNoTrackingAsync(q =>
                    q.Where(expression).OrderBy(x => x.ChunkIndex).ToListAsyncTestable());
            }
            return MergeChunks(entities);
        }

        public Task<BlobOutputDto> Get(Guid key)
        {
            return Get(x => x.BlobId == key);
        }

        public async Task<IList<BlobOutputDto>> GetAll(Expression<Func<BlobEntity, bool>> expression)
        {
            // 不获取body防止数据量过大
            IList<BlobEntity> entities;
            using (var transaction = await _repository.BeginTransactionAsync())
            {
                entities = await _repository.QueryNoTrackingAsync(q =>
                {
                    if (expression != null)
                        q = q.Where(expression);
                    return q.Select(x => new BlobEntity()
                    {
                        Id = x.Id,
                        BlobId = x.BlobId,
                        TimeStamp = x.TimeStamp,
                        Remark = x.Remark,
                    }).GroupBy(x => x.BlobId).Select(x => x.First()).ToListAsyncTestable();
                });
            }
            var dtos = new List<BlobOutputDto>(32);
            foreach (var entity in entities)
            {
                dtos.Add(new BlobOutputDto()
                {
                    Id = entity.BlobId,
                    TimeStamp = Mapper.Map<DateTime, long>(entity.TimeStamp),
                    Remark = entity.Remark,
                });
            }
            return dtos;
        }

        public Task<long> Patch(Expression<Func<BlobEntity, bool>> expression, BlobInputDto dto)
        {
            throw new NotSupportedException("modify an exist blob without changing it's blob id is dangerous and unsupported");
        }

        public Task<long> Patch(Guid key, BlobInputDto dto)
        {
            return Patch(x => x.BlobId == key, dto);
        }

        public async Task<Guid> Put(BlobInputDto dto)
        {
            // 获取提交过来的内容, 计算校验值
            var originalBytes = Mapper.Map<string, byte[]>(dto.Body);
            if (dto.IsCompressed)
                originalBytes = ArchiveUtils.DecompressFromGZip(originalBytes);
            var hash = HashUtils.GetSHA256Hash(originalBytes);
            using (var transaction = await _repository.BeginTransactionAsync())
            {
                // 如果有相同校验值的blob, 返回该blob
                var existBlobId = await _repository.QueryNoTrackingAsync(q =>
                    q.Where(x => x.BodyHash == hash)
                        .Select(x => x.BlobId)
                        .FirstOrDefaultAsyncTestable());
                if (existBlobId != Guid.Empty)
                    return existBlobId;
            }
            // 对内容进行分块
            var blobId = PrimaryKeyUtils.Generate<Guid>();
            var chunks = SplitChunks(blobId, originalBytes, hash, dto.Remark, dto.TimeStamp);
            // 添加新的blob
            // 注意并发添加时可能会添加相同内容的blob
            using (var transaction = await _repository.BeginTransactionAsync())
            {
                foreach (var chunk in chunks)
                {
                    // 这里需要逐个分块提交, 否则会触发mysql的max_allowed_packet错误
                    await _repository.AddAsync(chunk);
                    await _repository.SaveChangesAsync();
                }
                transaction.Commit();
            }
            return blobId;
        }
    }
}

using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 文件
    /// 文件会分块储存, 最多10MB一块
    /// </summary>
    public class BlobEntity :
        IEntity<Guid>,
        IEntityWithCreateTime,
        IEntityWithUpdateTime
    {
        /// <summary>
        /// 文件分块大小, 10MB
        /// </summary>
        public const int BlobChunkSize = 10 * 1024 * 1024;
        /// <summary>
        /// 文件分块Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 文件Id
        /// 有多个分块时多个分块的文件Id都一样
        /// </summary>
        public Guid BlobId { get; set; }
        /// <summary>
        /// 文件分块序号
        /// 从0开始
        /// </summary>
        public int ChunkIndex { get; set; }
        /// <summary>
        /// 文件名
        /// 各个分块的文件名应该一致
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件内容
        /// 最大不超过10MB
        /// </summary>
        public byte[] Body { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}

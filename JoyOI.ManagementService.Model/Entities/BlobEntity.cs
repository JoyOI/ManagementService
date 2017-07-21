using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 文件
    /// 文件会分块储存
    /// </summary>
    public class BlobEntity :
        IEntity<Guid>,
        IEntityWithCreateTime
    {
        /// <summary>
        /// 文件分块大小
        /// 修改后需要测试是否会引发 "Got a packet bigger than 'max_allowed_packet' bytes" 错误
        /// </summary>
        public const int BlobChunkSize = 3 * 1024 * 1024;
        // public const int BlobChunkSize = 20;

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
        /// 文件内容
        /// </summary>
        public byte[] Body { get; set; }
        /// <summary>
        /// 文件时间戳
        /// 由外部传入
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// 文件内容的校验值 (SHA256)
        /// </summary>
        public string BodyHash { get; set; }
        /// <summary>
        /// 备注
        /// 第一次上传时可以设置备注, 后续如果重复使用了这个blob则备注不会更新
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}

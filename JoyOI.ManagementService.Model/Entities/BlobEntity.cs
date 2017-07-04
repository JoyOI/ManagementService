using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities
{
    /// <summary>
    /// 文件
    /// </summary>
    public class BlobEntity : IEntity<Guid>
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件内容
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

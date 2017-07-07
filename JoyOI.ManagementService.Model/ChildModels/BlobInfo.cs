using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 输入或输出的文件信息
    /// </summary>
    public struct BlobInfo
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 输入或输出的文件名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件被使用或产生时的State，可空
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Actor的ID
        /// </summary>
        public Guid ActorId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 输入或输出的文件信息
    /// </summary>
    public class BlobInfo
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 输入或输出的文件名称
        /// </summary>
        public string Name { get; set; }
    }
}

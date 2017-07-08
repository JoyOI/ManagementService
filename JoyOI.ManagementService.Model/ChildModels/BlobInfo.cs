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
        /// <summary>
        /// 附加信息, 可以是空值
        /// </summary>
        public string Tag { get; set; }

        public BlobInfo()
        {

        }

        public BlobInfo(Guid id, string name)
            : this(id, name, null)
        {

        }

        public BlobInfo(Guid id, string name, string tag)
        {
            Id = id;
            Name = name;
            Tag = tag;
        }
    }
}

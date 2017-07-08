using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 运行任务使用的参数
    /// </summary>
    public class RunActorParam
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 输入文件列表
        /// </summary>
        public IEnumerable<BlobInfo> Inputs { get; set; }
        /// <summary>
        /// 附加信息, 可以是空值
        /// </summary>
        public string Tag { get; set; }

        public RunActorParam()
        {

        }

        public RunActorParam(string name)
            : this(name, new BlobInfo[0])
        {

        }

        public RunActorParam(string name, params BlobInfo[] inputs)
            : this(name, inputs.AsEnumerable())
        {

        }

        public RunActorParam(string name, IEnumerable<BlobInfo> inputs)
            : this(name, inputs, null)
        {
        }

        public RunActorParam(string name, IEnumerable<BlobInfo> inputs, string tag)
        {
            Name = name;
            Inputs = inputs;
            Tag = tag;
        }
    }
}

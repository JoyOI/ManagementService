using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 运行任务的结果
    /// 容器中的任务执行后会写一个json文件, 保存跟这个类格式相同的内容
    /// </summary>
    public class RunActorResult
    {
        /// <summary>
        /// 输出的文件列表
        /// </summary>
        public IList<string> Outputs { get; set; }
    }
}

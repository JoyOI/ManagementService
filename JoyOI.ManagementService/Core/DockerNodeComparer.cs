using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// DockerNode的比较器
    /// 注意节点在集合中时不要修改RunningJobs, 必须先从集合中删除再修改
    /// </summary>
    internal class DockerNodeComparer : IComparer<DockerNode>
    {
        public int Compare(DockerNode x, DockerNode y)
        {
            return x.RunningJobs - y.RunningJobs;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// DockerNode的比较器
    /// </summary>
    internal class DockerNodeComparer : IComparer<DockerNode>
    {
        public int Compare(DockerNode x, DockerNode y)
        {
            return x.RunningJobs - y.RunningJobs;
        }
    }
}

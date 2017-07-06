using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// 容器的限制参数
    /// https://docker-py.readthedocs.io/en/stable/containers.html
    /// https://blog.docker.com/2017/01/cpu-management-docker-1-13/
    /// https://docs.docker.com/engine/admin/resource_constraints/
    /// https://stackoverflow.com/questions/24391660/limit-disk-size-and-bandwidth-of-a-docker-container
    /// https://github.com/snitm/docker/blob/master/daemon/graphdriver/devmapper/README.md
    /// </summary>
    public class ContainerLimitation
    {
        /// <summary>
        /// 限制CPU时使用的间隔时间
        /// 单位是微秒, 默认是1秒 = 1000000
        /// </summary>
        public int? CPUPeriod { get; set; }
        /// <summary>
        /// 限制CPU在间隔时间内可以使用的时间
        /// 单位是微秒, 设置为跟CPUPeriod一致时表示只能用一个核心
        /// </summary>
        public int? CPUQuota { get; set; }
        /// <summary>
        /// 可以使用的内存
        /// 单位是字节, 默认无限制
        /// </summary>
        public long? Memory { get; set; }
        /// <summary>
        /// 可以使用的交换内存
        /// 单位是字节, 默认是Memory的两倍
        /// 设为0时等于默认值(Memory的两倍)
        /// </summary>
        public long? MemorySwap { get; set; }
        /// <summary>
        /// 储存大小的限制
        /// 单位是GB, 默认是10GB
        /// </summary>
        public int? StorageBaseSize { get; set; }

        /// <summary>
        /// 判断是否所有值都是默认值
        /// </summary>
        /// <returns></returns>
        public bool IsAllDefault()
        {
            return CPUPeriod == null &&
                CPUQuota == null &&
                Memory == null &&
                MemorySwap == null &&
                StorageBaseSize == null;
        }
    }
}

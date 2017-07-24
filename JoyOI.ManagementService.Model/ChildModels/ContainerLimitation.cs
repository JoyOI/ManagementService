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
        /// 一秒最多读取的字节数
        /// 单位是字节, 默认无限制
        /// </summary>
        public int? BlkioDeviceReadBps { get; set; }
        /// <summary>
        /// 一秒最多写入的字节数
        /// 单位是字节, 默认无限制
        /// </summary>
        public int? BlkioDeviceWriteBps { get; set; }
        /// <summary>
        /// 容器最长可以执行的时间
        /// 单位是毫秒, 默认无限制
        /// </summary>
        public int? ExecutionTimeout { get; set; }
        /// <summary>
        /// 是否启用网络, 默认不启用
        /// </summary>
        public bool? EnableNetwork { get; set; }
        /// <summary>
        /// Ulimit限制
        /// 例如:
        /// memlock, core, nofile, cpu, nproc, locks, sigpending, msgqueue, nice, rtprio
        /// </summary>
        public IDictionary<string, long> Ulimit { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public ContainerLimitation()
        {
            Ulimit = new Dictionary<string, long>();
        }

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
                BlkioDeviceReadBps == null &&
                BlkioDeviceWriteBps == null &&
                ExecutionTimeout == null &&
                EnableNetwork == null &&
                Ulimit.Count == 0;
        }

        /// <summary>
        /// 创建一个新的限制参数, 把当前限制参数中未设置的值替换到默认值
        /// </summary>
        public ContainerLimitation WithDefaults(ContainerLimitation limitation)
        {
            var inst = this;
            if (inst == Default)
                inst = new ContainerLimitation();
            inst.CPUPeriod = inst.CPUPeriod ?? limitation?.CPUPeriod;
            inst.CPUQuota = inst.CPUQuota ?? limitation?.CPUQuota;
            inst.Memory = inst.Memory ?? limitation?.Memory;
            inst.MemorySwap = inst.MemorySwap ?? limitation?.MemorySwap;
            inst.BlkioDeviceReadBps = inst.BlkioDeviceReadBps ?? limitation?.BlkioDeviceReadBps;
            inst.BlkioDeviceWriteBps = inst.BlkioDeviceWriteBps ?? limitation?.BlkioDeviceWriteBps;
            inst.ExecutionTimeout = inst.ExecutionTimeout ?? limitation?.ExecutionTimeout;
            inst.EnableNetwork = inst.EnableNetwork ?? limitation?.EnableNetwork;
            if (limitation != null)
            {
                foreach (var ulimit in limitation.Ulimit)
                {
                    inst.Ulimit[ulimit.Key] = ulimit.Value;
                }
            }
            return inst;
        }

        /// <summary>
        /// 默认的限制参数
        /// </summary>
        public static ContainerLimitation Default = new ContainerLimitation();
    }
}

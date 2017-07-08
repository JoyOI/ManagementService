using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using static JoyOI.ManagementService.Configuration.JoyOIManagementConfiguration;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// Docker主机设置的工具函数
    /// </summary>
    public static class HostConfigUtils
    {
        /// <summary>
        /// 根据限制参数修改主机设置
        /// </summary>
        public static HostConfig WithLimitation(
            HostConfig hostConfig, ContainerLimitation limitation, ContainerConfiguration containerConfiguration)
        {
            if (limitation.CPUPeriod.HasValue)
                hostConfig.CPUPeriod = limitation.CPUPeriod.Value;
            if (limitation.CPUQuota.HasValue)
                hostConfig.CPUQuota = limitation.CPUQuota.Value;
            if (limitation.Memory.HasValue)
                hostConfig.Memory = limitation.Memory.Value;
            if (limitation.MemorySwap.HasValue)
                hostConfig.MemorySwap = limitation.MemorySwap.Value;
            if (limitation.BlkioDeviceReadBps.HasValue)
            {
                hostConfig.BlkioDeviceReadBps = hostConfig.BlkioDeviceReadBps ?? new List<ThrottleDevice>();
                hostConfig.BlkioDeviceReadBps.Add(new ThrottleDevice()
                {
                    Path = containerConfiguration.DevicePath,
                    Rate = (ulong)limitation.BlkioDeviceReadBps.Value
                });
            }
            if (limitation.BlkioDeviceWriteBps.HasValue)
            {
                hostConfig.BlkioDeviceWriteIOps = hostConfig.BlkioDeviceWriteIOps ?? new List<ThrottleDevice>();
                hostConfig.BlkioDeviceWriteIOps.Add(new ThrottleDevice()
                {
                    Path = containerConfiguration.DevicePath,
                    Rate = (ulong)limitation.BlkioDeviceWriteBps.Value
                });
            }
            return hostConfig;
        }
    }
}

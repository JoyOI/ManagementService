using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static HostConfig WithLimitation(HostConfig hostConfig, ContainerLimitation limitation)
        {
            if (limitation.CPUPeriod.HasValue)
                hostConfig.CPUPeriod = limitation.CPUPeriod.Value;
            if (limitation.CPUQuota.HasValue)
                hostConfig.CPUQuota = limitation.CPUQuota.Value;
            if (limitation.Memory.HasValue)
                hostConfig.Memory = limitation.Memory.Value;
            if (limitation.MemorySwap.HasValue)
                hostConfig.MemorySwap = limitation.MemorySwap.Value;
            if (limitation.StorageBaseSize.HasValue)
            {
                hostConfig.StorageOpt = hostConfig.StorageOpt ?? new Dictionary<string, string>();
                hostConfig.StorageOpt["dm.basesize"] = limitation.StorageBaseSize.Value + "G";
            }
            return hostConfig;
        }
    }
}

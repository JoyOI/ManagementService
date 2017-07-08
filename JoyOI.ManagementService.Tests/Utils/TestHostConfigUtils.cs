using Docker.DotNet.Models;
using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static JoyOI.ManagementService.Configuration.JoyOIManagementConfiguration;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestHostConfigUtils
    {
        [Fact]
        public void WithLimitation()
        {
            var hostConfig = new HostConfig();
            var containerConfiguration = new ContainerConfiguration() { DevicePath = "dp" };
            hostConfig = HostConfigUtils.WithLimitation(hostConfig, new ContainerLimitation()
            {
                CPUPeriod = 123,
                CPUQuota = 321,
                Memory = 888
            }, containerConfiguration);
            Assert.Equal(123, hostConfig.CPUPeriod);
            Assert.Equal(321, hostConfig.CPUQuota);
            Assert.Equal(888, hostConfig.Memory);
            hostConfig = HostConfigUtils.WithLimitation(hostConfig, new ContainerLimitation()
            {
                MemorySwap = 168,
                BlkioDeviceReadBps = 169,
                BlkioDeviceWriteBps = 170
            }, containerConfiguration);
            Assert.Equal(123, hostConfig.CPUPeriod);
            Assert.Equal(321, hostConfig.CPUQuota);
            Assert.Equal(888, hostConfig.Memory);
            Assert.Equal(168, hostConfig.MemorySwap);
            Assert.Equal(1, hostConfig.BlkioDeviceReadBps.Count);
            Assert.Equal("dp", hostConfig.BlkioDeviceReadBps[0].Path);
            Assert.Equal(169ul, hostConfig.BlkioDeviceReadBps[0].Rate);
            Assert.Equal(1, hostConfig.BlkioDeviceWriteBps.Count);
            Assert.Equal("dp", hostConfig.BlkioDeviceWriteBps[0].Path);
            Assert.Equal(170ul, hostConfig.BlkioDeviceWriteBps[0].Rate);
        }
    }
}

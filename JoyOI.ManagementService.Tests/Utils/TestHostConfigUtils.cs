using Docker.DotNet.Models;
using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestHostConfigUtils
    {
        [Fact]
        public void WithLimitation()
        {
            var hostConfig = new HostConfig();
            hostConfig = HostConfigUtils.WithLimitation(hostConfig, new ContainerLimitation()
            {
                CPUPeriod = 123,
                CPUQuota = 321,
                Memory = 888
            });
            Assert.Equal(123, hostConfig.CPUPeriod);
            Assert.Equal(321, hostConfig.CPUQuota);
            Assert.Equal(888, hostConfig.Memory);
            hostConfig = HostConfigUtils.WithLimitation(hostConfig, new ContainerLimitation()
            {
                MemorySwap = 168,
                StorageBaseSize = 8
            });
            Assert.Equal(123, hostConfig.CPUPeriod);
            Assert.Equal(321, hostConfig.CPUQuota);
            Assert.Equal(888, hostConfig.Memory);
            Assert.Equal(168, hostConfig.MemorySwap);
            Assert.Equal("8G", hostConfig.StorageOpt["dm.basesize"]);
        }
    }
}

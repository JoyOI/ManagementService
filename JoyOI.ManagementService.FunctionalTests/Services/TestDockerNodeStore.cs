using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Services.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public class TestDockerNodeStore
    {
        [Fact]
        public void GetNode()
        {
            var store = new DockerNodeStore(new JoyOIManagementConfiguration()
            {

            });

            // TODO
        }

        [Fact]
        public void GetNodes()
        {
            // TOOD
        }

        [Fact]
        public void AcquireAndReleaseNode()
        {
            // TODO
        }
    }
}

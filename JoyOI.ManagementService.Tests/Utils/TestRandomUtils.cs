using JoyOI.ManagementService.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestRandomUtils
    {
        [Fact]
        public void GetRandomInstance()
        {
            Assert.True(RandomUtils.GetRandomInstance() != null);
            Assert.Equal(RandomUtils.GetRandomInstance(), RandomUtils.GetRandomInstance());
        }
    }
}

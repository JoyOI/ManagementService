using JoyOI.ManagementService.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestPrimaryKeyUtils
    {
        [Fact]
        public void Generate()
        {
            Assert.Equal(0, PrimaryKeyUtils.Generate<int>());
            Assert.Equal(0, PrimaryKeyUtils.Generate<long>());
            Assert.True(PrimaryKeyUtils.Generate<Guid>() != Guid.Empty);
            Assert.True(PrimaryKeyUtils.Generate<Guid>() != PrimaryKeyUtils.Generate<Guid>());
        }
    }
}

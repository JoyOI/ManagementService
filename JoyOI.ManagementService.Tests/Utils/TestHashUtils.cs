using JoyOI.ManagementService.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestHashUtils
    {
        [Fact]
        public void GetSHA256Hash()
        {
            Assert.Equal(
                "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                HashUtils.GetSHA256Hash(Encoding.UTF8.GetBytes("123")));
            Assert.Equal(
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                HashUtils.GetSHA256Hash(Encoding.UTF8.GetBytes("abc")));
        }
    }
}

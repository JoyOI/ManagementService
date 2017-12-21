using JoyOI.ManagementService.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestLRUCache
    {
        [Fact]
        public void StringKey()
        {
            var cache = new LRUCache<string, int>(3);
            cache.Set("1", 1);
            cache.Set("2", 2);
            cache.Set("3", 3);
            Assert.Equal(1, cache.Get("1"));
            Assert.Equal(2, cache.Get("2"));
            Assert.Equal(3, cache.Get("3"));
            cache.Set("100", 100);
            Assert.Equal(100, cache.Get("100"));
            Assert.Equal(0, cache.Get("1"));
            Assert.Equal(2, cache.Get("2"));
            Assert.Equal(3, cache.Get("3"));
            cache.Set("100", 101);
            Assert.Equal(101, cache.Get("100"));
        }

        [Fact]
        public void BytesKey()
        {
            var cache = new LRUCache<byte[], int>(3);
            cache.Set(new byte[] { 1 }, 1);
            cache.Set(new byte[] { 1, 2 }, 2);
            cache.Set(new byte[] { 2, 1 }, 3);
            Assert.Equal(1, cache.Get(new byte[] { 1 }));
            Assert.Equal(2, cache.Get(new byte[] { 1, 2 }));
            Assert.Equal(3, cache.Get(new byte[] { 2, 1 }));
            cache.Set(new byte[] { 2, 3 }, 100);
            Assert.Equal(100, cache.Get(new byte[] { 2, 3 }));
            Assert.Equal(0, cache.Get(new byte[] { 1 }));
            Assert.Equal(2, cache.Get(new byte[] { 1, 2 }));
            Assert.Equal(3, cache.Get(new byte[] { 2, 1 }));
            cache.Set(new byte[] { 2, 3 }, 101);
            Assert.Equal(101, cache.Get(new byte[] { 2, 3 }));
        }
    }
}

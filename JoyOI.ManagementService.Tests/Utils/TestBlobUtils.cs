using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestBlobUtils
    {
        [Fact]
        public void MergeChunksBody()
        {
            var entities = new List<BlobEntity>()
            {
                new BlobEntity() { Body = new byte[] { 1, 2, 3 }},
                new BlobEntity() { Body = new byte[] { 3, 2, 1 }},
                new BlobEntity() { Body = new byte[] { 8, 8, 8 }},
            };
            Assert.Equal(
                JsonConvert.SerializeObject(new byte[] { 1, 2, 3, 3, 2, 1, 8, 8, 8 }),
                JsonConvert.SerializeObject(BlobUtils.MergeChunksBody(entities)));
        }
    }
}

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
                new BlobEntity() { Body = new byte[] { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00 }},
                new BlobEntity() { Body = new byte[] { 0x00, 0x00, 0x0b, 0x62, 0x64, 0x62, 0x06 }},
                new BlobEntity() { Body = new byte[] { 0x00, 0x00, 0x00, 0xff, 0xff }},
            };
            Assert.Equal(
                JsonConvert.SerializeObject(new byte[] { 1, 2, 3 }),
                JsonConvert.SerializeObject(BlobUtils.MergeChunksBody(entities)));
        }
    }
}

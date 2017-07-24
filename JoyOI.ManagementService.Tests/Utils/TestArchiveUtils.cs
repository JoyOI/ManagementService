using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace JoyOI.ManagementService.Tests.Utils
{
    public class TestArchiveUtils
    {
        [Fact]
        public void Tar()
        {
            var compressed = ArchiveUtils.CompressToTar(new[]
            {
                ("123.txt", new byte[] { 1, 2, 3 }),
                ("/321.txt", new byte[] { 3, 2, 1 }),
            });
            var decompressed = ArchiveUtils.DecompressFromTar(compressed).ToDictionary(x => x.Item1);
            Assert.Equal(2, decompressed.Count);
            Assert.Equal(
                JsonConvert.SerializeObject(new byte[] { 1, 2, 3 }),
                JsonConvert.SerializeObject(decompressed["123.txt"].Item2));
            Assert.Equal(
                JsonConvert.SerializeObject(new byte[] { 3, 2, 1 }),
                JsonConvert.SerializeObject(decompressed["321.txt"].Item2));
        }

        [Fact]
        public void GZip()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var compressed = ArchiveUtils.CompressToGZip(bytes);
            var decompressed = ArchiveUtils.DecompressFromGZip(compressed);
            Assert.True(
                compressed.Length != 0,
                JsonConvert.SerializeObject(compressed));
            Assert.Equal(
                JsonConvert.SerializeObject(bytes),
                JsonConvert.SerializeObject(decompressed));
        }
    }
}

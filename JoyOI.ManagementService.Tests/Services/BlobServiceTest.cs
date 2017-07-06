using AutoMapper;
using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.Services.Impl;
using JoyOI.ManagementService.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JoyOI.ManagementService.Tests.Services
{
    public class BlobServiceTest : ServiceTestBase
    {
        private IBlobService _service;

        public BlobServiceTest()
        {
            _service = new BlobService(_context);
        }

        private BlobInputDto GetSmallBlob()
        {
            var dto = new BlobInputDto();
            dto.Name = "small blob";
            dto.TimeStamp = Mapper.Map<DateTime, long>(DateTime.UtcNow);
            var body = new byte[BlobEntity.BlobChunkSize];
            RandomUtils.GetRandomInstance().NextBytes(body);
            dto.Body = Mapper.Map<byte[], string>(body);
            return dto;
        }

        private BlobInputDto GetLargeBlob()
        {
            var dto = new BlobInputDto();
            dto.Name = "large blob";
            dto.TimeStamp = Mapper.Map<DateTime, long>(DateTime.UtcNow);
            var body = new byte[BlobEntity.BlobChunkSize * 2 + 100];
            RandomUtils.GetRandomInstance().NextBytes(body);
            dto.Body = Mapper.Map<byte[], string>(body);
            return dto;
        }

        [Fact]
        public async Task GetAll()
        {
            var smallBlob = GetSmallBlob();
            var largeBlob = GetLargeBlob();
            var smallId = await _service.Put(smallBlob);
            var largeId = await _service.Put(largeBlob);
            var all = await _service.GetAll(null);
            Assert.Equal(2, all.Count);
            Assert.True(all.Any(x =>
                x.Name == smallBlob.Name &&
                x.Body == smallBlob.Body &&
                x.TimeStamp == smallBlob.TimeStamp));
            Assert.True(all.Any(x =>
                x.Name == largeBlob.Name &&
                x.Body == largeBlob.Body &&
                x.TimeStamp == largeBlob.TimeStamp));
        }

        [Fact]
        public async Task Get_SmallBlob()
        {
            var smallBlob = GetSmallBlob();
            var smallId = await _service.Put(smallBlob);

            var smallBlobGet = await _service.Get(smallId);
            Assert.True(smallBlobGet != null);
            Assert.Equal(smallBlob.Name, smallBlobGet.Name);
            Assert.Equal(smallBlob.Body, smallBlobGet.Body);
            Assert.Equal(smallBlob.TimeStamp, smallBlobGet.TimeStamp);

            var notExistBlob = await _service.Get(Guid.NewGuid());
            Assert.True(notExistBlob == null);
        }

        [Fact]
        public async Task Get_LargeBlob()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);

            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet != null);
            Assert.Equal(largeBlob.Name, largeBlobGet.Name);
            Assert.Equal(largeBlob.Body, largeBlobGet.Body);
            Assert.Equal(largeBlob.TimeStamp, largeBlobGet.TimeStamp);
        }

        [Fact]
        public async Task Put_SmallBlob()
        {
            var smallBlob = GetSmallBlob();
            var smallId = await _service.Put(smallBlob);
            var smallChunks = _context.Set<BlobEntity>().Count(x => x.BlobId == smallId);
            Assert.Equal(1, smallChunks);

            var smallBlobGet = await _service.Get(smallId);
            Assert.True(smallBlobGet != null);
            Assert.Equal(smallBlob.Name, smallBlobGet.Name);
            Assert.Equal(smallBlob.Body, smallBlobGet.Body);
            Assert.Equal(smallBlob.TimeStamp, smallBlobGet.TimeStamp);
        }

        [Fact]
        public async Task Put_LargeBlob()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);
            var largeChunks = _context.Set<BlobEntity>().Count(x => x.BlobId == largeId);
            Assert.Equal(3, largeChunks);

            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet != null);
            Assert.Equal(largeBlob.Name, largeBlobGet.Name);
            Assert.Equal(largeBlob.Body, largeBlobGet.Body);
            Assert.Equal(largeBlob.TimeStamp, largeBlobGet.TimeStamp);
        }

        [Fact]
        public async Task Patch_Name()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);

            var largePatch = _service.Patch(largeId, new BlobInputDto() { Name = "new large name" });
            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet != null);
            Assert.Equal("new large name", largeBlobGet.Name);
            Assert.Equal(largeBlob.Body, largeBlobGet.Body);
            Assert.Equal(largeBlob.TimeStamp, largeBlobGet.TimeStamp);
        }

        [Fact]
        public async Task Patch_Body()
        {
            var largeBlob = GetLargeBlob();
            var newLargeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);

            var largePatch = _service.Patch(largeId, new BlobInputDto()
            {
                Body = newLargeBlob.Body,
                TimeStamp = newLargeBlob.TimeStamp
            });
            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet != null);
            Assert.Equal(largeBlob.Name, largeBlobGet.Name);
            Assert.Equal(newLargeBlob.Body, largeBlobGet.Body);
            Assert.Equal(newLargeBlob.TimeStamp, largeBlobGet.TimeStamp);
        }

        [Fact]
        public async Task Delete_SmallBlob()
        {
            var smallBlob = GetSmallBlob();
            var smallId = await _service.Put(smallBlob);
            var smallDelete = await _service.Delete(smallId);
            Assert.Equal(1, smallDelete);

            var smallBlobGet = await _service.Get(smallId);
            Assert.True(smallBlobGet == null);

            var noDelete = await _service.Delete(Guid.NewGuid());
            Assert.Equal(0, noDelete);
        }

        [Fact]
        public async Task Delete_LargeBlob()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);
            var largeDelete = await _service.Delete(largeId);
            Assert.Equal(1, largeDelete);

            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet == null);
        }
    }
}

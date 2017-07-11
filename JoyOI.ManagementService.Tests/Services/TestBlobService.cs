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
    public class TestBlobService : ServiceTestBase
    {
        private IBlobService _service;

        public TestBlobService()
        {
            _service = new BlobService(_context);
        }

        private BlobInputDto GetSmallBlob()
        {
            var dto = new BlobInputDto();
            dto.TimeStamp = Mapper.Map<DateTime, long>(DateTime.UtcNow);
            dto.Remark = "small blob";
            var body = new byte[BlobEntity.BlobChunkSize];
            RandomUtils.GetRandomInstance().NextBytes(body);
            dto.Body = Mapper.Map<byte[], string>(body);
            return dto;
        }

        private BlobInputDto GetLargeBlob()
        {
            var dto = new BlobInputDto();
            dto.TimeStamp = Mapper.Map<DateTime, long>(DateTime.UtcNow);
            dto.Remark = "large blob";
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
                x.Body == smallBlob.Body &&
                x.TimeStamp == smallBlob.TimeStamp &&
                x.Remark == smallBlob.Remark));
            Assert.True(all.Any(x =>
                x.Body == largeBlob.Body &&
                x.TimeStamp == largeBlob.TimeStamp &&
                x.Remark == largeBlob.Remark));
        }

        [Fact]
        public async Task Get_SmallBlob()
        {
            var smallBlob = GetSmallBlob();
            var smallId = await _service.Put(smallBlob);

            var smallBlobGet = await _service.Get(smallId);
            Assert.True(smallBlobGet != null);
            Assert.Equal(smallBlob.Body, smallBlobGet.Body);
            Assert.Equal(smallBlob.TimeStamp, smallBlobGet.TimeStamp);
            Assert.Equal(smallBlob.Remark, smallBlobGet.Remark);

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
            Assert.Equal(largeBlob.Body, largeBlobGet.Body);
            Assert.Equal(largeBlob.TimeStamp, largeBlobGet.TimeStamp);
            Assert.Equal(largeBlob.Remark, largeBlobGet.Remark);
        }

        [Fact]
        public async Task Put_SmallBlob()
        {
            var smallBlob = GetSmallBlob();
            var smallId = await _service.Put(smallBlob);
            var smallChunks = _context.Blobs.Count(x => x.BlobId == smallId);
            Assert.Equal(1, smallChunks);

            var smallBlobGet = await _service.Get(smallId);
            Assert.True(smallBlobGet != null);
            Assert.Equal(smallBlob.Body, smallBlobGet.Body);
            Assert.Equal(smallBlob.TimeStamp, smallBlobGet.TimeStamp);
            Assert.Equal(smallBlob.Remark, smallBlobGet.Remark);
        }

        [Fact]
        public async Task Put_LargeBlob()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);
            var largeChunks = _context.Blobs.Count(x => x.BlobId == largeId);
            Assert.Equal(3, largeChunks);

            var largeBlobGet = await _service.Get(largeId);
            Assert.True(largeBlobGet != null);
            Assert.Equal(largeBlob.Body, largeBlobGet.Body);
            Assert.Equal(largeBlob.TimeStamp, largeBlobGet.TimeStamp);
            Assert.Equal(largeBlob.Remark, largeBlobGet.Remark);
        }

        [Fact]
        public async Task Put_DuplicateBlob()
        {
            var largeBlob = GetLargeBlob();
            var largeId = await _service.Put(largeBlob);
            var largeIdDup = await _service.Put(largeBlob);
            Assert.Equal(largeId, largeIdDup);
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

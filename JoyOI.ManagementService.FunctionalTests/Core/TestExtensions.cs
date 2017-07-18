using JoyOI.ManagementService.FunctionalTests.Services;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services.Impl;
using JoyOI.ManagementService.Utils;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using JoyOI.ManagementService.Core;

namespace JoyOI.ManagementService.FunctionalTests.Core
{
    public class TestExtensions : TestServiceBase
    {
        private StateMachineInstanceStore _store;

        public TestExtensions()
        {
            _store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration),
                new DynamicCompileService());
            _store.Initialize(
                () => new EmptyDisposable(),
                _ => new DummyRepository<BlobEntity, Guid>(_storage),
                _ => new DummyRepository<ActorEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineInstanceEntity, Guid>(_storage));
        }

        [Fact]
        public void ReadAllBytesAsync()
        {

        }

        [Fact]
        public void ReadAllTextAsync()
        {

        }

        [Fact]
        public void ReadAsJsonAsync()
        {

        }

        [Fact]
        public void FindActor()
        {

        }

        [Fact]
        public void FindSingleActor()
        {

        }

        [Fact]
        public void FindBlob()
        {
            var blobInfoA = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "a.txt");
            var blobInfoB = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "b.txt");
            var blobs = new[] { blobInfoA, blobInfoB };
            Assert.True(blobInfoA == blobs.FindBlob("a.txt"));
            Assert.True(blobInfoB == blobs.FindBlob("b.txt"));
            Assert.True(null == blobs.FindBlob("c.txt"));
        }

        [Fact]
        public void FindInputBlob()
        {
            var blobInfoA = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "a.txt");
            var blobInfoB = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "b.txt");
            var actorInfo = new ActorInfo()
            {
                Inputs = new[]
                {
                    blobInfoA,
                    blobInfoB
                }
            };
            Assert.True(blobInfoA == actorInfo.FindInputBlob("a.txt"));
            Assert.True(blobInfoB == actorInfo.FindInputBlob("b.txt"));
            Assert.True(null == actorInfo.FindInputBlob("c.txt"));
        }

        [Fact]
        public void FindOutputBlob()
        {
            var blobInfoA = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "a.txt");
            var blobInfoB = new BlobInfo(PrimaryKeyUtils.Generate<Guid>(), "b.txt");
            var actorInfo = new ActorInfo()
            {
                Outputs = new []
                {
                    blobInfoA,
                    blobInfoB
                }
            };
            Assert.True(blobInfoA == actorInfo.FindOutputBlob("a.txt"));
            Assert.True(blobInfoB == actorInfo.FindOutputBlob("b.txt"));
            Assert.True(null == actorInfo.FindOutputBlob("c.txt"));
        }
    }
}

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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JoyOI.ManagementService.FunctionalTests.Core
{
    public class TestCoreExtensions : TestServiceBase
    {
        private StateMachineInstanceStore _store;

        public TestCoreExtensions()
        {
            _store = new StateMachineInstanceStore(
                _configuration,
                new DockerNodeStore(_configuration, new NotificationService()),
                new DynamicCompileService());
            _store.Initialize(
                () => new EmptyDisposable(),
                _ => new DummyRepository<BlobEntity, Guid>(_storage),
                _ => new DummyRepository<ActorEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineEntity, Guid>(_storage),
                _ => new DummyRepository<StateMachineInstanceEntity, Guid>(_storage));
        }

        private class DummyStateMachine : StateMachineBase
        {
            public override Task RunAsync()
            {
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task ReadAllBytesAsync()
        {
            var blobId = await PutTestBlob("a.txt", new byte[] { 1, 2, 3 });
            var blobInfo = new BlobInfo(blobId, "a.txt");
            var stm = new DummyStateMachine();
            stm.Store = _store;
            var result = await blobInfo.ReadAllBytesAsync(stm);
            Assert.Equal(
                JsonConvert.SerializeObject(new byte[] { 1, 2, 3 }),
                JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task ReadAllTextAsync()
        {
            var blobId = await PutTestBlob("a.txt", Encoding.UTF8.GetBytes("some text in a.txt"));
            var blobInfo = new BlobInfo(blobId, "a.txt");
            var stm = new DummyStateMachine();
            stm.Store = _store;
            var result = await blobInfo.ReadAllTextAsync(stm);
            Assert.Equal("some text in a.txt", result);
        }

        [Fact]
        public async Task ReadAsJsonAsync()
        {
            var blobId = await PutTestBlob("a.txt",
                Encoding.UTF8.GetBytes("{ \"Name\": \"ActorInBlob\", \"Stage\": \"Start\" }"));
            var blobInfo = new BlobInfo(blobId, "a.txt");
            var stm = new DummyStateMachine();
            stm.Store = _store;
            var result = await blobInfo.ReadAsJsonAsync<ActorInfo>(stm);
            Assert.Equal("ActorInBlob", result.Name);
            Assert.Equal("Start", result.Stage);
        }

        [Fact]
        public void FindActor()
        {
            var actorInfoA = new ActorInfo() { Name = "ActorA", Stage = "FirstStage" };
            var actorInfoB = new ActorInfo() { Name = "ActorB", Stage = "SecondStage" };
            var actors = new[] { actorInfoA, actorInfoB };
            var foundActors = actors.FindActor().ToList();
            Assert.Equal(2, foundActors.Count);
            Assert.Equal("ActorA", foundActors[0].Name);
            Assert.Equal("FirstStage", foundActors[0].Stage);
            Assert.Equal("ActorB", foundActors[1].Name);
            Assert.Equal("SecondStage", foundActors[1].Stage);
            Assert.True(actorInfoA == actors.FindActor("FirstStage").Single());
            Assert.True(actorInfoB == actors.FindActor("SecondStage").Single());
            Assert.True(actorInfoA == actors.FindActor(null, "ActorA").Single());
            Assert.True(actorInfoB == actors.FindActor(null, "ActorB").Single());
        }

        [Fact]
        public void FindSingleActor()
        {
            var actorInfoA = new ActorInfo() { Name = "ActorA", Stage = "FirstStage" };
            var actorInfoB = new ActorInfo() { Name = "ActorB", Stage = "SecondStage" };
            var actors = new[] { actorInfoA, actorInfoB };
            Assert.True(actorInfoA == actors.FindSingleActor("FirstStage"));
            Assert.True(actorInfoB == actors.FindSingleActor("SecondStage"));
            Assert.True(actorInfoA == actors.FindSingleActor(null, "ActorA"));
            Assert.True(actorInfoB == actors.FindSingleActor(null, "ActorB"));
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;

namespace JoyOI.ManagementService.Core
{
    public static class CoreExtensions
    {
        public static async Task<byte[]> ReadAllBytesAsync(this BlobInfo self, StateMachineBase stm)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            var blobs = await stm.Store.ReadBlobs(new[] { self });
            return blobs.First().Item2;
        }

        public static async Task<string> ReadAllTextAsync(this BlobInfo self, StateMachineBase stm)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            var blobs = await stm.Store.ReadBlobs(new[] { self });
            return Encoding.UTF8.GetString(blobs.First().Item2);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this BlobInfo self, StateMachineBase stm)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            return JsonConvert.DeserializeObject<T>(await self.ReadAllTextAsync(stm));
        }

        public static IEnumerable<ActorInfo> FindActor(this IEnumerable<ActorInfo> self, string stage = null, string actor = null)
        {
            if (stage != null)
            {
                self = self.Where(x => x.Stage == stage);
            }

            if (actor != null)
            {
                self = self.Where(x => x.Name == actor);
            }

            return self;
        }

        public static ActorInfo FindSingleActor(this IEnumerable<ActorInfo> self, string stage = null, string actor = null)
        {
            var actorInfo = self.FindActor(stage, actor).SingleOrDefault();
            if (actorInfo == null)
                throw new KeyNotFoundException($"find single actor failed: stage is \"{stage}\", actor is \"{actor}\"");
            return actorInfo;
        }

        public static BlobInfo FindBlob(this IEnumerable<BlobInfo> self, string filename)
        {
            return self.SingleOrDefault(x => x.Name == filename);
        }

        public static BlobInfo FindInputBlob(this ActorInfo self, string filename)
        {
            return self.Inputs.SingleOrDefault(x => x.Name == filename);
        }

        public static BlobInfo FindOutputBlob(this ActorInfo self, string filename)
        {
            return self.Outputs.SingleOrDefault(x => x.Name == filename);
        }

        public static BlobInfo FindSingleBlob(this IEnumerable<BlobInfo> self, string filename)
        {
            var blobInfo = FindBlob(self, filename);
            if (blobInfo == null)
                throw new KeyNotFoundException($"find single blob failed: filename is \"{filename}\"");
            return blobInfo;
        }

        public static BlobInfo FindSingleInputBlob(this ActorInfo self, string filename)
        {
            var blobInfo = self.Inputs.SingleOrDefault(x => x.Name == filename);
            if (blobInfo == null)
                throw new KeyNotFoundException($"find single input blob failed: filename is \"{filename}\"");
            return blobInfo;
        }

        public static BlobInfo FindSingleOutputBlob(this ActorInfo self, string filename)
        {
            var blobInfo = self.Outputs.SingleOrDefault(x => x.Name == filename);
            if (blobInfo == null)
                throw new KeyNotFoundException($"find single output blob failed: filename is \"{filename}\"");
            return blobInfo;
        }
    }
}

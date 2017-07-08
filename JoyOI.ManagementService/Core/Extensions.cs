using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;

namespace JoyOI.ManagementService.Core
{
    public static class Extensions
    {
        public static Task<byte[]> ReadAllBytesAsync(this BlobInfo self)
        {
            throw new NotImplementedException();
        }

        public static Task<string> ReadAllTextAsync(this BlobInfo self)
        {
            throw new NotImplementedException();
        }

        public static async Task<T> ReadAsJsonAsync<T>(this BlobInfo self) => JsonConvert.DeserializeObject<T>(await self.ReadAllTextAsync());

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
            return self.FindActor(stage, actor).Single();
        }

        public static BlobInfo FindBlob(this IEnumerable<BlobInfo> self, string filename) => self.SingleOrDefault(x => x.Name == filename);

        public static BlobInfo FindInputBlob(this ActorInfo self, string filename) => self.Inputs.SingleOrDefault(x => x.Name == filename);

        public static BlobInfo FindOutputBlob(this ActorInfo self, string filename) => self.Outputs.SingleOrDefault(x => x.Name == filename);
    }
}

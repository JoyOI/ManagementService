﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using JoyOI.ManagementService.Services;

namespace JoyOI.ManagementService.Core
{
    public static class CoreExtensions
    {
        public static async Task<byte[]> ReadAllBytesAsync(this BlobInfo self, StateMachineBase stm)
        {
            var blobs = await stm.Store.ReadBlobs(new[] { self });
            return blobs.First().Item2;
        }

        public static async Task<string> ReadAllTextAsync(this BlobInfo self, StateMachineBase stm)
        {
            var blobs = await stm.Store.ReadBlobs(new[] { self });
            return Encoding.UTF8.GetString(blobs.First().Item2);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this BlobInfo self, StateMachineBase stm)
        {
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
            return self.FindActor(stage, actor).Single();
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
    }
}

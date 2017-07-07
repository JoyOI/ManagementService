using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 文件的工具类
    /// </summary>
    public static class BlobUtils
    {
        /// <summary>
        /// 整合文件分块
        /// 注意文件必须已经按ChunkIndex排序, 如果未排序好则会返回错误的结果
        /// </summary>
        /// <param name="blobs">文件分库列表</param>
        /// <returns></returns>
        public static byte[] MergeChunksBody(IList<BlobEntity> blobs)
        {
            if (blobs.Count == 1)
            {
                return blobs[0].Body;
            }
            var bodyBytes = new byte[blobs.Sum(e => e.Body.Length)];
            var bodyBytesStart = 0;
            foreach (var entity in blobs)
            {
                Array.Copy(entity.Body, 0, bodyBytes, bodyBytesStart, entity.Body.Length);
                bodyBytesStart += entity.Body.Length;
            }
            return bodyBytes;
        }
    }
}

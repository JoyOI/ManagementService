using Microsoft.EntityFrameworkCore.Migrations;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 压缩和解压缩的工具函数
    /// </summary>
    public static class ArchiveUtils
    {
        /// <summary>
        /// 压缩文件到tar
        /// </summary>
        public static Stream CompressToTar(IEnumerable<(BlobInfo, byte[])> blobs)
        {
            var tmpStream = new MemoryStream();
            var resultStream = new MemoryStream();
            using (var writer = WriterFactory.Open(tmpStream, ArchiveType.Tar,
                new WriterOptions(CompressionType.None) { LeaveStreamOpen = true }))
            {
                foreach (var (blob, bytes) in blobs)
                {
                    using (var blobStream = new MemoryStream(bytes))
                    {
                        writer.Write(blob.Name, blobStream);
                    }
                }
                // 类库有问题, TarWriter的LeaveStreamOpen不起作用, 解决后可以省掉这个处理
                // https://github.com/adamhathcock/sharpcompress/issues/270
                tmpStream.Seek(0, SeekOrigin.Begin);
                tmpStream.CopyTo(resultStream);
                resultStream.Seek(0, SeekOrigin.Begin);
            }
            return resultStream;
        }

        /// <summary>
        /// 从tar解压缩文件
        /// </summary>
        public static IEnumerable<(string, byte[])> DecompressFromTar(Stream stream)
        {
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    var entry = reader.Entry;
                    if (entry.IsDirectory)
                    {
                        continue;
                    }
                    var filename = entry.Key;
                    using (var fromStream = reader.OpenEntryStream())
                    using (var toStream = new MemoryStream())
                    {
                        fromStream.CopyTo(toStream);
                        yield return (filename, toStream.ToArray());
                    }
                }
            }
        }
    }
}

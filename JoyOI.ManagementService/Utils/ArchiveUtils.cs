using Microsoft.EntityFrameworkCore.Migrations;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        public static Stream CompressToTar(IEnumerable<(string, byte[])> blobs)
        {
            var tmpStream = new MemoryStream();
            var resultStream = new MemoryStream();
            using (var writer = WriterFactory.Open(tmpStream, ArchiveType.Tar,
                new WriterOptions(CompressionType.None) { LeaveStreamOpen = true }))
            {
                foreach (var (path, bytes) in blobs)
                {
                    var uploadPath = path;
                    if (uploadPath.StartsWith("/"))
                        uploadPath = path.Substring(1);
                    using (var blobStream = new MemoryStream(bytes))
                    {
                        writer.Write(path, blobStream);
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
            using (var reader = ReaderFactory.Open(stream,
                new ReaderOptions() { LeaveStreamOpen = false }))
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

        /// <summary>
        /// 使用gzip压缩字节数组
        /// </summary>
        public static byte[] CompressToGZip(byte[] bytes)
        {
            using (var memStream = new MemoryStream())
            using (var stream = new GZipStream(memStream, CompressionLevel.Optimal))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                return memStream.ToArray();
            }
        }

        /// <summary>
        /// 使用gzip解压缩字节数组
        /// </summary>
        public static byte[] DecompressFromGZip(byte[] bytes)
        {
            using (var memStream = new MemoryStream(bytes))
            using (var stream = new GZipStream(memStream, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                stream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }
    }
}

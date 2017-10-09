using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 主键的工具类
    /// </summary>
    public static class PrimaryKeyUtils
    {
        /// <summary>
        /// 自增数值
        /// </summary>
        private static long _count = 0;
        
        /// <summary>
        /// 根据8个字节的缓冲区和指定时间生成序列Guid
        /// UUID Version是1, 时钟序号和MAC地址由缓冲区得到
        /// </summary>
        private static Guid SequentialGuid(DateTime time, byte[] buffer)
        {
            var ticks = (time - new DateTime(1900, 1, 1)).Ticks;
            var guid = new Guid(
                (uint)(ticks >> 32),
                (ushort)(ticks >> 16),
                (ushort)((ticks & 0x3f) | (1 << 12)),
                buffer[0], buffer[1], buffer[2], buffer[3],
                buffer[4], buffer[5], buffer[6], buffer[7]);
            return guid;
        }

        /// <summary>
        /// 根据指定时间生成序列GUID
        /// 它使用了随机值代替时钟序列和MAC地址
        /// </summary>
        private static Guid SequentialGuid(DateTime time)
        {
            // TODO: linux上部分环境会导致生成全0, 暂时改用自增
            // RandomUtils.GetRandomInstance().NextBytes(buffer);
            var count = Interlocked.Increment(ref _count);
            var buffer = BitConverter.GetBytes(count);
            return SequentialGuid(time, buffer);
        }

        /// <summary>
        /// 生成主键的默认值
        /// </summary>
        public static TPrimaryKey Generate<TPrimaryKey>()
        {
            if (typeof(TPrimaryKey) == typeof(Guid))
            {
                return (TPrimaryKey)(object)SequentialGuid(DateTime.UtcNow);
            }
            return default(TPrimaryKey);
        }
    }
}

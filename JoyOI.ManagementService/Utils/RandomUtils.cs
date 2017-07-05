using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 随机的工具类
    /// </summary>
    public static class RandomUtils
    {
        private static Random _instance = new Random();

        /// <summary>
        /// 获取全局使用的随机数生成器实例
        /// </summary>
        public static Random GetRandomInstance()
        {
            return _instance;
        }
    }
}

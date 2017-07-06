using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 计算校验值的工具类
    /// </summary>
    public static class HashUtils
    {
        public static string GetSHA256Hash(byte[] body)
        {
            using (var sha256 = SHA256.Create())
            {
                var result = sha256.ComputeHash(body);
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
        }
    }
}

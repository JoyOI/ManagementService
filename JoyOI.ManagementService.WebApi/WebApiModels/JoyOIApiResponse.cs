using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    /// <summary>
    /// WebApi返回的结果都应该使用这个类包装
    /// </summary>
    public class JoyOIApiResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }

        /// <summary>
        /// 返回成功的结果
        /// </summary>
        public static JoyOIApiResponse Success(object data)
        {
            return new JoyOIApiResponse()
            {
                code = 200,
                msg = null,
                data = data
            };
        }

        /// <summary>
        /// 返回错误的结果
        /// </summary>
        public static JoyOIApiResponse Exception(Exception ex)
        {
            return new JoyOIApiResponse()
            {
                code = 500,
                msg = $"{ex.GetType().Name}: {ex.Message}",
                data = null
            };
        }
    }
}

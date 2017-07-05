using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    /// <summary>
    /// WebApi返回的结果都应该使用这个类包装
    /// </summary>
    public class JoyOIApiResponse<TData>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public TData data { get; set; }
    }

    /// <summary>
    /// 构建JoyOIApiResponse的静态函数
    /// </summary>
    public static class JoyOIApiResponse
    {
        /// <summary>
        /// 返回200(成功)
        /// </summary>
        public static JoyOIApiResponse<TData> OK<TData>(TData data)
        {
            return new JoyOIApiResponse<TData>()
            {
                code = 200,
                msg = null,
                data = data
            };
        }

        /// <summary>
        /// 返回404(找不到)
        /// </summary>
        public static JoyOIApiResponse<TData> NotFound<TData>(string msg)
        {
            return new JoyOIApiResponse<TData>()
            {
                code = 404,
                msg = msg,
                data = default(TData)
            };
        }

        /// <summary>
        /// 返回500(内部错误)
        /// </summary>
        public static JoyOIApiResponse<object> InternalServerError(Exception ex)
        {
            return new JoyOIApiResponse<object>()
            {
                code = 500,
                msg = $"{ex.GetType().Name}: {ex.Message}",
                data = null
            };
        }
    }
}

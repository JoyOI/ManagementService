using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    /// <summary>
    /// WebApi返回的结果都应该使用这个类包装
    /// </summary>
    public class ApiResponse<TData>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public TData data { get; set; }
    }

    /// <summary>
    /// 构建JoyOIApiResponse的静态函数
    /// </summary>
    public static class ApiResponse
    {
        /// <summary>
        /// 返回200(成功)
        /// </summary>
        public static ApiResponse<TData> OK<TData>(TData data)
        {
            return new ApiResponse<TData>()
            {
                code = 200,
                msg = null,
                data = data
            };
        }

        /// <summary>
        /// 返回404(找不到)
        /// </summary>
        public static ApiResponse<TData> NotFound<TData>(string msg, TData data)
        {
            return new ApiResponse<TData>()
            {
                code = 404,
                msg = msg,
                data = data
            };
        }

        /// <summary>
        /// 返回500(内部错误)
        /// </summary>
        public static ApiResponse<object> InternalServerError(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return new ApiResponse<object>()
            {
                code = 500,
                msg = $"{ex.GetType().Name}: {ex.Message}",
                data = null
            };
        }
    }
}

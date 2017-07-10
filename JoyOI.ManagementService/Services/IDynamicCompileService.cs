using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 动态编译代码的服务, 应该为单例
    /// </summary>
    public interface IDynamicCompileService
    {
        /// <summary>
        /// 编译代码到程序集的字节数组
        /// </summary>
        byte[] Compile(string code);
    }
}

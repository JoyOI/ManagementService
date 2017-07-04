using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Enums
{
    /// <summary>
    /// 状态机的当前状态
    /// </summary>
    public enum StateMachineStatus
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 0,
        /// <summary>
        /// 运行失败
        /// </summary>
        Failed = 1,
        /// <summary>
        /// 运行成功
        /// </summary>
        Succeeded = 2
    }
}

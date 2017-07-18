using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 标记状态机实例已中断
    /// 抛出此例外前必须更新好状态机实例的数据实体, 中断后不会再更新
    /// </summary>
    public class StateMachineInterpretedException : Exception
    {
    }
}

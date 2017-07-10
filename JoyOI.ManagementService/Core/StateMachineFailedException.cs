using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 标记状态机已失败, 不继续执行
    /// </summary>
    public class StateMachineFailedException : Exception
    {
    }
}

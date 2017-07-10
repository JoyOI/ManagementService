using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 执行任务时在容器中发生的错误
    /// </summary>
    public class ActorExecuteException : Exception
    {
        public ActorExecuteException(string message) : base(message)
        {
        }
    }
}

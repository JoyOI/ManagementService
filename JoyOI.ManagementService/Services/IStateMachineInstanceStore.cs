using JoyOI.ManagementService.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理状态机实例的仓库, 应该为单例
    /// </summary>
    public interface IStateMachineInstanceStore
    {
        /// <summary>
        /// 初始化仓库
        /// </summary>
        void Initialize(Func<JoyOIManagementContext> contextFactory);
    }
}

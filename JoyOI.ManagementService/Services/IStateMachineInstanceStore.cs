using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理状态机实例的仓库, 应该为单例
    /// </summary>
    internal interface IStateMachineInstanceStore
    {
        /// <summary>
        /// 初始化仓库
        /// </summary>
        void Initialize(Func<JoyOIManagementContext> contextFactory);

        /// <summary>
        /// 编译状态机代码并返回实例
        /// </summary>
        Task<StateMachineBase> CreateInstance(
            StateMachineEntity stateMachineEntity,
            StateMachineInstanceEntity stateMachineInstanceEntity);

        /// <summary>
        /// 运行状态机实例
        /// </summary>
        Task RunInstance(StateMachineBase instance);

        /// <summary>
        /// 运行任务
        /// 流程
        /// - 发布当前actor的代码到docker容器
        /// - 运行actor
        /// - 更新当前actor的Status和Outputs
        /// 当前actor和已完成actor会在下次RunActor或者状态机结束后更新
        /// </summary>
        Task RunActor(StateMachineBase instance);
    }
}

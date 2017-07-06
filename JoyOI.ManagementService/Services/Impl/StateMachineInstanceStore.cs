using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机实例的仓库, 应该为单例
    /// 
    /// 外部启动状态机的流程:
    /// - (可选) 上传一个或多个blob
    /// - 调用CreateInstance(name, blobs)
    ///   - 获取name对应的状态机代码
    ///   - 使用roslyn编译状态机代码
    ///   - 添加状态机实例到数据库
    ///     - 初始的 CurrentActor 是 { Name = null, Inputs = blobs }, 这个actor不会加到finished中
    ///   - 调用RunAsync(null, blobs)
    /// - 状态机实例调用DeployAndRunActorAsync(name, blobs)
    ///   - 调用docker的api创建容器
    ///   - 上传blobs到容器
    ///   - 执行actor中的代码
    ///   - 执行完毕后更新状态机实例的CurrentActor的状态为Succeeded
    ///   - 如果状态机执行完毕
    ///     - 更新状态机实例的状态为Succeeded
    ///     - 否则状态机会继续调用DeployAndRunActorAsync(name, blobs)
    /// 
    /// 启动已中断的状态机的流程:
    /// - 获取数据库中Running的状态机实例
    ///   - 因为有可能配置多个管理服务, 获取时需要传入FromManagementService
    ///   - 如果ReRunTimes >= MaxReRunTimes, 则直接标记为Failed
    /// - 调用docker的api删除CurrentNode中的CurrentContainer
    /// - 调用RunAsync(CurrentActor.Name, CurrentActor.Inputs), 之后同上
    /// </summary>
    public class StateMachineInstanceStore : IStateMachineInstanceStore
    {
        private JoyOIManagementConfiguration _configuration;
        private Func<JoyOIManagementContext> _contextFactory;
        private bool _initilaized;
        private object _initializeLock;

        public StateMachineInstanceStore(JoyOIManagementConfiguration configuration)
        {
            _configuration = configuration;
            _contextFactory = null;
            _initilaized = false;
            _initializeLock = new object();
        }

        public void Initialize(Func<JoyOIManagementContext> contextFactory)
        {
            if (_initilaized)
            {
                return;
            }
            lock (_initializeLock)
            {
                _contextFactory = contextFactory;
                ContinueExecutingInstances();
                _initilaized = true;
            }
        }

        /// <summary>
        /// 继续执行之前未执行完毕的实例
        /// </summary>
        private void ContinueExecutingInstances()
        {

        }
    }
}

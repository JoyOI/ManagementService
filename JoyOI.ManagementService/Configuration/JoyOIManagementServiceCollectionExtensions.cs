using AutoMapper;
using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Model.MapperProfiles;
using JoyOI.ManagementService.Repositories;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 配置管理服务的扩展函数
    /// </summary>
    public static class JoyOIManagementServiceCollectionExtensions
    {
        private static bool StaticFunctionsInitialized = false;
        private static object StaticFunctionsInitializeLock = new object();

        /// <summary>
        /// 初始化静态功能
        /// </summary>
        internal static void InitializeStaticFunctions()
        {
            if (StaticFunctionsInitialized)
            {
                return;
            }
            lock (StaticFunctionsInitializeLock)
            {
                Mapper.Initialize(c =>
                {
                    c.AddProfile<BaseMapperProfile>();
                    c.AddProfile<ActorMapperProfile>();
                    c.AddProfile<BlobMapperProfile>();
                    c.AddProfile<StateMachineMapperProfile>();
                    c.AddProfile<StateMachineInstanceMapperProfile>();
                });
                StaticFunctionsInitialized = true;
            }
        }

        /// <summary>
        /// 初始化管理服务
        /// </summary>
        public static void AddJoyOIManagement(
            this IServiceCollection services, JoyOIManagementConfiguration configuration)
        {
            // 注册配置
            configuration.AfterLoaded();
            services.AddSingleton(configuration);

            // 注册服务
            services.AddTransient<IRepository<ActorEntity, Guid>, EFCoreRepository<ActorEntity, Guid>>();
            services.AddTransient<IRepository<BlobEntity, Guid>, EFCoreRepository<BlobEntity, Guid>>();
            services.AddTransient<IRepository<StateMachineEntity, Guid>, EFCoreRepository<StateMachineEntity, Guid>>();
            services.AddTransient<IRepository<StateMachineInstanceEntity, Guid>, EFCoreRepository<StateMachineInstanceEntity, Guid>>();
            services.AddTransient<IActorService, ActorService>();
            services.AddTransient<IBlobService, BlobService>();
            services.AddTransient<IStateMachineService, StateMachineService>();
            services.AddTransient<IStateMachineInstanceService, StateMachineInstanceService>();
            services.AddSingleton<IStateMachineInstanceStore, StateMachineInstanceStore>();
            services.AddSingleton<IDockerNodeStore, DockerNodeStore>();
            services.AddSingleton<IDynamicCompileService, DynamicCompileService>();

            // 静态功能
            InitializeStaticFunctions();
        }

        /// <summary>
        /// 开始管理服务
        /// </summary>
        public static void StartJoyOIManagement(this IServiceProvider services)
        {
            // 启动docker节点仓库
            var dockerNodeStore = services.GetRequiredService<IDockerNodeStore>();

            // 启动状态机实例仓库
            // 会继续之前未执行完毕的状态机
            var stateMahcineInstaceStore = services.GetRequiredService<IStateMachineInstanceStore>();
            stateMahcineInstaceStore.Initialize(
                () => new JoyOIManagementContext(),
                ctx => new EFCoreRepository<BlobEntity, Guid>((JoyOIManagementContext)ctx),
                ctx => new EFCoreRepository<ActorEntity, Guid>((JoyOIManagementContext)ctx),
                ctx => new EFCoreRepository<StateMachineEntity, Guid>((JoyOIManagementContext)ctx),
                ctx => new EFCoreRepository<StateMachineInstanceEntity, Guid>((JoyOIManagementContext)ctx));
            CoreExtensions.StaticStore = stateMahcineInstaceStore;
        }
    }
}

﻿using JoyOI.ManagementService.Core;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Entities;
using JoyOI.ManagementService.Repositories;
using Microsoft.EntityFrameworkCore.Migrations;
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
        void Initialize(
            Func<IDisposable> contextFactory,
            Func<IDisposable, IRepository<BlobEntity, Guid>> blobRepositoryFactory,
            Func<IDisposable, IRepository<ActorEntity, Guid>> actorRepositoryFactory,
            Func<IDisposable, IRepository<StateMachineEntity, Guid>> stateMachineRepositoryFactory,
            Func<IDisposable, IRepository<StateMachineInstanceEntity, Guid>> stateMachineInstanceRepositoryFactory);

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
        /// 设置状态机实例的当前阶段
        /// </summary>
        Task SetInstanceStage(StateMachineBase instance, string stage);

        /// <summary>
        /// 同时运行多个任务并等待全部返回
        /// </summary>
        Task RunActors(StateMachineBase instance, IList<ActorInfo> actorInfos);

        /// <summary>
        /// 插入文件, 返回插入后的文件Id
        /// </summary>
        Task<Guid> PutBlob(string filename, byte[] contents, DateTime timeStamp);

        /// <summary>
        /// 读取文件内容, 如果文件不存在内容会等于null
        /// </summary>
        Task<IEnumerable<(BlobInfo, byte[])>> ReadBlobs(IEnumerable<BlobInfo> blobInfos);

        /// <summary>
        /// 读取任务代码, 为了减轻服务器压力会缓存非常短的时间
        /// </summary>
        Task<string> ReadActorCode(string name);
    }
}

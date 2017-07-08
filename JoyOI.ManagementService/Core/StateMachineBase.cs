using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Services;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Core
{
    /// <summary>
    /// 状态机实例的基础类
    /// </summary>
    public abstract class StateMachineBase : IDisposable
    {
        /// <summary>
        /// 第一个阶段
        /// </summary>
        public const string InitialStage = "Start";
        /// <summary>
        /// 最后一个阶段, 状态机到这个阶段后应该不做任何处理
        /// </summary>
        public const string FinalStage = "Finished";

        /// <summary>
        /// 状态机Id
        /// </summary>
        public Guid Id { get; internal set; }
        /// <summary>
        /// 状态机的当前状态
        /// </summary>
        public StateMachineStatus Status { get; internal set; }
        /// <summary>
        /// 状态机的当前阶段
        /// </summary>
        public string Stage { get; internal set; }
        /// <summary>
        /// 已开始的任务列表
        /// </summary>
        public IList<ActorInfo> StartedActors { get; internal set; }
        /// <summary>
        /// 初始的文件列表
        /// </summary>
        public IList<BlobInfo> InitialBlobs { get; internal set; }
        /// <summary>
        /// 管理状态机实例的仓库
        /// </summary>
        internal IStateMachineInstanceStore Store { get; set; }
        /// <summary>
        /// 使用的限制参数
        /// </summary>
        internal ContainerLimitation Limitation { get; set; }
        /// <summary>
        /// 更新数据库时使用的锁
        /// 目前仅在并列执行任务时会使用
        /// </summary>
        internal SemaphoreSlim DbUpdateLock { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public StateMachineBase()
        {
            DbUpdateLock = new SemaphoreSlim(1);
        }

        /// <summary>
        /// 状态机运行完毕后执行的函数
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// 发布单个任务到容器并运行
        /// </summary>
        protected Task DeployAndRunActorAsync(RunActorParam parameter)
        {
            return DeployAndRunActorsAsync(parameter);
        }

        /// <summary>
        /// 发布多个任务到容器并同时运行
        /// </summary>
        protected Task DeployAndRunActorsAsync(params RunActorParam[] parameters)
        {
            var runActors = new List<ActorInfo>();
            foreach (var parameter in parameters)
            {
                var actorInfo = new ActorInfo()
                {
                    Name = parameter.Name,
                    StartTime = DateTime.UtcNow,
                    EndTime = null,
                    Inputs = parameter.Inputs,
                    Outputs = new BlobInfo[0],
                    Exceptions = new string[0],
                    Status = ActorStatus.Running,
                    Stage = Stage,
                    Tag = parameter.Tag,
                    RunningNode = null,
                    RunningContainer = null
                };
                runActors.Add(actorInfo);
            }
            return Store.RunActors(this, runActors);
        }

        /// <summary>
        /// 切换到新的阶段
        /// </summary>
        protected Task SetStage(string stage)
        {
            return Store.SetInstanceStage(this, stage);
        }

        /// <summary>
        /// 查找指定阶段和名称的任务列表, 阶段和名称可以不指定
        /// </summary>
        protected IEnumerable<ActorInfo> FindActors(
            string stage = null, string actor = null, string tag = null)
        {
            var result = StartedActors.AsEnumerable();
            if (!string.IsNullOrEmpty(stage))
            {
                result = result.Where(x => x.Stage == actor);
            }
            if (!string.IsNullOrEmpty(actor))
            {
                result = result.Where(x => x.Name == actor);
            }
            if (!string.IsNullOrEmpty(tag))
            {
                result = result.Where(x => x.Tag == tag);
            }
            return result;
        }

        /// <summary>
        /// 查找指定阶段和名称的单个列表, 阶段和名称可以不指定, 找不到或者找到多个时抛出错误
        /// </summary>
        protected ActorInfo FindSingleActor(
            string stage = null, string actor = null, string tag = null)
        {
            var actorInfos = FindActors(stage, actor);
            var actorInfo = actorInfos.SingleOrDefault();
            if (actorInfo != null)
            {
                return actorInfo;
            }
            throw new InvalidOperationException(
                actorInfos.Any() ? "more than one actors found" : "no actors found");
        }

        /// <summary>
        /// 读取单个文件到字符串
        /// </summary>
        protected Task<string> ReadAllText(BlobInfo blobInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取单个文件到字节数组
        /// </summary>
        protected Task<byte[]> ReadAllBytes(BlobInfo blobInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 批量读取多个文件到字符串列表
        /// </summary>
        protected Task<IList<string>> BatchReadAllText(IEnumerable<BlobInfo> blobInfos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 批量读取多个文件到字节数组列表
        /// </summary>
        protected Task<IList<byte[]>> BatchReadAllBytes(IEnumerable<BlobInfo> blobInfos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 作用: TODO
        /// </summary>
        protected Task<string> HttpInvokeAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从指定的阶段开始重新运行状态机
        /// </summary>
        public abstract Task RunAsync(string stage);
    }
}

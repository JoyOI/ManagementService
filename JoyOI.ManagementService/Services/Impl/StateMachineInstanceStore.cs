using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JoyOI.ManagementService.Core;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using JoyOI.ManagementService.Model.Enums;
using JoyOI.ManagementService.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.CodeAnalysis;
using System.IO;
using JoyOI.ManagementService.Utils;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机实例的仓库, 应该为单例
    /// 
    /// 外部创建状态机实例:
    /// - (可选) 上传一个或多个blob
    /// - 调用StateMachineInstanceService.Put
    ///   - 调用StateMachineInstanceStore.CreateInstance创建状态机实例
    ///   - 添加状态机实例到数据库
    ///   - 调用StateMachineInstanceStore.RunInstance运行状态机实例, 会在后台运行
    /// 
    /// 运行状态机实例:
    /// - 如果当前Stage不是初始阶段
    ///   - 查找属于该Stage的StartedActors
    ///     - 调用docker的api删除CurrentNode中的CurrentContainer
    ///     - 删除这些actors并更新到数据库
    /// - 从当前Stage开始运行
    ///   - 调用StateMachineBase.RunAsync
    /// - 循环切换状态
    ///   - 调用StateMachineInstanceStore.SetInstanceStage切换阶段
    ///     - 修改Stage并写入到数据库
    ///   - 调用StateMachineInstanceStore.RunActors运行任务
    ///     - 更新StartedActors
    ///     - 并列处理
    ///       - 选择一个容器 (应该考虑到负载均衡)
    ///       - 生成一个container标识
    ///       - 更新actor的RunningNode和RunningContainer, 注意线程安全
    ///       - 调用docker的api创建容器
    ///       - 上传Inputs到容器
    ///       - 上传代码到容器
    ///       - 执行容器
    ///       - 等待执行完毕
    ///       - 下载result.json
    ///       - 根据result.json下载文件并插入到blob
    ///       - 更新actor的Outputs和Status, 注意线程安全
    /// - 设置状态机实例的Status和EndTime, 更新到数据库
    /// 
    /// 启动已中断的状态机实例:
    /// - 获取数据库中Running的状态机实例
    ///   - 因为有可能配置多个管理服务, 获取时需要传入FromManagementService
    ///   - 如果ReRunTimes >= MaxReRunTimes, 则直接标记为Failed
    /// - 调用StateMachineInstanceStore.RunInstance运行状态机实例
    /// 
    /// 强制修改状态机的流程:
    /// - 修改状态机实例的Stage
    /// - 调用StateMachineInstanceStore.RunInstance运行状态机实例
    /// </summary>
    internal class StateMachineInstanceStore : IStateMachineInstanceStore
    {
        private JoyOIManagementConfiguration _configuration;
        private IDockerNodeStore _dockerNodeStore;
        private ConcurrentDictionary<string, (string, Func<StateMachineBase>)> _factoryCache;
        private Func<JoyOIManagementContext> _contextFactory;
        private bool _initilaized;
        private object _initializeLock;

        public StateMachineInstanceStore(JoyOIManagementConfiguration configuration, IDockerNodeStore dockerNodeStore)
        {
            _configuration = configuration;
            _dockerNodeStore = dockerNodeStore;
            _factoryCache = new ConcurrentDictionary<string, (string, Func<StateMachineBase>)>();
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

        private void ContinueExecutingInstances()
        {
            // 继续执行之前未执行完毕的实例
        }

        public Task<StateMachineBase> CreateInstance(
            StateMachineEntity stateMachineEntity,
            StateMachineInstanceEntity stateMachineInstanceEntity)
        {
            // 从缓存中获取
            if (!_factoryCache.TryGetValue(stateMachineEntity.Name, out var factory) ||
                factory.Item1 != stateMachineEntity.Body)
            {
                // 不存在或内容有变化时调用roslyn重新编译
                // https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples#delegate
                var assemblyName = $"__{stateMachineEntity.Name}_{DateTime.UtcNow.Ticks}";
                var optimizationLevel = OptimizationLevel.Debug;
                var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: optimizationLevel);
                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !a.FullName.StartsWith("__"))
                    .Select(a => MetadataReference.CreateFromFile(a.Location));
                var syntaxTree = CSharpSyntaxTree.ParseText(stateMachineEntity.Body);
                var compilation = CSharpCompilation.Create(assemblyName)
                    .WithOptions(compilationOptions)
                    .AddReferences(references)
                    .AddSyntaxTrees(syntaxTree);
                var assemblyPath = string.Join(Path.GetTempPath(), assemblyName + ".dll");
                var emitResult = compilation.Emit(assemblyPath);
                if (!emitResult.Success)
                {
                    throw new InvalidOperationException(string.Join("\r\n",
                        emitResult.Diagnostics.Where(d => d.WarningLevel == 0)));
                }
                var assemblyBytes = File.ReadAllBytes(assemblyPath);
                File.Delete(assemblyPath);
                var assembly = Assembly.Load(assemblyBytes);
                var stateMachineType = assembly.GetTypes()
                    .FirstOrDefault(x => typeof(StateMachineBase).IsAssignableFrom(x));
                if (stateMachineType == null)
                {
                    throw new InvalidOperationException(
                        "no state machine type defined, please create a class inherit StateMachineBase");
                }
                factory.Item1 = stateMachineEntity.Body;
                factory.Item2 = Expression.Lambda<Func<StateMachineBase>>(
                    Expression.New(stateMachineType.GetConstructors()[0])).Compile();
                // 保存到缓存
                _factoryCache[stateMachineEntity.Name] = factory;
            }
            // 创建状态机实例
            var instance = factory.Item2();
            instance.Id = stateMachineInstanceEntity.Id;
            instance.Status = stateMachineInstanceEntity.Status;
            instance.Stage = stateMachineInstanceEntity.Stage;
            instance.StartedActors = stateMachineInstanceEntity.StartedActors;
            instance.InitialBlobs = stateMachineInstanceEntity.InitialBlobs;
            instance.Store = this;
            instance.Limitation = stateMachineInstanceEntity.Limitation;
            return Task.FromResult(instance);
        }

        public async Task RunInstance(StateMachineBase instance)
        {
            try
            {
                // 如果当前Stage不是初始阶段
                if (instance.Stage != StateMachineBase.InitialStage)
                {
                    // 查找属于该Stage的StartedActors
                    // TODO
                    // 调用docker的api删除CurrentNode中的CurrentContainer
                    // TODO
                    // 删除这些actors并更新到数据库
                    // TODO
                }
                // 从当前Stage开始运行
                await instance.RunAsync();
                // 设置状态机实例的Status和EndTime, 更新到数据库
                instance.Status = StateMachineStatus.Succeeded;
                instance.Stage = StateMachineBase.FinalStage;
                var endTime = DateTime.UtcNow;
                using (var context = _contextFactory())
                {
                    var set = context.Set<StateMachineInstanceEntity>();
                    var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                    instanceEntity.Status = instance.Status;
                    instanceEntity.Stage = instance.Stage;
                    instanceEntity.EndTime = endTime;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // 设置状态机实例的Status到失败
                using (var context = _contextFactory())
                {
                    var set = context.Set<StateMachineInstanceEntity>();
                    var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                    instanceEntity.Status = StateMachineStatus.Failed;
                    instanceEntity.Exception = ex.ToString();
                    await context.SaveChangesAsync();
                }
            }
            finally
            {
                // 释放资源 (默认无处理, 继承类中可能会重写此函数)
                instance.Dispose();
            }
        }

        public Task SetInstanceStage(StateMachineBase instance, string stage)
        {
            throw new NotImplementedException();
        }

        public Task RunActors(StateMachineBase instance, IList<ActorInfo> actorInfos)
        {
            throw new NotImplementedException();
            // TODO: 需要并发处理
            /*var node = await _dockerNodeStore.AcquireNode();
            try
            {
                using (var client = node.CreateDockerClient())
                {

                }
            }
            finally
            {
                _dockerNodeStore.ReleaseNode(node);
            }*/
        }

        public async Task<IEnumerable<(BlobInfo, byte[])>> ReadBlobs(IEnumerable<BlobInfo> blobInfos)
        {
            var result = blobInfos.Select(blob => (blob, (byte[])null)).ToList();
            var blobIds = result.Select(x => x.Item1.Id).ToList();
            using (var context = _contextFactory())
            {
                var blobs = await context.Set<BlobEntity>()
                    .Where(x => blobIds.Contains(x.BlobId))
                    .GroupBy(x => x.BlobId)
                    .ToDictionaryAsync(x => x.Key, x => x.OrderBy(b => b.ChunkIndex).ToList());
                for (var x = 0; x < result.Count; ++x)
                {
                    var (blob, contents) = result[x];
                    if (blobs.TryGetValue(blob.Id, out var blobEntities))
                    {
                        result[x] = (blob, BlobUtils.MergeChunksBody(blobEntities));
                    }
                }
            }
            return result;
        }
    }
}

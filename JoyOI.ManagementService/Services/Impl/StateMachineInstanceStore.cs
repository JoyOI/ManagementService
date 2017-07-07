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

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理状态机实例的仓库, 应该为单例
    /// 
    /// 外部启动状态机的流程:
    /// - (可选) 上传一个或多个blob
    /// - 调用StateMachineInstanceService.Put
    ///   - 获取name对应的状态机代码
    ///   - 使用roslyn编译状态机代码
    ///   - 添加状态机实例到数据库
    ///     - 初始的CurrentActor是{ Name = null, Inputs = blobs }, 这个actor不会加到finished中
    ///     - 注意要设置FromManagementService
    ///   - 调用RunAsync(null, blobs)
    /// - 状态机实例调用DeployAndRunActorAsync(name, blobs)
    ///   - 更新状态机
    ///     - 把CurrentActor添加到FinishedActor
    ///     - 更新CurrentActor
    ///     - 重置CurrentNode和CurrentContainer
    ///   - 选择一个容器 (应该考虑到负载均衡)
    ///   - 调用docker的api创建容器
    ///     - 更新CurrentNode和CurrentContainer
    ///   - 上传blobs到容器
    ///   - 执行actor中的代码
    ///   - 下载result.json
    ///   - 根据result.json下载各个文件并插入blob
    ///   - 更新状态机
    ///     - 更新CurrentActor的状态到Succeeded
    ///     - 更新CurrentActor的Outputs
    ///     - 重置CurrentNode和CurrentContainer
    ///   - 状态机是否执行完毕?
    ///     - 执行完毕后更新状态机
    ///       - 把CurrentActor添加到FinishedActor
    ///       - 重置CurrentActor
    ///       - 更新状态机实例的状态为Succeeded
    ///     - 继续调用DeployAndRunActorAsync(name, blobs)
    /// 
    /// 启动已中断的状态机的流程:
    /// - 获取数据库中Running的状态机实例
    ///   - 因为有可能配置多个管理服务, 获取时需要传入FromManagementService
    ///   - 如果ReRunTimes >= MaxReRunTimes, 则直接标记为Failed
    /// - 调用docker的api删除CurrentNode中的CurrentContainer
    /// - 调用RunAsync(CurrentActor.Name, CurrentActor.Inputs), 之后同上
    /// 
    /// 强制修改状态机的流程:
    /// - 调用docker的api删除CurrentNode中的CurrentContainer
    /// - 修改状态机实例
    /// - 调用RunAsync(CurrentActor.Name, CurrentActor.Inputs), 之后同上
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
            instance.FinishedActors = stateMachineInstanceEntity.FinishedActors;
            instance.CurrentActor = stateMachineInstanceEntity.CurrentActor;
            instance.Store = this;
            instance.Limitation = stateMachineInstanceEntity.Limitation;
            return Task.FromResult(instance);
        }

        public async Task RunInstance(StateMachineBase instance)
        {
            try
            {
                // 运行第一个actor
                var actor = instance.CurrentActor;
                if (actor != null)
                {
                    instance.Store = this;
                    await instance.RunAsync(actor.Name, actor.Inputs);
                }
                // 更新实例的状态到完成
                instance.Status = StateMachineStatus.Succeeded;
                instance.FinishedActors.Add(instance.CurrentActor);
                instance.CurrentActor = null;
                using (var context = _contextFactory())
                {
                    var set = context.Set<StateMachineInstanceEntity>();
                    var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                    instanceEntity.Status = instance.Status;
                    instanceEntity.FinishedActors = instance.FinishedActors;
                    instanceEntity.CurrentActor = instance.CurrentActor;
                    instanceEntity.EndTime = instance.FinishedActors.Last().EndTime;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // 更新实例的状态到失败
                using (var context = _contextFactory())
                {
                    var set = context.Set<StateMachineInstanceEntity>();
                    var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                    instanceEntity.Status = StateMachineStatus.Failed;
                    var actor = instanceEntity.CurrentActor;
                    actor.Exceptions = new[] { ex.ToString() };
                    instanceEntity.CurrentActor = actor;
                    await context.SaveChangesAsync();
                }
            }
            finally
            {
                // 释放资源 (默认无处理, 继承类中可能会重写此函数)
                instance.Dispose();
            }
        }

        public async Task RunActor(StateMachineBase instance)
        {
            // 操作docker客户端
            var node = await _dockerNodeStore.AcquireNode();
            try
            {
                using (var client = node.CreateDockerClient())
                {

                }
            }
            finally
            {
                _dockerNodeStore.ReleaseNode(node);
            }
        }
    }
}

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
using Docker.DotNet.Models;
using System.Threading;

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
    ///     - 调用docker的api删除残留的容器
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
                ContinueRunningInstances();
                _initilaized = true;
            }
        }

        private void ContinueRunningInstances()
        {
            // 获取运行中的状态机实例
            IDictionary<string, StateMachineEntity> stateMachineMap;
            var continueInstances = new ConcurrentQueue<StateMachineInstanceEntity>();
            using (var context = _contextFactory())
            {
                var runningInstances = context.Set<StateMachineInstanceEntity>()
                    .Where(x =>
                        x.Status == StateMachineStatus.Running &&
                        x.FromManagementService == _configuration.Name)
                    .ToList();
                var stateMachineNames = runningInstances
                    .Select(c => c.Name).Distinct().ToList();
                stateMachineMap = context.Set<StateMachineEntity>()
                    .Where(x => stateMachineNames.Contains(x.Name))
                    .ToDictionary(x => x.Name);
                // 判断重新运行次数
                // 超过最大次数的标记状态到失败
                // 标记失败的需要删除残留的容器, 删除可以并发处理
                var childTasks = runningInstances.Select(instance => Task.Run(async () =>
                {
                    if (instance.ReRunTimes >= StateMachineInstanceEntity.MaxReRunTimes)
                    {
                        instance.Status = StateMachineStatus.Failed;
                        instance.Exception = "re-run times exhausted";
                    }
                    else if (!stateMachineMap.ContainsKey(instance.Name))
                    {
                        instance.Status = StateMachineStatus.Failed;
                        instance.Exception = "state machine not found";
                    }
                    else
                    {
                        ++instance.ReRunTimes;
                        continueInstances.Enqueue(instance);
                    }
                    if (instance.Status == StateMachineStatus.Failed)
                    {
                        var startedActors = instance.StartedActors;
                        await RemoveRunningContainers(startedActors);
                        instance.StartedActors = startedActors;
                    }
                })).ToArray();
                Task.WaitAll(childTasks);
                context.SaveChanges();
            }
            // 继续执行这些状态机实例
            foreach (var instance in continueInstances)
            {
                var stateMachine = stateMachineMap[instance.Name];
                var instanceObj = CreateInstance(stateMachine, instance).Result;
#pragma warning disable CS4014
                RunInstance(instanceObj); // 在后台运行
#pragma warning restore CS4014
            }
        }

        private async Task UpdateInstanceEntity(Guid id, Action<StateMachineInstanceEntity> update)
        {
            using (var context = _contextFactory())
            {
                var set = context.Set<StateMachineInstanceEntity>();
                var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == id);
                if (instanceEntity == null)
                {
                    throw new InvalidOperationException($"state machine instance entity {id} not found");
                }
                update(instanceEntity);
                await context.SaveChangesAsync();
            }
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
                    var startedActors = instance.StartedActors;
                    var removeActors = startedActors.Where(x => x.Stage == instance.Stage).ToList();
                    // 调用docker的api删除残留的容器
                    await RemoveRunningContainers(removeActors);
                    // 删除这些actors并更新到数据库
                    foreach (var removeActor in removeActors)
                    {
                        startedActors.Remove(removeActor);
                    }
                    await UpdateInstanceEntity(instance.Id, instanceEntity =>
                    {
                        instanceEntity.StartedActors = startedActors;
                    });
                }
                // 从当前Stage开始运行
                await instance.RunAsync();
                // 设置状态机实例的Status和EndTime, 更新到数据库
                instance.Status = StateMachineStatus.Succeeded;
                instance.Stage = StateMachineBase.FinalStage;
                var endTime = DateTime.UtcNow;
                await UpdateInstanceEntity(instance.Id, instanceEntity =>
                {
                    instanceEntity.Status = instance.Status;
                    instanceEntity.Stage = instance.Stage;
                    instanceEntity.EndTime = endTime;
                });
            }
            catch (Exception ex)
            {
                // 调用docker的api删除残留的容器
                var startedActors = instance.StartedActors;
                try
                {
                    await RemoveRunningContainers(startedActors);
                }
                catch (Exception newEx)
                {
                    ex = new InvalidOperationException(
                        "Remove running containers after error failed:" + newEx.ToString(), ex);
                }
                // 设置状态机实例的Status到失败
                await UpdateInstanceEntity(instance.Id, instanceEntity =>
                {
                    instanceEntity.StartedActors = startedActors;
                    instanceEntity.Status = StateMachineStatus.Failed;
                    instanceEntity.Exception = ex.ToString();
                });
            }
            finally
            {
                // 释放资源 (默认无处理, 继承类中可能会重写此函数)
                instance.Dispose();
            }
        }

        public async Task SetInstanceStage(StateMachineBase instance, string stage)
        {
            // 更新实体中的状态
            using (var context = _contextFactory())
            {
                var set = context.Set<StateMachineInstanceEntity>();
                var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                instanceEntity.Stage = stage;
                await context.SaveChangesAsync();
            }
            // 更新实例中的状态
            instance.Stage = stage;
        }

        private async Task RemoveRunningContainers(IList<ActorInfo> actorInfos)
        {
            var childTasks = actorInfos.GroupBy(x => x.RunningNode)
                .Select(group => Task.Run(async () =>
            {
                // 按节点分组
                var node = _dockerNodeStore.GetNode(group.Key);
                if (node == null)
                {
                    return;
                }
                var containerTags = new HashSet<string>(group.Select(x => x.RunningContainer));
                using (var client = node.CreateDockerClient())
                {
                    // 查找节点下的所有容器
                    // 节点下的容器数量较少(<100)时可以直接查询全部以减少查询次数
                    // 如果需要按名称查找可以使用Filters["name"]
                    var containers = await client.Containers.ListContainersAsync(
                        new ContainersListParameters() { All = true });
                    foreach (var container in containers)
                    {
                        if (container.Names.Any(name => containerTags.Contains(name)))
                        {
                            // 删除容器
                            await client.Containers.RemoveContainerAsync(
                                container.ID, new ContainerRemoveParameters() { Force = true });
                        }
                    }
                }
            })).ToList();
            await Task.WhenAll(childTasks);
            // 清空所有RunningNode和RunningContainer
            foreach (var actorInfo in actorInfos)
            {
                actorInfo.RunningNode = null;
                actorInfo.RunningContainer = null;
            }
        }

        private async Task RunActorInternal(StateMachineBase instance, ActorInfo actorInfo)
        {
            // 请勿直接调用此函数, 此函数不会更新StartedActors
            // 选择一个容器
            var node = await _dockerNodeStore.AcquireNode();
            try
            {
                using (var client = node.CreateDockerClient())
                {
                    // 生成一个container标识
                    var containerTag = PrimaryKeyUtils.Generate<Guid>().ToString();
                    // 更新actor的RunningNode和RunningContainer, 注意线程安全
                    actorInfo.RunningNode = node.Name;
                    actorInfo.RunningContainer = containerTag;
                    await instance.DbUpdateLock.WaitAsync();
                    try
                    {
                        using (var context = _contextFactory())
                        {
                            var set = context.Set<StateMachineInstanceEntity>();
                            var instanceEntity = await set.FirstOrDefaultAsync(x => x.Id == instance.Id);
                            instanceEntity.StartedActors = instance.StartedActors;
                            await context.SaveChangesAsync();
                        }
                    }
                    finally
                    {
                        instance.DbUpdateLock.Release();
                    }
                    // 调用docker的api创建容器
                    var createContainerResponse = await client.Containers.CreateContainerAsync(
                        new CreateContainerParameters()
                        {
                            NetworkDisabled = true,
                            Image = node.NodeInfo.Image,
                            Name = containerTag,
                            HostConfig = HostConfigUtils.WithLimitation(new HostConfig(), instance.Limitation)
                        });
                    var containerId = createContainerResponse.ID;
                    // 上传Inputs到容器
                    var inputBlobs = await ReadBlobs(actorInfo.Inputs);
                    foreach (var (blob, bytes) in inputBlobs)
                    {

                    }
                    // 上传代码到容器

                    // 执行容器

                    // 等待执行完毕

                    // 下载result.json

                    // 根据result.json下载文件并插入到blob

                    // 更新actor的Outputs和Status, 注意线程安全
                }
            }
            finally
            {
                _dockerNodeStore.ReleaseNode(node);
            }
        }

        public async Task RunActors(StateMachineBase instance, IList<ActorInfo> actorInfos)
        {
            // 更新StartedActors
            foreach (var actorInfo in actorInfos)
            {
                instance.StartedActors.Add(actorInfo);
            }
            // 并列处理
            var childTasks = new List<Task>();
            foreach (var actorInfo in actorInfos)
            {
                childTasks.Add(RunActorInternal(instance, actorInfo));
            }
            // 等待全部完成
            await Task.WhenAll(childTasks);
        }

        public async Task<IEnumerable<(BlobInfo, byte[])>> ReadBlobs(IEnumerable<BlobInfo> blobInfos)
        {
            var result = blobInfos.Select(x => ValueTuple.Create(x, (byte[])null)).ToList();
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

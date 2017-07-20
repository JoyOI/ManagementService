using JoyOI.ManagementService.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JoyOI.ManagementService.Core;
using System.Collections.Concurrent;
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
using Newtonsoft.Json;
using JoyOI.ManagementService.Model.Dtos;
using AutoMapper;
using Docker.DotNet;
using JoyOI.ManagementService.Repositories;
using System.Net.Http;
using System.Net.Sockets;

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
    /// - 如果之前已经运行过该状态机
    ///   - 查找该Stage之后的StartedActors
    ///     - 删除这些actors并更新到数据库
    /// - 从当前Stage开始运行
    ///   - 调用StateMachineBase.RunAsync
    /// - 循环切换状态
    ///   - 调用StateMachineInstanceStore.SetInstanceStage切换阶段
    ///     - 修改Stage并写入到数据库
    ///   - 调用StateMachineInstanceStore.RunActors运行任务
    ///     - 更新状态机实例的StartedActors, 更新到数据库
    ///     - 并列处理
    ///       - 选择一个节点 (应该考虑到负载均衡)
    ///       - 生成一个容器名称
    ///       - 更新actorInfo的UsedNode和UsedContainer, 不更新到数据库
    ///       - 编译actor代码
    ///       - 创建容器
    ///       - 上传Inputs和actor.dll到容器
    ///       - 运行容器并等待完毕
    ///       - 成功时下载return.json, 失败时下载run-actor.log并抛出里面的内容
    ///       - 根据return.json下载文件并插入到blob
    ///       - 更新actorInfo的Outputs, Status, EndTime, 不更新到数据库
    ///       - 删除容器 (finally)
    /// - 是否发生错误?
    ///   - 是: 更新状态机实例的StartedActors, Status, Exceptions, 更新到数据库
    ///   - 否: 更新状态机实例的StartedActors, 更新到数据库
    /// 
    /// 启动已中断的状态机实例:
    /// - 获取数据库中Running的状态机实例
    ///   - 因为有可能配置多个管理服务, 获取时需要传入FromManagementService
    ///   - 如果ReRunTimes >= MaxReRunTimes, 则直接标记为Failed
    /// - 调用StateMachineInstanceStore.RunInstance运行状态机实例
    /// 
    /// 强制修改状态机:
    /// - 修改状态机实例的ExecutionKey
    ///   - 修改后正在运行的实例将不能修改这个实例, 并且在检测到不能修改时中断运行
    /// - 修改状态机实例的Stage
    /// - 删除该Stage之后的StartedActors
    /// - 调用StateMachineInstanceStore.RunInstance运行状态机实例
    /// 
    /// 残留的容器:
    /// - 如果容器执行过程中发生了错误, 会在finally中删除
    /// - 如果管理服务本身崩溃了, 则会在下次启动时删除所有节点中已停止的, 并且属于当前管理服务的容器
    /// </summary>
    internal class StateMachineInstanceStore : IStateMachineInstanceStore
    {
        private JoyOIManagementConfiguration _configuration;
        private IDockerNodeStore _dockerNodeStore;
        private IDynamicCompileService _dynamicCompileService;
        private ConcurrentDictionary<string, (string, Func<StateMachineBase>)> _factoryCache;
        private ConcurrentDictionary<string, (string, DateTime)> _actorCodeCache;
        private TimeSpan _actorCodeCacheTime;
        private int _actorMaxRetryTimes;
        private Func<IDisposable> _contextFactory;
        private Func<IDisposable, IRepository<BlobEntity, Guid>> _blobRepositoryFactory;
        private Func<IDisposable, IRepository<ActorEntity, Guid>> _actorRepositoryFactory;
        private Func<IDisposable, IRepository<StateMachineEntity, Guid>> _stateMachineRepositoryFactory;
        private Func<IDisposable, IRepository<StateMachineInstanceEntity, Guid>> _stateMachineInstanceRepositoryFactory;
        private bool _initilaized;
        private object _initializeLock;

        public StateMachineInstanceStore(
            JoyOIManagementConfiguration configuration,
            IDockerNodeStore dockerNodeStore,
            IDynamicCompileService dynamicCompileService)
        {
            _configuration = configuration;
            _dockerNodeStore = dockerNodeStore;
            _dynamicCompileService = dynamicCompileService;
            _factoryCache = new ConcurrentDictionary<string, (string, Func<StateMachineBase>)>();
            _actorCodeCache = new ConcurrentDictionary<string, (string, DateTime)>();
            _actorCodeCacheTime = TimeSpan.FromSeconds(1);
            _actorMaxRetryTimes = 3;
            _contextFactory = null;
            _blobRepositoryFactory = null;
            _actorRepositoryFactory = null;
            _stateMachineRepositoryFactory = null;
            _stateMachineInstanceRepositoryFactory = null;
            _initilaized = false;
            _initializeLock = new object();
        }

        public void Initialize(
            Func<IDisposable> contextFactory,
            Func<IDisposable, IRepository<BlobEntity, Guid>> blobRepositoryFactory,
            Func<IDisposable, IRepository<ActorEntity, Guid>> actorRepositoryFactory,
            Func<IDisposable, IRepository<StateMachineEntity, Guid>> stateMachineRepositoryFactory,
            Func<IDisposable, IRepository<StateMachineInstanceEntity, Guid>> stateMachineInstanceRepositoryFactory)
        {
            if (_initilaized)
            {
                return;
            }
            lock (_initializeLock)
            {
                _contextFactory = contextFactory;
                _blobRepositoryFactory = blobRepositoryFactory;
                _actorRepositoryFactory = actorRepositoryFactory;
                _stateMachineRepositoryFactory = stateMachineRepositoryFactory;
                _stateMachineInstanceRepositoryFactory = stateMachineInstanceRepositoryFactory;
                RemoveStoppedContainers();
                ContinueRunningInstances();
                _initilaized = true;
            }
        }



        private void RemoveStoppedContainers()
        {
            // 删除已停止的, 并且属于当前管理服务的容器
            var prefix = _configuration.Name + ".";
            var childTasks = _dockerNodeStore.GetNodes().Select(node => Task.Run(async () =>
            {
                try
                {
                    var client = node.Client;
                    var result = await client.Containers.ListContainersAsync(
                        new ContainersListParameters() { All = true });
                    foreach (var container in result)
                    {
                        if (!(container.State == "created" || container.State == "exited"))
                            continue;
                        if (!container.Names.Any(x => x.StartsWith(prefix)))
                            continue;
                        try
                        {
                            await client.Containers.RemoveContainerAsync(container.ID,
                                new ContainerRemoveParameters() { Force = true });
                        }
                        catch (DockerContainerNotFoundException)
                        {
                            // 可能已经被删除了
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 如果是连接错误则记录日志并正常启动程序, 否则让程序启动失败
                    if (DockerNode.IsConnectionError(ex))
                        Console.Error.WriteLine(ex.ToString());
                    else
                        throw;
                }
            })).ToArray();
            Task.WaitAll(childTasks);
        }

        private void ContinueRunningInstances()
        {
            // 获取运行中的状态机实例
            IDictionary<string, StateMachineEntity> stateMachineMap;
            var continueInstances = new List<StateMachineInstanceEntity>();
            using (var context = _contextFactory())
            {
                var stateMachineRepository = _stateMachineRepositoryFactory(context);
                var stateMachineInstanceRepository = _stateMachineInstanceRepositoryFactory(context);
                var runningInstances = stateMachineInstanceRepository.QueryAsync(q => q
                    .Where(x =>
                        x.Status == StateMachineStatus.Running &&
                        x.FromManagementService == _configuration.Name).ToListAsyncTestable()).Result;
                var stateMachineNames = runningInstances
                    .Select(c => c.Name).Distinct().ToList();
                stateMachineMap = stateMachineRepository.QueryAsync(q => q
                    .Where(x => stateMachineNames
                    .Contains(x.Name)).ToDictionaryAsyncTestable(x => x.Name)).Result;
                // 判断重新运行次数, 超过最大次数的标记状态到失败
                foreach (var instance in runningInstances)
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
                        instance.ExecutionKey = PrimaryKeyUtils.Generate<Guid>().ToString();
                        continueInstances.Add(instance);
                    }
                    if (instance.Status == StateMachineStatus.Failed)
                    {
                        var startedActors = instance.StartedActors;
                        instance.StartedActors = startedActors;
                    }
                }
                stateMachineRepository.SaveChangesAsync().Wait();
                stateMachineInstanceRepository.SaveChangesAsync().Wait();
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

        private async Task UpdateInstanceEntity(
            Guid id, string executionKey, Action<StateMachineInstanceEntity> update)
        {
            using (var context = _contextFactory())
            {
                var repository = _stateMachineInstanceRepositoryFactory(context);
                var instanceEntity = await repository.QueryAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Id == id));
                if (instanceEntity == null)
                {
                    // 运行中被DELETE了
                    throw new StateMachineInterpretedException();
                }
                else if (instanceEntity.ExecutionKey != executionKey)
                {
                    // 运行中被PATCH了
                    throw new StateMachineInterpretedException();
                }
                update(instanceEntity);
                try
                {
                    await repository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new StateMachineInterpretedException();
                }
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
                var assemblyBytes = _dynamicCompileService.Compile(
                    stateMachineEntity.Body, OutputKind.DynamicallyLinkedLibrary);
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
            instance.ExecutionKey = stateMachineInstanceEntity.ExecutionKey;
            instance.Status = stateMachineInstanceEntity.Status;
            instance.Stage = stateMachineInstanceEntity.Stage;
            instance.StartedActors = stateMachineInstanceEntity.StartedActors;
            instance.InitialBlobs = stateMachineInstanceEntity.InitialBlobs;
            instance.Store = this;
            instance.Limitation = stateMachineInstanceEntity.Limitation;
            instance.Parameters = stateMachineInstanceEntity.Parameters;
            instance.Priority = stateMachineInstanceEntity.Priority;
            return Task.FromResult(instance);
        }

        public async Task RunInstance(StateMachineBase instance)
        {
            try
            {
                // 如果之前已经运行过该状态机
                if (instance.Stage != StateMachineBase.InitialStage ||
                    instance.StartedActors.Count > 0)
                {
                    // 查找该Stage之后的StartedActors
                    var startedActors = instance.StartedActors;
                    // 删除这些actors并更新到数据库
                    var newStartedActors = new List<ActorInfo>();
                    if (instance.Stage != StateMachineBase.InitialStage)
                    {
                        foreach (var startedActor in startedActors)
                        {
                            if (startedActor.Stage == instance.Stage)
                            {
                                break;
                            }
                            newStartedActors.Add(startedActor);
                        }
                    }
                    startedActors = newStartedActors;
                    instance.StartedActors = startedActors;
                    await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
                    {
                        instanceEntity.StartedActors = startedActors;
                        instanceEntity.Status = StateMachineStatus.Running;
                        instanceEntity.Stage = instance.Stage;
                        instanceEntity.ReRunTimes = 0;
                        instanceEntity.Exception = null;
                        instanceEntity.EndTime = null;
                    });
                }
                // 从当前Stage开始运行
                await instance.RunAsync();
                // 设置状态机实例的Status和EndTime, 更新到数据库
                instance.Status = StateMachineStatus.Succeeded;
                instance.Stage = StateMachineBase.FinalStage;
                var endTime = DateTime.UtcNow;
                await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
                {
                    instanceEntity.Status = instance.Status;
                    instanceEntity.Stage = instance.Stage;
                    instanceEntity.EndTime = endTime;
                });
            }
            catch (StateMachineInterpretedException)
            {
                // 状态机实例已中断, 不需要做额外处理
            }
            catch (Exception ex)
            {
                // 状态机的代码本身发生了错误, 更新Status到失败
                try
                {
                    await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
                    {
                        instanceEntity.Status = StateMachineStatus.Failed;
                        instanceEntity.Exception = ex.ToString();
                    });
                    await instance.HandleErrorAsync(ex);
                }
                catch (StateMachineInterpretedException)
                {
                }
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
            await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
            {
                instanceEntity.Stage = stage;
            });
            // 更新实例中的状态
            instance.Stage = stage;
        }

        private async Task RunActorInternalRetryable(StateMachineBase instance, ActorInfo actorInfo)
        {
            // 请勿直接调用此函数, 此函数不会更新StartedActors
            // 此函数可以反复重试, 注意不要做出不可逆的修改
            var node = await _dockerNodeStore.AcquireNode(instance.Priority);
            try
            {
                // 获取docker客户端
                var client = node.Client;
                // 生成一个容器名称
                var containerTag = $"{_configuration.Name}.{PrimaryKeyUtils.Generate<Guid>()}";
                // 更新actorInfo的UsedNode和UsedContainer, 不更新到数据库
                actorInfo.UsedNode = node.Name;
                actorInfo.UsedContainer = containerTag;
                // 编译actor代码
                var actorCode = await ReadActorCode(actorInfo.Name);
                var actorBytes = _dynamicCompileService.Compile(
                    actorCode, OutputKind.ConsoleApplication);
                // 创建容器
                var hostConfig = HostConfigUtils.WithLimitation(
                    new HostConfig(), instance.Limitation, node.NodeInfo.Container);
                var createContainerResponse = await client.Containers.CreateContainerAsync(
                    new CreateContainerParameters()
                    {
                        Tty = true,
                        NetworkDisabled = true,
                        Image = node.NodeInfo.Image,
                        Name = containerTag,
                        HostConfig = hostConfig,
                        WorkingDir = node.NodeInfo.Container.WorkDir,
                        Cmd = new[] { "bash", "-c", node.NodeInfo.Container.ActorExecuteCommand }
                    });
                var containerId = createContainerResponse.ID;
                try
                {
                    // 上传Inputs和actor.dll到容器
                    var inputBlobs = await ReadBlobs(actorInfo.Inputs);
                    var noExistBlob = inputBlobs.FirstOrDefault(x => x.Item2 == null);
                    if (noExistBlob.Item1 != null)
                    {
                        throw new ArgumentException(
                            $"blob with id '{noExistBlob.Item1.Id}' and name '{noExistBlob.Item1.Name}' not found");
                    }
                    if (string.IsNullOrEmpty(actorCode))
                    {
                        throw new ArgumentException($"no code for actor '{actorInfo.Name}'");
                    }
                    inputBlobs = inputBlobs.Concat(new[]
                    {
                        (new BlobInfo(Guid.Empty, node.NodeInfo.Container.ActorExecutablePath), actorBytes)
                    });
                    using (var tarStream = ArchiveUtils.CompressToTar(inputBlobs))
                    {
                        await client.Containers.ExtractArchiveToContainerAsync(
                            containerId,
                            new ContainerPathStatParameters() { Path = node.NodeInfo.Container.WorkDir },
                            tarStream,
                            new CancellationToken());
                    }
                    // 运行容器并等待完毕
                    var startContainerResponse = await client.Containers.StartContainerAsync(
                        containerId, new ContainerStartParameters());
                    if (!startContainerResponse)
                    {
                        throw new InvalidOperationException("start container failed");
                    }
                    var waitContainerResponse = await client.Containers.WaitContainerAsync(
                        containerId, new CancellationToken());
                    // 成功时下载return.json, 失败时下载run-actor.log并抛出里面的内容
                    string resultJson;
                    if (waitContainerResponse.StatusCode == 0)
                    {
                        var getArchiveFromContainerResponse = await client.Containers.GetArchiveFromContainerAsync(
                            containerId,
                            new GetArchiveFromContainerParameters()
                            {
                                Path = node.NodeInfo.Container.WorkDir + node.NodeInfo.Container.ResultPath
                            },
                            false, new CancellationToken());
                        resultJson = await Task.Run(() => Encoding.UTF8.GetString(
                            ArchiveUtils.DecompressFromTar(
                            getArchiveFromContainerResponse.Stream).First().Item2));
                    }
                    else
                    {
                        var getArchiveFromContainerResponse = await client.Containers.GetArchiveFromContainerAsync(
                            containerId,
                            new GetArchiveFromContainerParameters()
                            {
                                Path = node.NodeInfo.Container.WorkDir + node.NodeInfo.Container.ActorExecuteLogPath
                            },
                            false, new CancellationToken());
                        var log = await Task.Run(() => Encoding.UTF8.GetString(
                            ArchiveUtils.DecompressFromTar(
                            getArchiveFromContainerResponse.Stream).First().Item2));
                        throw new ActorExecuteException(log);
                    }
                    // 根据return.json下载文件并插入到blob
                    var result = JsonConvert.DeserializeObject<RunActorResult>(resultJson);
                    var actorOutputs = new List<BlobInfo>();
                    foreach (var output in result.Outputs)
                    {
                        var getArchiveFromContainerResponse = await client.Containers.GetArchiveFromContainerAsync(
                            containerId,
                            new GetArchiveFromContainerParameters()
                            {
                                Path = node.NodeInfo.Container.WorkDir + output
                            },
                            false, new CancellationToken());
                        var bytes = await Task.Run(() => ArchiveUtils.DecompressFromTar(
                            getArchiveFromContainerResponse.Stream).First().Item2);
                        var blobId = await PutBlob(output, bytes, getArchiveFromContainerResponse.Stat.Mtime);
                        actorOutputs.Add(new BlobInfo()
                        {
                            Id = blobId,
                            Name = output
                        });
                    }
                    // 更新actorInfo的Outputs, Status, EndTime, 不更新到数据库
                    actorInfo.Outputs = actorOutputs;
                    actorInfo.Status = ActorStatus.Succeeded;
                    actorInfo.EndTime = DateTime.UtcNow;
                    // 标记节点未发生错误
                    node.ErrorFlags = false;
                }
                finally
                {
                    // 删除容器 (finally)
                    await client.Containers.RemoveContainerAsync(
                        containerId,
                        new ContainerRemoveParameters() { Force = true });
                }
            }
            catch (Exception ex)
            {
                // 如果是连接错误则标记节点发生过错误
                if (DockerNode.IsConnectionError(ex))
                    node.ErrorFlags = true;
                throw;
            }
            finally
            {
                // 释放节点
                _dockerNodeStore.ReleaseNode(node);
            }
        }

        private async Task RunActorInternal(StateMachineBase instance, ActorInfo actorInfo)
        {
            // 请勿直接调用此函数, 此函数不会更新StartedActors
            // 运行actor, 最多重试_actorMaxRetryTimes次 
            Exception lastEx = null;
            for (var x = 0; x < _actorMaxRetryTimes; ++x)
            {
                try
                {
                    await RunActorInternalRetryable(instance, actorInfo);
                    return;
                }
                catch (Exception ex)
                {
                    // 仅在连接错误时允许重试
                    lastEx = ex;
                    if (!DockerNode.IsConnectionError(ex))
                        break;
                }
            }
            // 运行任务失败
            actorInfo.Status = ActorStatus.Failed;
            actorInfo.Exceptions = new[] { lastEx.ToString() };
        }

        public async Task RunActors(StateMachineBase instance, IList<ActorInfo> actorInfos)
        {
            // 更新状态机实例的StartedActors, 更新到数据库
            foreach (var actorInfo in actorInfos)
            {
                instance.StartedActors.Add(actorInfo);
            }
            await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
            {
                instanceEntity.StartedActors = instance.StartedActors;
            });
            // 并列处理
            var childTasks = new List<Task>();
            foreach (var actorInfo in actorInfos)
            {
                childTasks.Add(RunActorInternal(instance, actorInfo));
            }
            // 等待全部完成
            await Task.WhenAll(childTasks);
            // 是否发生错误?
            var anyErrorHappended = false;
            await UpdateInstanceEntity(instance.Id, instance.ExecutionKey, instanceEntity =>
            {
                instanceEntity.StartedActors = instance.StartedActors;
                if (instance.StartedActors.Any(x => x.Status == ActorStatus.Failed))
                {
                    anyErrorHappended = true;
                    instanceEntity.Status = StateMachineStatus.Failed;
                    instanceEntity.Exception = string.Join("\r\n\r\n",
                        instance.StartedActors.SelectMany(a => a.Exceptions));
                }
            });
            if (anyErrorHappended)
            {
                throw new StateMachineInterpretedException();
            }
        }

        public async Task<Guid> PutBlob(string filename, byte[] contents, DateTime timeStamp)
        {
            // 添加单个blob, 返回blob id
            using (var context = _contextFactory())
            {
                var repository = _blobRepositoryFactory(context);
                var blobService = new BlobService(repository);
                return await blobService.Put(new BlobInputDto()
                {
                    Body = Mapper.Map<byte[], string>(contents),
                    TimeStamp = Mapper.Map<DateTime, long>(timeStamp),
                    Remark = filename
                });
            }
        }

        public async Task<IEnumerable<(BlobInfo, byte[])>> ReadBlobs(IEnumerable<BlobInfo> blobInfos)
        {
            // 批量获取blob的内容
            var result = blobInfos.Select(x => ValueTuple.Create(x, (byte[])null)).ToList();
            var blobIds = result.Select(x => x.Item1.Id).ToList();
            using (var context = _contextFactory())
            {
                var repository = _blobRepositoryFactory(context);
                var blobs = await repository.QueryNoTrackingAsync(q => q
                    .Where(x => blobIds.Contains(x.BlobId))
                    .GroupBy(x => x.BlobId)
                    .ToDictionaryAsyncTestable(x => x.Key, x => x.OrderBy(b => b.ChunkIndex).ToList()));
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

        public async Task<string> ReadActorCode(string name)
        {
            // 从缓存获取
            _actorCodeCache.TryGetValue(name, out var codeAndTime);
            if (DateTime.UtcNow - codeAndTime.Item2 <= _actorCodeCacheTime)
            {
                return codeAndTime.Item1;
            }
            // 从数据库获取
            using (var context = _contextFactory())
            {
                var repository = _actorRepositoryFactory(context);
                var actor = await repository.QueryNoTrackingAsync(q =>
                    q.FirstOrDefaultAsyncTestable(x => x.Name == name));
                codeAndTime.Item1 = actor?.Body;
                codeAndTime.Item2 = DateTime.UtcNow;
            }
            // 设置到缓存
            _actorCodeCache[name] = codeAndTime;
            return codeAndTime.Item1;
        }
    }
}

# 简介

构建一个服务, 允许通过多个Docker节点分布式执行任务.

其中服务负责从外部接收请求, 接收到请求后创建状态机, 状态机负责执行任务直到结束.

任务执行过程中可能会有输出的文件, 所有输出的文件都保存到数据库中.

# 总体架构

``` text
                                 +---------------+
                              +-->  Docker Node  |
+---------+     +----------+  |  +---------------+
|         |     |          |  |  +---------------+
| Web Api +---->+ Mgmt Svc +----->  Docker Node  |
|         |     |          |  |  +---------------+
+---------+     +----------+  |  +---------------+
                              +-->  Docker Node  |
                                 +---------------+
```

- 概念
  - StateMachine: 状态机, 负责执行一连串的任务
  - Actor: 任务, 有输入文件和输出文件, 不一定会在同一个容器中执行
  - Blob: 输入或输出文件

- 组成部分
  - Web Api: 负责接收来自外部的请求
  - Mgmt Svc: 负责管理状态机和任务
  - Docker Node: 负责执行任务

Mgmt Svc与Docker Node使用Docker Remote Api通信, Docker Node需要使用自定义的镜像.

# 项目文件

- JoyOI.ManagementService: 管理服务的核心项目
- JoyOI.ManagementService.FunctionalTests: 功能测试项目, 要求节点可以正常连接
- JoyOI.ManagementService.Model: 储存模型类的项目
- JoyOI.ManagementService.Playground: 用于实验性的编写Actor和StateMachine的代码
- JoyOI.ManagementService.Tests: 单元测试项目, 不要求节点可以正常连接
- JoyOI.ManagementService.WebApi: WebApi项目, 提供对外的Http接口

# 配置文档

[配置节点](DeployDockerNode.md)

[配置管理服务(windows)](DeployMgmtSvcWindows.md)

[配置管理服务(linux)](DeployMgmtSvcLinux.md)

# Api一览

以下是管理服务包含的Api, 更详细的格式请在本地打开[swagger](http://localhost:38415/swagger/)查看.

```
get /api/v1/Actor/All
delete /api/v1/Actor/{name}
get /api/v1/Actor/{name}
patch /api/v1/Actor/{name}
put /api/v1/Actor 

get /api/v1/Blob/All
get /api/v1/Blob/{id}
put /api/v1/Blob 

get /api/v1/DockerNode/All 

get /api/v1/StateMachine/All
delete /api/v1/StateMachine/{name}
get /api/v1/StateMachine/{name}
patch /api/v1/StateMachine/{name}
put /api/v1/StateMachine 

get /api/v1/StateMachineInstance/All
delete /api/v1/StateMachineInstance/{id}
get /api/v1/StateMachineInstance/{id}
patch /api/v1/StateMachineInstance/{id}
put /api/v1/StateMachineInstance 
```

# 创建状态机实例时的参数(Parameters)一览

- Host: 完成或错误时提交到的回调地址
- Debug: 值等于true时开启除错模式, 运行的容器不会自动删除

# 注意事项

- 默认配置有限制IO, 如果不想限制请删除BlkioDeviceReadBps和BlkioDeviceWriteBps的所在行
- 同时执行多个Actor请使用DeployAndRunActorsAsync, 否则不能保证线程安全

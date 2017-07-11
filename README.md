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

# 配置节点

环境 Ubuntu 16.04.2 Server LTS

** 生成CA的私钥和公钥 **

这个步骤只需要做一次, 生成的CA证书可以用在所有节点上.

``` text
# 问密码的时候随便填, 但是要记住, 其他项可以忽略
openssl genrsa -aes256 -out ca-key.pem 4096
openssl req -new -x509 -days 36500 -key ca-key.pem -sha256 -out ca.pem
openssl pkcs12 -export -inkey ca-key.pem -in ca.pem -out ca-key.pfx
```

运行后会生成`ca-key.pem ca-key.pfx ca.pem`这三个文件, 都下载到本地.

** 配置Docker节点 **

请确保hostname已设置, 并且hostname指向当前节点的连接IP.

上传以下文件到节点上, 并运行`sudo su -c "sh node-deploy.sh"`

- ca-key.pem
- ca.pem
- node-deploy.sh

# 配置管理服务

**导入CA证书**:

下载所有docker节点生成的ca-key.pfx, 然后双击导入到"本地计算机", 并选择导入到"受信任的根证书颁发机构".

注意检查导入的证书的有效期, 有效期之前请重新生成并更新证书.

**修改网站下的appsettings.json, 例如**:

``` json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=127.0.0.1;Port=3306;Database=joyoi;User Id=root;Password=123456;"
	},
	"Logging": {
		"IncludeScopes": false,
		"LogLevel": {
			"Default": "Debug",
			"System": "Information",
			"Microsoft": "Information"
		}
	},
	"JoyOIManagement": {
		"Name": "Default",
		"Container": {
			"DevicePath": "/dev/sda",
			"MaxRunningJobs": 8,
			"WorkDir": "/workdir/",
			"ActorExecutablePath": "actor/bin/Debug/netcoreapp2.0/actor.dll",
			"ActorExecuteCommand": "sh run-actor.sh &> run-actor.log",
			"ActorExecuteLogPath": "run-actor.log",
			"ResultPath": "return.json"
		},
		"Limitation": {
			"CPUPeriod": 1000000,
			"CPUQuota": 1000000,
			"Memory": 268435456,
			"MemorySwap": 268435456,
			"BlkioDeviceReadBps": 33554432,
			"BlkioDeviceWriteBps": 33554432
		},
		"Nodes": {
			"docker-1": {
				"Image": "joyoi",
				"Address": "http://docker-1:2376",
				"ClientCertificatePath": "ClientCerts/docker-1.pfx",
				"ClientCertificatePassword": "123456"
			},
			"docker-2": {
				"Image": "joyoi",
				"Address": "http://docker-2:2376",
				"ClientCertificatePath": "ClientCerts/docker-2.pfx",
				"ClientCertificatePassword": "123456"
			}
		}
	}
}
```

配置说明:

- "Name": 管理服务的名称, 如果要配置多个管理服务必须使用不同的名称
- "Container": 容器相关的配置
  - "DevicePath": 主设备路径
  - "MaxRunningJobs": 单个节点可以同时运行的任务数量
  - "WorkDir": 容器中的工作目录路径, 需要以"/"结尾
  - "ActorCodePath": 任务代码的路径, 相对于工作目录
  - "ActorExecuteCommand": 执行任务的命令
  - "ActorExecuteLogPath": 执行任务的记录文件
  - "ResultPath": 执行任务的结果文件
- "Limitation": 运行任务时对容器的限制
  - "CPUPeriod": 限制CPU时使用的间隔时间, 单位是微秒, 默认是1秒 = 1000000
  - "CPUQuota": 限制CPU在间隔时间内可以使用的时间, 单位是微秒, 设置为跟CPUPeriod一致时表示只能用一个核心
  - "Memory": 可以使用的内存, 单位是字节, 默认无限制
  - "MemorySwap": 可以使用的交换内存, 单位是字节, 默认是Memory的两倍, 设为0时等于默认值(Memory的两倍)
  - "BlkioDeviceReadBps": 一秒最多读取的字节数, 单位是字节, 默认无限制
  - "BlkioDeviceWriteBps": 一秒最多写入的字节数, 单位是字节, 默认无限制
- "Nodes"是docker节点列表
  - "Image": docker镜像的名称, 自己构建的镜像是"joyoi", 从hub下载的镜像是"yuko/joyoi"
  - "Address": 节点的地址
  - "ClientCertificatePath": 客户端证书的路径
  - "ClientCertificatePassword": 客户端证书的密码
  - "Container": 阶段单独的容器配置, 可以等于null也可以只设置部分属性, 不设置的属性会使用上面的值

**存放客户端证书**

下载所有docker节点生成的"/root/.docker/key.pfx"到上面配置的"ClientCertificatePath"属性对应的目录下.

**配置WebApi**

TODO

https://stackoverflow.com/questions/8309780/does-iis-do-the-ssl-certificate-check-or-do-i-have-to-verify-it
https://blogs.msdn.microsoft.com/bradleycotier/2011/12/14/mutual-authentication-with-a-iis-hosted-wcf-data-service-installed-in-a-workgroup-environment/
https://blogs.msdn.microsoft.com/asiatech/2014/02/12/how-to-configure-iis-client-certificate-mapping-authentication-for-iis7/


# Api一览

TODO

# 注意事项

- 默认配置有限制IO, 如果不想限制请删除BlkioDeviceReadBps和BlkioDeviceWriteBps的所在行
- 同时执行多个Actor请使用DeployAndRunActorsAsync, 否则不能保证线程安全

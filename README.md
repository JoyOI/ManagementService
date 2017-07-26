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

**生成CA的私钥和公钥**

这个步骤只需要做一次, 生成的CA证书可以用在所有节点上.

``` text
# 问密码的时候随便填, 但是要记住, 其他项可以忽略
openssl genrsa -aes256 -out ca-key.pem 4096
openssl req -new -x509 -days 36500 -key ca-key.pem -sha256 -out ca.pem
openssl pkcs12 -export -inkey ca-key.pem -in ca.pem -out ca-key.pfx
```

运行后会生成`ca-key.pem ca-key.pfx ca.pem`这三个文件, 都下载到本地.

**配置Docker节点**

请确保hostname已设置, 并且hostname指向当前节点的连接IP.

上传以下文件到节点上, 并运行`sudo su -c "sh node-deploy.sh"`

- ca-key.pem
- ca.pem
- node-deploy.sh (在NodeDeployment文件夹下)

# 配置管理服务

**导入CA证书**:

下载所有docker节点生成的ca-key.pfx, 然后双击导入到"本地计算机", 并选择导入到"受信任的根证书颁发机构".

注意检查导入的证书的有效期, 有效期之前请重新生成并更新证书.

**保存WebApi服务端和客户端证书**

使用以下命令生成WebApi使用的服务端和客户端证书, CA可以用上面生成的

``` text
export domain=mgmtsvc.1234.sh

# 生成WebApi服务器的私钥和公钥, 并使用CA签名
openssl genrsa -out webapi-server-key.pem 2048
openssl req -subj "/CN=${domain}" -sha256 -new -key webapi-server-key.pem -out webapi-server.csr
echo "subjectAltName = DNS:${domain}" > extfile.cnf
openssl x509 -req -days 36500 -sha256 -in webapi-server.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out webapi-server-cert.pem -extfile extfile.cnf
openssl pkcs12 -export -inkey webapi-server-key.pem -in webapi-server-cert.pem -out webapi-server.pfx

# 生成WebApi客户端的私钥和公钥，并使用CA签名
openssl genrsa -out webapi-client-key.pem 2048
openssl req -subj "/CN=${domain}" -sha256 -new -key webapi-client-key.pem -out webapi-client.csr
echo "subjectAltName = DNS:${domain}" > extfile.cnf
openssl x509 -req -days 36500 -sha256 -in webapi-client.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out webapi-client-cert.pem -extfile extfile.cnf
openssl pkcs12 -export -inkey webapi-client-key.pem -in webapi-client-cert.pem -out webapi-client.pfx
```

下载生成的"webapi-server.pfx"到下面的配置中的"Kestrel"下的"ServerCertificatePath"对应的路径.<br/>
然后下载生成的"webapi-client.pfx", 给调用管理服务的客户端使用.<br/>
管理服务验证WebApi客户端证书需要导入CA, 请先导入CA到"受信任的根证书颁发机构".<br/>

**保存节点客户端证书**

下载所有docker节点生成的"/root/.docker/key.pfx"到下面配置中"Nodes"下的"ClientCertificatePath"对应的路径.

**修改网站下的appsettings.json, 如下**:

``` json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=127.0.0.1;Port=3306;Database=joyoi;User Id=root;Password=123456;"
	},
	"Logging": {
		"IncludeScopes": false,
		"LogLevel": {
			"Default": "Debug",
			"System": "Warning",
			"Microsoft": "Warning"
		}
	},
	"Kestrel": {
		"HttpsListenPort": 443,
		"ServerCertificatePath": "WebApiCerts/webapi-server.pfx",
		"ServerCertificatePassword": "123456"
	},
	"JoyOIManagement": {
		"Name": "Default",
		"Container": {
			"DevicePath": "/dev/sda",
			"MaxRunningJobs": 8,
			"WorkDir": "/workdir/",
			"ActorExecutablePath": "/actor/bin/Debug/netcoreapp2.0/actor.dll",
			"ActorExecuteCommand": "/actor/run-actor.sh &> /actor/run-actor.log",
			"ActorExecuteLogPath": "/actor/run-actor.log",
			"ResultPath": "/workdir/return.json"
		},
		"Limitation": {
			"CPUPeriod": 10000,
			"CPUQuota": 10000,
			"Memory": 536870912,
			"MemorySwap": 1,
			"BlkioDeviceReadBps": 33554432,
			"BlkioDeviceWriteBps": 33554432,
			"ExecutionTimeout": 30000,
			"Ulimit": {
				"memlock": 8196,
				"core": 8196,
				"nofile": 512,
				"cpu": 30,
				"nproc": 32,
				"locks": 1000,
				"sigpending": 100,
				"msgqueue": 100,
				"nice": 100,
				"rtprio": 100
			}
		},
		"Nodes": {
			"docker-1": {
				"Image": "yuko/joyoi",
				"Address": "http://remote-docker-1:2376",
				"ClientCertificatePath": "ClientCerts/remote-docker-1.pfx",
				"ClientCertificatePassword": "123456"
			},
			"docker-2": {
				"Image": "yuko/joyoi",
				"Address": "http://remote-docker-2:2376",
				"ClientCertificatePath": "ClientCerts/remote-docker-2.pfx",
				"ClientCertificatePassword": "123456"
			}
		}
	}
}
```

配置说明:

- "Kestrel": 生产环境的配置
  - "HttpsListenPort": 监听的端口
  - "ServerCertificatePath": WebApi服务端证书的路径
  - "ServerCertificatePassword": WebApi服务端证书的密码
- "JoyOIManagement": 管理服务的配置
  - "Name": 管理服务的名称, 如果要配置多个管理服务必须使用不同的名称
  - "Container": 容器相关的配置
    - "DevicePath": 主设备路径
    - "MaxRunningJobs": 单个节点可以同时运行的任务数量
    - "WorkDir": 容器中的工作目录路径, 需要以"/"结尾
    - "ActorExecutablePath": 任务可执行文件的路径
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
    - "ExecutionTimeout": 容器最长可以执行的时间, 单位是毫秒, 默认无限制
      - 这个限制用于防止容器因为不可预料的原因无限期运行, 设置后所有任务都必须在这个时间内完成
    - "Ulimit": Ulimit限制, 详细参考[这个地址](http://man7.org/linux/man-pages/man2/getrlimit.2.html)
      - 例如限制RLIMIT_CPU, 可以设置`"cpu": 30`, 表示进程占用的cpu时间最多30秒, 超过时杀死
  - "Nodes"是docker节点列表
    - "Image": docker镜像的名称, 自己构建的镜像是"joyoi", 从hub下载的镜像是"yuko/joyoi"
    - "Address": 节点的地址
    - "ClientCertificatePath": 客户端证书的路径
    - "ClientCertificatePassword": 客户端证书的密码
    - "Container": 节点单独的容器配置, 可以等于null也可以只设置部分属性, 不设置的属性会使用上面的值

**常驻服务**

从"[http://www.nssm.cc/download](http://www.nssm.cc/download)"下载添加服务的工具,<br/>
假定管理服务在"C:\inetpub\mgmtsvc"下, 执行以下命令:

``` text
nssm install joyoi_mgmtsvc "C:\Program Files\dotnet\dotnet.exe" "JoyOI.ManagementService.WebApi.dll"
nssm set joyoi_mgmtsvc AppDirectory "C:\inetpub\mgmtsvc"
nssm set joyoi_mgmtsvc ObjectName LocalSystem
nssm set joyoi_mgmtsvc Start SERVICE_AUTO_START
nssm set joyoi_mgmtsvc AppThrottle 1500
nssm set joyoi_mgmtsvc AppExit Default Restart
nssm set joyoi_mgmtsvc AppRestartDelay 0
nssm set joyoi_mgmtsvc AppStdout "C:\inetpub\mgmtsvc\stdout.log"
nssm set joyoi_mgmtsvc AppStderr "C:\inetpub\mgmtsvc\stderr.log"
nssm start joyoi_mgmtsvc
```

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

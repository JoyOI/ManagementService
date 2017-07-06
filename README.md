﻿# 简介

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

- JoyOI.ManagementService
  - 管理服务的核心项目
- JoyOI.ManagementService.FunctionalTests
  - 功能测试项目, 要求节点可以正常连接
- JoyOI.ManagementService.Model
  - 储存模型类的项目
- JoyOI.ManagementService.Tests
  - 单元测试项目, 不要求节点可以正常连接
- JoyOI.ManagementService.WebApi
  - WebApi项目, 提供对外的Http接口

# 配置节点

环境 Ubuntu 16.04.2 Server LTS

运行命令

```
sudo su
apt-get install curl software-properties-common

############ 安装docker ############
# https://docs.docker.com/engine/installation/linux/docker-ce/ubuntu
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
apt-get update
apt-get install docker-ce

# 拉取镜像
docker pull daocloud.io/ubuntu:16.04

############ 设置docker远程api ############
# 请确保hostname和hostname指向的ip地址都正确设置
# https://docs.docker.com/engine/security/https
mkdir tmp
cd tmp

# 生成CA的私钥和公钥
# 问密码的时候随便填, 但是要记住
openssl genrsa -aes256 -out ca-key.pem 4096
# 会问很多东西, 只需要在"Common Name"上写主机的名称, 其他项可以忽略
openssl req -new -x509 -days 365 -key ca-key.pem -sha256 -out ca.pem

# 生成服务器的私钥和公钥, 并使用CA签名
openssl genrsa -out server-key.pem 4096
openssl req -subj "/CN=$(hostname)" -sha256 -new -key server-key.pem -out server.csr
# 允许连接使用hostname指向的ip和127.0.0.1
echo "subjectAltName = DNS:$(hostname),IP:$(hostname -i),IP:127.0.0.1" > extfile.cnf
openssl x509 -req -days 36500 -sha256 -in server.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out server-cert.pem -extfile extfile.cnf

# 生成客户端的私钥和公钥，并使用CA签名
openssl genrsa -out key.pem 4096
openssl req -subj '/CN=client' -new -key key.pem -out client.csr
echo extendedKeyUsage = clientAuth > client-extfile.cnf
openssl x509 -req -days 36500 -sha256 -in client.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out cert.pem -extfile client-extfile.cnf

# 让服务端使用证书, 这里未设置私钥的所有者和权限, 如果节点有多用户请自行设置
mkdir -p /etc/docker/cert.d
cp * /etc/docker/cert.d
sed -i "s/-H fd:\/\///g" /lib/systemd/system/docker.service
systemctl daemon-reload
echo '{ "tlsverify": true, "tlscacert": "/etc/docker/cert.d/ca.pem", "tlscert": "/etc/docker/cert.d/server-cert.pem", "tlskey": "/etc/docker/cert.d/server-key.pem", "hosts": [ "unix:///var/run/docker.sock", "tcp://0.0.0.0:2376" ] }' > /etc/docker/daemon.json
systemctl stop docker
systemctl start docker
systemctl status docker

# 让客户端使用证书, 可选, 仅测试使用
mkdir -pv ~/.docker
cp -v {ca,cert,key}.pem ~/.docker

# 测试客户端证书, 如果输出正常则表示配置成功
docker --tlsverify -H="tcp://$(hostname):2376" images

# 删除临时文件夹
cd ..
rm -rfv tmp

# 生成管理服务用的客户端证书, 生成时会问密码, 记住这个密码
cd ~/.docker
openssl pkcs12 -export -inkey key.pem -in cert.pem -out key.pfx

# 生成管理服务用的CA证书, 生成时会问密码, 记住这个密码
cd /etc/docker/cert.d
openssl pkcs12 -export -inkey ca-key.pem -in ca.pem -out ca-key.pfx

############ 构建docker镜像 ############
# 你可以选择拖取hub上的镜像, 或者自己构建

# 自己构建的步骤
# 上传 Dockerfile 和 runner 到 /root/docker 下
# cd ~/docker
# docker build -t joyoi .

# 拖取hub上的镜像的步骤
docker pull yuko/joyoi

# 完成后确认本地的镜像列表
docker images
```

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
			"Default": "Warning"
		}
	},
	"JoyOIManagement": {
		"Name": "Default",
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

"Name"是管理服务的名称, 如果要配置多个管理服务必须使用不同的名称

"Image"是docker镜像的名称, 自己构建的镜像是"joyoi", 从hub下载的镜像是"yuko/joyoi".

"Address"是节点的地址.

"ClientCertificatePath"是客户端证书的路径.

"ClientCertificatePassword"是客户端证书的密码.

**存放客户端证书**

下载所有docker节点生成的key.pfx, 放到上面配置的"ClientCertificatePath"属性对应的目录下.

### Webapi的客户端验证

TODO

# Api一览

TODO

# 注意事项

TODO

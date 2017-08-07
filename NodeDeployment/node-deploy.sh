#!/usr/bin/env bash

apt-get update
apt-get install curl software-properties-common -y

############ 安装docker ############
# https://docs.docker.com/engine/installation/linux/docker-ce/ubuntu
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
apt-get update
apt-get install docker-ce -y

# 拉取镜像
docker pull daocloud.io/ubuntu:16.04

############ 设置docker远程api ############
# 请确保hostname已设置, 并且hostname指向当前节点的公网IP
# https://docs.docker.com/engine/security/https
rm -rf tmp
mkdir tmp
mv ca.pem ca-key.pem tmp
cd tmp

# 生成服务器的私钥和公钥, 并使用CA签名
openssl genrsa -out server-key.pem 2048
openssl req -subj "/CN=$(hostname)" -sha256 -new -key server-key.pem -out server.csr
# 允许连接使用hostname指向的ip和127.0.0.1
echo "subjectAltName = DNS:$(hostname)" > extfile.cnf
openssl x509 -req -days 36500 -sha256 -in server.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out server-cert.pem -extfile extfile.cnf

# 生成客户端的私钥和公钥，并使用CA签名
openssl genrsa -out key.pem 2048
openssl req -subj '/CN=client' -new -key key.pem -out client.csr
echo extendedKeyUsage = clientAuth > client-extfile.cnf
openssl x509 -req -days 36500 -sha256 -in client.csr -CA ca.pem -CAkey ca-key.pem \
  -CAcreateserial -out cert.pem -extfile client-extfile.cnf

# 让服务端使用证书, 这里未设置私钥的所有者和权限, 如果节点有多用户请自行设置
mkdir -p /etc/docker/cert.d
cp -fv * /etc/docker/cert.d
sed -i "s/-H fd:\/\///g" /lib/systemd/system/docker.service
systemctl daemon-reload
echo '{ "tlsverify": true, "tlscacert": "/etc/docker/cert.d/ca.pem", "tlscert": "/etc/docker/cert.d/server-cert.pem", "tlskey": "/etc/docker/cert.d/server-key.pem", "hosts": [ "unix:///var/run/docker.sock", "tcp://0.0.0.0:2376" ] }' > /etc/docker/daemon.json
systemctl stop docker
systemctl start docker
# systemctl status docker

# 让客户端使用证书, 可选, 仅测试使用
mkdir -pv ~/.docker
cp -fv ca.pem cert.pem key.pem ~/.docker

# 测试客户端证书, 如果输出正常则表示配置成功
docker --tlsverify -H="tcp://$(hostname):2376" images

# 删除临时文件夹
cd ..
rm -rfv tmp

# 生成管理服务用的客户端证书, 生成时会问密码, 记住这个密码
cd ~/.docker
openssl pkcs12 -export -inkey key.pem -in cert.pem -out key.pfx

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

# 配置节点

环境 Ubuntu 16.04 Server LTS

### 生成CA的私钥和公钥

这个步骤只需要做一次, 生成的CA证书可以用在所有节点上.

``` text
# 问密码的时候随便填, 但是要记住, 其他项可以忽略
openssl genrsa -aes256 -out ca-key.pem 4096
openssl req -new -x509 -days 36500 -key ca-key.pem -sha256 -out ca.pem
openssl pkcs12 -export -inkey ca-key.pem -in ca.pem -out ca-key.pfx
```

运行后会生成`ca-key.pem ca-key.pfx ca.pem`这三个文件, 都下载到本地.

### 配置Docker节点

请确保hostname已设置, 并且hostname指向当前节点的连接IP.

上传以下文件到节点上, 并运行`sudo su -c "sh node-deploy.sh"`

- ca-key.pem
- ca.pem
- node-deploy.sh (在NodeDeployment文件夹下)

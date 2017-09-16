# 配置管理服务

之前的步骤跟[配置管理服务(windows)](DeployMgmtSvcWindows.md)基本相同, 这里仅列出不同的部分.

### 导入证书(linux)

因为Docker节点的证书是手动颁发的, 需要导入前面生成的CA证书:

``` text
# ca.pem是配置节点时生成的, 注意目标文件的后缀一定要crt
cp ca.pem /usr/local/share/ca-certificates/ca.crt

# 更新证书仓库, 注意成功会提示'1 added'
update-ca-certificates
```

### 常驻服务(linux)

添加新的systemd服务:

``` text
vim /etc/systemd/system/mgmtsvc.service
```

粘贴以下内容, 注意修改网站的路径:

``` text
[Unit]
Description=Joyoi MgmtSvc

[Service]
WorkingDirectory=/home/joyoi/mgmtsvc
ExecStart=/usr/bin/dotnet /home/joyoi/mgmtsvc/JoyOI.ManagementService.WebApi.dll
Restart=always
RestartSec=10
SyslogIdentifier=joyoi-mgmtsvc
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install]
WantedBy=multi-user.target
```

设置服务自动启动:

``` text
systemctl enable mgmtsvc.service
```

启动服务:

``` text
systemctl start mgmtsvc.service
```

查看服务状态:

``` text
systemctl status mgmtsvc.service
```

查看服务日志:

``` text
journalctl -fu mgmtsvc.service
```

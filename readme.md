sdk验证地址的域名是`as.arcsoftai.com` 对应的DNS是`114.114.114.114`（http请求）

网络检查分为两个阶段。在进行http请求前，会先测试设备是否能连接公网，策略是尝试`telnet`以下DNS解析服务器：<br/>
`114.114.114.114 53，14.215.177.38 80，122.228.95.106 80，8.8.8.8 53` 优先级从上至下，有其一能连通则认为能上公网。<br/>
若设备能上公网，则向`as.arcsoftai.com`进行http请求。所以，如果连不上公网，请尝试将`114.114.114.114`和域名`as.arcsoftai.com`添加到白名单。


# Linux 启动方式

Net8 依赖 libicu
虹软 依赖 glibc 2.17 及以上
Yang

安装依赖环境：
CentOS 8.2 以上
```
yum install libicu
yum install glibc 
```

ubuntu 20.04 不用额外安装依赖
```
//apt update
//apt install libicu
//apt install glibc 
```

设置虹软库文件软连接
```
ln -s /root/FaceCheck.Server/ArcLib/4.0/libarcsoft_face_engine.so /usr/lib64/libarcsoft_face_engine.so
ln -s /root/FaceCheck.Server/ArcLib/4.0/libarcsoft_face.so /usr/lib64/libarcsoft_face.so

ln -s /root/FaceCheck.Server/ArcLib/4.0/libarcsoft_face_engine.so /usr/lib/libarcsoft_face_engine.so
ln -s /root/FaceCheck.Server/ArcLib/4.0/libarcsoft_face.so /usr/lib/libarcsoft_face.so

```

后台静默持续运行 且丢弃命令行输出数据
```
 nohup ./FaceCheck.Server >/dev/null &
```

设置系统服务 (开机自启动等)
创建`FaceCheck.service`文件
```
[Unit]
Description=扣脸及人证对比服务
After=network.target
 
[Service]
Type=simple
WorkingDirectory=/root/FaceCheck.Server
ExecStart=/root/FaceCheck.Server/FaceCheck.Server
Restart=always
 
[Install]
WantedBy=multi-user.target
```
创建`FaceCheck.service`软连接
```
ln -s /root/FaceCheck.Server/FaceCheck.service /etc/systemd/system/FaceCheck.service
```

相关命令
```
sudo systemctl daemon-reload  # 刷新服务加载

sudo systemctl start FaceCheck.service  # 启动服务

sudo systemctl restart FaceCheck.service  # 重启服务

sudo systemctl stop FaceCheck.service  # 停止服务

sudo systemctl status FaceCheck  # 查看服务状态
```

查看目录
进入日志目录
```
tail -f 2025.log
```

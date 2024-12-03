sdk验证地址的域名是as.arcsoftai.com 对应的DNS是114.114.114.114（http请求）
网络检查分为两个阶段。在进行http请求前，会先测试设备是否能连接公网，策略是尝试telnet以下DNS解析服务器：114.114.114.114 53，14.215.177.38 80，122.228.95.106 80，8.8.8.8 53，优先级从上至下，有其一能连通则认为能上公网。若设备能上公网，则向as.arcsoftai.com进行http请求。所以，如果连不上公网，请尝试将（114.114.114.114）和域名（as.arcsoftai.com）添加到白名单。

免费版47.102.100.237（80端口）
47.103.64.115

# Linux 启动方式

Net8 依赖 libicu
虹软 依赖 glibc 2.17 及以上
Yang

安装依赖环境：
CentOS 8.2 以上
yum install libicu
yum install glibc 

ubuntu 20.04 不用额外安装依赖
//apt update
//apt install libicu
//apt install glibc 

设置虹软库文件软连接

cp -Rf /usr/Projects/FaceCheck.Server/ArcLib/Sox64/* /usr/lib64
cp -Rf /usr/Projects/FaceCheck.Server/ArcLib/Sox64/* /usr/lib


后台静默持续运行 且丢弃命令行输出数据
 nohup ./FaceCheck.Server >/dev/null &

# ctwebplayer

这个项目的目的是用于给Unity3D WebPlayer 做一个基于webview2的专属浏览器。增加一些缓存功能，避免频繁的访问网络静态资源，用于提速。

缓存文件存储在./cache目录下 每次下载静态资源的时候。先查看缓存文件是否存在，如果不存在就下载，并且保存到./cache目录。目录结构跟下载地址的相对URL一一对应。

原始网页地址 https://game.ero-labs.live/cn/cloud_game.html?id=27&connect_type=1&connection_id=20

下载的时候需要支持代理服务器的设置。 默认的代理服务器要存到配置文件里。比如 ./config.json
默认的代理服务器  http_proxy=http://127.0.0.1:7890  https_proxy=http://127.0.0.1:7890 socks5=127.0.0.1:7890


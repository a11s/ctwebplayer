# CTWebPlayer - Unity3D WebPlayer 专属浏览器

## 项目介绍

CTWebPlayer 是一个为 Unity3D WebPlayer 游戏开发的基于 WebView2 的专属浏览器工具。这是一个**纯粹的浏览器优化工具**，专注于提升游戏加载速度和用户体验，不会修改游戏代码，也不会改变任何内存数据。

### 核心特性
- **智能缓存系统**：自动缓存游戏静态资源，避免重复下载，显著提升加载速度
- **代理服务器支持**：支持 HTTP、HTTPS 和 SOCKS5 代理配置
- **窗口管理**：可自定义窗口大小，支持全屏显示 Unity 画布
- **请求日志记录**：详细记录所有资源请求，便于分析和调试
- **CORS 问题处理**：自动处理跨域请求问题，确保游戏正常运行

## 主要功能

### 1. 智能缓存系统
- **自动缓存管理**：根据预设规则自动缓存静态资源
- **缓存命中率统计**：实时显示缓存命中率和缓存大小
- **缓存清理功能**：支持一键清理所有缓存
- **离线游戏支持**：缓存后可在无网络环境下运行游戏

### 2. 代理服务器支持
- 支持 HTTP/HTTPS 代理
- 支持 SOCKS5 代理
- 可随时启用或禁用代理
- 代理设置自动保存

### 3. 窗口管理
- 可自定义窗口大小（默认 1236x740）
- 自动处理 Unity canvas 全屏显示
- 窗口设置自动保存

### 4. 请求日志记录
- 详细记录每个资源请求的状态（命中/未命中/错误）
- 记录文件大小和下载时间
- 支持日志查看器，方便分析
- 可配置日志级别和文件大小限制

### 5. 其他实用功能
- **自动 iframe 导航**：自动检测并导航到游戏 iframe 内容
- **CORS 处理**：自动注入脚本处理跨域请求问题
- **导航控制**：支持后退、前进、刷新等基本浏览器功能
- **地址栏**：可手动输入和导航到指定 URL
- **状态栏**：实时显示加载状态和缓存统计信息

## 缓存规则

CTWebPlayer 支持两种缓存规则：

### 1. 基于文件扩展名的缓存
支持以下扩展名的文件自动缓存：
- **脚本文件**：`.js`
- **样式文件**：`.css`
- **图片文件**：`.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.svg`, `.ico`
- **字体文件**：`.woff`, `.woff2`, `.ttf`, `.eot`, `.otf`
- **Unity 资源**：`.data`, `.wasm`

### 2. 基于路径的缓存
- 自动缓存 `/patch/files/` 路径下的所有文件
- 适用于没有标准扩展名但需要缓存的 CDN 资源文件
- 例如：`https://hefc.hrbzqy.com/patch/files/assets_resources_honly_baseassets_uiprefabs_simpleprefab_img_hcg_dynamic_a002_02_hcg1_1_spine.498fc09afb981f5ce4a345b5f32d70ae`

缓存文件存储在 `./cache` 目录下，目录结构与下载地址的相对 URL 一一对应。

## 配置说明

### 配置文件位置
配置文件位于程序根目录：`./config.json`

### 配置文件示例
```json
{
  "proxy": {
    "enabled": false,
    "http_proxy": "http://127.0.0.1:7890",
    "https_proxy": "http://127.0.0.1:7890",
    "socks5": "127.0.0.1:7890"
  },
  "logging": {
    "enabled": true,
    "logLevel": "Info",
    "maxFileSize": 10485760
  },
  "enableAutoIframeNavigation": true,
  "ui": {
    "windowWidth": 1236,
    "windowHeight": 740
  },
  "debugMode": false
}
```

### 配置项说明
- **proxy**：代理服务器设置
  - `enabled`：是否启用代理
  - `http_proxy`：HTTP 代理地址
  - `https_proxy`：HTTPS 代理地址
  - `socks5`：SOCKS5 代理地址
- **logging**：日志设置
  - `enabled`：是否启用日志
  - `logLevel`：日志级别（Debug/Info/Warning/Error）
  - `maxFileSize`：日志文件最大大小（字节）
- **enableAutoIframeNavigation**：是否自动导航到 iframe 内容
- **ui**：界面设置
  - `windowWidth`：窗口宽度
  - `windowHeight`：窗口高度
- **debugMode**：是否启用调试模式

## 使用方法

### 启动程序
1. 确保已安装 WebView2 运行时（见系统要求）
2. 双击运行 `ctwebplayer.exe`
3. 程序将自动加载默认游戏地址

### 基本操作
- **导航控制**
  - 点击"后退"按钮返回上一页
  - 点击"前进"按钮前进到下一页
  - 点击"刷新"按钮重新加载当前页面
- **地址栏**
  - 在地址栏输入 URL 并按回车键或点击"转到"按钮进行导航
- **设置**
  - 点击"设置"按钮打开设置窗口
  - 可配置代理服务器、窗口大小、日志等选项
- **缓存管理**
  - 在设置窗口中可查看缓存统计信息
  - 可执行清理缓存操作

### 快捷键
- `Enter`：在地址栏中按下回车键进行导航
- `F5`：刷新当前页面（WebView2 内置）
- `F12`：打开开发者工具（WebView2 内置）

## 系统要求

### 操作系统
- Windows 10 或更高版本
- Windows Server 2016 或更高版本

### 运行时要求
- **WebView2 运行时**（必需）
  - 如果系统未安装，程序会提示安装
  - 下载地址：https://developer.microsoft.com/microsoft-edge/webview2/

### 硬件要求
- 处理器：1 GHz 或更快
- 内存：512 MB RAM（建议 2 GB 或更多）
- 存储空间：至少 200 MB 可用空间（用于程序和缓存）

## 文件结构
```
ctwebplayer/
├── ctwebplayer.exe          # 主程序
├── config.json             # 配置文件
├── cache/                  # 缓存目录
├── logs/                   # 日志目录
└── data/                   # WebView2 用户数据目录
```

## 注意事项
1. 本工具仅用于优化浏览器加载体验，不会修改任何游戏内容
2. 缓存功能仅适用于静态资源，API 请求不会被缓存
3. 首次运行需要下载资源，之后的加载速度会显著提升
4. 定期清理缓存可以释放磁盘空间

## 许可证
本项目使用 MIT 许可证。详见 LICENSE 文件。

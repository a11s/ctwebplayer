# CTWebPlayer 浏览器插件

CTWebPlayer Resource Cache 是一个专为云游戏设计的浏览器插件，通过智能缓存机制显著提升游戏加载速度和用户体验。

## 功能特性

### 🚀 核心功能
- **智能资源缓存**：自动缓存游戏资源，减少重复下载
- **跨域资源拦截**：突破 iframe 限制，缓存所有游戏资源
- **Native Messaging 集成**：与 CTWebPlayer 客户端无缝协作
- **实时性能监控**：显示缓存命中率、节省流量等统计信息
- **Unity WebGL 支持**：完整支持 .data 和 .wasm 文件缓存，优化游戏加载体验

### 💡 高级特性
- **LRU 缓存策略**：智能管理缓存空间，自动清理过期资源
- **自定义缓存规则**：可配置不同类型资源的缓存策略
- **离线游戏支持**：缓存的资源可在离线状态下使用
- **性能优化**：使用 IndexedDB 存储大型资源，提升读写性能

## 安装指南

### 开发版安装

1. **下载插件文件**
   ```bash
   git clone https://github.com/ctwebplayer/extension.git
   cd extension/browser-extension
   ```

2. **生成图标文件**
   - 使用 `icons/icon.svg` 生成所需的 PNG 图标
   - 详见 `icons/README.md` 中的说明

3. **加载到浏览器**
   - 打开 Chrome/Edge 浏览器
   - 访问 `chrome://extensions/`（或 `edge://extensions/`）
   - 开启"开发者模式"
   - 点击"加载已解压的扩展程序"
   - 选择 `browser-extension` 文件夹

### 正式版安装

1. 访问 [Chrome Web Store](https://chrome.google.com/webstore)
2. 搜索 "CTWebPlayer Resource Cache"
3. 点击"添加至 Chrome"

## 使用说明

### 基本使用

1. **首次使用**
   - 安装插件后，点击工具栏图标查看状态
   - 确保 CTWebPlayer 客户端已安装并运行

2. **自动缓存**
   - 访问云游戏网站时，插件会自动开始缓存资源
   - 可在弹出窗口查看缓存统计信息

3. **手动管理**
   - 点击"清除缓存"按钮可清空所有缓存
   - 在设置中可调整缓存规则

### 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Shift+C` | 显示/隐藏性能统计悬浮窗 |
| `Ctrl+Shift+X` | 快速清除缓存 |
| `Ctrl+Shift+S` | 打开设置页面 |

### 缓存规则配置

在弹出窗口中可以配置以下缓存规则：

#### 基于文件扩展名的缓存
- **图片资源**：JPG、PNG、WebP 等图片文件
- **脚本文件**：JavaScript、CSS 文件
- **字体文件**：WOFF、TTF 等字体文件
- **媒体文件**：MP4、MP3 等音视频文件
- **Unity WebGL 资源**：.data、.wasm 文件（支持大型游戏资源缓存）

#### 基于路径的缓存（新增）
- **CDN 资源路径**：自动缓存 `/patch/files/` 路径下的所有文件
- **适用场景**：没有标准扩展名的游戏资源文件
- **示例**：`https://hefc.hrbzqy.com/patch/files/assets_resources_honly_baseassets_uiprefabs_simpleprefab_img_hcg_dynamic_a002_02_hcg1_1_spine.498fc09afb981f5ce4a345b5f32d70ae`

## 技术架构

### 文件结构
```
browser-extension/
├── manifest.json          # 插件配置文件
├── background.js          # Service Worker 后台脚本
├── content.js            # 内容脚本
├── popup.html            # 弹出窗口页面
├── popup.js              # 弹出窗口脚本
├── popup.css             # 弹出窗口样式
├── config/
│   └── constants.js      # 常量配置
├── utils/
│   ├── cache-manager.js  # 缓存管理器
│   ├── storage.js        # 存储工具
│   └── native-messaging.js # 原生消息通信
└── icons/                # 图标文件
```

### 核心组件

1. **Background Service Worker**
   - 管理插件生命周期
   - 拦截和缓存网络请求
   - 处理 Native Messaging 通信

2. **Content Script**
   - 注入到云游戏页面
   - 监控 iframe 加载
   - 收集性能数据

3. **Cache Manager**
   - 使用 IndexedDB 存储资源
   - 实现 LRU 缓存淘汰策略
   - 管理缓存元数据

4. **Native Messaging**
   - 与 CTWebPlayer 通信
   - 接收登录成功通知
   - 自动导航到游戏页面

## 开发指南

### 环境要求
- Node.js 14+
- Chrome/Edge 88+
- CTWebPlayer 客户端

### 调试方法

1. **查看后台日志**
   - 在扩展管理页面点击"Service Worker"
   - 打开开发者工具查看控制台

2. **调试内容脚本**
   - 在游戏页面打开开发者工具
   - 查看控制台中的 "CTWebPlayer" 相关日志

3. **测试 Native Messaging**
   - 确保 Native Host 已正确注册
   - 检查 `chrome://extensions` 中的错误信息

### 构建发布

1. **打包插件**
   ```bash
   # 安装依赖
   npm install
   
   # 构建生产版本
   npm run build
   
   # 生成 CRX 文件
   npm run package
   ```

2. **发布到商店**
   - 准备商店截图和描述
   - 上传到 Chrome Web Store 开发者控制台
   - 等待审核通过

## 常见问题

### Q: 插件无法连接到 CTWebPlayer？
A: 请检查：
- CTWebPlayer 是否已安装并运行
- Native Host 是否正确注册
- 防火墙是否阻止了通信

### Q: 缓存没有生效？
A: 可能原因：
- 网站使用了防缓存机制
- 资源 URL 包含动态参数
- 缓存空间已满

### Q: 如何查看缓存内容？
A: 可以通过以下方式：
- 点击插件图标查看统计
- 导出缓存统计报告
- 使用开发者工具查看 IndexedDB

### Q: 支持哪些浏览器？
A: 目前支持：
- Chrome 88+
- Edge 88+
- 其他 Chromium 内核浏览器

## 隐私说明

本插件：
- ✅ 仅缓存游戏资源，不收集个人信息
- ✅ 所有数据存储在本地
- ✅ 不会上传任何数据到第三方服务器
- ✅ 可随时清除所有缓存数据

## 更新日志

### v1.0.2 (2025-01-11)
- ✨ 新增基于路径的缓存规则支持
- 🔧 支持缓存 `/patch/files/` 路径下的所有文件
- 🐛 修复无标准扩展名的 CDN 资源文件无法缓存的问题
- 📝 更新缓存规则文档

### v1.0.1 (2025-01-11)
- ✨ 新增对 Unity WebGL 资源的支持（.data 和 .wasm 文件）
- 🔧 优化大文件缓存处理机制
- 📝 更新缓存策略文档

### v1.0.0 (2025-01-11)
- 🎉 首次发布
- ✨ 实现基础缓存功能
- ✨ 支持 Native Messaging
- ✨ 添加性能统计
- ✨ 支持自定义缓存规则

## 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 联系方式

- 项目主页：[https://github.com/ctwebplayer/extension](https://github.com/ctwebplayer/extension)
- 问题反馈：[https://github.com/ctwebplayer/extension/issues](https://github.com/ctwebplayer/extension/issues)
- 开发文档：[https://github.com/ctwebplayer/extension/wiki](https://github.com/ctwebplayer/extension/wiki)

---

Made with ❤️ by CTWebPlayer Team
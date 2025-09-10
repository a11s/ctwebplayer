# CTWebPlayer 浏览器插件设计文档

## 1. 项目概述

### 1.1 背景介绍
CTWebPlayer 是一个基于 WebView2 的云游戏客户端，在实际使用中遇到了以下核心问题：
- WebView2 无法有效缓存 iframe 中的资源
- 云游戏平台使用动态加载的 iframe 嵌入游戏内容
- 跨域安全策略限制了资源的拦截和缓存

### 1.2 问题分析
#### WebView2 的限制
1. **拦截机制限制**：WebView2 的 NavigationStarting 和 DOMContentLoaded 事件无法捕获 iframe 内部的资源请求
2. **跨域隔离**：由于同源策略，主页面无法访问跨域 iframe 的内容
3. **动态加载**：游戏资源通过 JavaScript 动态加载，传统的缓存机制难以生效

#### 现有缓存机制的不足
- HTTP 缓存头依赖服务器配置
- Service Worker 受跨域限制
- 本地代理方案实现复杂且影响性能

### 1.3 解决方案概述
开发一个浏览器插件，通过以下方式解决缓存问题：
1. 在用户登录成功后，直接导航到 iframe 地址
2. 利用浏览器插件的高权限拦截所有资源请求
3. 实现自定义缓存策略
4. 通过 Native Messaging 与 CTWebPlayer 集成

## 2. 解决方案架构

### 2.1 整体架构图
```
┌─────────────────────────────────────────────────────────────┐
│                        用户浏览器                              │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  CTWebPlayer    │    │  浏览器插件      │                │
│  │  (WebView2)     │◄───┤                 │                │
│  │                 │    │  - 内容脚本      │                │
│  │  - 登录管理     │    │  - 后台脚本      │                │
│  │  - 会话保持     │    │  - 缓存管理      │                │
│  │  - Native Host │    │  - 资源拦截      │                │
│  └────────┬────────┘    └────────┬────────┘                │
│           │                      │                          │
│           └──────────────────────┘                          │
│                  Native Messaging                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                      ┌──────────────┐
                      │  云游戏服务器  │
                      └──────────────┘
```

### 2.2 组件说明

#### 2.2.1 CTWebPlayer 组件
- **职责**：管理用户登录、会话保持、与插件通信
- **关键功能**：
  - 检测登录成功事件
  - 提取 iframe URL 和认证信息
  - 通过 Native Messaging 发送数据给插件

#### 2.2.2 浏览器插件组件
- **Manifest V3 架构**
- **核心模块**：
  - Background Service Worker：管理插件生命周期
  - Content Scripts：注入页面，监听和修改 DOM
  - Web Request API：拦截和修改网络请求
  - Storage API：管理缓存数据

### 2.3 数据流程
1. 用户在 CTWebPlayer 中登录
2. CTWebPlayer 检测到登录成功，提取 iframe URL
3. 通过 Native Messaging 发送信息给插件
4. 插件在新标签页打开 iframe URL
5. 插件拦截所有资源请求并缓存
6. 后续访问直接从缓存加载

## 3. 技术实现细节

### 3.1 Manifest V3 配置
```json
{
  "manifest_version": 3,
  "name": "CTWebPlayer Resource Cache",
  "version": "1.0.0",
  "description": "云游戏资源缓存插件",
  
  "permissions": [
    "webRequest",
    "webRequestBlocking",
    "storage",
    "unlimitedStorage",
    "tabs",
    "nativeMessaging",
    "<all_urls>"
  ],
  
  "host_permissions": [
    "https://*.cloudgame.com/*",
    "https://*.cdn-cloudgame.com/*"
  ],
  
  "background": {
    "service_worker": "background.js",
    "type": "module"
  },
  
  "content_scripts": [{
    "matches": ["https://*.cloudgame.com/*"],
    "js": ["content.js"],
    "run_at": "document_start"
  }],
  
  "action": {
    "default_popup": "popup.html",
    "default_icon": {
      "16": "icon16.png",
      "48": "icon48.png",
      "128": "icon128.png"
    }
  }
}
```

### 3.2 资源拦截机制
```javascript
// background.js - 资源拦截核心逻辑
chrome.webRequest.onBeforeRequest.addListener(
  async (details) => {
    // 检查是否为需要缓存的资源
    if (shouldCache(details.url)) {
      // 尝试从缓存获取
      const cachedData = await getCachedResource(details.url);
      if (cachedData) {
        return {
          redirectUrl: createDataUrl(cachedData)
        };
      }
    }
    return {};
  },
  { urls: ["<all_urls>"] },
  ["blocking"]
);

// 响应完成后缓存资源
chrome.webRequest.onCompleted.addListener(
  async (details) => {
    if (shouldCache(details.url) && details.statusCode === 200) {
      await cacheResource(details);
    }
  },
  { urls: ["<all_urls>"] }
);
```

### 3.3 缓存策略
```javascript
// 缓存策略配置
const CACHE_CONFIG = {
  // 资源类型缓存时长（毫秒）
  ttl: {
    'image': 7 * 24 * 60 * 60 * 1000,  // 7天
    'script': 24 * 60 * 60 * 1000,     // 1天
    'style': 24 * 60 * 60 * 1000,      // 1天
    'font': 30 * 24 * 60 * 60 * 1000,  // 30天
    'media': 3 * 60 * 60 * 1000,       // 3小时
    'unity': 7 * 24 * 60 * 60 * 1000,  // 7天（.data 和 .wasm 文件）
  },
  
  // 最大缓存大小（MB）
  maxSize: 500,
  
  // 缓存清理策略
  evictionPolicy: 'LRU',  // Least Recently Used
  
  // 需要缓存的资源模式
  patterns: [
    /\.(jpg|jpeg|png|gif|webp|svg)$/i,
    /\.(js|css)$/i,
    /\.(woff|woff2|ttf|eot)$/i,
    /\.(mp4|webm|ogg|mp3|wav)$/i,
    /\.(data|wasm)$/i,  // Unity WebGL 资源
  ]
};
```

### 3.4 Native Messaging 通信
```javascript
// Native Host 配置 (ctwebplayer_bridge.json)
{
  "name": "com.ctwebplayer.bridge",
  "description": "CTWebPlayer Native Messaging Bridge",
  "path": "C:\\Program Files\\CTWebPlayer\\native_host.exe",
  "type": "stdio",
  "allowed_origins": [
    "chrome-extension://[EXTENSION_ID]/"
  ]
}

// 插件端通信代码
let port = chrome.runtime.connectNative('com.ctwebplayer.bridge');

port.onMessage.addListener((message) => {
  if (message.type === 'navigate') {
    chrome.tabs.create({
      url: message.url,
      active: true
    });
  }
});

// CTWebPlayer 端通信代码 (C#)
public class NativeMessagingHost
{
    public void SendMessage(object message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        // 发送消息长度（4字节）
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        Console.OpenStandardOutput().Write(lengthBytes, 0, 4);
        
        // 发送消息内容
        Console.OpenStandardOutput().Write(bytes, 0, bytes.Length);
        Console.OpenStandardOutput().Flush();
    }
}
```

### 3.5 存储管理
```javascript
// 使用 IndexedDB 存储大型资源
class ResourceCache {
  constructor() {
    this.dbName = 'CTWebPlayerCache';
    this.version = 1;
    this.storeName = 'resources';
  }
  
  async init() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, this.version);
      
      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };
      
      request.onupgradeneeded = (event) => {
        const db = event.target.result;
        if (!db.objectStoreNames.contains(this.storeName)) {
          const store = db.createObjectStore(this.storeName, { keyPath: 'url' });
          store.createIndex('timestamp', 'timestamp', { unique: false });
          store.createIndex('size', 'size', { unique: false });
        }
      };
    });
  }
  
  async store(url, data, metadata) {
    const transaction = this.db.transaction([this.storeName], 'readwrite');
    const store = transaction.objectStore(this.storeName);
    
    const record = {
      url,
      data,
      metadata,
      timestamp: Date.now(),
      size: data.byteLength || data.length
    };
    
    return store.put(record);
  }
  
  async retrieve(url) {
    const transaction = this.db.transaction([this.storeName], 'readonly');
    const store = transaction.objectStore(this.storeName);
    
    return new Promise((resolve, reject) => {
      const request = store.get(url);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }
}
```

## 4. 集成方案

### 4.1 CTWebPlayer 端集成
```csharp
// 检测登录成功并提取 iframe URL
private async void OnLoginSuccess()
{
    // 等待 iframe 加载
    await Task.Delay(2000);
    
    // 执行 JavaScript 获取 iframe URL
    var script = @"
        const iframe = document.querySelector('iframe#gameFrame');
        if (iframe) {
            return {
                url: iframe.src,
                cookies: document.cookie
            };
        }
        return null;
    ";
    
    var result = await webView.ExecuteScriptAsync(script);
    if (result != null)
    {
        // 发送给浏览器插件
        SendToExtension(new {
            type = "navigate",
            url = result.url,
            cookies = result.cookies
        });
    }
}
```

### 4.2 插件端集成
```javascript
// 接收来自 CTWebPlayer 的消息
chrome.runtime.onMessageExternal.addListener(
  (request, sender, sendResponse) => {
    if (request.type === 'navigate') {
      // 创建新标签页并导航到游戏 URL
      chrome.tabs.create({
        url: request.url,
        active: true
      }, (tab) => {
        // 注入认证信息
        chrome.cookies.set({
          url: request.url,
          name: 'auth_token',
          value: request.authToken
        });
      });
    }
  }
);
```

### 4.3 安装和配置流程
1. **插件安装**
   - 用户安装浏览器插件
   - 插件自动注册 Native Messaging Host

2. **CTWebPlayer 配置**
   - 检测插件是否安装
   - 配置 Native Messaging 通信

3. **首次使用**
   - 用户在 CTWebPlayer 登录
   - 自动跳转到浏览器并加载游戏
   - 插件开始缓存资源

## 5. 实施步骤

### 5.1 第一阶段：基础功能开发（2周）
- [ ] 创建 Manifest V3 插件框架
- [ ] 实现基本的资源拦截功能
- [ ] 开发 IndexedDB 存储模块
- [ ] 实现简单的缓存策略

### 5.2 第二阶段：通信集成（1周）
- [ ] 实现 Native Messaging Host
- [ ] CTWebPlayer 端集成代码
- [ ] 插件端通信处理
- [ ] 测试双向通信

### 5.3 第三阶段：优化和完善（2周）
- [ ] 实现 LRU 缓存淘汰策略
- [ ] 添加缓存统计和管理界面
- [ ] 性能优化和错误处理
- [ ] 多浏览器兼容性测试

### 5.4 第四阶段：部署和发布（1周）
- [ ] 插件打包和签名
- [ ] 发布到 Chrome Web Store
- [ ] 编写用户文档
- [ ] 制作安装引导

## 6. 注意事项

### 6.1 安全考虑
1. **权限最小化**：只请求必要的权限
2. **数据加密**：敏感数据在存储前进行加密
3. **来源验证**：验证 Native Messaging 消息来源
4. **XSS 防护**：对注入的内容进行严格过滤

### 6.2 性能优化
1. **异步处理**：所有 I/O 操作使用异步方式
2. **批量操作**：合并多个小资源的缓存操作
3. **压缩存储**：对文本资源进行压缩存储
4. **预加载**：智能预加载可能需要的资源

### 6.3 兼容性
1. **浏览器支持**：
   - Chrome/Edge (Chromium) 88+
   - Firefox 109+ (需要调整 Manifest)
   - Safari 不支持 WebRequest API

2. **操作系统**：
   - Windows 10/11
   - macOS 10.15+
   - Linux (Ubuntu 20.04+)

### 6.4 已知限制
1. **Manifest V3 限制**：
   - 不能使用持久化后台页面
   - Service Worker 有生命周期限制
   - 某些 API 功能受限

2. **存储限制**：
   - IndexedDB 单个数据库最大 2GB
   - 需要实现存储配额管理

3. **性能影响**：
   - 资源拦截可能略微增加延迟
   - 大文件缓存可能影响内存使用
   - Unity WebGL 的 .data 和 .wasm 文件通常较大（几十MB到几百MB），需要特别注意存储空间管理

## 7. 测试计划

### 7.1 单元测试
- 缓存策略逻辑测试
- 存储管理功能测试
- 通信协议测试

### 7.2 集成测试
- CTWebPlayer 与插件通信测试
- 资源拦截和缓存测试
- 跨域资源处理测试

### 7.3 性能测试
- 缓存命中率测试
- 加载速度对比测试
- 内存使用监控

### 7.4 兼容性测试
- 不同浏览器版本测试
- 不同操作系统测试
- 不同网络环境测试

## 8. 维护和更新

### 8.1 版本管理
- 使用语义化版本号
- 维护更新日志
- 提供版本回滚机制

### 8.2 监控和分析
- 收集匿名使用统计
- 监控缓存效率
- 错误日志收集

### 8.3 用户支持
- 提供详细的使用文档
- 建立问题反馈渠道
- 定期更新 FAQ

## 9. 总结

本设计文档详细描述了通过浏览器插件解决 CTWebPlayer 中 iframe 资源缓存问题的完整方案。该方案利用浏览器插件的高权限特性，绕过了 WebView2 和跨域限制，实现了有效的资源缓存机制。

通过 Native Messaging 实现了 CTWebPlayer 与浏览器插件的无缝集成，用户体验流畅。整个方案具有良好的可扩展性和维护性，能够适应未来的需求变化。

### 关键优势
1. **解决核心问题**：有效缓存跨域 iframe 资源
2. **用户体验好**：自动化流程，无需手动干预
3. **性能提升明显**：减少重复下载，加快加载速度
4. **易于维护**：模块化设计，便于更新和扩展

### 后续展望
- 支持更多浏览器平台
- 增加智能预加载功能
- 开发移动端解决方案
- 探索 WebView2 新版本特性

---

文档版本：1.0.1
创建日期：2025-01-11
最后更新：2025-01-11
作者：CTWebPlayer 开发团队
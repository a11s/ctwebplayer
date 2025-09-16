# 窗口标题显示优化说明

## 需求描述
1. 窗口标题栏需要显示网页标题和用户昵称
2. 格式：`网页标题 - 玩家名` 或 `网页标题 - （未登录）`
3. 原问题：标题栏始终显示默认的 "Unity 3D webplayer 专属浏览器"

## 实现方案

### 1. 标题格式规则
- **已登录**：`网页标题 - 玩家昵称`
- **未登录**：`网页标题 - （未登录）`
- **无网页标题时**：仅显示玩家昵称或默认标题

### 2. 添加的功能
在 `Form1.cs` 中实现：
- `UpdateWindowTitleWithNickname()` - 获取用户 nickname
- `UpdateWindowTitle()` - 更新窗口标题（组合网页标题和玩家名）
- 保存当前网页标题和玩家名称状态

### 3. Cookie 读取机制
- 优先使用 JavaScript 直接读取 erolabsnickname cookie
- 备用方案使用 CookieManager API
- 自动 URL 解码处理

### 4. 事件处理
- `WebView2_NavigationCompleted` - 页面加载完成时更新
- `CoreWebView2_DocumentTitleChanged` - 网页标题改变时更新
- 退出登录时清空玩家名称并更新标题

## 实现细节

### JavaScript 方法（主要方法）
```javascript
// 直接从 document.cookie 中读取
var cookies = document.cookie.split(';');
for (var i = 0; i < cookies.length; i++) {
    var cookie = cookies[i].trim();
    if (cookie.startsWith('erolabsnickname=')) {
        var value = cookie.substring('erolabsnickname='.length);
        // URL 解码
        value = decodeURIComponent(value);
        return value;
    }
}
```

### CookieManager API 方法（备用）
```csharp
var allCookies = await _cookieManager.GetAllCookiesAsync();
var nicknameCookie = allCookies.FirstOrDefault(c => 
    c.Name.Equals("erolabsnickname", StringComparison.OrdinalIgnoreCase));
```

## 测试步骤

1. **启动应用程序**
   ```powershell
   cd d:\tools\ctwebplayer
   .\src\bin\Debug\net8.0-windows\ctwebplayer.exe
   ```

2. **未登录状态测试**
   - 启动后浏览任意网页
   - 标题格式：`网页标题 - （未登录）`

3. **登录账号**
   - 使用您的账号登录游戏
   - 确保登录成功

4. **已登录状态测试**
   - 登录后标题格式：`网页标题 - 您的昵称`
   - 浏览不同页面，标题应该动态更新

5. **退出登录测试**
   - 点击退出登录
   - 标题应该变回：`网页标题 - （未登录）`

6. **查看日志**
   检查 logs 目录下的日志文件，应该能看到类似：
   ```
   成功获取用户 nickname: [您的昵称]
   窗口标题已更新为: [网页标题] - [您的昵称]
   ```

## 注意事项

1. **Cookie 必须存在**
   - 必须登录后才能获取到 erolabsnickname cookie
   - 如果未登录，将显示默认标题

2. **域名匹配**
   - Cookie 只在正确的域名下才能读取
   - 支持主域名和子域名的 cookie

3. **编码处理**
   - 自动处理 URL 编码的 nickname
   - 支持中文、日文等多语言字符

## 故障排查

如果标题仍然不正确，请检查：

1. **查看日志文件**
   - 检查 `logs` 目录下的最新日志
   - 搜索 "nickname" 相关的日志

2. **确认 Cookie 存在**
   - 打开浏览器开发者工具（F12）
   - 在 Console 中输入：`document.cookie`
   - 检查是否有 `erolabsnickname` cookie

3. **手动测试**
   在浏览器控制台执行：
   ```javascript
   document.cookie.split(';').find(c => c.trim().startsWith('erolabsnickname='))
   ```

## 更新历史

- **2025-01-17**: 初始修复实现
  - 添加 JavaScript 方法读取 cookie
  - 添加 CookieManager API 备用方法
  - 修改标题更新逻辑
  - 添加调试日志
  - 修复退出登录时的线程错误（删除 Cookie 必须在 UI 线程执行）

## 额外修复：退出登录线程错误

### 问题
点击"退出登录"按钮时报错：
```
System.InvalidOperationException: CoreWebView2 members can only be accessed from the UI thread.
```

### 原因
`CookieManager.cs` 中的 `DeleteAllCookiesAsync()` 方法使用了 `Task.Run()` 在后台线程执行，但 WebView2 的 Cookie 操作必须在 UI 线程上执行。

### 解决方案
移除所有 Cookie 操作中的 `Task.Run()`，直接在 UI 线程上执行：

```csharp
// 之前（错误）
await Task.Run(() => _cookieManager.DeleteAllCookies());

// 之后（正确）
_cookieManager.DeleteAllCookies();
await Task.CompletedTask;
```

影响的方法：
- `DeleteAllCookiesAsync()`
- `SetCookieAsync()`
- `DeleteCookieAsync()`
- `AddOrUpdateCookieAsync()`
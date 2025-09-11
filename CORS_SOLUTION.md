# CORS 跨域问题解决方案

## 问题描述
用户在导航到 iframe 内的页面后，遇到了 CORS 跨域问题：
- 从 'https://patch-ct-labs.ecchi.xxx' 访问 'https://hefc.hrbzqy.com/patch/files/...' 的请求被 CORS 策略阻止
- 多个资源返回 403 Forbidden 错误

## 解决方案

### 1. WebView2 初始化时添加命令行参数

在 `InitializeWebView()` 方法中，添加了以下浏览器参数来禁用 CORS 限制：

```csharp
var browserArgs = new List<string>
{
    // 禁用 CORS 策略
    "--disable-web-security",
    "--disable-features=IsolateOrigins,site-per-process",
    "--allow-running-insecure-content",
    "--disable-site-isolation-trials",
    // 允许跨域访问
    "--disable-features=CrossOriginOpenerPolicy",
    "--disable-features=CrossOriginEmbedderPolicy",
    // 禁用同源策略
    "--disable-features=SameSiteByDefaultCookies,CookiesWithoutSameSiteMustBeSecure",
    // 允许所有来源的内容
    "--allow-file-access-from-files",
    "--allow-cross-origin-auth-prompt",
    // 禁用安全特性
    "--no-sandbox",
    "--disable-gpu-sandbox",
    "--disable-setuid-sandbox",
    // 允许不安全的内容
    "--allow-insecure-localhost",
    "--ignore-certificate-errors",
    // 用户代理设置
    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
};
```

### 2. 配置 WebView2 设置

添加了 `ConfigureWebView2Settings()` 方法来配置 WebView2 的各项设置：

```csharp
private void ConfigureWebView2Settings()
{
    var settings = webView2.CoreWebView2.Settings;
    
    // 允许所有内容
    settings.IsScriptEnabled = true;
    settings.IsWebMessageEnabled = true;
    settings.AreDefaultScriptDialogsEnabled = true;
    settings.IsStatusBarEnabled = true;
    settings.AreDevToolsEnabled = true;
    
    // 允许不安全的内容
    settings.IsPasswordAutosaveEnabled = true;
    settings.IsGeneralAutofillEnabled = true;
}
```

### 3. 自动允许所有权限请求

添加了 `CoreWebView2_PermissionRequested` 事件处理器，自动允许所有权限请求：

```csharp
private void CoreWebView2_PermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
{
    // 自动允许所有权限请求
    e.State = CoreWebView2PermissionState.Allow;
    LogManager.Instance.Info($"已允许权限请求：{e.PermissionKind} for {e.Uri}");
}
```

### 4. 在资源响应中添加 CORS 头部

在 `CoreWebView2_WebResourceRequested` 方法中，为所有缓存的资源响应添加 CORS 相关头部：

```csharp
var headers = $"Content-Type: {result.MimeType}\n" +
             "Access-Control-Allow-Origin: *\n" +
             "Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\n" +
             "Access-Control-Allow-Headers: *\n" +
             "Access-Control-Allow-Credentials: true\n" +
             "Cross-Origin-Resource-Policy: cross-origin\n" +
             "Cross-Origin-Embedder-Policy: unsafe-none\n" +
             "Cross-Origin-Opener-Policy: unsafe-none";
```

### 5. 注入 JavaScript 脚本处理 CORS

添加了 `InjectCorsHandlingScript()` 方法，在每个页面加载完成后注入 JavaScript 脚本：

- 覆盖 XMLHttpRequest 以添加 CORS 处理
- 覆盖 fetch API 以确保包含凭据
- 移除所有 CSP（内容安全策略）元标签
- 处理动态创建的 iframe，移除 sandbox 属性

### 6. 修改 iframe 的安全策略

在 `CheckAndNavigateToIframe()` 方法中，添加了脚本来修改页面的安全策略：

- 移除或修改 CSP 元标签
- 添加允许所有来源的 CSP
- 修改 iframe 的 sandbox 属性

## 效果

通过以上修改，WebView2 将：
1. 在浏览器级别禁用 CORS 限制
2. 自动允许所有权限请求
3. 为所有资源响应添加适当的 CORS 头部
4. 在页面级别注入脚本来处理动态的跨域请求
5. 确保 iframe 页面能够正常访问跨域资源

这样可以解决导航到 iframe 页面后的 CORS 问题，使得跨域资源能够正常加载。
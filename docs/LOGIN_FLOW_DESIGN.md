# CTWebPlayer 项目登录引导流程技术方案

## 1. 概述

本方案针对 CTWebPlayer 项目设计登录引导流程，基于 WebView2 控件实现 cookie 检查、用户交互和页面跳转逻辑。现有项目使用 WinForms + WebView2，无专用 cookie 管理，现需新增模块支持 "erolabsnickname" cookie 检测和引导。

### 背景
- **默认游戏 URL**：`https://game.ero-labs.live/cn/cloud_game.html?id=27&connect_type=1&connection_id=20`
- **BaseURL**：从 `config.json` 读取，可配置。
- **Cookie 存储**：WebView2 默认存储在 `data` 目录。
- **需求流程**：
  1. 打开游戏页面前检查 "erolabsnickname" cookie。
  2. 有 cookie → 直接进入游戏页面。
  3. 无 cookie → 弹出对话询问账号 → 有账号跳转登录页，无账号跳转注册页。
  4. 注册后跳转 `BaseURL/cn/index.html` → 检测 cookie 并自动跳转游戏。

### 设计原则
- **兼容性**：集成到现有 `Form1.cs` 和 `ConfigManager.cs`，最小化修改。
- **异步处理**：使用 async/await 处理 cookie 和导航，避免 UI 阻塞。
- **可配置**：通过 `config.json` 控制流程启用/禁用。
- **错误处理**：添加日志记录（使用现有 `LogManager`）。

## 2. Cookie 管理模块架构

### 2.1 模块位置
- 新增文件：`src/CookieManager.cs`
- 集成到 `Form1.cs`：在 `InitializeWebView` 中初始化。

### 2.2 核心 API 使用
使用 `Microsoft.Web.WebView2.Core.CoreWebView2CookieManager` API：
- **获取 CookieManager**：`webView2.CoreWebView2.CookieManager`
- **关键方法**：
  - `GetCookiesAsync(string uri, ICoreWebView2CookieListCompletedHandler handler)`：异步获取指定 URI 的所有 cookie。
  - `CreateCookie(string name, string value, string domain, string path, int? expires, bool? isSecure, bool? isHttpOnly, bool? sameSite, ICoreWebView2CookieListCompletedHandler handler)`：创建 cookie。
  - `DeleteCookie(ICoreWebView2Cookie cookie, ICoreWebView2CookieListCompletedHandler handler)`：删除 cookie。
  - `DeleteCookies(string name, string uri, ICoreWebView2CookieListCompletedHandler handler)`：删除指定 cookie。
  - `DeleteCookiesWithNameAndDomainAndPath(string name, string domain, string path, ICoreWebView2CookieListCompletedHandler handler)`：删除指定名称/域/路径的 cookie。

### 2.3 类设计：CookieManager
```csharp
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ctwebplayer
{
    public class CookieManager
    {
        private readonly CoreWebView2CookieManager _cookieManager;
        private readonly string _baseDomain; // e.g., "ero-labs.live"

        public CookieManager(CoreWebView2CookieManager cookieManager, string baseDomain)
        {
            _cookieManager = cookieManager;
            _baseDomain = baseDomain;
        }

        /// <summary>
        /// 检查指定 cookie 是否存在
        /// </summary>
        public async Task<bool> HasCookieAsync(string cookieName)
        {
            var uri = new Uri($"https://{_baseDomain}");
            var cookies = await GetCookiesAsync(uri);
            return cookies.Exists(c => c.Name == cookieName);
        }

        /// <summary>
        /// 获取所有 cookie
        /// </summary>
        private async Task<List<CoreWebView2Cookie>> GetCookiesAsync(Uri uri)
        {
            var tcs = new TaskCompletionSource<List<CoreWebView2Cookie>>();
            var cookies = new List<CoreWebView2Cookie>();

            void handler(CoreWebView2CookieList? cookieList, CoreWebView2CookieListCompletedHandler? args)
            {
                if (args?.Result == CoreWebView2CookieListCompletedHandlerResult.Success && cookieList != null)
                {
                    foreach (CoreWebView2Cookie cookie in cookieList)
                    {
                        cookies.Add(cookie);
                    }
                }
                tcs.SetResult(cookies);
            }

            _cookieManager.GetCookiesAsync(uri.ToString(), handler);
            return await tcs.Task;
        }

        // 其他方法：SetCookieAsync, DeleteCookieAsync 等（类似实现）
    }
}
```

### 2.4 集成方式
- 在 `Form1.InitializeWebView` 末尾：
  ```csharp
  var cookieManager = new CookieManager(webView2.CoreWebView2.CookieManager, new Uri(_configManager.Config.BaseURL).Host);
  // 存储到 Form1 字段：private CookieManager _cookieManager;
  ```
- 优势：封装 API 回调为 Task，便于异步使用；支持域特定检查。

## 3. 登录引导流程的状态机设计

### 3.1 状态定义
使用简单枚举状态机，集成到 `Form1.cs` 中，避免复杂框架。
```csharp
public enum LoginFlowState
{
    Initial,          // 初始化检查
    HasCookie,        // 有 cookie，直接游戏
    NoCookie,         // 无 cookie，询问账号
    HasAccount,       // 有账号，跳转登录
    NoAccount,        // 无账号，跳转注册
    LoginComplete,    // 登录完成，检查 cookie 跳转游戏
    GameLoaded        // 游戏加载完成
}
```

### 3.2 状态转换逻辑
- **Initial** → 检查 cookie：
  - 存在 "erolabsnickname" → HasCookie → Navigate to Game URL
  - 不存在 → NoCookie → Show Dialog
- **NoCookie** → 用户选择：
  - 是（有账号） → HasAccount → Navigate to `{BaseURL}/cn/login.html`
  - 否（无账号） → NoAccount → Navigate to `https://game.erolabsshare.net/app/627a8937/Cherry_Tale`
- **HasAccount/NoAccount** → NavigationCompleted：
  - 如果 URL == `{BaseURL}/cn/index.html`（注册后） → Check cookie → If has → LoginComplete → Navigate to Game URL
- **事件触发**：在 `InitializeWebView` 后调用 `StartLoginFlowAsync()`；在 `WebView2_NavigationCompleted` 中检查 URL 并转换状态。

### 3.3 伪代码实现
```csharp
private LoginFlowState _currentState = LoginFlowState.Initial;
private async Task StartLoginFlowAsync()
{
    switch (_currentState)
    {
        case LoginFlowState.Initial:
            bool hasCookie = await _cookieManager.HasCookieAsync("erolabsnickname");
            _currentState = hasCookie ? LoginFlowState.HasCookie : LoginFlowState.NoCookie;
            if (hasCookie)
            {
                NavigateToGame();
            }
            else
            {
                ShowAccountDialog();
            }
            break;
        // 其他 case...
    }
}

private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
{
    // 现有逻辑...
    var currentUrl = webView2.Source.ToString();
    if (currentUrl.Contains("/cn/index.html") && _currentState == LoginFlowState.HasAccount)
    {
        // 检查 cookie 并跳转
        _ = Task.Run(async () =>
        {
            bool hasCookie = await _cookieManager.HasCookieAsync("erolabsnickname");
            if (hasCookie)
            {
                _currentState = LoginFlowState.LoginComplete;
                this.Invoke(new Action(() => NavigateToGame()));
            }
        });
    }
}
```

### 3.4 优势
- 简单易维护；状态集中管理；支持异步转换。

## 4. 用户对话框的交互设计

### 4.1 对话框类型
- 使用自定义 WinForms Dialog（`src/LoginDialog.cs`），而非 MessageBox，便于样式和逻辑控制。
- 标题："登录引导"
- 内容："检测到您尚未登录，是否已有账号？"
- 按钮：["已有账号" (Yes), "新注册" (No), "跳过" (可选，配置控制)]
- 图标：信息图标。

### 4.2 实现
```csharp
public partial class LoginDialog : Form
{
    public enum DialogResultType { HasAccount, NoAccount, Skip }
    public DialogResultType UserChoice { get; private set; }

    private LoginDialog()
    {
        InitializeComponent();
        // UI: Label + Two Buttons
        btnHasAccount.Click += (s, e) => { UserChoice = DialogResultType.HasAccount; DialogResult = DialogResult.OK; Close(); };
        btnNoAccount.Click += (s, e) => { UserChoice = DialogResultType.NoAccount; DialogResult = DialogResult.OK; Close(); };
        if (_configManager.Config.Login.SkipEnabled)
        {
            btnSkip.Click += (s, e) => { UserChoice = DialogResultType.Skip; DialogResult = DialogResult.OK; Close(); };
        }
    }

    public static DialogResultType ShowDialog(IWin32Window owner, ConfigManager config)
    {
        using var dialog = new LoginDialog(config);
        dialog.ShowDialog(owner);
        return dialog.UserChoice;
    }
}
```

### 4.3 交互流程
- 弹出 → 用户点击 → 根据选择设置状态并关闭 → Form1 根据结果导航。
- 超时/取消：默认 NoAccount 或配置 Skip。
- 日志：记录用户选择。

### 4.4 UX 考虑
- 居中显示；简洁文案；支持 ESC 取消（= Skip）。

## 5. 页面跳转和 Cookie 检测的时机

### 5.1 跳转时机
- **初始跳转**：`InitializeWebView` 完成后，调用 `StartLoginFlowAsync()`，根据状态 Navigate。
- **登录/注册跳转**：对话后立即 `webView2.CoreWebView2.Navigate(url)`。
- **自动跳转**：`NavigationCompleted` 事件中，如果 URL 匹配 index.html，异步检查 cookie 并 Navigate to Game URL。
- **游戏 URL 构建**：复用现有 `BuildGameUrl` 方法。

### 5.2 Cookie 检测时机
- **初始检测**：WebView2 初始化后，环境创建完成（`EnsureCoreWebView2Async` 后）。
- **注册后检测**：页面加载完成（`NavigationCompleted`），延迟 1s 确保 cookie 设置。
- **域匹配**：检测时使用 BaseURL 的 Host，确保 cookie 域正确。
- **重试机制**：如果检测失败，重试 3 次（间隔 500ms）。

### 5.3 事件集成
- `NavigationStarting`：记录日志，防止无效跳转。
- `SourceChanged`：更新地址栏。
- 避免循环：状态机确保一次跳转后不重复检查。

## 6. 配置文件的扩展方案

### 6.1 扩展内容
在 `AppConfig` 添加 `Login` 配置对象，支持启用/禁用流程。
```csharp
public class LoginConfig
{
    public bool Enabled { get; set; } = true;  // 是否启用登录引导
    public bool SkipEnabled { get; set; } = true;  // 对话框是否显示"跳过"
    public string LoginUrl { get; set; } = "/cn/login.html";  // 登录页相对路径
    public string RegisterUrl { get; set; } = "https://game.erolabsshare.net/app/627a8937/Cherry_Tale";  // 注册页
    public string CookieName { get; set; } = "erolabsnickname";  // 检查 cookie 名
}

public class AppConfig
{
    // 现有字段...
    public LoginConfig Login { get; set; } = new LoginConfig();
}
```

### 6.2 更新 ConfigManager
- `GetDefaultConfig` 添加默认 `Login`。
- 新方法：`UpdateLoginConfigAsync(LoginConfig config)` 保存并重载。
- 版本兼容：加载时如果无 `Login`，使用默认。

### 6.3 使用
- 在 `Form1`：`if (!_configManager.Config.Login.Enabled) { NavigateToGame(); return; }`
- 对话框：根据 `SkipEnabled` 显示按钮。
- 优势：用户可通过 SettingsForm 配置；热重载（保存后重启应用）。

## 7. 实现注意事项
- **依赖**：确保 WebView2 NuGet 包支持 CookieManager（1.0.1072+）。
- **测试**：模拟 cookie（SetCookieAsync）；测试域跳转；边缘 case（如网络错误）。
- **性能**：Cookie 检查异步，非阻塞 UI。
- **安全**：Cookie 操作限于 BaseURL 域；不存储敏感数据。
- **后续**：集成到 SettingsForm 添加登录配置 UI。

此方案提供完整指导，后续可在 Code 模式实现。

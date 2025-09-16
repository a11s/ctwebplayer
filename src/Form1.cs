using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace ctwebplayer
{
    /// <summary>
    /// 登录流程状态枚举
    /// </summary>
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

    public partial class Form1 : Form
    {
        #region Win32 API声明
        // Win32 API函数声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        // Win32常量定义
        private const int WM_HOTKEY = 0x0312;
        
        // 修饰键常量
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000; // Windows 7及以上版本，防止重复触发
        
        // 热键ID定义
        private const int HOTKEY_ID_F11 = 1; // F11全屏切换
        private const int HOTKEY_ID_F4 = 2;  // F4静音切换
        private const int HOTKEY_ID_F10 = 3; // F10截图
        
        // 虚拟键码
        private const uint VK_F4 = 0x73;
        private const uint VK_F10 = 0x79;
        private const uint VK_F11 = 0x7A;
        #endregion
        
        #region 对话框实例管理
        // 保存对话框实例，避免重复创建
        private SettingsForm _settingsForm = null;
        private ProxySettingsForm _proxyForm = null;
        private LogViewerForm _logViewerForm = null;
        private UpdateForm _updateForm = null;
        private AboutForm _aboutForm = null;
        #endregion
        
        // 缓存管理器
        private CacheManager _cacheManager = null!; // 在 InitializeWebView 中初始化
        
        // 配置管理器
        private ConfigManager _configManager;
        
        // Cookie管理器
        private CookieManager _cookieManager = null!; // 在 InitializeWebView 中初始化
        
        // 自定义协议处理器
        private CustomProtocolHandler _protocolHandler = null!; // 在 InitializeWebView 中初始化
        
        // 登录流程状态
        private LoginFlowState _currentLoginState = LoginFlowState.Initial;
        
        // 缓存统计
        private int _cacheHits = 0;
        private int _cacheMisses = 0;

        // 全屏相关字段
        private bool _isFullScreen = false;
        private FormWindowState _previousWindowState;
        private FormBorderStyle _previousBorderStyle;
        private Rectangle _previousBounds;
        private bool _previousToolStripVisible;
        private bool _previousStatusStripVisible;
        
        // 静音相关字段
        private bool _isMuted = false;

        public Form1()
        {
            InitializeComponent();
            
            // 初始化配置管理器
            _configManager = new ConfigManager();
            
            // 设置 LanguageManager 使用相同的 ConfigManager 实例
            LanguageManager.SetConfigManager(_configManager);
            
            // 初始化语言管理器
            LanguageManager.Instance.Initialize();
            
            // 运行资源文件验证（仅在调试模式下）
            #if DEBUG
            ResourcesValidator.RunValidation();
            #endif
            
            // 应用语言设置
            LanguageManager.Instance.ApplyToForm(this);
            
            // 应用窗口大小设置
            ApplyWindowSize();
            
            InitializeWebView();

            // 不再订阅键盘事件，改用全局热键
            // this.KeyDown += Form1_KeyDown;
            
            // 添加调试日志
            LogManager.Instance.Info($"Form1构造函数：KeyPreview = {this.KeyPreview}");
            LogManager.Instance.Info("Form1构造函数：准备使用全局热键");
        }
        
        /// <summary>
        /// 应用窗口大小设置
        /// </summary>
        private void ApplyWindowSize()
        {
            if (_configManager?.Config?.Ui != null)
            {
                this.Width = _configManager.Config.Ui.WindowWidth;
                this.Height = _configManager.Config.Ui.WindowHeight;
                
                // 居中显示窗口
                this.StartPosition = FormStartPosition.CenterScreen;
                
                LogManager.Instance.Info($"应用窗口大小设置：{this.Width} x {this.Height}");
            }
        }

        /// <summary>
        /// 初始化WebView2控件
        /// </summary>
        private async void InitializeWebView()
        {
            try
            {
                // 记录应用程序启动
                LogManager.Instance.Info("应用程序启动");
                
                // 设置状态
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_InitializingBrowser");
                
                // 配置管理器已在构造函数中初始化
                LogManager.Instance.Info("配置管理器已初始化");
                
                // 初始化缓存管理器
                _cacheManager = new CacheManager();
                LogManager.Instance.Info("缓存管理器已初始化");
                
                // 初始化自定义协议处理器
                _protocolHandler = new CustomProtocolHandler(this, _configManager);
                LogManager.Instance.Info("自定义协议处理器已初始化");
                
                // 构建浏览器参数
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
                    // "--no-sandbox",
                    // "--disable-gpu-sandbox",
                    // "--disable-setuid-sandbox",
                    // 允许不安全的内容
                    "--allow-insecure-localhost",
                    "--ignore-certificate-errors",
                    // 用户代理设置
                    //"--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                };
                
                // 应用代理设置
                var proxyString = _configManager.GetProxyString();
                if (!string.IsNullOrEmpty(proxyString))
                {
                    browserArgs.Add($"--proxy-server={proxyString}");
                    LogManager.Instance.Info($"已应用代理设置：{proxyString}");
                }
                
                // 创建用户数据目录
                string userDataFolder = Path.Combine(Application.StartupPath, "data");
                if (!Directory.Exists(userDataFolder))
                {
                    Directory.CreateDirectory(userDataFolder);
                    LogManager.Instance.Info($"创建用户数据目录：{userDataFolder}");
                }
                
                // 创建WebView2环境选项
                var options = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = string.Join(" ", browserArgs)
                };
                
                LogManager.Instance.Info("WebView2初始化参数：" + options.AdditionalBrowserArguments);
                
                // 初始化WebView2，使用自定义用户数据文件夹
                // CreateAsync 参数顺序：(browserExecutableFolder, userDataFolder, options)
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await webView2.EnsureCoreWebView2Async(environment);
                LogManager.Instance.Info($"WebView2初始化成功，用户数据文件夹：{userDataFolder}");
                
                // 配置WebView2设置
                ConfigureWebView2Settings();
                
                // 配置WebView2以拦截资源请求
                ConfigureResourceInterception();
                
                // 初始化ConsoleLogger
                ConsoleLogger.Instance.Configure(true); // 启用ConsoleLogger
                LogManager.Instance.Info("ConsoleLogger已初始化并启用");
                
                // 注入JavaScript来捕获console输出
                webView2.CoreWebView2.DOMContentLoaded += async (sender, args) =>
                {
                    await InjectConsoleCapture();
                };
                LogManager.Instance.Info("已配置console输出捕获机制");
                
                // 订阅事件
                webView2.NavigationStarting += WebView2_NavigationStarting;
                webView2.NavigationCompleted += WebView2_NavigationCompleted;
                webView2.SourceChanged += WebView2_SourceChanged;
                webView2.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                webView2.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
                webView2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
                
                // 不再订阅WebView2的键盘事件，改用全局热键
                // webView2.PreviewKeyDown += WebView2_PreviewKeyDown;
                webView2.CoreWebView2.WindowCloseRequested += CoreWebView2_WindowCloseRequested;
                
                // 注册全局热键
                RegisterGlobalHotkeys();
                
                // 暂时不初始化 CookieManager，等导航到正确的域名后再初始化
                LogManager.Instance.Info("等待导航到正确域名后初始化 CookieManager");
                
                // 添加调试日志
                LogManager.Instance.Info($"登录配置：Enabled={_configManager.Config.Login?.Enabled ?? false}");
                LogManager.Instance.Info($"Cookie名称：{_configManager.Config.Login?.CookieName ?? "null"}");
                
                // 先导航到注册页面，它会自动跳转到游戏页面，确保 WebView2 完全初始化并加载 cookies
                LogManager.Instance.Info($"先导航到注册页面以初始化：{_configManager.Config.Login.RegisterUrl}");
                webView2.CoreWebView2.Navigate(_configManager.Config.Login.RegisterUrl);
                
                // 等待导航完成后再进行登录流程检查
                // 这将在 NavigationCompleted 事件中处理
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("WebView2初始化失败", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_Error_WebView2InitFailed")}: {ex.Message}\n\n{LanguageManager.Instance.GetString("Form1_Error_EnsureWebView2Installed")}",
                    LanguageManager.Instance.GetString("Message_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_BrowserInitFailed");
            }
        }

        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegisterGlobalHotkeys()
        {
            try
            {
                // 注册F11热键（全屏切换）
                bool f11Registered = RegisterHotKey(this.Handle, HOTKEY_ID_F11, MOD_NOREPEAT, VK_F11);
                if (f11Registered)
                {
                    LogManager.Instance.Info("已成功注册F11全局热键");
                }
                else
                {
                    LogManager.Instance.Warning("F11全局热键注册失败，可能被其他程序占用");
                }

                // 注册F4热键（静音切换）
                bool f4Registered = RegisterHotKey(this.Handle, HOTKEY_ID_F4, MOD_NOREPEAT, VK_F4);
                if (f4Registered)
                {
                    LogManager.Instance.Info("已成功注册F4全局热键");
                }
                else
                {
                    LogManager.Instance.Warning("F4全局热键注册失败，可能被其他程序占用");
                }

                // 注册F10热键（截图）
                bool f10Registered = RegisterHotKey(this.Handle, HOTKEY_ID_F10, MOD_NOREPEAT, VK_F10);
                if (f10Registered)
                {
                    LogManager.Instance.Info("已成功注册F10全局热键");
                }
                else
                {
                    LogManager.Instance.Warning("F10全局热键注册失败，可能被其他程序占用");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注册全局热键时出错", ex);
            }
        }

        /// <summary>
        /// 注销全局热键
        /// </summary>
        private void UnregisterGlobalHotkeys()
        {
            try
            {
                // 注销F11热键
                if (UnregisterHotKey(this.Handle, HOTKEY_ID_F11))
                {
                    LogManager.Instance.Info("已成功注销F11全局热键");
                }
                
                // 注销F4热键
                if (UnregisterHotKey(this.Handle, HOTKEY_ID_F4))
                {
                    LogManager.Instance.Info("已成功注销F4全局热键");
                }
                
                // 注销F10热键
                if (UnregisterHotKey(this.Handle, HOTKEY_ID_F10))
                {
                    LogManager.Instance.Info("已成功注销F10全局热键");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注销全局热键时出错", ex);
            }
        }

        /// <summary>
        /// 配置WebView2设置
        /// </summary>
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
            
            LogManager.Instance.Info("WebView2设置已配置");
        }

        /// <summary>
        /// 处理权限请求事件
        /// </summary>
        private void CoreWebView2_PermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            // 自动允许所有权限请求
            e.State = CoreWebView2PermissionState.Allow;
            LogManager.Instance.Info($"已允许权限请求：{e.PermissionKind} for {e.Uri}");
        }

        /// <summary>
        /// 标记是否已经订阅了WebMessage事件
        /// </summary>
        private bool _webMessageEventSubscribed = false;
        
        /// <summary>
        /// 注入JavaScript代码来捕获console输出
        /// </summary>
        private async Task InjectConsoleCapture()
        {
            try
            {
                var script = @"
                    (function() {
                        // 如果已经注入过，则跳过
                        if (window.__consoleLoggerInjected) return;
                        window.__consoleLoggerInjected = true;
                        
                        // 保存原始的console方法
                        var originalConsole = {
                            log: console.log,
                            info: console.info,
                            warn: console.warn,
                            error: console.error,
                            debug: console.debug
                        };
                        
                        // 获取调用堆栈信息
                        function getStackInfo() {
                            var stack = new Error().stack;
                            if (!stack) return { source: '', line: 0, column: 0 };
                            
                            var lines = stack.split('\n');
                            // 通常第3行包含调用者信息（第1行是Error，第2行是这个函数）
                            if (lines.length >= 3) {
                                var match = lines[2].match(/\((.+):(\d+):(\d+)\)/);
                                if (match) {
                                    return {
                                        source: match[1],
                                        line: parseInt(match[2]),
                                        column: parseInt(match[3])
                                    };
                                }
                            }
                            return { source: '', line: 0, column: 0 };
                        }
                        
                        // 包装console方法
                        function wrapConsoleMethod(method, kind) {
                            console[method] = function() {
                                // 调用原始方法
                                originalConsole[method].apply(console, arguments);
                                
                                // 获取消息和堆栈信息
                                var message = Array.prototype.slice.call(arguments).map(function(arg) {
                                    if (typeof arg === 'object') {
                                        try {
                                            return JSON.stringify(arg);
                                        } catch (e) {
                                            return String(arg);
                                        }
                                    }
                                    return String(arg);
                                }).join(' ');
                                
                                var stackInfo = getStackInfo();
                                
                                // 将对象转换为JSON字符串后发送
                                var messageData = JSON.stringify({
                                    type: 'consoleLog',
                                    kind: kind,
                                    message: message,
                                    source: stackInfo.source,
                                    line: stackInfo.line,
                                    column: stackInfo.column
                                });
                                
                                // 发送JSON字符串到C#端
                                window.chrome.webview.postMessage(messageData);
                            };
                        }
                        
                        // 包装所有console方法
                        wrapConsoleMethod('log', 'Log');
                        wrapConsoleMethod('info', 'Info');
                        wrapConsoleMethod('warn', 'Warning');
                        wrapConsoleMethod('error', 'Error');
                        wrapConsoleMethod('debug', 'Debug');
                        
                        console.log('Console capture initialized');
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 只订阅一次WebMessage事件
                if (!_webMessageEventSubscribed)
                {
                    webView2.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                    _webMessageEventSubscribed = true;
                    LogManager.Instance.Info("已订阅WebMessage事件");
                }
                
                LogManager.Instance.Info("已注入console捕获脚本");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注入console捕获脚本时出错", ex);
            }
        }

        /// <summary>
        /// 处理从JavaScript发送的消息
        /// </summary>
        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // 尝试获取字符串消息
                string message = null;
                try
                {
                    message = e.TryGetWebMessageAsString();
                }
                catch (ArgumentException)
                {
                    // 如果不是字符串，尝试获取JSON
                    try
                    {
                        message = e.WebMessageAsJson;
                    }
                    catch
                    {
                        // 如果都失败了，忽略这个消息
                        return;
                    }
                }
                
                if (string.IsNullOrEmpty(message)) return;
                
                // 解析JSON消息
                var json = System.Text.Json.JsonDocument.Parse(message);
                var root = json.RootElement;
                
                // 检查消息类型
                if (root.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == "consoleLog")
                {
                    // 提取console日志信息
                    var kind = root.TryGetProperty("kind", out var kindElement) ? kindElement.GetString() : "Log";
                    var logMessage = root.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "";
                    var source = root.TryGetProperty("source", out var srcElement) ? srcElement.GetString() : "";
                    var line = root.TryGetProperty("line", out var lineElement) ? lineElement.GetInt32() : 0;
                    var column = root.TryGetProperty("column", out var columnElement) ? columnElement.GetInt32() : 0;
                    
                    // 记录到ConsoleLogger
                    ConsoleLogger.Instance.LogFromWebView(kind ?? "Log", logMessage ?? "", source ?? "", line, column);
                    
                    // 同时记录到主日志系统（仅记录警告和错误）
                    if (kind != null)
                    {
                        if (kind.Equals("Error", StringComparison.OrdinalIgnoreCase))
                        {
                            LogManager.Instance.Error($"[WebView2 Console Error] {logMessage} (Source: {source}:{line}:{column})");
                        }
                        else if (kind.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                        {
                            LogManager.Instance.Warning($"[WebView2 Console Warning] {logMessage} (Source: {source}:{line}:{column})");
                        }
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // 不是我们的console消息，忽略
            }
            catch (Exception ex)
            {
                // 只记录非预期的错误
                if (!(ex is ArgumentException))
                {
                    LogManager.Instance.Error("处理WebMessage时出错", ex);
                }
            }
        }

        /// <summary>
        /// 配置资源拦截
        /// </summary>
        private void ConfigureResourceInterception()
        {
            // 添加资源请求过滤器，拦截所有资源请求
            webView2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            
            // 订阅资源请求事件
            webView2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            
            LogManager.Instance.Info("资源拦截已配置");
        }

        /// <summary>
        /// 处理资源请求事件
        /// </summary>
        private async void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uri = e.Request.Uri;
            
            // 检查是否应该缓存此资源
            if (!_cacheManager.ShouldCache(uri))
            {
                _ = RequestLogger.Instance.WriteRequestLog(uri, "NOT_CACHEABLE");
                return; // 不缓存，让WebView2正常处理
            }

            // 获取延迟对象以异步处理
            var deferral = e.GetDeferral();
            
            try
            {
                CacheResult? result = null;
                
                // 首先尝试从缓存读取
                if (_cacheManager.IsCached(uri))
                {
                    result = await _cacheManager.GetFromCacheAsync(uri);
                    if (result.Success)
                    {
                        _cacheHits++;
                        UpdateCacheStatus();
                        #if DEBUG
                        LogManager.Instance.Debug($"缓存命中：{uri}");
                        #endif
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "HIT", result.Data!.LongLength, $"Cache file: {result.FilePath}");
                    }
                }
                
                // 如果缓存中没有，则下载并缓存
                if (result == null || !result.Success)
                {
                    // 记录开始下载
                    _ = RequestLogger.Instance.WriteRequestLog(uri, "DOWNLOADING");
                    
                    var stopwatch = Stopwatch.StartNew();
                    result = await _cacheManager.DownloadAndCacheAsync(uri);
                    stopwatch.Stop();
                    
                    if (result.Success)
                    {
                        _cacheMisses++;
                        UpdateCacheStatus();
                        #if DEBUG
                        LogManager.Instance.Debug($"缓存未命中，已下载并缓存：{uri}");
                        #endif
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "MISS", result.Data!.LongLength, $"Download time: {stopwatch.ElapsedMilliseconds}ms, Cache file: {result.FilePath}");
                    }
                    else
                    {
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "ERROR", null, "Download failed");
                    }
                }
                
                // 如果成功获取资源，创建响应
                if (result != null && result.Success)
                {
                    var stream = new MemoryStream(result.Data);
                    
                    // 构建响应头，添加 CORS 相关头部
                    var headers = $"Content-Type: {result.MimeType}\n" +
                                 "Access-Control-Allow-Origin: *\n" +
                                 "Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\n" +
                                 "Access-Control-Allow-Headers: *\n" +
                                 "Access-Control-Allow-Credentials: true\n" +
                                 "Cross-Origin-Resource-Policy: cross-origin\n" +
                                 "Cross-Origin-Embedder-Policy: unsafe-none\n" +
                                 "Cross-Origin-Opener-Policy: unsafe-none";
                    
                    var response = webView2.CoreWebView2.Environment.CreateWebResourceResponse(
                        stream,
                        200,
                        "OK",
                        headers
                    );
                    
                    e.Response = response;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"处理资源请求时出错：{uri}", ex);
                _ = RequestLogger.Instance.WriteRequestLog(uri, "ERROR", null, ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// 更新缓存状态显示
        /// </summary>
        private void UpdateCacheStatus()
        {
            // 在UI线程上更新状态
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateCacheStatus));
                return;
            }
            
            var cacheSize = _cacheManager.GetCacheSize();
            var cacheSizeText = FormatFileSize(cacheSize);
            var hitRate = _cacheHits + _cacheMisses > 0
                ? (_cacheHits * 100.0 / (_cacheHits + _cacheMisses)).ToString("F1")
                : "0";
            
            // 获取代理状态
            var proxyStatus = _configManager.Config.Proxy?.Enabled == true
                ? $" | {LanguageManager.Instance.GetString("Form1_Proxy_Enabled")}"
                : $" | {LanguageManager.Instance.GetString("Form1_Proxy_Disabled")}";
            
            // 获取静音状态
            var muteStatus = _isMuted
                ? $" | {LanguageManager.Instance.GetString("Form1_Mute_On")}"
                : $" | {LanguageManager.Instance.GetString("Form1_Mute_Off")}";

            // 使用格式化字符串
            var cacheStatusText = string.Format(LanguageManager.Instance.GetString("Form1_Cache_Status"),
                _cacheHits, _cacheMisses, hitRate, cacheSizeText);
            
            statusLabel.Text = $"{cacheStatusText}{proxyStatus}{muteStatus}";
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 导航开始事件
        /// </summary>
        private async void WebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // 检查是否是自定义协议
            if (CustomProtocolHandler.ShouldIntercept(e.Uri))
            {
                // 取消导航
                e.Cancel = true;
                
                LogManager.Instance.Info($"拦截自定义协议URL：{e.Uri}");
                
                // 异步处理协议
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _protocolHandler.HandleProtocolAsync(e.Uri);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"处理自定义协议时出错：{e.Uri}", ex);
                    }
                });
                
                return;
            }
            
            // 显示进度条
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_Loading");
            
            LogManager.Instance.Info($"开始导航到：{e.Uri}");
            
            // 更新按钮状态
            UpdateNavigationButtons();
        }

        /// <summary>
        /// 导航完成事件
        /// </summary>
        private async void WebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 隐藏进度条
            progressBar.Visible = false;
            
            if (e.IsSuccess)
            {
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_LoadComplete");
                LogManager.Instance.Info($"页面加载完成：{webView2.Source}");
                
                // 注入 CORS 处理脚本
                await InjectCorsHandlingScript();
                
                // 注入console捕获脚本
                await InjectConsoleCapture();
                
                // 检测并处理 ero-labs 域名跳转
                await CheckAndUpdateBaseUrl();
                
                // 获取当前URL
                var currentUrl = webView2.Source?.ToString() ?? "";
                
                // 检查当前域名是否与 baseURL 匹配（支持跳转后的域名）
                if (!string.IsNullOrEmpty(currentUrl))
                {
                    try
                    {
                        var currentUri = new Uri(currentUrl);
                        var baseUri = new Uri(_configManager.Config.BaseURL);
                        
                        // 检查是否是同一个主域名（如 ero-labs.live）
                        var currentDomain = currentUri.Host;
                        var baseDomain = baseUri.Host;
                        
                        // 提取主域名（去掉子域名）
                        var currentMainDomain = GetMainDomain(currentDomain);
                        var baseMainDomain = GetMainDomain(baseDomain);
                        
                        if (currentMainDomain.Equals(baseMainDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            // 如果 CookieManager 还未初始化，或域名发生了变化，重新初始化
                            if (_cookieManager == null || !currentDomain.Equals(baseDomain, StringComparison.OrdinalIgnoreCase))
                            {
                                // 使用新的构造函数，传入 CoreWebView2 实例以支持 JavaScript 检测
                                _cookieManager = new CookieManager(webView2.CoreWebView2.CookieManager, currentDomain, webView2.CoreWebView2);
                                LogManager.Instance.Info($"CookieManager 已初始化/更新，使用当前域名：{currentDomain}，支持 JavaScript 检测");
                            }
                            
                            // 检查是否导航到了游戏相关页面并且还未进行登录流程检查
                            if ((currentUrl.Contains("/game/") || currentUrl.Contains("/cn/game")) &&
                                _currentLoginState == LoginFlowState.Initial &&
                                _configManager.Config.Login?.Enabled == true)
                            {
                                LogManager.Instance.Info($"检测到游戏页面 {currentUrl}，等待页面稳定后开始登录流程检查");
                                
                                // 等待页面完全加载并且 cookies 已同步
                                await Task.Delay(1500);
                                
                                LogManager.Instance.Info("开始登录流程检查");
                                await StartLoginFlowAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"处理导航完成事件时出错：{ex.Message}", ex);
                    }
                }
                
                // 调试日志：检查iframe自动导航
                LogManager.Instance.Info("NavigationCompleted: 检查是否需要iframe自动导航，URL=" + currentUrl);
                
                // 检查当前URL是否是目标URL
                if (currentUrl.StartsWith("https://game.ero-labs.live/cn/cloud_game.html") ||
                    currentUrl.StartsWith("https://mg.ero-labs.live/cn/cloud_game.html"))
                {
                    LogManager.Instance.Info("NavigationCompleted: 检测到游戏URL，调用CheckAndNavigateToIframe");
                    await CheckAndNavigateToIframe();
                }
                else
                {
                    LogManager.Instance.Info("NavigationCompleted: URL不匹配游戏URL，跳过iframe检测");
                }
                
                // 检查并处理Unity canvas元素
                await CheckAndHandleUnityCanvas();
                
                // 无论如何都要检查登录流程状态，确保登录后自动跳转
                await CheckLoginFlowStatus();
            }
            else
            {
                statusLabel.Text = $"{LanguageManager.Instance.GetString("Form1_Status_LoadFailed")}: {e.WebErrorStatus}";
                LogManager.Instance.Warning($"页面加载失败：{webView2.Source}，错误：{e.WebErrorStatus}");
            }
            
            // 更新按钮状态
            UpdateNavigationButtons();
        }

        /// <summary>
        /// 注入 CORS 处理脚本
        /// </summary>
        private async Task InjectCorsHandlingScript()
        {
            try
            {
                var script = @"
                    (function() {
                        // 覆盖 XMLHttpRequest 以添加 CORS 处理
                        var originalXHR = window.XMLHttpRequest;
                        window.XMLHttpRequest = function() {
                            var xhr = new originalXHR();
                            
                            // 保存原始的 open 方法
                            var originalOpen = xhr.open;
                            xhr.open = function(method, url, async, user, password) {
                                // 记录请求
                                console.log('XHR Request:', method, url);
                                
                                // 调用原始的 open 方法
                                return originalOpen.apply(this, arguments);
                            };
                            
                            // 覆盖 setRequestHeader 以确保正确的头部
                            var originalSetRequestHeader = xhr.setRequestHeader;
                            xhr.setRequestHeader = function(header, value) {
                                // 允许所有头部
                                return originalSetRequestHeader.apply(this, arguments);
                            };
                            
                            return xhr;
                        };
                        
                        // 覆盖 fetch API
                        if (window.fetch) {
                            var originalFetch = window.fetch;
                            window.fetch = function(url, options) {
                                options = options || {};
                                
                                // 确保包含凭据
                                if (!options.credentials) {
                                    options.credentials = 'include';
                                }
                                
                                // 记录请求
                                console.log('Fetch Request:', url, options);
                                
                                return originalFetch.apply(this, arguments);
                            };
                        }
                        
                        // 移除所有 CSP 元标签
                        var cspMetas = document.querySelectorAll('meta[http-equiv=""Content-Security-Policy""]');
                        cspMetas.forEach(function(meta) {
                            meta.remove();
                        });
                        
                        // 处理动态创建的 iframe
                        var originalCreateElement = document.createElement;
                        document.createElement = function(tagName) {
                            var element = originalCreateElement.apply(document, arguments);
                            
                            if (tagName.toLowerCase() === 'iframe') {
                                // 移除 sandbox 属性
                                element.removeAttribute('sandbox');
                                // 添加允许跨域的属性
                                element.setAttribute('allow', 'cross-origin-isolated *');
                            }
                            
                            return element;
                        };
                        
                        console.log('CORS handling script injected');
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
                LogManager.Instance.Info("已注入 CORS 处理脚本");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注入 CORS 处理脚本时出错", ex);
            }
        }

        /// <summary>
        /// 检查页面中的iframe并导航到其src地址
        /// </summary>
        private async Task CheckAndNavigateToIframe()
        {
            try
            {
                LogManager.Instance.Info("开始检查自动导航iframe功能");
                
                // 检查是否启用了自动导航功能
                if (!_configManager.Config.EnableAutoIframeNavigation)
                {
                    LogManager.Instance.Info("自动导航到iframe功能已禁用");
                    return;
                }
                
                // 等待一小段时间确保页面完全加载
                await Task.Delay(500);
                
                // 先注入脚本来修改页面的安全策略
                var policyScript = @"
                    (function() {
                        // 移除或修改 CSP 元标签
                        var metaTags = document.querySelectorAll('meta[http-equiv=""Content-Security-Policy""]');
                        metaTags.forEach(function(tag) {
                            tag.remove();
                        });
                        
                        // 添加允许所有来源的 CSP
                        var newMeta = document.createElement('meta');
                        newMeta.httpEquiv = 'Content-Security-Policy';
                        newMeta.content = ""default-src * 'unsafe-inline' 'unsafe-eval' data: blob:; script-src * 'unsafe-inline' 'unsafe-eval'; connect-src * 'unsafe-inline'; img-src * data: blob: 'unsafe-inline'; frame-src *; style-src * 'unsafe-inline';"";
                        document.head.appendChild(newMeta);
                        
                        // 修改 iframe 的 sandbox 属性
                        var iframes = document.getElementsByTagName('iframe');
                        for (var i = 0; i < iframes.length; i++) {
                            iframes[i].removeAttribute('sandbox');
                            iframes[i].setAttribute('allow', 'cross-origin-isolated *');
                        }
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(policyScript);
                
                // 添加调试脚本，输出页面元素信息
                var debugScript = @"
                    (function() {
                        console.log('=== Page Debug Info ===');
                        console.log('Current URL: ' + window.location.href);
                        console.log('Page Title: ' + document.title);
                        
                        // 检查 unity-canvas
                        var unityCanvas = document.getElementById('unity-canvas');
                        if (unityCanvas) {
                            console.log('Unity canvas found: ', unityCanvas);
                            console.log('Unity canvas parent: ', unityCanvas.parentElement);
                        } else {
                            console.log('No unity-canvas found');
                        }
                        
                        // 检查所有 canvas 元素
                        var allCanvas = document.getElementsByTagName('canvas');
                        console.log('Total canvas elements: ' + allCanvas.length);
                        for (var i = 0; i < allCanvas.length; i++) {
                            console.log('Canvas ' + i + ': id=' + allCanvas[i].id + ', class=' + allCanvas[i].className);
                        }
                        
                        // 检查所有 iframe 元素
                        var allIframes = document.getElementsByTagName('iframe');
                        console.log('Total iframe elements: ' + allIframes.length);
                        for (var i = 0; i < allIframes.length; i++) {
                            console.log('Iframe ' + i + ': src=' + allIframes[i].src + ', id=' + allIframes[i].id);
                        }
                        
                        console.log('=== End Debug Info ===');
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(debugScript);
                
                // 执行JavaScript来查找iframe
                var script = @"
                    (function() {
                        var iframes = document.getElementsByTagName('iframe');
                        console.log('Found ' + iframes.length + ' iframes');
                        
                        if (iframes.length > 0) {
                            var results = [];
                            for (var i = 0; i < iframes.length; i++) {
                                var iframe = iframes[i];
                                var src = iframe.src;
                                console.log('Iframe ' + i + ' src: ' + src);
                                results.push({
                                    index: i,
                                    src: src,
                                    id: iframe.id,
                                    className: iframe.className
                                });
                            }
                            
                            // 首先检查是否有包含 patch-ct-labs.ecchi.xxx 的iframe
                            for (var i = 0; i < results.length; i++) {
                                if (results[i].src && results[i].src.includes('patch-ct-labs.ecchi.xxx')) {
                                    console.log('Found patch-ct-labs iframe: ' + results[i].src);
                                    return results[i].src;
                                }
                            }
                            
                            // 如果没有找到特定域名的iframe，检查是否有任何游戏相关的iframe
                            for (var i = 0; i < results.length; i++) {
                                if (results[i].src &&
                                    (results[i].src.includes('game') ||
                                     results[i].src.includes('unity') ||
                                     results[i].src.includes('cloud'))) {
                                    console.log('Found game-related iframe: ' + results[i].src);
                                    return results[i].src;
                                }
                            }
                            
                            // 如果还是没有找到，返回第一个有src的iframe
                            for (var i = 0; i < results.length; i++) {
                                if (results[i].src && results[i].src.length > 0) {
                                    console.log('Using first iframe with src: ' + results[i].src);
                                    return results[i].src;
                                }
                            }
                        }
                        
                        console.log('No suitable iframe found');
                        return null;
                    })();
                ";
                
                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 解析结果（结果是JSON格式的字符串）
                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    // 移除引号
                    var iframeSrc = result.Trim('"');
                    
                    if (!string.IsNullOrEmpty(iframeSrc) && iframeSrc != "null")
                    {
                        LogManager.Instance.Info($"检测到iframe，准备导航到：{iframeSrc}");
                        
                        // 导航到iframe的src地址
                        webView2.CoreWebView2.Navigate(iframeSrc);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检查iframe时出错", ex);
            }
        }

        /// <summary>
        /// 检查页面中的Unity canvas元素并进行全屏处理
        /// </summary>
        private async Task CheckAndHandleUnityCanvas()
        {
            try
            {
                // 等待一小段时间确保页面完全加载
                await Task.Delay(500);
                
                // 执行JavaScript来查找并处理Unity canvas
                var script = @"
                    (function() {
                        // 查找Unity canvas元素
                        var canvas = document.getElementById('unity-canvas');
                        if (!canvas) {
                            console.log('JS: Unity canvas 未找到');
                            return false;
                        }
                        
                        // 记录原始canvas信息
                        console.log('Unity canvas detected:', canvas);
                        
                        // 检查 Unity 实例是否存在
                        if (window.unityInstance) {
                            console.log('JS: Unity 实例已加载');
                        } else {
                            console.log('JS: Unity 实例未加载，可能注入时机过早');
                        }
                        
                        // 首先移除所有可能存在的旧样式
                        var oldStyles = document.querySelectorAll('style[data-unity-fullscreen]');
                        oldStyles.forEach(function(style) {
                            style.remove();
                        });
                        
                        // 创建新的样式表，使用更简单直接的方法
                        var style = document.createElement('style');
                        style.setAttribute('data-unity-fullscreen', 'true');
                        style.innerHTML = `
                            /* 重置html和body */
                            html, body {
                                margin: 0 !important;
                                padding: 0 !important;
                                width: 100% !important;
                                height: 100% !important;
                                overflow: hidden !important;
                                position: relative !important;
                                background: #000 !important;
                            }
                            
                            /* 隐藏所有不必要的元素 */
                            #unity-loading-bar,
                            .footer,
                            .header,
                            .navbar,
                            .loading-bar,
                            .progress-bar:not(#unity-progress-bar-full),
                            body > *:not(.webgl-content):not(#gameContainer):not(#unity-canvas):not(script):not(style) {
                                display: none !important;
                            }
                            
                            /* 重置所有容器 */
                            .webgl-content,
                            #gameContainer {
                                position: fixed !important;
                                top: 0 !important;
                                left: 0 !important;
                                right: 0 !important;
                                bottom: 0 !important;
                                width: 100% !important;
                                height: 100% !important;
                                margin: 0 !important;
                                padding: 0 !important;
                                border: 0 !important;
                                background: #000 !important;
                                transform: none !important;
                                max-width: none !important;
                                max-height: none !important;
                                min-width: 0 !important;
                                min-height: 0 !important;
                            }
                            
                            /* 设置canvas全屏 */
                            #unity-canvas {
                                position: fixed !important;
                                top: 0 !important;
                                left: 0 !important;
                                width: 100% !important;
                                height: 100% !important;
                                margin: 0 !important;
                                padding: 0 !important;
                                border: 0 !important;
                                display: block !important;
                                background: #000 !important;
                                transform: none !important;
                                max-width: none !important;
                                max-height: none !important;
                                min-width: 0 !important;
                                min-height: 0 !important;
                                object-fit: contain !important;
                            }
                            
                            /* 确保没有任何元素限制尺寸 */
                            * {
                                max-width: none !important;
                                max-height: none !important;
                            }
                        `;
                        document.head.appendChild(style);
                        
                        // 直接设置canvas的尺寸为窗口大小
                        function updateCanvasSize() {
                            var windowWidth = window.innerWidth;
                            var windowHeight = window.innerHeight;
                            
                            // 设置canvas的实际像素尺寸
                            canvas.width = windowWidth;
                            canvas.height = windowHeight;
                            
                            // 设置canvas的CSS尺寸
                            canvas.style.width = windowWidth + 'px';
                            canvas.style.height = windowHeight + 'px';
                            
                            console.log('Canvas resized to:', windowWidth, 'x', windowHeight);
                            
                            // 通知Unity窗口大小改变
                            if (window.unityInstance && window.unityInstance.SendMessage) {
                                try {
                                    window.unityInstance.SendMessage('Canvas', 'OnWindowResize',
                                        windowWidth + ',' + windowHeight);
                                } catch (e) {
                                    console.log('Unity SendMessage not available');
                                }
                            }
                        }
                        
                        // 移除所有父元素的内联样式
                        var parent = canvas.parentElement;
                        while (parent && parent !== document.body) {
                            parent.style.cssText = '';
                            parent = parent.parentElement;
                        }
                        
                        // 确保canvas直接在body下或在简单的容器中
                        if (canvas.parentElement && canvas.parentElement !== document.body) {
                            // 清除父容器的所有内联样式
                            canvas.parentElement.style.cssText = '';
                        }
                        
                        // 立即更新尺寸
                        updateCanvasSize();
                        
                        // 监听窗口大小变化
                        window.removeEventListener('resize', updateCanvasSize); // 移除可能存在的旧监听器
                        window.addEventListener('resize', updateCanvasSize);
                        
                        // 再次强制更新，确保生效
                        setTimeout(updateCanvasSize, 100);
                        setTimeout(updateCanvasSize, 500);
                        
                        return true;
                    })();
                ";
                
                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 解析结果
                if (result == "true")
                {
                    LogManager.Instance.Info("检测到Unity canvas元素，已应用全屏样式");
                    statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_UnityCanvasFullscreen");
                } else {
                    LogManager.Instance.Warning("未检测到 Unity canvas 或处理失败，result: " + result);
                }
                LogManager.Instance.Info("CheckAndHandleUnityCanvas: 完成处理");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("处理Unity canvas时出错", ex);
            }
        }

        /// <summary>
        /// URL改变事件
        /// </summary>
        private void WebView2_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            // 更新地址栏
            txtAddress.Text = webView2.Source?.ToString() ?? "";
        }

        /// <summary>
        /// 文档标题改变事件
        /// </summary>
        private void CoreWebView2_DocumentTitleChanged(object? sender, object e)
        {
            // 更新窗口标题
            string title = webView2.CoreWebView2.DocumentTitle;
            if (!string.IsNullOrEmpty(title))
            {
                this.Text = $"{title} - {LanguageManager.Instance.GetString("Form1_Title")}";
            }
            else
            {
                this.Text = LanguageManager.Instance.GetString("Form1_Title");
            }
        }

        /// <summary>
        /// 下载开始事件
        /// </summary>
        private void CoreWebView2_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            // 可以在这里处理下载逻辑
            statusLabel.Text = $"{LanguageManager.Instance.GetString("Form1_Status_Downloading")}: {e.DownloadOperation.Uri}";
            LogManager.Instance.Info($"开始下载资源：{e.DownloadOperation.Uri}");
        }

        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (webView2.CoreWebView2 != null)
            {
                btnBack.Enabled = webView2.CanGoBack;
                btnForward.Enabled = webView2.CanGoForward;
            }
        }

        /// <summary>
        /// 主页按钮点击事件
        /// </summary>
        private void btnHome_Click(object? sender, EventArgs e)
        {
            // 导航到游戏主页
            NavigateToGame();
        }

        /// <summary>
        /// 后退按钮点击事件
        /// </summary>
        private void btnBack_Click(object? sender, EventArgs e)
        {
            if (webView2.CanGoBack)
            {
                webView2.GoBack();
            }
        }

        /// <summary>
        /// 前进按钮点击事件
        /// </summary>
        private void btnForward_Click(object? sender, EventArgs e)
        {
            if (webView2.CanGoForward)
            {
                webView2.GoForward();
            }
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private void btnRefresh_Click(object? sender, EventArgs e)
        {
            webView2.Reload();
        }

        /// <summary>
        /// 转到按钮点击事件
        /// </summary>
        private void btnGo_Click(object? sender, EventArgs e)
        {
            NavigateToUrl();
        }

        /// <summary>
        /// 地址栏按键事件
        /// </summary>
        private void txtAddress_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 导航到地址栏中的URL
        /// </summary>
        private async void NavigateToUrl()
        {
            string url = txtAddress.Text.Trim();
            
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            
            // 检查是否是ct://协议
            if (url.StartsWith("ct://", StringComparison.OrdinalIgnoreCase))
            {
                LogManager.Instance.Info($"地址栏输入自定义协议URL：{url}");
                
                // 直接处理协议，不进行导航
                try
                {
                    await _protocolHandler.HandleProtocolAsync(url);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"处理自定义协议时出错：{url}", ex);
                    MessageBox.Show($"处理协议时出错：{ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            
            // 如果没有协议，添加http://
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }
            
            try
            {
                webView2.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"无法导航到URL：{url}", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_Error_NavigateUrlFailed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void btnSettings_Click(object? sender, EventArgs e)
        {
            // 打开综合设置窗口（非模式）
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_configManager);
                _settingsForm.FormClosed += (s, args) =>
                {
                    // 窗口关闭时更新状态
                    UpdateCacheStatus();
                    LogManager.Instance.Info("设置窗口已关闭");
                };
            }
            
            if (!_settingsForm.Visible)
            {
                _settingsForm.Show(this);
            }
            else
            {
                _settingsForm.BringToFront();
                _settingsForm.Focus();
            }
        }

        /// <summary>
        /// 显示缓存信息
        /// </summary>
        private void ShowCacheInfo()
        {
            var cacheSize = _cacheManager.GetCacheSize();
            var cacheSizeText = FormatFileSize(cacheSize);
            var hitRate = _cacheHits + _cacheMisses > 0
                ? (_cacheHits * 100.0 / (_cacheHits + _cacheMisses)).ToString("F1")
                : "0";
            
            var message = $"{LanguageManager.Instance.GetString("Form1_CacheInfo_Statistics")}:\n\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_Directory")}: ./cache\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_Size")}: {cacheSizeText}\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_HitCount")}: {_cacheHits}\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_MissCount")}: {_cacheMisses}\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_HitRate")}: {hitRate}%\n\n" +
                         $"{LanguageManager.Instance.GetString("Form1_CacheInfo_Policy")}:\n" +
                         $"- {LanguageManager.Instance.GetString("Form1_CacheInfo_PolicyJS")}\n" +
                         $"- {LanguageManager.Instance.GetString("Form1_CacheInfo_PolicyImages")}\n" +
                         $"- {LanguageManager.Instance.GetString("Form1_CacheInfo_PolicyAPI")}";

            MessageBox.Show(message, LanguageManager.Instance.GetString("Form1_CacheInfo_Title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private async Task ClearCache()
        {
            var result = MessageBox.Show(LanguageManager.Instance.GetString("Form1_ClearCache_ConfirmMessage"),
                LanguageManager.Instance.GetString("Form1_ClearCache_ConfirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _cacheManager.ClearCacheAsync();
                    _cacheHits = 0;
                    _cacheMisses = 0;
                    UpdateCacheStatus();
                    LogManager.Instance.Info("缓存已清理");
                    MessageBox.Show(LanguageManager.Instance.GetString("Form1_ClearCache_Success"), LanguageManager.Instance.GetString("Message_Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理缓存时出错", ex);
                    MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_ClearCache_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 显示代理设置对话框
        /// </summary>
        private void ShowProxySettings()
        {
            if (_proxyForm == null || _proxyForm.IsDisposed)
            {
                _proxyForm = new ProxySettingsForm(_configManager);
                _proxyForm.FormClosed += (s, args) =>
                {
                    // 更新状态栏显示
                    UpdateCacheStatus();
                    LogManager.Instance.Info("代理设置窗口已关闭");
                };
            }
            
            if (!_proxyForm.Visible)
            {
                _proxyForm.Show(this);
            }
            else
            {
                _proxyForm.BringToFront();
                _proxyForm.Focus();
            }
        }

        /// <summary>
        /// 显示日志查看器
        /// </summary>
        private void ShowLogViewer()
        {
            if (_logViewerForm == null || _logViewerForm.IsDisposed)
            {
                _logViewerForm = new LogViewerForm();
                _logViewerForm.FormClosed += (s, args) =>
                {
                    LogManager.Instance.Info("日志查看器窗口已关闭");
                };
            }
            
            if (!_logViewerForm.Visible)
            {
                _logViewerForm.Show(this);
            }
            else
            {
                _logViewerForm.BringToFront();
                _logViewerForm.Focus();
            }
        }


        /// <summary>
        /// 窗体即将关闭时的处理
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            LogManager.Instance.Info("OnFormClosing 开始执行");
            
            try
            {
                // 关闭所有打开的子窗口
                LogManager.Instance.Info("准备关闭所有子窗口");
                _settingsForm?.Close();
                _proxyForm?.Close();
                _logViewerForm?.Close();
                _updateForm?.Close();
                _aboutForm?.Close();
                
                // 同步清理资源
                LogManager.Instance.Info("准备注销全局热键");
                UnregisterGlobalHotkeys();
                
                // 停止 WebView2
                if (webView2 != null && webView2.CoreWebView2 != null)
                {
                    LogManager.Instance.Info("准备停止 WebView2");
                    try
                    {
                        webView2.CoreWebView2.Stop();
                        webView2.CoreWebView2.Navigate("about:blank");
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error("停止 WebView2 时出错", ex);
                    }
                }
                
                // 释放缓存管理器
                LogManager.Instance.Info("准备释放缓存管理器");
                _cacheManager?.Dispose();
                
                // 刷新ConsoleLogger的缓冲区并关闭
                LogManager.Instance.Info("准备刷新ConsoleLogger缓冲区");
                ConsoleLogger.Instance.FlushAsync().Wait(TimeSpan.FromSeconds(2));
                
                LogManager.Instance.Info("OnFormClosing 执行完毕，准备退出进程");
                
                // 立即强制退出进程
                LogManager.Instance.Info("强制调用 Environment.Exit(0)");
                LogManager.Instance.FlushAsync().Wait(1000); // 等待最多1秒让日志写入
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                // 即使出错也要确保退出
                LogManager.Instance.Error("OnFormClosing 执行时出错", ex);
                Environment.Exit(1);
            }
            
            base.OnFormClosing(e);
        }

        /// <summary>
        /// 窗体关闭时清理资源（备用方法）
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LogManager.Instance.Info("OnFormClosed 开始执行（如果看到这条日志说明 OnFormClosing 没有正常退出）");
            
            try
            {
                base.OnFormClosed(e);
                
                // 如果执行到这里，说明 OnFormClosing 中的 Environment.Exit 没有生效
                // 再次尝试退出
                Environment.Exit(0);
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 全屏切换菜单项点击事件
        /// </summary>
        private void toggleFullScreenMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        /// <summary>
        /// 静音切换菜单项点击事件
        /// </summary>
        private void toggleMuteMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleMute();
        }

        /// <summary>
        /// 截图菜单项点击事件
        /// </summary>
        private void screenshotMenuItem_Click(object? sender, EventArgs e)
        {
            _ = CaptureUnityCanvas();
        }

        /// <summary>
        /// 检查更新菜单项点击事件
        /// </summary>
        private async void checkUpdateMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_CheckingUpdate");
                
                var updateManager = new UpdateManager();
                var updateInfo = await updateManager.CheckForUpdatesAsync();
                
                if (updateInfo != null && updateInfo.IsUpdateRequired())
                {
                    // 显示更新窗口（非模式）
                    if (_updateForm == null || _updateForm.IsDisposed)
                    {
                        _updateForm = new UpdateForm(updateInfo);
                        _updateForm.FormClosed += (s, args) =>
                        {
                            LogManager.Instance.Info("更新窗口已关闭");
                        };
                    }
                    
                    if (!_updateForm.Visible)
                    {
                        _updateForm.Show(this);
                    }
                    else
                    {
                        _updateForm.BringToFront();
                        _updateForm.Focus();
                    }
                }
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_Ready");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检查更新时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_CheckUpdate_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_CheckUpdateFailed");
            }
        }

        /// <summary>
        /// 关于菜单项点击事件
        /// </summary>
        private void aboutMenuItem_Click(object? sender, EventArgs e)
        {
            if (_aboutForm == null || _aboutForm.IsDisposed)
            {
                _aboutForm = new AboutForm();
                _aboutForm.FormClosed += (s, args) =>
                {
                    LogManager.Instance.Info("关于窗口已关闭");
                };
            }
            
            if (!_aboutForm.Visible)
            {
                _aboutForm.Show(this);
            }
            else
            {
                _aboutForm.BringToFront();
                _aboutForm.Focus();
            }
        }

        /// <summary>
        /// 捐助菜单项点击事件
        /// </summary>
        private void donateToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                const string donateUrl = "https://ko-fi.com/magicnumber";
                
                LogManager.Instance.Info($"打开捐助页面：{donateUrl}");
                
                // 在默认浏览器中打开捐助链接
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = donateUrl,
                    UseShellExecute = true
                });
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_OpenedDonate");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("打开捐助页面时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_OpenDonate_Failed")}: {ex.Message}",
                    LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 退出登录菜单项点击事件
        /// </summary>
        private async void logoutMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(LanguageManager.Instance.GetString("Form1_Logout_ConfirmMessage"),
                    LanguageManager.Instance.GetString("Form1_Logout_ConfirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_LoggingOut");
                    LogManager.Instance.Info("用户选择退出登录");

                    // 清除所有 cookies
                    if (_cookieManager != null)
                    {
                        await _cookieManager.DeleteAllCookiesAsync();
                        LogManager.Instance.Info("已清除所有 Cookies");
                    }

                    // 重置登录状态
                    _currentLoginState = LoginFlowState.Initial;

                    // 导航到登录页面
                    var loginUrl = _configManager.Config.BaseURL.TrimEnd('/') + _configManager.Config.Login.LoginUrl;
                    webView2.CoreWebView2.Navigate(loginUrl);
                    
                    statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_LoggedOut");
                    LogManager.Instance.Info($"已导航到登录页面：{loginUrl}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("退出登录时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_Logout_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_LogoutFailed");
            }
        }

        /// <summary>
        /// 官方讨论区菜单项点击事件
        /// </summary>
        private void forumMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                // 获取当前主站域名
                var currentUrl = webView2.Source?.ToString() ?? "";
                var baseUrl = _configManager.Config.BaseURL;
                
                // 从 baseURL 中提取域名
                Uri baseUri = new Uri(baseUrl);
                var domain = baseUri.Host;
                
                // 构建论坛URL
                var forumUrl = $"https://{domain}/cn/forum/cherry-tale";
                
                LogManager.Instance.Info($"打开官方讨论区：{forumUrl}");
                
                // 在新标签页中打开（实际上是在默认浏览器中打开）
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = forumUrl,
                    UseShellExecute = true
                });
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_OpenedForum");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("打开官方讨论区时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_OpenForum_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 官方Discord菜单项点击事件
        /// </summary>
        private void discordMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                const string discordUrl = "https://discord.gg/h6q59YNwGW";
                
                LogManager.Instance.Info($"打开官方Discord：{discordUrl}");
                
                // 在默认浏览器中打开Discord邀请链接
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = discordUrl,
                    UseShellExecute = true
                });
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_OpenedDiscord");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("打开官方Discord时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_OpenDiscord_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// GitHub菜单项点击事件
        /// </summary>
        private void githubMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                const string githubUrl = "https://github.com/a11s/ctwebplayer";
                
                LogManager.Instance.Info($"打开GitHub源码：{githubUrl}");
                
                // 在默认浏览器中打开GitHub链接
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = githubUrl,
                    UseShellExecute = true
                });
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_OpenedGitHub");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("打开GitHub时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_OpenGitHub_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 手机版下载菜单项点击事件
        /// </summary>
        private void mobileDownloadMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                const string mobileDownloadUrl = "https://game.erolabsshare.net/app/627a8937/Cherry_Tale";
                
                LogManager.Instance.Info($"打开手机版下载：{mobileDownloadUrl}");
                
                // 在默认浏览器中打开手机版下载链接
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = mobileDownloadUrl,
                    UseShellExecute = true
                });
                
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Status_OpenedMobileDownload");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("打开手机版下载页面时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_OpenMobileDownload_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 以下方法已被全局热键替代，不再使用
        /*
        /// <summary>
        /// 窗体键盘事件处理（F11和F4快捷键）
        /// </summary>
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // 添加调试日志
            LogManager.Instance.Info($"Form1_KeyDown触发：键码 = {e.KeyCode}, Alt = {e.Alt}, Ctrl = {e.Control}, Shift = {e.Shift}");
            
            if (e.KeyCode == Keys.F11)
            {
                LogManager.Instance.Info("检测到F11按键，准备切换全屏");
                ToggleFullScreen();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                LogManager.Instance.Info("检测到F4按键，准备切换静音");
                ToggleMute();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        
        /// <summary>
        /// WebView2的PreviewKeyDown事件处理
        /// </summary>
        private void WebView2_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            LogManager.Instance.Info($"WebView2_PreviewKeyDown触发：键码 = {e.KeyCode}");
            
            // 处理F11和F4键
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.F4)
            {
                // 标记为输入键，防止WebView2默认处理
                e.IsInputKey = true;
                
                // 触发Form的KeyDown事件
                var keyEventArgs = new KeyEventArgs(e.KeyCode);
                Form1_KeyDown(this, keyEventArgs);
            }
        }
        */
        
        /// <summary>
        /// 处理窗口关闭请求
        /// </summary>
        private void CoreWebView2_WindowCloseRequested(object? sender, object e)
        {
            // 防止网页关闭窗口
            LogManager.Instance.Info("网页尝试关闭窗口，已阻止");
        }

        /// <summary>
        /// 切换全屏模式
        /// </summary>
        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                // 进入全屏模式
                EnterFullScreen();
            }
            else
            {
                // 退出全屏模式
                ExitFullScreen();
            }
        }

        /// <summary>
        /// 进入全屏模式
        /// </summary>
        private void EnterFullScreen()
        {
            // 保存当前状态
            _previousWindowState = this.WindowState;
            _previousBorderStyle = this.FormBorderStyle;
            _previousBounds = this.Bounds;
            _previousToolStripVisible = toolStrip1.Visible;
            _previousStatusStripVisible = statusStrip1.Visible;

            // 隐藏工具栏和状态栏
            toolStrip1.Visible = false;
            statusStrip1.Visible = false;

            // 设置无边框
            this.FormBorderStyle = FormBorderStyle.None;

            // 设置窗口状态为最大化
            this.WindowState = FormWindowState.Maximized;

            // 设置全屏标志
            _isFullScreen = true;

            // 显示全屏提示
            ShowFullScreenTip();
            
            LogManager.Instance.Info("已进入全屏模式");
        }

        /// <summary>
        /// 退出全屏模式
        /// </summary>
        private void ExitFullScreen()
        {
            // 恢复工具栏和状态栏的可见性
            toolStrip1.Visible = _previousToolStripVisible;
            statusStrip1.Visible = _previousStatusStripVisible;

            // 恢复边框样式
            this.FormBorderStyle = _previousBorderStyle;

            // 恢复窗口状态
            this.WindowState = _previousWindowState;

            // 如果之前不是最大化状态，恢复窗口位置和大小
            if (_previousWindowState != FormWindowState.Maximized)
            {
                this.Bounds = _previousBounds;
            }

            // 清除全屏标志
            _isFullScreen = false;

            LogManager.Instance.Info("已退出全屏模式");
        }

        /// <summary>
        /// 显示全屏提示
        /// </summary>
        private async void ShowFullScreenTip()
        {
            try
            {
                // 获取提示文本并转义单引号
                var tipText = LanguageManager.Instance.GetString("Form1_Tip_PressF11ToExitFullscreen").Replace("'", "\\'");
                
                // 在网页中显示提示
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提示
                        var existingTip = document.getElementById('fullscreen-tip');
                        if (existingTip) {{
                            existingTip.remove();
                        }}
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'fullscreen-tip';
                        tip.innerHTML = '{tipText}';
                        tip.style.cssText = `
                            position: fixed;
                            top: 20px;
                            right: 20px;
                            background-color: rgba(0, 0, 0, 0.7);
                            color: white;
                            padding: 10px 20px;
                            border-radius: 5px;
                            font-size: 14px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: none;
                            transition: opacity 0.5s ease;
                        `;
                        document.body.appendChild(tip);
                        
                        // 3秒后淡出
                        setTimeout(function() {{
                            tip.style.opacity = '0';
                            setTimeout(function() {{
                                tip.remove();
                            }}, 500);
                        }}, 3000);
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示全屏提示时出错", ex);
            }
        }

        /// <summary>
        /// 切换静音状态
        /// </summary>
        private async void ToggleMute()
        {
            try
            {
                if (webView2.CoreWebView2 == null)
                {
                    LogManager.Instance.Warning("WebView2尚未初始化，无法切换静音");
                    return;
                }
                
                _isMuted = !_isMuted;
                
                // 首先尝试使用WebView2的原生静音API（如果可用）
                try
                {
                    webView2.CoreWebView2.IsMuted = _isMuted;
                    LogManager.Instance.Info($"使用WebView2原生API设置静音状态：{_isMuted}");
                }
                catch (Exception apiEx)
                {
                    LogManager.Instance.Warning($"WebView2原生静音API不可用：{apiEx.Message}");
                }
                
                // 使用增强的JavaScript控制页面音频
                var script = $@"
                    (function() {{
                        var muteState = {_isMuted.ToString().ToLower()};
                        var muteCount = 0;
                        
                        // 定义静音函数
                        function muteElement(element) {{
                            if (element && (element.tagName === 'AUDIO' || element.tagName === 'VIDEO')) {{
                                element.muted = muteState;
                                element.volume = muteState ? 0 : 1;
                                muteCount++;
                                
                                // 监听播放事件，确保静音状态保持
                                element.addEventListener('play', function() {{
                                    element.muted = muteState;
                                }});
                                
                                element.addEventListener('volumechange', function() {{
                                    if (muteState && !element.muted) {{
                                        element.muted = true;
                                        element.volume = 0;
                                    }}
                                }});
                            }}
                        }}
                        
                        // 获取所有的音频和视频元素
                        var mediaElements = document.querySelectorAll('audio, video');
                        mediaElements.forEach(muteElement);
                        
                        // 处理Unity WebGL音频
                        if (window.WEBAudio && window.WEBAudio.audioContext) {{
                            try {{
                                if (muteState) {{
                                    window.WEBAudio.audioContext.suspend();
                                }} else {{
                                    window.WEBAudio.audioContext.resume();
                                }}
                                console.log('Unity WebGL audio context ' + (muteState ? 'suspended' : 'resumed'));
                            }} catch (e) {{
                                console.log('Failed to control Unity audio context:', e);
                            }}
                        }}
                        
                        // 处理Web Audio API
                        if (window.AudioContext || window.webkitAudioContext) {{
                            var audioContexts = [];
                            
                            // 拦截AudioContext创建
                            var OriginalAudioContext = window.AudioContext || window.webkitAudioContext;
                            var NewAudioContext = function() {{
                                var ctx = new OriginalAudioContext();
                                audioContexts.push(ctx);
                                if (muteState) {{
                                    ctx.suspend();
                                }}
                                return ctx;
                            }};
                            window.AudioContext = NewAudioContext;
                            if (window.webkitAudioContext) {{
                                window.webkitAudioContext = NewAudioContext;
                            }}
                            
                            // 处理已存在的AudioContext
                            if (window.audioContext) {{
                                audioContexts.push(window.audioContext);
                            }}
                            
                            audioContexts.forEach(function(ctx) {{
                                if (muteState) {{
                                    ctx.suspend();
                                }} else {{
                                    ctx.resume();
                                }}
                            }});
                        }}
                        
                        // 处理iframe中的媒体元素
                        var iframes = document.querySelectorAll('iframe');
                        iframes.forEach(function(iframe) {{
                            try {{
                                var iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
                                var iframeMedia = iframeDoc.querySelectorAll('audio, video');
                                iframeMedia.forEach(muteElement);
                            }} catch (e) {{
                                // 跨域iframe无法访问
                                console.log('Cannot access iframe content:', e);
                            }}
                        }});
                        
                        // 使用MutationObserver监听新添加的媒体元素
                        var observer = new MutationObserver(function(mutations) {{
                            mutations.forEach(function(mutation) {{
                                mutation.addedNodes.forEach(function(node) {{
                                    if (node.nodeType === 1) {{ // Element node
                                        if (node.tagName === 'AUDIO' || node.tagName === 'VIDEO') {{
                                            muteElement(node);
                                        }}
                                        // 检查子节点
                                        var childMedia = node.querySelectorAll ? node.querySelectorAll('audio, video') : [];
                                        childMedia.forEach(muteElement);
                                    }}
                                }});
                            }});
                        }});
                        
                        // 开始观察整个文档
                        observer.observe(document.body, {{
                            childList: true,
                            subtree: true
                        }});
                        
                        // 保存观察器引用，以便后续使用
                        window.__muteObserver = observer;
                        window.__currentMuteState = muteState;
                        
                        console.log('Mute state set to:', muteState, 'Muted elements:', muteCount);
                        return muteState;
                    }})();
                ";
                
                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 如果是游戏页面，尝试额外的静音方法
                var currentUrl = webView2.Source?.ToString() ?? "";
                if (currentUrl.Contains("unity") || currentUrl.Contains("game"))
                {
                    await ApplyGameSpecificMute();
                }
                
                // 更新状态栏显示
                UpdateCacheStatus();
                
                // 显示静音状态提示
                ShowMuteTip(_isMuted);
                
                LogManager.Instance.Info($"静音状态已切换为：{(_isMuted ? "开启" : "关闭")}");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("切换静音状态时出错", ex);
                MessageBox.Show($"{LanguageManager.Instance.GetString("Form1_ToggleMute_Failed")}: {ex.Message}", LanguageManager.Instance.GetString("Message_Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 应用游戏特定的静音方法
        /// </summary>
        private async Task ApplyGameSpecificMute()
        {
            try
            {
                var script = $@"
                    (function() {{
                        var muteState = {_isMuted.ToString().ToLower()};
                        
                        // Unity特定的静音方法
                        if (window.unityInstance) {{
                            try {{
                                if (window.unityInstance.SendMessage) {{
                                    window.unityInstance.SendMessage('AudioManager', 'SetMute', muteState.toString());
                                }}
                                if (window.unityInstance.Module && window.unityInstance.Module.setMute) {{
                                    window.unityInstance.Module.setMute(muteState);
                                }}
                            }} catch (e) {{
                                console.log('Unity mute failed:', e);
                            }}
                        }}
                        
                        // 检查全局音频变量
                        var audioVars = ['gameAudio', 'soundManager', 'audioManager', 'audio'];
                        audioVars.forEach(function(varName) {{
                            if (window[varName]) {{
                                try {{
                                    if (typeof window[varName].mute === 'function') {{
                                        window[varName].mute(muteState);
                                    }} else if (typeof window[varName].setMute === 'function') {{
                                        window[varName].setMute(muteState);
                                    }} else if (typeof window[varName].muted !== 'undefined') {{
                                        window[varName].muted = muteState;
                                    }}
                                }} catch (e) {{
                                    console.log('Failed to mute ' + varName + ':', e);
                                }}
                            }}
                        }});
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("应用游戏特定静音时出错", ex);
            }
        }

        /// <summary>
        /// 显示静音状态提示
        /// </summary>
        private async void ShowMuteTip(bool isMuted)
        {
            try
            {
                var message = isMuted ? LanguageManager.Instance.GetString("Form1_Notification_MuteOn") : LanguageManager.Instance.GetString("Form1_Notification_MuteOff");
                var icon = isMuted ? "🔇" : "🔊";
                
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提示
                        var existingTip = document.getElementById('mute-tip');
                        if (existingTip) {{
                            existingTip.remove();
                        }}
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'mute-tip';
                        tip.innerHTML = '{icon} {message}';
                        tip.style.cssText = `
                            position: fixed;
                            top: 50%;
                            left: 50%;
                            transform: translate(-50%, -50%);
                            background-color: rgba(0, 0, 0, 0.8);
                            color: white;
                            padding: 20px 40px;
                            border-radius: 10px;
                            font-size: 18px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: none;
                            transition: opacity 0.3s ease;
                        `;
                        document.body.appendChild(tip);
                        
                        // 1.5秒后淡出
                        setTimeout(function() {{
                            tip.style.opacity = '0';
                            setTimeout(function() {{
                                tip.remove();
                            }}, 300);
                        }}, 1500);
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示静音提示时出错", ex);
            }
        }

        /// <summary>
        /// 检测并更新 BaseURL（如果检测到 ero-labs 域名跳转）
        /// </summary>
        private async Task CheckAndUpdateBaseUrl()
        {
            try
            {
                var currentUrl = webView2.Source?.ToString() ?? "";
                
                // 使用正则表达式匹配 ero-labs 域名模式（支持 game. 和 mg. 前缀）
                var eroLabsRegex = new System.Text.RegularExpressions.Regex(@"https?://(game|mg)\.ero-labs\.[^/]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var match = eroLabsRegex.Match(currentUrl);
                
                if (match.Success)
                {
                    var detectedBaseUrl = match.Value;
                    var currentBaseUrl = _configManager.Config.BaseURL;
                    
                    // 如果检测到的域名与当前配置的不同，则更新配置
                    if (!string.Equals(detectedBaseUrl, currentBaseUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        LogManager.Instance.Info($"检测到 ero-labs 域名跳转：从 {currentBaseUrl} 到 {detectedBaseUrl}");
                        
                        // 更新配置中的 BaseURL
                        await _configManager.UpdateBaseUrlAsync(detectedBaseUrl);
                        
                        LogManager.Instance.Info($"已自动更新 BaseURL 配置为：{detectedBaseUrl}");
                        
                        // 显示提示信息
                        await ShowBaseUrlUpdateTip(detectedBaseUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检测和更新 BaseURL 时出错", ex);
            }
        }

        /// <summary>
        /// 构建完整的游戏URL
        /// </summary>
        private string BuildGameUrl(string baseUrl)
        {
            // 确保 baseUrl 以斜杠结尾
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            
            // 添加游戏路径和参数
            return $"{baseUrl}cn/cloud_game.html?id=27&connect_type=1&connection_id=20";
        }

        /// <summary>
        /// 显示 BaseURL 更新提示
        /// </summary>
        private async Task ShowBaseUrlUpdateTip(string newBaseUrl)
        {
            try
            {
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提示
                        var existingTip = document.getElementById('baseurl-update-tip');
                        if (existingTip) {{
                            existingTip.remove();
                        }}
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'baseurl-update-tip';
                        tip.innerHTML = '🔄 已自动更新服务器地址为：{newBaseUrl.Replace("'", "\\'")}';
                        tip.style.cssText = `
                            position: fixed;
                            top: 20px;
                            left: 50%;
                            transform: translateX(-50%);
                            background-color: rgba(0, 128, 0, 0.9);
                            color: white;
                            padding: 15px 30px;
                            border-radius: 8px;
                            font-size: 14px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: none;
                            transition: opacity 0.5s ease;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
                        `;
                        document.body.appendChild(tip);
                        
                        // 5秒后淡出
                        setTimeout(function() {{
                            tip.style.opacity = '0';
                            setTimeout(function() {{
                                tip.remove();
                            }}, 500);
                        }}, 5000);
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示 BaseURL 更新提示时出错", ex);
            }
        }

        /// <summary>
        /// 启动登录引导流程
        /// </summary>
        private async Task StartLoginFlowAsync()
        {
            try
            {
                LogManager.Instance.Info($"开始登录流程，当前状态：{_currentLoginState}");
                
                switch (_currentLoginState)
                {
                    case LoginFlowState.Initial:
                        // 检查是否有登录 cookie
                        bool hasCookie = await _cookieManager.HasCookieAsync(_configManager.Config.Login.CookieName);
                        
                        if (hasCookie)
                        {
                            _currentLoginState = LoginFlowState.HasCookie;
                            LogManager.Instance.Info("检测到登录 cookie，直接进入游戏");
                            NavigateToGame();
                        }
                        else
                        {
                            _currentLoginState = LoginFlowState.NoCookie;
                            LogManager.Instance.Info("未检测到登录 cookie，显示登录对话框");
                            ShowLoginDialog();
                        }
                        break;
                        
                    case LoginFlowState.NoCookie:
                        // 已经在 ShowLoginDialog 中处理
                        break;
                        
                    case LoginFlowState.HasAccount:
                        // 导航到登录页面
                        var loginUrl = _configManager.Config.BaseURL.TrimEnd('/') + _configManager.Config.Login.LoginUrl;
                        webView2.CoreWebView2.Navigate(loginUrl);
                        txtAddress.Text = loginUrl;
                        LogManager.Instance.Info($"导航到登录页面：{loginUrl}");
                        break;
                        
                    case LoginFlowState.NoAccount:
                        // 导航到注册页面
                        var registerUrl = _configManager.Config.Login.RegisterUrl;
                        webView2.CoreWebView2.Navigate(registerUrl);
                        txtAddress.Text = registerUrl;
                        LogManager.Instance.Info($"导航到注册页面：{registerUrl}");
                        break;
                        
                    case LoginFlowState.LoginComplete:
                        // 登录完成，导航到游戏页面
                        NavigateToGame();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("登录流程出错", ex);
                // 出错时直接进入游戏
                NavigateToGame();
            }
        }

        /// <summary>
        /// 显示登录对话框
        /// </summary>
        private void ShowLoginDialog()
        {
            try
            {
                var result = LoginDialog.ShowLoginDialog(this, _configManager.Config.Login.SkipEnabled);
                
                switch (result)
                {
                    case LoginDialog.DialogResultType.HasAccount:
                        _currentLoginState = LoginFlowState.HasAccount;
                        LogManager.Instance.Info("用户选择：已有账号");
                        _ = StartLoginFlowAsync();
                        break;
                        
                    case LoginDialog.DialogResultType.NoAccount:
                        _currentLoginState = LoginFlowState.NoAccount;
                        LogManager.Instance.Info("用户选择：新注册");
                        _ = StartLoginFlowAsync();
                        break;
                        
                    case LoginDialog.DialogResultType.Skip:
                        LogManager.Instance.Info("用户选择：跳过登录");
                        NavigateToGame();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示登录对话框时出错", ex);
                NavigateToGame();
            }
        }

        /// <summary>
        /// 导航到游戏页面
        /// </summary>
        private void NavigateToGame()
        {
            _currentLoginState = LoginFlowState.GameLoaded;
            string gameUrl = BuildGameUrl(_configManager.Config.BaseURL);
            webView2.CoreWebView2.Navigate(gameUrl);
            txtAddress.Text = gameUrl;
            LogManager.Instance.Info($"导航到游戏页面：{gameUrl}");
        }

        /// <summary>
        /// 检查登录流程状态
        /// </summary>
        private async Task CheckLoginFlowStatus()
        {
            try
            {
                var currentUrl = webView2.Source?.ToString() ?? "";
                
                // 检查是否是从登录/注册后重定向到 index.html 或 profile.html
                if ((currentUrl.Contains("/cn/index.html") || currentUrl.Contains("/cn/profile.html")) &&
                    (_currentLoginState == LoginFlowState.HasAccount || _currentLoginState == LoginFlowState.NoAccount))
                {
                    LogManager.Instance.Info($"检测到登录/注册后的重定向页面：{currentUrl}，准备检查 cookie");
                    
                    // 等待一段时间确保 cookie 已设置
                    await Task.Delay(1000);
                    
                    // 检查是否有登录 cookie
                    bool hasCookie = await _cookieManager.HasCookieAsync(_configManager.Config.Login.CookieName);
                    
                    if (hasCookie)
                    {
                        _currentLoginState = LoginFlowState.LoginComplete;
                        LogManager.Instance.Info("检测到登录 cookie，准备跳转到游戏页面");
                        
                        // 延迟一下再跳转，给用户一个视觉反馈
                        await Task.Delay(500);
                        NavigateToGame();
                    }
                    else
                    {
                        // 如果没有检测到 cookie，可能需要重试
                        LogManager.Instance.Warning("未检测到登录 cookie，可能登录未成功");
                        
                        // 重试最多3次
                        for (int i = 0; i < 3; i++)
                        {
                            await Task.Delay(500);
                            hasCookie = await _cookieManager.HasCookieAsync(_configManager.Config.Login.CookieName);
                            
                            if (hasCookie)
                            {
                                _currentLoginState = LoginFlowState.LoginComplete;
                                LogManager.Instance.Info($"第 {i + 2} 次检测到登录 cookie");
                                NavigateToGame();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检查登录流程状态时出错", ex);
            }
        }

        /// <summary>
        /// 从完整域名中提取主域名
        /// </summary>
        /// <param name="domain">完整域名，如 mg.ero-labs.live</param>
        /// <returns>主域名，如 ero-labs.live</returns>
        private string GetMainDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return domain;
            
            var parts = domain.Split('.');
            if (parts.Length >= 2)
            {
                // 返回最后两部分作为主域名（如 ero-labs.live）
                return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
            }
            
            return domain;
        }

        /// <summary>
        /// 重写窗口过程以处理热键消息
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // 处理WM_HOTKEY消息
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                
                switch (hotkeyId)
                {
                    case HOTKEY_ID_F11:
                        LogManager.Instance.Info("检测到F11全局热键，准备切换全屏");
                        ToggleFullScreen();
                        break;
                        
                    case HOTKEY_ID_F4:
                        LogManager.Instance.Info("检测到F4全局热键，准备切换静音");
                        ToggleMute();
                        break;
                        
                    case HOTKEY_ID_F10:
                        LogManager.Instance.Info("检测到F10全局热键，准备截图");
                        _ = CaptureUnityCanvas();
                        break;
                }
            }
            
            // 调用基类的WndProc处理其他消息
            base.WndProc(ref m);
        }

        /// <summary>
        /// 截取Unity Canvas的内容
        /// </summary>
        private async Task CaptureUnityCanvas()
        {
            try
            {
                if (webView2.CoreWebView2 == null)
                {
                    LogManager.Instance.Warning("WebView2尚未初始化，无法截图");
                    return;
                }

                // 全屏模式下的特殊处理：先检查是否在全屏模式
                var isFullScreenMode = _isFullScreen;
                
                LogManager.Instance.Info("开始执行Unity WebGL截图");
                
                // 方案1：首先尝试使用改进的canvas截图方法
                var captureResult = await TryCaptureUnityCanvasImproved();
                
                if (captureResult.Success)
                {
                    // 检测是否为黑屏
                    if (IsBlackImage(captureResult.ImageData))
                    {
                        LogManager.Instance.Warning("检测到截图为黑屏，尝试备用方案");
                        
                        // 方案2：尝试使用Unity内置截图命令
                        captureResult = await TryCaptureWithUnityCommand();
                        
                        if (!captureResult.Success || IsBlackImage(captureResult.ImageData))
                        {
                            // 方案3：使用WebView2截图API
                            LogManager.Instance.Info("尝试WebView2截图API");
                            captureResult = await TryCaptureWithWebView2API();
                        }
                    }
                    
                    if (captureResult.Success && !IsBlackImage(captureResult.ImageData))
                    {
                        await SaveScreenshot(captureResult.ImageData, isFullScreenMode);
                        return;
                    }
                }
                
                // 显示错误信息
                string displayError = captureResult.ErrorMessage ?? LanguageManager.Instance.GetString("Form1_Screenshot_Failed");
                LogManager.Instance.Warning($"截图失败：{displayError}");
                statusLabel.Text = LanguageManager.Instance.GetString("Form1_Screenshot_Error");
                
                // 在全屏模式下使用页面内提示
                if (isFullScreenMode)
                {
                    await ShowScreenshotErrorInPage(displayError);
                }
                else
                {
                    MessageBox.Show(displayError, LanguageManager.Instance.GetString("Form1_Screenshot_ErrorTitle"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("截取Unity Canvas时出错", ex);
                var errorMsg = string.Format(LanguageManager.Instance.GetString("Form1_Screenshot_Failed"), ex.Message);
                
                if (_isFullScreen)
                {
                    await ShowScreenshotErrorInPage(errorMsg);
                }
                else
                {
                    MessageBox.Show(errorMsg, LanguageManager.Instance.GetString("Form1_Screenshot_ErrorTitle"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 使用改进的方法尝试捕获Unity Canvas
        /// </summary>
        private async Task<CaptureResult> TryCaptureUnityCanvasImproved()
        {
            try
            {
                // 执行JavaScript来获取unity-canvas的内容
                var script = @"
                    (function() {
                        try {
                            // 查找canvas元素
                            var canvas = document.getElementById('unity-canvas');
                            
                            if (!canvas) {
                                console.log('Canvas not found by ID, trying other methods');
                                canvas = document.querySelector('canvas');
                                
                                if (!canvas) {
                                    return JSON.stringify({ success: false, error: 'canvas_not_found' });
                                }
                            }
                            
                            console.log('Found canvas:', canvas.id || 'no-id', 'size:', canvas.width, 'x', canvas.height);
                            
                            // 尝试直接截图
                            var dataUrl = canvas.toDataURL('image/png');
                            
                            if (!dataUrl || dataUrl === 'data:,' || dataUrl.length < 100) {
                                console.log('toDataURL returned empty or invalid data');
                                
                                // 尝试获取WebGL context
                                var gl = canvas.getContext('webgl') || canvas.getContext('webgl2');
                                if (gl && !gl.isContextLost()) {
                                    console.log('WebGL context is active but cannot capture');
                                    return JSON.stringify({ success: false, error: 'webgl_capture_not_supported' });
                                } else {
                                    return JSON.stringify({ success: false, error: 'empty_canvas' });
                                }
                            }
                            
                            console.log('Canvas captured successfully, dataUrl length:', dataUrl.length);
                            
                            return JSON.stringify({
                                success: true,
                                dataUrl: dataUrl
                            });
                            
                        } catch (e) {
                            console.error('Screenshot error:', e);
                            return JSON.stringify({
                                success: false,
                                error: e.message || 'unknown_error'
                            });
                        }
                    })();
                ";

                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 记录原始结果用于调试
                LogManager.Instance.Info($"JavaScript原始返回值长度：{result?.Length ?? 0}");
                
                // 解析结果
                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    try
                    {
                        // ExecuteScriptAsync返回的结果是JSON编码的字符串，需要先解码
                        string jsonResult;
                        
                        // 如果结果以引号开始和结束，说明是JSON字符串
                        if (result.StartsWith("\"") && result.EndsWith("\""))
                        {
                            // 使用System.Text.Json来正确解码JSON字符串
                            jsonResult = System.Text.Json.JsonSerializer.Deserialize<string>(result) ?? "";
                        }
                        else
                        {
                            jsonResult = result;
                        }
                        
                        LogManager.Instance.Info($"解码后的JSON结果前100字符：{jsonResult.Substring(0, Math.Min(100, jsonResult.Length))}");
                        
                        // 使用正则表达式来解析JSON（更稳定）
                        var successMatch = System.Text.RegularExpressions.Regex.Match(jsonResult, @"""success""\s*:\s*(true|false)");
                        
                        if (!successMatch.Success)
                        {
                            LogManager.Instance.Warning($"无法找到success字段，JSON内容：{jsonResult}");
                            return new CaptureResult { Success = false, ErrorMessage = "Invalid JSON format - no success field found" };
                        }
                        
                        bool success = successMatch.Groups[1].Value == "true";
                        
                        if (success)
                        {
                            // 提取dataUrl
                            var dataUrlMatch = System.Text.RegularExpressions.Regex.Match(jsonResult, @"""dataUrl""\s*:\s*""([^""]+)""");
                            
                            if (!dataUrlMatch.Success)
                            {
                                return new CaptureResult { Success = false, ErrorMessage = "Invalid JSON format - dataUrl not found" };
                            }
                            
                            var dataUrl = dataUrlMatch.Groups[1].Value;
                            
                            // 处理转义字符
                            dataUrl = dataUrl.Replace("\\/", "/");
                            
                            // 解码base64数据
                            var base64Start = dataUrl.IndexOf("base64,");
                            if (base64Start == -1)
                            {
                                return new CaptureResult { Success = false, ErrorMessage = "Invalid data URL format - no base64 marker" };
                            }
                            base64Start += 7;
                            
                            if (base64Start >= dataUrl.Length)
                            {
                                return new CaptureResult { Success = false, ErrorMessage = "Empty base64 data" };
                            }
                            
                            var base64Data = dataUrl.Substring(base64Start);
                            
                            LogManager.Instance.Info($"Base64数据长度：{base64Data.Length}");
                            
                            var imageBytes = Convert.FromBase64String(base64Data);
                            
                            LogManager.Instance.Info($"解码后的图片大小：{imageBytes.Length} bytes");
                            
                            return new CaptureResult { Success = true, ImageData = imageBytes };
                        }
                        else
                        {
                            // 提取错误信息
                            var errorMatch = System.Text.RegularExpressions.Regex.Match(jsonResult, @"""error""\s*:\s*""([^""]+)""");
                            
                            if (errorMatch.Success)
                            {
                                var errorMsg = errorMatch.Groups[1].Value;
                                LogManager.Instance.Warning($"JavaScript返回错误：{errorMsg}");
                                return new CaptureResult { Success = false, ErrorMessage = GetLocalizedError(errorMsg) };
                            }
                            else
                            {
                                return new CaptureResult { Success = false, ErrorMessage = "Unknown error - no error message provided" };
                            }
                        }
                    }
                    catch (Exception parseEx)
                    {
                        LogManager.Instance.Error($"解析JavaScript结果时出错，原始结果：{result}", parseEx);
                        return new CaptureResult { Success = false, ErrorMessage = $"JSON parse error: {parseEx.Message}" };
                    }
                }
                
                return new CaptureResult { Success = false, ErrorMessage = "No result from JavaScript" };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("改进的canvas截图方法失败", ex);
                return new CaptureResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 尝试使用Unity内置截图命令
        /// </summary>
        private async Task<CaptureResult> TryCaptureWithUnityCommand()
        {
            try
            {
                LogManager.Instance.Info("尝试Unity内置截图命令");
                
                var script = @"
                    (function() {
                        // 尝试通过Unity实例发送截图命令
                        if (window.unityInstance && window.unityInstance.SendMessage) {
                            try {
                                // 创建临时canvas用于接收截图数据
                                window.__screenshotCanvas = document.createElement('canvas');
                                window.__screenshotReady = false;
                                window.__screenshotData = null;
                                
                                // 监听截图完成事件
                                window.__onScreenshotReady = function(base64Data) {
                                    window.__screenshotData = base64Data;
                                    window.__screenshotReady = true;
                                };
                                
                                // 发送截图命令到Unity
                                window.unityInstance.SendMessage('ScreenCapture', 'TakeScreenshot', '');
                                
                                // 等待截图完成（最多2秒）
                                var waitTime = 0;
                                var checkInterval = setInterval(function() {
                                    if (window.__screenshotReady || waitTime >= 2000) {
                                        clearInterval(checkInterval);
                                        if (window.__screenshotData) {
                                            window.postMessage({
                                                type: 'screenshot',
                                                data: window.__screenshotData
                                            }, '*');
                                        }
                                    }
                                    waitTime += 100;
                                }, 100);
                                
                                return JSON.stringify({ success: true, waiting: true });
                            } catch (e) {
                                return JSON.stringify({ success: false, error: 'Unity command failed: ' + e.message });
                            }
                        } else {
                            return JSON.stringify({ success: false, error: 'Unity instance not found' });
                        }
                    })();
                ";
                
                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // Unity截图通常需要时间，所以这里返回失败，让主流程继续尝试其他方法
                return new CaptureResult { Success = false, ErrorMessage = "Unity内置截图不可用" };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("Unity截图命令失败", ex);
                return new CaptureResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 使用WebView2 API进行截图
        /// </summary>
        private async Task<CaptureResult> TryCaptureWithWebView2API()
        {
            try
            {
                LogManager.Instance.Info("使用WebView2 API截图");
                
                // 使用WebView2的CapturePreviewAsync方法
                using (var stream = new MemoryStream())
                {
                    // 设置截图格式为PNG
                    await webView2.CoreWebView2.CapturePreviewAsync(
                        CoreWebView2CapturePreviewImageFormat.Png,
                        stream);
                    
                    var imageBytes = stream.ToArray();
                    
                    if (imageBytes.Length > 0)
                    {
                        LogManager.Instance.Info($"WebView2截图成功，大小：{imageBytes.Length} bytes");
                        return new CaptureResult { Success = true, ImageData = imageBytes };
                    }
                    else
                    {
                        return new CaptureResult { Success = false, ErrorMessage = "WebView2截图为空" };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("WebView2 API截图失败", ex);
                return new CaptureResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 检测图片是否为纯黑色
        /// </summary>
        private bool IsBlackImage(byte[] imageData)
        {
            try
            {
                using (var ms = new MemoryStream(imageData))
                using (var bitmap = new Bitmap(ms))
                {
                    // 采样检查图片是否为纯黑色
                    int sampleSize = 10; // 每隔10个像素采样一次
                    int blackPixelCount = 0;
                    int totalSamples = 0;
                    
                    for (int x = 0; x < bitmap.Width; x += sampleSize)
                    {
                        for (int y = 0; y < bitmap.Height; y += sampleSize)
                        {
                            var pixel = bitmap.GetPixel(x, y);
                            totalSamples++;
                            
                            // 检查像素是否接近黑色（RGB值都小于10）
                            if (pixel.R < 10 && pixel.G < 10 && pixel.B < 10)
                            {
                                blackPixelCount++;
                            }
                        }
                    }
                    
                    // 如果超过95%的采样点都是黑色，认为是黑屏
                    var blackRatio = (double)blackPixelCount / totalSamples;
                    var isBlack = blackRatio > 0.95;
                    
                    if (isBlack)
                    {
                        LogManager.Instance.Warning($"检测到黑屏图片，黑色像素比例：{blackRatio:P}");
                    }
                    
                    return isBlack;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检测黑屏图片时出错", ex);
                return false; // 出错时假设不是黑屏，继续保存
            }
        }

        /// <summary>
        /// 获取本地化的错误信息
        /// </summary>
        private string GetLocalizedError(string errorMsg)
        {
            if (errorMsg == "canvas_not_found")
            {
                return LanguageManager.Instance.GetString("Form1_Screenshot_CanvasNotFound");
            }
            else if (errorMsg == "webgl_capture_not_supported")
            {
                return LanguageManager.Instance.GetString("Form1_Screenshot_WebGLNotSupported");
            }
            else if (errorMsg == "empty_canvas")
            {
                return LanguageManager.Instance.GetString("Form1_Screenshot_EmptyCanvas");
            }
            else if (errorMsg.StartsWith("capture_failed:"))
            {
                var detailError = errorMsg.Substring("capture_failed:".Length).Trim();
                return string.Format(LanguageManager.Instance.GetString("Form1_Screenshot_CaptureFailed"), detailError);
            }
            else
            {
                return string.Format(LanguageManager.Instance.GetString("Form1_Screenshot_Failed"), errorMsg);
            }
        }

        /// <summary>
        /// 截图结果类
        /// </summary>
        private class CaptureResult
        {
            public bool Success { get; set; }
            public byte[] ImageData { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// 保存截图到文件
        /// </summary>
        private async Task SaveScreenshot(byte[] imageData, bool isFullScreenMode = false)
        {
            try
            {
                // 创建Capture目录
                string captureDir = Path.Combine(Application.StartupPath, "Capture");
                if (!Directory.Exists(captureDir))
                {
                    Directory.CreateDirectory(captureDir);
                    LogManager.Instance.Info($"创建截图目录：{captureDir}");
                }

                // 使用时间戳命名文件
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Screenshot_{timestamp}.png";
                string filePath = Path.Combine(captureDir, fileName);

                // 保存图片文件
                await File.WriteAllBytesAsync(filePath, imageData);
                
                LogManager.Instance.Info($"截图已保存：{filePath}");
                
                // 在状态栏显示成功信息（使用多语言）
                statusLabel.Text = string.Format(LanguageManager.Instance.GetString("Form1_Screenshot_SavedTo"), fileName);
                
                // 显示截图成功提示
                await ShowScreenshotTip(fileName, isFullScreenMode);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("保存截图时出错", ex);
                throw;
            }
        }

        /// <summary>
        /// 显示截图成功提示
        /// </summary>
        private async Task ShowScreenshotTip(string fileName, bool isFullScreenMode = false)
        {
            try
            {
                // 获取多语言提示文本
                var tipText = string.Format(LanguageManager.Instance.GetString("Form1_Screenshot_SaveSuccess"), fileName).Replace("'", "\\'");
                
                // 全屏模式下使用更醒目的样式
                var tipStyle = isFullScreenMode ? @"
                            position: fixed;
                            top: 50%;
                            left: 50%;
                            transform: translate(-50%, -50%);
                            background-color: rgba(0, 128, 0, 0.95);
                            color: white;
                            padding: 30px 60px;
                            border-radius: 15px;
                            font-size: 20px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: none;
                            transition: opacity 0.3s ease;
                            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.5);
                            animation: pulse 0.5s ease-out;
                        " : @"
                            position: fixed;
                            top: 50%;
                            left: 50%;
                            transform: translate(-50%, -50%);
                            background-color: rgba(0, 128, 0, 0.9);
                            color: white;
                            padding: 20px 40px;
                            border-radius: 10px;
                            font-size: 16px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: none;
                            transition: opacity 0.3s ease;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
                        ";
                
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提示
                        var existingTip = document.getElementById('screenshot-tip');
                        if (existingTip) {{
                            existingTip.remove();
                        }}
                        
                        // 添加动画样式（仅在全屏模式下）
                        if ({isFullScreenMode.ToString().ToLower()}) {{
                            var style = document.createElement('style');
                            style.innerHTML = `
                                @keyframes pulse {{
                                    0% {{ transform: translate(-50%, -50%) scale(0.9); opacity: 0; }}
                                    50% {{ transform: translate(-50%, -50%) scale(1.05); }}
                                    100% {{ transform: translate(-50%, -50%) scale(1); opacity: 1; }}
                                }}
                            `;
                            document.head.appendChild(style);
                        }}
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'screenshot-tip';
                        tip.innerHTML = '{tipText}';
                        tip.style.cssText = `{tipStyle}`;
                        document.body.appendChild(tip);
                        
                        // 显示时间根据全屏模式调整
                        var displayTime = {(isFullScreenMode ? 3000 : 2000)};
                        
                        // 延时后淡出
                        setTimeout(function() {{
                            tip.style.opacity = '0';
                            setTimeout(function() {{
                                tip.remove();
                            }}, 300);
                        }}, displayTime);
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示截图提示时出错", ex);
            }
        }

        /// <summary>
        /// 在页面内显示截图错误提示（全屏模式下使用）
        /// </summary>
        private async Task ShowScreenshotErrorInPage(string errorMessage)
        {
            try
            {
                var tipText = errorMessage.Replace("'", "\\'").Replace("\n", "\\n");
                
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提示
                        var existingTip = document.getElementById('screenshot-error-tip');
                        if (existingTip) {{
                            existingTip.remove();
                        }}
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'screenshot-error-tip';
                        tip.innerHTML = '❌ ' + '{tipText}';
                        tip.style.cssText = `
                            position: fixed;
                            top: 50%;
                            left: 50%;
                            transform: translate(-50%, -50%);
                            background-color: rgba(220, 53, 69, 0.95);
                            color: white;
                            padding: 30px 60px;
                            border-radius: 15px;
                            font-size: 18px;
                            font-family: Arial, sans-serif;
                            z-index: 999999;
                            pointer-events: auto;
                            transition: opacity 0.3s ease;
                            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.5);
                            max-width: 80%;
                            text-align: center;
                            cursor: pointer;
                        `;
                        
                        // 点击即可关闭
                        tip.onclick = function() {{
                            tip.style.opacity = '0';
                            setTimeout(function() {{
                                tip.remove();
                            }}, 300);
                        }};
                        
                        document.body.appendChild(tip);
                        
                        // 5秒后自动淡出
                        setTimeout(function() {{
                            if (document.getElementById('screenshot-error-tip')) {{
                                tip.style.opacity = '0';
                                setTimeout(function() {{
                                    tip.remove();
                                }}, 300);
                            }}
                        }}, 5000);
                    }})();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示截图错误提示时出错", ex);
            }
        }
    }
}

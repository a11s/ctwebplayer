using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ctwebplayer
{
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
        
        // 修饰键常�?
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000; // Windows 7及以上版本，防止重复触发
        
        // 热键ID定义
        private const int HOTKEY_ID_F11 = 1; // F11全屏切换
        private const int HOTKEY_ID_F4 = 2;  // F4静音切换
        
        // 虚拟键码
        private const uint VK_F4 = 0x73;
        private const uint VK_F11 = 0x7A;
        #endregion
        
        // 默认加载的URL
        private const string DEFAULT_URL = "https://game.ero-labs.live/cn/cloud_game.html?id=27&connect_type=1&connection_id=20";
        
        // 缓存管理�?
        private CacheManager _cacheManager = null!; // �?InitializeWebView 中初始化
        
        // 配置管理�?
        private ConfigManager _configManager;
        
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
                
                // 设置状�?
                statusLabel.Text = "正在初始化浏览器...";
                
                // 配置管理器已在构造函数中初始�?
                LogManager.Instance.Info("配置管理器已初始�?);
                
                // 初始化缓存管理器
                _cacheManager = new CacheManager();
                LogManager.Instance.Info("缓存管理器已初始�?);
                
                // 构建浏览器参�?
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
                    // 禁用安全特�?
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
                
                // 初始化WebView2，使用自定义用户数据文件�?
                // CreateAsync 参数顺序�?browserExecutableFolder, userDataFolder, options)
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await webView2.EnsureCoreWebView2Async(environment);
                LogManager.Instance.Info($"WebView2初始化成功，用户数据文件夹：{userDataFolder}");
                
                // 配置WebView2设置
                ConfigureWebView2Settings();
                
                // 配置WebView2以拦截资源请�?
                ConfigureResourceInterception();
                
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
                
                // 导航到默认URL
                webView2.CoreWebView2.Navigate(DEFAULT_URL);
                txtAddress.Text = DEFAULT_URL;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("WebView2初始化失�?, ex);
                MessageBox.Show($"WebView2初始化失�? {ex.Message}\n\n请确保已安装WebView2运行时�?,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "浏览器初始化失败";
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
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注册全局热键时出�?, ex);
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
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注销全局热键时出�?, ex);
            }
        }

        /// <summary>
        /// 配置WebView2设置
        /// </summary>
        private void ConfigureWebView2Settings()
        {
            var settings = webView2.CoreWebView2.Settings;
            
            // 允许所有内�?
            settings.IsScriptEnabled = true;
            settings.IsWebMessageEnabled = true;
            settings.AreDefaultScriptDialogsEnabled = true;
            settings.IsStatusBarEnabled = true;
            settings.AreDevToolsEnabled = true;
            
            // 允许不安全的内容
            settings.IsPasswordAutosaveEnabled = true;
            settings.IsGeneralAutofillEnabled = true;
            
            LogManager.Instance.Info("WebView2设置已配�?);
        }

        /// <summary>
        /// 处理权限请求事件
        /// </summary>
        private void CoreWebView2_PermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            // 自动允许所有权限请�?
            e.State = CoreWebView2PermissionState.Allow;
            LogManager.Instance.Info($"已允许权限请求：{e.PermissionKind} for {e.Uri}");
        }

        /// <summary>
        /// 配置资源拦截
        /// </summary>
        private void ConfigureResourceInterception()
        {
            // 添加资源请求过滤器，拦截所有资源请�?
            webView2.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            
            // 订阅资源请求事件
            webView2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            
            LogManager.Instance.Info("资源拦截已配�?);
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

            // 获取延迟对象以异步处�?
            var deferral = e.GetDeferral();
            
            try
            {
                CacheResult? result = null;
                
                // 首先尝试从缓存读�?
                if (_cacheManager.IsCached(uri))
                {
                    result = await _cacheManager.GetFromCacheAsync(uri);
                    if (result.Success)
                    {
                        _cacheHits++;
                        UpdateCacheStatus();
                        LogManager.Instance.Debug($"缓存命中：{uri}");
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "HIT", result.Data!.LongLength, $"Cache file: {result.FilePath}");
                    }
                }
                
                // 如果缓存中没有，则下载并缓存
                if (result == null || !result.Success)
                {
                    // 记录开始下�?
                    _ = RequestLogger.Instance.WriteRequestLog(uri, "DOWNLOADING");
                    
                    var stopwatch = Stopwatch.StartNew();
                    result = await _cacheManager.DownloadAndCacheAsync(uri);
                    stopwatch.Stop();
                    
                    if (result.Success)
                    {
                        _cacheMisses++;
                        UpdateCacheStatus();
                        LogManager.Instance.Debug($"缓存未命中，已下载并缓存：{uri}");
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "MISS", result.Data!.LongLength, $"Download time: {stopwatch.ElapsedMilliseconds}ms, Cache file: {result.FilePath}");
                    }
                    else
                    {
                        _ = RequestLogger.Instance.WriteRequestLog(uri, "ERROR", null, "Download failed");
                    }
                }
                
                // 如果成功获取资源，创建响�?
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
        /// 更新缓存状态显�?
        /// </summary>
        private void UpdateCacheStatus()
        {
            // 在UI线程上更新状�?
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
            
            // 获取代理状�?
            var proxyStatus = _configManager.Config.Proxy?.Enabled == true ? " | 代理：已启用" : " | 代理：已禁用";
            
            // 获取静音状�?
            var muteStatus = _isMuted ? " | 静音：开�? : " | 静音：关�?;
            
            statusLabel.Text = $"缓存命中：{_cacheHits} | 未命中：{_cacheMisses} | 命中率：{hitRate}% | 缓存大小：{cacheSizeText}{proxyStatus}{muteStatus}";
        }

        /// <summary>
        /// 格式化文件大�?
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
        /// 导航开始事�?
        /// </summary>
        private void WebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // 显示进度�?
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = "正在加载...";
            
            LogManager.Instance.Info($"开始导航到：{e.Uri}");
            
            // 更新按钮状�?
            UpdateNavigationButtons();
        }

        /// <summary>
        /// 导航完成事件
        /// </summary>
        private async void WebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 隐藏进度�?
            progressBar.Visible = false;
            
            if (e.IsSuccess)
            {
                statusLabel.Text = "加载完成";
                LogManager.Instance.Info($"页面加载完成：{webView2.Source}");
                
                // 注入 CORS 处理脚本
                await InjectCorsHandlingScript();
                
                // 检查当前URL是否是目标URL
                var currentUrl = webView2.Source?.ToString() ?? "";
                if (currentUrl.StartsWith("https://game.ero-labs.live/cn/cloud_game.html"))
                {
                    await CheckAndNavigateToIframe();
                }
                
                // 检查并处理Unity canvas元素
                await CheckAndHandleUnityCanvas();
            }
            else
            {
                statusLabel.Text = $"加载失败: {e.WebErrorStatus}";
                LogManager.Instance.Warning($"页面加载失败：{webView2.Source}，错误：{e.WebErrorStatus}");
            }
            
            // 更新按钮状�?
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
                        // 覆盖 XMLHttpRequest 以添�?CORS 处理
                        var originalXHR = window.XMLHttpRequest;
                        window.XMLHttpRequest = function() {
                            var xhr = new originalXHR();
                            
                            // 保存原始�?open 方法
                            var originalOpen = xhr.open;
                            xhr.open = function(method, url, async, user, password) {
                                // 记录请求
                                console.log('XHR Request:', method, url);
                                
                                // 调用原始�?open 方法
                                return originalOpen.apply(this, arguments);
                            };
                            
                            // 覆盖 setRequestHeader 以确保正确的头部
                            var originalSetRequestHeader = xhr.setRequestHeader;
                            xhr.setRequestHeader = function(header, value) {
                                // 允许所有头�?
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
                        
                        // 移除所�?CSP 元标�?
                        var cspMetas = document.querySelectorAll('meta[http-equiv=""Content-Security-Policy""]');
                        cspMetas.forEach(function(meta) {
                            meta.remove();
                        });
                        
                        // 处理动态创建的 iframe
                        var originalCreateElement = document.createElement;
                        document.createElement = function(tagName) {
                            var element = originalCreateElement.apply(document, arguments);
                            
                            if (tagName.toLowerCase() === 'iframe') {
                                // 移除 sandbox 属�?
                                element.removeAttribute('sandbox');
                                // 添加允许跨域的属�?
                                element.setAttribute('allow', 'cross-origin-isolated *');
                            }
                            
                            return element;
                        };
                        
                        console.log('CORS handling script injected');
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
                LogManager.Instance.Info("已注�?CORS 处理脚本");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("注入 CORS 处理脚本时出�?, ex);
            }
        }

        /// <summary>
        /// 检查页面中的iframe并导航到其src地址
        /// </summary>
        private async Task CheckAndNavigateToIframe()
        {
            try
            {
                // 检查是否启用了自动导航功能
                if (!_configManager.Config.EnableAutoIframeNavigation)
                {
                    LogManager.Instance.Info("自动导航到iframe功能已禁�?);
                    return;
                }
                
                // 等待一小段时间确保页面完全加载
                await Task.Delay(500);
                
                // 先注入脚本来修改页面的安全策�?
                var policyScript = @"
                    (function() {
                        // 移除或修�?CSP 元标�?
                        var metaTags = document.querySelectorAll('meta[http-equiv=""Content-Security-Policy""]');
                        metaTags.forEach(function(tag) {
                            tag.remove();
                        });
                        
                        // 添加允许所有来源的 CSP
                        var newMeta = document.createElement('meta');
                        newMeta.httpEquiv = 'Content-Security-Policy';
                        newMeta.content = ""default-src * 'unsafe-inline' 'unsafe-eval' data: blob:; script-src * 'unsafe-inline' 'unsafe-eval'; connect-src * 'unsafe-inline'; img-src * data: blob: 'unsafe-inline'; frame-src *; style-src * 'unsafe-inline';"";
                        document.head.appendChild(newMeta);
                        
                        // 修改 iframe �?sandbox 属�?
                        var iframes = document.getElementsByTagName('iframe');
                        for (var i = 0; i < iframes.length; i++) {
                            iframes[i].removeAttribute('sandbox');
                            iframes[i].setAttribute('allow', 'cross-origin-isolated *');
                        }
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(policyScript);
                
                // 执行JavaScript来查找iframe
                var script = @"
                    (function() {
                        var iframes = document.getElementsByTagName('iframe');
                        if (iframes.length > 0) {
                            var iframe = iframes[0];
                            var src = iframe.src;
                            if (src && src.includes('patch-ct-labs.ecchi.xxx')) {
                                return src;
                            }
                        }
                        return null;
                    })();
                ";
                
                var result = await webView2.CoreWebView2.ExecuteScriptAsync(script);
                
                // 解析结果（结果是JSON格式的字符串�?
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
                LogManager.Instance.Error("检查iframe时出�?, ex);
            }
        }

        /// <summary>
        /// 检查页面中的Unity canvas元素并进行全屏处�?
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
                            return false;
                        }
                        
                        // 记录原始canvas信息
                        console.log('Unity canvas detected:', canvas);
                        
                        // 首先移除所有可能存在的旧样�?
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
                            
                            /* 隐藏所有不必要的元�?*/
                            #unity-loading-bar,
                            .footer,
                            .header,
                            .navbar,
                            .loading-bar,
                            .progress-bar:not(#unity-progress-bar-full),
                            body > *:not(.webgl-content):not(#gameContainer):not(#unity-canvas):not(script):not(style) {
                                display: none !important;
                            }
                            
                            /* 重置所有容�?*/
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
                            
                            // 设置canvas的实际像素尺�?
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
                        
                        // 移除所有父元素的内联样�?
                        var parent = canvas.parentElement;
                        while (parent && parent !== document.body) {
                            parent.style.cssText = '';
                            parent = parent.parentElement;
                        }
                        
                        // 确保canvas直接在body下或在简单的容器�?
                        if (canvas.parentElement && canvas.parentElement !== document.body) {
                            // 清除父容器的所有内联样�?
                            canvas.parentElement.style.cssText = '';
                        }
                        
                        // 立即更新尺寸
                        updateCanvasSize();
                        
                        // 监听窗口大小变化
                        window.removeEventListener('resize', updateCanvasSize); // 移除可能存在的旧监听�?
                        window.addEventListener('resize', updateCanvasSize);
                        
                        // 再次强制更新，确保生�?
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
                    statusLabel.Text = "Unity canvas已全屏显�?;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("处理Unity canvas时出�?, ex);
            }
        }

        /// <summary>
        /// URL改变事件
        /// </summary>
        private void WebView2_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            // 更新地址�?
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
                this.Text = $"{title} - Unity3D WebPlayer 专属浏览�?;
            }
            else
            {
                this.Text = "Unity3D WebPlayer 专属浏览�?;
            }
        }

        /// <summary>
        /// 下载开始事�?
        /// </summary>
        private void CoreWebView2_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            // 可以在这里处理下载逻辑
            statusLabel.Text = $"正在下载: {e.DownloadOperation.Uri}";
            LogManager.Instance.Info($"开始下载资源：{e.DownloadOperation.Uri}");
        }

        /// <summary>
        /// 更新导航按钮状�?
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
        /// 地址栏按键事�?
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
        private void NavigateToUrl()
        {
            string url = txtAddress.Text.Trim();
            
            if (string.IsNullOrEmpty(url))
            {
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
                MessageBox.Show($"无法导航到URL: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void btnSettings_Click(object? sender, EventArgs e)
        {
            // 打开综合设置窗口
            using (var settingsForm = new SettingsForm(_configManager))
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 更新状态栏显示
                    UpdateCacheStatus();
                    LogManager.Instance.Info("设置已更�?);
                }
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
            
            var message = $"缓存统计信息：\n\n" +
                         $"缓存目录�?/cache\n" +
                         $"缓存大小：{cacheSizeText}\n" +
                         $"缓存命中次数：{_cacheHits}\n" +
                         $"缓存未命中次数：{_cacheMisses}\n" +
                         $"缓存命中率：{hitRate}%\n\n" +
                         $"缓存策略：\n" +
                         $"- JavaScript和CSS文件（带版本号）：长期缓存\n" +
                         $"- 图片和字体文件：长期缓存\n" +
                         $"- API请求：不缓存";
            
            MessageBox.Show(message, "缓存信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private async Task ClearCache()
        {
            var result = MessageBox.Show("确定要清理所有缓存吗？\n\n这将删除所有已缓存的静态资源�?,
                "确认清理", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _cacheManager.ClearCacheAsync();
                    _cacheHits = 0;
                    _cacheMisses = 0;
                    UpdateCacheStatus();
                    LogManager.Instance.Info("缓存已清�?);
                    MessageBox.Show("缓存已清理完成�?, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理缓存时出�?, ex);
                    MessageBox.Show($"清理缓存时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 显示代理设置对话�?
        /// </summary>
        private void ShowProxySettings()
        {
            using (var proxyForm = new ProxySettingsForm(_configManager))
            {
                if (proxyForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 更新状态栏显示
                    UpdateCacheStatus();
                    LogManager.Instance.Info("代理设置已更�?);
                }
            }
        }

        /// <summary>
        /// 显示日志查看�?
        /// </summary>
        private void ShowLogViewer()
        {
            using (var logViewerForm = new LogViewerForm())
            {
                logViewerForm.ShowDialog(this);
            }
        }


        /// <summary>
        /// 窗体关闭时清理资�?
        /// </summary>
        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            
            // 注销全局热键
            UnregisterGlobalHotkeys();
            
            _cacheManager?.Dispose();
            
            // 记录应用程序关闭
            LogManager.Instance.Info("应用程序关闭");
            
            // 确保所有日志都已写�?
            await LogManager.Instance.FlushAsync();
        }

        /// <summary>
        /// 全屏切换菜单项点击事�?
        /// </summary>
        private void toggleFullScreenMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        /// <summary>
        /// 静音切换菜单项点击事�?
        /// </summary>
        private void toggleMuteMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleMute();
        }

        /// <summary>
        /// 检查更新菜单项点击事件
        /// </summary>
        private async void checkUpdateMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                statusLabel.Text = "正在检查更�?..";
                
                var updateManager = new UpdateManager();
                var updateInfo = await updateManager.CheckForUpdatesAsync();
                
                if (updateInfo != null && updateInfo.IsUpdateRequired())
                {
                    // 显示更新窗口
                    using (var updateForm = new UpdateForm(updateInfo))
                    {
                        updateForm.ShowDialog(this);
                    }
                }
                
                statusLabel.Text = "就绪";
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("检查更新时出错", ex);
                MessageBox.Show($"检查更新时出错：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "检查更新失�?;
            }
        }

        /// <summary>
        /// 关于菜单项点击事�?
        /// </summary>
        private void aboutMenuItem_Click(object? sender, EventArgs e)
        {
            using (var aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog(this);
            }
        }

        // 以下方法已被全局热键替代，不再使�?
        /*
        /// <summary>
        /// 窗体键盘事件处理（F11和F4快捷键）
        /// </summary>
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // 添加调试日志
            LogManager.Instance.Info($"Form1_KeyDown触发：键�?= {e.KeyCode}, Alt = {e.Alt}, Ctrl = {e.Control}, Shift = {e.Shift}");
            
            if (e.KeyCode == Keys.F11)
            {
                LogManager.Instance.Info("检测到F11按键，准备切换全�?);
                ToggleFullScreen();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                LogManager.Instance.Info("检测到F4按键，准备切换静�?);
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
            LogManager.Instance.Info($"WebView2_PreviewKeyDown触发：键�?= {e.KeyCode}");
            
            // 处理F11和F4�?
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
                // 退出全屏模�?
                ExitFullScreen();
            }
        }

        /// <summary>
        /// 进入全屏模式
        /// </summary>
        private void EnterFullScreen()
        {
            // 保存当前状�?
            _previousWindowState = this.WindowState;
            _previousBorderStyle = this.FormBorderStyle;
            _previousBounds = this.Bounds;
            _previousToolStripVisible = toolStrip1.Visible;
            _previousStatusStripVisible = statusStrip1.Visible;

            // 隐藏工具栏和状态栏
            toolStrip1.Visible = false;
            statusStrip1.Visible = false;

            // 设置无边�?
            this.FormBorderStyle = FormBorderStyle.None;

            // 设置窗口状态为最大化
            this.WindowState = FormWindowState.Maximized;

            // 设置全屏标志
            _isFullScreen = true;

            // 显示全屏提示
            ShowFullScreenTip();
            
            LogManager.Instance.Info("已进入全屏模�?);
        }

        /// <summary>
        /// 退出全屏模�?
        /// </summary>
        private void ExitFullScreen()
        {
            // 恢复工具栏和状态栏的可见�?
            toolStrip1.Visible = _previousToolStripVisible;
            statusStrip1.Visible = _previousStatusStripVisible;

            // 恢复边框样式
            this.FormBorderStyle = _previousBorderStyle;

            // 恢复窗口状�?
            this.WindowState = _previousWindowState;

            // 如果之前不是最大化状态，恢复窗口位置和大�?
            if (_previousWindowState != FormWindowState.Maximized)
            {
                this.Bounds = _previousBounds;
            }

            // 清除全屏标志
            _isFullScreen = false;

            LogManager.Instance.Info("已退出全屏模�?);
        }

        /// <summary>
        /// 显示全屏提示
        /// </summary>
        private async void ShowFullScreenTip()
        {
            try
            {
                // 在网页中显示提示
                var script = @"
                    (function() {
                        // 检查是否已经存在提�?
                        var existingTip = document.getElementById('fullscreen-tip');
                        if (existingTip) {
                            existingTip.remove();
                        }
                        
                        // 创建提示元素
                        var tip = document.createElement('div');
                        tip.id = 'fullscreen-tip';
                        tip.innerHTML = '�?F11 退出全�?;
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
                        setTimeout(function() {
                            tip.style.opacity = '0';
                            setTimeout(function() {
                                tip.remove();
                            }, 500);
                        }, 3000);
                    })();
                ";
                
                await webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("显示全屏提示时出�?, ex);
            }
        }

        /// <summary>
        /// 切换静音状�?
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
                                
                                // 监听播放事件，确保静音状态保�?
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
                        
                        // 获取所有的音频和视频元�?
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
                        
                        // 开始观察整个文�?
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
                
                // 如果是游戏页面，尝试额外的静音方�?
                var currentUrl = webView2.Source?.ToString() ?? "";
                if (currentUrl.Contains("unity") || currentUrl.Contains("game"))
                {
                    await ApplyGameSpecificMute();
                }
                
                // 更新状态栏显示
                UpdateCacheStatus();
                
                // 显示静音状态提�?
                ShowMuteTip(_isMuted);
                
                LogManager.Instance.Info($"静音状态已切换为：{(_isMuted ? "开�? : "关闭")}");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("切换静音状态时出错", ex);
                MessageBox.Show($"切换静音状态时出错：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 应用游戏特定的静音方�?
        /// </summary>
        private async Task ApplyGameSpecificMute()
        {
            try
            {
                var script = $@"
                    (function() {{
                        var muteState = {_isMuted.ToString().ToLower()};
                        
                        // Unity特定的静音方�?
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
                LogManager.Instance.Error("应用游戏特定静音时出�?, ex);
            }
        }

        /// <summary>
        /// 显示静音状态提�?
        /// </summary>
        private async void ShowMuteTip(bool isMuted)
        {
            try
            {
                var message = isMuted ? "静音已开�? : "静音已关�?;
                var icon = isMuted ? "🔇" : "🔊";
                
                var script = $@"
                    (function() {{
                        // 检查是否已经存在提�?
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
                LogManager.Instance.Error("显示静音提示时出�?, ex);
            }
        }

        /// <summary>
        /// 重写窗口过程以处理热键消�?
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
                        LogManager.Instance.Info("检测到F11全局热键，准备切换全�?);
                        ToggleFullScreen();
                        break;
                        
                    case HOTKEY_ID_F4:
                        LogManager.Instance.Info("检测到F4全局热键，准备切换静�?);
                        ToggleMute();
                        break;
                }
            }
            
            // 调用基类的WndProc处理其他消息
            base.WndProc(ref m);
        }
    }
}


using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;

namespace ctwebplayer
{
    public partial class Form1 : Form
    {
        // 默认加载的URL
        private const string DEFAULT_URL = "https://game.ero-labs.live/cn/cloud_game.html?id=27&connect_type=1&connection_id=20";
        
        // 缓存管理器
        private CacheManager _cacheManager;
        
        // 配置管理器
        private ConfigManager _configManager;
        
        // 缓存统计
        private int _cacheHits = 0;
        private int _cacheMisses = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
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
                statusLabel.Text = "正在初始化浏览器...";
                
                // 初始化配置管理器
                _configManager = new ConfigManager();
                LogManager.Instance.Info("配置管理器已初始化");
                
                // 初始化缓存管理器
                _cacheManager = new CacheManager();
                LogManager.Instance.Info("缓存管理器已初始化");
                
                // 初始化自动导航复选框状态
                chkAutoIframeNav.Checked = _configManager.Config.EnableAutoIframeNavigation;
                
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
                
                // 创建WebView2环境选项
                var options = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = string.Join(" ", browserArgs)
                };
                
                LogManager.Instance.Info("WebView2初始化参数：" + options.AdditionalBrowserArguments);
                
                // 初始化WebView2
                var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                await webView2.EnsureCoreWebView2Async(environment);
                LogManager.Instance.Info("WebView2初始化成功");
                
                // 配置WebView2设置
                ConfigureWebView2Settings();
                
                // 配置WebView2以拦截资源请求
                ConfigureResourceInterception();
                
                // 订阅事件
                webView2.NavigationStarting += WebView2_NavigationStarting;
                webView2.NavigationCompleted += WebView2_NavigationCompleted;
                webView2.SourceChanged += WebView2_SourceChanged;
                webView2.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                webView2.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
                webView2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
                
                // 导航到默认URL
                webView2.CoreWebView2.Navigate(DEFAULT_URL);
                txtAddress.Text = DEFAULT_URL;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("WebView2初始化失败", ex);
                MessageBox.Show($"WebView2初始化失败: {ex.Message}\n\n请确保已安装WebView2运行时。",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "浏览器初始化失败";
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
                return; // 不缓存，让WebView2正常处理
            }

            // 获取延迟对象以异步处理
            var deferral = e.GetDeferral();
            
            try
            {
                CacheResult result = null;
                
                // 首先尝试从缓存读取
                if (_cacheManager.IsCached(uri))
                {
                    result = await _cacheManager.GetFromCacheAsync(uri);
                    if (result.Success)
                    {
                        _cacheHits++;
                        UpdateCacheStatus();
                        LogManager.Instance.Debug($"缓存命中：{uri}");
                    }
                }
                
                // 如果缓存中没有，则下载并缓存
                if (result == null || !result.Success)
                {
                    result = await _cacheManager.DownloadAndCacheAsync(uri);
                    if (result.Success)
                    {
                        _cacheMisses++;
                        UpdateCacheStatus();
                        LogManager.Instance.Debug($"缓存未命中，已下载并缓存：{uri}");
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
            var proxyStatus = _configManager.Config.Proxy?.Enabled == true ? " | 代理：已启用" : " | 代理：已禁用";
            
            statusLabel.Text = $"缓存命中：{_cacheHits} | 未命中：{_cacheMisses} | 命中率：{hitRate}% | 缓存大小：{cacheSizeText}{proxyStatus}";
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
        private void WebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // 显示进度条
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = "正在加载...";
            
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
                            return false;
                        }
                        
                        // 记录原始canvas信息
                        console.log('Unity canvas detected:', canvas);
                        
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
                    statusLabel.Text = "Unity canvas已全屏显示";
                }
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
                this.Text = $"{title} - Unity3D WebPlayer 专属浏览器";
            }
            else
            {
                this.Text = "Unity3D WebPlayer 专属浏览器";
            }
        }

        /// <summary>
        /// 下载开始事件
        /// </summary>
        private void CoreWebView2_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            // 可以在这里处理下载逻辑
            statusLabel.Text = $"正在下载: {e.DownloadOperation.Uri}";
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
            // 这里可以打开设置对话框
            // 目前只显示一个提示
            // 创建设置菜单
            var contextMenu = new ContextMenuStrip();
            
            // 缓存管理选项
            var cacheMenuItem = new ToolStripMenuItem("缓存管理");
            cacheMenuItem.DropDownItems.Add("查看缓存信息", null, (s, args) => ShowCacheInfo());
            cacheMenuItem.DropDownItems.Add("清理缓存", null, async (s, args) => await ClearCache());
            contextMenu.Items.Add(cacheMenuItem);
            
            // 其他设置选项
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("代理设置", null, (s, args) => ShowProxySettings());
            contextMenu.Items.Add("查看日志", null, (s, args) => ShowLogViewer());
            
            // 显示菜单 - 获取按钮在屏幕上的位置
            var buttonBounds = btnSettings.Bounds;
            var screenPoint = toolStrip1.PointToScreen(new Point(buttonBounds.Left, buttonBounds.Bottom));
            contextMenu.Show(screenPoint);
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
                         $"缓存目录：./cache\n" +
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
            var result = MessageBox.Show("确定要清理所有缓存吗？\n\n这将删除所有已缓存的静态资源。",
                "确认清理", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _cacheManager.ClearCacheAsync();
                    _cacheHits = 0;
                    _cacheMisses = 0;
                    UpdateCacheStatus();
                    LogManager.Instance.Info("缓存已清理");
                    MessageBox.Show("缓存已清理完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("清理缓存时出错", ex);
                    MessageBox.Show($"清理缓存时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 显示代理设置对话框
        /// </summary>
        private void ShowProxySettings()
        {
            using (var proxyForm = new ProxySettingsForm(_configManager))
            {
                if (proxyForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 更新状态栏显示
                    UpdateCacheStatus();
                    LogManager.Instance.Info("代理设置已更新");
                }
            }
        }

        /// <summary>
        /// 显示日志查看器
        /// </summary>
        private void ShowLogViewer()
        {
            using (var logViewerForm = new LogViewerForm())
            {
                logViewerForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 自动导航到iframe内容复选框状态改变事件
        /// </summary>
        private async void chkAutoIframeNav_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                // 更新配置
                _configManager.Config.EnableAutoIframeNavigation = chkAutoIframeNav.Checked;
                
                // 保存配置
                await _configManager.SaveConfigAsync();
                
                LogManager.Instance.Info($"自动导航到iframe功能已{(chkAutoIframeNav.Checked ? "启用" : "禁用")}");
                
                // 如果当前页面是目标页面且刚刚启用了功能，立即尝试导航
                if (chkAutoIframeNav.Checked)
                {
                    var currentUrl = webView2.Source?.ToString() ?? "";
                    if (currentUrl.StartsWith("https://game.ero-labs.live/cn/cloud_game.html"))
                    {
                        await CheckAndNavigateToIframe();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("更新自动导航设置时出错", ex);
                MessageBox.Show($"保存设置时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _cacheManager?.Dispose();
            
            // 记录应用程序关闭
            LogManager.Instance.Info("应用程序关闭");
            
            // 确保所有日志都已写入
            await LogManager.Instance.FlushAsync();
        }
    }
}

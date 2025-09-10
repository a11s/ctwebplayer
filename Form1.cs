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
                
                // 应用代理设置
                var proxyString = _configManager.GetProxyString();
                if (!string.IsNullOrEmpty(proxyString))
                {
                    // 创建WebView2环境选项
                    var options = new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = $"--proxy-server={proxyString}"
                    };
                    LogManager.Instance.Info($"已应用代理设置：{proxyString}");
                    
                    // 初始化WebView2
                    var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                    await webView2.EnsureCoreWebView2Async(environment);
                }
                else
                {
                    // 初始化WebView2（无代理）
                    await webView2.EnsureCoreWebView2Async(null);
                    LogManager.Instance.Info("WebView2初始化成功（无代理）");
                }
                
                // 配置WebView2以拦截资源请求
                ConfigureResourceInterception();
                
                // 订阅事件
                webView2.NavigationStarting += WebView2_NavigationStarting;
                webView2.NavigationCompleted += WebView2_NavigationCompleted;
                webView2.SourceChanged += WebView2_SourceChanged;
                webView2.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                webView2.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
                
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
                    var response = webView2.CoreWebView2.Environment.CreateWebResourceResponse(
                        stream,
                        200,
                        "OK",
                        $"Content-Type: {result.MimeType}"
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
        private void WebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 隐藏进度条
            progressBar.Visible = false;
            
            if (e.IsSuccess)
            {
                statusLabel.Text = "加载完成";
                LogManager.Instance.Info($"页面加载完成：{webView2.Source}");
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

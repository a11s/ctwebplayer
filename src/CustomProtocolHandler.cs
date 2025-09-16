using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace ctwebplayer
{
    /// <summary>
    /// 自定义协议处理器，用于处理ct://协议
    /// </summary>
    public class CustomProtocolHandler
    {
        private readonly ConfigManager _configManager;
        private readonly Form1 _mainForm;
        
        // 保存对话框实例，避免重复创建
        private AboutForm _aboutForm = null;
        private CookieManagerForm _cookieManagerForm = null;
        private ProxySettingsForm _proxyForm = null;
        private SettingsForm _settingsForm = null;
        private LogViewerForm _logViewerForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainForm">主窗体引用</param>
        /// <param name="configManager">配置管理器</param>
        public CustomProtocolHandler(Form1 mainForm, ConfigManager configManager)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        }

        /// <summary>
        /// 处理自定义协议URL
        /// </summary>
        /// <param name="url">要处理的URL</param>
        /// <returns>是否已处理该URL</returns>
        public async Task<bool> HandleProtocolAsync(string url)
        {
            try
            {
                LogManager.Instance.Info($"处理自定义协议URL：{url}");

                // 解析URL
                var route = ProtocolRoute.Parse(url);
                if (route == null)
                {
                    LogManager.Instance.Warning($"无法解析协议URL：{url}");
                    return false;
                }

                // 检查是否是ct://协议
                if (!route.Protocol.Equals("ct", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                LogManager.Instance.Info($"解析协议路由：{route}");

                // 根据不同的路由执行不同的操作
                bool handled = await RouteToHandlerAsync(route);
                
                if (handled)
                {
                    LogManager.Instance.Info($"协议URL已处理：{url}");
                }
                else
                {
                    LogManager.Instance.Warning($"未找到协议URL的处理器：{url}");
                }

                return handled;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"处理协议URL时出错：{url}", ex);
                return false;
            }
        }

        /// <summary>
        /// 根据路由分发到相应的处理器
        /// </summary>
        /// <param name="route">协议路由</param>
        /// <returns>是否已处理</returns>
        private async Task<bool> RouteToHandlerAsync(ProtocolRoute route)
        {
            // 根据主机名进行路由
            switch (route.Host.ToLowerInvariant())
            {
                case "settings":
                    return await HandleSettingsRouteAsync(route);
                
                case "about":
                    return await HandleAboutRouteAsync(route);
                
                case "cache":
                    return await HandleCacheRouteAsync(route);
                
                case "help":
                    return await HandleHelpRouteAsync(route);
                
                case "debug":
                    return await HandleDebugRouteAsync(route);
                
                default:
                    return await HandleUnknownRouteAsync(route);
            }
        }

        /// <summary>
        /// 处理设置相关的路由
        /// </summary>
        private async Task<bool> HandleSettingsRouteAsync(ProtocolRoute route)
        {
            var path = route.Path.ToLowerInvariant();
            
            switch (path)
            {
                case "/cookie":
                case "/cookies":
                    await ShowCookieManagementAsync();
                    return true;
                
                case "/proxy":
                    ShowProxySettings();
                    return true;
                
                case "/language":
                    ShowLanguageSettings();
                    return true;
                
                case "/":
                case "":
                    ShowGeneralSettings();
                    return true;
                
                default:
                    LogManager.Instance.Warning($"未知的设置路径：{path}");
                    return false;
            }
        }

        /// <summary>
        /// 处理关于页面路由
        /// </summary>
        private async Task<bool> HandleAboutRouteAsync(ProtocolRoute route)
        {
            await Task.Run(() =>
            {
                _mainForm.Invoke(new Action(() =>
                {
                    if (_aboutForm == null || _aboutForm.IsDisposed)
                    {
                        _aboutForm = new AboutForm();
                        _aboutForm.FormClosed += (s, args) =>
                        {
                            LogManager.Instance.Info("关于窗口已关闭（从协议处理器打开）");
                        };
                    }
                    
                    if (!_aboutForm.Visible)
                    {
                        _aboutForm.Show(_mainForm);
                    }
                    else
                    {
                        _aboutForm.BringToFront();
                        _aboutForm.Focus();
                    }
                }));
            });
            
            return true;
        }

        /// <summary>
        /// 处理缓存相关路由
        /// </summary>
        private async Task<bool> HandleCacheRouteAsync(ProtocolRoute route)
        {
            var path = route.Path.ToLowerInvariant();
            
            switch (path)
            {
                case "/clear":
                    await ClearCacheAsync();
                    return true;
                
                case "/info":
                case "/":
                case "":
                    ShowCacheInfo();
                    return true;
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理帮助相关路由
        /// </summary>
        private async Task<bool> HandleHelpRouteAsync(ProtocolRoute route)
        {
            // 打开帮助文档或GitHub页面
            await Task.Run(() =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/a11s/ctwebplayer",
                    UseShellExecute = true
                });
            });
            
            return true;
        }

        /// <summary>
        /// 处理调试相关路由
        /// </summary>
        private async Task<bool> HandleDebugRouteAsync(ProtocolRoute route)
        {
            var path = route.Path.ToLowerInvariant();
            
            switch (path)
            {
                case "/log":
                case "/logs":
                    ShowLogViewer();
                    return true;
                
                case "/devtools":
                    await OpenDevToolsAsync();
                    return true;
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理未知路由
        /// </summary>
        private async Task<bool> HandleUnknownRouteAsync(ProtocolRoute route)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(
                    $"未知的协议路由：ct://{route.GetFullPath()}\n\n" +
                    "支持的路由：\n" +
                    "• ct://settings/cookie - Cookie管理\n" +
                    "• ct://settings/proxy - 代理设置\n" +
                    "• ct://settings/language - 语言设置\n" +
                    "• ct://cache/clear - 清理缓存\n" +
                    "• ct://cache/info - 缓存信息\n" +
                    "• ct://about - 关于\n" +
                    "• ct://help - 帮助\n" +
                    "• ct://debug/log - 查看日志\n" +
                    "• ct://debug/devtools - 开发者工具",
                    "未知路由",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
            
            return true;
        }

        /// <summary>
        /// 显示Cookie管理界面
        /// </summary>
        private async Task ShowCookieManagementAsync()
        {
            await Task.Run(() =>
            {
                _mainForm.Invoke(new Action(() =>
                {
                    LogManager.Instance.Info("ShowCookieManagementAsync开始执行");
                    
                    // 获取WebView2控件
                    CoreWebView2 coreWebView2 = null;
                    
                    try
                    {
                        // 通过反射获取webView2字段
                        var webView2Field = _mainForm.GetType().GetField("webView2",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        LogManager.Instance.Info($"webView2Field获取结果: {webView2Field != null}");
                        
                        if (webView2Field != null)
                        {
                            var webView2 = webView2Field.GetValue(_mainForm) as Microsoft.Web.WebView2.WinForms.WebView2;
                            LogManager.Instance.Info($"webView2实例获取结果: {webView2 != null}");
                            
                            if (webView2 != null)
                            {
                                LogManager.Instance.Info($"webView2.CoreWebView2状态: {webView2.CoreWebView2 != null}");
                                
                                if (webView2.CoreWebView2 != null)
                                {
                                    coreWebView2 = webView2.CoreWebView2;
                                    LogManager.Instance.Info($"CoreWebView2.CookieManager状态: {coreWebView2.CookieManager != null}");
                                    
                                    if (coreWebView2.CookieManager != null)
                                    {
                                        LogManager.Instance.Info("成功获取WebView2实例和CookieManager");
                                    }
                                    else
                                    {
                                        LogManager.Instance.Error("CoreWebView2.CookieManager为null");
                                    }
                                }
                                else
                                {
                                    LogManager.Instance.Warning("WebView2.CoreWebView2为null，WebView2可能未完全初始化");
                                }
                            }
                            else
                            {
                                LogManager.Instance.Warning("webView2实例为null");
                            }
                        }
                        else
                        {
                            LogManager.Instance.Warning("无法通过反射获取webView2字段");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"获取WebView2实例失败: {ex.GetType().Name}: {ex.Message}", ex);
                        LogManager.Instance.Error($"堆栈跟踪: {ex.StackTrace}");
                    }
                    
                    LogManager.Instance.Info($"准备打开CookieManagerForm，CoreWebView2实例: {coreWebView2 != null}");
                    
                    // 打开Cookie管理窗体（非模式）
                    if (_cookieManagerForm == null || _cookieManagerForm.IsDisposed)
                    {
                        _cookieManagerForm = new CookieManagerForm(coreWebView2);
                        _cookieManagerForm.FormClosed += (s, args) =>
                        {
                            LogManager.Instance.Info("Cookie管理界面已关闭（从协议处理器打开）");
                        };
                    }
                    
                    if (!_cookieManagerForm.Visible)
                    {
                        _cookieManagerForm.Show(_mainForm);
                    }
                    else
                    {
                        _cookieManagerForm.BringToFront();
                        _cookieManagerForm.Focus();
                    }
                }));
            });
        }

        /// <summary>
        /// 显示代理设置
        /// </summary>
        private void ShowProxySettings()
        {
            _mainForm.Invoke(new Action(() =>
            {
                if (_proxyForm == null || _proxyForm.IsDisposed)
                {
                    _proxyForm = new ProxySettingsForm(_configManager);
                    _proxyForm.FormClosed += (s, args) =>
                    {
                        LogManager.Instance.Info("代理设置窗口已关闭（从协议处理器打开）");
                    };
                }
                
                if (!_proxyForm.Visible)
                {
                    _proxyForm.Show(_mainForm);
                }
                else
                {
                    _proxyForm.BringToFront();
                    _proxyForm.Focus();
                }
            }));
        }

        /// <summary>
        /// 显示语言设置
        /// </summary>
        private void ShowLanguageSettings()
        {
            _mainForm.Invoke(new Action(() =>
            {
                if (_settingsForm == null || _settingsForm.IsDisposed)
                {
                    _settingsForm = new SettingsForm(_configManager);
                    _settingsForm.FormClosed += (s, args) =>
                    {
                        LogManager.Instance.Info("设置窗口已关闭（从协议处理器打开-语言）");
                    };
                }
                
                if (!_settingsForm.Visible)
                {
                    _settingsForm.Show(_mainForm);
                }
                else
                {
                    _settingsForm.BringToFront();
                    _settingsForm.Focus();
                }
            }));
        }

        /// <summary>
        /// 显示通用设置
        /// </summary>
        private void ShowGeneralSettings()
        {
            _mainForm.Invoke(new Action(() =>
            {
                if (_settingsForm == null || _settingsForm.IsDisposed)
                {
                    _settingsForm = new SettingsForm(_configManager);
                    _settingsForm.FormClosed += (s, args) =>
                    {
                        LogManager.Instance.Info("设置窗口已关闭（从协议处理器打开-通用）");
                    };
                }
                
                if (!_settingsForm.Visible)
                {
                    _settingsForm.Show(_mainForm);
                }
                else
                {
                    _settingsForm.BringToFront();
                    _settingsForm.Focus();
                }
            }));
        }

        /// <summary>
        /// 显示缓存信息
        /// </summary>
        private void ShowCacheInfo()
        {
            _mainForm.Invoke(new Action(() =>
            {
                // 调用主窗体的ShowCacheInfo方法
                var showCacheInfoMethod = _mainForm.GetType().GetMethod("ShowCacheInfo", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (showCacheInfoMethod != null)
                {
                    showCacheInfoMethod.Invoke(_mainForm, null);
                }
            }));
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private async Task ClearCacheAsync()
        {
            await Task.Run(() =>
            {
                _mainForm.Invoke(new Action(async () =>
                {
                    // 调用主窗体的ClearCache方法
                    var clearCacheMethod = _mainForm.GetType().GetMethod("ClearCache",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (clearCacheMethod != null)
                    {
                        var task = clearCacheMethod.Invoke(_mainForm, null) as Task;
                        if (task != null)
                        {
                            await task;
                        }
                    }
                }));
            });
        }

        /// <summary>
        /// 显示日志查看器
        /// </summary>
        private void ShowLogViewer()
        {
            _mainForm.Invoke(new Action(() =>
            {
                if (_logViewerForm == null || _logViewerForm.IsDisposed)
                {
                    _logViewerForm = new LogViewerForm();
                    _logViewerForm.FormClosed += (s, args) =>
                    {
                        LogManager.Instance.Info("日志查看器窗口已关闭（从协议处理器打开）");
                    };
                }
                
                if (!_logViewerForm.Visible)
                {
                    _logViewerForm.Show(_mainForm);
                }
                else
                {
                    _logViewerForm.BringToFront();
                    _logViewerForm.Focus();
                }
            }));
        }

        /// <summary>
        /// 打开开发者工具
        /// </summary>
        private async Task OpenDevToolsAsync()
        {
            await Task.Run(() =>
            {
                _mainForm.Invoke(new Action(() =>
                {
                    // 获取WebView2控件
                    var webView2Field = _mainForm.GetType().GetField("webView2",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (webView2Field != null)
                    {
                        var webView2 = webView2Field.GetValue(_mainForm) as Microsoft.Web.WebView2.WinForms.WebView2;
                        webView2?.CoreWebView2?.OpenDevToolsWindow();
                    }
                }));
            });
        }

        /// <summary>
        /// 检查URL是否应该被拦截
        /// </summary>
        /// <param name="url">要检查的URL</param>
        /// <returns>是否应该拦截</returns>
        public static bool ShouldIntercept(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            
            return url.StartsWith("ct://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
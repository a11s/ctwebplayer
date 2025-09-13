using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ctwebplayer
{
    /// <summary>
    /// 配置管理器，负责读取和保存应用程序配置
    /// </summary>
    public class ConfigManager
    {
        private static readonly string ConfigFilePath = "./config.json";
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private AppConfig _config = null!; // 在 LoadConfig 中初始化

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public AppConfig Config => _config;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConfigManager()
        {
            LoadConfig();
            InitializeLogManager();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? GetDefaultConfig();
                    LogManager.Instance.Info("配置文件加载成功");
                }
                else
                {
                    _config = GetDefaultConfig();
                    SaveConfig(); // 创建默认配置文件
                    LogManager.Instance.Info("创建默认配置文件");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"加载配置文件失败：{ex.Message}", ex);
                _config = GetDefaultConfig();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public async Task SaveConfigAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(_config, _jsonOptions);
                await File.WriteAllTextAsync(ConfigFilePath, json);
                LogManager.Instance.Info("配置文件保存成功");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"保存配置文件失败：{ex.Message}", ex);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 同步保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            _semaphore.Wait();
            try
            {
                var json = JsonSerializer.Serialize(_config, _jsonOptions);
                File.WriteAllText(ConfigFilePath, json);
                LogManager.Instance.Info("配置文件保存成功");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"保存配置文件失败：{ex.Message}", ex);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        private AppConfig GetDefaultConfig()
        {
            return new AppConfig
            {
                Proxy = new ProxyConfig
                {
                    Enabled = false,
                    HttpProxy = "",
                    HttpsProxy = "",
                    Socks5 = ""
                },
                Logging = new LoggingConfig
                {
                    Enabled = true,
                    LogLevel = "Info",
                    MaxFileSize = 10485760 // 10MB
                },
                EnableAutoIframeNavigation = true, // 默认启用
                Ui = new UIConfig
                {
                    WindowWidth = 1236,
                    WindowHeight = 740
                },
                DebugMode = false,
                BaseURL = "https://game.ero-labs.live"
            };
        }

        /// <summary>
        /// 更新代理配置
        /// </summary>
        public async Task UpdateProxyConfigAsync(ProxyConfig proxyConfig)
        {
            _config.Proxy = proxyConfig;
            await SaveConfigAsync();
        }
        
        /// <summary>
        /// 更新UI配置
        /// </summary>
        public async Task UpdateUIConfigAsync(UIConfig uiConfig)
        {
            _config.Ui = uiConfig;
            await SaveConfigAsync();
        }

        /// <summary>
        /// 更新BaseURL配置
        /// </summary>
        public async Task UpdateBaseUrlAsync(string baseUrl)
        {
            _config.BaseURL = baseUrl;
            await SaveConfigAsync();
        }

        /// <summary>
        /// 获取代理配置字符串（用于WebView2）
        /// </summary>
        public string GetProxyString()
        {
            if (_config.Proxy == null || !_config.Proxy.Enabled)
            {
                return string.Empty;
            }

            // WebView2使用的代理格式
            // 优先使用SOCKS5，然后是HTTP/HTTPS
            if (!string.IsNullOrWhiteSpace(_config.Proxy.Socks5))
            {
                return $"socks5://{_config.Proxy.Socks5}";
            }
            else if (!string.IsNullOrWhiteSpace(_config.Proxy.HttpProxy))
            {
                // 如果HTTP代理地址已经包含协议，直接使用
                if (_config.Proxy.HttpProxy.StartsWith("http://") || 
                    _config.Proxy.HttpProxy.StartsWith("https://"))
                {
                    return _config.Proxy.HttpProxy;
                }
                else
                {
                    return $"http://{_config.Proxy.HttpProxy}";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 初始化日志管理器
        /// </summary>
        private void InitializeLogManager()
        {
            if (_config?.Logging != null)
            {
                // 解析日志级别
                if (Enum.TryParse<LogLevel>(_config.Logging.LogLevel, true, out var logLevel))
                {
                    LogManager.Instance.Configure(
                        _config.Logging.Enabled,
                        logLevel,
                        _config.Logging.MaxFileSize
                    );
                }
                else
                {
                    // 如果解析失败，使用默认值
                    LogManager.Instance.Configure(true, LogLevel.Info, 10485760);
                }
            }
        }
    }

    /// <summary>
    /// 应用程序配置
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 代理配置
        /// </summary>
        public ProxyConfig Proxy { get; set; } = new ProxyConfig();

        /// <summary>
        /// 日志配置
        /// </summary>
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        /// <summary>
        /// 是否启用自动导航到 iframe 内容
        /// </summary>
        public bool EnableAutoIframeNavigation { get; set; }
        
        /// <summary>
        /// UI配置
        /// </summary>
        public UIConfig Ui { get; set; } = new UIConfig();

        /// <summary>
        /// 基础URL
        /// </summary>
        public string BaseURL { get; set; } = "https://game.ero-labs.live";

        public bool DebugMode { get; set; } = false;
    }

    /// <summary>
    /// 代理配置
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// 是否启用代理
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// HTTP代理地址
        /// </summary>
        [JsonPropertyName("http_proxy")]
        public string HttpProxy { get; set; } = string.Empty;

        /// <summary>
        /// HTTPS代理地址
        /// </summary>
        [JsonPropertyName("https_proxy")]
        public string HttpsProxy { get; set; } = string.Empty;

        /// <summary>
        /// SOCKS5代理地址
        /// </summary>
        public string Socks5 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 日志配置
    /// </summary>
    public class LoggingConfig
    {
        /// <summary>
        /// 是否启用日志
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 日志级别 (Debug, Info, Warning, Error)
        /// </summary>
        public string LogLevel { get; set; } = "Info";

        /// <summary>
        /// 日志文件最大大小（字节）
        /// </summary>
        public long MaxFileSize { get; set; }
    }
    
    /// <summary>
    /// UI配置
    /// </summary>
    public class UIConfig
    {
        /// <summary>
        /// 窗口宽度
        /// </summary>
        public int WindowWidth { get; set; }
        
        /// <summary>
        /// 窗口高度
        /// </summary>
        public int WindowHeight { get; set; }
    }
}
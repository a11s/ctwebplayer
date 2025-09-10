using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ctwebplayer
{
    /// <summary>
    /// 缓存管理器，负责管理WebView2的静态资源缓存
    /// </summary>
    public class CacheManager
    {
        private readonly string _cacheRootPath;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, bool> _downloadingUrls;
        
        // 需要缓存的文件扩展名
        private readonly string[] _cacheableExtensions = new[]
        {
            ".js", ".css", ".jpg", ".jpeg", ".png", ".gif", ".webp", 
            ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot", ".otf"
        };

        public CacheManager(string cacheRootPath = "./cache")
        {
            _cacheRootPath = Path.GetFullPath(cacheRootPath);
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _downloadingUrls = new ConcurrentDictionary<string, bool>();
            
            // 确保缓存根目录存在
            Directory.CreateDirectory(_cacheRootPath);
            LogManager.Instance.Info($"缓存管理器初始化完成，缓存目录：{_cacheRootPath}");
        }

        /// <summary>
        /// 检查URL是否应该被缓存
        /// </summary>
        public bool ShouldCache(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath.ToLower();
                
                // 不缓存API请求
                if (path.Contains("/api/") || path.Contains("/v2/") || path.Contains("/v1/"))
                {
                    return false;
                }
                
                // 检查文件扩展名
                foreach (var ext in _cacheableExtensions)
                {
                    if (path.EndsWith(ext))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取URL对应的缓存文件路径
        /// </summary>
        public string GetCacheFilePath(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host;
                var path = uri.AbsolutePath.TrimStart('/');
                
                // 处理查询参数（版本号等）
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    // 使用URL的哈希值作为文件名的一部分，以区分不同版本
                    var hash = GetUrlHash(url);
                    var extension = Path.GetExtension(path);
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                    var directory = Path.GetDirectoryName(path)?.Replace('/', Path.DirectorySeparatorChar) ?? "";
                    
                    path = Path.Combine(directory, $"{fileNameWithoutExt}_{hash}{extension}");
                }
                
                // 构建完整的缓存文件路径
                var cacheFilePath = Path.Combine(_cacheRootPath, host, path.Replace('/', Path.DirectorySeparatorChar));
                return cacheFilePath;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"获取缓存文件路径失败：{url}", ex);
                return null;
            }
        }

        /// <summary>
        /// 检查缓存文件是否存在
        /// </summary>
        public bool IsCached(string url)
        {
            var cacheFilePath = GetCacheFilePath(url);
            return !string.IsNullOrEmpty(cacheFilePath) && File.Exists(cacheFilePath);
        }

        /// <summary>
        /// 从缓存读取资源
        /// </summary>
        public async Task<CacheResult> GetFromCacheAsync(string url)
        {
            try
            {
                var cacheFilePath = GetCacheFilePath(url);
                if (string.IsNullOrEmpty(cacheFilePath) || !File.Exists(cacheFilePath))
                {
                    return new CacheResult { Success = false };
                }

                var data = await File.ReadAllBytesAsync(cacheFilePath);
                var mimeType = GetMimeType(cacheFilePath);
                
                LogManager.Instance.Debug($"从缓存读取：{url} -> {cacheFilePath}");
                
                return new CacheResult
                {
                    Success = true,
                    Data = data,
                    MimeType = mimeType,
                    FilePath = cacheFilePath
                };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"读取缓存失败：{url}", ex);
                return new CacheResult { Success = false };
            }
        }

        /// <summary>
        /// 下载资源并保存到缓存
        /// </summary>
        public async Task<CacheResult> DownloadAndCacheAsync(string url)
        {
            // 防止重复下载
            if (!_downloadingUrls.TryAdd(url, true))
            {
                // 等待其他线程完成下载
                await Task.Delay(100);
                return await GetFromCacheAsync(url);
            }

            try
            {
                LogManager.Instance.Debug($"开始下载：{url}");
                
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    LogManager.Instance.Warning($"下载失败：{url} - 状态码：{response.StatusCode}");
                    return new CacheResult { Success = false };
                }

                var data = await response.Content.ReadAsByteArrayAsync();
                var mimeType = response.Content.Headers.ContentType?.MediaType ?? GetMimeType(url);
                
                // 保存到缓存
                var cacheFilePath = GetCacheFilePath(url);
                if (!string.IsNullOrEmpty(cacheFilePath))
                {
                    await SaveToCacheAsync(cacheFilePath, data);
                    LogManager.Instance.Debug($"已缓存：{url} -> {cacheFilePath} ({data.Length} 字节)");
                }

                return new CacheResult
                {
                    Success = true,
                    Data = data,
                    MimeType = mimeType,
                    FilePath = cacheFilePath
                };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"下载资源失败：{url}", ex);
                return new CacheResult { Success = false };
            }
            finally
            {
                _downloadingUrls.TryRemove(url, out _);
            }
        }

        /// <summary>
        /// 保存数据到缓存文件
        /// </summary>
        private async Task SaveToCacheAsync(string filePath, byte[] data)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(filePath, data);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"保存缓存文件失败：{filePath}", ex);
            }
        }

        /// <summary>
        /// 获取URL的哈希值（用于处理带版本号的URL）
        /// </summary>
        private string GetUrlHash(string url)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(url));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 8).ToLower();
            }
        }

        /// <summary>
        /// 根据文件扩展名获取MIME类型
        /// </summary>
        private string GetMimeType(string filePathOrUrl)
        {
            var extension = Path.GetExtension(filePathOrUrl).ToLower();
            return extension switch
            {
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".html" => "text/html",
                ".htm" => "text/html",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".eot" => "application/vnd.ms-fontobject",
                ".otf" => "font/otf",
                ".json" => "application/json",
                ".xml" => "application/xml",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 清理缓存（可选功能）
        /// </summary>
        public async Task ClearCacheAsync()
        {
            try
            {
                if (Directory.Exists(_cacheRootPath))
                {
                    Directory.Delete(_cacheRootPath, true);
                    Directory.CreateDirectory(_cacheRootPath);
                    LogManager.Instance.Info("缓存已清理");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error("清理缓存失败", ex);
            }
        }

        /// <summary>
        /// 获取缓存大小
        /// </summary>
        public long GetCacheSize()
        {
            try
            {
                if (!Directory.Exists(_cacheRootPath))
                    return 0;

                var dirInfo = new DirectoryInfo(_cacheRootPath);
                return GetDirectorySize(dirInfo);
            }
            catch
            {
                return 0;
            }
        }

        private long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;
            
            // 计算所有文件大小
            foreach (var file in dirInfo.GetFiles())
            {
                size += file.Length;
            }
            
            // 递归计算子目录
            foreach (var dir in dirInfo.GetDirectories())
            {
                size += GetDirectorySize(dir);
            }
            
            return size;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 缓存操作结果
    /// </summary>
    public class CacheResult
    {
        public bool Success { get; set; }
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
    }
}
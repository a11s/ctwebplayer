using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ctwebplayer
{
    public sealed class RequestLogger
    {
        private static readonly Lazy<RequestLogger> _instance = new Lazy<RequestLogger>(() => new RequestLogger());
        private static readonly Lazy<ConfigManager> _configManagerLazy = new Lazy<ConfigManager>(() => new ConfigManager());
        private readonly object _lockObject = new object();
        private const string LogPath = "./logs/request.log";
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        private RequestLogger() { }

        public static RequestLogger Instance => _instance.Value;

    public async Task WriteRequestLog(string url, string cacheStatus, long? responseSize = null, string? additionalInfo = null)
    {
        if (!_configManagerLazy.Value.Config.DebugMode)
            return;

        try
        {
            string sizeStr = responseSize?.ToString() ?? "N/A";
            string infoStr = additionalInfo ?? string.Empty;
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {url} {cacheStatus} {sizeStr} {infoStr}\n";

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Check file size and rotate if necessary
                    if (File.Exists(LogPath))
                    {
                        var fileInfo = new FileInfo(LogPath);
                        if (fileInfo.Length > MaxFileSize)
                        {
                            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string backupPath = LogPath + "." + timestamp;
                            File.Move(LogPath, backupPath);
                        }
                    }

                    File.AppendAllText(LogPath, logEntry, Encoding);
                }
            });
        }
        catch (IOException)
        {
            // Silently ignore IO errors to avoid blocking
        }
        catch (Exception ex)
        {
            // Log critical errors if possible, but avoid recursion
            Debug.WriteLine($"RequestLogger error: {ex.Message}");
        }
    }

    public async Task ClearLog()
    {
        try
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    if (File.Exists(LogPath))
                    {
                        File.Delete(LogPath);
                    }
                }
            });
        }
        catch (IOException)
        {
            // Silently ignore
        }
    }
    }
}
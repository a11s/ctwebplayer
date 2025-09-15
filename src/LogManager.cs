using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;

namespace ctwebplayer
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// 日志管理器 - 单例模式
    /// </summary>
    public sealed class LogManager
    {
        private static readonly Lazy<LogManager> _instance = new Lazy<LogManager>(() => new LogManager());
        private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly System.Threading.Timer _flushTimer;
        private readonly object _lockObject = new object();
        
        private string _logFilePath = "./logs/app.log";
        private long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private LogLevel _minLogLevel = LogLevel.Info;
        private bool _isEnabled = true;
        private int _bufferSize = 100; // 缓冲区大小
        private volatile bool _isDisposing = false;

        /// <summary>
        /// 获取LogManager单例实例
        /// </summary>
        public static LogManager Instance => _instance.Value;

        /// <summary>
        /// 日志条目内部类
        /// </summary>
        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public Exception? Exception { get; set; }
        }

        private LogManager()
        {
            // 每秒刷新一次日志缓冲区
            _flushTimer = new System.Threading.Timer(async _ => await FlushLogsAsync(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            
            // 注册应用程序退出事件
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// 配置日志管理器
        /// </summary>
        public void Configure(bool isEnabled, LogLevel minLogLevel, long maxFileSize)
        {
            _isEnabled = isEnabled;
            _minLogLevel = minLogLevel;
            _maxFileSize = maxFileSize;
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public void Debug(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Debug, message, GetClassName(filePath), null);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public void Info(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Info, message, GetClassName(filePath), null);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public void Warning(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Warning, message, GetClassName(filePath), null);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public void Error(string message, Exception? ex = null, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            Log(LogLevel.Error, message, GetClassName(filePath), ex);
        }

        /// <summary>
        /// 核心日志记录方法
        /// </summary>
        private void Log(LogLevel level, string message, string className, Exception? exception)
        {
            if (!_isEnabled || level < _minLogLevel || _isDisposing)
                return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                ClassName = className,
                Message = message,
                Exception = exception
            };

            _logQueue.Enqueue(entry);

            // 如果队列大小超过缓冲区大小，立即刷新
            if (_logQueue.Count >= _bufferSize)
            {
                Task.Run(async () => await FlushLogsAsync());
            }
        }

        /// <summary>
        /// 异步刷新日志到文件
        /// </summary>
        private async Task FlushLogsAsync()
        {
            if (_isDisposing || _logQueue.IsEmpty)
                return;

            await _semaphore.WaitAsync();
            try
            {
                var logs = new StringBuilder();
                var count = 0;

                while (_logQueue.TryDequeue(out var entry) && count < _bufferSize)
                {
                    logs.AppendLine(FormatLogEntry(entry));
                    count++;
                }

                if (logs.Length > 0)
                {
                    await WriteToFileAsync(logs.ToString());
                }
            }
            catch (Exception ex)
            {
                // 避免日志系统本身的错误导致程序崩溃
                System.Diagnostics.Debug.WriteLine($"LogManager FlushLogsAsync error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 格式化日志条目
        /// </summary>
        private string FormatLogEntry(LogEntry entry)
        {
            var message = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] [{entry.ClassName}] {entry.Message}";
            
            if (entry.Exception != null)
            {
                message += $"\nException: {entry.Exception.GetType().Name}: {entry.Exception.Message}\nStackTrace: {entry.Exception.StackTrace}";
            }

            return message;
        }

        /// <summary>
        /// 异步写入文件
        /// </summary>
        private async Task WriteToFileAsync(string content)
        {
            try
            {
                // 检查文件大小并进行轮转
                await CheckAndRotateLogFileAsync();

                // 确保目录存在
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 异步追加写入文件
                using (var writer = new StreamWriter(_logFilePath, append: true, encoding: Encoding.UTF8))
                {
                    await writer.WriteAsync(content);
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogManager WriteToFileAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查并轮转日志文件
        /// </summary>
        private async Task CheckAndRotateLogFileAsync()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return;

                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length >= _maxFileSize)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var archivePath = $"{_logFilePath}.{timestamp}";
                    
                    // 移动当前日志文件到归档文件
                    await Task.Run(() => File.Move(_logFilePath, archivePath));
                    
                    // 清理旧的日志文件（保留最近5个）
                    await CleanupOldLogFilesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogManager CheckAndRotateLogFileAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理旧的日志文件
        /// </summary>
        private async Task CleanupOldLogFilesAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_logFilePath) ?? ".";
                var logFileName = Path.GetFileName(_logFilePath);
                var pattern = $"{logFileName}.*";
                
                var files = await Task.Run(() => Directory.GetFiles(directory, pattern)
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Skip(5)
                    .ToArray());

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // 忽略删除失败的文件
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogManager CleanupOldLogFilesAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件路径获取类名
        /// </summary>
        private string GetClassName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "Unknown";

            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// 强制刷新所有待写入的日志
        /// </summary>
        public async Task FlushAsync()
        {
            while (!_logQueue.IsEmpty)
            {
                await FlushLogsAsync();
            }
        }

        /// <summary>
        /// 处理进程退出事件
        /// </summary>
        private void OnProcessExit(object? sender, EventArgs e)
        {
            _isDisposing = true;
            
            // 停止定时器
            _flushTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            
            // 刷新所有剩余的日志
            FlushAsync().Wait(TimeSpan.FromSeconds(5));
            
            // 清理资源
            _flushTimer?.Dispose();
            _semaphore?.Dispose();
        }

        /// <summary>
        /// 处理未处理的异常
        /// </summary>
        private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Error("Unhandled exception occurred", ex);
                FlushAsync().Wait(TimeSpan.FromSeconds(2));
            }
        }

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        /// <summary>
        /// 清空日志文件
        /// </summary>
        public async Task ClearLogFileAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // 清空队列
                while (_logQueue.TryDequeue(out _)) { }
                
                // 删除日志文件
                if (File.Exists(_logFilePath))
                {
                    await Task.Run(() => File.Delete(_logFilePath));
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// 清理所有日志文件（包括归档的）
        /// </summary>
        public async Task ClearLogsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // 清空队列
                while (_logQueue.TryDequeue(out _)) { }
                
                // 获取日志目录
                var directory = Path.GetDirectoryName(_logFilePath) ?? ".";
                var logFileName = Path.GetFileName(_logFilePath);
                
                // 删除主日志文件
                if (File.Exists(_logFilePath))
                {
                    await Task.Run(() => File.Delete(_logFilePath));
                }
                
                // 删除所有归档的日志文件
                var pattern = $"{logFileName}.*";
                var archiveFiles = await Task.Run(() => Directory.GetFiles(directory, pattern));
                
                foreach (var file in archiveFiles)
                {
                    try
                    {
                        await Task.Run(() => File.Delete(file));
                    }
                    catch
                    {
                        // 忽略删除失败的文件
                    }
                }
                
                Info("所有日志文件已清理");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogManager ClearLogsAsync error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
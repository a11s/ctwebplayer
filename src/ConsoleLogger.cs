using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ctwebplayer
{
    /// <summary>
    /// 浏览器控制台日志级别
    /// </summary>
    public enum ConsoleLogLevel
    {
        Log = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Debug = 4
    }

    /// <summary>
    /// 控制台日志管理器 - 记录浏览器console输出
    /// 继承现有日志架构模式，实现日志轮转和大小限制
    /// </summary>
    public sealed class ConsoleLogger
    {
        private static readonly Lazy<ConsoleLogger> _instance = new Lazy<ConsoleLogger>(() => new ConsoleLogger());
        private readonly ConcurrentQueue<ConsoleLogEntry> _logQueue = new ConcurrentQueue<ConsoleLogEntry>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly System.Threading.Timer _flushTimer;
        private readonly object _lockObject = new object();
        
        private string _logFilePath = "./logs/console.log";
        private long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private bool _isEnabled = true;
        private int _bufferSize = 100; // 缓冲区大小
        private volatile bool _isDisposing = false;
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

        /// <summary>
        /// 获取ConsoleLogger单例实例
        /// </summary>
        public static ConsoleLogger Instance => _instance.Value;

        /// <summary>
        /// 控制台日志条目
        /// </summary>
        public class ConsoleLogEntry
        {
            public DateTime Timestamp { get; set; }
            public ConsoleLogLevel Level { get; set; }
            public string Source { get; set; } = string.Empty;  // 来源URL或脚本位置
            public string Message { get; set; } = string.Empty;
            public object[]? Arguments { get; set; }  // console.log可能包含多个参数
            public string? StackTrace { get; set; }  // 错误堆栈跟踪
        }

        private ConsoleLogger()
        {
            // 程序启动时进行日志轮转
            LogManager.RotateLogFileOnStartup(_logFilePath);
            
            // 每秒刷新一次日志缓冲区
            _flushTimer = new System.Threading.Timer(async _ => await FlushLogsAsync(), null, 
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            
            // 注册应用程序退出事件
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// 配置控制台日志记录器
        /// </summary>
        public void Configure(bool isEnabled, long maxFileSize = 10 * 1024 * 1024)
        {
            _isEnabled = isEnabled;
            _maxFileSize = maxFileSize;
        }

        /// <summary>
        /// 记录console.log输出
        /// </summary>
        public void Log(string message, string source = "", params object[] args)
        {
            RecordConsoleOutput(ConsoleLogLevel.Log, message, source, args, null);
        }

        /// <summary>
        /// 记录console.info输出
        /// </summary>
        public void Info(string message, string source = "", params object[] args)
        {
            RecordConsoleOutput(ConsoleLogLevel.Info, message, source, args, null);
        }

        /// <summary>
        /// 记录console.warn输出
        /// </summary>
        public void Warn(string message, string source = "", params object[] args)
        {
            RecordConsoleOutput(ConsoleLogLevel.Warn, message, source, args, null);
        }

        /// <summary>
        /// 记录console.error输出
        /// </summary>
        public void Error(string message, string source = "", string? stackTrace = null, params object[] args)
        {
            RecordConsoleOutput(ConsoleLogLevel.Error, message, source, args, stackTrace);
        }

        /// <summary>
        /// 记录console.debug输出
        /// </summary>
        public void Debug(string message, string source = "", params object[] args)
        {
            RecordConsoleOutput(ConsoleLogLevel.Debug, message, source, args, null);
        }

        /// <summary>
        /// 从WebView2记录控制台消息
        /// </summary>
        public void LogFromWebView(string kind, string message, string source, int line, int column)
        {
            var level = kind.ToLower() switch
            {
                "error" => ConsoleLogLevel.Error,
                "warning" => ConsoleLogLevel.Warn,
                "info" => ConsoleLogLevel.Info,
                "log" => ConsoleLogLevel.Log,
                _ => ConsoleLogLevel.Debug
            };

            var sourceInfo = !string.IsNullOrEmpty(source) ? $"{source}:{line}:{column}" : "";
            RecordConsoleOutput(level, message, sourceInfo, null, null);
        }

        /// <summary>
        /// 核心记录方法
        /// </summary>
        private void RecordConsoleOutput(ConsoleLogLevel level, string message, string source, 
            object[]? arguments, string? stackTrace)
        {
            if (!_isEnabled || _isDisposing)
                return;

            var entry = new ConsoleLogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message,
                Arguments = arguments,
                StackTrace = stackTrace
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
                    logs.AppendLine(FormatConsoleEntry(entry));
                    count++;
                }

                if (logs.Length > 0)
                {
                    await WriteToFileAsync(logs.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConsoleLogger FlushLogsAsync error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 格式化控制台日志条目
        /// </summary>
        private string FormatConsoleEntry(ConsoleLogEntry entry)
        {
            var message = new StringBuilder();
            message.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
            message.Append($"[{entry.Level.ToString().ToUpper()}] ");
            
            if (!string.IsNullOrEmpty(entry.Source))
            {
                message.Append($"[{entry.Source}] ");
            }
            
            message.Append(entry.Message);
            
            // 如果有额外参数，将它们序列化
            if (entry.Arguments != null && entry.Arguments.Length > 0)
            {
                message.Append(" | Args: ");
                foreach (var arg in entry.Arguments)
                {
                    message.Append($"{arg?.ToString() ?? "null"} ");
                }
            }
            
            // 如果有堆栈跟踪，添加它
            if (!string.IsNullOrEmpty(entry.StackTrace))
            {
                message.AppendLine();
                message.Append($"  Stack Trace: {entry.StackTrace}");
            }

            return message.ToString();
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
                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        File.AppendAllText(_logFilePath, content, Encoding);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConsoleLogger WriteToFileAsync error: {ex.Message}");
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
                    // 使用LogManager的轮转方法，保持一致性
                    await Task.Run(() => LogManager.RotateLogFileOnStartup(_logFilePath));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConsoleLogger CheckAndRotateLogFileAsync error: {ex.Message}");
            }
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
                Error("Unhandled exception in console context", "", ex.StackTrace ?? "", ex.Message);
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
        /// 清空控制台日志文件
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
        /// 清理所有控制台日志文件（包括归档的）
        /// </summary>
        public async Task ClearAllConsoleLogsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // 清空队列
                while (_logQueue.TryDequeue(out _)) { }
                
                // 获取日志目录
                var directory = Path.GetDirectoryName(_logFilePath) ?? ".";
                var fileName = Path.GetFileNameWithoutExtension(_logFilePath);
                var extension = Path.GetExtension(_logFilePath);
                
                // 删除主日志文件
                if (File.Exists(_logFilePath))
                {
                    await Task.Run(() => File.Delete(_logFilePath));
                }
                
                // 删除所有归档的日志文件 (console.1.log, console.2.log, etc.)
                var pattern = $"{fileName}.*{extension}";
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
                
                System.Diagnostics.Debug.WriteLine("All console log files cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConsoleLogger ClearAllConsoleLogsAsync error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 批量记录控制台消息
        /// </summary>
        public void LogBatch(ConsoleLogEntry[] entries)
        {
            if (!_isEnabled || _isDisposing || entries == null)
                return;

            foreach (var entry in entries)
            {
                _logQueue.Enqueue(entry);
            }

            // 如果队列过大，立即刷新
            if (_logQueue.Count >= _bufferSize)
            {
                Task.Run(async () => await FlushLogsAsync());
            }
        }

        /// <summary>
        /// 设置日志文件路径
        /// </summary>
        public void SetLogFilePath(string path)
        {
            lock (_lockObject)
            {
                _logFilePath = path;
            }
        }

        /// <summary>
        /// 获取日志统计信息
        /// </summary>
        public async Task<(long fileSize, int rotatedFiles)> GetLogStatisticsAsync()
        {
            return await Task.Run(() =>
            {
                long totalSize = 0;
                int fileCount = 0;

                try
                {
                    // 检查主日志文件
                    if (File.Exists(_logFilePath))
                    {
                        totalSize += new FileInfo(_logFilePath).Length;
                        fileCount++;
                    }

                    // 检查轮转的日志文件
                    var directory = Path.GetDirectoryName(_logFilePath) ?? ".";
                    var fileName = Path.GetFileNameWithoutExtension(_logFilePath);
                    var extension = Path.GetExtension(_logFilePath);
                    
                    for (int i = 1; i <= 10; i++)
                    {
                        var rotatedPath = Path.Combine(directory, $"{fileName}.{i}{extension}");
                        if (File.Exists(rotatedPath))
                        {
                            totalSize += new FileInfo(rotatedPath).Length;
                            fileCount++;
                        }
                    }
                }
                catch
                {
                    // 忽略错误
                }

                return (totalSize, fileCount - 1); // 减1因为不计算主文件
            });
        }
    }
}
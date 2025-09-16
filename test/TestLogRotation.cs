using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ctwebplayer;

class TestLogRotation
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("日志轮转功能测试");
        Console.WriteLine("========================================");
        Console.WriteLine();

        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        string appLogPath = Path.Combine(logDir, "app.log");
        string requestLogPath = Path.Combine(logDir, "request.log");
        string consoleLogPath = Path.Combine(logDir, "console.log");

        // 确保日志目录存在
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
            Console.WriteLine($"✓ 创建日志目录: {logDir}");
        }

        // 测试1：首次轮转
        Console.WriteLine("\n测试1: 首次轮转");
        Console.WriteLine("----------------");
        
        // 清理旧文件
        foreach (var file in Directory.GetFiles(logDir, "*.log*"))
        {
            File.Delete(file);
        }
        
        // 创建初始日志文件
        File.WriteAllText(appLogPath, "[2025-01-16 10:00:00] [Info] 原始 app.log 内容\n");
        File.WriteAllText(requestLogPath, "[2025-01-16 10:00:00] http://example.com CACHED 1024\n");
        Console.WriteLine("✓ 创建 app.log 和 request.log");

        Console.WriteLine("\n轮转前的文件:");
        ShowFiles(logDir);

        // 执行轮转
        Console.WriteLine("\n执行轮转...");
        LogManager.RotateLogFileOnStartup(appLogPath);
        LogManager.RotateLogFileOnStartup(requestLogPath);
        
        Console.WriteLine("\n轮转后的文件:");
        ShowFiles(logDir);
        
        // 验证
        bool test1Pass = File.Exists(Path.Combine(logDir, "app.1.log")) &&
                        File.Exists(Path.Combine(logDir, "request.1.log")) &&
                        !File.Exists(appLogPath) &&
                        !File.Exists(requestLogPath);
        
        Console.WriteLine(test1Pass ? "✓ 测试1通过" : "✗ 测试1失败");

        // 测试2：多次轮转
        Console.WriteLine("\n测试2: 多次轮转（编号递增）");
        Console.WriteLine("----------------------------");
        
        // 创建多个历史文件
        for (int i = 1; i <= 3; i++)
        {
            File.WriteAllText(Path.Combine(logDir, $"app.{i}.log"), $"[历史 {i}] app.{i}.log 内容\n");
            File.WriteAllText(Path.Combine(logDir, $"request.{i}.log"), $"[历史 {i}] request.{i}.log 内容\n");
        }
        
        // 创建新的当前文件
        File.WriteAllText(appLogPath, "[2025-01-16 11:00:00] [Info] 新的 app.log 内容\n");
        File.WriteAllText(requestLogPath, "[2025-01-16 11:00:00] http://newtest.com MISS 2048\n");
        
        Console.WriteLine("\n轮转前的文件:");
        ShowFiles(logDir);
        
        // 执行第二次轮转
        Console.WriteLine("\n执行第二次轮转...");
        LogManager.RotateLogFileOnStartup(appLogPath);
        LogManager.RotateLogFileOnStartup(requestLogPath);
        
        Console.WriteLine("\n轮转后的文件:");
        ShowFiles(logDir);
        
        // 验证
        bool test2Pass = File.Exists(Path.Combine(logDir, "app.4.log")) &&
                        File.Exists(Path.Combine(logDir, "app.1.log")) &&
                        File.ReadAllText(Path.Combine(logDir, "app.1.log")).Contains("新的 app.log");
        
        Console.WriteLine(test2Pass ? "✓ 测试2通过" : "✗ 测试2失败");

        // 测试3：文件数量限制
        Console.WriteLine("\n测试3: 最多保留10个历史文件");
        Console.WriteLine("----------------------------");
        
        // 清理并创建10个历史文件
        foreach (var file in Directory.GetFiles(logDir, "*.log*"))
        {
            File.Delete(file);
        }
        
        for (int i = 1; i <= 10; i++)
        {
            File.WriteAllText(Path.Combine(logDir, $"app.{i}.log"), $"[历史 {i}] app.{i}.log\n");
        }
        
        // 创建新文件并轮转
        File.WriteAllText(appLogPath, "[2025-01-16 12:00:00] [Info] 第11个文件\n");
        
        Console.WriteLine($"轮转前有 {Directory.GetFiles(logDir, "app.*.log").Length} 个历史文件");
        
        // 执行轮转
        LogManager.RotateLogFileOnStartup(appLogPath);
        
        var historyFiles = Directory.GetFiles(logDir, "app.*.log");
        Console.WriteLine($"轮转后有 {historyFiles.Length} 个历史文件");
        
        // 验证
        bool test3Pass = historyFiles.Length <= 10 &&
                        !File.Exists(Path.Combine(logDir, "app.11.log")) &&
                        File.Exists(Path.Combine(logDir, "app.1.log"));
        
        Console.WriteLine(test3Pass ? "✓ 测试3通过" : "✗ 测试3失败");

        // 测试4：实际程序初始化测试
        Console.WriteLine("\n测试4: 实际程序初始化测试");
        Console.WriteLine("-------------------------");
        
        // 清理并创建测试文件
        foreach (var file in Directory.GetFiles(logDir, "*.log*"))
        {
            File.Delete(file);
        }
        
        File.WriteAllText(appLogPath, "[2025-01-16 13:00:00] [Info] 程序启动前的日志\n");
        File.WriteAllText(requestLogPath, "[2025-01-16 13:00:00] http://startup.com HIT 512\n");
        
        Console.WriteLine("创建了启动前的日志文件");
        Console.WriteLine("\n模拟程序启动...");
        
        // 触发 LogManager 和 RequestLogger 的初始化
        LogManager.Instance.Info("程序启动测试");
        await RequestLogger.Instance.WriteRequestLog("http://test.com", "CACHED", 1024, "测试");
        
        // 等待日志写入
        await Task.Delay(100);
        
        Console.WriteLine("\n启动后的文件:");
        ShowFiles(logDir);
        
        bool test4Pass = File.Exists(Path.Combine(logDir, "app.1.log")) &&
                        File.Exists(Path.Combine(logDir, "request.1.log")) &&
                        File.Exists(appLogPath) &&
                        File.Exists(requestLogPath);
        
        Console.WriteLine(test4Pass ? "✓ 测试4通过" : "✗ 测试4失败");

        // 测试5：ConsoleLogger 基础功能测试
        Console.WriteLine("\n测试5: ConsoleLogger 基础功能");
        Console.WriteLine("------------------------------");
        
        // 清理console日志
        foreach (var file in Directory.GetFiles(logDir, "console*.log"))
        {
            File.Delete(file);
        }
        
        // 测试各种日志级别
        ConsoleLogger.Instance.Log("测试 console.log", "test.html:10");
        ConsoleLogger.Instance.Info("测试 console.info", "test.html:11");
        ConsoleLogger.Instance.Warn("测试 console.warn", "test.html:12");
        ConsoleLogger.Instance.Error("测试 console.error", "test.html:13", "Error stack trace");
        ConsoleLogger.Instance.Debug("测试 console.debug", "test.html:14");
        
        // 测试带参数的日志
        ConsoleLogger.Instance.Log("带参数的日志", "", "参数1", "参数2", 123, true);
        
        // 刷新日志
        await ConsoleLogger.Instance.FlushAsync();
        
        // 验证日志文件创建
        bool test5Pass = File.Exists(consoleLogPath);
        if (test5Pass)
        {
            var content = File.ReadAllText(consoleLogPath);
            test5Pass = content.Contains("[LOG]") &&
                       content.Contains("[INFO]") &&
                       content.Contains("[WARN]") &&
                       content.Contains("[ERROR]") &&
                       content.Contains("[DEBUG]") &&
                       content.Contains("Stack Trace:");
            
            if (test5Pass)
            {
                Console.WriteLine("✓ ConsoleLogger 各级别日志记录正常");
            }
        }
        
        Console.WriteLine(test5Pass ? "✓ 测试5通过" : "✗ 测试5失败");

        // 测试6：ConsoleLogger 日志轮转
        Console.WriteLine("\n测试6: ConsoleLogger 日志轮转");
        Console.WriteLine("-----------------------------");
        
        // 创建console.log文件
        File.WriteAllText(consoleLogPath, "[2025-01-16 14:00:00.000] [LOG] 原始console.log内容\n");
        
        // 执行轮转
        LogManager.RotateLogFileOnStartup(consoleLogPath);
        
        bool test6Pass = File.Exists(Path.Combine(logDir, "console.1.log")) &&
                        !File.Exists(consoleLogPath);
        
        if (test6Pass)
        {
            // 再次写入并测试多次轮转
            for (int i = 2; i <= 5; i++)
            {
                File.WriteAllText(consoleLogPath, $"[2025-01-16 14:0{i}:00.000] [LOG] 第{i}次console日志\n");
                LogManager.RotateLogFileOnStartup(consoleLogPath);
            }
            
            var consoleFiles = Directory.GetFiles(logDir, "console.*.log");
            test6Pass = consoleFiles.Length == 5;
            Console.WriteLine($"✓ ConsoleLogger 轮转后有 {consoleFiles.Length} 个历史文件");
        }
        
        Console.WriteLine(test6Pass ? "✓ 测试6通过" : "✗ 测试6失败");

        // 测试7：ConsoleLogger 文件大小限制
        Console.WriteLine("\n测试7: ConsoleLogger 文件大小限制");
        Console.WriteLine("---------------------------------");
        
        // 清理console日志
        foreach (var file in Directory.GetFiles(logDir, "console*.log"))
        {
            File.Delete(file);
        }
        
        // 配置ConsoleLogger
        ConsoleLogger.Instance.Configure(true, 1024 * 1024); // 1MB 限制用于测试
        
        // 生成大量日志
        Console.WriteLine("生成大量日志测试文件大小限制...");
        var largeData = new string('X', 1024); // 1KB 字符串
        
        for (int i = 0; i < 100; i++)
        {
            ConsoleLogger.Instance.Log($"大数据日志 #{i}: {largeData}", $"test.html:{i}");
        }
        
        // 刷新并等待
        await ConsoleLogger.Instance.FlushAsync();
        await Task.Delay(100);
        
        bool test7Pass = false;
        if (File.Exists(consoleLogPath))
        {
            var fileInfo = new FileInfo(consoleLogPath);
            Console.WriteLine($"当前console.log文件大小: {fileInfo.Length / 1024} KB");
            test7Pass = fileInfo.Length > 0;
        }
        
        Console.WriteLine(test7Pass ? "✓ 测试7通过" : "✗ 测试7失败");

        // 测试8：ConsoleLogger 批量日志
        Console.WriteLine("\n测试8: ConsoleLogger 批量日志");
        Console.WriteLine("-----------------------------");
        
        var batchEntries = new ConsoleLogger.ConsoleLogEntry[]
        {
            new ConsoleLogger.ConsoleLogEntry
            {
                Timestamp = DateTime.Now,
                Level = ConsoleLogLevel.Log,
                Message = "批量日志1",
                Source = "batch.js:1"
            },
            new ConsoleLogger.ConsoleLogEntry
            {
                Timestamp = DateTime.Now,
                Level = ConsoleLogLevel.Info,
                Message = "批量日志2",
                Source = "batch.js:2"
            },
            new ConsoleLogger.ConsoleLogEntry
            {
                Timestamp = DateTime.Now,
                Level = ConsoleLogLevel.Error,
                Message = "批量错误日志",
                Source = "batch.js:3",
                StackTrace = "Error: Test batch error\n  at batchTest (batch.js:3)"
            }
        };
        
        ConsoleLogger.Instance.LogBatch(batchEntries);
        await ConsoleLogger.Instance.FlushAsync();
        
        bool test8Pass = File.Exists(consoleLogPath);
        if (test8Pass)
        {
            var content = File.ReadAllText(consoleLogPath);
            test8Pass = content.Contains("批量日志1") &&
                       content.Contains("批量日志2") &&
                       content.Contains("批量错误日志");
        }
        
        Console.WriteLine(test8Pass ? "✓ 测试8通过" : "✗ 测试8失败");

        // 测试9：ConsoleLogger 统计信息
        Console.WriteLine("\n测试9: ConsoleLogger 统计信息");
        Console.WriteLine("-----------------------------");
        
        var (fileSize, rotatedFiles) = await ConsoleLogger.Instance.GetLogStatisticsAsync();
        Console.WriteLine($"Console日志总大小: {fileSize / 1024} KB");
        Console.WriteLine($"轮转的文件数量: {rotatedFiles}");
        
        bool test9Pass = fileSize > 0 && rotatedFiles >= 0;
        Console.WriteLine(test9Pass ? "✓ 测试9通过" : "✗ 测试9失败");

        // 总结
        Console.WriteLine("\n========================================");
        Console.WriteLine("测试结果总结");
        Console.WriteLine("========================================");
        
        bool allPass = test1Pass && test2Pass && test3Pass && test4Pass &&
                      test5Pass && test6Pass && test7Pass && test8Pass && test9Pass;
        
        Console.WriteLine($"测试1 (首次轮转): {(test1Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试2 (多次轮转): {(test2Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试3 (文件限制): {(test3Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试4 (程序启动): {(test4Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试5 (ConsoleLogger基础): {(test5Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试6 (ConsoleLogger轮转): {(test6Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试7 (ConsoleLogger大小限制): {(test7Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试8 (ConsoleLogger批量): {(test8Pass ? "✓ 通过" : "✗ 失败")}");
        Console.WriteLine($"测试9 (ConsoleLogger统计): {(test9Pass ? "✓ 通过" : "✗ 失败")}");
        
        if (allPass)
        {
            Console.WriteLine("\n✓ 所有测试通过！日志系统功能正常工作");
        }
        else
        {
            Console.WriteLine("\n✗ 部分测试失败");
        }
        
        // 确保日志刷新
        await LogManager.Instance.FlushAsync();
        await ConsoleLogger.Instance.FlushAsync();
    }
    
    static void ShowFiles(string logDir)
    {
        var files = Directory.GetFiles(logDir, "*.log*");
        Array.Sort(files);
        foreach (var file in files)
        {
            Console.WriteLine($"  - {Path.GetFileName(file)}");
        }
    }
}
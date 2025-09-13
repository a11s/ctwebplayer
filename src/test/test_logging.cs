using System;
using System.IO;
using System.Threading.Tasks;
using ctwebplayer;

namespace LoggingTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试日志功能...\n");

            // 测试 LogManager
            Console.WriteLine("1. 测试 LogManager (app.log):");
            var logManager = LogManager.Instance;
            
            // 配置日志管理器
            logManager.Configure(true, LogLevel.Debug, 10 * 1024 * 1024);
            
            // 写入不同级别的日志
            logManager.Debug("这是一条调试日志");
            logManager.Info("这是一条信息日志");
            logManager.Warning("这是一条警告日志");
            logManager.Error("这是一条错误日志", new Exception("测试异常"));
            
            // 强制刷新日志
            await logManager.FlushAsync();
            
            // 检查日志文件是否创建
            string appLogPath = "./logs/app.log";
            if (File.Exists(appLogPath))
            {
                Console.WriteLine($"✓ app.log 已成功创建在: {Path.GetFullPath(appLogPath)}");
                Console.WriteLine($"  文件大小: {new FileInfo(appLogPath).Length} 字节");
            }
            else
            {
                Console.WriteLine("✗ app.log 创建失败");
            }

            Console.WriteLine("\n2. 测试 RequestLogger (request.log):");
            var requestLogger = RequestLogger.Instance;
            
            // 写入请求日志
            await requestLogger.WriteRequestLog(
                "https://example.com/api/data", 
                "CACHE_HIT", 
                1024, 
                "测试请求"
            );
            
            await requestLogger.WriteRequestLog(
                "https://example.com/api/users", 
                "CACHE_MISS", 
                2048, 
                "另一个测试请求"
            );
            
            // 检查日志文件是否创建
            string requestLogPath = "./logs/request.log";
            if (File.Exists(requestLogPath))
            {
                Console.WriteLine($"✓ request.log 已成功创建在: {Path.GetFullPath(requestLogPath)}");
                Console.WriteLine($"  文件大小: {new FileInfo(requestLogPath).Length} 字节");
            }
            else
            {
                Console.WriteLine("✗ request.log 创建失败");
            }

            // 显示日志内容预览
            Console.WriteLine("\n3. 日志内容预览:");
            if (File.Exists(appLogPath))
            {
                Console.WriteLine("\napp.log 内容:");
                Console.WriteLine("================");
                var appLogContent = File.ReadAllText(appLogPath);
                Console.WriteLine(appLogContent.Length > 500 ? appLogContent.Substring(0, 500) + "..." : appLogContent);
            }
            
            if (File.Exists(requestLogPath))
            {
                Console.WriteLine("\nrequest.log 内容:");
                Console.WriteLine("==================");
                var requestLogContent = File.ReadAllText(requestLogPath);
                Console.WriteLine(requestLogContent);
            }

            Console.WriteLine("\n测试完成！");
            Console.WriteLine("请按任意键退出...");
            Console.ReadKey();
        }
    }
}
# Console Logger 系统文档

## 概述
Console Logger 系统用于捕获和记录浏览器控制台输出，支持日志轮转和文件大小限制。

## 实现方式

### 当前实现：JavaScript 注入方式
由于项目使用的 WebView2 版本（1.0.2478.35）不支持原生的 ConsoleMessage 事件，我们采用了 JavaScript 注入方式来捕获 console 输出。

### 工作原理
1. **JavaScript 注入**：在页面 DOM 加载完成后，注入 JavaScript 代码覆盖原生 console 方法
2. **消息传递**：通过 `window.chrome.webview.postMessage` 将 console 消息发送到 C# 端
3. **日志记录**：C# 端接收消息并通过 ConsoleLogger 类记录到文件

## 主要组件

### 1. ConsoleLogger.cs
- 单例模式实现
- 异步缓冲写入
- 日志轮转（最多10个文件）
- 单文件最大10MB限制
- 支持多级别日志：Log、Info、Warn、Error、Debug

### 2. Form1.cs 集成
- `InjectConsoleCapture()` 方法：注入 JavaScript 代码
- `CoreWebView2_WebMessageReceived()` 方法：处理来自 JavaScript 的消息

### 3. 日志文件结构
```
logs/
├── console.log      # 当前日志文件
├── console.1.log    # 第一个轮转文件
├── console.2.log    # 第二个轮转文件
└── ...             # 最多10个文件
```

## 日志格式
```
[2024-01-17 12:00:00.123] [LOG] [https://example.com] Page loaded
[2024-01-17 12:00:01.456] [ERROR] [script.js:42:15] Uncaught TypeError
```

## 测试文件
- `test/console_logger_test.html` - HTML测试页面
- `test/test_console_logger.ps1` - PowerShell测试脚本
- `test/TestLogRotation.cs` - C#单元测试

## 升级说明
当 WebView2 升级到 1.0.1185+ 版本后，可以改用原生的 ConsoleMessage 事件：

```csharp
// 原生 API 方式（需要 WebView2 1.0.1185+）
webView2.CoreWebView2.ConsoleMessage += (sender, e) => {
    ConsoleLogger.Instance.LogFromWebView(
        e.Kind.ToString(),
        e.Message,
        e.Source,
        e.Line,
        e.Column
    );
};
```

## 配置选项
```csharp
// 启用/禁用 ConsoleLogger
ConsoleLogger.Instance.Configure(isEnabled: true);

// 设置最大文件大小（默认10MB）
ConsoleLogger.Instance.Configure(maxFileSize: 10 * 1024 * 1024);

// 自定义日志路径
ConsoleLogger.Instance.SetLogFilePath("./custom/path/console.log");
```

## 性能优化
- 使用 ConcurrentQueue 实现线程安全的日志队列
- 定时器每秒自动刷新缓冲区
- 缓冲区满时立即触发异步写入
- 程序退出时自动刷新所有待写入的日志

## 注意事项
1. JavaScript 注入方式可能无法捕获某些原生错误
2. 跨域 iframe 中的 console 输出可能无法捕获
3. 确保在程序退出时调用 `FlushAsync()` 方法
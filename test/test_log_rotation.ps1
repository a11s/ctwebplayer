# 测试日志轮转功能的脚本
# 此脚本用于验证日志文件在程序启动时的轮转功能

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "日志轮转功能测试脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 设置日志目录
$logDir = "../src/logs"
$appLog = "$logDir/app.log"
$requestLog = "$logDir/request.log"

# 创建日志目录（如果不存在）
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
    Write-Host "创建日志目录: $logDir" -ForegroundColor Green
}

# 清理旧的测试文件
Write-Host "清理旧的测试文件..." -ForegroundColor Yellow
Remove-Item "$logDir/*.log*" -Force -ErrorAction SilentlyContinue

# 测试场景1：创建初始日志文件
Write-Host ""
Write-Host "测试场景1: 创建初始日志文件" -ForegroundColor Cyan
Write-Host "----------------------------" -ForegroundColor Gray

# 创建 app.log
"[2025-01-16 10:00:00] [Info] 这是原始的 app.log 文件" | Out-File -FilePath $appLog -Encoding UTF8
Write-Host "✓ 创建 app.log" -ForegroundColor Green

# 创建 request.log
"[2025-01-16 10:00:00] http://example.com CACHED 1024 测试请求日志" | Out-File -FilePath $requestLog -Encoding UTF8
Write-Host "✓ 创建 request.log" -ForegroundColor Green

# 显示当前文件状态
Write-Host ""
Write-Host "当前日志文件状态:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 测试场景2：模拟程序启动，触发日志轮转
Write-Host ""
Write-Host "测试场景2: 模拟程序启动，触发日志轮转" -ForegroundColor Cyan
Write-Host "---------------------------------------" -ForegroundColor Gray

# 编译并运行一个简单的测试程序
$testCode = @'
using System;
using System.IO;
using ctwebplayer;

class TestLogRotation
{
    static void Main()
    {
        Console.WriteLine("测试日志轮转功能...");
        
        // 触发 LogManager 初始化（会执行日志轮转）
        LogManager.Instance.Info("程序启动，日志轮转已完成");
        
        // 触发 RequestLogger 初始化（会执行日志轮转）
        RequestLogger.Instance.WriteRequestLog("http://test.com", "CACHED", 2048, "测试").Wait();
        
        Console.WriteLine("日志轮转测试完成");
    }
}
'@

# 保存测试代码
$testFile = "$logDir/../test_rotation.cs"
$testCode | Out-File -FilePath $testFile -Encoding UTF8

# 编译测试程序
Write-Host "编译测试程序..." -ForegroundColor Yellow
$compileResult = & dotnet build ../src/ctwebplayer.csproj 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 编译成功" -ForegroundColor Green
} else {
    Write-Host "✗ 编译失败" -ForegroundColor Red
    Write-Host $compileResult
    exit 1
}

# 运行测试程序
Write-Host "运行测试程序..." -ForegroundColor Yellow
Push-Location ../src/bin/Debug/net8.0-windows
Start-Process -FilePath "ctwebplayer.exe" -ArgumentList "--test-log-rotation" -Wait -NoNewWindow
Pop-Location

# 等待一下让日志写入完成
Start-Sleep -Seconds 2

# 显示轮转后的文件状态
Write-Host ""
Write-Host "日志轮转后的文件状态:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    $content = Get-Content $_.FullName -First 1 -ErrorAction SilentlyContinue
    Write-Host "  - $($_.Name) (大小: $($_.Length) 字节)" -ForegroundColor Gray
    if ($content) {
        Write-Host "    首行: $content" -ForegroundColor DarkGray
    }
}

# 测试场景3：多次轮转测试
Write-Host ""
Write-Host "测试场景3: 多次轮转测试" -ForegroundColor Cyan
Write-Host "------------------------" -ForegroundColor Gray

# 创建多个历史文件
for ($i = 2; $i -le 5; $i++) {
    $historyFile = "$logDir/app.$i.log"
    "[历史日志 $i] 这是 app.$i.log 的内容" | Out-File -FilePath $historyFile -Encoding UTF8
    Write-Host "✓ 创建 app.$i.log" -ForegroundColor Green
}

for ($i = 2; $i -le 5; $i++) {
    $historyFile = "$logDir/request.$i.log"
    "[历史请求日志 $i] 这是 request.$i.log 的内容" | Out-File -FilePath $historyFile -Encoding UTF8
    Write-Host "✓ 创建 request.$i.log" -ForegroundColor Green
}

# 再次创建新的当前日志文件
"[2025-01-16 11:00:00] [Info] 新的 app.log 文件" | Out-File -FilePath $appLog -Encoding UTF8
"[2025-01-16 11:00:00] http://newtest.com MISS 4096 新请求" | Out-File -FilePath $requestLog -Encoding UTF8

Write-Host ""
Write-Host "执行第二次轮转前的文件状态:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 再次运行测试触发轮转
Write-Host ""
Write-Host "再次触发日志轮转..." -ForegroundColor Yellow
Push-Location ../src/bin/Debug/net8.0-windows
Start-Process -FilePath "ctwebplayer.exe" -ArgumentList "--test-log-rotation" -Wait -NoNewWindow
Pop-Location

Start-Sleep -Seconds 2

Write-Host ""
Write-Host "第二次轮转后的文件状态:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    $content = Get-Content $_.FullName -First 1 -ErrorAction SilentlyContinue
    Write-Host "  - $($_.Name) (大小: $($_.Length) 字节)" -ForegroundColor Gray
    if ($content) {
        Write-Host "    首行: $content" -ForegroundColor DarkGray
    }
}

# 测试场景4：测试最大文件数限制（10个）
Write-Host ""
Write-Host "测试场景4: 测试最大文件数限制（保留10个历史文件）" -ForegroundColor Cyan
Write-Host "--------------------------------------------------" -ForegroundColor Gray

# 创建超过10个历史文件
for ($i = 1; $i -le 12; $i++) {
    $historyFile = "$logDir/app.$i.log"
    "[超限测试 $i] 这是 app.$i.log 的内容" | Out-File -FilePath $historyFile -Encoding UTF8
}

# 创建新的当前文件
"[2025-01-16 12:00:00] [Info] 测试超限的 app.log 文件" | Out-File -FilePath $appLog -Encoding UTF8

Write-Host "创建了12个历史文件和1个当前文件" -ForegroundColor Yellow

# 触发轮转
Push-Location ../src/bin/Debug/net8.0-windows
Start-Process -FilePath "ctwebplayer.exe" -ArgumentList "--test-log-rotation" -Wait -NoNewWindow
Pop-Location

Start-Sleep -Seconds 2

Write-Host ""
Write-Host "超限测试后的文件状态（应该只有10个历史文件）:" -ForegroundColor Yellow
$appLogs = Get-ChildItem $logDir -Filter "app.*.log" | Sort-Object Name
Write-Host "app.log 历史文件数量: $($appLogs.Count)" -ForegroundColor Cyan
$appLogs | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 验证结果
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试结果验证" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$success = $true

# 检查是否有 app.1.log
if (Test-Path "$logDir/app.1.log") {
    Write-Host "✓ app.1.log 存在" -ForegroundColor Green
} else {
    Write-Host "✗ app.1.log 不存在" -ForegroundColor Red
    $success = $false
}

# 检查是否有 request.1.log
if (Test-Path "$logDir/request.1.log") {
    Write-Host "✓ request.1.log 存在" -ForegroundColor Green
} else {
    Write-Host "✗ request.1.log 不存在" -ForegroundColor Red
    $success = $false
}

# 检查历史文件数量是否不超过10个
$appHistoryCount = (Get-ChildItem $logDir -Filter "app.*.log").Count
$requestHistoryCount = (Get-ChildItem $logDir -Filter "request.*.log").Count

if ($appHistoryCount -le 10) {
    Write-Host "✓ app.log 历史文件数量: $appHistoryCount (≤10)" -ForegroundColor Green
} else {
    Write-Host "✗ app.log 历史文件数量: $appHistoryCount (>10)" -ForegroundColor Red
    $success = $false
}

if ($requestHistoryCount -le 10) {
    Write-Host "✓ request.log 历史文件数量: $requestHistoryCount (≤10)" -ForegroundColor Green
} else {
    Write-Host "✗ request.log 历史文件数量: $requestHistoryCount (>10)" -ForegroundColor Red
    $success = $false
}

# 总结
Write-Host ""
if ($success) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "所有测试通过！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "部分测试失败，请检查日志轮转实现" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
}

# 清理测试文件
if (Test-Path $testFile) {
    Remove-Item $testFile -Force
}
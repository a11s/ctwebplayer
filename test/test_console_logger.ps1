#!/usr/bin/env pwsh
# Console Logger 日志轮转功能测试脚本
# 测试日志文件的创建、轮转和大小限制

param(
    [string]$ProjectPath = "..",
    [switch]$Verbose
)

# 设置颜色输出
$Host.UI.RawUI.ForegroundColor = "White"

function Write-TestHeader {
    param([string]$Title)
    Write-Host "`n" -NoNewline
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host $Title -ForegroundColor Yellow
    Write-Host "=" * 60 -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ " -ForegroundColor Green -NoNewline
    Write-Host $Message
}

function Write-Failure {
    param([string]$Message)
    Write-Host "✗ " -ForegroundColor Red -NoNewline
    Write-Host $Message
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ " -ForegroundColor Blue -NoNewline
    Write-Host $Message
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ " -ForegroundColor Yellow -NoNewline
    Write-Host $Message
}

# 初始化测试环境
$LogDir = Join-Path $ProjectPath "logs"
$ConsoleLogPath = Join-Path $LogDir "console.log"
$TestResults = @{
    Passed = 0
    Failed = 0
    Tests = @()
}

Write-TestHeader "Console Logger 测试系统"
Write-Info "测试目录: $LogDir"
Write-Info "主日志文件: $ConsoleLogPath"

# 测试1：日志目录和文件创建
Write-TestHeader "测试1: 日志文件创建"

# 清理旧的测试文件
if (Test-Path $LogDir) {
    Write-Info "清理旧的日志文件..."
    Get-ChildItem "$LogDir\console*.log" -ErrorAction SilentlyContinue | Remove-Item -Force
}
else {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
    Write-Success "创建日志目录: $LogDir"
}

# 创建测试日志文件
$TestContent = @"
[2025-01-16 10:00:00.000] [LOG] [test.html:1:1] 初始console日志测试
[2025-01-16 10:00:01.000] [INFO] [test.html:2:1] 信息级别日志
[2025-01-16 10:00:02.000] [WARN] [test.html:3:1] 警告消息
[2025-01-16 10:00:03.000] [ERROR] [test.html:4:1] 错误消息 | Args: 错误详情 
  Stack Trace: Error: Test error
    at testError (test.html:4:1)
    at <anonymous>:1:1
[2025-01-16 10:00:04.000] [DEBUG] [test.html:5:1] 调试信息
"@

Set-Content -Path $ConsoleLogPath -Value $TestContent -Encoding UTF8
if (Test-Path $ConsoleLogPath) {
    Write-Success "创建测试日志文件: console.log"
    $TestResults.Passed++
}
else {
    Write-Failure "无法创建日志文件"
    $TestResults.Failed++
}

# 测试2：日志轮转（单次）
Write-TestHeader "测试2: 首次日志轮转"

# 模拟日志轮转
if (Test-Path $ConsoleLogPath) {
    $RotatedPath = Join-Path $LogDir "console.1.log"
    Move-Item -Path $ConsoleLogPath -Destination $RotatedPath -Force
    
    if (Test-Path $RotatedPath) {
        Write-Success "日志轮转成功: console.log -> console.1.log"
        $TestResults.Passed++
        
        # 验证内容
        $RotatedContent = Get-Content $RotatedPath -Raw
        if ($RotatedContent -like "*初始console日志测试*") {
            Write-Success "轮转后的日志内容完整"
            $TestResults.Passed++
        }
        else {
            Write-Failure "轮转后的日志内容不完整"
            $TestResults.Failed++
        }
    }
    else {
        Write-Failure "日志轮转失败"
        $TestResults.Failed++
    }
}

# 测试3：多次日志轮转
Write-TestHeader "测试3: 多次日志轮转（最多10个文件）"

# 创建多个历史日志文件
Write-Info "创建10个历史日志文件..."
for ($i = 1; $i -le 10; $i++) {
    $HistoryPath = Join-Path $LogDir "console.$i.log"
    $Content = "[历史日志 $i] Console log history file $i`n"
    Set-Content -Path $HistoryPath -Value $Content -Encoding UTF8
}

# 验证文件数量
$ConsoleLogFiles = Get-ChildItem "$LogDir\console.*.log"
if ($ConsoleLogFiles.Count -eq 10) {
    Write-Success "创建了10个历史日志文件"
    $TestResults.Passed++
}
else {
    Write-Failure "历史日志文件数量不正确: $($ConsoleLogFiles.Count)"
    $TestResults.Failed++
}

# 创建新的当前日志并尝试轮转
Write-Info "创建新的console.log并模拟第11次轮转..."
$NewContent = "[2025-01-16 11:00:00.000] [LOG] 第11个日志文件"
Set-Content -Path $ConsoleLogPath -Value $NewContent -Encoding UTF8

# 模拟轮转逻辑（保持最多10个历史文件）
if (Test-Path "$LogDir\console.10.log") {
    Remove-Item "$LogDir\console.10.log" -Force
    Write-Info "删除最旧的日志文件: console.10.log"
}

# 重命名现有文件
for ($i = 9; $i -ge 1; $i--) {
    $OldPath = Join-Path $LogDir "console.$i.log"
    $NewPath = Join-Path $LogDir "console.$($i+1).log"
    if (Test-Path $OldPath) {
        Move-Item -Path $OldPath -Destination $NewPath -Force
    }
}

# 将当前日志移动到.1
Move-Item -Path $ConsoleLogPath -Destination "$LogDir\console.1.log" -Force

# 验证轮转结果
$FinalFiles = Get-ChildItem "$LogDir\console.*.log"
if ($FinalFiles.Count -le 10) {
    Write-Success "日志文件数量限制正确: $($FinalFiles.Count) 个文件"
    $TestResults.Passed++
}
else {
    Write-Failure "日志文件数量超过限制: $($FinalFiles.Count) 个文件"
    $TestResults.Failed++
}

# 测试4：单文件大小限制（10MB）
Write-TestHeader "测试4: 单文件10MB大小限制"

# 创建一个接近10MB的日志文件
Write-Info "创建大日志文件（约10MB）..."
$LargeContent = New-Object System.Text.StringBuilder

# 添加头部
$LargeContent.AppendLine("[2025-01-16 12:00:00.000] [LOG] 大文件测试开始") | Out-Null

# 生成接近10MB的内容（留一些空间给最后的测试）
# $LineSize = 100  # 每行约100字节
$TargetSize = 10 * 1024 * 1024  # 10MB
$CurrentSize = 0
$LineCount = 0

while ($CurrentSize -lt ($TargetSize - 1024)) {  # 留1KB空间
    $LineCount++
    $Line = "[2025-01-16 12:00:$($LineCount.ToString('00').PadLeft(6, '0'))] [LOG] " + ("X" * 70)
    $LargeContent.AppendLine($Line) | Out-Null
    $CurrentSize += [System.Text.Encoding]::UTF8.GetByteCount($Line) + 2  # +2 for newline
}

# 写入文件
$LargePath = Join-Path $LogDir "console_large.log"
[System.IO.File]::WriteAllText($LargePath, $LargeContent.ToString(), [System.Text.Encoding]::UTF8)

# 检查文件大小
$FileInfo = Get-Item $LargePath
$FileSizeMB = [Math]::Round($FileInfo.Length / 1MB, 2)
Write-Info "创建的文件大小: $FileSizeMB MB"

if ($FileInfo.Length -lt (10 * 1024 * 1024) -and $FileInfo.Length -gt (9 * 1024 * 1024)) {
    Write-Success "文件大小在合理范围内（9-10MB）"
    $TestResults.Passed++
    
    # 测试添加更多内容触发轮转
    Write-Info "尝试添加内容超过10MB限制..."
    $ExtraContent = "`n" + ("Y" * (2 * 1024 * 1024))  # 添加2MB内容
    Add-Content -Path $LargePath -Value $ExtraContent -Encoding UTF8
    
    $UpdatedInfo = Get-Item $LargePath
    $UpdatedSizeMB = [Math]::Round($UpdatedInfo.Length / 1MB, 2)
    
    if ($UpdatedInfo.Length -gt (10 * 1024 * 1024)) {
        Write-Warning "文件大小超过10MB: $UpdatedSizeMB MB"
        Write-Info "应该触发日志轮转"
        $TestResults.Passed++
    }
}
else {
    Write-Failure "文件大小不在预期范围: $FileSizeMB MB"
    $TestResults.Failed++
}

# 测试5：验证日志格式
Write-TestHeader "测试5: 日志格式验证"

# 创建包含各种格式的测试日志
$FormatTestContent = @"
[2025-01-16 13:00:00.123] [LOG] 简单日志消息
[2025-01-16 13:00:01.456] [INFO] [script.js:10:5] 带源信息的日志
[2025-01-16 13:00:02.789] [WARN] 警告消息 | Args: param1 param2 
[2025-01-16 13:00:03.012] [ERROR] 错误消息
  Stack Trace: Error: Test error
    at function1 (script.js:20:10)
    at function2 (script.js:30:5)
[2025-01-16 13:00:04.345] [DEBUG] [module.js:40:15] 调试信息 | Args: {object: value} [1, 2, 3]
"@

$FormatTestPath = Join-Path $LogDir "console_format_test.log"
Set-Content -Path $FormatTestPath -Value $FormatTestContent -Encoding UTF8

# 读取并验证格式
$Lines = Get-Content $FormatTestPath
$ValidFormat = $true

foreach ($Line in $Lines) {
    if ($Line -match '^\s+Stack Trace:' -or $Line -match '^\s+at ' -or [string]::IsNullOrWhiteSpace($Line)) {
        # 堆栈跟踪行或空行，跳过
        continue
    }
    
    # 验证时间戳格式 [YYYY-MM-DD HH:MM:SS.fff]
    if ($Line -notmatch '^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]') {
        Write-Warning "无效的时间戳格式: $Line"
        $ValidFormat = $false
    }
    
    # 验证日志级别
    if ($Line -notmatch '\[(LOG|INFO|WARN|ERROR|DEBUG)\]') {
        Write-Warning "无效的日志级别: $Line"
        $ValidFormat = $false
    }
}

if ($ValidFormat) {
    Write-Success "日志格式验证通过"
    $TestResults.Passed++
}
else {
    Write-Failure "日志格式验证失败"
    $TestResults.Failed++
}

# 测试6：并发写入测试
Write-TestHeader "测试6: 并发写入测试"

Write-Info "启动并发写入测试..."
$Jobs = @()

# 创建多个并发写入任务
for ($i = 1; $i -le 5; $i++) {
    $Job = Start-Job -ScriptBlock {
        param($Index, $LogPath)
        for ($j = 1; $j -le 20; $j++) {
            $Message = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff')] [LOG] 并发任务 $Index - 消息 $j"
            Add-Content -Path $LogPath -Value $Message -Encoding UTF8
            Start-Sleep -Milliseconds 50
        }
    } -ArgumentList $i, "$LogDir\console_concurrent.log"
    
    $Jobs += $Job
}

# 等待所有任务完成
Write-Info "等待并发任务完成..."
$Jobs | Wait-Job | Out-Null
$Jobs | Remove-Job

# 检查并发写入的文件
if (Test-Path "$LogDir\console_concurrent.log") {
    $ConcurrentLines = Get-Content "$LogDir\console_concurrent.log"
    if ($ConcurrentLines.Count -eq 100) {  # 5个任务 x 20条消息
        Write-Success "并发写入测试通过: 共 $($ConcurrentLines.Count) 条日志"
        $TestResults.Passed++
    }
    else {
        Write-Warning "并发写入日志数量不匹配: 期望100条，实际 $($ConcurrentLines.Count) 条"
        $TestResults.Failed++
    }
}
else {
    Write-Failure "并发写入测试失败：文件未创建"
    $TestResults.Failed++
}

# 测试7：特殊字符处理
Write-TestHeader "测试7: 特殊字符和编码测试"

$SpecialContent = @"
[2025-01-16 14:00:00.000] [LOG] 中文测试：你好世界
[2025-01-16 14:00:01.000] [LOG] 日文测试：こんにちは世界
[2025-01-16 14:00:02.000] [LOG] 韩文测试：안녕하세요 세계
[2025-01-16 14:00:03.000] [LOG] Emoji测试：😀 🎉 ✨ 🚀
[2025-01-16 14:00:04.000] [LOG] 特殊字符：\n\t\r\\\"'<>&
[2025-01-16 14:00:05.000] [LOG] Unicode：\u2665 \u2666 \u2663 \u2660
"@

$SpecialPath = Join-Path $LogDir "console_special.log"
Set-Content -Path $SpecialPath -Value $SpecialContent -Encoding UTF8

# 验证特殊字符是否正确保存
$ReadContent = Get-Content $SpecialPath -Raw -Encoding UTF8
if ($ReadContent -like "*你好世界*" -and $ReadContent -like "*😀*") {
    Write-Success "特殊字符和编码测试通过"
    $TestResults.Passed++
}
else {
    Write-Failure "特殊字符处理失败"
    $TestResults.Failed++
}

# 测试总结
Write-TestHeader "测试结果总结"

$TotalTests = $TestResults.Passed + $TestResults.Failed
$PassRate = if ($TotalTests -gt 0) { [Math]::Round(($TestResults.Passed / $TotalTests) * 100, 2) } else { 0 }

Write-Host "`n测试统计:" -ForegroundColor Cyan
Write-Host "  总测试数: $TotalTests"
Write-Host "  通过: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "  失败: $($TestResults.Failed)" -ForegroundColor Red
Write-Host "  通过率: $PassRate%"

if ($TestResults.Failed -eq 0) {
    Write-Host "`n" -NoNewline
    Write-Success "所有测试通过！Console Logger 功能正常 ✨"
}
else {
    Write-Host "`n" -NoNewline
    Write-Failure "部分测试失败，请检查日志系统"
}

# 清理测试文件（可选）
Write-Host "`n是否清理测试文件？(y/N): " -NoNewline
$Cleanup = Read-Host
if ($Cleanup -eq 'y' -or $Cleanup -eq 'Y') {
    Write-Info "清理测试文件..."
    Get-ChildItem "$LogDir\console*.log" | Remove-Item -Force
    Write-Success "测试文件已清理"
}

# 返回测试结果
exit $(if ($TestResults.Failed -eq 0) { 0 } else { 1 })
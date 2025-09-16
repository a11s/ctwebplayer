# 简单的日志轮转功能测试脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "日志轮转功能测试（简化版）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 设置日志目录
$logDir = Join-Path (Split-Path $PSScriptRoot) "src\logs"
Write-Host "日志目录: $logDir" -ForegroundColor Yellow

# 创建日志目录（如果不存在）
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    Write-Host "✓ 创建日志目录" -ForegroundColor Green
}

# 清理旧的测试文件
Write-Host ""
Write-Host "步骤1: 清理旧的测试文件..." -ForegroundColor Cyan
Remove-Item "$logDir\*.log*" -Force -ErrorAction SilentlyContinue
Write-Host "✓ 清理完成" -ForegroundColor Green

# 测试场景1：首次轮转
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试场景1: 首次轮转" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 创建初始日志文件
Write-Host "创建初始日志文件..." -ForegroundColor Yellow
"[2025-01-16 10:00:00] [Info] 原始 app.log 内容" | Out-File -FilePath "$logDir\app.log" -Encoding UTF8
"[2025-01-16 10:00:00] http://example.com CACHED 1024" | Out-File -FilePath "$logDir\request.log" -Encoding UTF8
Write-Host "✓ 创建 app.log" -ForegroundColor Green
Write-Host "✓ 创建 request.log" -ForegroundColor Green

Write-Host ""
Write-Host "轮转前的文件:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 执行轮转（直接调用静态方法）
Write-Host ""
Write-Host "执行日志轮转..." -ForegroundColor Yellow
Add-Type -Path "..\src\bin\Debug\net8.0-windows\ctwebplayer.dll"
[ctwebplayer.LogManager]::RotateLogFileOnStartup("$logDir\app.log")
[ctwebplayer.LogManager]::RotateLogFileOnStartup("$logDir\request.log")
Write-Host "✓ 轮转完成" -ForegroundColor Green

Write-Host ""
Write-Host "轮转后的文件:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 验证轮转结果
$test1Pass = $true
if (Test-Path "$logDir\app.1.log") {
    Write-Host "✓ app.log 已轮转为 app.1.log" -ForegroundColor Green
} else {
    Write-Host "✗ app.1.log 不存在" -ForegroundColor Red
    $test1Pass = $false
}

if (Test-Path "$logDir\request.1.log") {
    Write-Host "✓ request.log 已轮转为 request.1.log" -ForegroundColor Green
} else {
    Write-Host "✗ request.1.log 不存在" -ForegroundColor Red
    $test1Pass = $false
}

if (-not (Test-Path "$logDir\app.log")) {
    Write-Host "✓ 原始 app.log 已被移动" -ForegroundColor Green
} else {
    Write-Host "✗ 原始 app.log 仍然存在" -ForegroundColor Red
    $test1Pass = $false
}

# 测试场景2：多次轮转
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试场景2: 多次轮转（编号递增）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 创建多个历史文件
Write-Host "创建历史文件..." -ForegroundColor Yellow
for ($i = 1; $i -le 3; $i++) {
    "[历史 $i] app.$i.log 内容" | Out-File -FilePath "$logDir\app.$i.log" -Encoding UTF8
    "[历史 $i] request.$i.log 内容" | Out-File -FilePath "$logDir\request.$i.log" -Encoding UTF8
}
Write-Host "✓ 创建了 app.1.log 到 app.3.log" -ForegroundColor Green
Write-Host "✓ 创建了 request.1.log 到 request.3.log" -ForegroundColor Green

# 创建新的当前文件
"[2025-01-16 11:00:00] [Info] 新的 app.log 内容" | Out-File -FilePath "$logDir\app.log" -Encoding UTF8
"[2025-01-16 11:00:00] http://newtest.com MISS 2048" | Out-File -FilePath "$logDir\request.log" -Encoding UTF8

Write-Host ""
Write-Host "轮转前的文件:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 执行第二次轮转
Write-Host ""
Write-Host "执行第二次轮转..." -ForegroundColor Yellow
[ctwebplayer.LogManager]::RotateLogFileOnStartup("$logDir\app.log")
[ctwebplayer.LogManager]::RotateLogFileOnStartup("$logDir\request.log")
Write-Host "✓ 轮转完成" -ForegroundColor Green

Write-Host ""
Write-Host "轮转后的文件:" -ForegroundColor Yellow
Get-ChildItem $logDir -Filter "*.log*" | Sort-Object Name | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 验证编号是否正确递增
$test2Pass = $true
if (Test-Path "$logDir\app.4.log") {
    Write-Host "✓ 原 app.3.log 已变为 app.4.log" -ForegroundColor Green
} else {
    Write-Host "✗ app.4.log 不存在" -ForegroundColor Red
    $test2Pass = $false
}

if (Test-Path "$logDir\app.1.log") {
    $content = Get-Content "$logDir\app.1.log" -First 1
    if ($content -like "*新的 app.log*") {
        Write-Host "✓ 新文件正确轮转为 app.1.log" -ForegroundColor Green
    } else {
        Write-Host "✗ app.1.log 内容不正确" -ForegroundColor Red
        $test2Pass = $false
    }
}

# 测试场景3：超过10个文件的限制
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试场景3: 最多保留10个历史文件" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 清理并创建10个历史文件
Remove-Item "$logDir\*.log*" -Force -ErrorAction SilentlyContinue
Write-Host "创建10个历史文件..." -ForegroundColor Yellow
for ($i = 1; $i -le 10; $i++) {
    "[历史 $i] app.$i.log" | Out-File -FilePath "$logDir\app.$i.log" -Encoding UTF8
}

# 创建新文件并轮转
"[2025-01-16 12:00:00] [Info] 第11个文件" | Out-File -FilePath "$logDir\app.log" -Encoding UTF8

Write-Host ""
Write-Host "轮转前有 $((Get-ChildItem $logDir -Filter 'app.*.log').Count) 个历史文件" -ForegroundColor Yellow

Write-Host "执行轮转（应删除最旧的 app.10.log）..." -ForegroundColor Yellow
[ctwebplayer.LogManager]::RotateLogFileOnStartup("$logDir\app.log")

$historyFiles = Get-ChildItem $logDir -Filter "app.*.log" | Sort-Object Name
Write-Host ""
Write-Host "轮转后有 $($historyFiles.Count) 个历史文件:" -ForegroundColor Yellow
$historyFiles | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}

# 验证文件数量限制
$test3Pass = $true
if ($historyFiles.Count -le 10) {
    Write-Host "✓ 历史文件数量 ≤ 10" -ForegroundColor Green
} else {
    Write-Host "✗ 历史文件数量 > 10" -ForegroundColor Red
    $test3Pass = $false
}

if (-not (Test-Path "$logDir\app.11.log")) {
    Write-Host "✓ app.11.log 不存在（最旧的文件已删除）" -ForegroundColor Green
} else {
    Write-Host "✗ app.11.log 仍然存在" -ForegroundColor Red
    $test3Pass = $false
}

if (Test-Path "$logDir\app.1.log") {
    Write-Host "✓ app.1.log 存在（新文件）" -ForegroundColor Green
} else {
    Write-Host "✗ app.1.log 不存在" -ForegroundColor Red
    $test3Pass = $false
}

# 总结
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试结果总结" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$allPass = $test1Pass -and $test2Pass -and $test3Pass

if ($test1Pass) {
    Write-Host "✓ 测试场景1: 首次轮转 - 通过" -ForegroundColor Green
} else {
    Write-Host "✗ 测试场景1: 首次轮转 - 失败" -ForegroundColor Red
}

if ($test2Pass) {
    Write-Host "✓ 测试场景2: 多次轮转 - 通过" -ForegroundColor Green
} else {
    Write-Host "✗ 测试场景2: 多次轮转 - 失败" -ForegroundColor Red
}

if ($test3Pass) {
    Write-Host "✓ 测试场景3: 文件数量限制 - 通过" -ForegroundColor Green
} else {
    Write-Host "✗ 测试场景3: 文件数量限制 - 失败" -ForegroundColor Red
}

Write-Host ""
if ($allPass) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✓ 所有测试通过！日志轮转功能正常工作" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "✗ 部分测试失败，请检查实现" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
}
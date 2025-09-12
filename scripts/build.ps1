# CTWebPlayer 构建脚本
# 用于构建项目并生成发布版本
# 需要: .NET 8.0 SDK

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "publish"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer 构建脚本" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET SDK
Write-Host "检查 .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "找到 .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "错误: 未找到 .NET SDK，请先安装 .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# 清理之前的构建输出
Write-Host ""
Write-Host "清理之前的构建输出..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "已清理目录: $OutputDir" -ForegroundColor Green
}

if (Test-Path "bin\$Configuration") {
    Remove-Item -Path "bin\$Configuration" -Recurse -Force
    Write-Host "已清理目录: bin\$Configuration" -ForegroundColor Green
}

if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force
    Write-Host "已清理目录: obj" -ForegroundColor Green
}

# 还原项目依赖
Write-Host ""
Write-Host "还原项目依赖..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 依赖还原失败" -ForegroundColor Red
    exit 1
}
Write-Host "依赖还原成功" -ForegroundColor Green

# 构建项目
Write-Host ""
Write-Host "开始构建项目..." -ForegroundColor Yellow
Write-Host "配置: $Configuration" -ForegroundColor Cyan
Write-Host "运行时: $Runtime" -ForegroundColor Cyan
Write-Host "输出目录: $OutputDir" -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",
    "-o", $OutputDir
)

# 包含 WebView2 运行时
$publishArgs += "-p:PublishReadyToRun=true"
$publishArgs += "-p:PublishTrimmed=false"  # 避免裁剪导致的问题

Write-Host ""
Write-Host "执行命令: dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray
dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 构建失败" -ForegroundColor Red
    exit 1
}

# 验证构建输出
Write-Host ""
Write-Host "验证构建输出..." -ForegroundColor Yellow
$exePath = Join-Path $OutputDir "ctwebplayer.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    Write-Host "构建成功!" -ForegroundColor Green
    Write-Host "可执行文件: $exePath" -ForegroundColor Cyan
    Write-Host "文件大小: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    
    # 获取文件版本信息
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath)
    if ($versionInfo.FileVersion) {
        Write-Host "文件版本: $($versionInfo.FileVersion)" -ForegroundColor Cyan
    }
} else {
    Write-Host "错误: 未找到构建输出文件" -ForegroundColor Red
    exit 1
}

# 复制配置文件和资源（如果存在）
Write-Host ""
Write-Host "复制资源文件..." -ForegroundColor Yellow

# 复制配置文件
if (Test-Path "config.json") {
    Copy-Item "config.json" -Destination $OutputDir -Force
    Write-Host "已复制: config.json" -ForegroundColor Green
}

# 复制资源目录
if (Test-Path "res") {
    Copy-Item "res" -Destination $OutputDir -Recurse -Force
    Write-Host "已复制: res/" -ForegroundColor Green
}

# 复制许可证文件
if (Test-Path "LICENSE") {
    Copy-Item "LICENSE" -Destination $OutputDir -Force
    Write-Host "已复制: LICENSE" -ForegroundColor Green
}

if (Test-Path "THIRD_PARTY_LICENSES.txt") {
    Copy-Item "THIRD_PARTY_LICENSES.txt" -Destination $OutputDir -Force
    Write-Host "已复制: THIRD_PARTY_LICENSES.txt" -ForegroundColor Green
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "构建完成!" -ForegroundColor Green
Write-Host "输出目录: $OutputDir" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
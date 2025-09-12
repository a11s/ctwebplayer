# CTWebPlayer 打包脚本
# 用于将构建输出打包成发布版本
# 依赖: 需要先运行 build.ps1

param(
    [string]$PublishDir = "publish",
    [string]$PackageDir = "release",
    [string]$Version = "1.0.0",
    [switch]$CreateInstaller = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer 打包脚本" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 检查构建输出是否存在
if (-not (Test-Path $PublishDir)) {
    Write-Host "错误: 未找到构建输出目录 '$PublishDir'" -ForegroundColor Red
    Write-Host "请先运行 build.ps1 构建项目" -ForegroundColor Yellow
    exit 1
}

$exePath = Join-Path $PublishDir "ctwebplayer.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "错误: 未找到可执行文件 '$exePath'" -ForegroundColor Red
    exit 1
}

# 创建打包目录
Write-Host "创建打包目录..." -ForegroundColor Yellow
if (Test-Path $PackageDir) {
    Remove-Item -Path $PackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PackageDir | Out-Null
Write-Host "已创建目录: $PackageDir" -ForegroundColor Green

# 设置发布文件名
$timestamp = Get-Date -Format "yyyyMMdd"
$zipFileName = "CTWebPlayer-v${Version}-win-x64.zip"
$zipFilePath = Join-Path $PackageDir $zipFileName
$checksumFileName = "CTWebPlayer-v${Version}-checksums.txt"
$checksumFilePath = Join-Path $PackageDir $checksumFileName

# 创建临时打包目录
$tempPackDir = Join-Path $PackageDir "CTWebPlayer"
New-Item -ItemType Directory -Path $tempPackDir | Out-Null

# 收集需要打包的文件
Write-Host ""
Write-Host "收集文件..." -ForegroundColor Yellow

# 复制主程序
Copy-Item $exePath -Destination $tempPackDir -Force
Write-Host "已复制: ctwebplayer.exe" -ForegroundColor Green

# 复制配置文件（如果存在）
$configPath = Join-Path $PublishDir "config.json"
if (Test-Path $configPath) {
    Copy-Item $configPath -Destination $tempPackDir -Force
    Write-Host "已复制: config.json" -ForegroundColor Green
} else {
    # 如果发布目录没有，从项目根目录查找
    if (Test-Path "config.json") {
        Copy-Item "config.json" -Destination $tempPackDir -Force
        Write-Host "已复制: config.json (从项目根目录)" -ForegroundColor Green
    }
}

# 复制资源目录
$resPath = Join-Path $PublishDir "res"
if (Test-Path $resPath) {
    Copy-Item $resPath -Destination $tempPackDir -Recurse -Force
    Write-Host "已复制: res/" -ForegroundColor Green
} elseif (Test-Path "res") {
    Copy-Item "res" -Destination $tempPackDir -Recurse -Force
    Write-Host "已复制: res/ (从项目根目录)" -ForegroundColor Green
}

# 复制许可证文件
$licensePath = Join-Path $PublishDir "LICENSE"
if (Test-Path $licensePath) {
    Copy-Item $licensePath -Destination $tempPackDir -Force
    Write-Host "已复制: LICENSE" -ForegroundColor Green
} elseif (Test-Path "LICENSE") {
    Copy-Item "LICENSE" -Destination $tempPackDir -Force
    Write-Host "已复制: LICENSE (从项目根目录)" -ForegroundColor Green
}

$thirdPartyPath = Join-Path $PublishDir "THIRD_PARTY_LICENSES.txt"
if (Test-Path $thirdPartyPath) {
    Copy-Item $thirdPartyPath -Destination $tempPackDir -Force
    Write-Host "已复制: THIRD_PARTY_LICENSES.txt" -ForegroundColor Green
} elseif (Test-Path "THIRD_PARTY_LICENSES.txt") {
    Copy-Item "THIRD_PARTY_LICENSES.txt" -Destination $tempPackDir -Force
    Write-Host "已复制: THIRD_PARTY_LICENSES.txt (从项目根目录)" -ForegroundColor Green
}

# 创建 README.txt 文件
$readmeContent = @"
CTWebPlayer v$Version
===================

Unity3D WebPlayer 专属浏览器

系统要求:
- Windows 10 或更高版本 (64位)
- Microsoft Edge WebView2 运行时 (如未安装，程序会提示下载)

使用说明:
1. 双击运行 ctwebplayer.exe
2. 程序会自动检查并提示安装 WebView2 运行时（如需要）
3. 在地址栏输入 Unity WebPlayer 游戏的 URL
4. 享受游戏！

功能特性:
- 缓存管理
- 代理设置支持
- CORS 处理
- 详细日志记录

许可证:
本软件基于 BSD 3-Clause 许可证发布
详见 LICENSE 文件

第三方组件许可证详见 THIRD_PARTY_LICENSES.txt

项目主页: https://github.com/a11s/ctwebplayer
"@

$readmeContent | Out-File -FilePath (Join-Path $tempPackDir "README.txt") -Encoding utf8
Write-Host "已创建: README.txt" -ForegroundColor Green

# 创建 ZIP 压缩包
Write-Host ""
Write-Host "创建 ZIP 压缩包..." -ForegroundColor Yellow

# 使用 .NET 压缩功能
Add-Type -AssemblyName System.IO.Compression.FileSystem

try {
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempPackDir, $zipFilePath, 
        [System.IO.Compression.CompressionLevel]::Optimal, $false)
    Write-Host "已创建: $zipFileName" -ForegroundColor Green
    
    $zipInfo = Get-Item $zipFilePath
    Write-Host "文件大小: $([math]::Round($zipInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
} catch {
    Write-Host "错误: 创建 ZIP 文件失败 - $_" -ForegroundColor Red
    exit 1
}

# 计算 SHA256 哈希值
Write-Host ""
Write-Host "计算文件哈希值..." -ForegroundColor Yellow

$checksumContent = @()
$checksumContent += "CTWebPlayer v$Version - SHA256 Checksums"
$checksumContent += "======================================="
$checksumContent += ""

# 计算 ZIP 文件的哈希
$zipHash = Get-FileHash -Path $zipFilePath -Algorithm SHA256
$checksumContent += "$($zipHash.Hash)  $zipFileName"

# 计算 EXE 文件的哈希
$exeFullPath = Join-Path $tempPackDir "ctwebplayer.exe"
$exeHash = Get-FileHash -Path $exeFullPath -Algorithm SHA256
$checksumContent += "$($exeHash.Hash)  ctwebplayer.exe"

# 保存哈希文件
$checksumContent | Out-File -FilePath $checksumFilePath -Encoding utf8
Write-Host "已创建: $checksumFileName" -ForegroundColor Green

# 显示哈希值
Write-Host ""
Write-Host "SHA256 哈希值:" -ForegroundColor Yellow
foreach ($line in $checksumContent) {
    if ($line -match "^[A-F0-9]{64}") {
        Write-Host $line -ForegroundColor DarkGray
    }
}

# 清理临时目录
Write-Host ""
Write-Host "清理临时文件..." -ForegroundColor Yellow
Remove-Item -Path $tempPackDir -Recurse -Force
Write-Host "已清理临时目录" -ForegroundColor Green

# 生成发布信息
$releaseInfoPath = Join-Path $PackageDir "release-info.json"
$releaseInfo = @{
    version = $Version
    date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    files = @(
        @{
            name = $zipFileName
            size = $zipInfo.Length
            sha256 = $zipHash.Hash
        }
    )
}

$releaseInfo | ConvertTo-Json -Depth 3 | Out-File -FilePath $releaseInfoPath -Encoding utf8
Write-Host "已创建: release-info.json" -ForegroundColor Green

# 创建安装程序（如果指定）
if ($CreateInstaller) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "创建安装程序..." -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    
    $installerScriptPath = Join-Path (Split-Path $PSScriptRoot -Parent) "installer\build-installer.ps1"
    if (Test-Path $installerScriptPath) {
        # 调用安装程序构建脚本
        & $installerScriptPath -PublishDir $PublishDir -Version $Version
        
        # 更新发布信息
        $setupFileName = "CTWebPlayer-v${Version}-Setup.exe"
        $setupFilePath = Join-Path $PackageDir $setupFileName
        if (Test-Path $setupFilePath) {
            $setupInfo = Get-Item $setupFilePath
            $setupHash = Get-FileHash -Path $setupFilePath -Algorithm SHA256
            
            # 重新加载并更新 release-info.json
            $releaseInfo = Get-Content $releaseInfoPath | ConvertFrom-Json
            $releaseInfo.files += @{
                name = $setupFileName
                size = $setupInfo.Length
                sha256 = $setupHash.Hash
            }
            
            $releaseInfo | ConvertTo-Json -Depth 3 | Out-File -FilePath $releaseInfoPath -Encoding utf8
            Write-Host "已更新 release-info.json 包含安装程序信息" -ForegroundColor Green
        }
    } else {
        Write-Host "警告: 未找到安装程序构建脚本: $installerScriptPath" -ForegroundColor Yellow
        Write-Host "跳过安装程序创建" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "打包完成!" -ForegroundColor Green
Write-Host "发布目录: $PackageDir" -ForegroundColor Cyan
Write-Host "压缩包: $zipFileName" -ForegroundColor Cyan
if ($CreateInstaller -and (Test-Path (Join-Path $PackageDir "CTWebPlayer-v${Version}-Setup.exe"))) {
    Write-Host "安装程序: CTWebPlayer-v${Version}-Setup.exe" -ForegroundColor Cyan
}
Write-Host "=====================================" -ForegroundColor Cyan
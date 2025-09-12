# CTWebPlayer 安装程序构建脚本
# 用于使用 Inno Setup 创建安装程序
# 依赖: Inno Setup 6.x

param(
    [string]$PublishDir = "..\publish",
    [string]$Version = "1.0.0",
    [switch]$Silent = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer 安装程序构建脚本" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 检查当前目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

try {
    # 检查构建输出是否存在
    if (-not (Test-Path $PublishDir)) {
        Write-Host "错误: 未找到构建输出目录 '$PublishDir'" -ForegroundColor Red
        Write-Host "请先运行 scripts/build.ps1 构建项目" -ForegroundColor Yellow
        exit 1
    }

    $exePath = Join-Path $PublishDir "ctwebplayer.exe"
    if (-not (Test-Path $exePath)) {
        Write-Host "错误: 未找到可执行文件 '$exePath'" -ForegroundColor Red
        exit 1
    }

    # 查找 Inno Setup 编译器
    Write-Host "查找 Inno Setup 编译器..." -ForegroundColor Yellow
    
    $innoSetupPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 5\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
    )
    
    $isccPath = $null
    foreach ($path in $innoSetupPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
    
    # 检查环境变量
    if (-not $isccPath) {
        $isccInPath = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
        if ($isccInPath) {
            $isccPath = $isccInPath.Source
        }
    }
    
    if (-not $isccPath) {
        Write-Host "错误: 未找到 Inno Setup 编译器 (ISCC.exe)" -ForegroundColor Red
        Write-Host ""
        Write-Host "请安装 Inno Setup 6:" -ForegroundColor Yellow
        Write-Host "1. 访问 https://jrsoftware.org/isdl.php" -ForegroundColor Gray
        Write-Host "2. 下载并安装 Inno Setup" -ForegroundColor Gray
        Write-Host "3. 确保安装到默认路径或将 ISCC.exe 添加到 PATH 环境变量" -ForegroundColor Gray
        Write-Host ""
        
        # 询问是否打开下载页面
        $response = Read-Host "是否打开 Inno Setup 下载页面? (Y/N)"
        if ($response -eq 'Y' -or $response -eq 'y') {
            Start-Process "https://jrsoftware.org/isdl.php"
        }
        
        exit 1
    }
    
    Write-Host "找到 Inno Setup: $isccPath" -ForegroundColor Green
    
    # 检查安装脚本是否存在
    $setupScriptPath = Join-Path $scriptDir "setup.iss"
    if (-not (Test-Path $setupScriptPath)) {
        Write-Host "错误: 未找到安装脚本 'setup.iss'" -ForegroundColor Red
        exit 1
    }
    
    # 更新版本号（如果指定）
    if ($Version -ne "1.0.0") {
        Write-Host "更新版本号为: $Version" -ForegroundColor Yellow
        
        # 读取并更新 setup.iss 中的版本号
        $content = Get-Content $setupScriptPath -Raw
        $content = $content -replace '#define AppVersion "[\d\.]+"', "#define AppVersion `"$Version`""
        
        # 创建临时文件
        $tempScriptPath = Join-Path $scriptDir "setup_temp.iss"
        $content | Out-File -FilePath $tempScriptPath -Encoding utf8
        $setupScriptPath = $tempScriptPath
    }
    
    # 准备输出目录
    $outputDir = Join-Path (Split-Path $scriptDir -Parent) "release"
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir | Out-Null
        Write-Host "创建输出目录: $outputDir" -ForegroundColor Green
    }
    
    # 构建命令行参数
    $arguments = @()
    
    # 静默模式
    if ($Silent) {
        $arguments += "/Q"
    }
    
    # 添加脚本路径
    $arguments += "`"$setupScriptPath`""
    
    # 编译安装程序
    Write-Host ""
    Write-Host "开始编译安装程序..." -ForegroundColor Yellow
    Write-Host "执行命令: $isccPath $($arguments -join ' ')" -ForegroundColor Gray
    Write-Host ""
    
    $process = Start-Process -FilePath $isccPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host "安装程序构建成功!" -ForegroundColor Green
        
        # 查找生成的安装程序
        $setupFileName = "CTWebPlayer-v${Version}-Setup.exe"
        $setupFilePath = Join-Path $outputDir $setupFileName
        
        if (Test-Path $setupFilePath) {
            $fileInfo = Get-Item $setupFilePath
            Write-Host "文件: $setupFileName" -ForegroundColor Cyan
            Write-Host "大小: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
            Write-Host "路径: $setupFilePath" -ForegroundColor Cyan
            
            # 计算 SHA256
            Write-Host ""
            Write-Host "计算 SHA256 哈希值..." -ForegroundColor Yellow
            $hash = Get-FileHash -Path $setupFilePath -Algorithm SHA256
            Write-Host "SHA256: $($hash.Hash)" -ForegroundColor Gray
            
            # 保存哈希值
            $hashFileName = "CTWebPlayer-v${Version}-Setup.exe.sha256"
            $hashFilePath = Join-Path $outputDir $hashFileName
            "$($hash.Hash)  $setupFileName" | Out-File -FilePath $hashFilePath -Encoding utf8
            Write-Host "已保存哈希文件: $hashFileName" -ForegroundColor Green
        }
        
        Write-Host "=====================================" -ForegroundColor Cyan
    } else {
        Write-Host ""
        Write-Host "错误: 安装程序构建失败 (退出代码: $($process.ExitCode))" -ForegroundColor Red
        exit $process.ExitCode
    }
    
} finally {
    # 清理临时文件
    if ($tempScriptPath -and (Test-Path $tempScriptPath)) {
        Remove-Item $tempScriptPath -Force
    }
    
    Pop-Location
}

# 提示后续操作
Write-Host ""
Write-Host "后续操作:" -ForegroundColor Yellow
Write-Host "1. 测试安装程序是否正常工作" -ForegroundColor Gray
Write-Host "2. 使用数字签名工具签名安装程序（可选但推荐）" -ForegroundColor Gray
Write-Host "3. 上传到 GitHub Release 或其他分发渠道" -ForegroundColor Gray
Write-Host ""
Write-Host "测试命令:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName" -ForegroundColor Gray
Write-Host ""
Write-Host "静默安装命令:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName /SILENT" -ForegroundColor Gray
Write-Host "  .\release\$setupFileName /VERYSILENT" -ForegroundColor Gray
Write-Host ""
Write-Host "自定义安装目录:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName /DIR=`"C:\MyApps\CTWebPlayer`"" -ForegroundColor Gray
Write-Host ""
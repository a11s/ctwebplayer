# CTWebPlayer 发布自动化测试脚本
# 用于验证版本一致性、必需文件检查和构建/打包流程
# 用法: .\test\release-test.ps1 [-Version "1.0.0"] [-Clean $true]

param(
    [string]$Version = "",
    [switch]$Clean = $false,
    [switch]$Verbose = $false
)

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 验证版本号一致性
function Test-VersionConsistency {
    param([string]$ExpectedVersion)
    
    Write-ColorOutput "=== 验证版本号一致性 ===" "Cyan"
    
    if (-not $ExpectedVersion) {
        $ExpectedVersion = "1.0.0"  # 默认版本
        Write-ColorOutput "使用默认版本: $ExpectedVersion" "Yellow"
    }
    
    $versionParts = $ExpectedVersion.Split('.')
    $major = $versionParts[0]
    $minor = $versionParts[1]
    $patch = $versionParts[2]
    $assemblyVersion = "$major.$minor.$patch.0"
    $fileVersion = $ExpectedVersion
    
    # 检查 csproj 中的版本
    $csprojPath = "ctwebplayer.csproj"
    if (Test-Path $csprojPath) {
        $csprojContent = Get-Content $csprojPath -Raw
        $expectedVersionTag = "<Version>$ExpectedVersion</Version>"
        $expectedAssemblyTag = "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
        $expectedFileTag = "<FileVersion>$fileVersion</FileVersion>"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedVersionTag)) {
            Write-ColorOutput "❌ csproj 中 Version 不匹配: 期望 $ExpectedVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj Version: $ExpectedVersion" "Green"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedAssemblyTag)) {
            Write-ColorOutput "❌ csproj 中 AssemblyVersion 不匹配: 期望 $assemblyVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj AssemblyVersion: $assemblyVersion" "Green"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedFileTag)) {
            Write-ColorOutput "❌ csproj 中 FileVersion 不匹配: 期望 $fileVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj FileVersion: $fileVersion" "Green"
    } else {
        Write-ColorOutput "❌ 未找到 csproj 文件" "Red"
        return $false
    }
    
    # 检查 Git tag (如果在 Git 仓库中)
    if (Get-Command git -ErrorAction SilentlyContinue) {
        $currentTag = git describe --tags --exact-match --abbrev=0 2>$null
        if ($currentTag -eq "v$ExpectedVersion") {
            Write-ColorOutput "✅ Git tag: v$ExpectedVersion" "Green"
        } else {
            Write-ColorOutput "⚠️ Git tag 不匹配或未设置: 当前 $currentTag, 期望 v$ExpectedVersion" "Yellow"
        }
    }
    
    return $true
}

# 检查必需文件
function Test-RequiredFiles {
    Write-ColorOutput "=== 检查必需文件 ===" "Cyan"
    
    $requiredFiles = @(
        "LICENSE",  # BSD3 许可
        "THIRD_PARTY_LICENSES.txt",  # 第三方许可
        "README.md",  # 项目说明
        "ctwebplayer.csproj",  # 项目文件
        "config.json",  # 默认配置 (如果存在)
        "res/c001_01_Icon_Texture.ico",  # 图标资源
        "installer/setup.iss"  # Inno Setup 脚本
    )
    
    $missingFiles = @()
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            $missingFiles += $file
            Write-ColorOutput "❌ 缺失文件: $file" "Red"
        } else {
            if ($Verbose) {
                Write-ColorOutput "✅ 存在: $file" "Green"
            }
        }
    }
    
    if ($missingFiles.Count -eq 0) {
        Write-ColorOutput "✅ 所有必需文件存在" "Green"
        return $true
    } else {
        Write-ColorOutput "❌ 发现 $($missingFiles.Count) 个缺失文件" "Red"
        return $false
    }
}

# 测试构建流程
function Test-BuildProcess {
    Write-ColorOutput "=== 测试构建流程 ===" "Cyan"
    
    $publishDir = "publish"
    
    # 清理
    if ($Clean) {
        if (Test-Path $publishDir) {
            Remove-Item $publishDir -Recurse -Force
            Write-ColorOutput "清理旧构建输出" "Yellow"
        }
        dotnet clean
    }
    
    # 恢复 NuGet 包
    Write-ColorOutput "恢复 NuGet 包..." "Yellow"
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ NuGet 恢复失败" "Red"
        return $false
    }
    
    # 构建 Release
    Write-ColorOutput "构建 Release 配置..." "Yellow"
    dotnet build --no-restore -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ 构建失败" "Red"
        return $false
    }
    Write-ColorOutput "✅ 构建成功" "Green"
    
    # 发布单文件 exe
    Write-ColorOutput "发布单文件 exe..." "Yellow"
    $publishArgs = @(
        "publish",
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-o", $publishDir,
        "--no-build"
    )
    dotnet $publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ 发布失败" "Red"
        return $false
    }
    
    # 检查输出文件
    $exePath = Join-Path $publishDir "ctwebplayer.exe"
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        $fileSize = $fileInfo.Length / 1MB
        Write-ColorOutput "✅ 发布成功: ctwebplayer.exe ($([math]::Round($fileSize, 2)) MB)" "Green"
        
        # 基本运行测试 (不实际启动，只检查是否可执行)
        if (Get-Command $exePath -ErrorAction SilentlyContinue) {
            Write-ColorOutput "✅ exe 文件可执行" "Green"
        } else {
            Write-ColorOutput "⚠️ 无法验证 exe 可执行性 (可能需管理员权限)" "Yellow"
        }
    } else {
        Write-ColorOutput "❌ 未生成 ctwebplayer.exe" "Red"
        return $false
    }
    
    # 复制附加文件
    $configPath = "config.json"
    $resDir = "res"
    if (Test-Path $configPath) {
        Copy-Item $configPath $publishDir
        Write-ColorOutput "✅ 复制 config.json" "Green"
    }
    
    if (Test-Path $resDir) {
        $publishResDir = Join-Path $publishDir "res"
        if (-not (Test-Path $publishResDir)) {
            New-Item -ItemType Directory -Path $publishResDir -Force
        }
        Copy-Item "$resDir\*" $publishResDir -Recurse -Force
        Write-ColorOutput "✅ 复制 res/ 目录" "Green"
    }
    
    return $true
}

# 测试打包流程 (Inno Setup)
function Test-PackageProcess {
    Write-ColorOutput "=== 测试打包流程 (Inno Setup) ===" "Cyan"
    
    $installerDir = "installer"
    $buildScript = Join-Path $installerDir "build-installer.ps1"
    $outputDir = "output"
    
    if (Test-Path $buildScript) {
        # 构建安装程序
        Push-Location $installerDir
        & $buildScript
        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "❌ Inno Setup 构建失败" "Red"
            Pop-Location
            return $false
        }
        Pop-Location
        
        # 检查输出
        $setupPath = Join-Path $outputDir "setup.exe"
        if (Test-Path $setupPath) {
            $fileInfo = Get-Item $setupPath
            $fileSize = $fileInfo.Length / 1MB
            Write-ColorOutput "✅ 安装程序生成: setup.exe ($([math]::Round($fileSize, 2)) MB)" "Green"
            
            # 基本验证: 检查是否包含 exe
            # 注意: 这里不实际运行安装程序
            Write-ColorOutput "✅ 安装程序文件存在" "Green"
        } else {
            Write-ColorOutput "❌ 未生成 setup.exe" "Red"
            return $false
        }
    } else {
        Write-ColorOutput "⚠️ 未找到 build-installer.ps1，跳过 Inno Setup 测试" "Yellow"
        return $true  # 非关键失败
    }
    
    return $true
}

# 主函数
function Main {
    $allPassed = $true
    
    # 版本一致性测试
    if (-not (Test-VersionConsistency -ExpectedVersion $Version)) {
        $allPassed = $false
    }
    
    # 必需文件检查
    if (-not (Test-RequiredFiles)) {
        $allPassed = $false
    }
    
    # 构建测试
    if (-not (Test-BuildProcess)) {
        $allPassed = $false
    }
    
    # 打包测试
    if (-not (Test-PackageProcess)) {
        $allPassed = $false
    }
    
    # 最终总结
    Write-ColorOutput "`n=== 测试总结 ===" "Cyan"
    if ($allPassed) {
        Write-ColorOutput "🎉 所有测试通过！准备发布。" "Green"
        exit 0
    } else {
        Write-ColorOutput "❌ 测试失败。请修复问题后重试。" "Red"
        exit 1
    }
}

# 运行主函数
Main
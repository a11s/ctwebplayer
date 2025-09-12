# CTWebPlayer 发布主脚本
# 协调整个发布流程：构建 -> 打包 -> 准备发布
# 使用方法: .\scripts\release.ps1 -Version "1.0.0"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipBuild = $false,
    [switch]$SkipTests = $false,
    [switch]$CreateTag = $false,
    [switch]$CreateGitHubRelease = $false,
    [string]$GitHubToken = $env:GITHUB_TOKEN
)

# 设置错误处理
$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer 发布流程" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "版本: v$Version" -ForegroundColor Yellow
Write-Host ""

# 验证脚本目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

# 切换到项目根目录
Push-Location $projectRoot
try {
    # 步骤 1: 更新版本号
    Write-Host "步骤 1: 更新项目版本号..." -ForegroundColor Yellow
    $csprojPath = "ctwebplayer.csproj"
    if (Test-Path $csprojPath) {
        $csprojContent = Get-Content $csprojPath -Raw
        
        # 更新版本号
        $csprojContent = $csprojContent -replace '<Version>[\d\.]+</Version>', "<Version>$Version</Version>"
        $csprojContent = $csprojContent -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
        $csprojContent = $csprojContent -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
        
        # 如果没有版本标签，添加它们
        if ($csprojContent -notmatch '<Version>') {
            $propertyGroupEnd = $csprojContent.IndexOf('</PropertyGroup>')
            if ($propertyGroupEnd -gt 0) {
                $versionTags = @"
    <Version>$Version</Version>
    <AssemblyVersion>$Version.0</AssemblyVersion>
    <FileVersion>$Version.0</FileVersion>
  </PropertyGroup>"@
                $csprojContent = $csprojContent.Remove($propertyGroupEnd, '</PropertyGroup>'.Length)
                $csprojContent = $csprojContent.Insert($propertyGroupEnd, $versionTags)
            }
        }
        
        $csprojContent | Out-File $csprojPath -Encoding utf8
        Write-Host "已更新版本号到: $Version" -ForegroundColor Green
    } else {
        Write-Host "警告: 未找到项目文件 $csprojPath" -ForegroundColor Yellow
    }
    
    # 步骤 2: 运行测试（可选）
    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "步骤 2: 运行测试..." -ForegroundColor Yellow
        # 如果有测试项目，在这里运行
        # dotnet test
        Write-Host "跳过测试（暂无测试项目）" -ForegroundColor DarkGray
    }
    
    # 步骤 3: 构建项目
    if (-not $SkipBuild) {
        Write-Host ""
        Write-Host "步骤 3: 构建项目..." -ForegroundColor Yellow
        & "$scriptDir\build.ps1" -Configuration $Configuration -Runtime $Runtime
        if ($LASTEXITCODE -ne 0) {
            throw "构建失败"
        }
    } else {
        Write-Host ""
        Write-Host "跳过构建步骤" -ForegroundColor DarkGray
    }
    
    # 步骤 4: 打包发布文件
    Write-Host ""
    Write-Host "步骤 4: 打包发布文件..." -ForegroundColor Yellow
    & "$scriptDir\package.ps1" -Version $Version
    if ($LASTEXITCODE -ne 0) {
        throw "打包失败"
    }
    
    # 步骤 5: 创建 CHANGELOG 条目
    Write-Host ""
    Write-Host "步骤 5: 准备更新日志..." -ForegroundColor Yellow
    $changelogPath = "CHANGELOG.md"
    $changelogTemplate = @"
# 更新日志

## [v$Version] - $(Get-Date -Format "yyyy-MM-dd")

### 新增功能
- 

### 改进
- 

### 修复
- 

### 其他
- 

---

"@
    
    if (-not (Test-Path $changelogPath)) {
        $changelogTemplate | Out-File $changelogPath -Encoding utf8
        Write-Host "已创建 CHANGELOG.md 模板" -ForegroundColor Green
    } else {
        Write-Host "CHANGELOG.md 已存在，请手动更新" -ForegroundColor Yellow
    }
    
    # 步骤 6: 创建 Git 标签（可选）
    if ($CreateTag) {
        Write-Host ""
        Write-Host "步骤 6: 创建 Git 标签..." -ForegroundColor Yellow
        
        # 检查是否有未提交的更改
        $gitStatus = git status --porcelain
        if ($gitStatus) {
            Write-Host "警告: 存在未提交的更改" -ForegroundColor Yellow
            Write-Host "请先提交更改再创建标签" -ForegroundColor Yellow
        } else {
            $tagName = "v$Version"
            git tag -a $tagName -m "Release version $Version"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "已创建 Git 标签: $tagName" -ForegroundColor Green
                Write-Host "使用 'git push origin $tagName' 推送标签到远程仓库" -ForegroundColor Cyan
            } else {
                Write-Host "创建 Git 标签失败" -ForegroundColor Red
            }
        }
    }
    
    # 生成发布摘要
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "发布准备完成!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "版本: v$Version" -ForegroundColor Cyan
    Write-Host "发布文件位置: release\" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "发布文件列表:" -ForegroundColor Yellow
    Get-ChildItem "release" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "下一步操作:" -ForegroundColor Yellow
    Write-Host "1. 更新 CHANGELOG.md 文件" -ForegroundColor Gray
    Write-Host "2. 提交所有更改到 Git" -ForegroundColor Gray
    Write-Host "3. 推送代码和标签到 GitHub" -ForegroundColor Gray
    Write-Host "4. 在 GitHub 上创建 Release" -ForegroundColor Gray
    Write-Host "5. 上传 release/ 目录中的文件作为 Release 资产" -ForegroundColor Gray
    Write-Host ""
    
    # 生成 GitHub Release 草稿内容
    $releaseDraftPath = "release\RELEASE_DRAFT.md"
    $releaseDraft = @"
# CTWebPlayer v$Version

发布日期: $(Get-Date -Format "yyyy-MM-dd")

## 下载

- [CTWebPlayer-v$Version-win-x64.zip](../../releases/download/v$Version/CTWebPlayer-v$Version-win-x64.zip) - Windows x64 版本

## 系统要求

- Windows 10 或更高版本 (64位)
- Microsoft Edge WebView2 运行时

## 更新内容

### 新增功能
- 

### 改进
- 

### 修复
- 

## 安装说明

1. 下载 ZIP 文件
2. 解压到任意目录
3. 运行 ctwebplayer.exe
4. 程序会自动检查并提示安装 WebView2 运行时（如需要）

## 文件校验

请查看 `CTWebPlayer-v$Version-checksums.txt` 文件以验证下载文件的完整性。

## 许可证

本软件基于 BSD 3-Clause 许可证发布。
"@
    
    $releaseDraft | Out-File $releaseDraftPath -Encoding utf8
    Write-Host "已生成 GitHub Release 草稿: $releaseDraftPath" -ForegroundColor Green
    
    # 步骤 7: 创建 GitHub Release（可选）
    if ($CreateGitHubRelease) {
        Write-Host ""
        Write-Host "步骤 7: 创建 GitHub Release..." -ForegroundColor Yellow
        
        if (-not $GitHubToken) {
            Write-Host "错误: 未提供 GitHub Token" -ForegroundColor Red
            Write-Host "请设置 GITHUB_TOKEN 环境变量或使用 -GitHubToken 参数" -ForegroundColor Yellow
        } else {
            # GitHub API 设置
            $headers = @{
                "Authorization" = "Bearer $GitHubToken"
                "Accept" = "application/vnd.github.v3+json"
            }
            $repoOwner = "a11s"
            $repoName = "ctwebplayer"
            $apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases"
            
            try {
                # 读取 Release 草稿内容
                $releaseDraftPath = "release\RELEASE_DRAFT.md"
                if (Test-Path $releaseDraftPath) {
                    $releaseBody = Get-Content $releaseDraftPath -Raw
                } else {
                    $releaseBody = "CTWebPlayer v$Version - 自动发布"
                }
                
                # 创建 Release
                $releaseData = @{
                    tag_name = "v$Version"
                    target_commitish = "main"
                    name = "CTWebPlayer v$Version"
                    body = $releaseBody
                    draft = $false
                    prerelease = $false
                } | ConvertTo-Json
                
                Write-Host "创建 GitHub Release..." -ForegroundColor Cyan
                $release = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $releaseData -ContentType "application/json"
                $releaseId = $release.id
                $uploadUrl = $release.upload_url -replace '\{.*\}', ''
                
                Write-Host "Release 创建成功: $($release.html_url)" -ForegroundColor Green
                
                # 上传发布文件
                Write-Host ""
                Write-Host "上传发布文件..." -ForegroundColor Yellow
                
                $releaseFiles = Get-ChildItem "release" -File | Where-Object {
                    $_.Extension -in @('.zip', '.txt') -and $_.Name -ne 'RELEASE_DRAFT.md' -and $_.Name -ne 'release-info.json'
                }
                
                foreach ($file in $releaseFiles) {
                    Write-Host "上传: $($file.Name)..." -ForegroundColor Cyan
                    
                    # 确定内容类型
                    $contentType = switch ($file.Extension) {
                        '.zip' { 'application/zip' }
                        '.txt' { 'text/plain' }
                        default { 'application/octet-stream' }
                    }
                    
                    $uploadHeaders = @{
                        "Authorization" = "Bearer $GitHubToken"
                        "Accept" = "application/vnd.github.v3+json"
                        "Content-Type" = $contentType
                    }
                    
                    $uploadUri = "${uploadUrl}?name=$($file.Name)"
                    $fileBytes = [System.IO.File]::ReadAllBytes($file.FullName)
                    
                    try {
                        $asset = Invoke-RestMethod -Uri $uploadUri -Method Post -Headers $uploadHeaders -Body $fileBytes
                        Write-Host "✓ 已上传: $($file.Name)" -ForegroundColor Green
                    } catch {
                        Write-Host "✗ 上传失败: $($file.Name) - $_" -ForegroundColor Red
                    }
                }
                
                Write-Host ""
                Write-Host "GitHub Release 发布完成！" -ForegroundColor Green
                Write-Host "查看 Release: $($release.html_url)" -ForegroundColor Cyan
                
            } catch {
                Write-Host "创建 GitHub Release 失败: $_" -ForegroundColor Red
                Write-Host "请手动在 GitHub 上创建 Release" -ForegroundColor Yellow
            }
        }
    }
    
    # 显示完成信息
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "🎉 所有步骤完成！" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    
} finally {
    Pop-Location
}
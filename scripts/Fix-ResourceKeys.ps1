<#
.SYNOPSIS
    Fix resource keys by adding missing keys and converting dot to underscore
.DESCRIPTION
    This script adds missing UpdateManager keys to all resx files and converts dot-connected keys to underscore in code
.PARAMETER ProjectPath
    Project root directory path
.PARAMETER WhatIf
    Preview mode - shows what would be changed without making changes
#>
param(
    [string]$ProjectPath = (Get-Item $PSScriptRoot).Parent.FullName,
    [switch]$WhatIf
)

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "=== Resource Key Fixer ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "`n[PREVIEW MODE] No files will be modified" -ForegroundColor Magenta
}

# Missing UpdateManager keys with default English values
$missingKeys = @{
    "UpdateManager_CheckingUpdate" = @{
        "en" = "Checking for updates..."
        "zh-CN" = "正在检查更新..."
        "zh-TW" = "正在檢查更新..."
        "ja" = "アップデートを確認中..."
        "ko" = "업데이트 확인 중..."
    }
    "UpdateManager_LatestVersionInfo" = @{
        "en" = "Latest version: {0}"
        "zh-CN" = "最新版本：{0}"
        "zh-TW" = "最新版本：{0}"
        "ja" = "最新バージョン：{0}"
        "ko" = "최신 버전: {0}"
    }
    "UpdateManager_NewVersionFound" = @{
        "en" = "New version found: {0} (current: {1})"
        "zh-CN" = "发现新版本：{0}（当前：{1}）"
        "zh-TW" = "發現新版本：{0}（目前：{1}）"
        "ja" = "新しいバージョンが見つかりました：{0}（現在：{1}）"
        "ko" = "새 버전 발견: {0} (현재: {1})"
    }
    "UpdateManager_CurrentVersion" = @{
        "en" = "You are using the latest version."
        "zh-CN" = "您正在使用最新版本。"
        "zh-TW" = "您正在使用最新版本。"
        "ja" = "最新バージョンを使用しています。"
        "ko" = "최신 버전을 사용하고 있습니다."
    }
    "UpdateManager_CheckFailed" = @{
        "en" = "Update check failed: {0}"
        "zh-CN" = "检查更新失败：{0}"
        "zh-TW" = "檢查更新失敗：{0}"
        "ja" = "アップデートの確認に失敗しました：{0}"
        "ko" = "업데이트 확인 실패: {0}"
    }
    "UpdateManager_DownloadingFile" = @{
        "en" = "Downloading: {0}"
        "zh-CN" = "正在下载：{0}"
        "zh-TW" = "正在下載：{0}"
        "ja" = "ダウンロード中：{0}"
        "ko" = "다운로드 중: {0}"
    }
    "UpdateManager_DownloadCompleteVerification" = @{
        "en" = "Download complete, verifying..."
        "zh-CN" = "下载完成，正在验证..."
        "zh-TW" = "下載完成，正在驗證..."
        "ja" = "ダウンロード完了、検証中..."
        "ko" = "다운로드 완료, 검증 중..."
    }
    "UpdateManager_VerificationPassed" = @{
        "en" = "Verification passed."
        "zh-CN" = "验证通过。"
        "zh-TW" = "驗證通過。"
        "ja" = "検証に成功しました。"
        "ko" = "검증 통과."
    }
    "UpdateManager_NoHashSkipVerification" = @{
        "en" = "No hash provided, skipping verification."
        "zh-CN" = "未提供哈希值，跳过验证。"
        "zh-TW" = "未提供雜湊值，略過驗證。"
        "ja" = "ハッシュが提供されていません、検証をスキップします。"
        "ko" = "해시가 제공되지 않음, 검증 건너뜀."
    }
    "UpdateManager_DownloadFailed" = @{
        "en" = "Download failed: {0}"
        "zh-CN" = "下载失败：{0}"
        "zh-TW" = "下載失敗：{0}"
        "ja" = "ダウンロードに失敗しました：{0}"
        "ko" = "다운로드 실패: {0}"
    }
    "UpdateManager_DownloadCancelled" = @{
        "en" = "Download cancelled."
        "zh-CN" = "下载已取消。"
        "zh-TW" = "下載已取消。"
        "ja" = "ダウンロードがキャンセルされました。"
        "ko" = "다운로드 취소됨."
    }
    "UpdateManager_ComputedHash" = @{
        "en" = "Computed hash: {0}"
        "zh-CN" = "计算的哈希值：{0}"
        "zh-TW" = "計算的雜湊值：{0}"
        "ja" = "計算されたハッシュ：{0}"
        "ko" = "계산된 해시: {0}"
    }
    "UpdateManager_ExpectedHash" = @{
        "en" = "Expected hash: {0}"
        "zh-CN" = "预期的哈希值：{0}"
        "zh-TW" = "預期的雜湊值：{0}"
        "ja" = "期待されるハッシュ：{0}"
        "ko" = "예상 해시: {0}"
    }
    "UpdateManager_HashCalculationFailed" = @{
        "en" = "Hash calculation failed: {0}"
        "zh-CN" = "哈希计算失败：{0}"
        "zh-TW" = "雜湊計算失敗：{0}"
        "ja" = "ハッシュ計算に失敗しました：{0}"
        "ko" = "해시 계산 실패: {0}"
    }
    "UpdateManager_ApplyingUpdate" = @{
        "en" = "Applying update..."
        "zh-CN" = "正在应用更新..."
        "zh-TW" = "正在套用更新..."
        "ja" = "アップデートを適用中..."
        "ko" = "업데이트 적용 중..."
    }
    "UpdateManager_BatchStarted" = @{
        "en" = "Update batch file started."
        "zh-CN" = "更新批处理文件已启动。"
        "zh-TW" = "更新批次檔案已啟動。"
        "ja" = "アップデートバッチファイルが開始されました。"
        "ko" = "업데이트 배치 파일 시작됨."
    }
    "UpdateManager_ApplyFailed" = @{
        "en" = "Failed to apply update: {0}"
        "zh-CN" = "应用更新失败：{0}"
        "zh-TW" = "套用更新失敗：{0}"
        "ja" = "アップデートの適用に失敗しました：{0}"
        "ko" = "업데이트 적용 실패: {0}"
    }
}

# Key mappings for code conversion
$codeKeyMappings = @{
    # UpdateManager keys (dot to underscore)
    "UpdateManager.CheckingUpdate" = "UpdateManager_CheckingUpdate"
    "UpdateManager.LatestVersionInfo" = "UpdateManager_LatestVersionInfo"
    "UpdateManager.NewVersionFound" = "UpdateManager_NewVersionFound"
    "UpdateManager.CurrentVersion" = "UpdateManager_CurrentVersion"
    "UpdateManager.CheckFailed" = "UpdateManager_CheckFailed"
    "UpdateManager.DownloadingFile" = "UpdateManager_DownloadingFile"
    "UpdateManager.DownloadCompleteVerification" = "UpdateManager_DownloadCompleteVerification"
    "UpdateManager.VerificationPassed" = "UpdateManager_VerificationPassed"
    "UpdateManager.NoHashSkipVerification" = "UpdateManager_NoHashSkipVerification"
    "UpdateManager.DownloadFailed" = "UpdateManager_DownloadFailed"
    "UpdateManager.DownloadCancelled" = "UpdateManager_DownloadCancelled"
    "UpdateManager.ComputedHash" = "UpdateManager_ComputedHash"
    "UpdateManager.ExpectedHash" = "UpdateManager_ExpectedHash"
    "UpdateManager.HashCalculationFailed" = "UpdateManager_HashCalculationFailed"
    "UpdateManager.ApplyingUpdate" = "UpdateManager_ApplyingUpdate"
    "UpdateManager.BatchStarted" = "UpdateManager_BatchStarted"
    "UpdateManager.ApplyFailed" = "UpdateManager_ApplyFailed"
    
    # UpdateForm keys need to be mapped to existing keys
    "UpdateForm.CurrentVersionLabel" = "UpdateForm_grpVersionInfo_lblCurrentVersion"
    "UpdateForm.NewVersionLabel" = "UpdateForm_grpVersionInfo_lblNewVersion"
    "UpdateForm.ReleaseDateLabel" = "UpdateForm_grpVersionInfo_lblReleaseDate"
    "UpdateForm.FileNameLabel" = "UpdateForm_grpFileInfo_lblFileName"
    "UpdateForm.FileSizeLabel" = "UpdateForm_grpFileInfo_lblFileSize"
    "UpdateForm.ProgressPreparing" = "UpdateForm_lblProgress_Ready"
    "UpdateForm.DownloadButton" = "UpdateForm_btnDownload"
    "UpdateForm.DownloadCancelled" = "UpdateForm_lblProgress_Cancelled"
    
    # LogViewerForm keys (already underscore in resx)
    "LogViewerForm.FileInfo" = "LogViewerForm_FileInfo"
    "LogViewerForm.ClearConfirmation" = "LogViewerForm_ClearConfirmation"
    "LogViewerForm.ClearConfirmationTitle" = "LogViewerForm_ClearConfirmationTitle"
    "LogViewerForm.ClearSuccess" = "LogViewerForm_ClearSuccess"
    "LogViewerForm.SuccessTitle" = "LogViewerForm_SuccessTitle"
    "LogViewerForm.ClearError" = "LogViewerForm_ClearError"
    "LogViewerForm.ErrorTitle" = "LogViewerForm_ErrorTitle"
}

# Create backup directory
$backupDir = Join-Path $ProjectPath "backup\fix-keys-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "`nBackup directory: $backupDir" -ForegroundColor Green
}

# 1. Add missing keys to resx files
Write-Host "`n=== Adding Missing Keys to Resource Files ===" -ForegroundColor Cyan
$resxFiles = @{
    "src\Resources\Strings.resx" = "en"
    "src\Resources\Strings.zh-CN.resx" = "zh-CN"
    "src\Resources\Strings.zh-TW.resx" = "zh-TW"
    "src\Resources\Strings.ja.resx" = "ja"
    "src\Resources\Strings.ko.resx" = "ko"
}

foreach ($file in $resxFiles.Keys) {
    $filePath = Join-Path $ProjectPath $file
    $language = $resxFiles[$file]
    
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $file" -ForegroundColor Yellow
        
        # Load XML
        $xml = New-Object System.Xml.XmlDocument
        $xml.PreserveWhitespace = $true
        $xml.Load($filePath)
        
        $addedCount = 0
        
        # Add missing keys
        foreach ($key in $missingKeys.Keys) {
            # Check if key already exists
            $existingNode = $xml.SelectSingleNode("//data[@name='$key']")
            if (-not $existingNode) {
                if (-not $WhatIf) {
                    # Create new data node
                    $dataNode = $xml.CreateElement("data")
                    $dataNode.SetAttribute("name", $key)
                    $dataNode.SetAttribute("xml:space", "preserve")
                    
                    $valueNode = $xml.CreateElement("value")
                    $valueNode.InnerText = $missingKeys[$key][$language]
                    $dataNode.AppendChild($valueNode)
                    
                    # Add before the last closing tag
                    $rootNode = $xml.SelectSingleNode("/root")
                    $rootNode.InsertBefore($dataNode, $rootNode.LastChild)
                }
                Write-Host "  + Adding: $key" -ForegroundColor Green
                $addedCount++
            }
        }
        
        if ($addedCount -gt 0) {
            if (-not $WhatIf) {
                # Backup original file
                $backupPath = Join-Path $backupDir $file
                $backupFileDir = Split-Path $backupPath -Parent
                New-Item -ItemType Directory -Path $backupFileDir -Force | Out-Null
                Copy-Item $filePath $backupPath
                
                # Save modified XML
                $settings = New-Object System.Xml.XmlWriterSettings
                $settings.Encoding = [System.Text.Encoding]::UTF8
                $settings.Indent = $true
                $settings.IndentChars = "  "
                $settings.NewLineChars = "`r`n"
                $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
                
                $writer = [System.Xml.XmlWriter]::Create($filePath, $settings)
                $xml.Save($writer)
                $writer.Close()
                
                Write-Host "  File updated (Added $addedCount keys)" -ForegroundColor Green
            } else {
                Write-Host "  [PREVIEW] Would add $addedCount keys" -ForegroundColor Magenta
            }
        } else {
            Write-Host "  No missing keys to add" -ForegroundColor Gray
        }
    }
}

# 2. Modify code files
Write-Host "`n=== Converting Dot to Underscore in Code Files ===" -ForegroundColor Cyan
$codeFiles = @(
    "src\UpdateManager.cs",
    "src\UpdateForm.cs", 
    "src\LogViewerForm.cs"
)

foreach ($file in $codeFiles) {
    $filePath = Join-Path $ProjectPath $file
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $file" -ForegroundColor Yellow
        
        # Read file content
        $content = Get-Content $filePath -Raw -Encoding UTF8
        $originalContent = $content
        $changeCount = 0
        
        # Replace each key
        foreach ($oldKey in $codeKeyMappings.Keys) {
            $newKey = $codeKeyMappings[$oldKey]
            $pattern = [regex]::Escape("GetString(`"$oldKey`")")
            $replacement = "GetString(`"$newKey`")"
            
            $matches = [regex]::Matches($content, $pattern)
            if ($matches.Count -gt 0) {
                Write-Host "  - Replace: `"$oldKey`" -> `"$newKey`" ($($matches.Count) occurrences)" -ForegroundColor Green
                $content = $content -replace $pattern, $replacement
                $changeCount += $matches.Count
            }
        }
        
        if ($changeCount -gt 0) {
            if (-not $WhatIf) {
                # Backup original file
                $backupPath = Join-Path $backupDir $file
                $backupFileDir = Split-Path $backupPath -Parent
                New-Item -ItemType Directory -Path $backupFileDir -Force | Out-Null
                Copy-Item $filePath $backupPath
                
                # Save modified content
                [System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
                Write-Host "  File updated (Total $changeCount changes)" -ForegroundColor Green
            } else {
                Write-Host "  [PREVIEW] Would modify $changeCount occurrences" -ForegroundColor Magenta
            }
        } else {
            Write-Host "  No changes needed" -ForegroundColor Gray
        }
    }
}

# Generate report
$reportPath = Join-Path $ProjectPath "scripts\fix-keys-report.txt"
$report = @"
Resource Key Fix Report
Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Project Path: $ProjectPath

Added Missing Keys:
$($missingKeys.Keys -join "`r`n  ")

Key Mappings Applied:
$($codeKeyMappings.GetEnumerator() | ForEach-Object { "  $($_.Key) -> $($_.Value)" } | Out-String)

Modified Resource Files:
$($resxFiles.Keys -join "`r`n  ")

Modified Code Files:
$($codeFiles -join "`r`n  ")
"@

if (-not $WhatIf) {
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`nFix report saved to: $reportPath" -ForegroundColor Green
    Write-Host "Backup files saved in: $backupDir" -ForegroundColor Green
    Write-Host "`nResource key fix completed!" -ForegroundColor Cyan
} else {
    Write-Host "`n[PREVIEW MODE] Report would be saved to: $reportPath" -ForegroundColor Magenta
}
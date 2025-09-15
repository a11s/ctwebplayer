<#
.SYNOPSIS
    Convert dot (.) to underscore (_) in resource keys
.DESCRIPTION
    This script unifies resource key naming style by converting all dot-connected keys to underscore-connected ones
    It modifies both code files and all language resx files
.PARAMETER ProjectPath
    Project root directory path, defaults to parent directory of script location
.PARAMETER WhatIf
    If specified, only shows what would be changed without actually making changes
.EXAMPLE
    .\Convert-DotToUnderscore.ps1
    Execute the conversion
.EXAMPLE
    .\Convert-DotToUnderscore.ps1 -WhatIf
    Preview changes that would be made
#>
param(
    [string]$ProjectPath = (Get-Item $PSScriptRoot).Parent.FullName,
    [switch]$WhatIf
)

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Key mappings to convert
$keyMappings = @{
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
    "UpdateForm.CurrentVersionLabel" = "UpdateForm_CurrentVersionLabel"
    "UpdateForm.NewVersionLabel" = "UpdateForm_NewVersionLabel"
    "UpdateForm.ReleaseDateLabel" = "UpdateForm_ReleaseDateLabel"
    "UpdateForm.FileNameLabel" = "UpdateForm_FileNameLabel"
    "UpdateForm.FileSizeLabel" = "UpdateForm_FileSizeLabel"
    "UpdateForm.ProgressPreparing" = "UpdateForm_ProgressPreparing"
    "UpdateForm.DownloadButton" = "UpdateForm_DownloadButton"
    "UpdateForm.DownloadCancelled" = "UpdateForm_DownloadCancelled"
    "LogViewerForm.FileInfo" = "LogViewerForm_FileInfo"
    "LogViewerForm.ClearConfirmation" = "LogViewerForm_ClearConfirmation"
    "LogViewerForm.ClearConfirmationTitle" = "LogViewerForm_ClearConfirmationTitle"
    "LogViewerForm.ClearSuccess" = "LogViewerForm_ClearSuccess"
    "LogViewerForm.SuccessTitle" = "LogViewerForm_SuccessTitle"
    "LogViewerForm.ClearError" = "LogViewerForm_ClearError"
    "LogViewerForm.ErrorTitle" = "LogViewerForm_ErrorTitle"
}

Write-Host "=== Resource Key Dot to Underscore Converter ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "`n[PREVIEW MODE] No files will be modified" -ForegroundColor Magenta
}

# Create backup directory
$backupDir = Join-Path $ProjectPath "backup\dot-to-underscore-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "`nBackup directory: $backupDir" -ForegroundColor Green
}

# 1. Modify code files
Write-Host "`n=== Modifying Code Files ===" -ForegroundColor Cyan
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
        foreach ($oldKey in $keyMappings.Keys) {
            $newKey = $keyMappings[$oldKey]
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

# 2. Modify resx files
Write-Host "`n=== Modifying Resource Files ===" -ForegroundColor Cyan
$resxFiles = @(
    "src\Resources\Strings.resx",
    "src\Resources\Strings.zh-CN.resx",
    "src\Resources\Strings.zh-TW.resx",
    "src\Resources\Strings.ja.resx",
    "src\Resources\Strings.ko.resx"
)

foreach ($file in $resxFiles) {
    $filePath = Join-Path $ProjectPath $file
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $file" -ForegroundColor Yellow
        
        # Load XML
        $xml = New-Object System.Xml.XmlDocument
        $xml.PreserveWhitespace = $true
        $xml.Load($filePath)
        
        $changeCount = 0
        
        # Find all data nodes
        $dataNodes = $xml.SelectNodes("//data")
        foreach ($node in $dataNodes) {
            $currentName = $node.GetAttribute("name")
            
            # Check if replacement needed
            if ($keyMappings.ContainsKey($currentName)) {
                $newName = $keyMappings[$currentName]
                if (-not $WhatIf) {
                    $node.SetAttribute("name", $newName)
                }
                Write-Host "  - Replace: `"$currentName`" -> `"$newName`"" -ForegroundColor Green
                $changeCount++
            }
        }
        
        if ($changeCount -gt 0) {
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
                
                Write-Host "  File updated (Total $changeCount keys)" -ForegroundColor Green
            } else {
                Write-Host "  [PREVIEW] Would modify $changeCount keys" -ForegroundColor Magenta
            }
        } else {
            Write-Host "  No changes needed" -ForegroundColor Gray
        }
    }
}

# Generate report
$reportPath = Join-Path $ProjectPath "scripts\dot-to-underscore-report.txt"
$report = @"
Resource Key Dot to Underscore Conversion Report
Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Project Path: $ProjectPath

Converted Keys:
$($keyMappings.GetEnumerator() | ForEach-Object { "  $($_.Key) -> $($_.Value)" } | Out-String)

Modified Code Files:
$($codeFiles -join "`r`n  ")

Modified Resource Files:
$($resxFiles -join "`r`n  ")
"@

if (-not $WhatIf) {
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`nConversion report saved to: $reportPath" -ForegroundColor Green
    Write-Host "Backup files saved in: $backupDir" -ForegroundColor Green
    Write-Host "`nConversion completed!" -ForegroundColor Cyan
} else {
    Write-Host "`n[PREVIEW MODE] Report would be saved to: $reportPath" -ForegroundColor Magenta
}
<#
.SYNOPSIS
    Add missing UpdateManager resource keys to all resx files
.DESCRIPTION
    This script adds the missing UpdateManager keys to all language resx files
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

Write-Host "=== Add Missing Resource Keys ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "`n[PREVIEW MODE] No files will be modified" -ForegroundColor Magenta
}

# Resource files and their languages
$resxFiles = @{
    "src\Resources\Strings.resx" = "en"
    "src\Resources\Strings.zh-CN.resx" = "zh-CN"
    "src\Resources\Strings.zh-TW.resx" = "zh-TW"
    "src\Resources\Strings.ja.resx" = "ja"
    "src\Resources\Strings.ko.resx" = "ko"
}

# Missing keys to add (English only for now, to be translated later)
$missingKeys = @{
    "UpdateManager_CheckingUpdate" = "Checking for updates..."
    "UpdateManager_LatestVersionInfo" = "Latest version: {0}"
    "UpdateManager_NewVersionFound" = "New version found: {0} (current: {1})"
    "UpdateManager_CurrentVersion" = "You are using the latest version."
    "UpdateManager_CheckFailed" = "Update check failed: {0}"
    "UpdateManager_DownloadingFile" = "Downloading: {0}"
    "UpdateManager_DownloadCompleteVerification" = "Download complete, verifying..."
    "UpdateManager_VerificationPassed" = "Verification passed."
    "UpdateManager_NoHashSkipVerification" = "No hash provided, skipping verification."
    "UpdateManager_DownloadFailed" = "Download failed: {0}"
    "UpdateManager_DownloadCancelled" = "Download cancelled."
    "UpdateManager_ComputedHash" = "Computed hash: {0}"
    "UpdateManager_ExpectedHash" = "Expected hash: {0}"
    "UpdateManager_HashCalculationFailed" = "Hash calculation failed: {0}"
    "UpdateManager_ApplyingUpdate" = "Applying update..."
    "UpdateManager_BatchStarted" = "Update batch file started."
    "UpdateManager_ApplyFailed" = "Failed to apply update: {0}"
}

# Create backup directory
$backupDir = Join-Path $ProjectPath "backup\add-missing-keys-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "`nBackup directory: $backupDir" -ForegroundColor Green
}

Write-Host "`n=== Adding Missing Keys to Resource Files ===" -ForegroundColor Cyan

foreach ($file in $resxFiles.Keys) {
    $filePath = Join-Path $ProjectPath $file
    $language = $resxFiles[$file]
    
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $file" -ForegroundColor Yellow
        
        # Load XML
        $xml = New-Object System.Xml.XmlDocument
        $xml.PreserveWhitespace = $true
        
        # Load with UTF-8 encoding
        $reader = New-Object System.IO.StreamReader($filePath, [System.Text.Encoding]::UTF8)
        $xml.Load($reader)
        $reader.Close()
        
        $addedCount = 0
        
        # Find the position to insert new keys (before the last closing tag)
        $rootNode = $xml.SelectSingleNode("/root")
        $lastNode = $rootNode.LastChild
        
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
                    # For now, use English value for all languages
                    # TODO: Add proper translations
                    $valueNode.InnerText = $missingKeys[$key]
                    $dataNode.AppendChild($valueNode)
                    
                    # Add some whitespace for formatting
                    $whitespace = $xml.CreateWhitespace("`r`n  ")
                    $rootNode.InsertBefore($whitespace, $lastNode)
                    $rootNode.InsertBefore($dataNode, $lastNode)
                }
                Write-Host "  + Adding: $key" -ForegroundColor Green
                $addedCount++
            } else {
                Write-Host "  - Already exists: $key" -ForegroundColor Gray
            }
        }
        
        if ($addedCount -gt 0) {
            if (-not $WhatIf) {
                # Backup original file
                $backupPath = Join-Path $backupDir $file
                $backupFileDir = Split-Path $backupPath -Parent
                New-Item -ItemType Directory -Path $backupFileDir -Force | Out-Null
                Copy-Item $filePath $backupPath
                
                # Save modified XML with proper encoding
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
            Write-Host "  No keys to add" -ForegroundColor Gray
        }
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Processed ${resxFiles.Count} resource files" -ForegroundColor Yellow

if ($language -ne "en") {
    Write-Host "`nNOTE: Keys have been added with English values." -ForegroundColor Magenta
    Write-Host "Please translate these keys for each language:" -ForegroundColor Magenta
    Write-Host "  - Chinese Simplified (zh-CN)" -ForegroundColor Yellow
    Write-Host "  - Chinese Traditional (zh-TW)" -ForegroundColor Yellow
    Write-Host "  - Japanese (ja)" -ForegroundColor Yellow
    Write-Host "  - Korean (ko)" -ForegroundColor Yellow
}

if (-not $WhatIf) {
    Write-Host "`nBackup files saved in: $backupDir" -ForegroundColor Green
    Write-Host "Keys added successfully!" -ForegroundColor Cyan
} else {
    Write-Host "`n[PREVIEW MODE] Run without -WhatIf to apply changes" -ForegroundColor Magenta
}
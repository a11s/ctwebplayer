<#
.SYNOPSIS
    Fix UpdateForm key mappings in code files
.DESCRIPTION
    This script maps UpdateForm keys in code to their correct counterparts in resx files
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

Write-Host "=== Fix UpdateForm Key Mapping ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "`n[PREVIEW MODE] No files will be modified" -ForegroundColor Magenta
}

# UpdateForm key mappings (from code to resx)
$keyMappings = @{
    "UpdateForm_CurrentVersionLabel" = "UpdateForm_grpVersionInfo_lblCurrentVersion"
    "UpdateForm_NewVersionLabel" = "UpdateForm_grpVersionInfo_lblNewVersion"
    "UpdateForm_ReleaseDateLabel" = "UpdateForm_grpVersionInfo_lblReleaseDate"
    "UpdateForm_FileNameLabel" = "UpdateForm_grpFileInfo_lblFileName"
    "UpdateForm_FileSizeLabel" = "UpdateForm_grpFileInfo_lblFileSize"
    "UpdateForm_ProgressPreparing" = "UpdateForm_lblProgress_Ready"
    "UpdateForm_DownloadButton" = "UpdateForm_btnDownload"
    "UpdateForm_DownloadCancelled" = "UpdateForm_lblProgress_Cancelled"
}

# Create backup directory
$backupDir = Join-Path $ProjectPath "backup\fix-updateform-mapping-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "`nBackup directory: $backupDir" -ForegroundColor Green
}

Write-Host "`n=== Fixing UpdateForm Key Mappings ===" -ForegroundColor Cyan

# Only UpdateForm.cs needs to be modified
$filePath = Join-Path $ProjectPath "src\UpdateForm.cs"
if (Test-Path $filePath) {
    Write-Host "`nProcessing: src\UpdateForm.cs" -ForegroundColor Yellow
    
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
            $backupPath = Join-Path $backupDir "src\UpdateForm.cs"
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
} else {
    Write-Host "  ERROR: File not found!" -ForegroundColor Red
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "UpdateForm key mappings have been fixed to match the existing resx keys." -ForegroundColor Yellow

if (-not $WhatIf) {
    Write-Host "`nBackup files saved in: $backupDir" -ForegroundColor Green
    Write-Host "Mapping fix completed!" -ForegroundColor Cyan
} else {
    Write-Host "`n[PREVIEW MODE] Run without -WhatIf to apply changes" -ForegroundColor Magenta
}
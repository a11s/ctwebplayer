<#
.SYNOPSIS
    Convert dot (.) to underscore (_) in resource keys in code files
.DESCRIPTION
    This script converts dot-connected resource keys to underscore-connected ones in code files
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

Write-Host "=== Simple Dot to Underscore Converter ===" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($WhatIf) {
    Write-Host "`n[PREVIEW MODE] No files will be modified" -ForegroundColor Magenta
}

# Create backup directory
$backupDir = Join-Path $ProjectPath "backup\simple-dot-to-underscore-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (-not $WhatIf) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "`nBackup directory: $backupDir" -ForegroundColor Green
}

# Code files to process
$codeFiles = @(
    "src\UpdateManager.cs",
    "src\UpdateForm.cs", 
    "src\LogViewerForm.cs"
)

Write-Host "`n=== Converting Dot to Underscore in Code Files ===" -ForegroundColor Cyan

$totalChanges = 0

foreach ($file in $codeFiles) {
    $filePath = Join-Path $ProjectPath $file
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $file" -ForegroundColor Yellow
        
        # Read file content
        $content = Get-Content $filePath -Raw -Encoding UTF8
        $originalContent = $content
        $changeCount = 0
        
        # Find all GetString calls with dot notation
        $pattern = 'GetString\("([a-zA-Z]+)\.([a-zA-Z_\.]+)"\)'
        
        $matches = [regex]::Matches($content, $pattern)
        $uniqueKeys = @{}
        
        foreach ($match in $matches) {
            $fullMatch = $match.Value
            $prefix = $match.Groups[1].Value
            $suffix = $match.Groups[2].Value
            $oldKey = "$prefix.$suffix"
            $newKey = "$prefix`_$($suffix -replace '\.', '_')"
            
            if (-not $uniqueKeys.ContainsKey($oldKey)) {
                $uniqueKeys[$oldKey] = $newKey
            }
        }
        
        # Apply replacements
        foreach ($oldKey in $uniqueKeys.Keys) {
            $newKey = $uniqueKeys[$oldKey]
            $oldPattern = [regex]::Escape("GetString(`"$oldKey`")")
            $newPattern = "GetString(`"$newKey`")"
            
            $occurrences = ([regex]::Matches($content, $oldPattern)).Count
            if ($occurrences -gt 0) {
                Write-Host "  - Replace: `"$oldKey`" -> `"$newKey`" ($occurrences occurrences)" -ForegroundColor Green
                $content = $content -replace $oldPattern, $newPattern
                $changeCount += $occurrences
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
            $totalChanges += $changeCount
        } else {
            Write-Host "  No changes needed" -ForegroundColor Gray
        }
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Total changes: $totalChanges" -ForegroundColor Yellow

# Generate list of keys that need to be added to resx files
Write-Host "`n=== Keys to Add to Resource Files ===" -ForegroundColor Cyan
Write-Host "The following UpdateManager keys need to be added to all resx files:" -ForegroundColor Yellow
Write-Host @"
  UpdateManager_CheckingUpdate
  UpdateManager_LatestVersionInfo
  UpdateManager_NewVersionFound
  UpdateManager_CurrentVersion
  UpdateManager_CheckFailed
  UpdateManager_DownloadingFile
  UpdateManager_DownloadCompleteVerification
  UpdateManager_VerificationPassed
  UpdateManager_NoHashSkipVerification
  UpdateManager_DownloadFailed
  UpdateManager_DownloadCancelled
  UpdateManager_ComputedHash
  UpdateManager_ExpectedHash
  UpdateManager_HashCalculationFailed
  UpdateManager_ApplyingUpdate
  UpdateManager_BatchStarted
  UpdateManager_ApplyFailed
"@ -ForegroundColor Magenta

Write-Host "`nNote: UpdateForm keys need special mapping:" -ForegroundColor Yellow
Write-Host @"
  UpdateForm.CurrentVersionLabel -> UpdateForm_grpVersionInfo_lblCurrentVersion
  UpdateForm.NewVersionLabel -> UpdateForm_grpVersionInfo_lblNewVersion
  UpdateForm.ReleaseDateLabel -> UpdateForm_grpVersionInfo_lblReleaseDate
  UpdateForm.FileNameLabel -> UpdateForm_grpFileInfo_lblFileName
  UpdateForm.FileSizeLabel -> UpdateForm_grpFileInfo_lblFileSize
  UpdateForm.ProgressPreparing -> UpdateForm_lblProgress_Ready
  UpdateForm.DownloadButton -> UpdateForm_btnDownload
  UpdateForm.DownloadCancelled -> UpdateForm_lblProgress_Cancelled
"@ -ForegroundColor Magenta

if (-not $WhatIf) {
    Write-Host "`nBackup files saved in: $backupDir" -ForegroundColor Green
    Write-Host "Conversion completed!" -ForegroundColor Cyan
} else {
    Write-Host "`n[PREVIEW MODE] Run without -WhatIf to apply changes" -ForegroundColor Magenta
}
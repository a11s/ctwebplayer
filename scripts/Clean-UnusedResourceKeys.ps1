<#
.SYNOPSIS
    Automatically clean unused resource keys from resx files

.DESCRIPTION
    This script scans all Designer.cs files and code files to collect used resource keys,
    then compares with keys in resx files to find and optionally delete unused keys.

.PARAMETER Backup
    Backup original files before deletion

.PARAMETER ExcludeKeys
    Exclude certain keys from deletion (supports wildcards)

.PARAMETER ProjectPath
    Project root directory path, defaults to parent directory of current directory

.EXAMPLE
    .\Clean-UnusedResourceKeys.ps1 -WhatIf
    Only shows keys that would be deleted without actual deletion

.EXAMPLE
    .\Clean-UnusedResourceKeys.ps1 -Backup
    Backup original resx files before deletion

.EXAMPLE
    .\Clean-UnusedResourceKeys.ps1 -ExcludeKeys "Message_*", "Common_*"
    Delete unused keys but exclude keys starting with Message_ or Common_
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Backup,
    [string[]]$ExcludeKeys = @(),
    [string]$ProjectPath = (Get-Item $PSScriptRoot).Parent.FullName
)

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Color definitions
$Colors = @{
    Success = 'Green'
    Warning = 'Yellow'
    Error   = 'Red'
    Info    = 'Cyan'
    Header  = 'Magenta'
}

# Log function
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = 'Info'
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = $Colors[$Level]
    
    Write-Host "[$timestamp] " -NoNewline -ForegroundColor DarkGray
    Write-Host $Message -ForegroundColor $color
}

# Progress report function
function Write-ProgressMessage {
    param(
        [string]$Activity,
        [string]$Status,
        [int]$PercentComplete = -1
    )
    
    if ($PercentComplete -ge 0) {
        Write-Progress -Activity $Activity -Status $Status -PercentComplete $PercentComplete
    } else {
        Write-Progress -Activity $Activity -Status $Status
    }
}

# Scan Tag attributes in Designer.cs files
function Get-DesignerResourceKeys {
    param(
        [string]$Path
    )
    
    Write-Log "Scanning resource keys in Designer.cs files..." -Level Info
    
    $designerFiles = Get-ChildItem -Path $Path -Filter "*.Designer.cs" -Recurse -ErrorAction SilentlyContinue
    $keys = @()
    $fileCount = $designerFiles.Count
    $currentFile = 0
    
    foreach ($file in $designerFiles) {
        $currentFile++
        Write-ProgressMessage -Activity "Scanning Designer files" -Status "Processing: $($file.Name)" -PercentComplete (($currentFile / $fileCount) * 100)
        
        try {
            $content = Get-Content $file.FullName -Raw -Encoding UTF8
            
            # Match Tag property settings, support multiple formats
            $tagPattern = '\.Tag\s*=\s*"([^"]+)"'
            $matches = [regex]::Matches($content, $tagPattern)
            
            foreach ($match in $matches) {
                $key = $match.Groups[1].Value
                $keys += $key
                Write-Log "  Found Tag: $key (in $($file.Name))" -Level Info
            }
        }
        catch {
            Write-Log "  Failed to read file: $($file.Name) - $_" -Level Error
        }
    }
    
    Write-Progress -Activity "Scanning Designer files" -Completed
    Write-Log "Designer file scan completed, found $($keys.Count) resource keys" -Level Success
    
    return $keys | Select-Object -Unique
}

# Scan GetString calls in code files
function Get-CodeResourceKeys {
    param(
        [string]$Path
    )
    
    Write-Log "Scanning GetString calls in code files..." -Level Info
    
    $codeFiles = Get-ChildItem -Path $Path -Include "*.cs" -Exclude "*.Designer.cs", "*.g.cs" -Recurse -ErrorAction SilentlyContinue
    $keys = @()
    $fileCount = $codeFiles.Count
    $currentFile = 0
    
    foreach ($file in $codeFiles) {
        $currentFile++
        Write-ProgressMessage -Activity "Scanning code files" -Status "Processing: $($file.Name)" -PercentComplete (($currentFile / $fileCount) * 100)
        
        try {
            $content = Get-Content $file.FullName -Raw -Encoding UTF8
            
            # Match various formats of GetString calls
            $patterns = @(
                'GetString\s*\(\s*"([^"]+)"',           # GetString("key")
                'GetString\s*\(\s*''([^'']+)''',        # GetString('key')
                'GetString\s*\(\s*@"([^"]+)"',          # GetString(@"key")
                '\$"[^"]*\{[^}]*GetString\s*\(\s*"([^"]+)"' # In string interpolation
            )
            
            foreach ($pattern in $patterns) {
                $matches = [regex]::Matches($content, $pattern)
                
                foreach ($match in $matches) {
                    $key = $match.Groups[1].Value
                    $keys += $key
                    Write-Log "  Found GetString: $key (in $($file.Name))" -Level Info
                }
            }
        }
        catch {
            Write-Log "  Failed to read file: $($file.Name) - $_" -Level Error
        }
    }
    
    Write-Progress -Activity "Scanning code files" -Completed
    Write-Log "Code file scan completed, found $($keys.Count) resource key references" -Level Success
    
    return $keys | Select-Object -Unique
}

# Read all keys from resx file
function Get-ResxKeys {
    param(
        [string]$ResxPath
    )
    
    Write-Log "Reading resx file: $ResxPath" -Level Info
    
    try {
        [xml]$resxContent = Get-Content $ResxPath -Raw -Encoding UTF8
        $keys = @()
        
        foreach ($data in $resxContent.root.data) {
            if ($data.name -and -not $data.type) {  # Exclude non-string resources
                $keys += $data.name
            }
        }
        
        Write-Log "  Found $($keys.Count) resource keys" -Level Info
        return $keys
    }
    catch {
        Write-Log "  Failed to read resx file: $_ " -Level Error
        return @()
    }
}

# Backup resx file
function Backup-ResxFile {
    param(
        [string]$FilePath
    )
    
    $backupDir = Join-Path (Split-Path $FilePath -Parent) "backup"
    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $fileName = Split-Path $FilePath -Leaf
    $backupPath = Join-Path $backupDir "$timestamp`_$fileName"
    
    Copy-Item $FilePath $backupPath -Force
    Write-Log "Backed up: $fileName -> $backupPath" -Level Success
    
    return $backupPath
}

# Remove specified keys from resx file
function Remove-ResxKeys {
    param(
        [string]$ResxPath,
        [string[]]$KeysToRemove
    )
    
    try {
        [xml]$resxContent = Get-Content $ResxPath -Raw -Encoding UTF8
        $removedCount = 0
        
        foreach ($key in $KeysToRemove) {
            $nodeToRemove = $resxContent.root.data | Where-Object { $_.name -eq $key }
            if ($nodeToRemove) {
                $nodeToRemove.ParentNode.RemoveChild($nodeToRemove) | Out-Null
                $removedCount++
                Write-Log "  Removed: $key" -Level Warning
            }
        }
        
        if ($removedCount -gt 0) {
            # Save file preserving original format
            $settings = New-Object System.Xml.XmlWriterSettings
            $settings.Indent = $true
            $settings.IndentChars = "  "
            $settings.NewLineChars = "`r`n"
            $settings.Encoding = [System.Text.Encoding]::UTF8
            
            $writer = [System.Xml.XmlWriter]::Create($ResxPath, $settings)
            $resxContent.Save($writer)
            $writer.Close()
            
            Write-Log "Removed $removedCount keys from $(Split-Path $ResxPath -Leaf)" -Level Success
        }
        
        return $removedCount
    }
    catch {
        Write-Log "Error removing keys: $_" -Level Error
        return 0
    }
}

# Main function
function Main {
    Write-Host ""
    Write-Log "===============================================" -Level Header
    Write-Log "       Unused Resource Key Cleanup Tool" -Level Header
    Write-Log "===============================================" -Level Header
    Write-Host ""
    
    # Validate project path
    if (-not (Test-Path $ProjectPath)) {
        Write-Log "Project path does not exist: $ProjectPath" -Level Error
        return
    }
    
    Write-Log "Project path: $ProjectPath" -Level Info
    Write-Host ""
    
    # Step 1: Collect used resource keys
    Write-Log "Step 1/4: Collecting used resource keys" -Level Header
    $designerKeys = Get-DesignerResourceKeys -Path $ProjectPath
    $codeKeys = Get-CodeResourceKeys -Path $ProjectPath
    $usedKeys = ($designerKeys + $codeKeys) | Select-Object -Unique
    
    Write-Log "Total found $($usedKeys.Count) unique used keys" -Level Success
    Write-Host ""
    
    # Step 2: Read all resx files
    Write-Log "Step 2/4: Reading resource files" -Level Header
    $resourcePath = Join-Path $ProjectPath "src\Resources"
    $resxFiles = @(
        "Strings.resx",
        "Strings.zh-CN.resx",
        "Strings.zh-TW.resx",
        "Strings.ja.resx",
        "Strings.ko.resx"
    )
    
    $allResxKeys = @{}
    $totalKeys = @()
    
    foreach ($resxFile in $resxFiles) {
        $resxPath = Join-Path $resourcePath $resxFile
        if (Test-Path $resxPath) {
            $keys = Get-ResxKeys -ResxPath $resxPath
            $allResxKeys[$resxFile] = $keys
            $totalKeys += $keys
        } else {
            Write-Log "  File not found: $resxFile" -Level Warning
        }
    }
    
    $uniqueResxKeys = $totalKeys | Select-Object -Unique
    Write-Log "Resource files contain $($uniqueResxKeys.Count) unique keys in total" -Level Success
    Write-Host ""
    
    # Step 3: Compare and find unused keys
    Write-Log "Step 3/4: Analyzing unused keys" -Level Header
    $unusedKeys = @()
    
    foreach ($key in $uniqueResxKeys) {
        if ($key -notin $usedKeys) {
            # Check if in exclude list
            $excluded = $false
            foreach ($excludePattern in $ExcludeKeys) {
                if ($key -like $excludePattern) {
                    $excluded = $true
                    break
                }
            }
            
            if (-not $excluded) {
                $unusedKeys += $key
            }
        }
    }
    
    if ($unusedKeys.Count -eq 0) {
        Write-Log "No unused resource keys found!" -Level Success
        return
    }
    
    Write-Log "Found $($unusedKeys.Count) unused keys:" -Level Warning
    foreach ($key in $unusedKeys | Sort-Object) {
        Write-Host "  - $key" -ForegroundColor Yellow
    }
    Write-Host ""
    
    # Generate report
    $reportPath = Join-Path $PSScriptRoot "unused-resource-keys-report.txt"
    $report = @"
Unused Resource Key Cleanup Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Project Path: $ProjectPath

Used keys count: $($usedKeys.Count)
Resource file keys count: $($uniqueResxKeys.Count)
Unused keys count: $($unusedKeys.Count)

Unused keys list:
$($unusedKeys | Sort-Object | ForEach-Object { "- $_" } | Out-String)

Distribution of unused keys in each file:
"@
    
    foreach ($resxFile in $resxFiles) {
        if ($allResxKeys.ContainsKey($resxFile)) {
            $fileUnusedKeys = $allResxKeys[$resxFile] | Where-Object { $_ -in $unusedKeys }
            $report += "`n$resxFile`: $($fileUnusedKeys.Count) unused keys"
        }
    }
    
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Log "Detailed report generated: $reportPath" -Level Success
    Write-Host ""
    
    # Step 4: Delete unused keys (if not WhatIf mode)
    Write-Log "Step 4/4: Processing unused keys" -Level Header
    
    if ($WhatIfPreference) {
        Write-Log "WhatIf mode: Nothing will be actually deleted" -Level Info
        return
    }
    
    # Confirm deletion
    $confirmation = Read-Host "Do you want to delete these unused keys? (Y/N)"
    if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
        Write-Log "Operation cancelled" -Level Warning
        return
    }
    
    # Execute deletion
    $totalRemoved = 0
    foreach ($resxFile in $resxFiles) {
        $resxPath = Join-Path $resourcePath $resxFile
        if (Test-Path $resxPath) {
            if ($allResxKeys.ContainsKey($resxFile)) {
                $fileUnusedKeys = $allResxKeys[$resxFile] | Where-Object { $_ -in $unusedKeys }
                
                if ($fileUnusedKeys.Count -gt 0) {
                    Write-Log "Processing $resxFile..." -Level Info
                    
                    # Backup file
                    if ($Backup) {
                        Backup-ResxFile -FilePath $resxPath
                    }
                    
                    # Delete unused keys
                    $removed = Remove-ResxKeys -ResxPath $resxPath -KeysToRemove $fileUnusedKeys
                    $totalRemoved += $removed
                }
            }
        }
    }
    
    Write-Host ""
    Write-Log "===============================================" -Level Header
    Write-Log "Cleanup completed! Total removed $totalRemoved unused keys" -Level Success
    Write-Log "===============================================" -Level Header
}

# Execute main function
Main
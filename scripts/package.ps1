# CTWebPlayer Packaging Script
# Used to package build output into release version
# Dependency: requires build.ps1 to be run first

param(
    [string]$PublishDir = "publish",
    [string]$PackageDir = "release",
    [string]$Version = "1.2.0",
    [switch]$CreateInstaller = $false
)

# Get project root directory (parent of scripts directory)
$projectRoot = Split-Path -Parent $PSScriptRoot

# Convert relative paths to absolute paths based on project root
if (-not [System.IO.Path]::IsPathRooted($PublishDir)) {
    $PublishDir = Join-Path $projectRoot $PublishDir
}
if (-not [System.IO.Path]::IsPathRooted($PackageDir)) {
    $PackageDir = Join-Path $projectRoot $PackageDir
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer Packaging Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if build output exists
if (-not (Test-Path $PublishDir)) {
    Write-Host "Error: Build output directory '$PublishDir' not found" -ForegroundColor Red
    Write-Host "Please run build.ps1 first to build the project" -ForegroundColor Yellow
    exit 1
}

$exePath = Join-Path $PublishDir "ctwebplayer.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "Error: Executable file '$exePath' not found" -ForegroundColor Red
    exit 1
}

# Create package directory
Write-Host "Creating package directory..." -ForegroundColor Yellow
if (Test-Path $PackageDir) {
    Remove-Item -Path $PackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PackageDir | Out-Null
Write-Host "Created directory: $PackageDir" -ForegroundColor Green

# Set release file names
$timestamp = Get-Date -Format "yyyyMMdd"
$zipFileName = "CTWebPlayer-v${Version}-win-x64.zip"
$zipFilePath = Join-Path $PackageDir $zipFileName
$checksumFileName = "CTWebPlayer-v${Version}-checksums.txt"
$checksumFilePath = Join-Path $PackageDir $checksumFileName

# Create temporary packaging directory
$tempPackDir = Join-Path $PackageDir "CTWebPlayer"
New-Item -ItemType Directory -Path $tempPackDir | Out-Null

# Collect files to package
Write-Host ""
Write-Host "Collecting files..." -ForegroundColor Yellow

# Copy main program
Copy-Item $exePath -Destination $tempPackDir -Force
Write-Host "Copied: ctwebplayer.exe" -ForegroundColor Green

# Copy config file (if exists)
# 注释掉 config.json 的复制，因为包含测试信息，不应该打包分发
# $configPath = Join-Path $PublishDir "config.json"
# if (Test-Path $configPath) {
#     Copy-Item $configPath -Destination $tempPackDir -Force
#     Write-Host "Copied: config.json" -ForegroundColor Green
# } else {
#     # If not in publish directory, check project root
#     $configInRoot = Join-Path $projectRoot "config.json"
#     if (Test-Path $configInRoot) {
#         Copy-Item $configInRoot -Destination $tempPackDir -Force
#         Write-Host "Copied: config.json (from project root)" -ForegroundColor Green
#     }
# }

# Copy resource directory
$resPath = Join-Path $PublishDir "res"
if (Test-Path $resPath) {
    Copy-Item $resPath -Destination $tempPackDir -Recurse -Force
    Write-Host "Copied: res/" -ForegroundColor Green
} else {
    $resInRoot = Join-Path $projectRoot "res"
    if (Test-Path $resInRoot) {
        Copy-Item $resInRoot -Destination $tempPackDir -Recurse -Force
        Write-Host "Copied: res/ (from project root)" -ForegroundColor Green
    }
}

# Copy license files
$licensePath = Join-Path $PublishDir "LICENSE"
if (Test-Path $licensePath) {
    Copy-Item $licensePath -Destination $tempPackDir -Force
    Write-Host "Copied: LICENSE" -ForegroundColor Green
} else {
    $licenseInRoot = Join-Path $projectRoot "LICENSE"
    if (Test-Path $licenseInRoot) {
        Copy-Item $licenseInRoot -Destination $tempPackDir -Force
        Write-Host "Copied: LICENSE (from project root)" -ForegroundColor Green
    }
}

$thirdPartyPath = Join-Path $PublishDir "THIRD_PARTY_LICENSES.txt"
if (Test-Path $thirdPartyPath) {
    Copy-Item $thirdPartyPath -Destination $tempPackDir -Force
    Write-Host "Copied: THIRD_PARTY_LICENSES.txt" -ForegroundColor Green
} else {
    $thirdPartyInRoot = Join-Path $projectRoot "THIRD_PARTY_LICENSES.txt"
    if (Test-Path $thirdPartyInRoot) {
        Copy-Item $thirdPartyInRoot -Destination $tempPackDir -Force
        Write-Host "Copied: THIRD_PARTY_LICENSES.txt (from project root)" -ForegroundColor Green
    }
}

# Create README.txt file
$readmeContent = @"
CTWebPlayer v$Version
===================

Unity3D WebPlayer Dedicated Browser

System Requirements:
- Windows 10 or later (64-bit)
- Microsoft Edge WebView2 Runtime (program will prompt to download if not installed)

Instructions:
1. Double-click to run ctwebplayer.exe
2. The program will automatically check and prompt to install WebView2 runtime (if needed)
3. Enter the Unity WebPlayer game URL in the address bar
4. Enjoy the game!

Features:
- Cache management
- Proxy settings support
- CORS handling
- Detailed logging

License:
This software is released under the BSD 3-Clause License
See LICENSE file for details

Third-party component licenses can be found in THIRD_PARTY_LICENSES.txt

Project homepage: https://github.com/a11s/ctwebplayer
"@

$readmeContent | Out-File -FilePath (Join-Path $tempPackDir "README.txt") -Encoding utf8
Write-Host "Created: README.txt" -ForegroundColor Green

# Create ZIP archive
Write-Host ""
Write-Host "Creating ZIP archive..." -ForegroundColor Yellow

# Use .NET compression functionality
Add-Type -AssemblyName System.IO.Compression.FileSystem

try {
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempPackDir, $zipFilePath, 
        [System.IO.Compression.CompressionLevel]::Optimal, $false)
    Write-Host "Created: $zipFileName" -ForegroundColor Green
    
    $zipInfo = Get-Item $zipFilePath
    Write-Host "File size: $([math]::Round($zipInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
} catch {
    Write-Host "Error: Failed to create ZIP file - $_" -ForegroundColor Red
    exit 1
}

# Calculate SHA256 hash
Write-Host ""
Write-Host "Calculating file hashes..." -ForegroundColor Yellow

$checksumContent = @()
$checksumContent += "CTWebPlayer v$Version - SHA256 Checksums"
$checksumContent += "======================================="
$checksumContent += ""

# Calculate ZIP file hash
$zipHash = Get-FileHash -Path $zipFilePath -Algorithm SHA256
$checksumContent += "$($zipHash.Hash)  $zipFileName"

# Calculate EXE file hash
$exeFullPath = Join-Path $tempPackDir "ctwebplayer.exe"
$exeHash = Get-FileHash -Path $exeFullPath -Algorithm SHA256
$checksumContent += "$($exeHash.Hash)  ctwebplayer.exe"

# Save hash file
$checksumContent | Out-File -FilePath $checksumFilePath -Encoding utf8
Write-Host "Created: $checksumFileName" -ForegroundColor Green

# Display hash values
Write-Host ""
Write-Host "SHA256 hashes:" -ForegroundColor Yellow
foreach ($line in $checksumContent) {
    if ($line -match "^[A-F0-9]{64}") {
        Write-Host $line -ForegroundColor DarkGray
    }
}

# Clean up temporary directory
Write-Host ""
Write-Host "Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item -Path $tempPackDir -Recurse -Force
Write-Host "Cleaned up temporary directory" -ForegroundColor Green

# Generate release info
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
Write-Host "Created: release-info.json" -ForegroundColor Green

# Create installer (if specified)
if ($CreateInstaller) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "Creating installer..." -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    
    $installerScriptPath = Join-Path (Split-Path $PSScriptRoot -Parent) "installer\build-installer.ps1"
    if (Test-Path $installerScriptPath) {
        # Call installer build script
        & $installerScriptPath -PublishDir $PublishDir -Version $Version
        
        # Update release info
        $setupFileName = "CTWebPlayer-v${Version}-Setup.exe"
        $setupFilePath = Join-Path $PackageDir $setupFileName
        if (Test-Path $setupFilePath) {
            $setupInfo = Get-Item $setupFilePath
            $setupHash = Get-FileHash -Path $setupFilePath -Algorithm SHA256
            
            # Reload and update release-info.json
            $releaseInfo = Get-Content $releaseInfoPath | ConvertFrom-Json
            $releaseInfo.files += @{
                name = $setupFileName
                size = $setupInfo.Length
                sha256 = $setupHash.Hash
            }
            
            $releaseInfo | ConvertTo-Json -Depth 3 | Out-File -FilePath $releaseInfoPath -Encoding utf8
            Write-Host "Updated release-info.json with installer info" -ForegroundColor Green
        }
    } else {
        Write-Host "Warning: Installer build script not found: $installerScriptPath" -ForegroundColor Yellow
        Write-Host "Skipping installer creation" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Packaging completed!" -ForegroundColor Green
Write-Host "Release directory: $PackageDir" -ForegroundColor Cyan
Write-Host "ZIP archive: $zipFileName" -ForegroundColor Cyan
if ($CreateInstaller -and (Test-Path (Join-Path $PackageDir "CTWebPlayer-v${Version}-Setup.exe"))) {
    Write-Host "Installer: CTWebPlayer-v${Version}-Setup.exe" -ForegroundColor Cyan
}
Write-Host "=====================================" -ForegroundColor Cyan
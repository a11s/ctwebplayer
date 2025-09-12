# CTWebPlayer Build Script
# For building the project and generating release version
# Requires: .NET 8.0 SDK

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "publish"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "Error: .NET SDK not found, please install .NET 8.0 SDK first" -ForegroundColor Red
    exit 1
}

# Clean previous build output
Write-Host ""
Write-Host "Cleaning previous build output..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "Cleaned directory: $OutputDir" -ForegroundColor Green
}

if (Test-Path "bin\$Configuration") {
    Remove-Item -Path "bin\$Configuration" -Recurse -Force
    Write-Host "Cleaned directory: bin\$Configuration" -ForegroundColor Green
}

if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force
    Write-Host "Cleaned directory: obj" -ForegroundColor Green
}

# Restore project dependencies
Write-Host ""
Write-Host "Restoring project dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to restore dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "Dependencies restored successfully" -ForegroundColor Green

# Build project
Write-Host ""
Write-Host "Starting project build..." -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Runtime: $Runtime" -ForegroundColor Cyan
Write-Host "Output Directory: $OutputDir" -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:DebugType=none",
    "-p:DebugSymbols=false",
    "-o", $OutputDir
)

# Include WebView2 runtime
$publishArgs += "-p:PublishReadyToRun=true"
$publishArgs += "-p:PublishTrimmed=false"  # Avoid trimming issues

Write-Host ""
Write-Host "Executing command: dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray
dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

# Verify build output
Write-Host ""
Write-Host "Verifying build output..." -ForegroundColor Yellow
$exePath = Join-Path $OutputDir "ctwebplayer.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host "Executable: $exePath" -ForegroundColor Cyan
    Write-Host "File size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    
    # Get file version info
    try {
        $fullPath = (Get-Item $exePath).FullName
        $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fullPath)
        if ($versionInfo.FileVersion) {
            Write-Host "File version: $($versionInfo.FileVersion)" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "Warning: Unable to get version info from executable" -ForegroundColor Yellow
    }

    # Copy configuration files and resources (if they exist)
    Write-Host ""
    Write-Host "Copying resource files..." -ForegroundColor Yellow

    # Copy config file
    # if (Test-Path "config.json") {
    #     Copy-Item "config.json" -Destination $OutputDir -Force
    #     Write-Host "Copied: config.json" -ForegroundColor Green
    # }

    # Copy resource directory
    if (Test-Path "res") {
        Copy-Item "res" -Destination $OutputDir -Recurse -Force
        Write-Host "Copied: res/" -ForegroundColor Green
    }

    # Copy license files
    if (Test-Path "LICENSE") {
        Copy-Item "LICENSE" -Destination $OutputDir -Force
        Write-Host "Copied: LICENSE" -ForegroundColor Green
    }

    if (Test-Path "THIRD_PARTY_LICENSES.txt") {
        Copy-Item "THIRD_PARTY_LICENSES.txt" -Destination $OutputDir -Force
        Write-Host "Copied: THIRD_PARTY_LICENSES.txt" -ForegroundColor Green
    }
}
else {
    Write-Host "Error: Build output file not found" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Build completed!" -ForegroundColor Green
Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

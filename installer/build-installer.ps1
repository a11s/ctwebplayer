# CTWebPlayer Installer Build Script
# Used to create installer using Inno Setup
# Dependency: Inno Setup 6.x

param(
    [string]$PublishDir = "..\publish",
    [string]$Version = "1.0.0",
    [switch]$Silent = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer Installer Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check current directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

try {
    # Check if build output exists
    if (-not (Test-Path $PublishDir)) {
        Write-Host "Error: Build output directory '$PublishDir' not found" -ForegroundColor Red
        Write-Host "Please run scripts/build.ps1 first to build the project" -ForegroundColor Yellow
        exit 1
    }

    $exePath = Join-Path $PublishDir "ctwebplayer.exe"
    if (-not (Test-Path $exePath)) {
        Write-Host "Error: Executable file '$exePath' not found" -ForegroundColor Red
        exit 1
    }

    # Find Inno Setup compiler
    Write-Host "Looking for Inno Setup compiler..." -ForegroundColor Yellow
    
    $innoSetupPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 5\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
    )
    
    $isccPath = $null
    foreach ($path in $innoSetupPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
    
    # Check environment variable
    if (-not $isccPath) {
        $isccInPath = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
        if ($isccInPath) {
            $isccPath = $isccInPath.Source
        }
    }
    
    if (-not $isccPath) {
        Write-Host "Error: Inno Setup compiler (ISCC.exe) not found" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install Inno Setup 6:" -ForegroundColor Yellow
        Write-Host "1. Visit https://jrsoftware.org/isdl.php" -ForegroundColor Gray
        Write-Host "2. Download and install Inno Setup" -ForegroundColor Gray
        Write-Host "3. Make sure to install to default path or add ISCC.exe to PATH environment variable" -ForegroundColor Gray
        Write-Host ""
        
        # Ask if user wants to open download page
        $response = Read-Host "Do you want to open Inno Setup download page? (Y/N)"
        if ($response -eq 'Y' -or $response -eq 'y') {
            Start-Process "https://jrsoftware.org/isdl.php"
        }
        
        exit 1
    }
    
    Write-Host "Found Inno Setup: $isccPath" -ForegroundColor Green
    
    # Check if setup script exists
    $setupScriptPath = Join-Path $scriptDir "setup.iss"
    if (-not (Test-Path $setupScriptPath)) {
        Write-Host "Error: Setup script 'setup.iss' not found" -ForegroundColor Red
        exit 1
    }
    
    # Update version number (if specified)
    if ($Version -ne "1.0.0") {
        Write-Host "Updating version to: $Version" -ForegroundColor Yellow
        
        # Read and update version in setup.iss
        $content = Get-Content $setupScriptPath -Raw
        $content = $content -replace '#define AppVersion "[\d\.]+"', "#define AppVersion `"$Version`""
        
        # Create temporary file
        $tempScriptPath = Join-Path $scriptDir "setup_temp.iss"
        $content | Out-File -FilePath $tempScriptPath -Encoding utf8
        $setupScriptPath = $tempScriptPath
    }
    
    # Prepare output directory
    $outputDir = Join-Path (Split-Path $scriptDir -Parent) "release"
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir | Out-Null
        Write-Host "Created output directory: $outputDir" -ForegroundColor Green
    }
    
    # Build command line arguments
    $arguments = @()
    
    # Silent mode
    if ($Silent) {
        $arguments += "/Q"
    }
    
    # Add script path
    $arguments += "`"$setupScriptPath`""
    
    # Compile installer
    Write-Host ""
    Write-Host "Starting installer compilation..." -ForegroundColor Yellow
    Write-Host "Executing command: $isccPath $($arguments -join ' ')" -ForegroundColor Gray
    Write-Host ""
    
    $process = Start-Process -FilePath $isccPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host "Installer build successful!" -ForegroundColor Green
        
        # Find generated installer
        $setupFileName = "CTWebPlayer-v${Version}-Setup.exe"
        $setupFilePath = Join-Path $outputDir $setupFileName
        
        if (Test-Path $setupFilePath) {
            $fileInfo = Get-Item $setupFilePath
            Write-Host "File: $setupFileName" -ForegroundColor Cyan
            Write-Host "Size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
            Write-Host "Path: $setupFilePath" -ForegroundColor Cyan
            
            # Calculate SHA256
            Write-Host ""
            Write-Host "Calculating SHA256 hash..." -ForegroundColor Yellow
            $hash = Get-FileHash -Path $setupFilePath -Algorithm SHA256
            Write-Host "SHA256: $($hash.Hash)" -ForegroundColor Gray
            
            # Save hash
            $hashFileName = "CTWebPlayer-v${Version}-Setup.exe.sha256"
            $hashFilePath = Join-Path $outputDir $hashFileName
            "$($hash.Hash)  $setupFileName" | Out-File -FilePath $hashFilePath -Encoding utf8
            Write-Host "Saved hash file: $hashFileName" -ForegroundColor Green
        }
        
        Write-Host "=====================================" -ForegroundColor Cyan
    } else {
        Write-Host ""
        Write-Host "Error: Installer build failed (exit code: $($process.ExitCode))" -ForegroundColor Red
        exit $process.ExitCode
    }
    
} finally {
    # Clean up temporary files
    if ($tempScriptPath -and (Test-Path $tempScriptPath)) {
        Remove-Item $tempScriptPath -Force
    }
    
    Pop-Location
}

# Prompt for next steps
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the installer to ensure it works correctly" -ForegroundColor Gray
Write-Host "2. Sign the installer with digital signature tool (optional but recommended)" -ForegroundColor Gray
Write-Host "3. Upload to GitHub Release or other distribution channels" -ForegroundColor Gray
Write-Host ""
Write-Host "Test command:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName" -ForegroundColor Gray
Write-Host ""
Write-Host "Silent install command:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName /SILENT" -ForegroundColor Gray
Write-Host "  .\release\$setupFileName /VERYSILENT" -ForegroundColor Gray
Write-Host ""
Write-Host "Custom install directory:" -ForegroundColor Yellow
Write-Host "  .\release\$setupFileName /DIR=`"C:\MyApps\CTWebPlayer`"" -ForegroundColor Gray
Write-Host ""
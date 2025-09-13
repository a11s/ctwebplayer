# CTWebPlayer Release Automation Test Script
# Used to verify version consistency, required file checks, and build/packaging process
# Usage: .\test\release-test.ps1 [-Version "1.0.0"] [-Clean $true]

param(
    [string]$Version = "",
    [switch]$Clean = $false,
    [switch]$Verbose = $false
)

# Color output function
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Verify version consistency
function Test-VersionConsistency {
    param([string]$ExpectedVersion)
    
    Write-ColorOutput "=== Verifying Version Consistency ===" "Cyan"
    
    if (-not $ExpectedVersion) {
        $ExpectedVersion = "1.0.0"  # Default version
        Write-ColorOutput "Using default version: $ExpectedVersion" "Yellow"
    }
    
    $versionParts = $ExpectedVersion.Split('.')
    $major = $versionParts[0]
    $minor = $versionParts[1]
    $patch = $versionParts[2]
    $assemblyVersion = "$major.$minor.$patch.0"
    $fileVersion = $ExpectedVersion
    
    # Check version in csproj
    $csprojPath = "src/ctwebplayer.csproj"
    if (Test-Path $csprojPath) {
        $csprojContent = Get-Content $csprojPath -Raw
        $expectedVersionTag = "<Version>$ExpectedVersion</Version>"
        $expectedAssemblyTag = "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
        $expectedFileTag = "<FileVersion>$fileVersion</FileVersion>"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedVersionTag)) {
            Write-ColorOutput "❌ Version mismatch in csproj: expected $ExpectedVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj Version: $ExpectedVersion" "Green"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedAssemblyTag)) {
            Write-ColorOutput "❌ AssemblyVersion mismatch in csproj: expected $assemblyVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj AssemblyVersion: $assemblyVersion" "Green"
        
        if ($csprojContent -notmatch [regex]::Escape($expectedFileTag)) {
            Write-ColorOutput "❌ FileVersion mismatch in csproj: expected $fileVersion" "Red"
            return $false
        }
        Write-ColorOutput "✅ csproj FileVersion: $fileVersion" "Green"
    } else {
        Write-ColorOutput "❌ csproj file not found" "Red"
        return $false
    }
    
    # Check Git tag (if in Git repository)
    if (Get-Command git -ErrorAction SilentlyContinue) {
        $currentTag = git describe --tags --exact-match --abbrev=0 2>$null
        if ($currentTag -eq "v$ExpectedVersion") {
            Write-ColorOutput "✅ Git tag: v$ExpectedVersion" "Green"
        } else {
            Write-ColorOutput "⚠️ Git tag mismatch or not set: current $currentTag, expected v$ExpectedVersion" "Yellow"
        }
    }
    
    return $true
}

# Check required files
function Test-RequiredFiles {
    Write-ColorOutput "=== Checking Required Files ===" "Cyan"
    
    $requiredFiles = @(
        "LICENSE",  # BSD3 license
        "THIRD_PARTY_LICENSES.txt",  # Third-party licenses
        "README.md",  # Project documentation
        "src/ctwebplayer.csproj",  # Project file
        "config.json",  # Default configuration (if exists)
        "res/c001_01_Icon_Texture.ico",  # Icon resource
        "installer/setup.iss"  # Inno Setup script
    )
    
    $missingFiles = @()
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            $missingFiles += $file
            Write-ColorOutput "❌ Missing file: $file" "Red"
        } else {
            if ($Verbose) {
                Write-ColorOutput "✅ Found: $file" "Green"
            }
        }
    }
    
    if ($missingFiles.Count -eq 0) {
        Write-ColorOutput "✅ All required files present" "Green"
        return $true
    } else {
        Write-ColorOutput "❌ Found $($missingFiles.Count) missing files" "Red"
        return $false
    }
}

# Test build process
function Test-BuildProcess {
    Write-ColorOutput "=== Testing Build Process ===" "Cyan"
    
    $publishDir = "publish"
    
    # Clean
    if ($Clean) {
        if (Test-Path $publishDir) {
            Remove-Item $publishDir -Recurse -Force
            Write-ColorOutput "Cleaned old build output" "Yellow"
        }
        dotnet clean
    }
    
    # Restore NuGet packages
    Write-ColorOutput "Restoring NuGet packages..." "Yellow"
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ NuGet restore failed" "Red"
        return $false
    }
    
    # Build Release
    Write-ColorOutput "Building Release configuration..." "Yellow"
    dotnet build --no-restore -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ Build failed" "Red"
        return $false
    }
    Write-ColorOutput "✅ Build successful" "Green"
    
    # Publish single-file exe
    Write-ColorOutput "Publishing single-file exe..." "Yellow"
    $publishArgs = @(
        "publish",
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-o", $publishDir,
        "--no-build"
    )
    dotnet $publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ Publish failed" "Red"
        return $false
    }
    
    # Check output file
    $exePath = Join-Path $publishDir "ctwebplayer.exe"
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        $fileSize = $fileInfo.Length / 1MB
        Write-ColorOutput "✅ Publish successful: ctwebplayer.exe ($([math]::Round($fileSize, 2)) MB)" "Green"
        
        # Basic run test (not actually running, just checking if executable)
        if (Get-Command $exePath -ErrorAction SilentlyContinue) {
            Write-ColorOutput "✅ exe file is executable" "Green"
        } else {
            Write-ColorOutput "⚠️ Cannot verify exe executability (may need admin rights)" "Yellow"
        }
    } else {
        Write-ColorOutput "❌ ctwebplayer.exe not generated" "Red"
        return $false
    }
    
    # Copy additional files
    $configPath = "config.json"
    $resDir = "res"
    if (Test-Path $configPath) {
        Copy-Item $configPath $publishDir
        Write-ColorOutput "✅ Copied config.json" "Green"
    }
    
    if (Test-Path $resDir) {
        $publishResDir = Join-Path $publishDir "res"
        if (-not (Test-Path $publishResDir)) {
            New-Item -ItemType Directory -Path $publishResDir -Force
        }
        Copy-Item "$resDir\*" $publishResDir -Recurse -Force
        Write-ColorOutput "✅ Copied res/ directory" "Green"
    }
    
    return $true
}

# Test packaging process (Inno Setup)
function Test-PackageProcess {
    Write-ColorOutput "=== Testing Packaging Process (Inno Setup) ===" "Cyan"
    
    $installerDir = "installer"
    $buildScript = Join-Path $installerDir "build-installer.ps1"
    $outputDir = "output"
    
    if (Test-Path $buildScript) {
        # Build installer
        Push-Location $installerDir
        & $buildScript
        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "❌ Inno Setup build failed" "Red"
            Pop-Location
            return $false
        }
        Pop-Location
        
        # Check output
        $setupPath = Join-Path $outputDir "setup.exe"
        if (Test-Path $setupPath) {
            $fileInfo = Get-Item $setupPath
            $fileSize = $fileInfo.Length / 1MB
            Write-ColorOutput "✅ Installer generated: setup.exe ($([math]::Round($fileSize, 2)) MB)" "Green"
            
            # Basic validation: check if file exists
            # Note: not actually running the installer
            Write-ColorOutput "✅ Installer file exists" "Green"
        } else {
            Write-ColorOutput "❌ setup.exe not generated" "Red"
            return $false
        }
    } else {
        Write-ColorOutput "⚠️ build-installer.ps1 not found, skipping Inno Setup test" "Yellow"
        return $true  # Non-critical failure
    }
    
    return $true
}

# Main function
function Main {
    $allPassed = $true
    
    # Version consistency test
    if (-not (Test-VersionConsistency -ExpectedVersion $Version)) {
        $allPassed = $false
    }
    
    # Required files check
    if (-not (Test-RequiredFiles)) {
        $allPassed = $false
    }
    
    # Build test
    if (-not (Test-BuildProcess)) {
        $allPassed = $false
    }
    
    # Package test
    if (-not (Test-PackageProcess)) {
        $allPassed = $false
    }
    
    # Final summary
    Write-ColorOutput "`n=== Test Summary ===" "Cyan"
    if ($allPassed) {
        Write-ColorOutput "🎉 All tests passed! Ready for release." "Green"
        exit 0
    } else {
        Write-ColorOutput "❌ Tests failed. Please fix issues and retry." "Red"
        exit 1
    }
}

# Run main function
Main
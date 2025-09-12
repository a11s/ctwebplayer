# CTWebPlayer Release Main Script
# Coordinates the entire release process: build -> package -> prepare release
# Usage: .\scripts\release.ps1 -Version "1.0.0"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipBuild = $false,
    [switch]$SkipTests = $false,
    [switch]$CreateTag = $false,
    [switch]$CreateGitHubRelease = $false,
    [string]$GitHubToken = $env:GITHUB_TOKEN
)

# Set error handling
$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CTWebPlayer Release Process" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Version: v$Version" -ForegroundColor Yellow
Write-Host ""

# Verify script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

# Switch to project root directory
Push-Location $projectRoot
try {
    # Step 1: Update version number
    Write-Host "Step 1: Updating project version number..." -ForegroundColor Yellow
    $csprojPath = "ctwebplayer.csproj"
    if (Test-Path $csprojPath) {
        $csprojContent = Get-Content $csprojPath -Raw
        
        # Update version number
        $versionPattern = '<Version>[\d\.]+</Version>'
        $assemblyPattern = '<AssemblyVersion>[\d\.]+</AssemblyVersion>'
        $filePattern = '<FileVersion>[\d\.]+</FileVersion>'
        
        $csprojContent = $csprojContent -replace $versionPattern, "<Version>$Version</Version>"
        $csprojContent = $csprojContent -replace $assemblyPattern, "<AssemblyVersion>$Version.0</AssemblyVersion>"
        $csprojContent = $csprojContent -replace $filePattern, "<FileVersion>$Version.0</FileVersion>"
        
        $csprojContent | Out-File $csprojPath -Encoding utf8
        Write-Host "Updated version to: $Version" -ForegroundColor Green
    } else {
        Write-Host "Warning: Project file $csprojPath not found" -ForegroundColor Yellow
    }
    
    # Step 2: Run tests (optional)
    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "Step 2: Running tests..." -ForegroundColor Yellow
        # If there are test projects, run them here
        # dotnet test
        Write-Host "Skipping tests (no test project available)" -ForegroundColor DarkGray
    }
    
    # Step 3: Build project
    if (-not $SkipBuild) {
        Write-Host ""
        Write-Host "Step 3: Building project..." -ForegroundColor Yellow
        & "$scriptDir\build.ps1" -Configuration $Configuration -Runtime $Runtime
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }
    } else {
        Write-Host ""
        Write-Host "Skipping build step" -ForegroundColor DarkGray
    }
    
    # Step 4: Package release files
    Write-Host ""
    Write-Host "Step 4: Packaging release files..." -ForegroundColor Yellow
    & "$scriptDir\package.ps1" -Version $Version
    if ($LASTEXITCODE -ne 0) {
        throw "Packaging failed"
    }
    
    # Step 5: Create CHANGELOG entry
    Write-Host ""
    Write-Host "Step 5: Preparing changelog..." -ForegroundColor Yellow
    $changelogPath = "CHANGELOG.md"
    $changelogTemplate = @"
# Changelog

## [v$Version] - $(Get-Date -Format "yyyy-MM-dd")

### New Features
- 

### Improvements
- 

### Bug Fixes
- 

### Other
- 

---

"@
    
    if (-not (Test-Path $changelogPath)) {
        $changelogTemplate | Out-File $changelogPath -Encoding utf8
        Write-Host "Created CHANGELOG.md template" -ForegroundColor Green
    } else {
        Write-Host "CHANGELOG.md already exists, please update manually" -ForegroundColor Yellow
    }
    
    # Step 6: Create Git tag (optional)
    if ($CreateTag) {
        Write-Host ""
        Write-Host "Step 6: Creating Git tag..." -ForegroundColor Yellow
        
        # Check for uncommitted changes
        $gitStatus = git status --porcelain
        if ($gitStatus) {
            Write-Host "Warning: Uncommitted changes detected" -ForegroundColor Yellow
            Write-Host "Please commit changes before creating tag" -ForegroundColor Yellow
        } else {
            $tagName = "v$Version"
            git tag -a $tagName -m "Release version $Version"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Created Git tag: $tagName" -ForegroundColor Green
                Write-Host "Use 'git push origin $tagName' to push tag to remote repository" -ForegroundColor Cyan
            } else {
                Write-Host "Failed to create Git tag" -ForegroundColor Red
            }
        }
    }
    
    # Generate release summary
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "Release preparation complete!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Version: v$Version" -ForegroundColor Cyan
    Write-Host "Release files location: release\" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Release file list:" -ForegroundColor Yellow
    Get-ChildItem "release" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Update CHANGELOG.md file" -ForegroundColor Gray
    Write-Host "2. Commit all changes to Git" -ForegroundColor Gray
    Write-Host "3. Push code and tags to GitHub" -ForegroundColor Gray
    Write-Host "4. Create Release on GitHub" -ForegroundColor Gray
    Write-Host "5. Upload files from release/ directory as Release assets" -ForegroundColor Gray
    Write-Host ""
    
    # Generate GitHub Release draft content
    $releaseDraftPath = "release\RELEASE_DRAFT.md"
    $releaseDraft = @"
# CTWebPlayer v$Version

Release Date: $(Get-Date -Format "yyyy-MM-dd")

## Downloads

- [CTWebPlayer-v$Version-win-x64.zip](../../releases/download/v$Version/CTWebPlayer-v$Version-win-x64.zip) - Windows x64 version

## System Requirements

- Windows 10 or later (64-bit)
- Microsoft Edge WebView2 Runtime

## What's New

### New Features
- 

### Improvements
- 

### Bug Fixes
- 

## Installation Instructions

1. Download the ZIP file
2. Extract to any directory
3. Run ctwebplayer.exe
4. The program will automatically check and prompt to install WebView2 runtime (if needed)

## File Verification

Please check the CTWebPlayer-v$Version-checksums.txt file to verify the integrity of downloaded files.

## License

This software is released under the BSD 3-Clause License.
"@
    
    $releaseDraft | Out-File $releaseDraftPath -Encoding utf8
    Write-Host "Generated GitHub Release draft: $releaseDraftPath" -ForegroundColor Green
    
    # Step 7: Create GitHub Release (optional)
    if ($CreateGitHubRelease) {
        Write-Host ""
        Write-Host "Step 7: Creating GitHub Release..." -ForegroundColor Yellow
        
        if (-not $GitHubToken) {
            Write-Host "Error: GitHub Token not provided" -ForegroundColor Red
            Write-Host "Please set GITHUB_TOKEN environment variable or use -GitHubToken parameter" -ForegroundColor Yellow
        } else {
            # GitHub API settings
            $headers = @{
                "Authorization" = "Bearer $GitHubToken"
                "Accept" = "application/vnd.github.v3+json"
            }
            $repoOwner = "a11s"
            $repoName = "ctwebplayer"
            $apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases"
            
            try {
                # Read Release draft content
                $releaseDraftPath = "release\RELEASE_DRAFT.md"
                if (Test-Path $releaseDraftPath) {
                    $releaseBody = Get-Content $releaseDraftPath -Raw
                } else {
                    $releaseBody = "CTWebPlayer v$Version - Automated Release"
                }
                
                # Create Release
                $releaseData = @{
                    tag_name = "v$Version"
                    target_commitish = "main"
                    name = "CTWebPlayer v$Version"
                    body = $releaseBody
                    draft = $false
                    prerelease = $false
                } | ConvertTo-Json
                
                Write-Host "Creating GitHub Release..." -ForegroundColor Cyan
                $release = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $releaseData -ContentType "application/json"
                $releaseId = $release.id
                $uploadUrl = $release.upload_url -replace '\{.*\}', ''
                
                Write-Host "Release created successfully: $($release.html_url)" -ForegroundColor Green
                
                # Upload release files
                Write-Host ""
                Write-Host "Uploading release files..." -ForegroundColor Yellow
                
                $releaseFiles = Get-ChildItem "release" -File | Where-Object {
                    $_.Extension -in @('.zip', '.txt') -and $_.Name -ne 'RELEASE_DRAFT.md' -and $_.Name -ne 'release-info.json'
                }
                
                foreach ($file in $releaseFiles) {
                    Write-Host "Uploading: $($file.Name)..." -ForegroundColor Cyan
                    
                    # Determine content type
                    $contentType = switch ($file.Extension) {
                        '.zip' { 'application/zip' }
                        '.txt' { 'text/plain' }
                        default { 'application/octet-stream' }
                    }
                    
                    $uploadHeaders = @{
                        "Authorization" = "Bearer $GitHubToken"
                        "Accept" = "application/vnd.github.v3+json"
                        "Content-Type" = $contentType
                    }
                    
                    $uploadUri = "${uploadUrl}?name=$($file.Name)"
                    $fileBytes = [System.IO.File]::ReadAllBytes($file.FullName)
                    
                    try {
                        $asset = Invoke-RestMethod -Uri $uploadUri -Method Post -Headers $uploadHeaders -Body $fileBytes
                        Write-Host "Uploaded: $($file.Name)" -ForegroundColor Green
                    } catch {
                        Write-Host "Upload failed: $($file.Name) - $_" -ForegroundColor Red
                    }
                }
                
                Write-Host ""
                Write-Host "GitHub Release published!" -ForegroundColor Green
                Write-Host "View Release: $($release.html_url)" -ForegroundColor Cyan
                
            } catch {
                Write-Host "Failed to create GitHub Release: $_" -ForegroundColor Red
                Write-Host "Please create Release manually on GitHub" -ForegroundColor Yellow
            }
        }
    }
    
    # Display completion message
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "All steps completed!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    
} finally {
    Pop-Location
}
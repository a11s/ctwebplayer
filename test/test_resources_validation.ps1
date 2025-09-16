# PowerShell script to test resource files validation
Write-Host "=== Testing Resource Files Validation ===" -ForegroundColor Cyan
Write-Host ""

# Function to check XML validity
function Test-XmlFile {
    param([string]$FilePath)
    
    try {
        [xml]$xml = Get-Content $FilePath -Raw
        return $true
    }
    catch {
        Write-Host "  ❌ XML Error in $FilePath : $_" -ForegroundColor Red
        return $false
    }
}

# Function to get resource keys
function Get-ResourceKeys {
    param([string]$FilePath)
    
    try {
        [xml]$xml = Get-Content $FilePath -Raw
        $keys = @()
        
        foreach ($node in $xml.root.data) {
            if ($node.name) {
                $keys += $node.name
            }
        }
        
        return $keys
    }
    catch {
        Write-Host "  Error reading $FilePath : $_" -ForegroundColor Red
        return @()
    }
}

# Define resource files
$resourcePath = "../src/Resources"
$resourceFiles = @{
    "English (Base)" = "Strings.resx"
    "Chinese Simplified" = "Strings.zh-CN.resx"
    "Chinese Traditional" = "Strings.zh-TW.resx"
    "Japanese" = "Strings.ja.resx"
    "Korean" = "Strings.ko.resx"
}

# Test XML validity
Write-Host "1. Testing XML Format:" -ForegroundColor Yellow
$xmlValid = $true
foreach ($kvp in $resourceFiles.GetEnumerator()) {
    $filePath = Join-Path $resourcePath $kvp.Value
    Write-Host "  Checking $($kvp.Key)..." -NoNewline
    
    if (Test-XmlFile $filePath) {
        Write-Host " ✓" -ForegroundColor Green
    }
    else {
        $xmlValid = $false
    }
}

if ($xmlValid) {
    Write-Host "  ✅ All XML files are valid!" -ForegroundColor Green
}
Write-Host ""

# Get base keys
$baseFile = Join-Path $resourcePath "Strings.resx"
$baseKeys = Get-ResourceKeys $baseFile
Write-Host "2. Base Resource Analysis:" -ForegroundColor Yellow
Write-Host "  Total keys in base file: $($baseKeys.Count)" -ForegroundColor Cyan
Write-Host ""

# Compare each language file
Write-Host "3. Language Files Comparison:" -ForegroundColor Yellow
$allMatch = $true

foreach ($kvp in $resourceFiles.GetEnumerator()) {
    if ($kvp.Key -eq 'English (Base)') { continue }
    
    $filePath = Join-Path $resourcePath $kvp.Value
    $keys = Get-ResourceKeys $filePath
    
    Write-Host "  $($kvp.Key):" -ForegroundColor Cyan
    Write-Host "    Total keys: $($keys.Count)"
    
    # Find missing keys
    $missingKeys = $baseKeys | Where-Object { $_ -notin $keys }
    $extraKeys = $keys | Where-Object { $_ -notin $baseKeys }
    
    if ($missingKeys.Count -gt 0) {
        Write-Host "    ⚠️  Missing keys ($($missingKeys.Count)):" -ForegroundColor Yellow
        foreach ($key in $missingKeys) {
            Write-Host "      - $key" -ForegroundColor Red
        }
        $allMatch = $false
    }
    
    if ($extraKeys.Count -gt 0) {
        Write-Host "    ⚠️  Extra keys ($($extraKeys.Count)):" -ForegroundColor Yellow
        foreach ($key in $extraKeys) {
            Write-Host "      - $key" -ForegroundColor Magenta
        }
        $allMatch = $false
    }
    
    if ($missingKeys.Count -eq 0 -and $extraKeys.Count -eq 0) {
        Write-Host "    ✅ All keys match!" -ForegroundColor Green
    }
    
    Write-Host ""
}

# Specific translation checks
Write-Host "4. Checking Specific Translations:" -ForegroundColor Yellow

$specificChecks = @(
    @{Key="Form1_Screenshot_WebGLNotSupported"; Files=@("Strings.ja.resx", "Strings.ko.resx")},
    @{Key="Form1_Screenshot_EmptyCanvas"; Files=@("Strings.ja.resx", "Strings.ko.resx")},
    @{Key="CookieManager_Loading"; Files=@("Strings.zh-TW.resx")},
    @{Key="CookieManager_LoadError"; Files=@("Strings.zh-TW.resx")}
)

$specificChecksPassed = $true
foreach ($check in $specificChecks) {
    Write-Host "  Checking '$($check.Key)'..." -NoNewline
    $found = $true
    
    foreach ($file in $check.Files) {
        $filePath = Join-Path $resourcePath $file
        $keys = Get-ResourceKeys $filePath
        
        if ($check.Key -notin $keys) {
            if ($found) {
                Write-Host ""
            }
            Write-Host "    ❌ Missing in $file" -ForegroundColor Red
            $found = $false
            $specificChecksPassed = $false
        }
    }
    
    if ($found) {
        Write-Host " ✓" -ForegroundColor Green
    }
}

Write-Host ""

# Final summary
Write-Host "5. Summary:" -ForegroundColor Yellow
if ($xmlValid -and $allMatch -and $specificChecksPassed) {
    Write-Host "  ✅ All resource files are properly formatted and complete!" -ForegroundColor Green
    Write-Host "  All translations have been successfully fixed." -ForegroundColor Green
}
else {
    Write-Host "  ⚠️  Issues found:" -ForegroundColor Yellow
    if (-not $xmlValid) {
        Write-Host "    - XML format errors detected" -ForegroundColor Red
    }
    if (-not $allMatch) {
        Write-Host "    - Key mismatches between language files" -ForegroundColor Red
    }
    if (-not $specificChecksPassed) {
        Write-Host "    - Specific translations are missing" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Test completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
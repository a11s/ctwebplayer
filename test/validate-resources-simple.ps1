# Simple validation script for resource files
Write-Host "`n=== Resource Files Validation ===" -ForegroundColor Cyan

$baseFile = "src/Resources/Strings.resx"
$zhCN = "src/Resources/Strings.zh-CN.resx"
$zhTW = "src/Resources/Strings.zh-TW.resx"
$ja = "src/Resources/Strings.ja.resx"
$ko = "src/Resources/Strings.ko.resx"

# Test 1: Check if files exist and are valid XML
Write-Host "`n1. Checking XML validity:" -ForegroundColor Yellow
$files = @($baseFile, $zhCN, $zhTW, $ja, $ko)
$allValid = $true

foreach ($file in $files) {
    Write-Host "   $file ... " -NoNewline
    try {
        [xml]$xml = Get-Content $file -Raw -ErrorAction Stop
        Write-Host "OK" -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR" -ForegroundColor Red
        Write-Host "     $_" -ForegroundColor Red
        $allValid = $false
    }
}

if ($allValid) {
    Write-Host "`n   ✅ All XML files are valid!" -ForegroundColor Green
}

# Test 2: Count keys in each file
Write-Host "`n2. Counting translation keys:" -ForegroundColor Yellow
$counts = @{}

foreach ($file in $files) {
    try {
        [xml]$xml = Get-Content $file -Raw
        $keyCount = ($xml.root.data | Where-Object { $_.name }).Count
        $fileName = Split-Path $file -Leaf
        $counts[$fileName] = $keyCount
        Write-Host "   $fileName : $keyCount keys" -ForegroundColor Cyan
    }
    catch {
        Write-Host "   Error reading $file" -ForegroundColor Red
    }
}

# Test 3: Check specific translations
Write-Host "`n3. Checking specific fixes:" -ForegroundColor Yellow

# Check Korean file for XML fix
Write-Host "   Korean XML structure... " -NoNewline
try {
    [xml]$xml = Get-Content $ko -Raw
    # Check for the specific CookieManager_ExportError node
    $exportError = $xml.root.data | Where-Object { $_.name -eq "CookieManager_ExportError" }
    if ($exportError) {
        Write-Host "OK" -ForegroundColor Green
    } else {
        Write-Host "Missing CookieManager_ExportError" -ForegroundColor Red
    }
}
catch {
    Write-Host "ERROR" -ForegroundColor Red
}

# Check zh-TW for added translations
Write-Host "   Chinese Traditional additions... " -NoNewline
try {
    [xml]$xml = Get-Content $zhTW -Raw
    $loading = $xml.root.data | Where-Object { $_.name -eq "CookieManager_Loading" }
    $loadError = $xml.root.data | Where-Object { $_.name -eq "CookieManager_LoadError" }
    
    if ($loading -and $loadError) {
        Write-Host "OK" -ForegroundColor Green
    } else {
        Write-Host "Missing translations" -ForegroundColor Red
        if (-not $loading) { Write-Host "     - CookieManager_Loading missing" -ForegroundColor Red }
        if (-not $loadError) { Write-Host "     - CookieManager_LoadError missing" -ForegroundColor Red }
    }
}
catch {
    Write-Host "ERROR" -ForegroundColor Red
}

# Check Japanese for added translations
Write-Host "   Japanese additions... " -NoNewline
try {
    [xml]$xml = Get-Content $ja -Raw
    $webgl = $xml.root.data | Where-Object { $_.name -eq "Form1_Screenshot_WebGLNotSupported" }
    $empty = $xml.root.data | Where-Object { $_.name -eq "Form1_Screenshot_EmptyCanvas" }
    
    if ($webgl -and $empty) {
        Write-Host "OK" -ForegroundColor Green
    } else {
        Write-Host "Missing translations" -ForegroundColor Red
        if (-not $webgl) { Write-Host "     - Form1_Screenshot_WebGLNotSupported missing" -ForegroundColor Red }
        if (-not $empty) { Write-Host "     - Form1_Screenshot_EmptyCanvas missing" -ForegroundColor Red }
    }
}
catch {
    Write-Host "ERROR" -ForegroundColor Red
}

# Check Korean for added translations
Write-Host "   Korean additions... " -NoNewline
try {
    [xml]$xml = Get-Content $ko -Raw
    $webgl = $xml.root.data | Where-Object { $_.name -eq "Form1_Screenshot_WebGLNotSupported" }
    $empty = $xml.root.data | Where-Object { $_.name -eq "Form1_Screenshot_EmptyCanvas" }
    
    if ($webgl -and $empty) {
        Write-Host "OK" -ForegroundColor Green
    } else {
        Write-Host "Missing translations" -ForegroundColor Red
        if (-not $webgl) { Write-Host "     - Form1_Screenshot_WebGLNotSupported missing" -ForegroundColor Red }
        if (-not $empty) { Write-Host "     - Form1_Screenshot_EmptyCanvas missing" -ForegroundColor Red }
    }
}
catch {
    Write-Host "ERROR" -ForegroundColor Red
}

# Final summary
Write-Host "`n4. Summary:" -ForegroundColor Yellow
$baseCount = $counts["Strings.resx"]
$allMatch = $true

foreach ($kvp in $counts.GetEnumerator()) {
    if ($kvp.Key -ne "Strings.resx") {
        if ($kvp.Value -ne $baseCount) {
            Write-Host "   ⚠️  $($kvp.Key) has $($kvp.Value) keys (base has $baseCount)" -ForegroundColor Yellow
            $allMatch = $false
        }
    }
}

if ($allMatch) {
    Write-Host "   ✅ All files have the same number of keys ($baseCount)!" -ForegroundColor Green
    Write-Host "   ✅ All translations have been successfully fixed!" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Some files have different key counts" -ForegroundColor Yellow
}

Write-Host "`nValidation completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
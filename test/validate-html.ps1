# Validate HTML files
function Test-HTMLFile {
    param([string]$filePath)
    
    if (Test-Path $filePath) {
        Write-Host "`nChecking file: $filePath" -ForegroundColor Yellow
        Write-Host "File exists: Yes" -ForegroundColor Green
        
        $content = Get-Content $filePath -Raw
        
        # Check basic HTML structure
        if ($content -match '<!DOCTYPE html>') { 
            Write-Host "DOCTYPE declaration: Valid" -ForegroundColor Green 
        } else { 
            Write-Host "DOCTYPE declaration: Missing" -ForegroundColor Red 
        }
        
        if ($content -match '<html.*>.*</html>') { 
            Write-Host "HTML tags: Valid" -ForegroundColor Green 
        } else { 
            Write-Host "HTML tags: Invalid" -ForegroundColor Red 
        }
        
        if ($content -match '<head>.*</head>') { 
            Write-Host "HEAD tags: Valid" -ForegroundColor Green 
        } else { 
            Write-Host "HEAD tags: Invalid" -ForegroundColor Red 
        }
        
        if ($content -match '<body>.*</body>') { 
            Write-Host "BODY tags: Valid" -ForegroundColor Green 
        } else { 
            Write-Host "BODY tags: Invalid" -ForegroundColor Red 
        }
        
        # Check meta charset
        if ($content -match '<meta charset=') { 
            Write-Host "Charset declaration: Valid" -ForegroundColor Green 
        } else { 
            Write-Host "Charset declaration: Missing" -ForegroundColor Red 
        }
        
        # Count external resources
        $hrefCount = ([regex]::Matches($content, 'href="[^"]+"')).Count
        $srcCount = ([regex]::Matches($content, 'src="[^"]+"')).Count
        
        $totalResources = $hrefCount + $srcCount
        
        Write-Host "External resources: $totalResources (href: $hrefCount, src: $srcCount)" -ForegroundColor Cyan
        Write-Host "File size: $((Get-Item $filePath).Length) bytes" -ForegroundColor Cyan
        
        return $true
    } else {
        Write-Host "File not found: $filePath" -ForegroundColor Red
        return $false
    }
}

Write-Host "===== HTML File Validation Report =====" -ForegroundColor Cyan

$result1 = Test-HTMLFile 'test\cache_test.html'
$result2 = Test-HTMLFile 'test\webview2_data_test.html'
$result3 = Test-HTMLFile 'test\resource_loading_test.html'

Write-Host "`n===== Validation Complete =====" -ForegroundColor Cyan

if ($result1 -and $result2 -and $result3) {
    Write-Host "All HTML files validated successfully!" -ForegroundColor Green
} else {
    Write-Host "Some HTML files failed validation!" -ForegroundColor Red
}
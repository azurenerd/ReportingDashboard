# Static Asset Verification Script for Executive Dashboard
# Verifies all wwwroot files are present and properly configured

param(
    [string]$ProjectRoot = (Get-Location).Path
)

$wwwrootPath = Join-Path $ProjectRoot "wwwroot"
$errors = @()
$warnings = @()
$success = @()

# Define required files
$requiredFiles = @(
    "index.html",
    "css/base.css",
    "css/dashboard.css",
    "css/print.css",
    "js/dashboard.js",
    "js/print-handler.js"
)

# Define optional files
$optionalFiles = @(
    "data.json",
    "images/.gitkeep",
    "fonts/.gitkeep"
)

Write-Host "=== Executive Dashboard - Static Asset Verification ===" -ForegroundColor Cyan
Write-Host "Checking: $wwwrootPath" -ForegroundColor Gray
Write-Host ""

# Check wwwroot directory exists
if (-not (Test-Path $wwwrootPath)) {
    $errors += "wwwroot directory not found at: $wwwrootPath"
} else {
    $success += "wwwroot directory found"
}

# Check required files
foreach ($file in $requiredFiles) {
    $filePath = Join-Path $wwwrootPath $file
    if (Test-Path $filePath) {
        $fileSize = (Get-Item $filePath).Length
        $success += "✓ $file ($fileSize bytes)"
    } else {
        $errors += "✗ Missing required file: $file"
    }
}

# Check optional files
foreach ($file in $optionalFiles) {
    $filePath = Join-Path $wwwrootPath $file
    if (Test-Path $filePath) {
        $success += "✓ $file (optional)"
    } else {
        $warnings += "⚠ Optional file missing: $file"
    }
}

# Verify CSS file integrity
$cssFiles = @("css/base.css", "css/dashboard.css", "css/print.css")
foreach ($cssFile in $cssFiles) {
    $filePath = Join-Path $wwwrootPath $cssFile
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        if ($content -match "@media print") {
            $success += "✓ $cssFile contains print media queries"
        }
        if ($content -match "color-palette|--color") {
            $success += "✓ $cssFile contains CSS variables"
        }
    }
}

# Verify JavaScript file integrity
$jsFiles = @("js/dashboard.js", "js/print-handler.js")
foreach ($jsFile in $jsFiles) {
    $filePath = Join-Path $wwwrootPath $jsFile
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        if ($content -match "function|const|let|var") {
            $success += "✓ $jsFile contains valid JavaScript"
        }
    }
}

# Verify index.html references all assets
$indexPath = Join-Path $wwwrootPath "index.html"
if (Test-Path $indexPath) {
    $indexContent = Get-Content $indexPath -Raw
    $cssRefs = @("css/base.css", "css/dashboard.css", "css/print.css")
    $jsRefs = @("js/dashboard.js", "js/print-handler.js", "chart.js")
    
    foreach ($ref in $cssRefs) {
        if ($indexContent -match [regex]::Escape($ref)) {
            $success += "✓ index.html references $ref"
        } else {
            $warnings += "⚠ index.html does not reference $ref"
        }
    }
    
    foreach ($ref in $jsRefs) {
        if ($indexContent -match [regex]::Escape($ref)) {
            $success += "✓ index.html references $ref"
        } else {
            $warnings += "⚠ index.html does not reference $ref"
        }
    }
}

# Output results
Write-Host ""
Write-Host "=== RESULTS ===" -ForegroundColor Cyan
Write-Host ""

if ($success.Count -gt 0) {
    Write-Host "✓ Passed ($($success.Count)):" -ForegroundColor Green
    foreach ($item in $success) {
        Write-Host "  $item" -ForegroundColor Green
    }
}

Write-Host ""

if ($warnings.Count -gt 0) {
    Write-Host "⚠ Warnings ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($item in $warnings) {
        Write-Host "  $item" -ForegroundColor Yellow
    }
}

Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "✗ Errors ($($errors.Count)):" -ForegroundColor Red
    foreach ($item in $errors) {
        Write-Host "  $item" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "✓ All static assets verified successfully!" -ForegroundColor Green
    exit 0
}
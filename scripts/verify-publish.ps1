#!/usr/bin/env pwsh
# Verification script for self-contained executable publishing (Step 4 of 4)
#
# IMPORTANT: Run from the REPOSITORY ROOT directory:
#   .\scripts\verify-publish.ps1
#
# The script expects the following repo layout:
#   <repo-root>/src/ReportingDashboard/ReportingDashboard.csproj
#   <repo-root>/README.md

$ErrorActionPreference = "Stop"
$projectPath = "src/ReportingDashboard/ReportingDashboard.csproj"
$maxSizeMB = 100
$allPassed = $true

# Validate working directory
if (-not (Test-Path $projectPath)) {
    Write-Host "[ERROR] Cannot find $projectPath. This script must be run from the repository root." -ForegroundColor Red
    exit 1
}

# Dynamically resolve TargetFramework from the .csproj
$csprojXml = [xml](Get-Content $projectPath)
$tfm = $csprojXml.Project.PropertyGroup.TargetFramework | Where-Object { $_ } | Select-Object -First 1
if (-not $tfm) {
    Write-Host "[ERROR] Could not resolve TargetFramework from $projectPath" -ForegroundColor Red
    exit 1
}
Write-Host "Resolved TargetFramework: $tfm" -ForegroundColor Cyan

$rid = "win-x64"

function Write-Check($name, $passed, $detail) {
    $icon = if ($passed) { "[PASS]" } else { "[FAIL]"; $script:allPassed = $false }
    Write-Host "$icon $name" -ForegroundColor $(if ($passed) { "Green" } else { "Red" })
    if ($detail) { Write-Host "       $detail" -ForegroundColor Gray }
}

# Extracts the publish output directory from dotnet publish stdout.
# MSBuild emits a line like: "ReportingDashboard -> C:\...\publish\"
function Get-PublishDir($publishOutput) {
    $match = [regex]::Match($publishOutput, '->\s*(.+[/\\]publish[/\\]?)\s*$', 'Multiline')
    if ($match.Success) {
        return $match.Groups[1].Value.Trim().TrimEnd('\', '/')
    }
    # Fallback: derive from conventions
    $fallback = "src/ReportingDashboard/bin/Release/$tfm/$rid/publish"
    Write-Host "  [WARN] Could not parse publish path from output; using fallback: $fallback" -ForegroundColor Yellow
    return $fallback
}

# 1. Verify dotnet build succeeds
Write-Host "`n=== Build Regression Check ===" -ForegroundColor Cyan
$buildOutput = & dotnet build $projectPath -c Release --nologo 2>&1 | Out-String
$buildExitCode = $LASTEXITCODE
Write-Check "dotnet build succeeds" ($buildExitCode -eq 0) "Exit code: $buildExitCode"

# Check no new PackageReference entries
$csproj = Get-Content $projectPath -Raw
$hasPackageRefs = $csproj -match "<PackageReference"
Write-Check "No external NuGet packages" (-not $hasPackageRefs) $(if ($hasPackageRefs) { "Found <PackageReference> in .csproj" } else { "Zero <PackageReference> entries" })

# Check ExcludeFromSingleFile is present for data.json
$hasExclude = $csproj -match 'ExcludeFromSingleFile\s*=\s*"true"'
Write-Check "data.json ExcludeFromSingleFile in .csproj" $hasExclude $(if ($hasExclude) { "ExcludeFromSingleFile='true' found" } else { "MISSING: data.json will be embedded in single-file exe" })

# Check Condition guard on data.json Content item
$hasCondition = $csproj -match "Condition\s*=\s*`"Exists\('data\.json'\)`""
Write-Check "data.json Content has Exists condition" $hasCondition $(if ($hasCondition) { "Condition guard present" } else { "MISSING: build may warn when data.json doesn't exist" })

# 2. Run publish (full CLI)
Write-Host "`n=== Publish Verification (CLI) ===" -ForegroundColor Cyan
$publishOutput = & dotnet publish $projectPath -c Release -r $rid --self-contained true -p:PublishSingleFile=true --nologo 2>&1 | Out-String
$publishExitCode = $LASTEXITCODE
Write-Check "dotnet publish succeeds" ($publishExitCode -eq 0) "Exit code: $publishExitCode"

if ($publishExitCode -ne 0) {
    Write-Host "`nPublish output:`n$publishOutput" -ForegroundColor Yellow
    exit 1
}

# Resolve the actual publish directory from MSBuild output
$publishDir = Get-PublishDir $publishOutput
Write-Host "Resolved publish directory: $publishDir" -ForegroundColor Cyan

# 3. Check executable exists and size
if (Test-Path $publishDir) {
    $exe = Get-ChildItem $publishDir -Filter "*.exe" | Select-Object -First 1
} else {
    $exe = $null
}
$exeExists = $null -ne $exe
Write-Check "Single executable exists" $exeExists $(if ($exeExists) { $exe.Name } else { "No .exe found in $publishDir" })

if ($exeExists) {
    $sizeMB = [math]::Round($exe.Length / 1MB, 2)
    $sizeOk = $sizeMB -lt $maxSizeMB
    Write-Check "Executable under ${maxSizeMB}MB" $sizeOk "${sizeMB}MB"
}

# 4. Check data.json is external (not embedded)
# Note: data.json is created by a separate task (#677). If it exists in the project,
# it should appear alongside the exe. If it doesn't exist yet, this check is skipped.
$dataJsonInProject = Test-Path "src/ReportingDashboard/data.json"
if ($dataJsonInProject) {
    $dataJsonPath = Join-Path $publishDir "data.json"
    $dataJsonExists = Test-Path $dataJsonPath
    Write-Check "data.json exists alongside exe (not embedded)" $dataJsonExists $(if ($dataJsonExists) { "Found: $dataJsonPath" } else { "Missing: $dataJsonPath" })
} else {
    Write-Host "[SKIP] data.json not yet in project (created by dependency task #677)" -ForegroundColor Yellow
}

# 5. Verify publish profile shorthand works
Write-Host "`n=== Publish Profile Verification ===" -ForegroundColor Cyan
$profileOutput = & dotnet publish $projectPath -p:PublishProfile=win-x64 --nologo 2>&1 | Out-String
$profileExitCode = $LASTEXITCODE
Write-Check "dotnet publish via PublishProfile succeeds" ($profileExitCode -eq 0) "Exit code: $profileExitCode"

if ($profileExitCode -eq 0) {
    $profilePublishDir = Get-PublishDir $profileOutput
    Write-Host "Profile publish directory: $profilePublishDir" -ForegroundColor Cyan
}

# 6. List publish directory contents
Write-Host "`n=== Publish Directory Contents ===" -ForegroundColor Cyan
if (Test-Path $publishDir) {
    Get-ChildItem $publishDir | ForEach-Object {
        $sizeMB = [math]::Round($_.Length / 1MB, 2)
        Write-Host "  $($_.Name) ($sizeMB MB)" -ForegroundColor Gray
    }
} else {
    Write-Host "  Publish directory not found: $publishDir" -ForegroundColor Yellow
}

# 7. Check pubxml exists
$pubxmlPath = "src/ReportingDashboard/Properties/PublishProfiles/win-x64.pubxml"
Write-Check "win-x64.pubxml exists" (Test-Path $pubxmlPath) $pubxmlPath

# 8. Check README exists
Write-Check "README.md exists at repo root" (Test-Path "README.md") "README.md"

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
if ($allPassed) {
    Write-Host "ALL CHECKS PASSED" -ForegroundColor Green
    exit 0
} else {
    Write-Host "SOME CHECKS FAILED - review output above" -ForegroundColor Red
    exit 1
}
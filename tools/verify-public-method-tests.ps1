param(
    [string]$Root = ".",
    [string[]]$CriticalFiles = @("BasicTypeConverter.cs", "CollectionConvertor.cs", "HolidayHelper.cs")
)

$ErrorActionPreference = "Stop"

$rootPath = Resolve-Path $Root
$prodDir = Join-Path $rootPath "Transformations"
$testDir = Join-Path $rootPath "Transformations.Tests"

if (-not (Test-Path $prodDir) -or -not (Test-Path $testDir)) {
    throw "Expected Transformations and Transformations.Tests directories under '$rootPath'."
}

$testContent = (Get-ChildItem $testDir -Filter "*.cs" -Recurse |
    Where-Object { $_.Name -notlike "*.AssemblyInfo.cs" -and $_.Name -notlike "*.GlobalUsings*" -and $_.Name -notlike "Microsoft.NET.Test.Sdk.Program.cs" } |
    Get-Content -Raw) -join "`n"

$uncovered = @()

foreach ($fileName in $CriticalFiles) {
    $filePath = Join-Path $prodDir $fileName
    if (-not (Test-Path $filePath)) {
        throw "Critical file not found: $filePath"
    }

    $methods = Select-String -Path $filePath -Pattern "public static\s+[^\(]+\s+(\w+)\s*\(" |
        ForEach-Object { $_.Matches[0].Groups[1].Value } |
        Where-Object { $_ -ne "class" } |
        Sort-Object -Unique

    foreach ($method in $methods) {
        if ($testContent -notmatch "\b$([regex]::Escape($method))\b") {
            $uncovered += "$fileName::$method"
        }
    }
}

if ($uncovered.Count -gt 0) {
    Write-Host "❌ Public method test policy failed. Uncovered methods in critical files:"
    $uncovered | ForEach-Object { Write-Host "  - $_" }
    exit 1
}

Write-Host "✅ Public method test policy passed for critical files: $($CriticalFiles -join ', ')"
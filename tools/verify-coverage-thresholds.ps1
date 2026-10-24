param(
    [string]$CoverageReportPath,
    [double]$LineThreshold = 65.0,
    [double]$BranchThreshold = 55.0,
    # Optional floor for one package (assembly) in the report, checked in addition to the totals.
    [string]$PackageName,
    [double]$PackageLineThreshold = 0.0,
    [double]$PackageBranchThreshold = 0.0
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $CoverageReportPath)) {
    throw "Coverage report not found at '$CoverageReportPath'."
}

[xml]$coverage = Get-Content $CoverageReportPath

if (-not $coverage.coverage) {
    throw "Invalid cobertura XML format."
}

$lineRate = [double]$coverage.coverage.'line-rate' * 100
$branchRate = [double]$coverage.coverage.'branch-rate' * 100

Write-Host "Line coverage:   $([math]::Round($lineRate,2))%"
Write-Host "Branch coverage: $([math]::Round($branchRate,2))%"
Write-Host "Required line threshold:   $LineThreshold%"
Write-Host "Required branch threshold: $BranchThreshold%"

$failed = $lineRate -lt $LineThreshold -or $branchRate -lt $BranchThreshold

if ($PackageName) {
    $package = $coverage.coverage.packages.package | Where-Object { $_.name -eq $PackageName } | Select-Object -First 1
    if (-not $package) {
        throw "Package '$PackageName' not found in the coverage report."
    }
    $packageLine = [double]$package.'line-rate' * 100
    $packageBranch = [double]$package.'branch-rate' * 100
    Write-Host "$PackageName line coverage:   $([math]::Round($packageLine,2))% (required $PackageLineThreshold%)"
    Write-Host "$PackageName branch coverage: $([math]::Round($packageBranch,2))% (required $PackageBranchThreshold%)"
    $failed = $failed -or $packageLine -lt $PackageLineThreshold -or $packageBranch -lt $PackageBranchThreshold
}

if ($failed) {
    Write-Host "❌ Coverage gate failed."
    exit 1
}

Write-Host "✅ Coverage gate passed."

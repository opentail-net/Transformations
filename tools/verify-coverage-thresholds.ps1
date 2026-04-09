param(
    [string]$CoverageReportPath,
    [double]$LineThreshold = 65.0,
    [double]$BranchThreshold = 55.0
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

if ($lineRate -lt $LineThreshold -or $branchRate -lt $BranchThreshold) {
    Write-Host "❌ Coverage gate failed."
    exit 1
}

Write-Host "✅ Coverage gate passed."
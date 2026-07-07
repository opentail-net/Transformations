param(
    [string]$Framework = "net10.0",
    [double]$MinimumSnrDb = 12.0,
    [double]$MinimumPsnrDb = 18.0,
    [double]$MaximumMeanSquaredError = 0.03,
    [int]$MaximumFrameCountError = 2,
    [ValidateSet("Default", "RealLike", "All")]
    [string]$Suite = "Default",
    [switch]$RatioThresholdSweep,
    [string[]]$Threshold,
    [switch]$StableOnly,
    [string[]]$Algorithm,
    [string]$Output = "$PSScriptRoot\fidelity-report.md",
    [string]$JsonOutput = "",
    [string]$CsvOutput = ""
)

$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\Transformations.Audio.Fidelity\Transformations.Audio.Fidelity.csproj"
$arguments = @(
    "run",
    "--project", $project,
    "--framework", $Framework,
    "--",
    "--min-snr", $MinimumSnrDb.ToString([Globalization.CultureInfo]::InvariantCulture),
    "--min-psnr", $MinimumPsnrDb.ToString([Globalization.CultureInfo]::InvariantCulture),
    "--max-mse", $MaximumMeanSquaredError.ToString([Globalization.CultureInfo]::InvariantCulture),
    "--max-frame-error", $MaximumFrameCountError.ToString([Globalization.CultureInfo]::InvariantCulture),
    "--suite", $Suite,
    "--output", $Output
)

if ($StableOnly) {
    $arguments += "--stable-only"
}

if ($RatioThresholdSweep) {
    $arguments += "--ratio-threshold-sweep"
}

foreach ($thresholdValue in $Threshold) {
    foreach ($part in $thresholdValue.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries -bor [System.StringSplitOptions]::TrimEntries)) {
        $arguments += @("--threshold", $part)
    }
}

foreach ($algorithmId in $Algorithm) {
    $arguments += @("--algorithm", $algorithmId)
}

if (-not [string]::IsNullOrWhiteSpace($JsonOutput)) {
    $arguments += @("--json-output", $JsonOutput)
}

if (-not [string]::IsNullOrWhiteSpace($CsvOutput)) {
    $arguments += @("--csv-output", $CsvOutput)
}

dotnet @arguments
exit $LASTEXITCODE

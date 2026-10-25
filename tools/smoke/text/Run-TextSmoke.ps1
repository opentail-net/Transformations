<#
.SYNOPSIS
    Smoke-tests the packed Transformations.Text package against the committed corpus, from a clean consumer.

.DESCRIPTION
    Packs Transformations.Text from the working tree into a fresh local feed, checks the package contents and
    dependencies, verifies the corpus manifest, clears the NuGet cache entries for that version, then builds and runs
    the consumer in consumer/ for each target framework. The consumer reads corpus/manifest.json and the sample files,
    and checks each sample against its label. A dated report is written to reports/.

    Samples are defined in code (Transformations.TestDocuments). To add one, see README.md.

.PARAMETER TargetFrameworks
    Consumer target frameworks to run. Each must have an installed runtime and targeting pack.

.PARAMETER WorkRoot
    Where run folders are written. Kept outside the repository so the consumer never sits under the repo's
    Directory.Build.props.

.EXAMPLE
    pwsh ./tools/smoke/text/Run-TextSmoke.ps1
#>
[CmdletBinding()]
param(
    [string[]]$TargetFrameworks = @('net8.0', 'net9.0', 'net10.0'),
    [string]$WorkRoot = (Join-Path $env:LOCALAPPDATA 'TransformationsSmoke')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$smokeRoot = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$corpusDir = Join-Path $smokeRoot 'corpus'
$reportDir = Join-Path $smokeRoot 'reports'

function Write-Step([string]$Text) {
    Write-Host "==> $Text" -ForegroundColor Cyan
}

function New-Check([string]$Name, [bool]$Passed, [string]$Detail = '') {
    [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

# ---------------------------------------------------------------------------------------------------------------------
# Identify the candidate
# ---------------------------------------------------------------------------------------------------------------------

$propsText = Get-Content -Raw (Join-Path $repoRoot 'Directory.Build.props')
$version = [regex]::Match($propsText, '<Version>([^<]+)</Version>').Groups[1].Value
if (-not $version) { throw 'Could not read <Version> from Directory.Build.props.' }

# NuGet drops a zero revision from file names, nuspec versions and cache folders (3.0.0.0 is stored as 3.0.0).
$parsedVersion = [version]$version
$packageVersion = if ($parsedVersion.Revision -eq 0) { "$($parsedVersion.Major).$($parsedVersion.Minor).$($parsedVersion.Build)" } else { $version }

$commit = (git -C $repoRoot rev-parse HEAD).Trim()
$shortCommit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$dirtyFiles = @(git -C $repoRoot status --porcelain --untracked-files=no)
$isDirty = $dirtyFiles.Count -gt 0

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $WorkRoot "text-runs\$stamp-$version-$shortCommit"
$feedDir = Join-Path $runDir 'feed'
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $feedDir, $logDir | Out-Null

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"
Write-Step "Run folder $runDir"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Pack
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Packing Transformations.Text (Release)'
& dotnet pack (Join-Path $repoRoot 'Transformations.TextExtractor\Transformations.Text.csproj') -c Release -o $feedDir *> (Join-Path $logDir 'pack-text.log')
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed. See $logDir." }

$textPackage = Join-Path $feedDir "Transformations.Text.$packageVersion.nupkg"

# ---------------------------------------------------------------------------------------------------------------------
# 2. Package contents and dependencies
# ---------------------------------------------------------------------------------------------------------------------

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageChecks = @()

$zip = [System.IO.Compression.ZipFile]::OpenRead($textPackage)
try {
    $entryNames = @($zip.Entries | ForEach-Object { $_.FullName })
    $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($nuspecEntry.Open())
    try { $nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
}
finally {
    $zip.Dispose()
}

foreach ($tfm in 'net8.0', 'net9.0', 'net10.0') {
    $entry = "lib/$tfm/Transformations.Text.dll"
    $packageChecks += New-Check "package contains $entry" ($entryNames -contains $entry)
}
$packageChecks += New-Check 'package contains README.md' ($entryNames -contains 'README.md')
$packageChecks += New-Check 'package contains LICENSE.md at its root' ($entryNames -contains 'LICENSE.md')
$packageChecks += New-Check 'nuspec declares the licence file' ($nuspec -match '<license type="file">LICENSE.md</license>')
$packageChecks += New-Check "nuspec version is $packageVersion" ($nuspec -match "<version>$([regex]::Escape($packageVersion))</version>")
$packageChecks += New-Check 'nuspec depends on TesseractOCR (native OCR binaries come through it)' ($nuspec -match 'dependency id="TesseractOCR"')
$packageChecks += New-Check 'nuspec depends on Microsoft.Extensions.DependencyInjection.Abstractions' ($nuspec -match 'dependency id="Microsoft.Extensions.DependencyInjection.Abstractions"')

$textSha256 = (Get-FileHash $textPackage -Algorithm SHA256).Hash
$textSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($textPackage)))

# ---------------------------------------------------------------------------------------------------------------------
# 3. Corpus manifest: every listed file must exist, and every label must be one the consumer knows
# ---------------------------------------------------------------------------------------------------------------------

$manifest = Get-Content -Raw (Join-Path $corpusDir 'manifest.json') | ConvertFrom-Json
$knownLabels = 'Ordinary', 'Malicious', 'KnownGap', 'Benign', 'Malformed', 'Ocr'
$samples = @($manifest.samples)
$missingFiles = @($samples | Where-Object { -not (Test-Path (Join-Path $corpusDir $_.file)) })
$badLabels = @($samples | Where-Object { $_.label -notin $knownLabels })
$packageChecks += New-Check "corpus manifest lists $($samples.Count) samples" ($samples.Count -gt 0) ''
$packageChecks += New-Check 'every corpus sample file exists' ($missingFiles.Count -eq 0) (($missingFiles | ForEach-Object { $_.file }) -join ', ')
$packageChecks += New-Check 'every corpus label is known' ($badLabels.Count -eq 0) (($badLabels | ForEach-Object { "$($_.id)=$($_.label)" }) -join ', ')

# ---------------------------------------------------------------------------------------------------------------------
# 4. Clear NuGet cache entries for this version, so restore cannot reuse an older copy of the same version
# ---------------------------------------------------------------------------------------------------------------------

$globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$clearedCache = @()
$cacheDir = Join-Path $globalPackages "transformations.text\$packageVersion"
if (Test-Path $cacheDir) {
    Remove-Item -Recurse -Force $cacheDir
    $clearedCache += $cacheDir
}
Write-Step "Cleared $($clearedCache.Count) cache entries for $version"

# ---------------------------------------------------------------------------------------------------------------------
# 5. Feed configuration: Transformations* packages come only from the local feed
# ---------------------------------------------------------------------------------------------------------------------

$nugetConfig = Join-Path $runDir 'nuget.config'
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="smoke-local" value="$feedDir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="smoke-local">
      <package pattern="Transformations*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content -Path $nugetConfig -Encoding utf8

# ---------------------------------------------------------------------------------------------------------------------
# 6. Consumer per target framework
# ---------------------------------------------------------------------------------------------------------------------

$tessdataDir = Join-Path $WorkRoot 'tessdata'
if (Test-Path (Join-Path $tessdataDir 'eng.traineddata')) {
    $env:TRANSFORMATIONS_TESSDATA = $tessdataDir
    Write-Step "Trained data: $tessdataDir"
} else {
    Remove-Item Env:TRANSFORMATIONS_TESSDATA -ErrorAction SilentlyContinue
    Write-Step 'No trained data at the stable path: the Tesseract rows will be skipped'
}

$consumerResults = @()

foreach ($tfm in $TargetFrameworks) {
    Write-Step "Consumer $tfm"
    $tfmDir = Join-Path $runDir $tfm
    $consumerDir = Join-Path $tfmDir 'consumer'
    New-Item -ItemType Directory -Force -Path $tfmDir | Out-Null
    Copy-Item -Recurse (Join-Path $smokeRoot 'consumer') $consumerDir
    $properties = @("-p:SmokeTfm=$tfm", "-p:SmokeTextVersion=$version")

    & dotnet restore (Join-Path $consumerDir 'Consumer.csproj') @properties --force --configfile $nugetConfig *> (Join-Path $logDir "$tfm-restore.log")
    $restoreCode = $LASTEXITCODE

    $resolvedOk = $false
    $resolvedNote = 'restore failed'
    if ($restoreCode -eq 0) {
        $assets = Get-Content -Raw (Join-Path $consumerDir 'obj\project.assets.json') | ConvertFrom-Json
        $textLib = $assets.libraries.PSObject.Properties["Transformations.Text/$packageVersion"].Value
        $resolvedOk = $textLib.sha512 -eq $textSha512
        $resolvedNote = if ($resolvedOk) { 'restored package matches the packed .nupkg (SHA-512)' } else { 'restored package hash differs from the packed .nupkg' }
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE
    $buildWarnings = @(Select-String -Path $buildLog -Pattern ': warning ').Count

    $runLog = Join-Path $logDir "$tfm-run.log"
    $resultsFile = Join-Path $tfmDir 'results.json'
    $results = @()
    $nativeOcr = @(Get-ChildItem (Join-Path $consumerDir 'bin') -Recurse -Filter '*.dll' -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^(tesseract|leptonica)' } | ForEach-Object { $_.Name } | Sort-Object -Unique)
    $runCode = -1
    if ($buildCode -eq 0) {
        & dotnet run --project (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-build @properties -- --corpus $corpusDir --results $resultsFile *> $runLog
        $runCode = $LASTEXITCODE
        if (Test-Path $resultsFile) { $results = @(Get-Content -Raw $resultsFile | ConvertFrom-Json) }
    }

    $consumerResults += [pscustomobject]@{
        Tfm           = $tfm
        NativeOcr     = $nativeOcr
        RestoreCode   = $restoreCode
        ResolvedOk    = $resolvedOk
        ResolvedNote  = $resolvedNote
        BuildCode     = $buildCode
        BuildWarnings = $buildWarnings
        RunCode       = $runCode
        Results       = $results
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 7. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$packagePassed = @($packageChecks | Where-Object { -not $_.Passed }).Count -eq 0
$consumerPassed = @($consumerResults | Where-Object {
        -not ($_.RestoreCode -eq 0 -and $_.ResolvedOk -and $_.BuildCode -eq 0 -and $_.BuildWarnings -eq 0 -and $_.RunCode -eq 0)
    }).Count -eq 0
$allChecksPassed = @($consumerResults | ForEach-Object { $_.Results } | Where-Object { -not $_.Passed -and -not $_.Skipped }).Count -eq 0
$nativeOk = @($consumerResults | Where-Object { $_.NativeOcr.Count -eq 0 }).Count -eq 0
$verdict = if ($packagePassed -and $consumerPassed -and $allChecksPassed -and $nativeOk) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations.Text smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version")
$lines.Add("- Commit: $commit")
$lines.Add("- Working tree dirty (tracked files): $isDirty$(if ($isDirty) { ' (' + ($dirtyFiles -join '; ') + ')' })")
$lines.Add("- Run: $stamp")
$lines.Add("- Transformations.Text .nupkg SHA-256: $textSha256")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ')")
$lines.Add("- Corpus: $($samples.Count) samples in $corpusDir")
$lines.Add("- Run folder (logs and feed): $runDir")
$lines.Add('')
$lines.Add('## Package')
$lines.Add('')
$lines.Add('| Check | Result | Detail |')
$lines.Add('|---|---|---|')
foreach ($check in $packageChecks) {
    $lines.Add("| $($check.Name) | $(if ($check.Passed) { 'PASS' } else { 'FAIL' }) | $($check.Detail) |")
}
$lines.Add('')
$lines.Add('## Consumer per target framework')
$lines.Add('')
$lines.Add('| Target | Restore | Package hash | Build | Warnings | Run | Checks passed |')
$lines.Add('|---|---|---|---|---|---|---|')
foreach ($row in $consumerResults) {
    $passedCount = @($row.Results | Where-Object { $_.Passed }).Count
    $skippedCount = @($row.Results | Where-Object { $_.Skipped }).Count
    $lines.Add("| $($row.Tfm) | $(if ($row.RestoreCode -eq 0) { 'PASS' } else { 'FAIL' }) | $(if ($row.ResolvedOk) { 'PASS' } else { 'FAIL' }) | $(if ($row.BuildCode -eq 0) { 'PASS' } else { 'FAIL' }) | $($row.BuildWarnings) | $(if ($row.RunCode -eq 0) { 'PASS' } else { 'FAIL' }) | $passedCount / $($row.Results.Count) ($skippedCount skipped) |")
}
$lines.Add('')

# One row per check, one column per framework, so a failure shows which frameworks it affects.
$lines.Add('## Checks')
$lines.Add('')
$header = '| Check | ' + (($consumerResults | ForEach-Object { $_.Tfm }) -join ' | ') + ' | ms |'
$lines.Add($header)
$lines.Add('|---' * ($consumerResults.Count + 2) + '|')
$checkNames = @($consumerResults[0].Results | ForEach-Object { $_.Name })
foreach ($name in $checkNames) {
    $cells = foreach ($row in $consumerResults) {
        $match = $row.Results | Where-Object { $_.Name -eq $name } | Select-Object -First 1
        if ($null -eq $match) { '-' } elseif ($match.Skipped) { 'SKIP' } elseif ($match.Passed) { 'PASS' } else { 'FAIL' }
    }
    $ms = ($consumerResults[0].Results | Where-Object { $_.Name -eq $name } | Select-Object -First 1).ElapsedMs
    $lines.Add("| $($name -replace '\|', '\|') | $($cells -join ' | ') | $ms |")
}
$lines.Add('')

$failures = @()
foreach ($row in $consumerResults) {
    if ($row.BuildCode -ne 0) { $failures += "- **$($row.Tfm)** build failed: see logs/$($row.Tfm)-build.log" }
    if ($row.BuildCode -eq 0 -and $row.RunCode -ne 0) { $failures += "- **$($row.Tfm)** run failed (exit $($row.RunCode)): see logs/$($row.Tfm)-run.log" }
}
foreach ($row in $consumerResults) {
    foreach ($result in @($row.Results | Where-Object { -not $_.Passed -and -not $_.Skipped })) {
        $failures += "- **$($row.Tfm)** $($result.Name): $($result.Detail -replace '\|', '\|')"
    }
}
$lines.Add('## Failures')
$lines.Add('')
if ($failures.Count -eq 0) { $lines.Add('None.') } else { $failures | ForEach-Object { $lines.Add($_) } }
$lines.Add('')
$skips = @()
foreach ($row in $consumerResults) { foreach ($result in @($row.Results | Where-Object { $_.Skipped })) { $skips += "- **$($row.Tfm)** $($result.Name): $($result.Detail)" } }
$lines.Add('## Skipped (did not run)')
$lines.Add('')
if ($skips.Count -eq 0) { $lines.Add('None.') } else { $skips | ForEach-Object { $lines.Add($_) } }
$lines.Add('')
$lines.Add('## Native OCR binaries in the consumer output')
$lines.Add('')
foreach ($row in $consumerResults) { $lines.Add("- **$($row.Tfm)**: $(if ($row.NativeOcr.Count -eq 0) { 'none found' } else { $row.NativeOcr -join ', ' })") }
$lines.Add('')
$lines.Add('## Cache entries cleared before restore')
$lines.Add('')
if ($clearedCache.Count -eq 0) { $lines.Add('None were present.') }
foreach ($path in $clearedCache) { $lines.Add("- $path") }

$lines | Set-Content -Path $reportPath -Encoding utf8

Write-Host ''
Write-Host "Verdict: $verdict" -ForegroundColor $(if ($verdict -eq 'PASS') { 'Green' } else { 'Red' })
Write-Host "Report:  $reportPath"

if ($verdict -eq 'PASS') { exit 0 } else { exit 1 }

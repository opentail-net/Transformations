<#
.SYNOPSIS
    Checks that the real Tesseract engine reads adversarial text in an image, and that the safety gate holds it back.

.DESCRIPTION
    Packs Transformations.Text from the working tree, checks the package contents, and clears the NuGet cache entries for
    the version. It then builds and runs the consumer in consumer/ for each target framework, with TRANSFORMATIONS_TESSDATA
    set to the trained-data folder. The consumer runs Tesseract on each rendered image in corpus/ and checks the words it
    reads, the OCR labelling, and the gate's decision for adversarial and benign images.

    Without trained data the run does not happen: the verdict is NOT RUN and the script exits 3. It never reports a pass
    for a test that did not run.

.PARAMETER TargetFrameworks
    Consumer target frameworks to run.

.PARAMETER WorkRoot
    Where run folders are written, and where the trained data is expected (WorkRoot\tessdata\eng.traineddata).

.EXAMPLE
    pwsh ./tools/smoke/ocr-injection/Run-OcrInjectionSmoke.ps1
#>
[CmdletBinding()]
param(
    [string[]]$TargetFrameworks = @('net8.0', 'net10.0'),
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
$parsedVersion = [version]$version
$packageVersion = if ($parsedVersion.Revision -eq 0) { "$($parsedVersion.Major).$($parsedVersion.Minor).$($parsedVersion.Build)" } else { $version }

$commit = (git -C $repoRoot rev-parse HEAD).Trim()
$shortCommit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$dirtyFiles = @(git -C $repoRoot status --porcelain --untracked-files=no)
$isDirty = $dirtyFiles.Count -gt 0

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$tessdataDir = Join-Path $WorkRoot 'tessdata'
$trainedDataPresent = Test-Path (Join-Path $tessdataDir 'eng.traineddata')
$platformOk = [Environment]::Is64BitProcess -and $IsWindows

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"

# Without trained data or a supported platform, the test cannot run. Write a NOT RUN report and stop.
if (-not $trainedDataPresent -or -not $platformOk) {
    $reason = if (-not $platformOk) { 'this is not a Windows x64 process' } else { "no eng.traineddata at $tessdataDir" }
    New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
    $reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"
    @(
        '# Transformations OCR injection smoke test: NOT RUN'
        ''
        "- Package version: $version"
        "- Commit: $commit (dirty: $isDirty)"
        "- Run: $stamp"
        "- Reason: $reason"
        ''
        'Nothing was tested. The Tesseract engine needs trained data and Windows x64 or x86.'
    ) | Set-Content -Path $reportPath -Encoding utf8
    Write-Host ''
    Write-Host "Verdict: NOT RUN ($reason)" -ForegroundColor Yellow
    Write-Host "Report:  $reportPath"
    exit 3
}

$runDir = Join-Path $WorkRoot "ocr-injection-runs\$stamp-$version-$shortCommit"
$feedDir = Join-Path $runDir 'feed'
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $feedDir, $logDir | Out-Null
Write-Step "Run folder $runDir"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Pack Transformations.Text
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Packing Transformations.Text (Release)'
& dotnet pack (Join-Path $repoRoot 'Transformations.TextExtractor\Transformations.Text.csproj') -c Release -o $feedDir *> (Join-Path $logDir 'pack-text.log')
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed. See $logDir." }

$textPackage = Join-Path $feedDir "Transformations.Text.$packageVersion.nupkg"
Add-Type -AssemblyName System.IO.Compression.FileSystem
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

$packageChecks = @(
    New-Check 'package contains lib/net8.0 and lib/net10.0' (($entryNames -contains 'lib/net8.0/Transformations.Text.dll') -and ($entryNames -contains 'lib/net10.0/Transformations.Text.dll'))
    New-Check 'nuspec depends on TesseractOCR (native binaries come through it)' ($nuspec -match 'dependency id="TesseractOCR"')
)
$textSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($textPackage)))

# ---------------------------------------------------------------------------------------------------------------------
# 2. Clear NuGet cache entries for this version
# ---------------------------------------------------------------------------------------------------------------------

$globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$clearedCache = @()
$cacheDir = Join-Path $globalPackages "transformations.text\$packageVersion"
if (Test-Path $cacheDir) {
    Remove-Item -Recurse -Force $cacheDir
    $clearedCache += $cacheDir
}

# ---------------------------------------------------------------------------------------------------------------------
# 3. Feed configuration
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
# 4. Consumer per target framework
# ---------------------------------------------------------------------------------------------------------------------

$env:TRANSFORMATIONS_TESSDATA = $tessdataDir

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
    if ($restoreCode -eq 0) {
        $assets = Get-Content -Raw (Join-Path $consumerDir 'obj\project.assets.json') | ConvertFrom-Json
        $textLib = $assets.libraries.PSObject.Properties["Transformations.Text/$packageVersion"].Value
        $resolvedOk = $textLib.sha512 -eq $textSha512
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE

    $nativeOcr = @(Get-ChildItem (Join-Path $consumerDir 'bin') -Recurse -Filter '*.dll' -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^(tesseract|leptonica)' } | ForEach-Object { $_.Name } | Sort-Object -Unique)

    $runLog = Join-Path $logDir "$tfm-run.log"
    $resultsFile = Join-Path $tfmDir 'results.json'
    $results = @()
    $runCode = -1
    if ($buildCode -eq 0) {
        & dotnet run --project (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-build @properties -- --corpus $corpusDir --results $resultsFile *> $runLog
        $runCode = $LASTEXITCODE
        if (Test-Path $resultsFile) { $results = @(Get-Content -Raw $resultsFile | ConvertFrom-Json) }
    }

    $consumerResults += [pscustomobject]@{
        Tfm         = $tfm
        RestoreCode = $restoreCode
        ResolvedOk  = $resolvedOk
        BuildCode   = $buildCode
        NativeOcr   = $nativeOcr
        RunCode     = $runCode
        Results     = $results
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 5. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$allChecks = @($packageChecks) + @($consumerResults | ForEach-Object { $_.Results })
$consumersOk = @($consumerResults | Where-Object {
        -not ($_.RestoreCode -eq 0 -and $_.ResolvedOk -and $_.BuildCode -eq 0 -and $_.RunCode -eq 0 -and $_.NativeOcr.Count -gt 0)
    }).Count -eq 0
$verdict = if ($consumersOk -and @($allChecks | Where-Object { -not $_.Passed }).Count -eq 0) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations OCR injection smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version")
$lines.Add("- Commit: $commit")
$lines.Add("- Working tree dirty (tracked files): $isDirty$(if ($isDirty) { ' (' + ($dirtyFiles -join '; ') + ')' })")
$lines.Add("- Run: $stamp")
$lines.Add("- Trained data: $tessdataDir")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ')")
$lines.Add('')
$lines.Add('## Package')
$lines.Add('')
$lines.Add('| Check | Result | Detail |')
$lines.Add('|---|---|---|')
foreach ($check in $packageChecks) { $lines.Add("| $($check.Name) | $(if ($check.Passed) { 'PASS' } else { 'FAIL' }) | $($check.Detail) |") }
$lines.Add('')
$lines.Add('## Consumer per target framework')
$lines.Add('')
$lines.Add('| Target | Restore | Package hash | Build | Native OCR binaries | Run | Checks passed |')
$lines.Add('|---|---|---|---|---|---|---|')
foreach ($row in $consumerResults) {
    $passedCount = @($row.Results | Where-Object { $_.Passed }).Count
    $lines.Add("| $($row.Tfm) | $(if ($row.RestoreCode -eq 0) { 'PASS' } else { 'FAIL' }) | $(if ($row.ResolvedOk) { 'PASS' } else { 'FAIL' }) | $(if ($row.BuildCode -eq 0) { 'PASS' } else { 'FAIL' }) | $(if ($row.NativeOcr.Count -gt 0) { $row.NativeOcr -join ', ' } else { 'none found' }) | $(if ($row.RunCode -eq 0) { 'PASS' } else { 'FAIL' }) | $passedCount / $($row.Results.Count) |")
}
$lines.Add('')
$lines.Add('## Checks')
$lines.Add('')
$lines.Add('| Check | ' + (($consumerResults | ForEach-Object { $_.Tfm }) -join ' | ') + ' |')
$lines.Add('|---' * ($consumerResults.Count + 1) + '|')
$checkNames = @($consumerResults[0].Results | ForEach-Object { $_.Name })
foreach ($name in $checkNames) {
    $cells = foreach ($row in $consumerResults) {
        $match = $row.Results | Where-Object { $_.Name -eq $name } | Select-Object -First 1
        if ($null -eq $match) { '-' } elseif ($match.Passed) { 'PASS' } else { 'FAIL' }
    }
    $lines.Add("| $($name -replace '\|', '\|') | $($cells -join ' | ') |")
}
$lines.Add('')
$lines.Add('## Failures')
$lines.Add('')
$failures = @()
foreach ($row in $consumerResults) {
    if ($row.BuildCode -ne 0) { $failures += "- **$($row.Tfm)** build failed: see logs/$($row.Tfm)-build.log" }
    if ($row.BuildCode -eq 0 -and $row.RunCode -ne 0) { $failures += "- **$($row.Tfm)** run failed (exit $($row.RunCode)): see logs/$($row.Tfm)-run.log" }
    foreach ($result in @($row.Results | Where-Object { -not $_.Passed })) {
        $failures += "- **$($row.Tfm)** $($result.Name): $($result.Detail -replace '\|', '\|')"
    }
}
if ($failures.Count -eq 0) { $lines.Add('None.') } else { $failures | ForEach-Object { $lines.Add($_) } }
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

<#
.SYNOPSIS
    Runs the Transformations.Web helpers against an in-process ASP.NET Core host, from a clean consumer.

.DESCRIPTION
    Packs Transformations.Core and Transformations.Web from the working tree into a fresh local feed, checks the Web package's
    dependency on Core, clears their NuGet cache entries, then builds and runs consumer/ for each target framework. The consumer
    hosts an app on 127.0.0.1, calls each helper from an endpoint, and checks the HTTP response.

.EXAMPLE
    pwsh ./tools/smoke/web/Run-WebSmoke.ps1
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
$reportDir = Join-Path $smokeRoot 'reports'

function Write-Step([string]$Text) {
    Write-Host "==> $Text" -ForegroundColor Cyan
}

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
$runDir = Join-Path $WorkRoot "web-runs\$stamp-$version-$shortCommit"
$feedDir = Join-Path $runDir 'feed'
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $feedDir, $logDir | Out-Null

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Pack Core and Web
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Packing Transformations.Core and Transformations.Web (Release)'
foreach ($project in 'Transformations.Core\Transformations.Core.csproj', 'Transformations.Web\Transformations.Web.csproj') {
    $name = [IO.Path]::GetFileNameWithoutExtension($project)
    & dotnet pack (Join-Path $repoRoot $project) -c Release -o $feedDir *> (Join-Path $logDir "pack-$name.log")
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for $name. See $logDir." }
}

$webPackage = Join-Path $feedDir "Transformations.Web.$packageVersion.nupkg"

# ---------------------------------------------------------------------------------------------------------------------
# 2. Package dependency: Web depends on Core at this version
# ---------------------------------------------------------------------------------------------------------------------

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Get-Nuspec([string]$Package) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
    }
    finally {
        $zip.Dispose()
    }
}

$webNuspec = Get-Nuspec $webPackage
$coreDependency = '<dependency id="Transformations.Core" version="' + [regex]::Escape($packageVersion) + '"'
$packageChecks = @(
    [pscustomobject]@{ Name = 'Web package depends on Core at this version'; Passed = ($webNuspec -match $coreDependency); Detail = '' }
)

$webSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($webPackage)))

# ---------------------------------------------------------------------------------------------------------------------
# 3. Clear NuGet cache entries for these packages at this version
# ---------------------------------------------------------------------------------------------------------------------

$globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$clearedCache = @()
foreach ($id in 'transformations.core', 'transformations.web') {
    $cacheDir = Join-Path $globalPackages "$id\$packageVersion"
    if (Test-Path $cacheDir) {
        Remove-Item -Recurse -Force $cacheDir
        $clearedCache += $cacheDir
    }
}

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

$consumerResults = @()
foreach ($tfm in $TargetFrameworks) {
    Write-Step "Consumer $tfm"
    $tfmDir = Join-Path $runDir $tfm
    $consumerDir = Join-Path $tfmDir 'consumer'
    New-Item -ItemType Directory -Force -Path $tfmDir | Out-Null
    Copy-Item -Recurse (Join-Path $smokeRoot 'consumer') $consumerDir
    $properties = @("-p:SmokeTfm=$tfm", "-p:SmokeVersion=$version")

    & dotnet restore (Join-Path $consumerDir 'Consumer.csproj') @properties --force --configfile $nugetConfig *> (Join-Path $logDir "$tfm-restore.log")
    $restoreCode = $LASTEXITCODE
    $resolvedOk = $false
    if ($restoreCode -eq 0) {
        $assets = Get-Content -Raw (Join-Path $consumerDir 'obj\project.assets.json') | ConvertFrom-Json
        $webLib = $assets.libraries.PSObject.Properties["Transformations.Web/$packageVersion"].Value
        $resolvedOk = ($webLib.sha512 -eq $webSha512)
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE

    $runLog = Join-Path $logDir "$tfm-run.log"
    $resultsFile = Join-Path $tfmDir 'results.json'
    $results = @()
    $runCode = -1
    if ($buildCode -eq 0) {
        & dotnet run --project (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-build @properties -- --results $resultsFile *> $runLog
        $runCode = $LASTEXITCODE
        if (Test-Path $resultsFile) { $results = @(Get-Content -Raw $resultsFile | ConvertFrom-Json) }
    }

    $consumerResults += [pscustomobject]@{
        Tfm         = $tfm
        RestoreCode = $restoreCode
        ResolvedOk  = $resolvedOk
        BuildCode   = $buildCode
        RunCode     = $runCode
        Results     = $results
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 5. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$consumersOk = @($consumerResults | Where-Object {
        -not ($_.RestoreCode -eq 0 -and $_.ResolvedOk -and $_.BuildCode -eq 0 -and $_.RunCode -eq 0)
    }).Count -eq 0
$checksOk = @($consumerResults | ForEach-Object { $_.Results } | Where-Object { -not $_.Passed }).Count -eq 0
$verdict = if ($consumersOk -and $checksOk -and @($packageChecks | Where-Object { -not $_.Passed }).Count -eq 0) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations Web smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version (stored as $packageVersion)")
$lines.Add("- Commit: $commit (dirty: $isDirty)")
$lines.Add("- Run: $stamp")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ')")
$lines.Add('- Host: in-process ASP.NET Core on 127.0.0.1, a free port per run')
$lines.Add("- Cache entries cleared: $($clearedCache.Count)")
$lines.Add('')
$lines.Add('## Package')
$lines.Add('')
$lines.Add('| Check | Result |')
$lines.Add('|---|---|')
foreach ($check in $packageChecks) { $lines.Add("| $($check.Name) | $(if ($check.Passed) { 'PASS' } else { 'FAIL' }) |") }
$lines.Add('')
$lines.Add('## Consumer per target framework')
$lines.Add('')
$lines.Add('| Target | Restore and hash | Build | Run | Checks passed |')
$lines.Add('|---|---|---|---|---|')
foreach ($row in $consumerResults) {
    $passedCount = @($row.Results | Where-Object { $_.Passed }).Count
    $lines.Add("| $($row.Tfm) | $(if ($row.RestoreCode -eq 0 -and $row.ResolvedOk) { 'PASS' } else { 'FAIL' }) | $(if ($row.BuildCode -eq 0) { 'PASS' } else { 'FAIL' }) | $(if ($row.RunCode -eq 0) { 'PASS' } else { 'FAIL' }) | $passedCount / $($row.Results.Count) |")
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
    if ($row.BuildCode -eq 0 -and $row.RunCode -ne 0) { $failures += "- **$($row.Tfm)** run finished with failures: see logs/$($row.Tfm)-run.log" }
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

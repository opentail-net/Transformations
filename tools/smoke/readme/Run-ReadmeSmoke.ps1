<#
.SYNOPSIS
    Runs the README Quick Start examples against the packed Transformations meta package, from a clean consumer.

.DESCRIPTION
    Packs every package from the working tree into a fresh local feed (or uses -FeedDir), clears the NuGet cache entries
    for the version, then builds and runs consumer/ for each target framework. The consumer references only the meta
    package. It asserts each deterministic example's stated value, and that no public type is defined in two
    Transformations assemblies.

    -FeedDir runs the same consumer against an existing feed, for example a feed packed before a fix, to show that the
    check fails there.

.EXAMPLE
    pwsh ./tools/smoke/readme/Run-ReadmeSmoke.ps1
.EXAMPLE
    pwsh ./tools/smoke/readme/Run-ReadmeSmoke.ps1 -FeedDir C:\path\to\old-feed
#>
[CmdletBinding()]
param(
    [string[]]$TargetFrameworks = @('net8.0', 'net10.0'),
    [string]$WorkRoot = (Join-Path $env:LOCALAPPDATA 'TransformationsSmoke'),
    [string]$FeedDir
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
$runDir = Join-Path $WorkRoot "readme-runs\$stamp-$version-$shortCommit"
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Feed: pack from the working tree, or use the folder given
# ---------------------------------------------------------------------------------------------------------------------

if ($FeedDir) {
    $feed = (Resolve-Path $FeedDir).Path
    Write-Step "Using existing feed $feed"
}
else {
    $feed = Join-Path $runDir 'feed'
    New-Item -ItemType Directory -Force -Path $feed | Out-Null
    Write-Step 'Packing all packages (repack-nuget.ps1)'
    $env:CI = '1'
    & (Join-Path $repoRoot 'repack-nuget.ps1') -OutputDir $feed *> (Join-Path $logDir 'repack.log')
    if ($LASTEXITCODE -ne 0) { throw "repack-nuget.ps1 failed. See $logDir." }
}

$metaPackage = Get-ChildItem $feed -Filter "Transformations.$packageVersion.nupkg" | Select-Object -First 1
if (-not $metaPackage) { throw "No Transformations.$packageVersion.nupkg in $feed." }
$metaSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($metaPackage.FullName)))

# ---------------------------------------------------------------------------------------------------------------------
# 2. Clear NuGet cache entries for this version
# ---------------------------------------------------------------------------------------------------------------------

$globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$clearedCache = @()
foreach ($dir in Get-ChildItem $globalPackages -Directory -Filter 'transformations*' -ErrorAction SilentlyContinue) {
    $versionDir = Join-Path $dir.FullName $packageVersion
    if (Test-Path $versionDir) {
        Remove-Item -Recurse -Force $versionDir
        $clearedCache += $versionDir
    }
}

$nugetConfig = Join-Path $runDir 'nuget.config'
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="smoke-local" value="$feed" />
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
# 3. Consumer per target framework
# ---------------------------------------------------------------------------------------------------------------------

$rows = @()
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
        $metaLib = $assets.libraries.PSObject.Properties["Transformations/$packageVersion"].Value
        $resolvedOk = $metaLib.sha512 -eq $metaSha512
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE
    $ambiguous = @(Select-String -Path $buildLog -Pattern 'error CS0121' -SimpleMatch).Count

    $runLog = Join-Path $logDir "$tfm-run.log"
    $runCode = -1
    $output = @()
    if ($buildCode -eq 0) {
        $output = @(& dotnet run --project (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-build @properties 2>&1)
        $runCode = $LASTEXITCODE
        $output | Set-Content -Path $runLog -Encoding utf8
    }

    $rows += [pscustomobject]@{
        Tfm         = $tfm
        RestoreOk   = ($restoreCode -eq 0) -and $resolvedOk
        BuildCode   = $buildCode
        Ambiguous   = $ambiguous
        RunCode     = $runCode
        Output      = @($output | Where-Object { $_ -match '^(PASS|FAIL)' -or $_ -match 'checks passed' })
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 4. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$verdict = if (@($rows | Where-Object { -not ($_.RestoreOk -and $_.BuildCode -eq 0 -and $_.RunCode -eq 0) }).Count -eq 0) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit$(if ($FeedDir) { '-existing-feed' }).md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations README smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version (stored as $packageVersion)")
$lines.Add("- Commit: $commit (dirty: $isDirty)")
$lines.Add("- Feed: $feed$(if ($FeedDir) { ' (existing feed, not repacked)' } else { '' })")
$lines.Add("- Run: $stamp")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ')")
$lines.Add("- Cache entries cleared: $($clearedCache.Count)")
$lines.Add('')
$lines.Add('| Target | Restore and hash | Build | CS0121 ambiguous calls | Run |')
$lines.Add('|---|---|---|---|---|')
foreach ($row in $rows) {
    $lines.Add("| $($row.Tfm) | $(if ($row.RestoreOk) { 'PASS' } else { 'FAIL' }) | $(if ($row.BuildCode -eq 0) { 'PASS' } else { 'FAIL' }) | $($row.Ambiguous) | $(if ($row.RunCode -eq 0) { 'PASS' } elseif ($row.RunCode -eq -1) { 'not run' } else { 'FAIL' }) |")
}
$lines.Add('')
foreach ($row in $rows) {
    $lines.Add("## Output on $($row.Tfm)")
    $lines.Add('')
    $lines.Add('```')
    if ($row.Output.Count -eq 0) { $lines.Add('(no output: the program did not run; see logs)') } else { $row.Output | ForEach-Object { $lines.Add($_) } }
    $lines.Add('```')
    $lines.Add('')
}

$lines | Set-Content -Path $reportPath -Encoding utf8

Write-Host ''
Write-Host "Verdict: $verdict" -ForegroundColor $(if ($verdict -eq 'PASS') { 'Green' } else { 'Red' })
Write-Host "Report:  $reportPath"

if ($verdict -eq 'PASS') { exit 0 } else { exit 1 }

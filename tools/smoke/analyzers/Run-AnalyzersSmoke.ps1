<#
.SYNOPSIS
    Smoke-tests the analyzer shipped in the Transformations meta package, from a clean consumer.

.DESCRIPTION
    Packs every package from the working tree into a fresh local feed (the meta package depends on the others), checks
    that the meta package ships the analyzer, the code fix and the generator, and clears the NuGet cache entries for the
    version. It then builds two consumers:

      consumer/      calls one deprecated member and one current member. The analyzer must report exactly one TX0001,
                     naming the replacement, and the program must run.
      removed-api/   calls a member removed in 3.0.0.0. It must fail with CS1061 and must not report TX0001, because
                     the analyzer can only flag a member that still exists.

    A dated report is written to reports/.

.PARAMETER TargetFrameworks
    Consumer target frameworks to run. Each must have an installed runtime and targeting pack.

.PARAMETER WorkRoot
    Where run folders are written. Kept outside the repository so the consumer never sits under the repo's
    Directory.Build.props.

.EXAMPLE
    pwsh ./tools/smoke/analyzers/Run-AnalyzersSmoke.ps1
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

function New-Check([string]$Name, [bool]$Passed, [string]$Detail = '') {
    [pscustomobject]@{ Name = $Name; Passed = $Passed; Detail = $Detail }
}

# ---------------------------------------------------------------------------------------------------------------------
# Identify the candidate
# ---------------------------------------------------------------------------------------------------------------------

$propsText = Get-Content -Raw (Join-Path $repoRoot 'Directory.Build.props')
$version = [regex]::Match($propsText, '<Version>([^<]+)</Version>').Groups[1].Value
if (-not $version) { throw 'Could not read <Version> from Directory.Build.props.' }

# NuGet drops a zero revision from file names and nuspec versions (3.0.0.0 is stored as 3.0.0).
$parsedVersion = [version]$version
$packageVersion = if ($parsedVersion.Revision -eq 0) { "$($parsedVersion.Major).$($parsedVersion.Minor).$($parsedVersion.Build)" } else { $version }

$commit = (git -C $repoRoot rev-parse HEAD).Trim()
$shortCommit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$dirtyFiles = @(git -C $repoRoot status --porcelain --untracked-files=no)
$isDirty = $dirtyFiles.Count -gt 0

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $WorkRoot "analyzers-runs\$stamp-$version-$shortCommit"
$feedDir = Join-Path $runDir 'feed'
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $feedDir, $logDir | Out-Null

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"
Write-Step "Run folder $runDir"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Pack every package from the working tree
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Packing all packages (repack-nuget.ps1)'
$env:CI = '1'
& (Join-Path $repoRoot 'repack-nuget.ps1') -OutputDir $feedDir *> (Join-Path $logDir 'repack.log')
if ($LASTEXITCODE -ne 0) { throw "repack-nuget.ps1 failed. See $logDir." }

$metaPackage = Join-Path $feedDir "Transformations.$packageVersion.nupkg"
if (-not (Test-Path $metaPackage)) { throw "The meta package was not produced: $metaPackage" }

# ---------------------------------------------------------------------------------------------------------------------
# 2. Package contents
# ---------------------------------------------------------------------------------------------------------------------

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageChecks = @()

$zip = [System.IO.Compression.ZipFile]::OpenRead($metaPackage)
try {
    $entryNames = @($zip.Entries | ForEach-Object { $_.FullName })
    $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
    $reader = New-Object System.IO.StreamReader($nuspecEntry.Open())
    try { $nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
}
finally {
    $zip.Dispose()
}

$packageChecks += New-Check 'package ships the analyzer (TX0001)' ($entryNames -contains 'analyzers/dotnet/cs/Transformations.Analyzers.dll')
$packageChecks += New-Check 'package ships the code fix provider' ($entryNames -contains 'analyzers/dotnet/cs/Transformations.Analyzers.CodeFixes.dll')
$packageChecks += New-Check 'package ships the source generator' ($entryNames -contains 'analyzers/dotnet/cs/Transformations.Mapping.Generator.dll')
$packageChecks += New-Check 'package contains LICENSE.md at its root' ($entryNames -contains 'LICENSE.md')
$packageChecks += New-Check "nuspec version is $packageVersion" ($nuspec -match "<version>$([regex]::Escape($packageVersion))</version>")
# The meta package depends on the packages it rolls up (Core arrives through them), each pinned to this version.
$transformationsDeps = @([regex]::Matches($nuspec, 'dependency id="(Transformations[^"]*)" version="([^"]*)"') | ForEach-Object { $_.Groups[2].Value })
$packageChecks += New-Check 'every Transformations dependency is pinned to this version' ($transformationsDeps.Count -gt 0 -and @($transformationsDeps | Where-Object { $_ -ne $packageVersion }).Count -eq 0) "$($transformationsDeps.Count) dependencies"

$metaSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($metaPackage)))

# ---------------------------------------------------------------------------------------------------------------------
# 3. Clear NuGet cache entries for this version, so restore cannot reuse an older copy
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
Write-Step "Cleared $($clearedCache.Count) cache entries for $packageVersion"

# ---------------------------------------------------------------------------------------------------------------------
# 4. Feed configuration
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
# 5. Consumer per target framework: the deprecated call is reported, the current call is not, and the program runs
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
        $metaLib = $assets.libraries.PSObject.Properties["Transformations/$packageVersion"].Value
        $resolvedOk = $metaLib.sha512 -eq $metaSha512
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE
    $buildText = Get-Content -Raw $buildLog
    # MSBuild repeats each warning in its summary, so count distinct diagnostics: same file position and same text.
    $tx0001 = @([regex]::Matches($buildText, '\(\d+,\d+\): warning TX0001: [^\r\n\[]*') | ForEach-Object { $_.Value.Trim() } | Sort-Object -Unique)

    $runLog = Join-Path $logDir "$tfm-run.log"
    $runCode = -1
    $runOutput = ''
    if ($buildCode -eq 0) {
        $runOutput = (& dotnet run --project (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-build @properties 2>&1 | Out-String)
        $runCode = $LASTEXITCODE
        $runOutput | Set-Content -Path $runLog -Encoding utf8
    }

    $consumerResults += [pscustomobject]@{
        Tfm        = $tfm
        ResolvedOk = $resolvedOk
        RestoreCode = $restoreCode
        BuildCode  = $buildCode
        Tx0001     = $tx0001
        RunCode    = $runCode
        RunOutput  = $runOutput.Trim()
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 6. Removed-API consumer: must fail to compile, with no TX0001 for the removed member
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Removed-API consumer (net10.0)'
$removedDir = Join-Path $runDir 'removed-api'
Copy-Item -Recurse (Join-Path $smokeRoot 'removed-api') $removedDir
$removedProps = @("-p:SmokeVersion=$version")
& dotnet restore (Join-Path $removedDir 'Removed.csproj') @removedProps --force --configfile $nugetConfig *> (Join-Path $logDir 'removed-restore.log')
$removedRestore = $LASTEXITCODE
$removedLog = Join-Path $logDir 'removed-build.log'
& dotnet build (Join-Path $removedDir 'Removed.csproj') -c Release --no-restore @removedProps *> $removedLog
$removedBuild = $LASTEXITCODE
$removedText = Get-Content -Raw $removedLog

# ---------------------------------------------------------------------------------------------------------------------
# 7. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$consumerChecks = @()
foreach ($row in $consumerResults) {
    $t = $row.Tfm
    $consumerChecks += New-Check "$t restore resolves the packed meta package" ($row.RestoreCode -eq 0 -and $row.ResolvedOk)
    $consumerChecks += New-Check "$t build succeeds" ($row.BuildCode -eq 0)
    $consumerChecks += New-Check "$t reports exactly one TX0001 (the deprecated call)" ($row.Tx0001.Count -eq 1) ($row.Tx0001 -join ' | ')
    $consumerChecks += New-Check "$t TX0001 names the replacement" (@($row.Tx0001 | Where-Object { $_ -match "'GetEnumDescription3' is deprecated — use 'GetDescription' instead" }).Count -eq 1)
    $consumerChecks += New-Check "$t program runs and prints the expected values" ($row.RunCode -eq 0 -and $row.RunOutput -eq 'Red Blue') "output: $($row.RunOutput)"
}

$removedChecks = @(
    New-Check 'removed-api restore succeeds' ($removedRestore -eq 0)
    New-Check 'removed-api build fails' ($removedBuild -ne 0)
    New-Check 'removed member fails with CS1061' ($removedText -match 'error CS1061:.*GetEnumDescription3')
    New-Check 'removed member produces no TX0001' ($removedText -notmatch 'TX0001')
)

$allChecks = @($packageChecks) + @($consumerChecks) + @($removedChecks)
$verdict = if (@($allChecks | Where-Object { -not $_.Passed }).Count -eq 0) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations analyzer smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version (stored as $packageVersion)")
$lines.Add("- Commit: $commit")
$lines.Add("- Working tree dirty (tracked files): $isDirty$(if ($isDirty) { ' (' + ($dirtyFiles -join '; ') + ')' })")
$lines.Add("- Run: $stamp")
$lines.Add("- Transformations .nupkg SHA-512 (base64, first 16 chars): $($metaSha512.Substring(0, 16))")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ') (consumer), net10.0 (removed-api)")
$lines.Add("- Run folder (logs and feed): $runDir")
$lines.Add('')
$lines.Add('## Checks')
$lines.Add('')
$lines.Add('| Check | Result | Detail |')
$lines.Add('|---|---|---|')
foreach ($check in $allChecks) {
    $lines.Add("| $($check.Name -replace '\|', '\|') | $(if ($check.Passed) { 'PASS' } else { 'FAIL' }) | $($check.Detail -replace '\|', '\|') |")
}
$lines.Add('')
$lines.Add('## Clear cache entries before restore')
$lines.Add('')
if ($clearedCache.Count -eq 0) { $lines.Add('None were present.') }
foreach ($path in $clearedCache) { $lines.Add("- $path") }

$lines | Set-Content -Path $reportPath -Encoding utf8

Write-Host ''
Write-Host "Verdict: $verdict" -ForegroundColor $(if ($verdict -eq 'PASS') { 'Green' } else { 'Red' })
Write-Host "Report:  $reportPath"

if ($verdict -eq 'PASS') { exit 0 } else { exit 1 }

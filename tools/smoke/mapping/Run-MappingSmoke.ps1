<#
.SYNOPSIS
    Smoke-tests the packed Transformations.Mapping package by consuming it from a clean project.

.DESCRIPTION
    Packs Transformations.Core and Transformations.Mapping from the working tree into a fresh local feed, checks the
    package contents, clears the NuGet cache entries for that version, then builds and runs the consumer in
    consumer/ for each target framework. A negative-control project in negative/ must fail with the expected
    diagnostics. A report is written to reports/. See README.md for what each check proves.

.PARAMETER TargetFrameworks
    Consumer target frameworks to run. Each must have an installed runtime and targeting pack.

.PARAMETER WorkRoot
    Where run folders are written. Kept outside the repository so the consumer never sits under the repo's
    Directory.Build.props.

.EXAMPLE
    pwsh ./tools/smoke/mapping/Run-MappingSmoke.ps1
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

$commit = (git -C $repoRoot rev-parse HEAD).Trim()
$shortCommit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$dirtyFiles = @(git -C $repoRoot status --porcelain --untracked-files=no)
$isDirty = $dirtyFiles.Count -gt 0

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $WorkRoot "runs\$stamp-$version-$shortCommit"
$feedDir = Join-Path $runDir 'feed'
$logDir = Join-Path $runDir 'logs'
New-Item -ItemType Directory -Force -Path $feedDir, $logDir | Out-Null

Write-Step "Candidate $version at $shortCommit (dirty: $isDirty)"
Write-Step "Run folder $runDir"

# ---------------------------------------------------------------------------------------------------------------------
# 1. Pack
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Packing Transformations.Core and Transformations.Mapping (Release)'
foreach ($project in 'Transformations.Core\Transformations.Core.csproj', 'Transformations.Mapping\Transformations.Mapping.csproj') {
    $name = [IO.Path]::GetFileNameWithoutExtension($project)
    & dotnet pack (Join-Path $repoRoot $project) -c Release -o $feedDir *> (Join-Path $logDir "pack-$name.log")
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for $name. See $logDir." }
}

$mappingPackage = Join-Path $feedDir "Transformations.Mapping.$version.nupkg"
$corePackage = Join-Path $feedDir "Transformations.Core.$version.nupkg"

# ---------------------------------------------------------------------------------------------------------------------
# 2. Package contents
# ---------------------------------------------------------------------------------------------------------------------

Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageChecks = @()

$zip = [System.IO.Compression.ZipFile]::OpenRead($mappingPackage)
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
    $entry = "lib/$tfm/Transformations.Mapping.dll"
    $packageChecks += New-Check "package contains $entry" ($entryNames -contains $entry)
}
$packageChecks += New-Check 'package contains the source generator under analyzers/' ($entryNames -contains 'analyzers/dotnet/cs/Transformations.Mapping.Generator.dll')
$packageChecks += New-Check 'package contains build/Transformations.Mapping.props (MSBuild switch)' ($entryNames -contains 'build/Transformations.Mapping.props')
$packageChecks += New-Check 'package contains README.md' ($entryNames -contains 'README.md')
$packageChecks += New-Check "nuspec version is $version" ($nuspec -match "<version>$([regex]::Escape($version))</version>")
$packageChecks += New-Check 'nuspec depends on Transformations.Core at the same version' ($nuspec -match "dependency id=""Transformations.Core"" version=""$([regex]::Escape($version))""")

$mappingSha256 = (Get-FileHash $mappingPackage -Algorithm SHA256).Hash
$mappingSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($mappingPackage)))
$coreSha512 = [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($corePackage)))

# ---------------------------------------------------------------------------------------------------------------------
# 3. Clear NuGet cache entries for this version, so restore cannot reuse an older copy of the same version
# ---------------------------------------------------------------------------------------------------------------------

$globalPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$clearedCache = @()
foreach ($id in 'transformations.mapping', 'transformations.core') {
    $cacheDir = Join-Path $globalPackages "$id\$version"
    if (Test-Path $cacheDir) {
        Remove-Item -Recurse -Force $cacheDir
        $clearedCache += $cacheDir
    }
}
Write-Step "Cleared $($clearedCache.Count) cache entries for $version"

# ---------------------------------------------------------------------------------------------------------------------
# 4. Feed configuration: only Transformations* packages come from the local feed, everything else from nuget.org
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
# 5. Consumer per target framework
# ---------------------------------------------------------------------------------------------------------------------

$consumerResults = @()
foreach ($tfm in $TargetFrameworks) {
    Write-Step "Consumer $tfm"
    $tfmDir = Join-Path $runDir $tfm
    $consumerDir = Join-Path $tfmDir 'consumer'
    New-Item -ItemType Directory -Force -Path $tfmDir | Out-Null
    Copy-Item -Recurse (Join-Path $smokeRoot 'consumer') $consumerDir
    $properties = @("-p:SmokeTfm=$tfm", "-p:SmokeMappingVersion=$version")

    $restoreCode = 0
    & dotnet restore (Join-Path $consumerDir 'Consumer.csproj') @properties --force --configfile $nugetConfig *> (Join-Path $logDir "$tfm-restore.log")
    $restoreCode = $LASTEXITCODE

    $resolvedCorrectly = $false
    $resolvedDetail = 'restore failed'
    if ($restoreCode -eq 0) {
        $assets = Get-Content -Raw (Join-Path $consumerDir 'obj\project.assets.json') | ConvertFrom-Json
        $mappingLib = $assets.libraries.PSObject.Properties["Transformations.Mapping/$version"].Value
        $coreLib = $assets.libraries.PSObject.Properties["Transformations.Core/$version"].Value
        $resolvedCorrectly = ($mappingLib.sha512 -eq $mappingSha512) -and ($coreLib.sha512 -eq $coreSha512)
        $resolvedDetail = if ($resolvedCorrectly) { 'restored packages match the packed .nupkg files (SHA-512)' } else { 'restored package hash differs from the packed .nupkg' }
    }

    $buildLog = Join-Path $logDir "$tfm-build.log"
    & dotnet build (Join-Path $consumerDir 'Consumer.csproj') -c Release --no-restore @properties *> $buildLog
    $buildCode = $LASTEXITCODE
    $buildWarnings = @(Select-String -Path $buildLog -Pattern ': warning ' -SimpleMatch:$false).Count

    $generatedFiles = @(Get-ChildItem (Join-Path $consumerDir 'obj') -Recurse -Filter '*.g.cs' | Where-Object { $_.FullName -match '\\generated\\' })
    $generatedText = ($generatedFiles | ForEach-Object { Get-Content -Raw $_.FullName }) -join "`n"
    $generatedOk = $generatedFiles.Count -gt 0 -and $generatedText -match 'ToOrderDtoList' -and $generatedText -match 'ProjectToOrderRowDto' -and $generatedText -match 'FromPaymentRecord'

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
        Tfm           = $tfm
        RestoreCode   = $restoreCode
        ResolvedOk    = $resolvedCorrectly
        ResolvedNote  = $resolvedDetail
        BuildCode     = $buildCode
        BuildWarnings = $buildWarnings
        GeneratedOk   = $generatedOk
        GeneratedFileCount = $generatedFiles.Count
        RunCode       = $runCode
        Results       = $results
    }
}

# ---------------------------------------------------------------------------------------------------------------------
# 6. Negative control: must fail, with the expected diagnostics
# ---------------------------------------------------------------------------------------------------------------------

Write-Step 'Negative control'
$negativeDir = Join-Path $runDir 'negative'
Copy-Item -Recurse (Join-Path $smokeRoot 'negative') $negativeDir
& dotnet restore (Join-Path $negativeDir 'Negative.csproj') --force --configfile $nugetConfig -p:SmokeMappingVersion=$version *> (Join-Path $logDir 'negative-restore.log')
$negativeLog = Join-Path $logDir 'negative-build.log'
& dotnet build (Join-Path $negativeDir 'Negative.csproj') -c Release --no-restore -p:SmokeMappingVersion=$version *> $negativeLog
$negativeCode = $LASTEXITCODE
$negativeText = Get-Content -Raw $negativeLog

$negativeChecks = @(
    New-Check 'negative build fails' ($negativeCode -ne 0)
    New-Check 'TXMAP001 is an error when TransformationsMappingUnmappedMembersAsErrors is set' ($negativeText -match ': error TXMAP001')
    New-Check 'TXMAP010 is reported for a constructor tie' ($negativeText -match ': warning TXMAP010')
    New-Check 'TXMAP023 is reported for weak nested inference' ($negativeText -match ': warning TXMAP023')
)

# ---------------------------------------------------------------------------------------------------------------------
# 7. Verdict and report
# ---------------------------------------------------------------------------------------------------------------------

$consumerPassed = $consumerResults | ForEach-Object {
    $_.RestoreCode -eq 0 -and $_.ResolvedOk -and $_.BuildCode -eq 0 -and $_.BuildWarnings -eq 0 -and $_.GeneratedOk -and $_.RunCode -eq 0
}
$allPassed = (@($packageChecks | Where-Object { -not $_.Passed }).Count -eq 0) `
    -and (@($consumerPassed | Where-Object { -not $_ }).Count -eq 0) `
    -and (@($negativeChecks | Where-Object { -not $_.Passed }).Count -eq 0)
$verdict = if ($allPassed) { 'PASS' } else { 'FAIL' }

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir "$stamp-$version-$shortCommit.md"

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Transformations.Mapping smoke test: $verdict")
$lines.Add('')
$lines.Add("- Package version: $version")
$lines.Add("- Commit: $commit")
$lines.Add("- Working tree dirty (tracked files): $isDirty$(if ($isDirty) { ' (' + ($dirtyFiles -join '; ') + ')' })")
$lines.Add("- Run: $stamp")
$lines.Add("- Mapping .nupkg SHA-256: $mappingSha256")
$lines.Add("- Target frameworks: $($TargetFrameworks -join ', ')")
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
$lines.Add('| Target | Restore | Package hash | Build | Warnings | Generated code | Run |')
$lines.Add('|---|---|---|---|---|---|---|')
foreach ($row in $consumerResults) {
    $lines.Add("| $($row.Tfm) | $(if ($row.RestoreCode -eq 0) { 'PASS' } else { 'FAIL' }) | $(if ($row.ResolvedOk) { 'PASS' } else { 'FAIL' }) | $(if ($row.BuildCode -eq 0) { 'PASS' } else { 'FAIL' }) | $($row.BuildWarnings) | $(if ($row.GeneratedOk) { "PASS ($($row.GeneratedFileCount) files)" } else { 'FAIL' }) | $(if ($row.RunCode -eq 0) { 'PASS' } else { 'FAIL' }) |")
}
$lines.Add('')
foreach ($row in $consumerResults) {
    $lines.Add("### $($row.Tfm) checks")
    $lines.Add('')
    if ($row.Results.Count -eq 0) {
        $lines.Add('No check results were written. See the build and run logs.')
    }
    else {
        $lines.Add('| Check | Result | Detail | ms |')
        $lines.Add('|---|---|---|---|')
        foreach ($result in $row.Results) {
            $detail = ($result.Detail -replace '\|', '\|')
            $lines.Add("| $($result.Name) | $(if ($result.Passed) { 'PASS' } else { 'FAIL' }) | $detail | $($result.ElapsedMs) |")
        }
    }
    $lines.Add('')
}
$lines.Add('## Negative control')
$lines.Add('')
$lines.Add('| Check | Result |')
$lines.Add('|---|---|')
foreach ($check in $negativeChecks) {
    $lines.Add("| $($check.Name) | $(if ($check.Passed) { 'PASS' } else { 'FAIL' }) |")
}
$lines.Add('')
$lines.Add('## Cache entries cleared before restore')
$lines.Add('')
if ($clearedCache.Count -eq 0) { $lines.Add('None were present.') }
foreach ($path in $clearedCache) { $lines.Add("- $path") }

$lines | Set-Content -Path $reportPath -Encoding utf8

Write-Host ''
Write-Host "Verdict: $verdict" -ForegroundColor $(if ($allPassed) { 'Green' } else { 'Red' })
Write-Host "Report:  $reportPath"

if ($allPassed) { exit 0 } else { exit 1 }

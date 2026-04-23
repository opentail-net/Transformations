param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "./artifacts/nuget",
    [switch]$CleanOutput
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $repoRoot

try {
    if ($CleanOutput -and (Test-Path $OutputDir)) {
        Remove-Item -Path $OutputDir -Recurse -Force
    }

    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null

    $packProjects = @(
        "Transformations/Transformations.csproj",
        "Transformations.Core/Transformations.Core.csproj",
        "Transformations.Data/Transformations.Data.csproj",
        "Transformations.Web/Transformations.Web.csproj",
        "Transformations.Dapper/Transformations.Dapper.csproj",
        "Transformations.EntityFramework/Transformations.EntityFramework.csproj",
        "Transformations.Mapping/Transformations.Mapping.csproj"
    )

    foreach ($project in $packProjects) {
        Write-Host "Packing $project..." -ForegroundColor Cyan
        dotnet pack $project -c $Configuration -o $OutputDir
    }

    Write-Host "Done. Packages are in $OutputDir" -ForegroundColor Green
}
finally {
    Pop-Location
}

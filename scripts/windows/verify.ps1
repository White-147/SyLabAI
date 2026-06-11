param(
    [switch]$SkipFrontend,
    [switch]$SkipBackend
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..\..")

function Invoke-CheckedStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    Write-Host "==> $Name"
    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE"
    }
}

Push-Location $repoRoot
try {
    $env:NUGET_PACKAGES = Join-Path $repoRoot ".cache\nuget"
    Remove-Item Env:npm_config_store_dir -ErrorAction SilentlyContinue

    if (-not $SkipBackend) {
        Invoke-CheckedStep "dotnet build" {
            dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln -v:minimal
        }
    }

    if (-not $SkipFrontend) {
        Invoke-CheckedStep "web build" {
            npm --prefix .\apps\web run build
        }
    }

    Invoke-CheckedStep "git diff --check" {
        git diff --check
    }
}
finally {
    Pop-Location
}

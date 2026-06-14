[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [ValidateSet("win", "macos")]
    [string]$OS = "win",
    [ValidateSet("x64", "arm64")]
    [string]$Architecture = "x64",
    [ValidateRange(1, 65535)]
    [int]$AppPort = 5555,
    [string]$RuntimeIdentifier,
    [string]$Framework = "net10.0",
    [switch]$NoClean,
    [switch]$FrameworkDependent,
    [switch]$NoSingleFile
)

$ErrorActionPreference = "Stop"

function Add-DotNetToProcessPath {
    if ([string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        return
    }

    $dotnetPath = Join-Path $env:ProgramFiles "dotnet"

    if (-not (Test-Path -LiteralPath $dotnetPath)) {
        return
    }

    $pathParts = $env:Path -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    if ($pathParts -contains $dotnetPath) {
        return
    }

    $env:Path = "$dotnetPath;$env:Path"
}

function Get-DotNetCommand {
    Add-DotNetToProcessPath

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue

    if (-not $dotnetCommand) {
        throw "dotnet was not found. Install the .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0 and run this script again."
    }

    return $dotnetCommand.Source
}

function Get-InstalledDotNetSdkVersions {
    param([string]$DotNetPath)

    try {
        $sdkLines = & $DotNetPath --list-sdks 2>$null
    }
    catch {
        return @()
    }

    return @(
        $sdkLines | ForEach-Object {
            if ($_ -match '^(?<Version>\d+\.\d+\.\d+)') {
                $Matches.Version
            }
        }
    )
}

function Assert-DotNet10SdkInstalled {
    param([string]$DotNetPath)

    $sdkVersions = Get-InstalledDotNetSdkVersions -DotNetPath $DotNetPath
    $hasDotNet10 = [bool]($sdkVersions | Where-Object { $_ -like "10.*" } | Select-Object -First 1)

    if (-not $hasDotNet10) {
        throw ".NET 10 SDK was not found. Install the SDK from https://dotnet.microsoft.com/download/dotnet/10.0 and run this script again."
    }
}

function Get-NormalizedFullPath {
    param([string]$Path)

    return [System.IO.Path]::GetFullPath($Path).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
}

function Assert-SafeAppOutputPath {
    param(
        [string]$PackageRoot,
        [string]$AppOutputPath
    )

    $expectedPath = Get-NormalizedFullPath -Path (Join-Path $PackageRoot "app")
    $actualPath = Get-NormalizedFullPath -Path $AppOutputPath

    if ($actualPath -ne $expectedPath) {
        throw "Refusing to clean unexpected publish output path '$actualPath'. Expected '$expectedPath'."
    }
}

function Resolve-PublishTarget {
    param(
        [string]$TargetOS,
        [string]$TargetArchitecture,
        [string]$RuntimeIdentifierOverride
    )

    if (-not [string]::IsNullOrWhiteSpace($RuntimeIdentifierOverride)) {
        if ($RuntimeIdentifierOverride -like "macos-*") {
            $RuntimeIdentifierOverride = "osx-$($RuntimeIdentifierOverride.Substring(6))"
        }

        if ($RuntimeIdentifierOverride -like "win-*") {
            return [PSCustomObject]@{
                OS = "win"
                RuntimeIdentifier = $RuntimeIdentifierOverride
                ExecutableName = "MyClass.Web.exe"
            }
        }

        if ($RuntimeIdentifierOverride -like "osx-*") {
            return [PSCustomObject]@{
                OS = "macos"
                RuntimeIdentifier = $RuntimeIdentifierOverride
                ExecutableName = "MyClass.Web"
            }
        }

        throw "Unsupported RuntimeIdentifier '$RuntimeIdentifierOverride'. Use a Windows RID like 'win-x64' or a macOS RID like 'osx-arm64'."
    }

    switch ($TargetOS) {
        "win" {
            return [PSCustomObject]@{
                OS = "win"
                RuntimeIdentifier = "win-$TargetArchitecture"
                ExecutableName = "MyClass.Web.exe"
            }
        }
        "macos" {
            return [PSCustomObject]@{
                OS = "macos"
                RuntimeIdentifier = "osx-$TargetArchitecture"
                ExecutableName = "MyClass.Web"
            }
        }
    }
}

function Set-RepoDotNetCliHome {
    param([string]$RepoRoot)

    if (-not [string]::IsNullOrWhiteSpace($env:DOTNET_CLI_HOME)) {
        return
    }

    $dotnetCliHome = Join-Path $RepoRoot ".dotnet-home"
    New-Item -ItemType Directory -Path $dotnetCliHome -Force | Out-Null
    $env:DOTNET_CLI_HOME = $dotnetCliHome
}

function Stop-MyClassWebProcess {
    param([string]$PidPath)

    $processes = @(Get-Process -Name "MyClass.Web" -ErrorAction SilentlyContinue)

    if (-not $processes) {
        return
    }

    foreach ($process in $processes) {
        Write-Host "Stopping existing MyClass.Web process $($process.Id)."
        Stop-Process -Id $process.Id -Force
        Wait-Process -Id $process.Id -Timeout 10 -ErrorAction SilentlyContinue
    }

    if (Test-Path -LiteralPath $PidPath) {
        Remove-Item -LiteralPath $PidPath -ErrorAction SilentlyContinue
    }
}

function Test-CurrentProcessIsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)

    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Set-MyClassFirewallRule {
    param([int]$Port)

    if (-not $IsWindows -and $PSVersionTable.PSEdition -eq "Core") {
        Write-Host "Skipping Windows firewall rule because this host is not Windows."
        return
    }

    if (-not (Get-Command New-NetFirewallRule -ErrorAction SilentlyContinue)) {
        Write-Host "Skipping Windows firewall rule because NetSecurity cmdlets are not available."
        return
    }

    if (-not (Test-CurrentProcessIsAdministrator)) {
        Write-Warning "Skipping Windows firewall rule for port $Port because this script is not running as administrator."
        return
    }

    $displayName = "Allow My Class App port access"
    $existingRule = Get-NetFirewallRule -DisplayName $displayName -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($existingRule) {
        Write-Host "Updating Windows firewall rule '$displayName' for TCP port $Port."
        $existingRule | Set-NetFirewallRule -Direction Inbound -Action Allow -Profile Private -Enabled True
        $existingRule | Get-NetFirewallPortFilter | Set-NetFirewallPortFilter -Protocol TCP -LocalPort $Port
        return
    }

    Write-Host "Creating Windows firewall rule '$displayName' for TCP port $Port."
    New-NetFirewallRule `
        -DisplayName $displayName `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $Port `
        -Profile Private | Out-Null
}

$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = Split-Path -Parent $scriptRoot
$srcRoot = Join-Path $repoRoot "src"
$webProjectRoot = Join-Path $srcRoot "MyClass.Web"
$projectPath = Join-Path $webProjectRoot "MyClass.Web.csproj"
$publishTarget = Resolve-PublishTarget -TargetOS $OS -TargetArchitecture $Architecture -RuntimeIdentifierOverride $RuntimeIdentifier
$packageRoot = Join-Path $repoRoot "dist-$($publishTarget.OS)"
$appOutputPath = Join-Path $packageRoot "app"
$pidPath = Join-Path $packageRoot "myclass.pid"

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Project file not found at $projectPath."
}

if (-not $FrameworkDependent -and [string]::IsNullOrWhiteSpace($publishTarget.RuntimeIdentifier)) {
    throw "RuntimeIdentifier is required for a self-contained publish."
}

Set-RepoDotNetCliHome -RepoRoot $repoRoot
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

$dotnetPath = Get-DotNetCommand
Assert-DotNet10SdkInstalled -DotNetPath $dotnetPath
Assert-SafeAppOutputPath -PackageRoot $packageRoot -AppOutputPath $appOutputPath
Stop-MyClassWebProcess -PidPath $pidPath

if ($publishTarget.OS -eq "win") {
    Set-MyClassFirewallRule -Port $AppPort
}

if (-not $NoClean -and (Test-Path -LiteralPath $appOutputPath)) {
    Write-Host "Cleaning $appOutputPath"
    Remove-Item -LiteralPath $appOutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $appOutputPath -Force | Out-Null

$publishArgs = @(
    "publish",
    $projectPath,
    "--configuration", $Configuration,
    "--framework", $Framework,
    "--output", $appOutputPath,
    "--nologo"
)

if (-not [string]::IsNullOrWhiteSpace($publishTarget.RuntimeIdentifier)) {
    $publishArgs += @("--runtime", $publishTarget.RuntimeIdentifier)
}

if ($FrameworkDependent) {
    $publishArgs += "--no-self-contained"
}
else {
    $publishArgs += @("--self-contained", "true")
}

$publishSingleFile = if ($NoSingleFile) { "false" } else { "true" }
$publishArgs += "-p:PublishSingleFile=$publishSingleFile"

Write-Host "Publishing $projectPath"
Write-Host "Output: $appOutputPath"
Write-Host "Target OS: $($publishTarget.OS)"
Write-Host "RuntimeIdentifier: $($publishTarget.RuntimeIdentifier)"

& $dotnetPath @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$exePath = Join-Path $appOutputPath $publishTarget.ExecutableName

if (-not $FrameworkDependent -and -not (Test-Path -LiteralPath $exePath)) {
    throw "Publish completed, but expected executable was not found at $exePath."
}

Write-Host "Published MyClass to $appOutputPath"
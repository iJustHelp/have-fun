[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SourcePath,
    [Parameter(Mandatory)]
    [string]$DestinationPath,
    [string]$ArchiveRootName = "Have Fun"
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    param(
        [string]$WorkingDirectory,
        [string[]]$Arguments
    )

    & git -C $WorkingDirectory @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

$gitCommand = Get-Command git -ErrorAction SilentlyContinue

if (-not $gitCommand) {
    throw "git was not found. Git is required to package the macOS distribution with executable permissions."
}

$sourceRoot = [System.IO.Path]::GetFullPath((Resolve-Path -LiteralPath $SourcePath))
$destination = [System.IO.Path]::GetFullPath($DestinationPath)
$destinationDirectory = [System.IO.Path]::GetDirectoryName($destination)
$stageRoot = Join-Path $destinationDirectory ".dist-zip-stage\macos-archive"
$resolvedStageRoot = [System.IO.Path]::GetFullPath($stageRoot)
$expectedStageParent = [System.IO.Path]::GetFullPath((Join-Path $destinationDirectory ".dist-zip-stage"))

if (-not $resolvedStageRoot.StartsWith(
    "$expectedStageParent$([System.IO.Path]::DirectorySeparatorChar)",
    [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to use unexpected staging path '$resolvedStageRoot'."
}

if (Test-Path -LiteralPath $destination) {
    Remove-Item -LiteralPath $destination -Force
}

if (Test-Path -LiteralPath $resolvedStageRoot) {
    Remove-Item -LiteralPath $resolvedStageRoot -Recurse -Force
}

try {
    $archiveRoot = Join-Path $resolvedStageRoot $ArchiveRootName
    New-Item -ItemType Directory -Path $archiveRoot -Force | Out-Null
    Get-ChildItem -LiteralPath $sourceRoot -Force |
        Copy-Item -Destination $archiveRoot -Recurse -Force

    Invoke-Git -WorkingDirectory $resolvedStageRoot -Arguments @("init", "--quiet")
    Invoke-Git -WorkingDirectory $resolvedStageRoot -Arguments @("config", "core.autocrlf", "false")
    Invoke-Git -WorkingDirectory $resolvedStageRoot -Arguments @("add", "--all", "--", ".")

    $executablePaths = @(
        "$ArchiveRootName/app.sh"
        "$ArchiveRootName/scripts/run.sh"
    )

    $appRoot = Join-Path $archiveRoot "app"

    if (Test-Path -LiteralPath $appRoot) {
        $executablePaths += Get-ChildItem -LiteralPath $appRoot -File |
            Where-Object {
                [string]::IsNullOrEmpty($_.Extension) -or
                $_.Name.EndsWith(".Web", [System.StringComparison]::OrdinalIgnoreCase)
            } |
            ForEach-Object { "$ArchiveRootName/app/$($_.Name)" }
    }

    foreach ($executablePath in $executablePaths) {
        $fullPath = Join-Path $resolvedStageRoot $executablePath

        if (Test-Path -LiteralPath $fullPath) {
            Invoke-Git -WorkingDirectory $resolvedStageRoot `
                -Arguments @("update-index", "--chmod=+x", "--", $executablePath)
        }
    }

    $treeId = (& git -C $resolvedStageRoot write-tree).Trim()

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($treeId)) {
        throw "git write-tree failed."
    }

    Invoke-Git -WorkingDirectory $resolvedStageRoot `
        -Arguments @("archive", "--format=zip", "--output=$destination", $treeId)

    Write-Host "Created $destination with macOS executable permissions."
}
finally {
    if (Test-Path -LiteralPath $resolvedStageRoot) {
        Remove-Item -LiteralPath $resolvedStageRoot -Recurse -Force
    }
}

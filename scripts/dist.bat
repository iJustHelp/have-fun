@echo off
setlocal

pushd "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1" -OS win %*
set "EXIT_CODE=%ERRORLEVEL%"
popd

if not "%EXIT_CODE%"=="0" (
    exit /b %EXIT_CODE%
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference = 'Stop';" ^
    "$repoRoot = Resolve-Path '%~dp0..';" ^
    "$distPath = Join-Path $repoRoot 'dist-win';" ^
    "$zipPath = Join-Path $repoRoot 'dist-win.zip';" ^
    "$stageRoot = Join-Path $repoRoot '.dist-zip-stage\win';" ^
    "$namedRoot = Join-Path $stageRoot 'Have Fun';" ^
    "if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force };" ^
    "if (Test-Path -LiteralPath $stageRoot) { Remove-Item -LiteralPath $stageRoot -Recurse -Force };" ^
    "New-Item -ItemType Directory -Path $namedRoot -Force | Out-Null;" ^
    "Get-ChildItem -LiteralPath $distPath -Force | Copy-Item -Destination $namedRoot -Recurse -Force;" ^
    "Compress-Archive -LiteralPath $namedRoot -DestinationPath $zipPath -Force;" ^
    "Remove-Item -LiteralPath $stageRoot -Recurse -Force;" ^
    "Write-Host ('Created ' + $zipPath)"
set "EXIT_CODE=%ERRORLEVEL%"

if not "%EXIT_CODE%"=="0" (
    exit /b %EXIT_CODE%
)

pushd "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1" -OS macos %*
set "EXIT_CODE=%ERRORLEVEL%"
popd

if not "%EXIT_CODE%"=="0" (
    exit /b %EXIT_CODE%
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference = 'Stop';" ^
    "$repoRoot = Resolve-Path '%~dp0..';" ^
    "$distPath = Join-Path $repoRoot 'dist-macos';" ^
    "$zipPath = Join-Path $repoRoot 'dist-macos.zip';" ^
    "$stageRoot = Join-Path $repoRoot '.dist-zip-stage\macos';" ^
    "$namedRoot = Join-Path $stageRoot 'Have Fun';" ^
    "if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force };" ^
    "if (Test-Path -LiteralPath $stageRoot) { Remove-Item -LiteralPath $stageRoot -Recurse -Force };" ^
    "New-Item -ItemType Directory -Path $namedRoot -Force | Out-Null;" ^
    "Get-ChildItem -LiteralPath $distPath -Force | Copy-Item -Destination $namedRoot -Recurse -Force;" ^
    "Compress-Archive -LiteralPath $namedRoot -DestinationPath $zipPath -Force;" ^
    "Remove-Item -LiteralPath $stageRoot -Recurse -Force;" ^
    "Write-Host ('Created ' + $zipPath)"
set "EXIT_CODE=%ERRORLEVEL%"
exit /b %EXIT_CODE%

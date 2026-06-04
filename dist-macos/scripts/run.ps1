[CmdletBinding()]

$Port = 3333
$ErrorActionPreference = "Stop"

function Test-PrivateIPv4Address {
    param([string]$Address)

    return $Address.StartsWith("10.") -or
        $Address.StartsWith("192.168.") -or
        $Address -match '^172\.(1[6-9]|2[0-9]|3[0-1])\.'
}

function Get-PreferredLocalIPv4Address {
    $interfaces =
        [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() |
        Where-Object {
            $_.OperationalStatus -eq [System.Net.NetworkInformation.OperationalStatus]::Up -and
            $_.NetworkInterfaceType -ne [System.Net.NetworkInformation.NetworkInterfaceType]::Loopback -and
            $_.NetworkInterfaceType -ne [System.Net.NetworkInformation.NetworkInterfaceType]::Tunnel
        }

    $physicalInterfaces = $interfaces | Where-Object {
        $_.Name -notmatch '^(vEthernet|VirtualBox|VMware|WSL|Hyper-V)' -and
        $_.Description -notmatch '(Hyper-V|VirtualBox|VMware|WSL)'
    }

    $candidateInterfaces = if ($physicalInterfaces) { $physicalInterfaces } else { $interfaces }

    $candidates =
        $candidateInterfaces |
        ForEach-Object {
            $networkInterface = $_
            $networkInterface.GetIPProperties().UnicastAddresses | ForEach-Object {
                [pscustomobject]@{
                    InterfaceType = $networkInterface.NetworkInterfaceType
                    IP = $_.Address.IPAddressToString
                    AddressFamily = $_.Address.AddressFamily
                }
            }
        } |
        Where-Object {
            $_.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetwork -and
            $_.IP -notlike '127.*' -and
            $_.IP -notlike '169.254.*'
        }

    if (-not $candidates) {
        return $null
    }

    $preferred = $candidates | Where-Object {
        $_.InterfaceType -eq [System.Net.NetworkInformation.NetworkInterfaceType]::Wireless80211 -and
        (Test-PrivateIPv4Address $_.IP)
    } | Select-Object -First 1

    if ($preferred) {
        return $preferred.IP
    }

    $preferred = $candidates | Where-Object {
        $_.InterfaceType -eq [System.Net.NetworkInformation.NetworkInterfaceType]::Ethernet -and
        (Test-PrivateIPv4Address $_.IP)
    } | Select-Object -First 1

    if ($preferred) {
        return $preferred.IP
    }

    $preferred = $candidates | Where-Object {
        Test-PrivateIPv4Address $_.IP
    } | Select-Object -First 1

    if ($preferred) {
        return $preferred.IP
    }

    return ($candidates | Select-Object -First 1).IP
}

function Stop-PreviousLaunch {
    param(
        [string]$PidPath,
        [string]$ExpectedExePath
    )

    if (-not (Test-Path -LiteralPath $PidPath)) {
        return
    }

    $rawPid = (Get-Content -LiteralPath $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1)
    $previousProcessId = 0

    if (-not [int]::TryParse($rawPid, [ref]$previousProcessId)) {
        Remove-Item -LiteralPath $PidPath -ErrorAction SilentlyContinue
        return
    }

    $process = Get-Process -Id $previousProcessId -ErrorAction SilentlyContinue

    if (-not $process) {
        Remove-Item -LiteralPath $PidPath -ErrorAction SilentlyContinue
        return
    }

    $expectedFullPath = [System.IO.Path]::GetFullPath($ExpectedExePath)
    $actualFullPath = if ($process.Path) { [System.IO.Path]::GetFullPath($process.Path) } else { "" }

    if ($actualFullPath -ne $expectedFullPath) {
        Write-Warning "PID file points to process $previousProcessId, but it is not $expectedFullPath. Leaving it running."
        return
    }

    Stop-Process -Id $previousProcessId -Force
    Wait-Process -Id $previousProcessId -Timeout 10 -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $PidPath -ErrorAction SilentlyContinue
}

function Stop-HaveFunWebProcess {
    param([string]$PidPath)

    $processes = @(Get-Process -Name "HaveFun.Web" -ErrorAction SilentlyContinue)

    if (-not $processes) {
        return
    }

    foreach ($process in $processes) {
        Write-Host "Stopping existing HaveFun.Web process $($process.Id)."
        Stop-Process -Id $process.Id -Force
        Wait-Process -Id $process.Id -Timeout 10 -ErrorAction SilentlyContinue
    }

    if (Test-Path -LiteralPath $PidPath) {
        Remove-Item -LiteralPath $PidPath -ErrorAction SilentlyContinue
    }
}

function Assert-PortAvailable {
    param([int]$Port)

    $listeners = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue

    if (-not $listeners) {
        return
    }

    $owners = $listeners |
        Select-Object -ExpandProperty OwningProcess -Unique |
        Where-Object { $_ -gt 0 }

    $details = $owners | ForEach-Object {
        $process = Get-Process -Id $_ -ErrorAction SilentlyContinue

        if ($process) {
            "$_ ($($process.ProcessName))"
        }
        else {
            "$_"
        }
    }

    throw "Port $Port is already in use by process id(s): $($details -join ', '). Stop that process and try again."
}

function Stop-HaveFunListenersOnPort {
    param(
        [int]$Port,
        [string]$ExpectedExePath
    )

    $listeners = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue

    if (-not $listeners) {
        return
    }

    $expectedFullPath = [System.IO.Path]::GetFullPath($ExpectedExePath)
    $owners = $listeners |
        Select-Object -ExpandProperty OwningProcess -Unique |
        Where-Object { $_ -gt 0 }

    foreach ($owner in $owners) {
        $process = Get-Process -Id $owner -ErrorAction SilentlyContinue

        if (-not $process) {
            continue
        }

        $actualFullPath = if ($process.Path) { [System.IO.Path]::GetFullPath($process.Path) } else { "" }
        $isExpectedApp = $actualFullPath -eq $expectedFullPath
        $isHaveFunApp = $process.ProcessName -eq "HaveFun.Web"

        if (-not ($isExpectedApp -or $isHaveFunApp)) {
            continue
        }

        Write-Host "Stopping existing Have Fun process $owner on port $Port."
        Stop-Process -Id $owner -Force
        Wait-Process -Id $owner -Timeout 10 -ErrorAction SilentlyContinue
    }
}

$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$distRoot = Split-Path -Parent $scriptRoot
$exePath = Join-Path $distRoot "app\HaveFun.Web.exe"
$pidPath = Join-Path $distRoot "havefun.pid"

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Published app not found at $exePath."
}

$localIp = Get-PreferredLocalIPv4Address

if ([string]::IsNullOrWhiteSpace($localIp)) {
    throw "Could not determine a WiFi/local IPv4 address."
}

$bindingUrl = "http://{0}:{1}" -f $localIp, $Port
$browserUrl = $bindingUrl
$workingDirectory = Split-Path -Path $exePath -Parent

Stop-PreviousLaunch -PidPath $pidPath -ExpectedExePath $exePath
Stop-HaveFunWebProcess -PidPath $pidPath
Stop-HaveFunListenersOnPort -Port $Port -ExpectedExePath $exePath
Assert-PortAvailable -Port $Port

$originalAspNetCoreUrls = $env:ASPNETCORE_URLS
$env:ASPNETCORE_URLS = $bindingUrl

try {
    $process = Start-Process -FilePath $exePath -WorkingDirectory $workingDirectory -WindowStyle Hidden -PassThru
}
finally {
    if ($null -eq $originalAspNetCoreUrls) {
        Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue
    }
    else {
        $env:ASPNETCORE_URLS = $originalAspNetCoreUrls
    }
}

Set-Content -LiteralPath $pidPath -Value $process.Id

Start-Sleep -Seconds 3
$process.Refresh()

if ($process.HasExited) {
    Remove-Item -LiteralPath $pidPath -ErrorAction SilentlyContinue
    throw "Have Fun exited immediately with code $($process.ExitCode)."
}

Start-Process $browserUrl

Write-Host "Started Have Fun (PID $($process.Id)) at $bindingUrl"
Write-Host "Opened $browserUrl"
Write-Host "Look at the opened tab in your browser."

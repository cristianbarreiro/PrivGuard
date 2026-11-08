<#
.SYNOPSIS
    Gathers read-only hardware, OS, and virtualization diagnostic information from the physical L0 host.

.DESCRIPTION
    This script performs read-only CIM/WMI and environment queries to determine if the host system
    is capable of provisioning an isolated Windows 11 virtual machine for PrivGuard lab validation (Topology B).
    Strictly read-only: does not modify system settings, firmware, registry, services, or boot configuration.

.PARAMETER OutputPath
    Optional path to save the diagnostic snapshot as JSON.

.EXAMPLE
    .\Get-L0HostPreflight.ps1 -OutputPath ".\l0-preflight.json"
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " PrivGuard -- L0 Physical Host Preflight Diagnostic Query " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$timestampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# 1. Core System & OS Information
try {
    $cs = Get-CimInstance -ClassName Win32_ComputerSystem -ErrorAction Stop
} catch {
    $cs = $null
    Write-Warning "Failed to query Win32_ComputerSystem: $($_.ToString())"
}

try {
    $os = Get-CimInstance -ClassName Win32_OperatingSystem -ErrorAction Stop
} catch {
    $os = $null
    Write-Warning "Failed to query Win32_OperatingSystem: $($_.ToString())"
}

try {
    $cpu = Get-CimInstance -ClassName Win32_Processor -ErrorAction Stop
} catch {
    $cpu = $null
    Write-Warning "Failed to query Win32_Processor: $($_.ToString())"
}

try {
    $bios = Get-CimInstance -ClassName Win32_BIOS -ErrorAction Stop
} catch {
    $bios = $null
    Write-Warning "Failed to query Win32_BIOS: $($_.ToString())"
}

$firstCpu = if ($cpu) { $cpu | Select-Object -First 1 } else { $null }

# 2. Virtualization & Hardware Extension Metrics
$manufacturer = if ($cs) { $cs.Manufacturer } else { "Unknown" }
$model = if ($cs) { $cs.Model } else { "Unknown" }
$hypervisorPresent = if ($cs) { [bool]$cs.HypervisorPresent } else { $false }

$totalMemoryGb = if ($os -and $os.TotalVisibleMemorySize) { [math]::Round($os.TotalVisibleMemorySize / 1MB, 2) } else { 0 }
$freeMemoryGb  = if ($os -and $os.FreePhysicalMemory) { [math]::Round($os.FreePhysicalMemory / 1MB, 2) } else { 0 }

$cpuName = if ($firstCpu) { $firstCpu.Name } else { "Unknown" }
$virtFirmwareEnabled = if ($firstCpu -and $null -ne $firstCpu.VirtualizationFirmwareEnabled) { [bool]$firstCpu.VirtualizationFirmwareEnabled } else { $null }
$vmMonitorModeExt = if ($firstCpu -and $null -ne $firstCpu.VMMonitorModeExtensions) { [bool]$firstCpu.VMMonitorModeExtensions } else { $null }
$slatExt = if ($firstCpu -and $null -ne $firstCpu.SecondLevelAddressTranslationExtensions) { [bool]$firstCpu.SecondLevelAddressTranslationExtensions } else { $null }

# 3. Known Virtual Machine Signals (to detect if running inside a Guest vs Physical Host)
$knownVmSignatures = @("VirtualBox", "VMware", "Virtual Machine", "KVM", "QEMU", "Bochs", "Xen")
$isKnownGuest = $false
foreach ($sig in $knownVmSignatures) {
    if ($manufacturer -match $sig -or $model -match $sig) {
        $isKnownGuest = $true
        break
    }
}

# 4. Storage & Free Disk Space
$systemDrive = "C:"
$freeSpaceGb = 0
try {
    $volume = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$systemDrive'" -ErrorAction SilentlyContinue
    if ($volume -and $volume.FreeSpace) {
        $freeSpaceGb = [math]::Round($volume.FreeSpace / 1GB, 2)
    }
} catch {
    $freeSpaceGb = 0
}

# 5. Secure Boot (UEFI) Status
$secureBootStatus = "Unknown"
try {
    if (Get-Command -Name Confirm-SecureBootUEFI -ErrorAction SilentlyContinue) {
        $sb = Confirm-SecureBootUEFI -ErrorAction SilentlyContinue
        $secureBootStatus = if ($sb) { "Enabled" } else { "DisabledOrUnsupported" }
    }
} catch {
    $secureBootStatus = "AccessDeniedOrNotUEFI"
}

# 6. Hyper-V Services or Hypervisors Check
$installedHypervisors = [System.Collections.Generic.List[string]]::new()
try {
    $vmmsService = Get-Service -Name "vmms" -ErrorAction SilentlyContinue
    if ($vmmsService) {
        $installedHypervisors.Add("Hyper-V ($($vmmsService.Status))")
    }
} catch { }

try {
    if (Test-Path "C:\Program Files\Oracle\VirtualBox\VBoxManage.exe") {
        $installedHypervisors.Add("VirtualBox")
    }
} catch { }

try {
    if (Test-Path "C:\Program Files (x86)\VMware\VMware Workstation\vmware.exe" -or Test-Path "C:\Program Files\VMware\VMware Workstation\vmware.exe") {
        $installedHypervisors.Add("VMware Workstation")
    }
} catch { }

# 7. Classification
$classification = if ($isKnownGuest) {
    "GUEST_DETECTED"
} elseif ($virtFirmwareEnabled -eq $true -and $totalMemoryGb -ge 12.0) {
    "L0_READY_FOR_TOPOLOGY_B"
} elseif ($virtFirmwareEnabled -eq $true) {
    "L0_CAPABLE_CONSTRAINED_RAM"
} else {
    "L0_VIRTUALIZATION_NOT_ENABLED_IN_FIRMWARE"
}

$result = [pscustomobject]@{
    TimestampUtc                           = $timestampUtc
    Classification                         = $classification
    IsKnownVirtualGuest                    = $isKnownGuest
    Manufacturer                           = $manufacturer
    Model                                  = $model
    HypervisorPresent                      = $hypervisorPresent
    OSCaption                              = if ($os) { $os.Caption } else { "Unknown" }
    OSVersion                              = if ($os) { $os.Version } else { "Unknown" }
    OSBuildNumber                          = if ($os) { $os.BuildNumber } else { "Unknown" }
    TotalVisibleMemoryGB                   = $totalMemoryGb
    FreePhysicalMemoryGB                   = $freeMemoryGb
    SystemDriveFreeSpaceGB                 = $freeSpaceGb
    CPUName                                = $cpuName
    VirtualizationFirmwareEnabled          = $virtFirmwareEnabled
    VMMonitorModeExtensions                = $vmMonitorModeExt
    SecondLevelAddressTranslationExtensions= $slatExt
    BIOSManufacturer                       = if ($bios) { $bios.Manufacturer } else { "Unknown" }
    BIOSVersion                            = if ($bios) { $bios.SMBIOSBIOSVersion } else { "Unknown" }
    SecureBoot                             = $secureBootStatus
    DetectedHypervisors                    = ($installedHypervisors -join ", ")
}

Write-Host "Diagnostic Summary:" -ForegroundColor Yellow
$result | Format-List

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    try {
        $outputDir = Split-Path -Path $OutputPath -Parent
        if (-not [string]::IsNullOrWhiteSpace($outputDir) -and -not (Test-Path $outputDir)) {
            New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        }
        $result | ConvertTo-Json -Depth 4 | Set-Content -Path $OutputPath -Encoding UTF8
        Write-Host ('Diagnostic snapshot saved to: ' + $OutputPath) -ForegroundColor Green
    } catch {
        $msg = $_.ToString()
        Write-Error $msg
    }
}

return $result

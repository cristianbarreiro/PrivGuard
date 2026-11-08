<#
.SYNOPSIS
    Gathers read-only hardware, OS, storage, and virtualization diagnostic information from the physical L0 host.

.DESCRIPTION
    This script performs strictly read-only CIM/WMI, environment, and storage queries to determine
    whether a physical host meets the prerequisite baseline for provisioning an isolated Windows 11
    virtual machine for PrivGuard lab validation (Topology B).
    Strictly read-only: does not modify system settings, firmware, registry, services, hypervisor configurations,
    or boot configuration (BCD). Writes an output snapshot report only if -OutputPath is explicitly specified.

.PARAMETER VmStoragePath
    Target local filesystem directory path where the VM disk (VHDX/VDI) and snapshots will be stored.
    When omitted, target volume metrics are reported as UNKNOWN and preflight is marked PREFLIGHT_INCOMPLETE
    to ensure the actual storage destination is explicitly verified before provisioning.

.PARAMETER OutputPath
    Optional local file path to save the structured diagnostic snapshot as JSON.

.EXAMPLE
    .\Get-L0HostPreflight.ps1 -VmStoragePath "D:\PrivGuard-Lab\VMs" -OutputPath "$env:TEMP\l0-preflight.json"
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]$VmStoragePath,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

# Import pure classification logic module
$logicModulePath = Join-Path $PSScriptRoot "L0HostPreflight.Logic.psm1"
if (-not (Test-Path $logicModulePath)) {
    throw "Logic module not found at: $logicModulePath"
}
Import-Module $logicModulePath -Force

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " PrivGuard -- L0 Physical Host Preflight Diagnostic Query " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$timestampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# 1. System, OS, Processor, and BIOS Information (CIM Queries)
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

# 2. Host Identity & Virtual Machine Signature Detection
$systemIdentityStatus = "UNKNOWN"
$manufacturer = "UNKNOWN"
$model = "UNKNOWN"

if ($cs) {
    if (-not [string]::IsNullOrWhiteSpace($cs.Manufacturer)) { $manufacturer = $cs.Manufacturer.Trim() }
    if (-not [string]::IsNullOrWhiteSpace($cs.Model)) { $model = $cs.Model.Trim() }
    if ($manufacturer -ne "UNKNOWN" -and $model -ne "UNKNOWN") {
        $systemIdentityStatus = "RESOLVED"
    }
}

$hypervisorPresent = if ($cs -and $null -ne $cs.HypervisorPresent) { [bool]$cs.HypervisorPresent } else { $null }

# Known Virtual Machine Signatures (Strict detection of guest environment)
$knownVmSignatures = @("VirtualBox", "VMware", "Virtual Machine", "KVM", "QEMU", "Bochs", "Xen")
$isKnownGuest = $false
foreach ($sig in $knownVmSignatures) {
    if ($manufacturer -match $sig -or $model -match $sig) {
        $isKnownGuest = $true
        break
    }
}

# 3. RAM Metrics (Converted strictly to binary GiB: 1 GiB = 1024^3 bytes = 1024^2 KB)
$totalMemoryGiB = if ($os -and $null -ne $os.TotalVisibleMemorySize) { [math]::Round($os.TotalVisibleMemorySize / 1024 / 1024, 2) } else { $null }
$freeMemoryGiB  = if ($os -and $null -ne $os.FreePhysicalMemory) { [math]::Round($os.FreePhysicalMemory / 1024 / 1024, 2) } else { $null }

# 4. CPU Metrics & Virtualization Flags across all sockets/processors
$cpuAggregation = Get-L0CpuAggregation -Processors $cpu
$firstCpu = if ($cpu) { $cpu | Select-Object -First 1 } else { $null }
$cpuName = if ($firstCpu -and $firstCpu.Name) { $firstCpu.Name.Trim() } else { "UNKNOWN" }
$cpuPhysicalCount = $cpuAggregation.PhysicalSocketCount
$cpuCores = $cpuAggregation.CPUCores
$cpuLogical = $cpuAggregation.CPULogicalProcessors
$virtFirmwareEnabled = $cpuAggregation.VirtualizationFirmwareEnabled
$vmMonitorModeExt = $cpuAggregation.VMMonitorModeExtensions
$slatExt = $cpuAggregation.SecondLevelAddressTranslationExtensions

# 5. Storage Inspection: System Drive & Target VM Storage Volume
$systemDrive = "C:"
$systemFreeSpaceGiB = 0.0
try {
    $sysVol = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$systemDrive'" -ErrorAction SilentlyContinue
    if ($sysVol -and $null -ne $sysVol.FreeSpace) {
        $systemFreeSpaceGiB = [math]::Round($sysVol.FreeSpace / 1GB, 2)
    }
} catch {
    $systemFreeSpaceGiB = 0.0
}

$vmStorageStatus = "UNKNOWN"
$vmStorageVolumeId = ""
$vmStorageFileSystem = ""
$vmStorageFreeSpaceGiB = $null

if (-not [string]::IsNullOrWhiteSpace($VmStoragePath)) {
    $pathFormat = Resolve-L0VmStoragePathFormat -Path $VmStoragePath
    if (-not $pathFormat.IsValid) {
        $vmStorageStatus = $pathFormat.Status
        Write-Warning $pathFormat.Reason
    } else {
        $driveId = $pathFormat.DriveId
        try {
            $targetVol = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$driveId'" -ErrorAction SilentlyContinue
            if ($null -ne $targetVol) {
                # Win32_LogicalDisk DriveType: 3 = Local Fixed Disk, 2 = Removable Disk
                if ($targetVol.DriveType -and $targetVol.DriveType -ne 3 -and $targetVol.DriveType -ne 2) {
                    $vmStorageStatus = "NOT_LOCAL_DISK"
                    Write-Warning "Target drive $driveId is not a local disk (DriveType: $($targetVol.DriveType))."
                } elseif ($null -ne $targetVol.FreeSpace) {
                    $vmStorageVolumeId = $targetVol.DeviceID
                    $vmStorageFileSystem = if ($targetVol.FileSystem) { $targetVol.FileSystem } else { "Unknown" }
                    $vmStorageFreeSpaceGiB = [math]::Round($targetVol.FreeSpace / 1GB, 2)
                    $vmStorageStatus = "RESOLVED"
                } else {
                    $vmStorageStatus = "VOLUME_SPACE_UNKNOWN"
                    Write-Warning "Could not determine free space for volume: $driveId"
                }
            } else {
                $vmStorageStatus = "VOLUME_NOT_FOUND"
                Write-Warning "Logical disk not found for drive: $driveId"
            }
        } catch {
            $vmStorageStatus = "CIM_QUERY_FAILED"
            Write-Warning "Failed to query logical disk for '$driveId': $($_.ToString())"
        }
    }
}

# 6. Secure Boot (UEFI) Status
$secureBootStatus = "UNKNOWN"
try {
    if (Get-Command -Name Confirm-SecureBootUEFI -ErrorAction SilentlyContinue) {
        $sb = Confirm-SecureBootUEFI -ErrorAction Stop
        $secureBootStatus = if ($sb) { "ENABLED" } else { "DISABLED" }
    } else {
        $secureBootStatus = "CMDLET_NOT_AVAILABLE"
    }
} catch [System.PlatformNotSupportedException] {
    $secureBootStatus = "NOT_UEFI_OR_UNSUPPORTED"
} catch [System.UnauthorizedAccessException] {
    $secureBootStatus = "ACCESS_DENIED"
} catch {
    $secureBootStatus = "UNKNOWN"
}

# 7. Installed Hypervisor Software Detection (Distinguishing Type 1 vs Hosted Type 2)
$installedHypervisors = [System.Collections.Generic.List[string]]::new()
try {
    $vmms = Get-Service -Name "vmms" -ErrorAction SilentlyContinue
    if ($vmms) {
        $installedHypervisors.Add("Hyper-V (Type 1 native hypervisor; Service: $($vmms.Status))")
    }
} catch { }

try {
    if (Test-Path "C:\Program Files\Oracle\VirtualBox\VBoxManage.exe") {
        $installedHypervisors.Add("VirtualBox (Hosted / Type 2 hypervisor)")
    }
} catch { }

try {
    if (Test-Path "C:\Program Files (x86)\VMware\VMware Workstation\vmware.exe" -or Test-Path "C:\Program Files\VMware\VMware Workstation\vmware.exe") {
        $installedHypervisors.Add("VMware Workstation (Hosted / Type 2 hypervisor)")
    }
} catch { }

# 8. Evaluate Classification via Pure Logic Function
$metricsDict = @{
    IsKnownVirtualGuest                    = $isKnownGuest
    ComputerSystemIdentityStatus           = $systemIdentityStatus
    Manufacturer                           = $manufacturer
    Model                                  = $model
    HypervisorPresent                      = if ($null -ne $hypervisorPresent) { $hypervisorPresent } else { $null }
    VirtualizationFirmwareEnabled          = $virtFirmwareEnabled
    VMMonitorModeExtensions                = $vmMonitorModeExt
    SecondLevelAddressTranslationExtensions= $slatExt
    TotalVisibleMemoryGiB                  = $totalMemoryGiB
    FreePhysicalMemoryGiB                  = $freeMemoryGiB
    CPUCores                               = $cpuCores
    CPULogicalProcessors                   = $cpuLogical
    VmStorageStatus                        = $vmStorageStatus
    VmStorageVolume                        = $vmStorageVolumeId
    VmStorageFreeSpaceGiB                  = $vmStorageFreeSpaceGiB
}

$evaluation = Get-L0HostClassification -Metrics $metricsDict

$result = [pscustomobject]@{
    TimestampUtc                           = $timestampUtc
    Classification                         = $evaluation.Classification
    ClassificationReasons                  = $evaluation.Reasons
    IsKnownVirtualGuest                    = $isKnownGuest
    ComputerSystemIdentityStatus           = $systemIdentityStatus
    Manufacturer                           = $manufacturer
    Model                                  = $model
    HypervisorPresent                      = if ($null -ne $hypervisorPresent) { $hypervisorPresent } else { "UNKNOWN" }
    OSCaption                              = if ($os -and $os.Caption) { $os.Caption } else { "UNKNOWN" }
    OSVersion                              = if ($os -and $os.Version) { $os.Version } else { "UNKNOWN" }
    OSBuildNumber                          = if ($os -and $os.BuildNumber) { $os.BuildNumber } else { "UNKNOWN" }
    CPUName                                = $cpuName
    CPUPhysicalCount                       = $cpuPhysicalCount
    CPUCores                               = if ($null -ne $cpuCores) { $cpuCores } else { "UNKNOWN" }
    CPULogicalProcessors                   = if ($null -ne $cpuLogical) { $cpuLogical } else { "UNKNOWN" }
    TotalVisibleMemoryGiB                  = if ($null -ne $totalMemoryGiB) { $totalMemoryGiB } else { "UNKNOWN" }
    FreePhysicalMemoryGiB                  = if ($null -ne $freeMemoryGiB) { $freeMemoryGiB } else { "UNKNOWN" }
    VirtualizationFirmwareEnabled          = $virtFirmwareEnabled
    VMMonitorModeExtensions                = $vmMonitorModeExt
    SecondLevelAddressTranslationExtensions= $slatExt
    BIOSManufacturer                       = if ($bios -and $bios.Manufacturer) { $bios.Manufacturer } else { "UNKNOWN" }
    BIOSVersion                            = if ($bios -and $bios.SMBIOSBIOSVersion) { $bios.SMBIOSBIOSVersion } else { "UNKNOWN" }
    SecureBoot                             = $secureBootStatus
    SystemDriveFreeSpaceGiB                = $systemFreeSpaceGiB
    VmStoragePathRequested                 = $VmStoragePath
    VmStorageStatus                        = $vmStorageStatus
    VmStorageVolume                        = $vmStorageVolumeId
    VmStorageFileSystem                    = $vmStorageFileSystem
    VmStorageFreeSpaceGiB                  = if ($null -ne $vmStorageFreeSpaceGiB) { $vmStorageFreeSpaceGiB } else { "UNKNOWN" }
    DetectedHypervisors                    = if ($installedHypervisors.Count -gt 0) { ($installedHypervisors -join "; ") } else { "None detected in standard program paths" }
}

Write-Host "`nDiagnostic Summary:" -ForegroundColor Yellow
$result | Format-List

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    try {
        $outputDir = Split-Path -Path $OutputPath -Parent
        if (-not [string]::IsNullOrWhiteSpace($outputDir) -and -not (Test-Path $outputDir)) {
            New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        }
        $result | ConvertTo-Json -Depth 4 | Set-Content -Path $OutputPath -Encoding UTF8
        Write-Host ("`nDiagnostic snapshot saved to: " + $OutputPath) -ForegroundColor Green
    } catch {
        $msg = $_.ToString()
        Write-Error $msg
    }
}

return $result

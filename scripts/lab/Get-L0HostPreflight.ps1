<#
.SYNOPSIS
    Gathers read-only hardware, OS, storage, and virtualization diagnostic information from the physical L0 host.

.DESCRIPTION
    This script performs strictly read-only CIM/WMI, environment, and storage queries to determine
    whether a physical host meets the prerequisite baseline for provisioning an isolated Windows 11
    virtual machine for PrivGuard lab validation (Topology B).
    Strictly read-only: does not modify system settings, firmware, registry, services, hypervisor configurations,
    or boot configuration (BCD).

.PARAMETER VmStoragePath
    Optional target local filesystem path where the VM disk (VHDX/VDI) and snapshots will be stored.
    When omitted, target volume metrics are reported as UNKNOWN and preflight is marked PREFLIGHT_INCOMPLETE
    to ensure the actual storage destination is explicitly verified before provisioning.

.PARAMETER OutputPath
    Optional local file path to save the structured diagnostic snapshot as JSON.

.EXAMPLE
    .\Get-L0HostPreflight.ps1 -VmStoragePath "D:\PrivGuard-Lab\VMs" -OutputPath ".\l0-preflight.json"
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

function Get-L0HostClassification {
    param (
        [Parameter(Mandatory = $true)]
        [hashtable]$Metrics
    )

    $reasons = [System.Collections.Generic.List[string]]::new()

    # 1. Guest Detection (takes precedence: script run inside a VM cannot validate L0 physical host)
    if ($Metrics.IsKnownVirtualGuest -eq $true) {
        $reasons.Add("Host matches a known virtual machine signature ($($Metrics.Manufacturer) / $($Metrics.Model)). Physical L0 host cannot be verified from within a guest.")
        return @{
            Classification = "GUEST_DETECTED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 2. Check for Incomplete / Unknown Critical Diagnostics
    $incomplete = $false
    if ($Metrics.VirtualizationFirmwareEnabled -eq "UNKNOWN") {
        $reasons.Add("VirtualizationFirmwareEnabled could not be determined from processor CIM query.")
        $incomplete = $true
    }
    if ($Metrics.SecondLevelAddressTranslationExtensions -eq "UNKNOWN") {
        $reasons.Add("SecondLevelAddressTranslationExtensions (SLAT) could not be determined.")
        $incomplete = $true
    }
    if ($Metrics.TotalVisibleMemoryGiB -le 0 -or $Metrics.FreePhysicalMemoryGiB -le 0) {
        $reasons.Add("Memory capacity could not be queried reliably.")
        $incomplete = $true
    }
    if ($Metrics.VmStorageStatus -eq "UNKNOWN" -or [string]::IsNullOrWhiteSpace($Metrics.VmStorageVolume)) {
        $reasons.Add("Target VM storage path (-VmStoragePath) was not specified or could not be resolved. Target storage volume cannot be verified.")
        $incomplete = $true
    }

    if ($incomplete) {
        return @{
            Classification = "PREFLIGHT_INCOMPLETE"
            Reasons        = $reasons.ToArray()
        }
    }

    # 3. Hard Blocking Conditions
    $blocked = $false
    if ($Metrics.VirtualizationFirmwareEnabled -eq $false -or $Metrics.VirtualizationFirmwareEnabled -eq "FALSE") {
        $reasons.Add("Hardware virtualization (VT-x / AMD-V) is disabled in host BIOS/UEFI firmware.")
        $blocked = $true
    }
    if ($Metrics.SecondLevelAddressTranslationExtensions -eq $false -or $Metrics.SecondLevelAddressTranslationExtensions -eq "FALSE") {
        $reasons.Add("Second-Level Address Translation (SLAT / EPT / NPT) is not supported by host CPU.")
        $blocked = $true
    }
    if ($Metrics.TotalVisibleMemoryGiB -lt 10.0) {
        $reasons.Add("Host total physical RAM ($($Metrics.TotalVisibleMemoryGiB) GiB) is below absolute minimum (10.0 GiB) to support Host OS + Windows 11 VM.")
        $blocked = $true
    }
    if ($Metrics.FreePhysicalMemoryGiB -lt 4.0) {
        $reasons.Add("Host free physical RAM ($($Metrics.FreePhysicalMemoryGiB) GiB) is below minimum (4.0 GiB) to allocate to Windows 11 VM.")
        $blocked = $true
    }
    if ($Metrics.VmStorageFreeSpaceGiB -lt 50.0) {
        $reasons.Add("Target VM volume free space ($($Metrics.VmStorageFreeSpaceGiB) GiB) is below minimum (50.0 GiB) for base image (>=64 GiB virtual) and snapshot delta.")
        $blocked = $true
    }

    if ($blocked) {
        return @{
            Classification = "L0_PREFLIGHT_BLOCKED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 4. Constrained Conditions (Functional but requires conservative configuration)
    $constrained = $false
    if ($Metrics.TotalVisibleMemoryGiB -lt 16.0) {
        $reasons.Add("Host total physical RAM ($($Metrics.TotalVisibleMemoryGiB) GiB) is constrained (< 16.0 GiB). VM should be allocated maximum 4.0 - 5.0 GiB vRAM.")
        $constrained = $true
    }
    if ($Metrics.FreePhysicalMemoryGiB -lt 7.0) {
        $reasons.Add("Host free physical RAM ($($Metrics.FreePhysicalMemoryGiB) GiB) is constrained (< 7.0 GiB). Host background applications must be closed before launching VM.")
        $constrained = $true
    }
    if ($Metrics.VmStorageFreeSpaceGiB -lt 80.0) {
        $reasons.Add("Target VM volume free space ($($Metrics.VmStorageFreeSpaceGiB) GiB) is constrained (< 80.0 GiB). Snapshot chain depth must be monitored strictly.")
        $constrained = $true
    }

    if ($constrained) {
        return @{
            Classification = "L0_CAPABLE_CONSTRAINED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 5. Full Candidate Passed (All measured hardware metrics meet recommended baseline)
    $reasons.Add("All measured physical host criteria meet recommended baseline for Topology B (hardware virtualization, SLAT, RAM, and target volume storage).")
    $reasons.Add("Note: PASS_CANDIDATE confirms host hardware capacity; VM provisioning, vTPM 2.0, Secure Boot, FUS user accounts, and snapshot validation remain mandatory manual lab gates.")
    return @{
        Classification = "L0_PREFLIGHT_PASS_CANDIDATE"
        Reasons        = $reasons.ToArray()
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " PrivGuard -- L0 Physical Host Preflight Diagnostic Query " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$timestampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# 1. System, OS, Processor, and BIOS Information
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

$manufacturer = if ($cs -and $cs.Manufacturer) { $cs.Manufacturer } else { "Unknown" }
$model = if ($cs -and $cs.Model) { $cs.Model } else { "Unknown" }
$hypervisorPresent = if ($cs -and $null -ne $cs.HypervisorPresent) { [bool]$cs.HypervisorPresent } else { $false }

# 2. RAM Metrics (Converted strictly to binary GiB: 1 GiB = 1024^3 bytes = 1024^2 KB)
$totalMemoryGiB = if ($os -and $os.TotalVisibleMemorySize) { [math]::Round($os.TotalVisibleMemorySize / 1024 / 1024, 2) } else { 0.0 }
$freeMemoryGiB  = if ($os -and $os.FreePhysicalMemory) { [math]::Round($os.FreePhysicalMemory / 1024 / 1024, 2) } else { 0.0 }

# 3. CPU Virtualization Flags
$cpuName = if ($firstCpu -and $firstCpu.Name) { $firstCpu.Name.Trim() } else { "Unknown" }
$cpuCount = if ($cpu) { ($cpu | Measure-Object).Count } else { 1 }
$cpuCores = if ($firstCpu -and $firstCpu.NumberOfCores) { $firstCpu.NumberOfCores } else { 0 }
$cpuLogical = if ($firstCpu -and $firstCpu.NumberOfLogicalProcessors) { $firstCpu.NumberOfLogicalProcessors } else { 0 }

$virtFirmwareEnabled = if ($firstCpu -and $null -ne $firstCpu.VirtualizationFirmwareEnabled) {
    if ([bool]$firstCpu.VirtualizationFirmwareEnabled) { "TRUE" } else { "FALSE" }
} else {
    "UNKNOWN"
}

$vmMonitorModeExt = if ($firstCpu -and $null -ne $firstCpu.VMMonitorModeExtensions) {
    if ([bool]$firstCpu.VMMonitorModeExtensions) { "TRUE" } else { "FALSE" }
} else {
    "UNKNOWN"
}

$slatExt = if ($firstCpu -and $null -ne $firstCpu.SecondLevelAddressTranslationExtensions) {
    if ([bool]$firstCpu.SecondLevelAddressTranslationExtensions) { "TRUE" } else { "FALSE" }
} else {
    "UNKNOWN"
}

# 4. Known Virtual Machine Signatures (Strict detection of guest environment)
$knownVmSignatures = @("VirtualBox", "VMware", "Virtual Machine", "KVM", "QEMU", "Bochs", "Xen")
$isKnownGuest = $false
foreach ($sig in $knownVmSignatures) {
    if ($manufacturer -match $sig -or $model -match $sig) {
        $isKnownGuest = $true
        break
    }
}

# 5. Storage Inspection: System Drive & Target VM Storage Volume
$systemDrive = "C:"
$systemFreeSpaceGiB = 0.0
try {
    $sysVol = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$systemDrive'" -ErrorAction SilentlyContinue
    if ($sysVol -and $sysVol.FreeSpace) {
        $systemFreeSpaceGiB = [math]::Round($sysVol.FreeSpace / 1GB, 2)
    }
} catch {
    $systemFreeSpaceGiB = 0.0
}

$vmStorageStatus = "UNKNOWN"
$vmStorageVolumeId = ""
$vmStorageFileSystem = ""
$vmStorageFreeSpaceGiB = 0.0

if (-not [string]::IsNullOrWhiteSpace($VmStoragePath)) {
    try {
        $resolvedPath = [System.IO.Path]::GetFullPath($VmStoragePath)
        $pathRoot = [System.IO.Path]::GetPathRoot($resolvedPath)
        if (-not [string]::IsNullOrWhiteSpace($pathRoot)) {
            $driveId = $pathRoot.TrimEnd('\')
            $targetVol = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DeviceID='$driveId'" -ErrorAction SilentlyContinue
            if ($targetVol -and $targetVol.FreeSpace) {
                $vmStorageVolumeId = $targetVol.DeviceID
                $vmStorageFileSystem = if ($targetVol.FileSystem) { $targetVol.FileSystem } else { "Unknown" }
                $vmStorageFreeSpaceGiB = [math]::Round($targetVol.FreeSpace / 1GB, 2)
                $vmStorageStatus = "RESOLVED"
            } else {
                $vmStorageStatus = "DRIVE_NOT_FOUND"
                Write-Warning "Could not retrieve logical disk info for path root: $driveId"
            }
        }
    } catch {
        $vmStorageStatus = "PATH_RESOLUTION_ERROR"
        Write-Warning "Failed to resolve VmStoragePath '$VmStoragePath': $($_.ToString())"
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
    Manufacturer                           = $manufacturer
    Model                                  = $model
    VirtualizationFirmwareEnabled          = $virtFirmwareEnabled
    VMMonitorModeExtensions                = $vmMonitorModeExt
    SecondLevelAddressTranslationExtensions= $slatExt
    TotalVisibleMemoryGiB                  = $totalMemoryGiB
    FreePhysicalMemoryGiB                  = $freeMemoryGiB
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
    Manufacturer                           = $manufacturer
    Model                                  = $model
    HypervisorPresent                      = $hypervisorPresent
    OSCaption                              = if ($os -and $os.Caption) { $os.Caption } else { "Unknown" }
    OSVersion                              = if ($os -and $os.Version) { $os.Version } else { "Unknown" }
    OSBuildNumber                          = if ($os -and $os.BuildNumber) { $os.BuildNumber } else { "Unknown" }
    CPUName                                = $cpuName
    CPUPhysicalCount                       = $cpuCount
    CPUCores                               = $cpuCores
    CPULogicalProcessors                   = $cpuLogical
    TotalVisibleMemoryGiB                  = $totalMemoryGiB
    FreePhysicalMemoryGiB                  = $freeMemoryGiB
    VirtualizationFirmwareEnabled          = $virtFirmwareEnabled
    VMMonitorModeExtensions                = $vmMonitorModeExt
    SecondLevelAddressTranslationExtensions= $slatExt
    BIOSManufacturer                       = if ($bios -and $bios.Manufacturer) { $bios.Manufacturer } else { "Unknown" }
    BIOSVersion                            = if ($bios -and $bios.SMBIOSBIOSVersion) { $bios.SMBIOSBIOSVersion } else { "Unknown" }
    SecureBoot                             = $secureBootStatus
    SystemDriveFreeSpaceGiB                = $systemFreeSpaceGiB
    VmStoragePathRequested                 = $VmStoragePath
    VmStorageStatus                        = $vmStorageStatus
    VmStorageVolume                        = $vmStorageVolumeId
    VmStorageFileSystem                    = $vmStorageFileSystem
    VmStorageFreeSpaceGiB                  = $vmStorageFreeSpaceGiB
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
        Write-Host ('`nDiagnostic snapshot saved to: ' + $OutputPath) -ForegroundColor Green
    } catch {
        $msg = $_.ToString()
        Write-Error $msg
    }
}

return $result

<#
.SYNOPSIS
    Pure classification and validation logic module for PrivGuard L0 Physical Host Preflight.

.DESCRIPTION
    Contains the pure, deterministic functions used by Get-L0HostPreflight.ps1 and Test-L0HostPreflightLogic.ps1:
    - Get-L0HostClassification: Evaluates hardware and diagnostic metrics into standard L0 classifications.
    - Resolve-L0VmStoragePathFormat: Validates that the requested storage path is a valid local Windows drive path.
    - Get-L0CpuAggregation: Aggregates cores, logical processors, and virtualization flags conservatively across CPU sockets.

    Strictly decoupled from CIM/WMI queries to enable hermetic testing and prevent regressions.
#>

function Resolve-L0VmStoragePathFormat {
    [CmdletBinding()]
    param (
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return @{
            IsValid = $false
            Status  = "PATH_EMPTY"
            Reason  = "Storage path is null, empty, or whitespace."
            DriveId = $null
        }
    }

    # Reject UNC paths (\\server\share or //server/share)
    if ($Path.StartsWith("\\") -or $Path.StartsWith("//")) {
        return @{
            IsValid = $false
            Status  = "UNC_PATH_NOT_SUPPORTED"
            Reason  = "UNC network share paths ($Path) are not supported. Target storage must be an absolute local Windows drive (e.g. D:\VMs)."
            DriveId = $null
        }
    }

    # Strict check for absolute local drive root (e.g. "C:\" or "D:/", rejecting relative paths like "C:dir" or "relative\path")
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($Path, '^[a-zA-Z]:[\\/]')) {
        return @{
            IsValid = $false
            Status  = "INVALID_PATH_FORMAT"
            Reason  = "Path '$Path' is not an absolute local drive path. Path must begin with a drive letter and slash (e.g. 'D:\...')."
            DriveId = $null
        }
    }

    $driveLetter = $Path.Substring(0, 1).ToUpper()
    $driveId = "${driveLetter}:"

    return @{
        IsValid = $true
        Status  = "FORMAT_VALID"
        Reason  = "Path format is a valid local drive path."
        DriveId = $driveId
    }
}

function Get-L0CpuAggregation {
    [CmdletBinding()]
    param (
        [array]$Processors
    )

    if ($null -eq $Processors -or $Processors.Count -eq 0) {
        return @{
            CPUCores                                = $null
            CPULogicalProcessors                    = $null
            VirtualizationFirmwareEnabled           = "UNKNOWN"
            SecondLevelAddressTranslationExtensions = "UNKNOWN"
            VMMonitorModeExtensions                 = "UNKNOWN"
            PhysicalSocketCount                     = 0
        }
    }

    $coresSum = 0
    $logicalSum = 0
    $hasCores = $false
    $hasLogical = $false

    $virtFirmwareList = [System.Collections.Generic.List[string]]::new()
    $slatList         = [System.Collections.Generic.List[string]]::new()
    $vmmExtList       = [System.Collections.Generic.List[string]]::new()

    foreach ($proc in $Processors) {
        if ($null -ne $proc) {
            if ($null -ne $proc.NumberOfCores -and $proc.NumberOfCores -ge 0) {
                $coresSum += [int]$proc.NumberOfCores
                $hasCores = $true
            }
            if ($null -ne $proc.NumberOfLogicalProcessors -and $proc.NumberOfLogicalProcessors -ge 0) {
                $logicalSum += [int]$proc.NumberOfLogicalProcessors
                $hasLogical = $true
            }

            # VirtualizationFirmwareEnabled
            if ($null -ne $proc.VirtualizationFirmwareEnabled) {
                if ($proc.VirtualizationFirmwareEnabled -eq $true -or $proc.VirtualizationFirmwareEnabled -eq "TRUE") {
                    $virtFirmwareList.Add("TRUE")
                } elseif ($proc.VirtualizationFirmwareEnabled -eq $false -or $proc.VirtualizationFirmwareEnabled -eq "FALSE") {
                    $virtFirmwareList.Add("FALSE")
                } else {
                    $virtFirmwareList.Add("UNKNOWN")
                }
            } else {
                $virtFirmwareList.Add("UNKNOWN")
            }

            # SecondLevelAddressTranslationExtensions
            if ($null -ne $proc.SecondLevelAddressTranslationExtensions) {
                if ($proc.SecondLevelAddressTranslationExtensions -eq $true -or $proc.SecondLevelAddressTranslationExtensions -eq "TRUE") {
                    $slatList.Add("TRUE")
                } elseif ($proc.SecondLevelAddressTranslationExtensions -eq $false -or $proc.SecondLevelAddressTranslationExtensions -eq "FALSE") {
                    $slatList.Add("FALSE")
                } else {
                    $slatList.Add("UNKNOWN")
                }
            } else {
                $slatList.Add("UNKNOWN")
            }

            # VMMonitorModeExtensions
            if ($null -ne $proc.VMMonitorModeExtensions) {
                if ($proc.VMMonitorModeExtensions -eq $true -or $proc.VMMonitorModeExtensions -eq "TRUE") {
                    $vmmExtList.Add("TRUE")
                } elseif ($proc.VMMonitorModeExtensions -eq $false -or $proc.VMMonitorModeExtensions -eq "FALSE") {
                    $vmmExtList.Add("FALSE")
                } else {
                    $vmmExtList.Add("UNKNOWN")
                }
            } else {
                $vmmExtList.Add("UNKNOWN")
            }
        }
    }

    # Conservative boolean aggregation:
    # If ANY socket has FALSE -> FALSE (virtualization fails if any CPU lacks capability).
    # Else if ANY socket has UNKNOWN -> UNKNOWN.
    # Only if ALL sockets have TRUE -> TRUE.
    $aggregateFlag = {
        param([System.Collections.Generic.List[string]]$list)
        if ($list.Count -eq 0) { return "UNKNOWN" }
        if ($list.Contains("FALSE")) { return "FALSE" }
        if ($list.Contains("UNKNOWN")) { return "UNKNOWN" }
        return "TRUE"
    }

    return @{
        CPUCores                                = if ($hasCores) { $coresSum } else { $null }
        CPULogicalProcessors                    = if ($hasLogical) { $logicalSum } else { $null }
        VirtualizationFirmwareEnabled           = & $aggregateFlag $virtFirmwareList
        SecondLevelAddressTranslationExtensions = & $aggregateFlag $slatList
        VMMonitorModeExtensions                 = & $aggregateFlag $vmmExtList
        PhysicalSocketCount                     = $Processors.Count
    }
}

function Get-L0HostClassification {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [hashtable]$Metrics
    )

    $reasons = [System.Collections.Generic.List[string]]::new()

    # 1. Guest Detection (Strict Precedence: a virtual guest cannot validate physical L0 host capacity)
    if ($Metrics.ContainsKey("IsKnownVirtualGuest") -and $Metrics.IsKnownVirtualGuest -eq $true) {
        $mfg = if ($Metrics.ContainsKey("Manufacturer") -and -not [string]::IsNullOrWhiteSpace($Metrics.Manufacturer)) { $Metrics.Manufacturer } else { "Unknown" }
        $mdl = if ($Metrics.ContainsKey("Model") -and -not [string]::IsNullOrWhiteSpace($Metrics.Model)) { $Metrics.Model } else { "Unknown" }
        $reasons.Add("Host matches a known virtual machine signature ($mfg / $mdl). Physical L0 host cannot be verified from within a guest.")
        return @{
            Classification = "GUEST_DETECTED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 2. Check for Incomplete / Unknown Critical Diagnostics
    $incomplete = $false

    # 2.1 System Identity Status
    $idStatus = if ($Metrics.ContainsKey("ComputerSystemIdentityStatus")) { $Metrics.ComputerSystemIdentityStatus } else { "UNKNOWN" }
    $mfgVal   = if ($Metrics.ContainsKey("Manufacturer")) { $Metrics.Manufacturer } else { "UNKNOWN" }
    $mdlVal   = if ($Metrics.ContainsKey("Model")) { $Metrics.Model } else { "UNKNOWN" }

    if ($idStatus -ne "RESOLVED" -or [string]::IsNullOrWhiteSpace($mfgVal) -or $mfgVal -eq "UNKNOWN" -or [string]::IsNullOrWhiteSpace($mdlVal) -or $mdlVal -eq "UNKNOWN") {
        $reasons.Add("Host system identity (Manufacturer / Model) could not be verified from CIM query Win32_ComputerSystem.")
        $incomplete = $true
    }

    # 2.2 Hardware Virtualization Flags (Authoritative: Firmware & SLAT)
    if (-not $Metrics.ContainsKey("VirtualizationFirmwareEnabled") -or $Metrics.VirtualizationFirmwareEnabled -eq "UNKNOWN" -or $null -eq $Metrics.VirtualizationFirmwareEnabled) {
        $reasons.Add("VirtualizationFirmwareEnabled could not be determined from processor CIM query.")
        $incomplete = $true
    }
    if (-not $Metrics.ContainsKey("SecondLevelAddressTranslationExtensions") -or $Metrics.SecondLevelAddressTranslationExtensions -eq "UNKNOWN" -or $null -eq $Metrics.SecondLevelAddressTranslationExtensions) {
        $reasons.Add("SecondLevelAddressTranslationExtensions (SLAT) could not be determined from processor CIM query.")
        $incomplete = $true
    }

    # 2.3 Memory Metrics Availability
    if (-not $Metrics.ContainsKey("TotalVisibleMemoryGiB") -or $null -eq $Metrics.TotalVisibleMemoryGiB -or $Metrics.TotalVisibleMemoryGiB -eq "UNKNOWN") {
        $reasons.Add("TotalVisibleMemoryGiB could not be queried reliably.")
        $incomplete = $true
    }
    if (-not $Metrics.ContainsKey("FreePhysicalMemoryGiB") -or $null -eq $Metrics.FreePhysicalMemoryGiB -or $Metrics.FreePhysicalMemoryGiB -eq "UNKNOWN") {
        $reasons.Add("FreePhysicalMemoryGiB could not be queried reliably.")
        $incomplete = $true
    }

    # 2.4 CPU Core Metrics Availability
    if (-not $Metrics.ContainsKey("CPUCores") -or $null -eq $Metrics.CPUCores -or $Metrics.CPUCores -eq "UNKNOWN" -or
        -not $Metrics.ContainsKey("CPULogicalProcessors") -or $null -eq $Metrics.CPULogicalProcessors -or $Metrics.CPULogicalProcessors -eq "UNKNOWN") {
        $reasons.Add("Host CPU cores / logical processors could not be queried reliably.")
        $incomplete = $true
    }

    # 2.5 Target Storage Volume Path Resolution
    $vmStorageStatus = if ($Metrics.ContainsKey("VmStorageStatus")) { $Metrics.VmStorageStatus } else { "UNKNOWN" }
    $vmStorageVol    = if ($Metrics.ContainsKey("VmStorageVolume")) { $Metrics.VmStorageVolume } else { "" }
    if ($vmStorageStatus -ne "RESOLVED" -or [string]::IsNullOrWhiteSpace($vmStorageVol) -or
        -not $Metrics.ContainsKey("VmStorageFreeSpaceGiB") -or $null -eq $Metrics.VmStorageFreeSpaceGiB -or $Metrics.VmStorageFreeSpaceGiB -eq "UNKNOWN") {
        $reasons.Add("Target VM storage path (-VmStoragePath) was not specified or could not be resolved. Target storage volume cannot be verified.")
        $incomplete = $true
    }

    if ($incomplete) {
        return @{
            Classification = "PREFLIGHT_INCOMPLETE"
            Reasons        = $reasons.ToArray()
        }
    }

    # Cast numeric metrics safely
    [double]$totalRam = [double]$Metrics.TotalVisibleMemoryGiB
    [double]$freeRam  = [double]$Metrics.FreePhysicalMemoryGiB
    [double]$freeDisk = [double]$Metrics.VmStorageFreeSpaceGiB
    [int]$cores       = [int]$Metrics.CPUCores
    [int]$logical     = [int]$Metrics.CPULogicalProcessors

    # 3. Hard Blocking Conditions (Below Minimum Practical Baseline)
    $blocked = $false

    if ($Metrics.VirtualizationFirmwareEnabled -eq $false -or $Metrics.VirtualizationFirmwareEnabled -eq "FALSE") {
        $reasons.Add("Hardware virtualization (VT-x / AMD-V) is disabled in host BIOS/UEFI firmware.")
        $blocked = $true
    }
    if ($Metrics.SecondLevelAddressTranslationExtensions -eq $false -or $Metrics.SecondLevelAddressTranslationExtensions -eq "FALSE") {
        $reasons.Add("Second-Level Address Translation (SLAT / EPT / NPT) is not supported by host CPU.")
        $blocked = $true
    }

    # VMMonitorModeExtensions Evaluation:
    # If HypervisorPresent is TRUE on physical host (e.g. Hyper-V root partition / VBS), VMMonitorModeExtensions
    # may report FALSE/UNKNOWN because the hypervisor already claims VMX root. In that case, it is diagnostic.
    # However, if HypervisorPresent is NOT true and VMMonitorModeExtensions is explicitly FALSE, the CPU lacks VMM capability.
    $hypervisorActive = ($Metrics.ContainsKey("HypervisorPresent") -and ($Metrics.HypervisorPresent -eq $true -or $Metrics.HypervisorPresent -eq "TRUE"))
    $vmmExtVal = if ($Metrics.ContainsKey("VMMonitorModeExtensions")) { $Metrics.VMMonitorModeExtensions } else { "UNKNOWN" }

    if (-not $hypervisorActive -and ($vmmExtVal -eq $false -or $vmmExtVal -eq "FALSE")) {
        $reasons.Add("Virtual Machine Monitor Mode Extensions (VT-x / AMD-V) are not supported by the host CPU.")
        $blocked = $true
    }

    if ($totalRam -lt 12.0) {
        $reasons.Add("Host total physical RAM ($totalRam GiB) is below minimum practical requirement (12.0 GiB) to support Host OS + Windows 11 VM.")
        $blocked = $true
    }
    if ($freeRam -lt 5.0) {
        $reasons.Add("Host free physical RAM ($freeRam GiB) is below minimum practical requirement (5.0 GiB) to allocate to Windows 11 VM.")
        $blocked = $true
    }
    if ($freeDisk -lt 70.0) {
        $reasons.Add("Target VM volume free space ($freeDisk GiB) is below minimum practical requirement (70.0 GiB) for base virtual disk (>=64 GiB) and snapshots.")
        $blocked = $true
    }
    if ($cores -lt 2 -or $logical -lt 4) {
        $reasons.Add("Host CPU capacity ($cores cores / $logical logical) is below minimum practical requirement (2 cores / 4 logical).")
        $blocked = $true
    }

    if ($blocked) {
        return @{
            Classification = "L0_PREFLIGHT_BLOCKED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 4. Constrained Conditions (Meets practical minimums, but below recommended goals)
    $constrained = $false

    if ($totalRam -lt 16.0) {
        $reasons.Add("Host total physical RAM ($totalRam GiB) is constrained (< 16.0 GiB). VM must be allocated maximum 4.0 - 5.0 GiB vRAM.")
        $constrained = $true
    }
    if ($freeRam -lt 8.0) {
        $reasons.Add("Host free physical RAM ($freeRam GiB) is constrained (< 8.0 GiB). Host background applications must be closed before launching VM.")
        $constrained = $true
    }
    if ($freeDisk -lt 100.0) {
        $reasons.Add("Target VM volume free space ($freeDisk GiB) is constrained (< 100.0 GiB). Differential snapshot chain depth must be monitored strictly.")
        $constrained = $true
    }
    if ($cores -lt 4 -or $logical -lt 8) {
        $reasons.Add("Host CPU capacity ($cores cores / $logical logical) is constrained (< 4 cores / 8 logical). VM should be allocated at most 2 vCPUs.")
        $constrained = $true
    }

    if ($constrained) {
        return @{
            Classification = "L0_CAPABLE_CONSTRAINED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 5. Full Pass Candidate (All measured hardware metrics meet recommended goals)
    $reasons.Add("All measured physical host criteria meet recommended baseline for Topology B (hardware virtualization, SLAT, >=16 GiB RAM, >=8 GiB free RAM, >=100 GiB target free storage, >=4 cores/8 logical).")
    $reasons.Add("Note: PASS_CANDIDATE confirms host hardware capacity; VM provisioning, vTPM 2.0, Secure Boot, FUS user accounts, and snapshot validation remain mandatory manual lab gates.")
    return @{
        Classification = "L0_PREFLIGHT_PASS_CANDIDATE"
        Reasons        = $reasons.ToArray()
    }
}

Export-ModuleMember -Function Get-L0HostClassification, Resolve-L0VmStoragePathFormat, Get-L0CpuAggregation

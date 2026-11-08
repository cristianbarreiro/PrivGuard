<#
.SYNOPSIS
    Pure classification, validation, and aggregation logic module for PrivGuard L0 Physical Host Preflight.

.DESCRIPTION
    Contains pure, deterministic functions used by Get-L0HostPreflight.ps1 and Test-L0HostPreflightLogic.ps1:
    - ConvertTo-L0NormalizedBoolean: Strict allowlist conversion for boolean/flag metrics.
    - Test-L0ValidDouble / Test-L0ValidInt: Strict numeric validation rejecting NaN, Infinity, negative values, and non-numbers.
    - Resolve-L0VmStoragePathFormat: Validates local Windows drive path syntax (rejecting UNC, relative, drive-relative, empty).
    - Test-L0StorageTargetResolution: Evaluates storage path format, directory existence/container type, and DriveType allowlist.
    - Get-L0CpuAggregation: Aggregates cores, logical processors, and virtualization flags fail-closed across CPU sockets.
    - Get-L0HostClassification: Evaluates hardware and diagnostic metrics into standard L0 classifications.

    Strictly decoupled from CIM/WMI queries to enable hermetic testing and prevent regressions.
#>

function ConvertTo-L0NormalizedBoolean {
    param ($Value)
    if ($null -eq $Value) { return "UNKNOWN" }
    if ($Value -is [bool]) {
        if ($Value) { return "TRUE" } else { return "FALSE" }
    }
    if ($Value -is [string]) {
        $trimmed = $Value.Trim().ToUpperInvariant()
        if ($trimmed -eq "TRUE") { return "TRUE" }
        if ($trimmed -eq "FALSE") { return "FALSE" }
        return "UNKNOWN"
    }
    return "UNKNOWN"
}

function Test-L0ValidDouble {
    param (
        $Value,
        [ref]$OutDouble
    )
    $OutDouble.Value = 0.0
    if ($null -eq $Value) { return $false }
    if ($Value -is [double] -or $Value -is [float] -or $Value -is [decimal] -or
        $Value -is [int] -or $Value -is [long] -or $Value -is [int16] -or $Value -is [byte] -or
        $Value -is [uint32] -or $Value -is [uint64] -or $Value -is [uint16] -or $Value -is [sbyte]) {
        [double]$d = [double]$Value
        if ([double]::IsNaN($d) -or [double]::IsInfinity($d) -or $d -lt 0.0) {
            return $false
        }
        $OutDouble.Value = $d
        return $true
    }
    if ($Value -is [string]) {
        $str = $Value.Trim()
        if ([string]::IsNullOrWhiteSpace($str)) { return $false }
        [double]$d = 0.0
        if ([double]::TryParse($str, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$d) -or
            [double]::TryParse($str, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::CurrentCulture, [ref]$d)) {
            if ([double]::IsNaN($d) -or [double]::IsInfinity($d) -or $d -lt 0.0) {
                return $false
            }
            $OutDouble.Value = $d
            return $true
        }
    }
    return $false
}

function Test-L0ValidInt {
    param (
        $Value,
        [ref]$OutInt
    )
    $OutInt.Value = 0
    if ($null -eq $Value) { return $false }
    if ($Value -is [int] -or $Value -is [long] -or $Value -is [int16] -or $Value -is [byte] -or
        $Value -is [uint32] -or $Value -is [uint64] -or $Value -is [uint16] -or $Value -is [sbyte]) {
        [long]$l = [long]$Value
        if ($l -lt 0 -or $l -gt [int]::MaxValue) { return $false }
        $OutInt.Value = [int]$l
        return $true
    }
    if ($Value -is [string]) {
        $str = $Value.Trim()
        if ([string]::IsNullOrWhiteSpace($str)) { return $false }
        [int]$i = 0
        if ([int]::TryParse($str, [System.Globalization.NumberStyles]::Integer, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$i)) {
            if ($i -ge 0) {
                $OutInt.Value = $i
                return $true
            }
        }
    }
    return $false
}

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

    $driveLetter = $Path.Substring(0, 1).ToUpperInvariant()
    $driveId = "${driveLetter}:"

    return @{
        IsValid = $true
        Status  = "FORMAT_VALID"
        Reason  = "Path format is a valid local drive path."
        DriveId = $driveId
    }
}

function Test-L0StorageTargetResolution {
    [CmdletBinding()]
    param (
        [string]$Path,
        [bool]$DirectoryExists = $true,
        [bool]$IsDirectory = $true,
        $LogicalDisk = $null
    )

    $fmt = Resolve-L0VmStoragePathFormat -Path $Path
    if (-not $fmt.IsValid) {
        return @{
            Status         = $fmt.Status
            Reason         = $fmt.Reason
            DriveId        = $null
            FileSystem     = "Unknown"
            FreeSpaceGiB   = $null
        }
    }

    if (-not $DirectoryExists) {
        return @{
            Status         = "DIRECTORY_NOT_FOUND"
            Reason         = "Target directory '$Path' does not exist. The directory must be created before running preflight."
            DriveId        = $fmt.DriveId
            FileSystem     = "Unknown"
            FreeSpaceGiB   = $null
        }
    }

    if (-not $IsDirectory) {
        return @{
            Status         = "PATH_NOT_A_DIRECTORY"
            Reason         = "Target path '$Path' is a file, not a directory."
            DriveId        = $fmt.DriveId
            FileSystem     = "Unknown"
            FreeSpaceGiB   = $null
        }
    }

    if ($null -eq $LogicalDisk) {
        return @{
            Status         = "VOLUME_NOT_FOUND"
            Reason         = "Logical disk volume for '$($fmt.DriveId)' was not found."
            DriveId        = $fmt.DriveId
            FileSystem     = "Unknown"
            FreeSpaceGiB   = $null
        }
    }

    # Strict allowlist: DriveType must be 3 (Local Fixed Disk)
    $hasDt = ($LogicalDisk.PSObject.Properties.Match('DriveType').Count -gt 0)
    $dt = if ($hasDt) { $LogicalDisk.DriveType } else { $null }

    if ($null -eq $dt -or $dt -ne 3) {
        $typeName = switch ($dt) {
            2 { "Removable Disk" }
            4 { "Network Drive" }
            5 { "Compact Disc" }
            default { "Unknown/Unsupported ($dt)" }
        }
        return @{
            Status         = "VOLUME_NOT_LOCAL_FIXED"
            Reason         = "Volume '$($fmt.DriveId)' has DriveType $typeName. Only local fixed disks (DriveType = 3) are supported for VM virtual disks."
            DriveId        = $fmt.DriveId
            FileSystem     = if ($LogicalDisk.FileSystem) { $LogicalDisk.FileSystem } else { "Unknown" }
            FreeSpaceGiB   = $null
        }
    }

    # Validate FreeSpace
    $hasFs = ($LogicalDisk.PSObject.Properties.Match('FreeSpace').Count -gt 0)
    $fsVal = if ($hasFs) { $LogicalDisk.FreeSpace } else { $null }

    if ($null -eq $fsVal) {
        return @{
            Status         = "VOLUME_SPACE_UNKNOWN"
            Reason         = "Free space could not be determined for volume '$($fmt.DriveId)'."
            DriveId        = $fmt.DriveId
            FileSystem     = if ($LogicalDisk.FileSystem) { $LogicalDisk.FileSystem } else { "Unknown" }
            FreeSpaceGiB   = $null
        }
    }

    [double]$freeGiB = [math]::Round([double]$fsVal / 1GB, 2)
    return @{
        Status         = "RESOLVED"
        Reason         = "Storage target format, directory existence, volume type, and free space verified successfully."
        DriveId        = $fmt.DriveId
        FileSystem     = if ($LogicalDisk.FileSystem) { $LogicalDisk.FileSystem } else { "Unknown" }
        FreeSpaceGiB   = $freeGiB
    }
}

function Get-L0CpuAggregation {
    [CmdletBinding()]
    param (
        [array]$Processors
    )

    if ($null -eq $Processors -or $Processors.Count -eq 0) {
        return @{
            IsInventoryComplete                     = $false
            CPUCores                                = $null
            CPULogicalProcessors                    = $null
            VirtualizationFirmwareEnabled           = "UNKNOWN"
            SecondLevelAddressTranslationExtensions = "UNKNOWN"
            VMMonitorModeExtensions                 = "UNKNOWN"
            PhysicalSocketCount                     = 0
            InventoryErrors                         = @("No processor instances provided or array is empty.")
        }
    }

    $errors = [System.Collections.Generic.List[string]]::new()
    $coresSum = 0
    $logicalSum = 0
    $validCores = $true
    $validLogical = $true

    $virtList = [System.Collections.Generic.List[string]]::new()
    $slatList = [System.Collections.Generic.List[string]]::new()
    $vmmList  = [System.Collections.Generic.List[string]]::new()

    for ($i = 0; $i -lt $Processors.Count; $i++) {
        $proc = $Processors[$i]
        $socketLabel = "Socket $i"

        if ($null -eq $proc) {
            $errors.Add("$socketLabel is null.")
            $validCores = $false
            $validLogical = $false
            $virtList.Add("UNKNOWN")
            $slatList.Add("UNKNOWN")
            $vmmList.Add("UNKNOWN")
            continue
        }

        # Validate NumberOfCores on this socket
        $cVal = 0
        $hasCoresProp = ($proc.PSObject.Properties.Match('NumberOfCores').Count -gt 0)
        if (-not $hasCoresProp -or -not (Test-L0ValidInt -Value $proc.NumberOfCores -OutInt ([ref]$cVal))) {
            $errors.Add("$socketLabel has missing, null, or invalid NumberOfCores.")
            $validCores = $false
        } else {
            $coresSum += $cVal
        }

        # Validate NumberOfLogicalProcessors on this socket
        $lVal = 0
        $hasLogProp = ($proc.PSObject.Properties.Match('NumberOfLogicalProcessors').Count -gt 0)
        if (-not $hasLogProp -or -not (Test-L0ValidInt -Value $proc.NumberOfLogicalProcessors -OutInt ([ref]$lVal))) {
            $errors.Add("$socketLabel has missing, null, or invalid NumberOfLogicalProcessors.")
            $validLogical = $false
        } else {
            $logicalSum += $lVal
        }

        # Virtualization flags per socket (allowlist via ConvertTo-L0NormalizedBoolean)
        $hasVirtProp = ($proc.PSObject.Properties.Match('VirtualizationFirmwareEnabled').Count -gt 0)
        $virtNorm = if ($hasVirtProp) { ConvertTo-L0NormalizedBoolean $proc.VirtualizationFirmwareEnabled } else { "UNKNOWN" }
        $virtList.Add($virtNorm)

        $hasSlatProp = ($proc.PSObject.Properties.Match('SecondLevelAddressTranslationExtensions').Count -gt 0)
        $slatNorm = if ($hasSlatProp) { ConvertTo-L0NormalizedBoolean $proc.SecondLevelAddressTranslationExtensions } else { "UNKNOWN" }
        $slatList.Add($slatNorm)

        $hasVmmProp = ($proc.PSObject.Properties.Match('VMMonitorModeExtensions').Count -gt 0)
        $vmmNorm = if ($hasVmmProp) { ConvertTo-L0NormalizedBoolean $proc.VMMonitorModeExtensions } else { "UNKNOWN" }
        $vmmList.Add($vmmNorm)
    }

    # Fail-closed aggregation:
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

    $isInventoryComplete = ($errors.Count -eq 0)
    $resCores = if ($validCores -and $isInventoryComplete) { $coresSum } else { $null }
    $resLogical = if ($validLogical -and $isInventoryComplete) { $logicalSum } else { $null }

    return @{
        IsInventoryComplete                     = $isInventoryComplete
        CPUCores                                = $resCores
        CPULogicalProcessors                    = $resLogical
        VirtualizationFirmwareEnabled           = & $aggregateFlag $virtList
        SecondLevelAddressTranslationExtensions = & $aggregateFlag $slatList
        VMMonitorModeExtensions                 = & $aggregateFlag $vmmList
        PhysicalSocketCount                     = $Processors.Count
        InventoryErrors                         = $errors.ToArray()
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
    $isGuest = ($Metrics.ContainsKey("IsKnownVirtualGuest") -and ($Metrics.IsKnownVirtualGuest -eq $true -or $Metrics.IsKnownVirtualGuest -eq "TRUE"))
    if ($isGuest) {
        $mfg = if ($Metrics.ContainsKey("Manufacturer") -and -not [string]::IsNullOrWhiteSpace($Metrics.Manufacturer)) { $Metrics.Manufacturer } else { "Unknown" }
        $mdl = if ($Metrics.ContainsKey("Model") -and -not [string]::IsNullOrWhiteSpace($Metrics.Model)) { $Metrics.Model } else { "Unknown" }
        $reasons.Add("Host matches a known virtual machine signature ($mfg / $mdl). Physical L0 host cannot be verified from within a guest.")
        return @{
            Classification = "GUEST_DETECTED"
            Reasons        = $reasons.ToArray()
        }
    }

    # 2. Check for Incomplete / Unknown / Non-finite Critical Diagnostics
    $incomplete = $false

    # 2.1 System Identity Status
    $idStatus = if ($Metrics.ContainsKey("ComputerSystemIdentityStatus")) { $Metrics.ComputerSystemIdentityStatus } else { "UNKNOWN" }
    $mfgVal   = if ($Metrics.ContainsKey("Manufacturer")) { $Metrics.Manufacturer } else { "UNKNOWN" }
    $mdlVal   = if ($Metrics.ContainsKey("Model")) { $Metrics.Model } else { "UNKNOWN" }

    if ($idStatus -ne "RESOLVED" -or [string]::IsNullOrWhiteSpace($mfgVal) -or $mfgVal -eq "UNKNOWN" -or [string]::IsNullOrWhiteSpace($mdlVal) -or $mdlVal -eq "UNKNOWN") {
        $reasons.Add("Host system identity (Manufacturer / Model) could not be verified from CIM query Win32_ComputerSystem.")
        $incomplete = $true
    }

    # 2.2 Hardware Virtualization Flags (Authoritative: Firmware & SLAT allowlist)
    $virtNorm = if ($Metrics.ContainsKey("VirtualizationFirmwareEnabled")) { ConvertTo-L0NormalizedBoolean $Metrics.VirtualizationFirmwareEnabled } else { "UNKNOWN" }
    $slatNorm = if ($Metrics.ContainsKey("SecondLevelAddressTranslationExtensions")) { ConvertTo-L0NormalizedBoolean $Metrics.SecondLevelAddressTranslationExtensions } else { "UNKNOWN" }
    $vmmNorm  = if ($Metrics.ContainsKey("VMMonitorModeExtensions")) { ConvertTo-L0NormalizedBoolean $Metrics.VMMonitorModeExtensions } else { "UNKNOWN" }

    if ($virtNorm -eq "UNKNOWN") {
        $reasons.Add("VirtualizationFirmwareEnabled could not be determined from processor CIM query.")
        $incomplete = $true
    }
    if ($slatNorm -eq "UNKNOWN") {
        $reasons.Add("SecondLevelAddressTranslationExtensions (SLAT) could not be determined from processor CIM query.")
        $incomplete = $true
    }

    # 2.3 Memory Metrics Validation (Strict Non-negative, Non-NaN, Non-Infinity)
    [double]$totalRam = 0.0
    [double]$freeRam  = 0.0
    if (-not $Metrics.ContainsKey("TotalVisibleMemoryGiB") -or -not (Test-L0ValidDouble -Value $Metrics.TotalVisibleMemoryGiB -OutDouble ([ref]$totalRam))) {
        $reasons.Add("TotalVisibleMemoryGiB could not be queried reliably or is invalid/non-finite.")
        $incomplete = $true
    }
    if (-not $Metrics.ContainsKey("FreePhysicalMemoryGiB") -or -not (Test-L0ValidDouble -Value $Metrics.FreePhysicalMemoryGiB -OutDouble ([ref]$freeRam))) {
        $reasons.Add("FreePhysicalMemoryGiB could not be queried reliably or is invalid/non-finite.")
        $incomplete = $true
    }

    # 2.4 CPU Core Metrics Validation (Strict Non-negative Integers)
    [int]$cores   = 0
    [int]$logical = 0
    if (-not $Metrics.ContainsKey("CPUCores") -or -not (Test-L0ValidInt -Value $Metrics.CPUCores -OutInt ([ref]$cores))) {
        $reasons.Add("Host CPU cores could not be queried reliably or is invalid.")
        $incomplete = $true
    }
    if (-not $Metrics.ContainsKey("CPULogicalProcessors") -or -not (Test-L0ValidInt -Value $Metrics.CPULogicalProcessors -OutInt ([ref]$logical))) {
        $reasons.Add("Host CPU logical processors could not be queried reliably or is invalid.")
        $incomplete = $true
    }

    # 2.5 Target Storage Volume Path & Directory Resolution
    $vmStorageStatus = if ($Metrics.ContainsKey("VmStorageStatus")) { $Metrics.VmStorageStatus } else { "UNKNOWN" }
    $vmStorageVol    = if ($Metrics.ContainsKey("VmStorageVolume")) { $Metrics.VmStorageVolume } else { "" }
    [double]$freeDisk = 0.0

    if ($vmStorageStatus -ne "RESOLVED" -or [string]::IsNullOrWhiteSpace($vmStorageVol) -or
        -not $Metrics.ContainsKey("VmStorageFreeSpaceGiB") -or -not (Test-L0ValidDouble -Value $Metrics.VmStorageFreeSpaceGiB -OutDouble ([ref]$freeDisk))) {
        $reasons.Add("Target VM storage path (-VmStoragePath) was not specified, does not exist, is not a local fixed volume, or free space is invalid.")
        $incomplete = $true
    }

    if ($incomplete) {
        return @{
            Classification = "PREFLIGHT_INCOMPLETE"
            Reasons        = $reasons.ToArray()
        }
    }

    # 3. Hard Blocking Conditions (Below Minimum Practical Baseline)
    $blocked = $false

    if ($virtNorm -eq "FALSE") {
        $reasons.Add("Hardware virtualization (VT-x / AMD-V) is disabled in host BIOS/UEFI firmware.")
        $blocked = $true
    }
    if ($slatNorm -eq "FALSE") {
        $reasons.Add("Second-Level Address Translation (SLAT / EPT / NPT) is not supported by host CPU.")
        $blocked = $true
    }

    # VMMonitorModeExtensions Evaluation:
    # If HypervisorPresent is TRUE on physical host (e.g. Hyper-V root partition / VBS / WSL2), VMMonitorModeExtensions
    # may report FALSE/UNKNOWN because the root partition lacks direct VMX root access. In that case, it is diagnostic.
    # However, if HypervisorPresent is NOT true and VMMonitorModeExtensions is explicitly FALSE, the CPU lacks VMM capability.
    $hypervisorActive = ($Metrics.ContainsKey("HypervisorPresent") -and ($Metrics.HypervisorPresent -eq $true -or $Metrics.HypervisorPresent -eq "TRUE"))
    if (-not $hypervisorActive -and $vmmNorm -eq "FALSE") {
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

Export-ModuleMember -Function `
    ConvertTo-L0NormalizedBoolean, `
    Test-L0ValidDouble, `
    Test-L0ValidInt, `
    Resolve-L0VmStoragePathFormat, `
    Test-L0StorageTargetResolution, `
    Get-L0CpuAggregation, `
    Get-L0HostClassification

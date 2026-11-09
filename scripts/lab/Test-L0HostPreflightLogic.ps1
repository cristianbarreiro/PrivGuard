<#
.SYNOPSIS
    Automated hermetic unit tests for PrivGuard L0 Physical Host Preflight logic.

.DESCRIPTION
    Validates deterministic unit test cases for:
    1. ConvertTo-L0NormalizedBoolean: Strict allowlist conversion (rejecting "YES", "ENABLED", "1", etc.).
    2. Test-L0ValidDouble / Test-L0ValidInt: Strict numeric validity (rejecting NaN, Infinity, negative, non-numeric).
    3. Resolve-L0VmStoragePathFormat: Local Windows drive format validation (rejecting UNC, relative, drive-relative).
    4. Test-L0StorageTargetResolution: Directory existence, container verification, and DriveType allowlist (DriveType = 3).
    5. Get-L0CpuAggregation: Fail-closed multi-socket CPU core/thread summing and conservative virtualization aggregation.
    6. Get-L0HostClassification: Hardware and diagnostic metrics classification with strict allowlist and boundary tests.

    Hermetic execution: Zero CIM/WMI calls, zero hardware modifications, no administrator required.
#>

[CmdletBinding()]
param ()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Import the pure classification module directly
$modulePath = Join-Path $PSScriptRoot "L0HostPreflight.Logic.psm1"
if (-not (Test-Path $modulePath)) {
    throw "Module not found at $modulePath"
}

Import-Module $modulePath -Force

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Running Automated Unit Tests for L0 Host Classification " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$testResults = [System.Collections.Generic.List[pscustomobject]]::new()

function Record-TestResult {
    param(
        [string]$Category,
        [string]$TestName,
        [bool]$Passed,
        [string]$Expected,
        [string]$Actual,
        [string]$Details = ""
    )
    $status = if ($Passed) { "PASS" } else { "FAIL" }
    $color  = if ($Passed) { "Green" } else { "Red" }
    Write-Host "[$status] $TestName (Expected: $Expected | Got: $Actual)" -ForegroundColor $color
    $testResults.Add([pscustomobject]@{
        Category = $Category
        TestName = $TestName
        Expected = $Expected
        Actual   = $Actual
        Passed   = $Passed
        Details  = $Details
    })
}

# --- SECTION 1: STRICT BOOLEAN ALLOWLIST TESTS ---
$boolCases = @(
    @{ Name = "BOOL-01: True boolean";          Input = $true;       Expected = "TRUE" },
    @{ Name = "BOOL-02: False boolean";         Input = $false;      Expected = "FALSE" },
    @{ Name = "BOOL-03: String 'TRUE'";         Input = "TRUE";      Expected = "TRUE" },
    @{ Name = "BOOL-04: String 'false' (lower)";Input = "false";     Expected = "FALSE" },
    @{ Name = "BOOL-05: String with spaces";    Input = "  True  ";  Expected = "TRUE" },
    @{ Name = "BOOL-06: Disallowed 'YES'";      Input = "YES";       Expected = "UNKNOWN" },
    @{ Name = "BOOL-07: Disallowed 'ENABLED'";  Input = "ENABLED";   Expected = "UNKNOWN" },
    @{ Name = "BOOL-08: Disallowed '1'";        Input = "1";         Expected = "UNKNOWN" },
    @{ Name = "BOOL-09: Disallowed '0'";        Input = "0";         Expected = "UNKNOWN" },
    @{ Name = "BOOL-10: Null value";            Input = $null;       Expected = "UNKNOWN" },
    @{ Name = "BOOL-11: Empty string";          Input = "";          Expected = "UNKNOWN" }
)

foreach ($c in $boolCases) {
    try {
        $actual = ConvertTo-L0NormalizedBoolean $c.Input
        Record-TestResult -Category "BooleanAllowlist" -TestName $c.Name -Passed ($actual -eq $c.Expected) -Expected $c.Expected -Actual $actual
    } catch {
        Record-TestResult -Category "BooleanAllowlist" -TestName $c.Name -Passed $false -Expected $c.Expected -Actual "EXCEPTION: $_"
    }
}

# --- SECTION 2: STRICT NUMERIC VALIDATION TESTS ---
[double]$outD = 0.0
$numDoubleCases = @(
    @{ Name = "NUM-01: Valid double (16.0)";     Input = 16.0;                    Expected = $true },
    @{ Name = "NUM-02: Valid zero (0.0)";        Input = 0.0;                     Expected = $true },
    @{ Name = "NUM-03: Valid string double";     Input = "12.5";                  Expected = $true },
    @{ Name = "NUM-04: Negative double (-1.0)";  Input = -1.0;                    Expected = $false },
    @{ Name = "NUM-05: Double NaN";              Input = [double]::NaN;           Expected = $false },
    @{ Name = "NUM-06: Double PositiveInfinity"; Input = [double]::PositiveInfinity; Expected = $false },
    @{ Name = "NUM-07: Double NegativeInfinity"; Input = [double]::NegativeInfinity; Expected = $false },
    @{ Name = "NUM-08: Non-numeric string";      Input = "invalid_num";           Expected = $false },
    @{ Name = "NUM-09: Null value";              Input = $null;                   Expected = $false }
)

foreach ($c in $numDoubleCases) {
    try {
        $actual = Test-L0ValidDouble -Value $c.Input -OutDouble ([ref]$outD)
        Record-TestResult -Category "NumericValidation" -TestName $c.Name -Passed ($actual -eq $c.Expected) -Expected ($c.Expected.ToString()) -Actual ($actual.ToString())
    } catch {
        Record-TestResult -Category "NumericValidation" -TestName $c.Name -Passed $false -Expected ($c.Expected.ToString()) -Actual "EXCEPTION: $_"
    }
}

[int]$outI = 0
$numIntCases = @(
    @{ Name = "NUM-10: Valid int (4)";           Input = 4;                       Expected = $true },
    @{ Name = "NUM-11: Valid zero int (0)";      Input = 0;                       Expected = $true },
    @{ Name = "NUM-12: Valid string int ('8')";  Input = "8";                     Expected = $true },
    @{ Name = "NUM-13: Negative int (-2)";       Input = -2;                      Expected = $false },
    @{ Name = "NUM-14: Fractional string ('2.5')";Input = "2.5";                  Expected = $false },
    @{ Name = "NUM-15: Non-numeric string ('x')";Input = "x";                     Expected = $false },
    @{ Name = "NUM-16: Null int";                Input = $null;                   Expected = $false }
)

foreach ($c in $numIntCases) {
    try {
        $actual = Test-L0ValidInt -Value $c.Input -OutInt ([ref]$outI)
        Record-TestResult -Category "NumericValidation" -TestName $c.Name -Passed ($actual -eq $c.Expected) -Expected ($c.Expected.ToString()) -Actual ($actual.ToString())
    } catch {
        Record-TestResult -Category "NumericValidation" -TestName $c.Name -Passed $false -Expected ($c.Expected.ToString()) -Actual "EXCEPTION: $_"
    }
}

# --- SECTION 3: STORAGE PATH FORMAT VALIDATION TESTS ---
$pathFmtCases = @(
    @{ Name = "PF-01: Empty storage path";      Path = "";                        ExpectedValid = $false; ExpectedStatus = "PATH_EMPTY" },
    @{ Name = "PF-02: Whitespace storage path"; Path = "   ";                     ExpectedValid = $false; ExpectedStatus = "PATH_EMPTY" },
    @{ Name = "PF-03: UNC network share path";  Path = "\\server\vm\disks";       ExpectedValid = $false; ExpectedStatus = "UNC_PATH_NOT_SUPPORTED" },
    @{ Name = "PF-04: Relative path";           Path = "VMs\Windows11";           ExpectedValid = $false; ExpectedStatus = "INVALID_PATH_FORMAT" },
    @{ Name = "PF-05: Drive-relative path";     Path = "D:VMs\Windows11";         ExpectedValid = $false; ExpectedStatus = "INVALID_PATH_FORMAT" },
    @{ Name = "PF-06: Valid local drive (\\)";  Path = "D:\PrivGuard-Lab\VMs";    ExpectedValid = $true;  ExpectedStatus = "FORMAT_VALID" },
    @{ Name = "PF-07: Valid local drive (//)";  Path = "C:/PrivGuard-Lab/VMs";    ExpectedValid = $true;  ExpectedStatus = "FORMAT_VALID" }
)

foreach ($c in $pathFmtCases) {
    try {
        $res = Resolve-L0VmStoragePathFormat -Path $c.Path
        $passed = ($res.IsValid -eq $c.ExpectedValid -and $res.Status -eq $c.ExpectedStatus)
        Record-TestResult -Category "PathFormat" -TestName $c.Name -Passed $passed -Expected "$($c.ExpectedValid):$($c.ExpectedStatus)" -Actual "$($res.IsValid):$($res.Status)" -Details $res.Reason
    } catch {
        Record-TestResult -Category "PathFormat" -TestName $c.Name -Passed $false -Expected "$($c.ExpectedValid):$($c.ExpectedStatus)" -Actual "EXCEPTION: $_"
    }
}

# --- SECTION 4: STORAGE TARGET RESOLUTION TESTS (Directory existence & DriveType allowlist) ---
$diskFixedValid = [pscustomobject]@{ DriveType = 3; FreeSpace = 161061273600; FileSystem = "NTFS" } # 150 GiB
$diskFixedZero  = [pscustomobject]@{ DriveType = 3; FreeSpace = 0;            FileSystem = "NTFS" }
$diskFixedNullFs= [pscustomobject]@{ DriveType = 3; FreeSpace = $null;        FileSystem = "NTFS" }
$diskRemovable  = [pscustomobject]@{ DriveType = 2; FreeSpace = 161061273600; FileSystem = "exFAT" }
$diskNetwork    = [pscustomobject]@{ DriveType = 4; FreeSpace = 161061273600; FileSystem = "SMB" }
$diskUnknownDt  = [pscustomobject]@{ DriveType = 0; FreeSpace = 161061273600; FileSystem = "RAW" }
$diskNullDt     = [pscustomobject]@{ DriveType = $null; FreeSpace = 161061273600; FileSystem = "NTFS" }

$storageResCases = @(
    @{ Name = "STR-01: Directory does not exist";       Path = "D:\NonExistent\VMs"; DirExists = $false; IsDir = $false; Disk = $diskFixedValid; ExpectedStatus = "DIRECTORY_NOT_FOUND" },
    @{ Name = "STR-02: Path is a file, not directory";   Path = "D:\VMs\disk.vhdx";   DirExists = $true;  IsDir = $false; Disk = $diskFixedValid; ExpectedStatus = "PATH_NOT_A_DIRECTORY" },
    @{ Name = "STR-03: Volume not found (null disk)";   Path = "Z:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $null;           ExpectedStatus = "VOLUME_NOT_FOUND" },
    @{ Name = "STR-04: DriveType = 2 (Removable)";       Path = "E:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskRemovable;  ExpectedStatus = "VOLUME_NOT_LOCAL_FIXED" },
    @{ Name = "STR-05: DriveType = 4 (Network Drive)";   Path = "Z:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskNetwork;    ExpectedStatus = "VOLUME_NOT_LOCAL_FIXED" },
    @{ Name = "STR-06: DriveType = 0 (Unknown)";         Path = "F:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskUnknownDt;  ExpectedStatus = "VOLUME_NOT_LOCAL_FIXED" },
    @{ Name = "STR-07: DriveType = null";               Path = "F:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskNullDt;     ExpectedStatus = "VOLUME_NOT_LOCAL_FIXED" },
    @{ Name = "STR-08: FreeSpace is null";              Path = "D:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskFixedNullFs;ExpectedStatus = "VOLUME_SPACE_UNKNOWN" },
    @{ Name = "STR-09: FreeSpace is 0 (measured)";      Path = "D:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskFixedZero;  ExpectedStatus = "RESOLVED" },
    @{ Name = "STR-10: All valid (Fixed, 150 GiB)";     Path = "D:\VMs";             DirExists = $true;  IsDir = $true;  Disk = $diskFixedValid; ExpectedStatus = "RESOLVED" }
)

foreach ($c in $storageResCases) {
    try {
        $res = Test-L0StorageTargetResolution -Path $c.Path -DirectoryExists $c.DirExists -IsDirectory $c.IsDir -LogicalDisk $c.Disk
        $passed = ($res.Status -eq $c.ExpectedStatus)
        Record-TestResult -Category "StorageTargetResolution" -TestName $c.Name -Passed $passed -Expected $c.ExpectedStatus -Actual $res.Status -Details $res.Reason
    } catch {
        Record-TestResult -Category "StorageTargetResolution" -TestName $c.Name -Passed $false -Expected $c.ExpectedStatus -Actual "EXCEPTION: $_"
    }
}

# --- SECTION 5: CPU MULTI-SOCKET AGGREGATION TESTS (Fail-Closed) ---
$cpuNormalSocket = @{
    NumberOfCores = 8
    NumberOfLogicalProcessors = 16
    VirtualizationFirmwareEnabled = $true
    SecondLevelAddressTranslationExtensions = $true
    VMMonitorModeExtensions = $true
}

$procSingle = @([pscustomobject]$cpuNormalSocket)
$procDualSymmetric = @([pscustomobject]$cpuNormalSocket, [pscustomobject]$cpuNormalSocket)

$procDualHeteroVirtFail = @(
    [pscustomobject]$cpuNormalSocket,
    [pscustomobject]@{
        NumberOfCores = 8
        NumberOfLogicalProcessors = 16
        VirtualizationFirmwareEnabled = $false
        SecondLevelAddressTranslationExtensions = $true
        VMMonitorModeExtensions = $true
    }
)

$procDualSocket1CoresNull = @(
    [pscustomobject]$cpuNormalSocket,
    [pscustomobject]@{
        NumberOfCores = $null
        NumberOfLogicalProcessors = 16
        VirtualizationFirmwareEnabled = $true
        SecondLevelAddressTranslationExtensions = $true
        VMMonitorModeExtensions = $true
    }
)

$procDualSocket1LogNull = @(
    [pscustomobject]$cpuNormalSocket,
    [pscustomobject]@{
        NumberOfCores = 8
        NumberOfLogicalProcessors = $null
        VirtualizationFirmwareEnabled = $true
        SecondLevelAddressTranslationExtensions = $true
        VMMonitorModeExtensions = $true
    }
)

$procNegativeCores = @(
    [pscustomobject]@{
        NumberOfCores = -4
        NumberOfLogicalProcessors = 8
        VirtualizationFirmwareEnabled = $true
        SecondLevelAddressTranslationExtensions = $true
        VMMonitorModeExtensions = $true
    }
)

$procWithNullElement = @(
    [pscustomobject]$cpuNormalSocket,
    $null
)

$cpuCases = @(
    @{ Name = "CPU-01: Single socket normal";               Procs = $procSingle;              ExpComplete = $true;  ExpCores = 8;    ExpLog = 16;  ExpVirt = "TRUE" },
    @{ Name = "CPU-02: Dual socket symmetric";              Procs = $procDualSymmetric;       ExpComplete = $true;  ExpCores = 16;   ExpLog = 32;  ExpVirt = "TRUE" },
    @{ Name = "CPU-03: Dual socket hetero Virt=FALSE";      Procs = $procDualHeteroVirtFail;  ExpComplete = $true;  ExpCores = 16;   ExpLog = 32;  ExpVirt = "FALSE" },
    @{ Name = "CPU-04: Socket 1 cores = null (fail-closed)";Procs = $procDualSocket1CoresNull; ExpComplete = $false; ExpCores = $null;ExpLog = $null;ExpVirt = "TRUE" },
    @{ Name = "CPU-05: Socket 1 logical=null (fail-closed)";Procs = $procDualSocket1LogNull;   ExpComplete = $false; ExpCores = $null;ExpLog = $null;ExpVirt = "TRUE" },
    @{ Name = "CPU-06: Negative cores (fail-closed)";       Procs = $procNegativeCores;       ExpComplete = $false; ExpCores = $null;ExpLog = $null;ExpVirt = "TRUE" },
    @{ Name = "CPU-07: Null element in array (fail-closed)";Procs = $procWithNullElement;     ExpComplete = $false; ExpCores = $null;ExpLog = $null;ExpVirt = "UNKNOWN" },
    @{ Name = "CPU-08: Empty processor array";              Procs = @();                      ExpComplete = $false; ExpCores = $null;ExpLog = $null;ExpVirt = "UNKNOWN" }
)

foreach ($c in $cpuCases) {
    try {
        $agg = Get-L0CpuAggregation -Processors $c.Procs
        $passed = ($agg.IsInventoryComplete -eq $c.ExpComplete -and
                   $agg.CPUCores -eq $c.ExpCores -and
                   $agg.CPULogicalProcessors -eq $c.ExpLog -and
                   $agg.VirtualizationFirmwareEnabled -eq $c.ExpVirt)
        Record-TestResult -Category "CpuAggregation" -TestName $c.Name -Passed $passed -Expected "Comp=$($c.ExpComplete),Cores=$($c.ExpCores),Log=$($c.ExpLog),Virt=$($c.ExpVirt)" -Actual "Comp=$($agg.IsInventoryComplete),Cores=$($agg.CPUCores),Log=$($agg.CPULogicalProcessors),Virt=$($agg.VirtualizationFirmwareEnabled)"
    } catch {
        Record-TestResult -Category "CpuAggregation" -TestName $c.Name -Passed $false -Expected "Comp=$($c.ExpComplete)" -Actual "EXCEPTION: $_"
    }
}

# --- SECTION 6: CLASSIFICATION DETERMINISTIC UNIT TESTS ---
$idealMetrics = @{
    IsKnownVirtualGuest                    = $false
    ComputerSystemIdentityStatus           = "RESOLVED"
    Manufacturer                           = "Dell Inc."
    Model                                  = "Precision 7920"
    VirtualizationFirmwareEnabled          = "TRUE"
    VMMonitorModeExtensions                = "TRUE"
    SecondLevelAddressTranslationExtensions= "TRUE"
    TotalVisibleMemoryGiB                  = 32.0
    FreePhysicalMemoryGiB                  = 18.0
    CPUCores                               = 8
    CPULogicalProcessors                   = 16
    VmStorageStatus                        = "RESOLVED"
    VmStorageVolume                        = "D:"
    VmStorageFreeSpaceGiB                  = 250.0
}

function Run-ClassificationTest {
    param(
        [string]$TestName,
        [hashtable]$MockMetrics,
        [string]$ExpectedClassification
    )

    try {
        $result = Get-L0HostClassification -Metrics $MockMetrics
        $passed = ($result.Classification -eq $ExpectedClassification)
        Record-TestResult -Category "Classification" -TestName $TestName -Passed $passed -Expected $ExpectedClassification -Actual $result.Classification -Details ($result.Reasons -join "; ")
    } catch {
        Record-TestResult -Category "Classification" -TestName $TestName -Passed $false -Expected $ExpectedClassification -Actual "EXCEPTION: $_"
    }
}

# 1. Known VM Signature
$m1 = $idealMetrics.Clone(); $m1.IsKnownVirtualGuest = $true; $m1.Manufacturer = "innotek GmbH"; $m1.Model = "VirtualBox"
Run-ClassificationTest -TestName "CLS-01: Known VM signature detected" -MockMetrics $m1 -ExpectedClassification "GUEST_DETECTED"

# 2. Known VM Signature with simultaneous invalid/corrupt metrics (precedence check)
$m2 = $idealMetrics.Clone(); $m2.IsKnownVirtualGuest = $true; $m2.TotalVisibleMemoryGiB = [double]::NaN; $m2.CPUCores = -1; $m2.VmStorageStatus = "UNKNOWN"
Run-ClassificationTest -TestName "CLS-02: Guest precedence over corrupt/incomplete metrics" -MockMetrics $m2 -ExpectedClassification "GUEST_DETECTED"

# 3. Virtualization Disabled in Firmware
$m3 = $idealMetrics.Clone(); $m3.VirtualizationFirmwareEnabled = "FALSE"
Run-ClassificationTest -TestName "CLS-03: Virtualization disabled in firmware" -MockMetrics $m3 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 4. SLAT Unsupported
$m4 = $idealMetrics.Clone(); $m4.SecondLevelAddressTranslationExtensions = "FALSE"
Run-ClassificationTest -TestName "CLS-04: SLAT unsupported" -MockMetrics $m4 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 5. Non-allowlist Virtualization flag ("YES" / "ENABLED") -> INCOMPLETE
$m5 = $idealMetrics.Clone(); $m5.VirtualizationFirmwareEnabled = "ENABLED"
Run-ClassificationTest -TestName "CLS-05: Non-allowlist flag ('ENABLED') produces INCOMPLETE" -MockMetrics $m5 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 6. Free Physical RAM below practical minimum (< 5.0 GiB)
$m6 = $idealMetrics.Clone(); $m6.FreePhysicalMemoryGiB = 4.2
Run-ClassificationTest -TestName "CLS-06: Free physical RAM < 5.0 GiB" -MockMetrics $m6 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 7. Total RAM below practical minimum (< 12.0 GiB)
$m7 = $idealMetrics.Clone(); $m7.TotalVisibleMemoryGiB = 10.5
Run-ClassificationTest -TestName "CLS-07: Total RAM < 12.0 GiB" -MockMetrics $m7 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 8. Target Storage Free Space below practical minimum (< 70.0 GiB)
$m8 = $idealMetrics.Clone(); $m8.VmStorageFreeSpaceGiB = 55.0
Run-ClassificationTest -TestName "CLS-08: Target volume free storage < 70.0 GiB" -MockMetrics $m8 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 9. CPU capacity below practical minimum (< 2 cores or < 4 logical)
$m9 = $idealMetrics.Clone(); $m9.CPUCores = 1; $m9.CPULogicalProcessors = 2
Run-ClassificationTest -TestName "CLS-09: CPU capacity below 2 cores / 4 logical" -MockMetrics $m9 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 10. Measured zero CPU cores / logical (0 cores / 0 logical)
$m10 = $idealMetrics.Clone(); $m10.CPUCores = 0; $m10.CPULogicalProcessors = 0
Run-ClassificationTest -TestName "CLS-10: CPU capacity measured as 0 cores / 0 logical" -MockMetrics $m10 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 11. Measured zero storage free space (0.0 GiB)
$m11 = $idealMetrics.Clone(); $m11.VmStorageFreeSpaceGiB = 0.0
Run-ClassificationTest -TestName "CLS-11: Target volume free space measured as 0.0 GiB" -MockMetrics $m11 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 12. Measured zero RAM (0.0 GiB)
$m12 = $idealMetrics.Clone(); $m12.TotalVisibleMemoryGiB = 0.0; $m12.FreePhysicalMemoryGiB = 0.0
Run-ClassificationTest -TestName "CLS-12: Total and Free RAM measured as 0.0 GiB" -MockMetrics $m12 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 13. Non-finite RAM (NaN or Infinity)
$m13 = $idealMetrics.Clone(); $m13.TotalVisibleMemoryGiB = [double]::NaN
Run-ClassificationTest -TestName "CLS-13: TotalVisibleMemoryGiB is NaN" -MockMetrics $m13 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 14. Non-finite storage (Infinity)
$m14 = $idealMetrics.Clone(); $m14.VmStorageFreeSpaceGiB = [double]::PositiveInfinity
Run-ClassificationTest -TestName "CLS-14: VmStorageFreeSpaceGiB is PositiveInfinity" -MockMetrics $m14 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 15. Non-integer CPU cores (fractional string)
$m15 = $idealMetrics.Clone(); $m15.CPUCores = "4.5"
Run-ClassificationTest -TestName "CLS-15: CPUCores is non-integer ('4.5')" -MockMetrics $m15 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 16. Negative CPU cores
$m16 = $idealMetrics.Clone(); $m16.CPUCores = -4
Run-ClassificationTest -TestName "CLS-16: CPUCores is negative (-4)" -MockMetrics $m16 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 17. Storage status DIRECTORY_NOT_FOUND
$m17 = $idealMetrics.Clone(); $m17.VmStorageStatus = "DIRECTORY_NOT_FOUND"
Run-ClassificationTest -TestName "CLS-17: Storage status DIRECTORY_NOT_FOUND" -MockMetrics $m17 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 18. System identity UNKNOWN / CIM failure
$m18 = $idealMetrics.Clone(); $m18.ComputerSystemIdentityStatus = "UNKNOWN"; $m18.Manufacturer = "UNKNOWN"; $m18.Model = "UNKNOWN"
Run-ClassificationTest -TestName "CLS-18: System identity UNKNOWN" -MockMetrics $m18 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 19. Full Recommended Baseline Met -> PASS_CANDIDATE
$m19 = $idealMetrics.Clone()
Run-ClassificationTest -TestName "CLS-19: Full recommended baseline met" -MockMetrics $m19 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 20. Constrained Host Resources (meets practical minimums, below recommended)
$m20 = $idealMetrics.Clone(); $m20.TotalVisibleMemoryGiB = 14.0; $m20.FreePhysicalMemoryGiB = 6.5; $m20.VmStorageFreeSpaceGiB = 85.0; $m20.CPUCores = 2; $m20.CPULogicalProcessors = 4
Run-ClassificationTest -TestName "CLS-20: Constrained host resources" -MockMetrics $m20 -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

# 21. Boundary Tests: Total RAM
$m21a = $idealMetrics.Clone(); $m21a.TotalVisibleMemoryGiB = 11.99
Run-ClassificationTest -TestName "CLS-21a: Total RAM 11.99 GiB (just below min)" -MockMetrics $m21a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m21b = $idealMetrics.Clone(); $m21b.TotalVisibleMemoryGiB = 12.0
Run-ClassificationTest -TestName "CLS-21b: Total RAM 12.0 GiB (exactly at min)" -MockMetrics $m21b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m21c = $idealMetrics.Clone(); $m21c.TotalVisibleMemoryGiB = 15.99
Run-ClassificationTest -TestName "CLS-21c: Total RAM 15.99 GiB (just below rec)" -MockMetrics $m21c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m21d = $idealMetrics.Clone(); $m21d.TotalVisibleMemoryGiB = 16.0
Run-ClassificationTest -TestName "CLS-21d: Total RAM 16.0 GiB (exactly at rec)" -MockMetrics $m21d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 22. Boundary Tests: Free RAM
$m22a = $idealMetrics.Clone(); $m22a.FreePhysicalMemoryGiB = 4.99
Run-ClassificationTest -TestName "CLS-22a: Free RAM 4.99 GiB (just below min)" -MockMetrics $m22a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m22b = $idealMetrics.Clone(); $m22b.FreePhysicalMemoryGiB = 5.0
Run-ClassificationTest -TestName "CLS-22b: Free RAM 5.0 GiB (exactly at min)" -MockMetrics $m22b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m22c = $idealMetrics.Clone(); $m22c.FreePhysicalMemoryGiB = 7.99
Run-ClassificationTest -TestName "CLS-22c: Free RAM 7.99 GiB (just below rec)" -MockMetrics $m22c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m22d = $idealMetrics.Clone(); $m22d.FreePhysicalMemoryGiB = 8.0
Run-ClassificationTest -TestName "CLS-22d: Free RAM 8.0 GiB (exactly at rec)" -MockMetrics $m22d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 23. Boundary Tests: Target Storage Free Space
$m23a = $idealMetrics.Clone(); $m23a.VmStorageFreeSpaceGiB = 69.99
Run-ClassificationTest -TestName "CLS-23a: Storage 69.99 GiB (just below min)" -MockMetrics $m23a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m23b = $idealMetrics.Clone(); $m23b.VmStorageFreeSpaceGiB = 70.0
Run-ClassificationTest -TestName "CLS-23b: Storage 70.0 GiB (exactly at min)" -MockMetrics $m23b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m23c = $idealMetrics.Clone(); $m23c.VmStorageFreeSpaceGiB = 99.99
Run-ClassificationTest -TestName "CLS-23c: Storage 99.99 GiB (just below rec)" -MockMetrics $m23c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m23d = $idealMetrics.Clone(); $m23d.VmStorageFreeSpaceGiB = 100.0
Run-ClassificationTest -TestName "CLS-23d: Storage 100.0 GiB (exactly at rec)" -MockMetrics $m23d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 24. VMMonitorModeExtensions: Physical Host without Hypervisor and VMM = FALSE (Blocks)
$m24 = $idealMetrics.Clone(); $m24.HypervisorPresent = $false; $m24.VMMonitorModeExtensions = "FALSE"
Run-ClassificationTest -TestName "CLS-24: Physical host without hypervisor and VMM = FALSE" -MockMetrics $m24 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 25. VMMonitorModeExtensions: Physical Host with HypervisorPresent = True and VMM = FALSE (Diagnostic, Passes)
$m25 = $idealMetrics.Clone(); $m25.HypervisorPresent = $true; $m25.VMMonitorModeExtensions = "FALSE"
Run-ClassificationTest -TestName "CLS-25: Physical host with HypervisorPresent = True and VMM = FALSE" -MockMetrics $m25 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 26. VMMonitorModeExtensions: Physical Host with VMM = UNKNOWN, Firmware Virt & SLAT = TRUE (Passes)
$m26 = $idealMetrics.Clone(); $m26.VMMonitorModeExtensions = "UNKNOWN"
Run-ClassificationTest -TestName "CLS-26: Physical host with VMM = UNKNOWN, Virt & SLAT = TRUE" -MockMetrics $m26 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

$failedList = @($testResults | Where-Object { -not $_.Passed })
$failedCount = $failedList.Count
Write-Host "`nTest Summary: Total: $($testResults.Count) | Passed: $($testResults.Count - $failedCount) | Failed: $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })

if ($failedCount -gt 0) {
    exit 1
}

exit 0

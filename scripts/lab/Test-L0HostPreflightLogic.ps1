<#
.SYNOPSIS
    Automated unit tests for the Get-L0HostClassification logic of Get-L0HostPreflight.ps1.

.DESCRIPTION
    Validates deterministic unit test cases for host classification under mock metrics
    without requiring external dependencies or executing native CIM queries on the host.
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

function Run-ClassificationTest {
    param(
        [string]$TestName,
        [hashtable]$MockMetrics,
        [string]$ExpectedClassification
    )

    try {
        $result = Get-L0HostClassification -Metrics $MockMetrics
        $passed = ($result.Classification -eq $ExpectedClassification)
        $status = if ($passed) { "PASS" } else { "FAIL" }
        $color  = if ($passed) { "Green" } else { "Red" }
        Write-Host "[$status] $TestName (Expected: $ExpectedClassification | Got: $($result.Classification))" -ForegroundColor $color
        $testResults.Add([pscustomobject]@{
            TestName = $TestName
            Expected = $ExpectedClassification
            Actual   = $result.Classification
            Passed   = $passed
            Reasons  = ($result.Reasons -join "; ")
        })
    } catch {
        Write-Host "[FAIL-EXCEPTION] $TestName : $($_.ToString())" -ForegroundColor Red
        $testResults.Add([pscustomobject]@{
            TestName = $TestName
            Expected = $ExpectedClassification
            Actual   = "EXCEPTION"
            Passed   = $false
            Reasons  = $_.ToString()
        })
    }
}

# Baseline "Ideal" Candidate Metrics
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

# 1. Known VM Signature
$m1 = $idealMetrics.Clone()
$m1.IsKnownVirtualGuest = $true
$m1.Manufacturer = "innotek GmbH"
$m1.Model = "VirtualBox"
Run-ClassificationTest -TestName "01: Known VM signature detected" -MockMetrics $m1 -ExpectedClassification "GUEST_DETECTED"

# 2. Virtualization Disabled in Firmware
$m2 = $idealMetrics.Clone()
$m2.VirtualizationFirmwareEnabled = "FALSE"
Run-ClassificationTest -TestName "02: Virtualization disabled in firmware" -MockMetrics $m2 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 3. SLAT Unsupported
$m3 = $idealMetrics.Clone()
$m3.SecondLevelAddressTranslationExtensions = "FALSE"
Run-ClassificationTest -TestName "03: SLAT unsupported" -MockMetrics $m3 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 4. Free Physical RAM below practical minimum (< 5.0 GiB)
$m4 = $idealMetrics.Clone()
$m4.FreePhysicalMemoryGiB = 4.2
Run-ClassificationTest -TestName "04: Free physical RAM < 5.0 GiB" -MockMetrics $m4 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 5. Target Storage Free Space below practical minimum (< 70.0 GiB)
$m5 = $idealMetrics.Clone()
$m5.VmStorageFreeSpaceGiB = 55.0
Run-ClassificationTest -TestName "05: Target volume free storage < 70.0 GiB" -MockMetrics $m5 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 6. Total RAM below practical minimum (< 12.0 GiB)
$m6 = $idealMetrics.Clone()
$m6.TotalVisibleMemoryGiB = 10.5
Run-ClassificationTest -TestName "06: Total RAM < 12.0 GiB" -MockMetrics $m6 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 7. CPU capacity below practical minimum (< 2 cores or < 4 logical)
$m7 = $idealMetrics.Clone()
$m7.CPUCores = 1
$m7.CPULogicalProcessors = 2
Run-ClassificationTest -TestName "07: CPU capacity below 2 cores / 4 logical" -MockMetrics $m7 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 8. VmStoragePath absent / unverified
$m8 = $idealMetrics.Clone()
$m8.VmStorageStatus = "UNKNOWN"
$m8.VmStorageVolume = ""
Run-ClassificationTest -TestName "08: VmStoragePath absent / unverified" -MockMetrics $m8 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 9. Virtualization or SLAT UNKNOWN
$m9 = $idealMetrics.Clone()
$m9.VirtualizationFirmwareEnabled = "UNKNOWN"
Run-ClassificationTest -TestName "09: VirtualizationFirmwareEnabled UNKNOWN" -MockMetrics $m9 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 10. ComputerSystemIdentityStatus UNKNOWN
$m10 = $idealMetrics.Clone()
$m10.ComputerSystemIdentityStatus = "UNKNOWN"
$m10.Manufacturer = "UNKNOWN"
$m10.Model = "UNKNOWN"
Run-ClassificationTest -TestName "10: System identity UNKNOWN / CIM failure" -MockMetrics $m10 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 11. CPU metrics UNKNOWN
$m11 = $idealMetrics.Clone()
$m11.CPUCores = "UNKNOWN"
$m11.CPULogicalProcessors = "UNKNOWN"
Run-ClassificationTest -TestName "11: CPU metrics UNKNOWN" -MockMetrics $m11 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# 12. Full Recommended Baseline Met -> PASS_CANDIDATE
$m12 = $idealMetrics.Clone()
Run-ClassificationTest -TestName "12: Full recommended baseline met" -MockMetrics $m12 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 13. Physical Host with HypervisorPresent = True
$m13 = $idealMetrics.Clone()
$m13.HypervisorPresent = $true
Run-ClassificationTest -TestName "13: Physical host with HypervisorPresent = True" -MockMetrics $m13 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 14. Constrained Host Resources (meets practical minimums, below recommended)
$m14 = $idealMetrics.Clone()
$m14.TotalVisibleMemoryGiB = 14.0
$m14.FreePhysicalMemoryGiB = 6.5
$m14.VmStorageFreeSpaceGiB = 85.0
$m14.CPUCores = 2
$m14.CPULogicalProcessors = 4
Run-ClassificationTest -TestName "14: Constrained host resources" -MockMetrics $m14 -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

# 15. Boundary Tests: Total RAM
$m15a = $idealMetrics.Clone(); $m15a.TotalVisibleMemoryGiB = 11.99
Run-ClassificationTest -TestName "15a: Total RAM 11.99 GiB (just below min)" -MockMetrics $m15a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m15b = $idealMetrics.Clone(); $m15b.TotalVisibleMemoryGiB = 12.0
Run-ClassificationTest -TestName "15b: Total RAM 12.0 GiB (exactly at min)" -MockMetrics $m15b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m15c = $idealMetrics.Clone(); $m15c.TotalVisibleMemoryGiB = 15.99
Run-ClassificationTest -TestName "15c: Total RAM 15.99 GiB (just below rec)" -MockMetrics $m15c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m15d = $idealMetrics.Clone(); $m15d.TotalVisibleMemoryGiB = 16.0
Run-ClassificationTest -TestName "15d: Total RAM 16.0 GiB (exactly at rec)" -MockMetrics $m15d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 16. Boundary Tests: Free RAM
$m16a = $idealMetrics.Clone(); $m16a.FreePhysicalMemoryGiB = 4.99
Run-ClassificationTest -TestName "16a: Free RAM 4.99 GiB (just below min)" -MockMetrics $m16a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m16b = $idealMetrics.Clone(); $m16b.FreePhysicalMemoryGiB = 5.0
Run-ClassificationTest -TestName "16b: Free RAM 5.0 GiB (exactly at min)" -MockMetrics $m16b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m16c = $idealMetrics.Clone(); $m16c.FreePhysicalMemoryGiB = 7.99
Run-ClassificationTest -TestName "16c: Free RAM 7.99 GiB (just below rec)" -MockMetrics $m16c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m16d = $idealMetrics.Clone(); $m16d.FreePhysicalMemoryGiB = 8.0
Run-ClassificationTest -TestName "16d: Free RAM 8.0 GiB (exactly at rec)" -MockMetrics $m16d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 17. Boundary Tests: Target Storage Free Space
$m17a = $idealMetrics.Clone(); $m17a.VmStorageFreeSpaceGiB = 69.99
Run-ClassificationTest -TestName "17a: Storage 69.99 GiB (just below min)" -MockMetrics $m17a -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

$m17b = $idealMetrics.Clone(); $m17b.VmStorageFreeSpaceGiB = 70.0
Run-ClassificationTest -TestName "17b: Storage 70.0 GiB (exactly at min)" -MockMetrics $m17b -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m17c = $idealMetrics.Clone(); $m17c.VmStorageFreeSpaceGiB = 99.99
Run-ClassificationTest -TestName "17c: Storage 99.99 GiB (just below rec)" -MockMetrics $m17c -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$m17d = $idealMetrics.Clone(); $m17d.VmStorageFreeSpaceGiB = 100.0
Run-ClassificationTest -TestName "17d: Storage 100.0 GiB (exactly at rec)" -MockMetrics $m17d -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# 18. Measured Storage Free Space Exactly 0.0 GiB (measured, not missing volume)
$m18 = $idealMetrics.Clone()
$m18.VmStorageFreeSpaceGiB = 0.0
Run-ClassificationTest -TestName "18: Target volume free space measured as 0.0 GiB" -MockMetrics $m18 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# 19. Precedence: Known VM Signature with simultaneous UNKNOWN fields
$m19 = $idealMetrics.Clone()
$m19.IsKnownVirtualGuest = $true
$m19.Manufacturer = "VirtualBox"
$m19.Model = "VirtualBox"
$m19.VirtualizationFirmwareEnabled = "UNKNOWN"
$m19.SecondLevelAddressTranslationExtensions = "UNKNOWN"
$m19.VmStorageStatus = "UNKNOWN"
Run-ClassificationTest -TestName "19: Precedence: Guest detected with UNKNOWN diagnostics" -MockMetrics $m19 -ExpectedClassification "GUEST_DETECTED"

$failedList = @($testResults | Where-Object { -not $_.Passed })
$failedCount = $failedList.Count
Write-Host "`nTest Summary: Total: $($testResults.Count) | Passed: $($testResults.Count - $failedCount) | Failed: $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })

if ($failedCount -gt 0) {
    exit 1
}

exit 0

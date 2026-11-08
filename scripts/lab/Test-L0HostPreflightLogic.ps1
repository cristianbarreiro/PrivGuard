<#
.SYNOPSIS
    Automated unit tests for the Get-L0HostClassification logic of Get-L0HostPreflight.ps1.

.DESCRIPTION
    Validates all 9 deterministic test cases for host classification under mock metrics
    without requiring external dependencies (Pester independent test harness).
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Extract the Get-L0HostClassification function definition from the script
$scriptPath = Join-Path $PSScriptRoot "Get-L0HostPreflight.ps1"
if (-not (Test-Path $scriptPath)) {
    throw "Script not found at $scriptPath"
}

$scriptContent = Get-Content -Path $scriptPath -Raw
$functionRegex = [regex]'(?ms)function\s+Get-L0HostClassification\s*\{.*?\n\}'
$match = $functionRegex.Match($scriptContent)
if (-not $match.Success) {
    throw "Could not extract Get-L0HostClassification function from $scriptPath"
}

Invoke-Expression $match.Value

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
        $color = if ($passed) { "Green" } else { "Red" }
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
    Manufacturer                           = "Dell Inc."
    Model                                  = "Precision 7920"
    VirtualizationFirmwareEnabled          = "TRUE"
    VMMonitorModeExtensions                = "TRUE"
    SecondLevelAddressTranslationExtensions= "TRUE"
    TotalVisibleMemoryGiB                  = 32.0
    FreePhysicalMemoryGiB                  = 18.0
    VmStorageStatus                        = "RESOLVED"
    VmStorageVolume                        = "D:"
    VmStorageFreeSpaceGiB                  = 250.0
}

# Test 1: Guest detected by manufacturer/model
$m1 = $idealMetrics.Clone()
$m1.IsKnownVirtualGuest = $true
$m1.Manufacturer = "innotek GmbH"
$m1.Model = "VirtualBox"
Run-ClassificationTest -TestName "Test 1: Guest detected by signature" -MockMetrics $m1 -ExpectedClassification "GUEST_DETECTED"

# Test 2: Firmware virtualization is FALSE
$m2 = $idealMetrics.Clone()
$m2.VirtualizationFirmwareEnabled = "FALSE"
Run-ClassificationTest -TestName "Test 2: Virtualization disabled in firmware" -MockMetrics $m2 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# Test 3: SLAT is FALSE
$m3 = $idealMetrics.Clone()
$m3.SecondLevelAddressTranslationExtensions = "FALSE"
Run-ClassificationTest -TestName "Test 3: SLAT unsupported" -MockMetrics $m3 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# Test 4: Total RAM sufficient but free RAM < 4.0 GiB
$m4 = $idealMetrics.Clone()
$m4.FreePhysicalMemoryGiB = 2.5
Run-ClassificationTest -TestName "Test 4: Free physical RAM < 4.0 GiB" -MockMetrics $m4 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# Test 5: Target storage volume free space < 50.0 GiB
$m5 = $idealMetrics.Clone()
$m5.VmStorageFreeSpaceGiB = 35.0
Run-ClassificationTest -TestName "Test 5: Target volume free storage < 50.0 GiB" -MockMetrics $m5 -ExpectedClassification "L0_PREFLIGHT_BLOCKED"

# Test 6: VmStoragePath absent / UNKNOWN
$m6 = $idealMetrics.Clone()
$m6.VmStorageStatus = "UNKNOWN"
$m6.VmStorageVolume = ""
Run-ClassificationTest -TestName "Test 6: VmStoragePath absent / unverified" -MockMetrics $m6 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# Test 7: CIM processor virtualization data UNKNOWN
$m7 = $idealMetrics.Clone()
$m7.VirtualizationFirmwareEnabled = "UNKNOWN"
Run-ClassificationTest -TestName "Test 7: VirtualizationFirmwareEnabled UNKNOWN" -MockMetrics $m7 -ExpectedClassification "PREFLIGHT_INCOMPLETE"

# Test 8: All criteria meet recommended baseline -> PASS_CANDIDATE (never L0_READY)
$m8 = $idealMetrics.Clone()
Run-ClassificationTest -TestName "Test 8: Full recommended baseline met" -MockMetrics $m8 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# Test 9: HypervisorPresent = True on physical hardware (e.g. Windows Hypervisor Platform / Hyper-V active on Dell physical host)
$m9 = $idealMetrics.Clone()
$m9.HypervisorPresent = $true
Run-ClassificationTest -TestName "Test 9: Physical host with HypervisorPresent = True" -MockMetrics $m9 -ExpectedClassification "L0_PREFLIGHT_PASS_CANDIDATE"

# Additional Test 10: Constrained hardware (Total RAM 14 GiB, Free RAM 5.5 GiB, Free Disk 70 GiB)
$m10 = $idealMetrics.Clone()
$m10.TotalVisibleMemoryGiB = 14.0
$m10.FreePhysicalMemoryGiB = 5.5
$m10.VmStorageFreeSpaceGiB = 70.0
Run-ClassificationTest -TestName "Test 10: Constrained host resources" -MockMetrics $m10 -ExpectedClassification "L0_CAPABLE_CONSTRAINED"

$failedList = @($testResults | Where-Object { -not $_.Passed })
$failedCount = $failedList.Count
Write-Host "`nTest Summary: Total: $($testResults.Count) | Passed: $($testResults.Count - $failedCount) | Failed: $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })

if ($failedCount -gt 0) {
    throw "$failedCount classification tests failed."
}

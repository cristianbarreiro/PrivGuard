<#
.SYNOPSIS
    Verifies that a generated MSIX package meets Microsoft Store Partner Center identity and D-01 capability rules.

.DESCRIPTION
    Inspects the AppxManifest.xml extracted directly from the generated .msix package (ZIP container).
    Uses XML namespaces to validate:
      - Identity/Name: cdevstudios.PrivGuard
      - Identity/Publisher: CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD
      - Properties/PublisherDisplayName: cdev studios
      - rescap:Capability Name="runFullTrust" is present
      - rescap:Capability Name="allowElevation" is absent (D-01)
      - rescap:Capability Name="unvirtualizedResources" is absent (D-01)
      - Package is not using Package.DevLab.appxmanifest
    Fails with exit code 1 if zero or multiple .msix packages are found in -PackageDir, or if any rule is violated.

.PARAMETER PackagePath
    Direct path to a specific .msix file.

.PARAMETER PackageDir
    Directory containing a single .msix file. Defaults to 'publish_out/msix'.

.EXAMPLE
    .\scripts\verify-store-msix.ps1 -PackagePath "publish_out\msix\PrivGuard-2.0.0.0-x64.msix"

.EXAMPLE
    .\scripts\verify-store-msix.ps1 -PackageDir "publish_out\msix"
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]$PackagePath,

    [Parameter(Mandatory = $false)]
    [string]$PackageDir
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Xml.Linq

try {
    if ([string]::IsNullOrWhiteSpace($PackagePath)) {
        if ([string]::IsNullOrWhiteSpace($PackageDir)) {
            $PackageDir = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\publish_out\msix")
        }

        if (-not (Test-Path -LiteralPath $PackageDir)) {
            throw "Package directory does not exist: '$PackageDir'"
        }

        $candidates = @(Get-ChildItem -LiteralPath $PackageDir -Filter "*.msix" -File)
        if ($candidates.Count -eq 0) {
            throw "Expected exactly 1 .msix package in '$PackageDir', but found 0."
        }
        if ($candidates.Count -gt 1) {
            $names = ($candidates | ForEach-Object { $_.Name }) -join ", "
            throw "Expected exactly 1 .msix package in '$PackageDir', but found $($candidates.Count): [$names]."
        }

        $PackagePath = $candidates[0].FullName
    }

    if (-not (Test-Path -LiteralPath $PackagePath)) {
        throw "MSIX package not found at: '$PackagePath'"
    }

    if ([System.IO.Path]::GetExtension($PackagePath) -ne ".msix") {
        throw "Expected .msix package file extension, got: '$PackagePath'"
    }

    Write-Host "`n==========================================================" -ForegroundColor Cyan
    Write-Host " Verifying Store MSIX Package: $([System.IO.Path]::GetFileName($PackagePath))" -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan

    $zip = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    $manifestXml = $null
    try {
        $manifestEntry = $zip.Entries | Where-Object { $_.FullName -eq "AppxManifest.xml" }
        if (-not $manifestEntry) {
            throw "AppxManifest.xml was not found inside MSIX package '$PackagePath'."
        }

        $stream = $manifestEntry.Open()
        $reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
        $manifestXml = $reader.ReadToEnd()
        $reader.Close()
        $stream.Close()
    }
    finally {
        $zip.Dispose()
    }

    if ([string]::IsNullOrWhiteSpace($manifestXml)) {
        throw "AppxManifest.xml content is empty."
    }

    [System.Xml.Linq.XDocument]$doc = [System.Xml.Linq.XDocument]::Parse($manifestXml)
    $ns = [System.Xml.Linq.XNamespace]::Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10")
    $rescap = [System.Xml.Linq.XNamespace]::Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities")

    if ($doc.Root.Name -ne ($ns + "Package")) {
        throw "Root element must be <Package> in namespace http://schemas.microsoft.com/appx/manifest/foundation/windows10"
    }

    $identity = $doc.Root.Element($ns + "Identity")
    if (-not $identity) {
        throw "<Identity> element is missing from AppxManifest.xml."
    }

    $attrName = $identity.Attribute("Name"); $name = if ($attrName) { $attrName.Value } else { $null }
    $attrPub = $identity.Attribute("Publisher"); $publisher = if ($attrPub) { $attrPub.Value } else { $null }
    $attrVer = $identity.Attribute("Version"); $version = if ($attrVer) { $attrVer.Value } else { $null }
    $attrArch = $identity.Attribute("ProcessorArchitecture"); $arch = if ($attrArch) { $attrArch.Value } else { $null }

    $properties = $doc.Root.Element($ns + "Properties")
    if (-not $properties) {
        throw "<Properties> element is missing from AppxManifest.xml."
    }
    $elemPub = $properties.Element($ns + "PublisherDisplayName"); $pubDisplay = if ($elemPub) { $elemPub.Value } else { $null }

    Write-Host "   Identity/Name:                   $name" -ForegroundColor White
    Write-Host "   Identity/Publisher:              $publisher" -ForegroundColor White
    Write-Host "   Properties/PublisherDisplayName: $pubDisplay" -ForegroundColor White
    Write-Host "   Identity/Version:                $version" -ForegroundColor DarkGray
    Write-Host "   Identity/ProcessorArchitecture:  $arch" -ForegroundColor DarkGray

    # Check DevLab exclusion
    if ($name -eq "cdevstudios.PrivGuard.DevLab" -or $publisher -eq "CN=PrivGuardDevLab" -or ($pubDisplay -and $pubDisplay.Contains("DevLab"))) {
        throw "Package appears to be a DevLab non-Store package ($name / $publisher), not an authentic Store package."
    }

    # Verify official Store identity
    $ExpectedName = "cdevstudios.PrivGuard"
    $ExpectedPublisher = "CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD"
    $ExpectedPubDisplay = "cdev studios"

    if ($name -ne $ExpectedName) {
        throw "Store identity mismatch: expected Name='$ExpectedName', got '$name'."
    }
    if ($publisher -ne $ExpectedPublisher) {
        throw "Store identity mismatch: expected Publisher='$ExpectedPublisher', got '$publisher'."
    }
    if ($pubDisplay -ne $ExpectedPubDisplay) {
        throw "Store identity mismatch: expected PublisherDisplayName='$ExpectedPubDisplay', got '$pubDisplay'."
    }

    # Verify Capabilities
    $capabilities = $doc.Root.Element($ns + "Capabilities")
    if (-not $capabilities) {
        throw "<Capabilities> element is missing from AppxManifest.xml."
    }

    # Check forbidden restricted capabilities anywhere in the document
    $forbiddenElevation = @($doc.Descendants() | Where-Object { $_.Name.Namespace -eq $rescap -and ($a = $_.Attribute("Name")) -and $a.Value -eq "allowElevation" })
    if ($forbiddenElevation.Count -gt 0) {
        throw "Forbidden restricted capability 'allowElevation' is present in AppxManifest.xml (violates D-01)."
    }

    $forbiddenUnvirtualized = @($doc.Descendants() | Where-Object { $_.Name.Namespace -eq $rescap -and ($a = $_.Attribute("Name")) -and $a.Value -eq "unvirtualizedResources" })
    if ($forbiddenUnvirtualized.Count -gt 0) {
        throw "Forbidden restricted capability 'unvirtualizedResources' is present in AppxManifest.xml (violates D-01)."
    }

    # Check restricted capabilities under <Capabilities>
    $rescapElements = @($capabilities.Elements() | Where-Object { $_.Name.Namespace -eq $rescap })
    $hasRunFullTrust = $rescapElements | Where-Object { $_.Name.LocalName -eq "Capability" -and ($a = $_.Attribute("Name")) -and $a.Value -eq "runFullTrust" }
    if (-not $hasRunFullTrust) {
        throw "Required restricted capability 'runFullTrust' is missing from AppxManifest.xml."
    }

    if ($rescapElements.Count -ne 1) {
        $declared = ($rescapElements | ForEach-Object { $a = $_.Attribute("Name"); if ($a) { $a.Value } else { $_.Name.LocalName } }) -join ", "
        throw "Expected exactly 1 restricted capability (runFullTrust), found $($rescapElements.Count): [$declared]."
    }

    Write-Host "   Capabilities (D-01):             runFullTrust (sole restricted capability verified)" -ForegroundColor Green
    Write-Host "   Forbidden Capabilities:          0 (allowElevation: absent, unvirtualizedResources: absent)" -ForegroundColor Green
    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host " Store MSIX Verification: PASS" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "`n==========================================================" -ForegroundColor Red
    Write-Host " Store MSIX Verification: FAILED" -ForegroundColor Red
    Write-Host " Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "==========================================================" -ForegroundColor Red
    exit 1
}

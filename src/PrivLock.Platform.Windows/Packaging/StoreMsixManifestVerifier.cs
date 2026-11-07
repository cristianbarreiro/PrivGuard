using System.IO.Compression;
using System.Xml.Linq;

namespace PrivLock.Platform.Windows.Packaging;

/// <summary>
/// Result of an MSIX AppxManifest.xml verification against Microsoft Store rules (D-01 / D-02).
/// </summary>
public sealed record StoreManifestVerificationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string? PackageName { get; init; }
    public string? Publisher { get; init; }
    public string? PublisherDisplayName { get; init; }
    public IReadOnlyList<string> RestrictedCapabilities { get; init; } = [];

    public static StoreManifestVerificationResult Success(
        string packageName,
        string publisher,
        string publisherDisplayName,
        IReadOnlyList<string> restrictedCapabilities) =>
        new()
        {
            IsValid = true,
            PackageName = packageName,
            Publisher = publisher,
            PublisherDisplayName = publisherDisplayName,
            RestrictedCapabilities = restrictedCapabilities
        };

    public static StoreManifestVerificationResult Fail(string errorMessage) =>
        new()
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
}

/// <summary>
/// Verifies that an MSIX package or extracted AppxManifest.xml meets official Microsoft Store
/// Partner Center identity and restricted-capability boundaries (D-01: sole capability runFullTrust,
/// zero allowElevation, zero unvirtualizedResources).
/// </summary>
public static class StoreMsixManifestVerifier
{
    public const string ExpectedPackageName = "cdevstudios.PrivGuard";
    public const string ExpectedPublisher = "CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD";
    public const string ExpectedPublisherDisplayName = "cdev studios";
    public const string DevLabPackageName = "cdevstudios.PrivGuard.DevLab";
    public const string DevLabPublisher = "CN=PrivGuardDevLab";

    public static readonly XNamespace FoundationNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
    public static readonly XNamespace RestrictedCapabilitiesNs = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";

    /// <summary>
    /// Verifies raw AppxManifest.xml content using XML namespaces.
    /// </summary>
    public static StoreManifestVerificationResult VerifyManifest(string manifestXml)
    {
        if (string.IsNullOrWhiteSpace(manifestXml))
            return StoreManifestVerificationResult.Fail("AppxManifest.xml content is empty.");

        XDocument doc;
        try
        {
            doc = XDocument.Parse(manifestXml);
        }
        catch (Exception ex)
        {
            return StoreManifestVerificationResult.Fail($"Invalid XML in AppxManifest.xml: {ex.Message}");
        }

        var root = doc.Root;
        if (root == null || root.Name != FoundationNs + "Package")
            return StoreManifestVerificationResult.Fail("Root element must be <Package> in namespace http://schemas.microsoft.com/appx/manifest/foundation/windows10.");

        var identity = root.Element(FoundationNs + "Identity");
        if (identity == null)
            return StoreManifestVerificationResult.Fail("<Identity> element is missing from AppxManifest.xml.");

        var actualName = identity.Attribute("Name")?.Value;
        var actualPublisher = identity.Attribute("Publisher")?.Value;

        var properties = root.Element(FoundationNs + "Properties");
        if (properties == null)
            return StoreManifestVerificationResult.Fail("<Properties> element is missing from AppxManifest.xml.");

        var actualPublisherDisplayName = properties.Element(FoundationNs + "PublisherDisplayName")?.Value;

        // Check for accidental DevLab manifest
        if (string.Equals(actualName, DevLabPackageName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(actualPublisher, DevLabPublisher, StringComparison.OrdinalIgnoreCase) ||
            actualPublisherDisplayName?.Contains("DevLab", StringComparison.OrdinalIgnoreCase) == true)
        {
            return StoreManifestVerificationResult.Fail(
                $"Package manifest appears to be DevLab manifest (Name='{actualName}', Publisher='{actualPublisher}'), not authentic Store manifest.");
        }

        if (actualName is null || !string.Equals(actualName, ExpectedPackageName, StringComparison.Ordinal))
            return StoreManifestVerificationResult.Fail($"Identity/Name mismatch: expected '{ExpectedPackageName}', got '{actualName}'.");

        if (actualPublisher is null || !string.Equals(actualPublisher, ExpectedPublisher, StringComparison.Ordinal))
            return StoreManifestVerificationResult.Fail($"Identity/Publisher mismatch: expected '{ExpectedPublisher}', got '{actualPublisher}'.");

        if (actualPublisherDisplayName is null || !string.Equals(actualPublisherDisplayName, ExpectedPublisherDisplayName, StringComparison.Ordinal))
            return StoreManifestVerificationResult.Fail($"Properties/PublisherDisplayName mismatch: expected '{ExpectedPublisherDisplayName}', got '{actualPublisherDisplayName}'.");

        // Validate Capabilities node
        var capabilities = root.Element(FoundationNs + "Capabilities");
        if (capabilities == null)
            return StoreManifestVerificationResult.Fail("<Capabilities> element is missing from AppxManifest.xml.");

        // Check for forbidden restricted capabilities anywhere in the document
        var hasAllowElevation = doc.Descendants()
            .Any(e => e.Name.Namespace == RestrictedCapabilitiesNs &&
                      string.Equals(e.Attribute("Name")?.Value, "allowElevation", StringComparison.OrdinalIgnoreCase));
        if (hasAllowElevation)
            return StoreManifestVerificationResult.Fail("Forbidden restricted capability 'allowElevation' is present in AppxManifest.xml (violates D-01).");

        var hasUnvirtualizedResources = doc.Descendants()
            .Any(e => e.Name.Namespace == RestrictedCapabilitiesNs &&
                      string.Equals(e.Attribute("Name")?.Value, "unvirtualizedResources", StringComparison.OrdinalIgnoreCase));
        if (hasUnvirtualizedResources)
            return StoreManifestVerificationResult.Fail("Forbidden restricted capability 'unvirtualizedResources' is present in AppxManifest.xml (violates D-01).");

        // Inspect restricted capabilities under <Capabilities>
        var rescapElements = capabilities.Elements()
            .Where(e => e.Name.Namespace == RestrictedCapabilitiesNs)
            .ToList();

        var hasRunFullTrust = rescapElements.Any(e =>
            string.Equals(e.Name.LocalName, "Capability", StringComparison.Ordinal) &&
            string.Equals(e.Attribute("Name")?.Value, "runFullTrust", StringComparison.Ordinal));

        if (!hasRunFullTrust)
            return StoreManifestVerificationResult.Fail("Required restricted capability 'runFullTrust' is missing from AppxManifest.xml.");

        if (rescapElements.Count != 1)
        {
            var declared = string.Join(", ", rescapElements.Select(e => e.Attribute("Name")?.Value ?? e.Name.LocalName));
            return StoreManifestVerificationResult.Fail($"Expected exactly 1 restricted capability (runFullTrust), found {rescapElements.Count}: [{declared}].");
        }

        return StoreManifestVerificationResult.Success(
            actualName,
            actualPublisher,
            actualPublisherDisplayName ?? string.Empty,
            ["runFullTrust"]);
    }

    /// <summary>
    /// Verifies the AppxManifest.xml inside an open stream of an MSIX zip container.
    /// </summary>
    public static StoreManifestVerificationResult VerifyPackageStream(Stream packageStream)
    {
        try
        {
            using var archive = new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: true);
            var manifestEntry = archive.GetEntry("AppxManifest.xml");
            if (manifestEntry == null)
                return StoreManifestVerificationResult.Fail("AppxManifest.xml was not found inside the package archive.");

            using var entryStream = manifestEntry.Open();
            using var reader = new StreamReader(entryStream);
            var xml = reader.ReadToEnd();
            return VerifyManifest(xml);
        }
        catch (Exception ex)
        {
            return StoreManifestVerificationResult.Fail($"Failed to read package archive: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies the AppxManifest.xml inside a specified .msix file.
    /// </summary>
    public static StoreManifestVerificationResult VerifyPackageFile(string packagePath)
    {
        if (!File.Exists(packagePath))
            return StoreManifestVerificationResult.Fail($"MSIX package file was not found: {packagePath}");

        if (!string.Equals(Path.GetExtension(packagePath), ".msix", StringComparison.OrdinalIgnoreCase))
            return StoreManifestVerificationResult.Fail($"Expected .msix package file extension, got: {packagePath}");

        try
        {
            using var stream = File.OpenRead(packagePath);
            return VerifyPackageStream(stream);
        }
        catch (Exception ex)
        {
            return StoreManifestVerificationResult.Fail($"Could not open package file: {ex.Message}");
        }
    }

    /// <summary>
    /// Locates the single .msix package inside the specified directory and verifies its AppxManifest.xml.
    /// Fails if zero or multiple .msix files are found.
    /// </summary>
    public static StoreManifestVerificationResult VerifyPackageDirectory(string packageDirectory)
    {
        if (!Directory.Exists(packageDirectory))
            return StoreManifestVerificationResult.Fail($"Package directory does not exist: {packageDirectory}");

        var candidateFiles = Directory.GetFiles(packageDirectory, "*.msix", SearchOption.TopDirectoryOnly);
        if (candidateFiles.Length == 0)
            return StoreManifestVerificationResult.Fail($"No .msix packages found in directory: {packageDirectory}");

        if (candidateFiles.Length > 1)
        {
            var names = string.Join(", ", candidateFiles.Select(Path.GetFileName));
            return StoreManifestVerificationResult.Fail($"Expected exactly 1 .msix package in directory, found {candidateFiles.Length}: [{names}].");
        }

        return VerifyPackageFile(candidateFiles[0]);
    }
}

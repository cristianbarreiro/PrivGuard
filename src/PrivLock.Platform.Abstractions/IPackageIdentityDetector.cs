namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Provides OS package identity queries to detect if the process runs within an MSIX/AppX container
/// and verify the authentic PrivGuard package identity.
/// </summary>
public interface IPackageIdentityDetector
{
    /// <summary>
    /// Gets the verified package identity classification.
    /// </summary>
    PackageIdentityKind Kind { get; }

    /// <summary>
    /// Gets the full package name if executing with package identity, or null if unpackaged or failed.
    /// </summary>
    string? PackageFullName { get; }

    /// <summary>
    /// Gets the package family name if executing with package identity, or null if unpackaged or failed.
    /// </summary>
    string? PackageFamilyName { get; }

    /// <summary>
    /// Determines whether the process is executing inside the authentic PrivGuard Store package.
    /// </summary>
    bool HasExpectedPackageIdentity();
}

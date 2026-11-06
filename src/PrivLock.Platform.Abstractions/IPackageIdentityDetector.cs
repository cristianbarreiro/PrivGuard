namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Provides OS package identity queries to detect if the process runs within an MSIX/AppX container
/// and verify the authentic PrivGuard package identity.
/// </summary>
public interface IPackageIdentityDetector
{
    /// <summary>
    /// Gets whether the current process is executing with Windows package identity (MSIX/AppX).
    /// </summary>
    bool IsRunningAsPackaged { get; }

    /// <summary>
    /// Gets the full package name if executing with package identity, or null if unpackaged.
    /// </summary>
    string? PackageFullName { get; }

    /// <summary>
    /// Determines whether the process is executing inside the authentic PrivGuard Store package.
    /// </summary>
    bool HasExpectedPackageIdentity();

    /// <summary>
    /// Determines whether the package full name matches a specific expected prefix or name.
    /// </summary>
    bool MatchesPackageName(string expectedPrefixOrName);
}

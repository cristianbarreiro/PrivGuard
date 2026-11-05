namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Identifies the outcome of operating system package identity detection.
/// </summary>
public enum PackageIdentityKind
{
    /// <summary>
    /// Process is running unpackaged (standard Win32 executable).
    /// </summary>
    NotPackaged = 0,

    /// <summary>
    /// Process is running inside the authentic PrivGuard Microsoft Store package.
    /// </summary>
    AuthenticStorePackage = 1,

    /// <summary>
    /// Process is running inside a package container, but the identity does not match authentic PrivGuard.
    /// </summary>
    UnexpectedPackage = 2,

    /// <summary>
    /// Detection failed due to an OS API error, exception, or unsupported environment.
    /// </summary>
    DetectionFailed = 3
}

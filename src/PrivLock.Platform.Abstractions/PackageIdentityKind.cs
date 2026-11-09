namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Identifies the outcome of operating system package identity detection.
/// Default value (0) is explicitly DetectionFailed to ensure fail-closed security.
/// </summary>
public enum PackageIdentityKind
{
    /// <summary>
    /// Default fail-closed state: detection has not executed, failed, or was uninitialized.
    /// </summary>
    DetectionFailed = 0,

    /// <summary>
    /// Process is verified as running unpackaged (standard Win32 executable).
    /// Requires explicit detection.
    /// </summary>
    NotPackaged = 1,

    /// <summary>
    /// Process is verified as running inside the authentic PrivGuard Microsoft Store package.
    /// Requires explicit detection.
    /// </summary>
    AuthenticStorePackage = 2,

    /// <summary>
    /// Process is running inside a package container, but the identity does not match authentic PrivGuard.
    /// Requires explicit detection.
    /// </summary>
    UnexpectedPackage = 3,

    /// <summary>
    /// Process is running inside a development or unsigned package container.
    /// </summary>
    UnsignedDevPackage = 4,

    /// <summary>
    /// Process is running inside a package container whose publisher cannot be verified as authentic Store identity.
    /// </summary>
    UnverifiedPublisherPackage = 5
}

using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Packaging;

/// <summary>
/// Non-packaged detector for non-Windows platforms or fallback scenarios.
/// Strictly untrusted: always reports DetectionFailed and cannot declare authentic package identity.
/// </summary>
public sealed class NoOpPackageIdentityDetector : IPackageIdentityDetector
{
    public static NoOpPackageIdentityDetector Instance { get; } = new();

    private NoOpPackageIdentityDetector()
    {
    }

    public PackageIdentityKind Kind => PackageIdentityKind.DetectionFailed;

    public string? PackageFullName => null;

    public string? PackageFamilyName => null;

    public bool HasExpectedPackageIdentity() => false;
}

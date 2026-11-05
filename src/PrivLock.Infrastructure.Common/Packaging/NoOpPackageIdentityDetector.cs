using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Packaging;

/// <summary>
/// Non-packaged detector for non-Windows platforms or fallback scenarios.
/// Fails closed by reporting DetectionFailed by default so that privileges are never granted silently.
/// </summary>
public sealed class NoOpPackageIdentityDetector : IPackageIdentityDetector
{
    public static NoOpPackageIdentityDetector Instance { get; } = new();

    public PackageIdentityKind Kind { get; }

    public NoOpPackageIdentityDetector(PackageIdentityKind kind = PackageIdentityKind.DetectionFailed)
    {
        Kind = kind;
    }

    public string? PackageFullName => null;

    public string? PackageFamilyName => null;

    public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
}

using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Packaging;

/// <summary>
/// Non-packaged detector for non-Windows platforms or fallback scenarios.
/// </summary>
public sealed class NoOpPackageIdentityDetector : IPackageIdentityDetector
{
    public static NoOpPackageIdentityDetector Instance { get; } = new();

    public bool IsRunningAsPackaged => false;

    public string? PackageFullName => null;

    public bool HasExpectedPackageIdentity() => false;

    public bool MatchesPackageName(string expectedPrefixOrName) => false;
}

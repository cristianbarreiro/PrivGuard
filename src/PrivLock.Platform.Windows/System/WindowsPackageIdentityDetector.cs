using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

/// <summary>
/// Windows-specific package identity detector implementing IPackageIdentityDetector.
/// Uses native kernel32 GetCurrentPackageFullName via PackageIdentityHelper and validates against authentic PrivGuard package constants.
/// </summary>
public sealed class WindowsPackageIdentityDetector : IPackageIdentityDetector
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsPackageIdentityDetector>();

    public const string ExpectedPackageName = "cdevstudios.PrivGuard";
    public const string ExpectedPackageFamilyName = "cdevstudios.PrivGuard_tv7b4yqg10sw2";
    public const string ExpectedPackageFullNamePrefix = "cdevstudios.PrivGuard_";
    public const string ExpectedPublisherId = "tv7b4yqg10sw2";

    private readonly Func<(bool IsPackaged, string? FullName)> _nativeQueryProvider;

    public WindowsPackageIdentityDetector()
        : this(() => (PackageIdentityHelper.IsRunningAsPackaged, PackageIdentityHelper.PackageFullName))
    {
    }

    internal WindowsPackageIdentityDetector(Func<(bool IsPackaged, string? FullName)> nativeQueryProvider)
    {
        _nativeQueryProvider = nativeQueryProvider ?? throw new ArgumentNullException(nameof(nativeQueryProvider));
    }

    public bool IsRunningAsPackaged
    {
        get
        {
            try
            {
                return _nativeQueryProvider().IsPackaged;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to query native package identity state");
                return false;
            }
        }
    }

    public string? PackageFullName
    {
        get
        {
            try
            {
                return _nativeQueryProvider().FullName;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to query native package full name");
                return null;
            }
        }
    }

    public bool HasExpectedPackageIdentity()
    {
        var fullName = PackageFullName;
        if (string.IsNullOrWhiteSpace(fullName))
            return false;

        // Authentic PrivGuard Store package begins with "cdevstudios.PrivGuard_" and ends with publisher suffix "tv7b4yqg10sw2"
        return fullName.StartsWith(ExpectedPackageFullNamePrefix, StringComparison.OrdinalIgnoreCase) &&
               fullName.EndsWith(ExpectedPublisherId, StringComparison.OrdinalIgnoreCase);
    }

    public bool MatchesPackageName(string expectedPrefixOrName)
    {
        if (string.IsNullOrWhiteSpace(expectedPrefixOrName))
            return false;

        var fullName = PackageFullName;
        if (string.IsNullOrWhiteSpace(fullName))
            return false;

        return fullName.StartsWith(expectedPrefixOrName, StringComparison.OrdinalIgnoreCase);
    }
}

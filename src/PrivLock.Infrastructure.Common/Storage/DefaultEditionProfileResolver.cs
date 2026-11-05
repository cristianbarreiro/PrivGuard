using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Storage;

/// <summary>
/// Resolves the edition profile and computes isolated directory paths and mutex names.
/// Pure, cross-platform implementation that receives IPackageIdentityDetector via DI.
/// Fails closed for untrusted or indeterminate package identities.
/// </summary>
public sealed class DefaultEditionProfileResolver : IEditionProfileResolver
{
    public const string StoreSubdirectory = "Store";
    public const string StandaloneSubdirectory = "Standalone";
    public const string UntrustedSubdirectory = "UntrustedOrIndeterminate";

    public const string StoreMutexName = @"Global\PrivGuard_Store_SingleInstance";
    public const string StandaloneMutexName = @"Global\PrivGuard_Standalone_SingleInstance";

    private readonly IPackageIdentityDetector _identityDetector;
    private readonly string _baseAppDataDirectory;

    public DefaultEditionProfileResolver(
        IPackageIdentityDetector identityDetector,
        string? customBaseAppDataDirectory = null)
    {
        _identityDetector = identityDetector ?? throw new ArgumentNullException(nameof(identityDetector));
        _baseAppDataDirectory = customBaseAppDataDirectory ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivGuard");
    }

    public EditionProfile Profile => _identityDetector.Kind switch
    {
        PackageIdentityKind.AuthenticStorePackage => EditionProfile.Store,
        PackageIdentityKind.NotPackaged => EditionProfile.Standalone,
        PackageIdentityKind.UnexpectedPackage => EditionProfile.UntrustedOrIndeterminate,
        PackageIdentityKind.DetectionFailed => EditionProfile.UntrustedOrIndeterminate,
        _ => EditionProfile.UntrustedOrIndeterminate
    };

    public string ProfileName => Profile switch
    {
        EditionProfile.Store => StoreSubdirectory,
        EditionProfile.Standalone => StandaloneSubdirectory,
        _ => UntrustedSubdirectory
    };

    public string DataDirectory
    {
        get
        {
            EnsureTrustedProfile();
            return Path.Combine(_baseAppDataDirectory, ProfileName);
        }
    }

    public string RecoveryDirectory
    {
        get
        {
            EnsureTrustedProfile();
            return Path.Combine(DataDirectory, "Recovery");
        }
    }

    public string LogsDirectory
    {
        get
        {
            EnsureTrustedProfile();
            return Path.Combine(DataDirectory, "Logs");
        }
    }

    public string CrashReportsDirectory
    {
        get
        {
            EnsureTrustedProfile();
            return Path.Combine(DataDirectory, "CrashReports");
        }
    }

    public string SingleInstanceMutexName
    {
        get
        {
            EnsureTrustedProfile();
            return Profile == EditionProfile.Store ? StoreMutexName : StandaloneMutexName;
        }
    }

    public bool SupportsPrivilegedOperations =>
        _identityDetector.Kind == PackageIdentityKind.NotPackaged && Profile == EditionProfile.Standalone;

    private void EnsureTrustedProfile()
    {
        if (Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            throw new InvalidOperationException(
                $"Access to edition storage paths and mutex is rejected for untrusted or indeterminate profile (PackageIdentityKind: {_identityDetector.Kind}).");
        }
    }
}

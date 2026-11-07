using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Infrastructure.Common.Storage;

/// <summary>
/// Resolves the edition profile and computes isolated directory paths and mutex names.
/// Pure, cross-platform implementation that receives IPackageIdentityDetector via DI.
/// Fails closed for untrusted or indeterminate package identities and captures an immutable resolution snapshot.
/// </summary>
public sealed class DefaultEditionProfileResolver : IEditionProfileResolver
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DefaultEditionProfileResolver>();

    public const string StoreSubdirectory = "Store";
    public const string StandaloneSubdirectory = "Standalone";
    public const string UntrustedSubdirectory = "UntrustedOrIndeterminate";

    public const string StoreMutexName = @"Global\PrivGuard_Store_SingleInstance";
    public const string StandaloneMutexName = @"Global\PrivGuard_Standalone_SingleInstance";

    private readonly PackageIdentityKind _resolvedIdentityKind;
    private readonly EditionProfile _resolvedProfile;
    private readonly string _profileName;
    private readonly string _baseAppDataDirectory;

    public DefaultEditionProfileResolver(
        IPackageIdentityDetector identityDetector,
        string? customBaseAppDataDirectory = null)
    {
        if (identityDetector == null)
            throw new ArgumentNullException(nameof(identityDetector));

        try
        {
            _resolvedIdentityKind = identityDetector.Kind;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Package identity detector threw during profile resolution; failing closed to DetectionFailed");
            _resolvedIdentityKind = PackageIdentityKind.DetectionFailed;
        }

        _resolvedProfile = _resolvedIdentityKind switch
        {
            PackageIdentityKind.AuthenticStorePackage => EditionProfile.Store,
            PackageIdentityKind.NotPackaged => EditionProfile.Standalone,
            PackageIdentityKind.UnexpectedPackage => EditionProfile.UntrustedOrIndeterminate,
            PackageIdentityKind.DetectionFailed => EditionProfile.UntrustedOrIndeterminate,
            _ => EditionProfile.UntrustedOrIndeterminate
        };

        _profileName = _resolvedProfile switch
        {
            EditionProfile.Store => StoreSubdirectory,
            EditionProfile.Standalone => StandaloneSubdirectory,
            _ => UntrustedSubdirectory
        };

        _baseAppDataDirectory = customBaseAppDataDirectory ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivGuard");
    }

    public EditionProfile Profile => _resolvedProfile;

    public string ProfileName => _profileName;

    public string DataDirectory
    {
        get
        {
            EnsureTrustedProfile();
            return Path.Combine(_baseAppDataDirectory, _profileName);
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
            return _resolvedProfile == EditionProfile.Store ? StoreMutexName : StandaloneMutexName;
        }
    }

    public bool SupportsPrivilegedOperations =>
        _resolvedIdentityKind == PackageIdentityKind.NotPackaged && _resolvedProfile == EditionProfile.Standalone;

    private void EnsureTrustedProfile()
    {
        if (_resolvedProfile == EditionProfile.UntrustedOrIndeterminate)
        {
            throw new InvalidOperationException(
                $"Access to edition storage paths and mutex is rejected for untrusted or indeterminate profile (PackageIdentityKind: {_resolvedIdentityKind}).");
        }
    }
}

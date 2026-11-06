using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Storage;

/// <summary>
/// Resolves the edition profile and computes isolated directory paths and mutex names.
/// Pure, cross-platform implementation that receives IPackageIdentityDetector via DI.
/// </summary>
public sealed class DefaultEditionProfileResolver : IEditionProfileResolver
{
    public const string StoreSubdirectory = "Store";
    public const string StandaloneSubdirectory = "Standalone";
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

    public EditionProfile Profile =>
        _identityDetector.IsRunningAsPackaged
            ? EditionProfile.Store
            : EditionProfile.Standalone;

    public string ProfileName => Profile == EditionProfile.Store ? StoreSubdirectory : StandaloneSubdirectory;

    public string DataDirectory => Path.Combine(_baseAppDataDirectory, ProfileName);

    public string RecoveryDirectory => Path.Combine(DataDirectory, "Recovery");

    public string LogsDirectory => Path.Combine(DataDirectory, "Logs");

    public string CrashReportsDirectory => Path.Combine(DataDirectory, "CrashReports");

    public string SingleInstanceMutexName => Profile == EditionProfile.Store ? StoreMutexName : StandaloneMutexName;

    public bool SupportsPrivilegedOperations => Profile == EditionProfile.Standalone;
}

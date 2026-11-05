namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Provides deterministic resolution of the active edition profile and its canonical, isolated storage directories.
/// </summary>
public interface IEditionProfileResolver
{
    /// <summary>
    /// Gets the active edition profile.
    /// </summary>
    EditionProfile Profile { get; }

    /// <summary>
    /// Gets the canonical name of the profile ("Store" or "Standalone").
    /// </summary>
    string ProfileName { get; }

    /// <summary>
    /// Gets the base data directory for the active edition profile.
    /// </summary>
    string DataDirectory { get; }

    /// <summary>
    /// Gets the recovery and WAL directory for the active edition profile.
    /// </summary>
    string RecoveryDirectory { get; }

    /// <summary>
    /// Gets the rolling logs directory for the active edition profile.
    /// </summary>
    string LogsDirectory { get; }

    /// <summary>
    /// Gets the crash reports directory for the active edition profile.
    /// </summary>
    string CrashReportsDirectory { get; }

    /// <summary>
    /// Gets the system-wide single instance mutex name for this profile.
    /// </summary>
    string SingleInstanceMutexName { get; }

    /// <summary>
    /// Gets whether this profile supports privileged operations (on-demand worker for PnP/HKLM).
    /// </summary>
    bool SupportsPrivilegedOperations { get; }
}

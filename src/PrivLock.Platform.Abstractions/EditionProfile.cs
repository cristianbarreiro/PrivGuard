namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Identifies the packaging and distribution edition profile of PrivGuard.
/// Default value (0) is explicitly UntrustedOrIndeterminate to ensure fail-closed security.
/// </summary>
public enum EditionProfile
{
    /// <summary>
    /// Default fail-closed state: untrusted, indeterminate, or uninitialized edition profile.
    /// Fails closed: refuses privileged operations, storage paths, and single-instance coordination.
    /// </summary>
    UntrustedOrIndeterminate = 0,

    /// <summary>
    /// Standalone edition (Inno Setup / Win32 standard).
    /// Executes asInvoker with on-demand elevation and full support for Standard and Secure protection.
    /// Requires explicit resolution.
    /// </summary>
    Standalone = 1,

    /// <summary>
    /// Microsoft Store edition (MSIX runFullTrust).
    /// Executes asInvoker without elevation; limited strictly to Standard protection (WASAPI).
    /// Requires explicit resolution.
    /// </summary>
    Store = 2
}

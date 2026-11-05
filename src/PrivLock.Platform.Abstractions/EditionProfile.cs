namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Identifies the packaging and distribution edition profile of PrivGuard.
/// </summary>
public enum EditionProfile
{
    /// <summary>
    /// Standalone edition (Inno Setup / Win32 standard).
    /// Executes asInvoker with on-demand elevation and full support for Standard and Secure protection.
    /// </summary>
    Standalone = 0,

    /// <summary>
    /// Microsoft Store edition (MSIX runFullTrust).
    /// Executes asInvoker without elevation; limited strictly to Standard protection (WASAPI).
    /// </summary>
    Store = 1
}

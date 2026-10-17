namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Listens for a same-user request to restore privacy state and close before Windows uninstall.
/// </summary>
public interface IUninstallRequestListener : IDisposable
{
    event EventHandler? UninstallRequested;

    void Start();
}

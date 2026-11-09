namespace PrivLock.Platform.Abstractions;

public interface IApplicationActivationListener : IDisposable
{
    event EventHandler? ActivationRequested;

    bool TryRequestExistingInstanceActivation(out string? error);

    void Start();
}

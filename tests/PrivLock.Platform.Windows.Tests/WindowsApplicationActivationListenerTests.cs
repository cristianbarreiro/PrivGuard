using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsApplicationActivationListenerTests
{
    [Fact]
    public void ActivationRequestReceivedBeforeUiStartupIsDeliveredWhenTheWindowSubscribes()
    {
        var eventName = $@"Local\PrivGvard_ActivateExistingInstance_{Guid.NewGuid():N}";
        using var activationRequested = new ManualResetEventSlim();
        using var listener = new WindowsApplicationActivationListener(eventName);
        using var launchingInstance = new WindowsApplicationActivationListener(eventName);
        listener.Start();

        var signaled = launchingInstance.TryRequestExistingInstanceActivation(out var error);

        Assert.True(signaled, error);
        listener.ActivationRequested += (_, _) => activationRequested.Set();
        Assert.True(activationRequested.Wait(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void MissingRunningInstanceActivationEndpointIsNotReportedAsAnError()
    {
        using var listener = new WindowsApplicationActivationListener(
            $@"Local\PrivGvard_ActivateExistingInstance_{Guid.NewGuid():N}");
        var signaled = listener.TryRequestExistingInstanceActivation(out var error);

        Assert.False(signaled);
        Assert.Null(error);
    }
}

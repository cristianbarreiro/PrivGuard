using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsUninstallRequestListenerTests
{
    [Fact]
    public void RunningInstanceReceivesSafeShutdownRequestAndReleasesSingleInstanceAdmission()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var eventName = $@"Local\PrivLock_UninstallRequested_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        using var shutdownRequested = new ManualResetEventSlim();
        using var listener = new WindowsUninstallRequestListener(eventName);
        listener.UninstallRequested += (_, _) => shutdownRequested.Set();
        listener.Start();

        var guard = new RequestAwareSingleInstanceGuard(shutdownRequested);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(5),
            out var error,
            eventName,
            mutexName);

        Assert.True(acquired, error);
        Assert.True(shutdownRequested.IsSet);
        Assert.True(guard.Acquired);
    }

    [Fact]
    public void BusyUninstallGateWithoutRunningInstanceFailsExplicitly()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var guard = new NeverAcquireSingleInstanceGuard();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            out var error,
            $@"Local\PrivLock_UninstallRequested_{suffix}",
            $@"Local\PrivLock_SingleInstance_{suffix}");

        Assert.False(acquired);
        Assert.Contains("no running PrivGvard instance", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RunningInstanceThatDoesNotReleaseAdmissionTimesOutExplicitly()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var eventName = $@"Local\PrivLock_UninstallRequested_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        using var listener = new WindowsUninstallRequestListener(eventName);
        listener.UninstallRequested += (_, _) => { };
        listener.Start();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            new NeverAcquireSingleInstanceGuard(),
            TimeSpan.FromMilliseconds(250),
            out var error,
            eventName,
            mutexName);

        Assert.False(acquired);
        Assert.Contains("did not finish its safe shutdown", error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RequestAwareSingleInstanceGuard : ISingleInstanceGuard
    {
        private readonly ManualResetEventSlim _shutdownRequested;

        public RequestAwareSingleInstanceGuard(ManualResetEventSlim shutdownRequested)
        {
            _shutdownRequested = shutdownRequested;
        }

        public bool Acquired { get; private set; }

        public bool TryAcquireSingleInstance()
        {
            Acquired = _shutdownRequested.IsSet;
            return Acquired;
        }

        public void Release()
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class NeverAcquireSingleInstanceGuard : ISingleInstanceGuard
    {
        public bool TryAcquireSingleInstance() => false;

        public void Release()
        {
        }

        public void Dispose()
        {
        }
    }
}

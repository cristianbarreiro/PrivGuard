using System.Diagnostics;
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

    /// <summary>
    /// Validates that when no instance receives a shutdown request and the guard never obtains admission,
    /// the handoff fails closed at timeout and reports that the uninstall gate is owned.
    /// Note: This is an intra-process unit state-machine test using a fake guard; multi-process OS contention is validated in VM acceptance.
    /// </summary>
    [Fact]
    public void BusyUninstallGateWithoutRunningInstanceFailsExplicitly()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var guard = new NeverAcquireSingleInstanceGuard();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromMilliseconds(200),
            out var error,
            $@"Local\PrivLock_UninstallRequested_{suffix}",
            $@"Local\PrivLock_SingleInstance_{suffix}",
            pollInterval: TimeSpan.FromMilliseconds(25));

        Assert.False(acquired);
        Assert.Contains("no running PrivGvard instance", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("uninstall gate is owned", error, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // Simulates FUS Release-Order Race: attempt 1 returns UninstallGateBusy, attempt 2 returns Acquired.
        var guard = new ContentionThenSuccessGuard(failuresBeforeSuccess: 2, failureResult: WindowsSingleInstanceAcquisitionResult.UninstallGateBusy);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            out var error,
            missingEventName,
            mutexName,
            pollInterval: TimeSpan.FromMilliseconds(20));

        Assert.True(acquired, error);
        Assert.Null(error);
        Assert.True(guard.Acquired);
        Assert.True(guard.BothLocksHeldAtSuccess);
        Assert.Equal(3, guard.Attempts);
    }

    [Fact]
    public void NoEventPermanentContention_FailsClosedAtTimeoutWithoutBusySpin()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.UninstallGateBusy);
        var stopwatch = Stopwatch.StartNew();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromMilliseconds(200),
            out var error,
            missingEventName,
            mutexName,
            pollInterval: TimeSpan.FromMilliseconds(25));

        stopwatch.Stop();

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("uninstall gate is owned", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no running PrivGvard instance accepted a shutdown request", error, StringComparison.OrdinalIgnoreCase);

        // Bounded attempts: with 200ms timeout and 25ms interval, attempts are bounded to prove no busy-spin (not thousands).
        Assert.InRange(guard.Attempts, 1, 20);
        Assert.True(stopwatch.ElapsedMilliseconds >= 180, $"Expected >= 180ms elapsed, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.AccessOrCreationError);
        var stopwatch = Stopwatch.StartNew();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(5),
            out var error,
            missingEventName,
            mutexName,
            pollInterval: TimeSpan.FromMilliseconds(50));

        stopwatch.Stop();

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("Failed to create or acquire single-instance mutex on Windows", error, StringComparison.OrdinalIgnoreCase);

        // Prompt termination: failed immediately on attempt 1 without waiting for 5 seconds.
        Assert.Equal(1, guard.Attempts);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Expected prompt exit under 1000ms, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // Simulates 3rd-process contention: attempt 1 returns SingleInstanceBusy, attempt 2 returns Acquired.
        var guard = new ContentionThenSuccessGuard(failuresBeforeSuccess: 1, failureResult: WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            out var error,
            missingEventName,
            mutexName,
            pollInterval: TimeSpan.FromMilliseconds(20));

        Assert.True(acquired, error);
        Assert.Null(error);
        Assert.True(guard.Acquired);
        Assert.True(guard.BothLocksHeldAtSuccess);
        Assert.Equal(2, guard.Attempts);
    }

    [Fact]
    public void AbandonedMutex_OnProbeHandle_PreservesOwnershipAndSucceedsIfGuardAcquires()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // Abandon the probe mutex in a separate thread
        using var threadStarted = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            var abandonedMutex = new Mutex(initiallyOwned: false, mutexName);
            abandonedMutex.WaitOne();
            threadStarted.Set();
            // Exit thread without calling ReleaseMutex() -> generates AbandonedMutexException in waiting threads.
        });
        thread.Start();
        threadStarted.Wait();
        thread.Join();

        var guard = new ContentionThenSuccessGuard(failuresBeforeSuccess: 0, failureResult: WindowsSingleInstanceAcquisitionResult.Acquired);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            out var error,
            missingEventName,
            mutexName,
            pollInterval: TimeSpan.FromMilliseconds(20));

        Assert.True(acquired, error);
        Assert.Null(error);
        Assert.True(guard.Acquired);
    }

    private sealed class ContentionThenSuccessGuard : ISingleInstanceGuard, IWindowsDetailedSingleInstanceGuard
    {
        private readonly int _failuresBeforeSuccess;
        private readonly WindowsSingleInstanceAcquisitionResult _failureResult;

        public ContentionThenSuccessGuard(int failuresBeforeSuccess, WindowsSingleInstanceAcquisitionResult failureResult)
        {
            _failuresBeforeSuccess = failuresBeforeSuccess;
            _failureResult = failureResult;
        }

        public int Attempts { get; private set; }
        public bool Acquired { get; private set; }
        public bool BothLocksHeldAtSuccess { get; private set; }

        public bool TryAcquireSingleInstance()
        {
            return TryAcquireSingleInstanceDetail() == WindowsSingleInstanceAcquisitionResult.Acquired;
        }

        public WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)
        {
            Attempts++;
            if (Attempts <= _failuresBeforeSuccess)
            {
                return _failureResult;
            }

            Acquired = true;
            BothLocksHeldAtSuccess = true;
            return WindowsSingleInstanceAcquisitionResult.Acquired;
        }

        public void Release()
        {
            Acquired = false;
            BothLocksHeldAtSuccess = false;
        }

        public void Dispose() => Release();
    }

    private sealed class ContentionCountingGuard : ISingleInstanceGuard, IWindowsDetailedSingleInstanceGuard
    {
        private readonly WindowsSingleInstanceAcquisitionResult _result;

        public ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult result)
        {
            _result = result;
        }

        public int Attempts { get; private set; }

        public bool TryAcquireSingleInstance() =>
            TryAcquireSingleInstanceDetail() == WindowsSingleInstanceAcquisitionResult.Acquired;

        public WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)
        {
            Attempts++;
            return _result;
        }

        public void Release() { }
        public void Dispose() { }
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

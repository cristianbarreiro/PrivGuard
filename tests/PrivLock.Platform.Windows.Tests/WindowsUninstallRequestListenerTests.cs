using System.Diagnostics;
using System.Reflection;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

/// <summary>
/// Unit and intra-session synchronization tests for <see cref="WindowsUninstallRequestListener"/>.
/// Note: Tests prefixed with <c>SimulatedGuard_</c> or <c>FakeGuard_</c> exercise the handoff state machine
/// using deterministic test doubles. Tests prefixed with <c>RealNamedMutexes_</c> use real Windows <c>Local\</c>
/// named mutexes across dedicated OS threads within the current logon session; they do not simulate
/// multi-session Fast User Switching (FUS), distinct user SIDs, or <c>Global\</c> DACL boundaries.
/// </summary>
public sealed class WindowsUninstallRequestListenerTests
{
    [Fact]
    public void PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature()
    {
        var publicMethods = typeof(WindowsUninstallRequestListener)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire))
            .ToArray();

        var method = Assert.Single(publicMethods);
        Assert.Equal(typeof(bool), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(5, parameters.Length);
        Assert.Equal(typeof(ISingleInstanceGuard), parameters[0].ParameterType);
        Assert.Equal(typeof(TimeSpan), parameters[1].ParameterType);
        Assert.True(parameters[2].IsOut);
        Assert.Equal(typeof(string).MakeByRefType(), parameters[2].ParameterType);
        Assert.Equal(typeof(string), parameters[3].ParameterType);
        Assert.True(parameters[3].HasDefaultValue);
        Assert.Equal(typeof(string), parameters[4].ParameterType);
        Assert.True(parameters[4].HasDefaultValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)] // Timeout.InfiniteTimeSpan (-1 ms)
    [InlineData(-250)]
    public void PollIntervalValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex(int pollIntervalMs)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.Acquired);
        var invalidPollInterval = pollIntervalMs == -1
            ? Timeout.InfiniteTimeSpan
            : TimeSpan.FromMilliseconds(pollIntervalMs);

        var stopwatch = Stopwatch.StartNew();
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
                guard,
                TimeSpan.FromSeconds(5),
                invalidPollInterval,
                out _,
                missingEventName,
                mutexName));
        stopwatch.Stop();

        Assert.Equal("pollInterval", ex.ParamName);
        Assert.Equal(0, guard.Attempts);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Expected immediate validation failure, took {stopwatch.ElapsedMilliseconds}ms");
        Assert.False(Mutex.TryOpenExisting(mutexName, out var existingMutex));
        existingMutex?.Dispose();
    }

    [Fact]
    public void PollIntervalValidation_SubMillisecondOrExceedingWaitHandleLimit_ThrowsArgumentOutOfRangeImmediately()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.Acquired);

        foreach (var invalidInterval in new[] { TimeSpan.FromTicks(100), TimeSpan.MaxValue })
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
                    guard,
                    TimeSpan.FromSeconds(5),
                    invalidInterval,
                    out _,
                    $@"Local\PrivLock_UninstallRequested_Missing_{suffix}",
                    mutexName));

            Assert.Equal("pollInterval", ex.ParamName);
            Assert.Equal(0, guard.Attempts);
            Assert.False(Mutex.TryOpenExisting(mutexName, out var existingMutex));
            existingMutex?.Dispose();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)] // Timeout.InfiniteTimeSpan (-1 ms)
    [InlineData(-500)]
    public void TimeoutValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex(int timeoutMs)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.Acquired);
        var invalidTimeout = timeoutMs == -1
            ? Timeout.InfiniteTimeSpan
            : TimeSpan.FromMilliseconds(timeoutMs);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
                guard,
                invalidTimeout,
                TimeSpan.FromMilliseconds(25),
                out _,
                $@"Local\PrivLock_UninstallRequested_Missing_{suffix}",
                mutexName));

        Assert.Equal("timeout", ex.ParamName);
        Assert.Equal(0, guard.Attempts);
        Assert.False(Mutex.TryOpenExisting(mutexName, out var existingMutex));
        existingMutex?.Dispose();
    }

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
    /// Validates that when a non-detailed <see cref="ISingleInstanceGuard"/> test double always returns <c>false</c>
    /// and no local shutdown event exists, the handoff fails closed at timeout and attributes the failure
    /// to <c>SingleInstanceBusy</c> rather than falsely claiming the uninstall gate is owned.
    /// </summary>
    [Fact]
    public void FakeGuardNeverAcquires_WithoutRunningInstance_TimesOutWithSingleInstanceBusyDiagnostic()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var guard = new NeverAcquireSingleInstanceGuard();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(25),
            out var error,
            $@"Local\PrivLock_UninstallRequested_{suffix}",
            $@"Local\PrivLock_SingleInstance_{suffix}");

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("single-instance mutex remained busy", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("last observed contention: SingleInstanceBusy", error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("uninstall gate", error, StringComparison.OrdinalIgnoreCase);
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
    public void SimulatedGuard_NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // State-machine simulation with fake guard: attempts 1-2 return UninstallGateBusy, attempt 3 returns Acquired.
        var guard = new ContentionThenSuccessGuard(failuresBeforeSuccess: 2, failureResult: WindowsSingleInstanceAcquisitionResult.UninstallGateBusy);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(20),
            out var error,
            missingEventName,
            mutexName);

        Assert.True(acquired, error);
        Assert.Null(error);
        Assert.True(guard.Acquired);
        Assert.True(guard.SimulatedBothLocksHeldAtSuccess);
        Assert.Equal(3, guard.Attempts);
    }

    [Fact]
    public void SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.UninstallGateBusy);
        var stopwatch = Stopwatch.StartNew();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(25),
            out var error,
            missingEventName,
            mutexName);

        stopwatch.Stop();

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("Windows uninstall gate remained busy", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("last observed contention: UninstallGateBusy", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no local shutdown request could be delivered", error, StringComparison.OrdinalIgnoreCase);

        // Bounded attempts: with 200ms timeout and 25ms interval, attempts are bounded to prove no busy-spin (not thousands).
        Assert.InRange(guard.Attempts, 1, 20);
        Assert.True(stopwatch.ElapsedMilliseconds >= 180, $"Expected >= 180ms elapsed, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void SimulatedGuard_NoEventPermanentSingleInstanceContention_FailsClosedWithSingleInstanceDiagnostic()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(25),
            out var error,
            missingEventName,
            mutexName);

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("Windows single-instance mutex remained busy", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("last observed contention: SingleInstanceBusy", error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("uninstall gate", error, StringComparison.OrdinalIgnoreCase);
        Assert.InRange(guard.Attempts, 1, 20);
    }

    [Fact]
    public void SimulatedGuard_MixedContentionCauses_TimeoutDiagnosticReflectsLastObservedContention()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // First attempt observes UninstallGateBusy; subsequent attempts observe SingleInstanceBusy.
        var guard1 = new SequenceContentionGuard(
            WindowsSingleInstanceAcquisitionResult.UninstallGateBusy,
            WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

        var acquired1 = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard1,
            TimeSpan.FromMilliseconds(150),
            TimeSpan.FromMilliseconds(20),
            out var error1,
            missingEventName,
            mutexName);

        Assert.False(acquired1);
        Assert.NotNull(error1);
        Assert.True(guard1.Attempts >= 2, $"Expected at least 2 attempts, got {guard1.Attempts}");
        Assert.Contains("last observed contention: SingleInstanceBusy", error1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mutex state is not guaranteed after timeout", error1, StringComparison.OrdinalIgnoreCase);

        // Reverse order: first attempt observes SingleInstanceBusy; subsequent attempts observe UninstallGateBusy.
        var guard2 = new SequenceContentionGuard(
            WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy,
            WindowsSingleInstanceAcquisitionResult.UninstallGateBusy);

        var acquired2 = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard2,
            TimeSpan.FromMilliseconds(150),
            TimeSpan.FromMilliseconds(20),
            out var error2,
            missingEventName,
            mutexName);

        Assert.False(acquired2);
        Assert.NotNull(error2);
        Assert.True(guard2.Attempts >= 2, $"Expected at least 2 attempts, got {guard2.Attempts}");
        Assert.Contains("last observed contention: UninstallGateBusy", error2, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mutex state is not guaranteed after timeout", error2, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RealNamedMutexes_WhenProbeMutexNeverAcquiredAndNoEvent_TimesOutWithNeutralDiagnostic()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        using var holderAcquired = new ManualResetEventSlim();
        using var releaseHolder = new ManualResetEventSlim();
        Exception? holderError = null;

        var holderThread = new Thread(() =>
        {
            try
            {
                using var heldMutex = new Mutex(initiallyOwned: false, mutexName);
                Assert.True(heldMutex.WaitOne(TimeSpan.FromSeconds(5)));
                try
                {
                    holderAcquired.Set();
                    Assert.True(releaseHolder.Wait(TimeSpan.FromSeconds(5)));
                }
                finally
                {
                    heldMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                holderError = ex;
                holderAcquired.Set();
            }
        });

        holderThread.Start();
        try
        {
            Assert.True(holderAcquired.Wait(TimeSpan.FromSeconds(5)));
            Assert.Null(holderError);

            var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.Acquired);

            var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
                guard,
                TimeSpan.FromMilliseconds(150),
                TimeSpan.FromMilliseconds(25),
                out var error,
                missingEventName,
                mutexName);

            Assert.False(acquired);
            Assert.Equal(0, guard.Attempts);
            Assert.Equal(
                "Single-instance admission was not obtained before the uninstall handoff expired, and the local shutdown request signal could not be delivered.",
                error);
        }
        finally
        {
            releaseHolder.Set();
            holderThread.Join(TimeSpan.FromSeconds(5));
        }

        Assert.Null(holderError);
    }

    [Fact]
    public void SimulatedGuard_PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        var guard = new ContentionCountingGuard(WindowsSingleInstanceAcquisitionResult.AccessOrCreationError);
        var stopwatch = Stopwatch.StartNew();

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(50),
            out var error,
            missingEventName,
            mutexName);

        stopwatch.Stop();

        Assert.False(acquired);
        Assert.NotNull(error);
        Assert.Contains("Failed to create or acquire single-instance mutex on Windows", error, StringComparison.OrdinalIgnoreCase);

        // Prompt termination: failed immediately on attempt 1 without waiting for 5 seconds.
        Assert.Equal(1, guard.Attempts);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Expected prompt exit under 1000ms, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void SimulatedGuard_ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";

        // Simulates 3rd-process contention with fake guard: attempt 1 returns SingleInstanceBusy, attempt 2 returns Acquired.
        var guard = new ContentionThenSuccessGuard(failuresBeforeSuccess: 1, failureResult: WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guard,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(20),
            out var error,
            missingEventName,
            mutexName);

        Assert.True(acquired, error);
        Assert.Null(error);
        Assert.True(guard.Acquired);
        Assert.True(guard.SimulatedBothLocksHeldAtSuccess);
        Assert.Equal(2, guard.Attempts);
    }

    [Fact]
    public void AbandonedMutex_OnProbeHandle_ReleasesProbeOwnershipBeforeGuardAndLeavesNoResidualLockAfterRelease()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var mutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        var gateName = $@"Local\PrivLock_UninstallGate_{suffix}";

        // Abandon the probe mutex in a separate thread
        using var threadStarted = new ManualResetEventSlim();
        var abandoningThread = new Thread(() =>
        {
            var abandonedMutex = new Mutex(initiallyOwned: false, mutexName);
            abandonedMutex.WaitOne();
            threadStarted.Set();
            // Exit thread without calling ReleaseMutex() -> generates AbandonedMutexException in waiting threads.
        });
        abandoningThread.Start();
        Assert.True(threadStarted.Wait(TimeSpan.FromSeconds(5)));
        Assert.True(abandoningThread.Join(TimeSpan.FromSeconds(5)));

        using var realGuard = new WindowsSingleInstanceGuard(mutexName, gateName);
        var probeReleasedBeforeGuardCheck = false;
        var guardWrapper = new ProbeReleaseVerifyingRealGuard(
            realGuard,
            onBeforeAcquire: () =>
            {
                // Verify on a distinct OS thread that the probe mutex handle was genuinely released
                // after AbandonedMutexException was caught, BEFORE the guard acquires it.
                probeReleasedBeforeGuardCheck = CanAcquireAndReleaseOnSeparateThread(mutexName);
            });

        var acquired = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
            guardWrapper,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(20),
            out var error,
            missingEventName,
            mutexName);

        try
        {
            Assert.True(acquired, error);
            Assert.Null(error);
            Assert.True(probeReleasedBeforeGuardCheck, "Probe mutex was not released before invoking guard acquisition.");

            // While realGuard holds admission, neither mutex is available to another thread.
            Assert.False(CanAcquireAndReleaseOnSeparateThread(gateName));
            Assert.False(CanAcquireAndReleaseOnSeparateThread(mutexName));
        }
        finally
        {
            realGuard.Release();
        }

        // After realGuard.Release(), both mutexes must be completely free (no leftover recursion count from probe).
        Assert.True(CanAcquireAndReleaseOnSeparateThread(gateName));
        Assert.True(CanAcquireAndReleaseOnSeparateThread(mutexName));
    }

    /// <summary>
    /// Intra-session integration test with real Windows <c>Local\</c> named mutexes and the real
    /// <see cref="WindowsSingleInstanceGuard"/> implementation.
    /// Reproduces the exact release-order interleaving (<c>SingleInstance</c> released before <c>UninstallGate</c>)
    /// across two coordinated OS threads and verifies that Thread B observes <c>UninstallGateBusy</c>,
    /// retries within the bounded timeout, and acquires both real mutexes once Thread A releases the gate.
    /// Note: This test executes within a single Windows logon session and process; multi-session FUS,
    /// distinct user SIDs, and <c>Global\</c> DACLs remain subject to TEST-09.7 in an isolated VM.
    /// </summary>
    [Fact]
    public void RealNamedMutexes_ReleaseOrderInterleaving_RetriesWhileUninstallGateHeldAndAcquiresBothWhenGateReleased()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var missingEventName = $@"Local\PrivLock_UninstallRequested_Missing_{suffix}";
        var instanceMutexName = $@"Local\PrivLock_SingleInstance_{suffix}";
        var gateMutexName = $@"Local\PrivLock_UninstallGate_{suffix}";

        using var threadAInReleaseOrderWindow = new ManualResetEventSlim();
        using var threadBObservedGateBusy = new ManualResetEventSlim();
        using var threadBCompletedHandoff = new ManualResetEventSlim();
        using var allowThreadBToRelease = new ManualResetEventSlim();

        Exception? threadAError = null;
        Exception? threadBError = null;
        var handoffResult = false;
        string? handoffError = null;
        var gateHeldByThreadBAtSuccess = false;
        var instanceHeldByThreadBAtSuccess = false;
        IReadOnlyList<WindowsSingleInstanceAcquisitionResult> observedOutcomes = Array.Empty<WindowsSingleInstanceAcquisitionResult>();

        var threadA = new Thread(() =>
        {
            try
            {
                using var gateMutex = new Mutex(initiallyOwned: false, gateMutexName);
                using var instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName);

                Assert.True(gateMutex.WaitOne(TimeSpan.FromSeconds(5)));
                var gateOwned = true;
                try
                {
                    Assert.True(instanceMutex.WaitOne(TimeSpan.FromSeconds(5)));
                    // Step 1: Thread A releases SingleInstance FIRST while still holding UninstallGate.
                    instanceMutex.ReleaseMutex();
                    threadAInReleaseOrderWindow.Set();

                    // Step 2: Wait until Thread B probes SingleInstance, calls the real guard, and observes UninstallGateBusy.
                    Assert.True(threadBObservedGateBusy.Wait(TimeSpan.FromSeconds(5)));

                    // Step 3: Thread A finishes its release sequence by releasing UninstallGate.
                    gateMutex.ReleaseMutex();
                    gateOwned = false;
                }
                finally
                {
                    if (gateOwned)
                    {
                        gateMutex.ReleaseMutex();
                    }
                }
            }
            catch (Exception ex)
            {
                threadAError = ex;
                threadAInReleaseOrderWindow.Set();
            }
        });

        var threadB = new Thread(() =>
        {
            try
            {
                Assert.True(threadAInReleaseOrderWindow.Wait(TimeSpan.FromSeconds(5)));
                using var realGuard = new WindowsSingleInstanceGuard(instanceMutexName, gateMutexName);
                var observingGuard = new ObservingRealSingleInstanceGuard(
                    realGuard,
                    onOutcome: outcome =>
                    {
                        if (outcome == WindowsSingleInstanceAcquisitionResult.UninstallGateBusy)
                        {
                            threadBObservedGateBusy.Set();
                        }
                    });

                try
                {
                    handoffResult = WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire(
                        observingGuard,
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromMilliseconds(20),
                        out handoffError,
                        missingEventName,
                        instanceMutexName);

                    observedOutcomes = observingGuard.Outcomes.ToArray();
                    threadBCompletedHandoff.Set();

                    Assert.True(allowThreadBToRelease.Wait(TimeSpan.FromSeconds(5)));
                }
                finally
                {
                    realGuard.Release();
                }
            }
            catch (Exception ex)
            {
                threadBError = ex;
                threadBObservedGateBusy.Set();
                threadBCompletedHandoff.Set();
            }
        });

        threadA.Start();
        threadB.Start();

        try
        {
            Assert.True(threadBCompletedHandoff.Wait(TimeSpan.FromSeconds(5)));
            Assert.Null(threadAError);
            Assert.Null(threadBError);
            Assert.True(handoffResult, handoffError);
            Assert.Null(handoffError);

            // Verify from the test thread that while Thread B holds admission, BOTH real OS mutexes are owned by Thread B.
            gateHeldByThreadBAtSuccess = !CanAcquireAndReleaseOnSeparateThread(gateMutexName);
            instanceHeldByThreadBAtSuccess = !CanAcquireAndReleaseOnSeparateThread(instanceMutexName);
            Assert.True(gateHeldByThreadBAtSuccess, "Expected Thread B to own the real UninstallGate mutex upon handoff success.");
            Assert.True(instanceHeldByThreadBAtSuccess, "Expected Thread B to own the real SingleInstance mutex upon handoff success.");
        }
        finally
        {
            allowThreadBToRelease.Set();
            Assert.True(threadA.Join(TimeSpan.FromSeconds(5)));
            Assert.True(threadB.Join(TimeSpan.FromSeconds(5)));
        }

        Assert.Contains(WindowsSingleInstanceAcquisitionResult.UninstallGateBusy, observedOutcomes);
        Assert.Equal(WindowsSingleInstanceAcquisitionResult.Acquired, observedOutcomes.Last());

        // After Thread B releases its guard, both real OS mutexes are free again.
        Assert.True(CanAcquireAndReleaseOnSeparateThread(gateMutexName));
        Assert.True(CanAcquireAndReleaseOnSeparateThread(instanceMutexName));
    }

    private static bool CanAcquireAndReleaseOnSeparateThread(string mutexName)
    {
        var acquired = false;
        var probeThread = new Thread(() =>
        {
            using var mutex = new Mutex(initiallyOwned: false, mutexName);
            try
            {
                acquired = mutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                acquired = true;
            }

            if (acquired)
            {
                mutex.ReleaseMutex();
            }
        });
        probeThread.Start();
        Assert.True(probeThread.Join(TimeSpan.FromSeconds(5)));
        return acquired;
    }

    private sealed class ObservingRealSingleInstanceGuard : ISingleInstanceGuard, IWindowsDetailedSingleInstanceGuard
    {
        private readonly WindowsSingleInstanceGuard _inner;
        private readonly Action<WindowsSingleInstanceAcquisitionResult> _onOutcome;
        private readonly List<WindowsSingleInstanceAcquisitionResult> _outcomes = new();

        public ObservingRealSingleInstanceGuard(
            WindowsSingleInstanceGuard inner,
            Action<WindowsSingleInstanceAcquisitionResult> onOutcome)
        {
            _inner = inner;
            _onOutcome = onOutcome;
        }

        public IReadOnlyList<WindowsSingleInstanceAcquisitionResult> Outcomes => _outcomes;

        public bool TryAcquireSingleInstance() =>
            TryAcquireSingleInstanceDetail(suppressLogging: false) == WindowsSingleInstanceAcquisitionResult.Acquired;

        public WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)
        {
            var outcome = _inner.TryAcquireSingleInstanceDetail(suppressLogging);
            _outcomes.Add(outcome);
            _onOutcome(outcome);
            return outcome;
        }

        public void Release() => _inner.Release();

        public void Dispose() => _inner.Dispose();
    }

    private sealed class ProbeReleaseVerifyingRealGuard : ISingleInstanceGuard, IWindowsDetailedSingleInstanceGuard
    {
        private readonly WindowsSingleInstanceGuard _inner;
        private readonly Action _onBeforeAcquire;

        public ProbeReleaseVerifyingRealGuard(WindowsSingleInstanceGuard inner, Action onBeforeAcquire)
        {
            _inner = inner;
            _onBeforeAcquire = onBeforeAcquire;
        }

        public bool TryAcquireSingleInstance() =>
            TryAcquireSingleInstanceDetail(suppressLogging: false) == WindowsSingleInstanceAcquisitionResult.Acquired;

        public WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)
        {
            _onBeforeAcquire();
            return _inner.TryAcquireSingleInstanceDetail(suppressLogging);
        }

        public void Release() => _inner.Release();

        public void Dispose() => _inner.Dispose();
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
        public bool SimulatedBothLocksHeldAtSuccess { get; private set; }

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
            SimulatedBothLocksHeldAtSuccess = true;
            return WindowsSingleInstanceAcquisitionResult.Acquired;
        }

        public void Release()
        {
            Acquired = false;
            SimulatedBothLocksHeldAtSuccess = false;
        }

        public void Dispose() => Release();
    }

    private sealed class SequenceContentionGuard : ISingleInstanceGuard, IWindowsDetailedSingleInstanceGuard
    {
        private readonly IReadOnlyList<WindowsSingleInstanceAcquisitionResult> _sequence;

        public SequenceContentionGuard(params WindowsSingleInstanceAcquisitionResult[] sequence)
        {
            _sequence = sequence;
        }

        public int Attempts { get; private set; }

        public bool TryAcquireSingleInstance() =>
            TryAcquireSingleInstanceDetail() == WindowsSingleInstanceAcquisitionResult.Acquired;

        public WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)
        {
            var index = Math.Min(Attempts, _sequence.Count - 1);
            Attempts++;
            return _sequence[index];
        }

        public void Release() { }
        public void Dispose() { }
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

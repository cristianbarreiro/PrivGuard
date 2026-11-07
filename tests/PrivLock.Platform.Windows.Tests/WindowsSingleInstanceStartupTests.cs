using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

/// <summary>
/// Unit tests for single-instance startup admission and concurrency.
/// Note: These tests demonstrate mutual exclusion between concurrent tasks sharing process memory space
/// with test-isolated Local\ mutexes. They do not simulate distinct OS processes, different user SIDs,
/// Fast User Switching sessions, or Global\ object DACL behavior (which are validated in VM acceptance tests).
/// </summary>
public sealed class WindowsSingleInstanceStartupTests
{
    [Fact]
    public async Task ConcurrentLaunches_OnlyAdmittedProcessCanEnterRecoveryOrMutatePrivacyState()
    {
        var testMutex = $@"Local\PrivLock_SingleInstance_Test_{Guid.NewGuid():N}";
        var testGate = $@"Local\PrivLock_UninstallGate_Test_{Guid.NewGuid():N}";
        using var ready = new CountdownEvent(2);
        using var acquisitionCompleted = new CountdownEvent(2);
        using var start = new ManualResetEventSlim();
        var recoveryCalls = 0;
        var privacyMutations = 0;

        var launches = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            using var guard = new WindowsSingleInstanceGuard(testMutex, testGate);
            ready.Signal();
            start.Wait();
            var admitted = guard.TryAcquireSingleInstance();
            acquisitionCompleted.Signal();
            acquisitionCompleted.Wait();
            if (!admitted)
                return false;

            Interlocked.Increment(ref recoveryCalls);
            Interlocked.Increment(ref privacyMutations);
            return true;
        })).ToArray();

        Assert.True(ready.Wait(TimeSpan.FromSeconds(5)));
        start.Set();
        var admitted = await Task.WhenAll(launches);

        Assert.Single(admitted.Where(value => value));
        Assert.Equal(1, recoveryCalls);
        Assert.Equal(1, privacyMutations);
    }

    [Fact]
    public void RealNamedMutexes_WhenUninstallGateOwnedByAnotherThread_ReturnsUninstallGateBusyAndLeavesMainMutexFree()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var testMutex = $@"Local\PrivLock_SingleInstance_Test_{suffix}";
        var testGate = $@"Local\PrivLock_UninstallGate_Test_{suffix}";

        using var gateAcquired = new ManualResetEventSlim();
        using var releaseGate = new ManualResetEventSlim();
        Exception? ownerThreadError = null;

        var ownerThread = new Thread(() =>
        {
            try
            {
                using var gate = new Mutex(initiallyOwned: false, testGate);
                Assert.True(gate.WaitOne(TimeSpan.FromSeconds(5)));
                try
                {
                    gateAcquired.Set();
                    Assert.True(releaseGate.Wait(TimeSpan.FromSeconds(5)));
                }
                finally
                {
                    gate.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                ownerThreadError = ex;
                gateAcquired.Set();
            }
        });

        ownerThread.Start();
        try
        {
            Assert.True(gateAcquired.Wait(TimeSpan.FromSeconds(5)));
            Assert.Null(ownerThreadError);

            using var guard = new WindowsSingleInstanceGuard(testMutex, testGate);
            var outcome = guard.TryAcquireSingleInstanceDetail(suppressLogging: true);

            Assert.Equal(WindowsSingleInstanceAcquisitionResult.UninstallGateBusy, outcome);
            Assert.True(CanAcquireAndReleaseOnSeparateThread(testMutex));
        }
        finally
        {
            releaseGate.Set();
            ownerThread.Join(TimeSpan.FromSeconds(5));
        }

        Assert.Null(ownerThreadError);
    }

    [Fact]
    public void RealNamedMutexes_WhenMainMutexOwnedByAnotherThread_ReturnsSingleInstanceBusyAndReleasesGate()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var testMutex = $@"Local\PrivLock_SingleInstance_Test_{suffix}";
        var testGate = $@"Local\PrivLock_UninstallGate_Test_{suffix}";

        using var instanceAcquired = new ManualResetEventSlim();
        using var releaseInstance = new ManualResetEventSlim();
        Exception? ownerThreadError = null;

        var ownerThread = new Thread(() =>
        {
            try
            {
                using var mainMutex = new Mutex(initiallyOwned: false, testMutex);
                Assert.True(mainMutex.WaitOne(TimeSpan.FromSeconds(5)));
                try
                {
                    instanceAcquired.Set();
                    Assert.True(releaseInstance.Wait(TimeSpan.FromSeconds(5)));
                }
                finally
                {
                    mainMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                ownerThreadError = ex;
                instanceAcquired.Set();
            }
        });

        ownerThread.Start();
        try
        {
            Assert.True(instanceAcquired.Wait(TimeSpan.FromSeconds(5)));
            Assert.Null(ownerThreadError);

            using var guard = new WindowsSingleInstanceGuard(testMutex, testGate);
            var outcome = guard.TryAcquireSingleInstanceDetail(suppressLogging: true);

            Assert.Equal(WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy, outcome);

            // Verify from a distinct thread (to avoid same-thread recursive acquisition)
            // that the guard released the uninstall gate after failing on the main mutex.
            Assert.True(CanAcquireAndReleaseOnSeparateThread(testGate));
        }
        finally
        {
            releaseInstance.Set();
            ownerThread.Join(TimeSpan.FromSeconds(5));
        }

        Assert.Null(ownerThreadError);
    }

    [Fact]
    public void RealNamedMutexes_WhenBothFree_AcquiresBothAndReleaseAllowsSubsequentAcquisition()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var testMutex = $@"Local\PrivLock_SingleInstance_Test_{suffix}";
        var testGate = $@"Local\PrivLock_UninstallGate_Test_{suffix}";

        using var guard1 = new WindowsSingleInstanceGuard(testMutex, testGate);
        var firstOutcome = guard1.TryAcquireSingleInstanceDetail(suppressLogging: true);
        Assert.Equal(WindowsSingleInstanceAcquisitionResult.Acquired, firstOutcome);

        // While guard1 holds admission, neither mutex can be acquired by another thread.
        Assert.False(CanAcquireAndReleaseOnSeparateThread(testGate));
        Assert.False(CanAcquireAndReleaseOnSeparateThread(testMutex));

        guard1.Release();

        // After Release(), both mutexes are free and a second real guard on another thread can acquire and release them.
        WindowsSingleInstanceAcquisitionResult? secondOutcome = null;
        var secondThread = new Thread(() =>
        {
            using var guard2 = new WindowsSingleInstanceGuard(testMutex, testGate);
            secondOutcome = guard2.TryAcquireSingleInstanceDetail(suppressLogging: true);
            guard2.Release();
        });
        secondThread.Start();
        Assert.True(secondThread.Join(TimeSpan.FromSeconds(5)));
        Assert.Equal(WindowsSingleInstanceAcquisitionResult.Acquired, secondOutcome);
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
}

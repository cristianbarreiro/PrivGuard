using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsSingleInstanceStartupTests
{
    [Fact]
    public async Task ConcurrentLaunches_OnlyAdmittedProcessCanEnterRecoveryOrMutatePrivacyState()
    {
        using var ready = new CountdownEvent(2);
        using var acquisitionCompleted = new CountdownEvent(2);
        using var start = new ManualResetEventSlim();
        var recoveryCalls = 0;
        var privacyMutations = 0;

        var launches = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            using var guard = new WindowsSingleInstanceGuard();
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
}

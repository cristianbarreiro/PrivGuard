using PrivLock.Desktop;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class ProgramStartupActivationTests
{
    [Fact]
    public void MinimizedLaunchDoesNotForwardActivationButKeepsTheListenerForLaterNormalLaunches()
    {
        var args = new[] { "--minimized" };

        Assert.False(Program.ShouldRequestExistingInstanceActivation(args, safeUninstallRequested: false));
        Assert.True(Program.ShouldStartDesktopUi(args, safeUninstallRequested: false));
    }

    [Theory]
    [InlineData("--unblock-and-exit")]
    [InlineData("-u")]
    public void RecoveryCommandDoesNotRequestOrStartDesktopActivation(string argument)
    {
        var args = new[] { argument };

        Assert.False(Program.ShouldRequestExistingInstanceActivation(args, safeUninstallRequested: false));
        Assert.False(Program.ShouldStartDesktopUi(args, safeUninstallRequested: false));
    }

    [Fact]
    public void SafeUninstallDoesNotRequestOrStartDesktopActivation()
    {
        var args = Array.Empty<string>();

        Assert.False(Program.ShouldRequestExistingInstanceActivation(args, safeUninstallRequested: true));
        Assert.False(Program.ShouldStartDesktopUi(args, safeUninstallRequested: true));
    }
}

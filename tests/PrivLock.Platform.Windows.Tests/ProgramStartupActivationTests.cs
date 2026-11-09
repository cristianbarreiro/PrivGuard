using Avalonia;
using PrivLock.Desktop;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class ProgramStartupActivationTests
{
    [Fact]
    public void WindowsAvaloniaConfigurationRegistersWindowingAndRenderingSubsystems()
    {
        var builder = AppBuilder.Configure(() => new StartupTestApplication());

        Program.ConfigureAvaloniaPlatform(builder);

        Assert.NotNull(builder.RuntimePlatformServicesInitializer);
        Assert.NotNull(builder.WindowingSubsystemInitializer);
        Assert.NotNull(builder.RenderingSubsystemInitializer);
    }

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

    private sealed class StartupTestApplication : Avalonia.Application
    {
    }
}

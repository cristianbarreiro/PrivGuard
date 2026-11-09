#if WINDOWS
using System.Runtime.Versioning;
using Windows.ApplicationModel;
#endif

namespace PrivLock.Platform.Windows.System;

internal enum WindowsStartupTaskState
{
    Disabled,
    DisabledByUser,
    DisabledByPolicy,
    Enabled,
    EnabledByPolicy
}

internal interface IWindowsStartupTaskAccessor
{
    Task<WindowsStartupTaskState> GetStateAsync();
    Task<WindowsStartupTaskState> RequestEnableAsync();
    Task DisableAsync();
}

#if WINDOWS
[SupportedOSPlatform("windows10.0.19041")]
internal sealed class WindowsStartupTaskAccessor(string taskId) : IWindowsStartupTaskAccessor
{
    public async Task<WindowsStartupTaskState> GetStateAsync()
    {
        var task = await StartupTask.GetAsync(taskId);
        return MapState(task.State);
    }

    public async Task<WindowsStartupTaskState> RequestEnableAsync()
    {
        var task = await StartupTask.GetAsync(taskId);
        return MapState(await task.RequestEnableAsync());
    }

    public async Task DisableAsync()
    {
        var task = await StartupTask.GetAsync(taskId);
        task.Disable();
    }

    private static WindowsStartupTaskState MapState(StartupTaskState state) =>
        state switch
        {
            StartupTaskState.Disabled => WindowsStartupTaskState.Disabled,
            StartupTaskState.DisabledByUser => WindowsStartupTaskState.DisabledByUser,
            StartupTaskState.DisabledByPolicy => WindowsStartupTaskState.DisabledByPolicy,
            StartupTaskState.Enabled => WindowsStartupTaskState.Enabled,
            StartupTaskState.EnabledByPolicy => WindowsStartupTaskState.EnabledByPolicy,
            _ => throw new InvalidOperationException($"Unsupported Windows StartupTask state: {state}.")
        };
}
#else
internal sealed class WindowsStartupTaskAccessor(string taskId) : IWindowsStartupTaskAccessor
{
    public Task<WindowsStartupTaskState> GetStateAsync() =>
        Task.FromException<WindowsStartupTaskState>(
            new PlatformNotSupportedException("MSIX StartupTask APIs require a Windows-targeted build."));

    public Task<WindowsStartupTaskState> RequestEnableAsync() =>
        Task.FromException<WindowsStartupTaskState>(
            new PlatformNotSupportedException("MSIX StartupTask APIs require a Windows-targeted build."));

    public Task DisableAsync() =>
        Task.FromException(new PlatformNotSupportedException(
            "MSIX StartupTask APIs require a Windows-targeted build."));
}
#endif

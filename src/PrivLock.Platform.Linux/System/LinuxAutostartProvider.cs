using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Linux.System;

/// <summary>
/// Manages XDG Autostart (.desktop entry in ~/.config/autostart) on Linux.
/// </summary>
public sealed class LinuxAutostartProvider : IAutostartProvider
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LinuxAutostartProvider>();

    private readonly string _desktopFilePath;

    public LinuxAutostartProvider()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var autostartDir = Path.Combine(home, ".config", "autostart");
        _desktopFilePath = Path.Combine(autostartDir, "privgvard.desktop");
    }

    public Task<AutostartStatus> GetAutostartStatusAsync()
    {
        try
        {
            return Task.FromResult(File.Exists(_desktopFilePath)
                ? new AutostartStatus(AutostartState.Enabled)
                : new AutostartStatus(AutostartState.Disabled));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read Linux autostart state");
            return Task.FromResult(new AutostartStatus(AutostartState.Failed, ex.Message));
        }
    }

    public Task<OperationResult> EnableAutostartAsync()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                return Task.FromResult(OperationResult.Fail("Cannot determine executable path for Linux autostart."));
            }

            var dir = Path.GetDirectoryName(_desktopFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var desktopEntry = $"""
                [Desktop Entry]
                Type=Application
                Name=PrivGvard
                Comment=Camera & Microphone Privacy Guard
                Exec="{exePath}" --minimized
                Icon=privgvard
                Terminal=false
                Categories=Utility;Security;
                X-GNOME-Autostart-enabled=true
                """;

            File.WriteAllText(_desktopFilePath, desktopEntry);
            if (!File.Exists(_desktopFilePath) ||
                !File.ReadAllText(_desktopFilePath).Contains($"Exec=\"{exePath}\" --minimized", StringComparison.Ordinal))
            {
                return Task.FromResult(OperationResult.Fail("Linux autostart registration could not be verified."));
            }

            Log.Information("Linux autostart enabled at {Path}", _desktopFilePath);
            return Task.FromResult(OperationResult.Ok());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create Linux autostart desktop entry");
            return Task.FromResult(OperationResult.Fail(ex.Message));
        }
    }

    public Task<OperationResult> DisableAutostartAsync()
    {
        try
        {
            if (File.Exists(_desktopFilePath))
            {
                File.Delete(_desktopFilePath);
                Log.Information("Linux autostart disabled");
            }
            return Task.FromResult(File.Exists(_desktopFilePath)
                ? OperationResult.Fail("Linux autostart disable could not be verified.")
                : OperationResult.Ok());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove Linux autostart desktop entry");
            return Task.FromResult(OperationResult.Fail(ex.Message));
        }
    }
}

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PrivLock.Application.Services;
using PrivLock.Platform.Abstractions;
using PrivLock.UI;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public class TrayIconLocalizationTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public void MinimizedStartup_HidesOnlyWhenTrayIsReady(
        bool startMinimized,
        bool trayInitialized,
        bool expected)
    {
        Assert.Equal(expected, App.ShouldHideStartupWindow(startMinimized, trayInitialized));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void Close_HidesOnlyWhenTrayIsAvailable(
        bool trayAvailable,
        bool allowApplicationClose,
        bool expectedHide)
    {
        Assert.Equal(
            expectedHide,
            PrivLock.UI.Views.MainWindow.ShouldHideOnClose(trayAvailable, allowApplicationClose));
    }

    private sealed class InMemoryStateStore : IStateStore
    {
        public DesiredState State { get; set; } = new DesiredState { Language = "es" };
        public DesiredState Load() => State;
        public void Save(DesiredState state) => State = state;
    }

    private static void EnsureAppInitialized()
    {
        if (Avalonia.Application.Current == null)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();
        }
    }

    [Fact]
    public void TrayIcon_MenuLocalization_UpdatesAcrossLanguages()
    {
        EnsureAppInitialized();

        var stateStore = new InMemoryStateStore();
        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();

        var app = new App(null!, null!, null, localizationService);
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        app.ApplicationLifetime = lifetime;

        var method = typeof(App).GetMethod("SetupTrayIcon", global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance);
        method?.Invoke(app, new object[] { lifetime });

        var trayIcons = TrayIcon.GetIcons(app);
        Assert.NotNull(trayIcons);
        Assert.NotEmpty(trayIcons);

        var trayIcon = trayIcons[0];
        Assert.NotNull(trayIcon.Menu);

        var openItem = trayIcon.Menu.Items[0] as NativeMenuItem;
        var exitItem = trayIcon.Menu.Items[2] as NativeMenuItem;

        Assert.NotNull(openItem);
        Assert.NotNull(exitItem);

        // 1. Spanish (Initial)
        Assert.Equal("Abrir PrivGvard", openItem.Header);
        Assert.Equal("Salir", exitItem.Header);
        Assert.Equal("PrivGvard - Bloqueador de Cámara y Micrófono", trayIcon.ToolTipText);

        // 2. Switch to English
        localizationService.SetLanguage("en");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Open PrivGvard", openItem.Header);
        Assert.Equal("Exit", exitItem.Header);
        Assert.Equal("PrivGvard - Camera & Microphone Blocker", trayIcon.ToolTipText);

        // 3. Switch back to Spanish
        localizationService.SetLanguage("es");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Abrir PrivGvard", openItem.Header);
        Assert.Equal("Salir", exitItem.Header);
        Assert.Equal("PrivGvard - Bloqueador de Cámara y Micrófono", trayIcon.ToolTipText);
    }
}

using System.Runtime.InteropServices;
using PrivLock.Domain.Capabilities;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;

namespace PrivLock.Platform.Windows;

/// <summary>
/// Declares capabilities and platform diagnostics on Windows, scoped by the resolved edition profile.
/// </summary>
public sealed class WindowsCapabilityProvider : IPlatformCapabilityProvider
{
    private readonly IElevationProvider _elevationProvider;
    private readonly IEditionProfileResolver _editionProfileResolver;

    public WindowsCapabilityProvider(IElevationProvider elevationProvider)
        : this(elevationProvider, new DefaultEditionProfileResolver(new WindowsPackageIdentityDetector()))
    {
    }

    public WindowsCapabilityProvider(
        IElevationProvider elevationProvider,
        IEditionProfileResolver editionProfileResolver)
    {
        ArgumentNullException.ThrowIfNull(elevationProvider);
        ArgumentNullException.ThrowIfNull(editionProfileResolver);
        _elevationProvider = elevationProvider;
        _editionProfileResolver = editionProfileResolver;
    }

    public PlatformCapabilities Capabilities => _editionProfileResolver.Profile switch
    {
        EditionProfile.Standalone => new PlatformCapabilities
        {
            CameraProtectionLevel = CapabilityLevel.DualLayer,
            MicrophoneProtectionLevel = CapabilityLevel.DualLayer,
            SupportsHardwareDisable = true,
            SupportsSystemPolicy = true,
            SupportsAudioMuteLock = true,
            RequiresElevationForBlock = true,
            SupportsGlobalHotkey = true,
            SupportsAutostart = true,
            SupportsSystemTray = true
        },
        EditionProfile.Store => new PlatformCapabilities
        {
            // Store Edition (D-01 / D-02): WASAPI endpoint mute and ms-settings: navigation only.
            // Excludes PnP hardware disablement, HKLM system policies, and elevation.
            CameraProtectionLevel = CapabilityLevel.Software,
            MicrophoneProtectionLevel = CapabilityLevel.Software,
            SupportsHardwareDisable = false,
            SupportsSystemPolicy = false,
            SupportsAudioMuteLock = true,
            RequiresElevationForBlock = false,
            SupportsGlobalHotkey = true,
            SupportsAutostart = true,
            SupportsSystemTray = true
        },
        _ => new PlatformCapabilities
        {
            // Fail closed for UntrustedOrIndeterminate
            CameraProtectionLevel = CapabilityLevel.None,
            MicrophoneProtectionLevel = CapabilityLevel.None,
            SupportsHardwareDisable = false,
            SupportsSystemPolicy = false,
            SupportsAudioMuteLock = false,
            RequiresElevationForBlock = false,
            SupportsGlobalHotkey = false,
            SupportsAutostart = false,
            SupportsSystemTray = true
        }
    };

    public PlatformInfo PlatformInfo => new()
    {
        OperatingSystemName = "Windows",
        OsVersion = Environment.OSVersion.VersionString,
        Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        Is64Bit = Environment.Is64BitOperatingSystem,
        IsElevated = _editionProfileResolver.SupportsPrivilegedOperations && _elevationProvider.IsElevated,
        DesktopEnvironment = "Windows Shell (Explorer)"
    };
}

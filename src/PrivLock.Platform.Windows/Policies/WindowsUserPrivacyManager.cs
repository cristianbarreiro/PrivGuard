using Microsoft.Win32;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Serilog;

namespace PrivLock.Platform.Windows.Policies;

/// <summary>
/// Read-only contract for inspecting Windows HKCU CapabilityAccessManager ConsentStore status.
/// Permitted in both Standalone and Store editions to observe the OS privacy settings state.
/// </summary>
public interface IWindowsUserPrivacyReader
{
    BlockStatus GetCameraUserPrivacyStatus();
    BlockStatus GetMicrophoneUserPrivacyStatus();
}

/// <summary>
/// Write contract for modifying Windows HKCU CapabilityAccessManager ConsentStore values.
/// Strictly restricted to Standalone edition; prohibited in Store and Untrusted profiles (D-02).
/// </summary>
public interface IWindowsUserPrivacyWriter
{
    OperationResult SetCameraUserPrivacy(BlockStatus status);
    OperationResult SetMicrophoneUserPrivacy(BlockStatus status);
}

/// <summary>
/// Manages user-level privacy permissions in the Windows Capability Consent Store (HKCU).
/// Separates read observation from write mutation and enforces edition profile guards (D-02).
/// </summary>
public class WindowsUserPrivacyManager : IWindowsUserPrivacyReader, IWindowsUserPrivacyWriter
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsUserPrivacyManager>();

    internal const string ConsentStoreCameraPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam";
    internal const string ConsentStoreCameraNonPackagedPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam\NonPackaged";
    internal const string ConsentStoreMicPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone";
    internal const string ConsentStoreMicNonPackagedPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone\NonPackaged";
    internal const string ConsentValueName = "Value";

    private readonly IEditionProfileResolver _editionProfileResolver;
    private readonly Action<string, string, string> _registryWriter;

    public WindowsUserPrivacyManager()
        : this(new DefaultEditionProfileResolver(new WindowsPackageIdentityDetector()), null)
    {
    }

    public WindowsUserPrivacyManager(
        IEditionProfileResolver editionProfileResolver,
        Action<string, string, string>? registryWriter = null)
    {
        ArgumentNullException.ThrowIfNull(editionProfileResolver);
        _editionProfileResolver = editionProfileResolver;
        _registryWriter = registryWriter ?? DefaultSetRegistryStringValue;
    }

    public virtual OperationResult SetCameraUserPrivacy(BlockStatus status)
    {
        if (_editionProfileResolver.Profile != EditionProfile.Standalone)
        {
            Log.Warning(
                "Rejected ConsentStore Camera write attempt under edition profile {Profile}",
                _editionProfileResolver.ProfileName);
            return OperationResult.Fail(
                $"Writing to HKCU ConsentStore is prohibited under edition profile '{_editionProfileResolver.ProfileName}'.");
        }

        Log.Information("Setting Windows user privacy for Camera: Status={Status}", status);

        try
        {
            var isBlocked = status == BlockStatus.Blocked;
            var consentValue = isBlocked ? "Deny" : "Allow";

            // 1. Consent Store (UWP / Windows Camera App / Packaged Apps)
            _registryWriter(ConsentStoreCameraPath, ConsentValueName, consentValue);

            // 2. Consent Store NonPackaged (Desktop Apps: Chrome, Firefox, Zoom, Teams, OBS, etc.)
            _registryWriter(ConsentStoreCameraNonPackagedPath, ConsentValueName, consentValue);

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update Windows Camera user privacy in ConsentStore");
            return OperationResult.Fail($"Camera user privacy error: {ex.Message}");
        }
    }

    public virtual OperationResult SetMicrophoneUserPrivacy(BlockStatus status)
    {
        if (_editionProfileResolver.Profile != EditionProfile.Standalone)
        {
            Log.Warning(
                "Rejected ConsentStore Microphone write attempt under edition profile {Profile}",
                _editionProfileResolver.ProfileName);
            return OperationResult.Fail(
                $"Writing to HKCU ConsentStore is prohibited under edition profile '{_editionProfileResolver.ProfileName}'.");
        }

        Log.Information("Setting Windows user privacy for Microphone: Status={Status}", status);

        try
        {
            var isBlocked = status == BlockStatus.Blocked;
            var consentValue = isBlocked ? "Deny" : "Allow";

            // 1. Consent Store (UWP / Voice Recorder / Packaged Apps)
            _registryWriter(ConsentStoreMicPath, ConsentValueName, consentValue);

            // 2. Consent Store NonPackaged (Desktop Apps: Chrome, Zoom, Teams, Discord, etc.)
            _registryWriter(ConsentStoreMicNonPackagedPath, ConsentValueName, consentValue);

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update Windows Microphone user privacy in ConsentStore");
            return OperationResult.Fail($"Microphone user privacy error: {ex.Message}");
        }
    }

    public virtual BlockStatus GetCameraUserPrivacyStatus()
    {
        if (_editionProfileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            return BlockStatus.Unknown;

        return ReadCombinedConsentStatus(
            ConsentStoreCameraPath,
            ConsentStoreCameraNonPackagedPath,
            "Camera");
    }

    public virtual BlockStatus GetMicrophoneUserPrivacyStatus()
    {
        if (_editionProfileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            return BlockStatus.Unknown;

        return ReadCombinedConsentStatus(
            ConsentStoreMicPath,
            ConsentStoreMicNonPackagedPath,
            "Microphone");
    }

    private static BlockStatus ReadCombinedConsentStatus(
        string packagedPath,
        string nonPackagedPath,
        string targetName)
    {
        try
        {
            var packaged = ReadConsentValue(packagedPath);
            var nonPackaged = ReadConsentValue(nonPackagedPath);
            if (packaged == BlockStatus.Blocked && nonPackaged == BlockStatus.Blocked)
                return BlockStatus.Blocked;
            if (packaged == BlockStatus.Allowed && nonPackaged == BlockStatus.Allowed)
                return BlockStatus.Allowed;

            Log.Warning(
                "Windows {Target} ConsentStore values disagree; protection state is partial/unknown",
                targetName);
            return BlockStatus.Unknown;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read Windows {Target} user privacy status", targetName);
            return BlockStatus.Unknown;
        }
    }

    private static BlockStatus ReadConsentValue(string path)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path, writable: false);
        if (key == null || !key.GetValueNames().Contains(ConsentValueName, StringComparer.OrdinalIgnoreCase))
            return BlockStatus.Allowed;
        if (key.GetValueKind(ConsentValueName) != RegistryValueKind.String)
            return BlockStatus.Unknown;

        var value = key?.GetValue(ConsentValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        if (value is not string stringValue)
            return BlockStatus.Unknown;
        if (string.Equals(stringValue, "Deny", StringComparison.OrdinalIgnoreCase))
            return BlockStatus.Blocked;
        if (string.Equals(stringValue, "Allow", StringComparison.OrdinalIgnoreCase))
            return BlockStatus.Allowed;
        return BlockStatus.Unknown;
    }

    private static void DefaultSetRegistryStringValue(string subKeyPath, string valueName, string value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(subKeyPath, writable: true)
            ?? throw new IOException($"Could not create/open Registry key '{subKeyPath}'.");
        key.SetValue(valueName, value, RegistryValueKind.String);
    }
}

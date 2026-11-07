using System.Diagnostics;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.Devices;
using PrivLock.Platform.Windows.Elevation;
using PrivLock.Platform.Windows.System;
using PrivLock.Platform.Windows.Policies;
using PrivLock.Platform.Windows.Privileged;
using Serilog;

namespace PrivLock.Platform.Windows;

/// <summary>
/// Native Windows implementation of IDeviceProtectionProvider.
/// Supports clean two-tier protection:
/// 1. Standard Protection: HKCU Capability Consent Store (Packaged & NonPackaged, Standalone only) + Windows Core Audio WASAPI Mute (0 elevation).
/// 2. Secure Protection: HKLM Group Policy & CfgMgr32 PnP hardware device node disable (Standalone only, on-demand UAC elevation, single prompt).
/// </summary>
public sealed class WindowsProtectionProvider : IDeviceProtectionProvider
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsProtectionProvider>();

    private readonly IDeviceDetector _deviceDetector;
    private readonly WindowsDeviceController _deviceController;
    private readonly WindowsCoreAudioController _coreAudioController;
    private readonly WindowsPolicyManager _policyManager;
    private readonly WindowsUserPrivacyManager _userPrivacyManager;
    private readonly IElevationProvider _elevationProvider;
    private readonly IPrivacySessionStore _privacySessionStore;
    private readonly IEditionProfileResolver _profileResolver;

    public WindowsProtectionProvider(
        IDeviceDetector deviceDetector,
        WindowsDeviceController deviceController,
        WindowsCoreAudioController coreAudioController,
        WindowsPolicyManager policyManager,
        WindowsUserPrivacyManager userPrivacyManager,
        IElevationProvider elevationProvider,
        IPrivacySessionStore privacySessionStore,
        IEditionProfileResolver? profileResolver = null)
    {
        _deviceDetector = deviceDetector;
        _deviceController = deviceController;
        _coreAudioController = coreAudioController;
        _policyManager = policyManager;
        _userPrivacyManager = userPrivacyManager;
        _elevationProvider = elevationProvider;
        _privacySessionStore = privacySessionStore;
        _profileResolver = profileResolver
            ?? new DefaultEditionProfileResolver(new WindowsPackageIdentityDetector());
    }

    public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
    {
        if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            Log.Warning(
                "Rejected EnableStandardProtectionAsync(Target={Target}) because EditionProfile is UntrustedOrIndeterminate.",
                target);
            return Task.FromResult(OperationResult.Fail(
                "Standard Protection is blocked because the runtime edition profile is untrusted or indeterminate."));
        }

        if (_profileResolver.Profile == EditionProfile.Store &&
            target is BlockTarget.Camera or BlockTarget.Both)
        {
            Log.Warning(
                "Rejected direct camera Standard Protection mutation in Store edition (Target={Target}).",
                target);
            return Task.FromResult(OperationResult.Fail(
                "Direct camera ConsentStore modification is not supported in the Microsoft Store edition. Use Windows Settings (ms-settings:privacy-webcam) instead."));
        }

        Log.Information(
            "Enabling Windows Standard Protection: Target={Target}, Profile={Profile}",
            target,
            _profileResolver.Profile);
        var details = new List<DeviceOperationDetail>();
        try
        {
            var session = LoadTrackedSession();

            if (_profileResolver.Profile == EditionProfile.Standalone)
            {
                // Apply each captured value independently. CompareAndApplyProtected repeats the
                // comparison immediately before the native write.
                foreach (var policy in GetTrackedResources<OriginalPolicyState>(
                             session, ProtectionLayer.Standard, target).Where(resource => resource.RequiresModification))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = WindowsRegistryValueCodec.CompareAndApplyProtected(policy);
                    details.Add(ToResourceDetail(policy, result, "Windows privacy consent value"));
                    if (!result.Success)
                        return Task.FromResult(FailForResource(result, details));
                }
            }

            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                foreach (var endpoint in GetTrackedResources<OriginalAudioEndpointState>(
                             session,
                             ProtectionLayer.Standard,
                             BlockTarget.Microphone).Where(resource => resource.RequiresModification))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = CompareAndMuteEndpoint(endpoint);
                    details.Add(ToResourceDetail(endpoint, result, "Audio capture endpoint"));
                    if (!result.Success)
                        return Task.FromResult(FailForResource(result, details));
                }
            }

            return Task.FromResult(OperationResult.Ok(details));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(OperationResult.Fail("The Standard protection operation was cancelled.", details));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected Standard protection failure after {CompletedCount} resource result(s)", details.Count);
            return Task.FromResult(OperationResult.Fail(
                $"Unexpected Standard protection failure: {ex.Message}",
                details,
                outcomeUncertain: true));
        }
    }

    public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
    {
        if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            return Task.FromResult(OperationResult.Fail(
                "Standard Protection teardown is blocked because the runtime edition profile is untrusted or indeterminate."));
        }

        if (_profileResolver.Profile == EditionProfile.Store &&
            target is BlockTarget.Camera or BlockTarget.Both)
        {
            return Task.FromResult(OperationResult.Fail(
                "Direct camera ConsentStore modification is not supported in the Microsoft Store edition. Use Windows Settings (ms-settings:privacy-webcam) instead."));
        }

        Log.Error("Rejected non-reversible Windows Standard teardown for Target={Target}", target);
        return Task.FromResult(OperationResult.Fail(
            "Windows protection must be disabled through PrivacySessionService so exact original state is restored."));
    }

    public async Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
    {
        if (_profileResolver.Profile != EditionProfile.Standalone || !_profileResolver.SupportsPrivilegedOperations)
        {
            Log.Warning(
                "Rejected EnableSecureProtectionAsync(Target={Target}) because EditionProfile is {Profile}.",
                target,
                _profileResolver.Profile);
            return OperationResult.Fail(
                $"Secure Protection (PnP + HKLM Group Policy) is not supported in the {_profileResolver.Profile} edition profile.");
        }

        var sw = Stopwatch.StartNew();
        Log.Information("Enabling Windows Secure Protection: Target={Target}, IsElevated={IsElevated}",
            target, _elevationProvider.IsElevated);

        var details = new List<DeviceOperationDetail>();
        try
        {
            var session = LoadTrackedSession();
            foreach (var policy in GetTrackedResources<OriginalPolicyState>(
                         session, ProtectionLayer.Secure, target).Where(resource => resource.RequiresModification))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await ExecuteSecureResourceAsync("apply-policy", policy);
                details.Add(ToResourceDetail(policy, result, "Windows machine privacy policy"));
                if (!result.Success)
                    return FailForResource(result, details);
            }

            foreach (var device in GetTrackedResources<OriginalDeviceState>(
                         session, ProtectionLayer.Secure, target).Where(resource => resource.RequiresModification))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await ExecuteSecureResourceAsync("apply-device", device);
                details.Add(ToResourceDetail(device, result, device.FriendlyName));
                if (!result.Success)
                    return FailForResource(result, details);
            }

            sw.Stop();
            Log.Information("Windows Secure Protection enabled in {DurationMs}ms", sw.ElapsedMilliseconds);
            return OperationResult.Ok(details);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Fail("The Secure protection operation was cancelled.", details);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected Secure protection failure after {CompletedCount} resource result(s)", details.Count);
            return OperationResult.Fail(
                $"Unexpected Secure protection failure: {ex.Message}",
                details,
                outcomeUncertain: true);
        }
    }

    public async Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (_profileResolver.Profile != EditionProfile.Standalone || !_profileResolver.SupportsPrivilegedOperations)
        {
            Log.Warning(
                "Rejected DisableSecureProtectionAsync(Target={Target}) because EditionProfile is {Profile}.",
                target,
                _profileResolver.Profile);
            return OperationResult.Fail(
                $"Secure Protection (PnP + HKLM Group Policy) is not supported in the {_profileResolver.Profile} edition profile.");
        }

        Log.Error("Rejected non-reversible Windows Secure teardown for Target={Target}", target);
        return OperationResult.Fail(
            "Windows protection must be disabled through PrivacySessionService so exact original state is restored.");
    }

    public async Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default)
    {
        if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            const string untrustedMsg = "Protection state is unavailable because the runtime edition profile is untrusted or indeterminate.";
            return new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Unknown,
                    SecureState = SecureProtectionState.Unknown,
                    Availability = TargetDeviceAvailability.Unknown,
                    StandardIsVerified = false,
                    SecureIsVerified = false,
                    IsVerified = false,
                    StatusMessage = untrustedMsg
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Unknown,
                    SecureState = SecureProtectionState.Unknown,
                    Availability = TargetDeviceAvailability.Unknown,
                    EndpointAvailability = AudioEndpointAvailability.Unknown,
                    StandardIsVerified = false,
                    SecureIsVerified = false,
                    IsVerified = false,
                    StatusMessage = untrustedMsg
                }
            };
        }

        var isStoreEdition = _profileResolver.Profile == EditionProfile.Store;

        // Derive every displayed state from current Windows state. DesiredState is a preference,
        // never proof that a privacy mechanism is effectively active.
        var camUserPrivacy = _userPrivacyManager.GetCameraUserPrivacyStatus();
        var micUserPrivacy = _userPrivacyManager.GetMicrophoneUserPrivacyStatus();
        var microphoneEndpoints = _coreAudioController.GetMicrophoneMuteStates();

        var cameras = await _deviceDetector.DetectCamerasAsync(cancellationToken);
        var mics = await _deviceDetector.DetectMicrophonesAsync(cancellationToken);

        var camStandardState = ToStandardState(camUserPrivacy);
        var micStandardState = isStoreEdition
            ? ToStandardState(EvaluateStoreMicrophoneStandardStatus(
                microphoneEndpoints.Success,
                microphoneEndpoints.States))
            : ToStandardState(EvaluateMicrophoneStandardStatus(
                micUserPrivacy,
                microphoneEndpoints.Success,
                microphoneEndpoints.States));

        SecureProtectionState camSecureState;
        SecureProtectionState micSecureState;
        bool camSecureIsVerified;
        bool micSecureIsVerified;

        if (isStoreEdition)
        {
            camSecureState = SecureProtectionState.Unavailable;
            micSecureState = SecureProtectionState.Unavailable;
            camSecureIsVerified = true;
            micSecureIsVerified = true;
        }
        else
        {
            var camPolicy = _policyManager.GetCameraPolicyStatus();
            var micPolicy = _policyManager.GetMicrophonePolicyStatus();

            var camDeviceStateKnown = cameras.All(IsCleanOrExplicitlyDisabled);
            var micDeviceStateKnown = mics.All(IsCleanOrExplicitlyDisabled);
            var camDeviceBlocked = cameras.Count > 0 && cameras.All(device =>
                device.ConfigurationProblemCode == CfgMgrInterop.CM_PROB_DISABLED);
            var micDeviceBlocked = mics.Count > 0 && mics.All(device =>
                device.ConfigurationProblemCode == CfgMgrInterop.CM_PROB_DISABLED);
            var camDevicesEnabled = cameras.Count > 0 && cameras.All(device =>
                device.ConfigurationProblemCode == 0);
            var micDevicesEnabled = mics.Count > 0 && mics.All(device =>
                device.ConfigurationProblemCode == 0);

            var camSecureEvaluation = EvaluateSecureState(
                camPolicy, cameras.Count, camDeviceStateKnown, camDeviceBlocked, camDevicesEnabled);
            var micSecureEvaluation = EvaluateSecureState(
                micPolicy, mics.Count, micDeviceStateKnown, micDeviceBlocked, micDevicesEnabled);

            camSecureState = ToSecureState(
                camSecureEvaluation.IsActive, camSecureEvaluation.IsUnknown, camStandardState);
            micSecureState = ToSecureState(
                micSecureEvaluation.IsActive, micSecureEvaluation.IsUnknown, micStandardState);
            camSecureIsVerified = camSecureState != SecureProtectionState.Unknown;
        }

        AudioHardwareMuteSummary? micHwSummary = null;
        if (microphoneEndpoints.Success && microphoneEndpoints.States.Count > 0)
        {
            micHwSummary = new AudioHardwareMuteSummary
            {
                SupportedCount = microphoneEndpoints.States.Count(s => s.HardwareMuteSupport == AudioEndpointHardwareMuteSupport.Supported),
                NotSupportedCount = microphoneEndpoints.States.Count(s => s.HardwareMuteSupport == AudioEndpointHardwareMuteSupport.NotSupported),
                UnknownCount = microphoneEndpoints.States.Count(s => s.HardwareMuteSupport == AudioEndpointHardwareMuteSupport.Unknown),
                TotalCount = microphoneEndpoints.States.Count
            };
        }

        var micEndpointAvailability = DetermineAudioEndpointAvailability(
            microphoneEndpoints.Success,
            microphoneEndpoints.States.Count);

        var micHardwareAvailability = DetermineMicrophoneHardwareAvailability(
            mics.Count,
            micEndpointAvailability);

        if (!isStoreEdition)
        {
            micSecureIsVerified = micSecureState != SecureProtectionState.Unknown &&
                                  micHardwareAvailability != TargetDeviceAvailability.Unknown;
        }
        else
        {
            micSecureIsVerified = micHardwareAvailability != TargetDeviceAvailability.Unknown;
        }

        var micStandardIsVerified = EvaluateMicrophoneStandardVerification(
            micStandardState,
            micEndpointAvailability,
            micHwSummary);
        var micIsVerified = micStandardIsVerified && micSecureIsVerified;

        var micStatusMessage = DetermineMicrophoneStatusMessage(
            micHardwareAvailability,
            micEndpointAvailability,
            micStandardState,
            micSecureState,
            micHwSummary);

        var camAvailability = cameras.Count > 0
            ? TargetDeviceAvailability.Available
            : TargetDeviceAvailability.NotDetected;
        var camStandardIsVerified = camStandardState != StandardProtectionState.Unknown;
        var camIsVerified = camStandardIsVerified && camSecureIsVerified;
        var camStatusMessage = camAvailability == TargetDeviceAvailability.NotDetected
            ? "No camera detected."
            : (!camIsVerified
                ? "Windows camera protection state is partial or could not be fully verified."
                : null);

        return new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = camStandardState,
                SecureState = camSecureState,
                Availability = camAvailability,
                StandardIsVerified = camStandardIsVerified,
                SecureIsVerified = camSecureIsVerified,
                IsVerified = camIsVerified,
                StatusMessage = camStatusMessage
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = micStandardState,
                SecureState = micSecureState,
                Availability = micHardwareAvailability,
                EndpointAvailability = micEndpointAvailability,
                HardwareMuteSummary = micHwSummary,
                StandardIsVerified = micStandardIsVerified,
                SecureIsVerified = micSecureIsVerified,
                IsVerified = micIsVerified,
                StatusMessage = micStatusMessage
            }
        };
    }

    internal static bool EvaluateMicrophoneStandardVerification(
        StandardProtectionState standardState,
        AudioEndpointAvailability endpointAvailability,
        AudioHardwareMuteSummary? hardwareMuteSummary)
    {
        if (standardState == StandardProtectionState.Unknown ||
            endpointAvailability == AudioEndpointAvailability.Unknown)
        {
            return false;
        }

        if (endpointAvailability == AudioEndpointAvailability.NotDetected)
        {
            return true;
        }

        return hardwareMuteSummary is { TotalCount: > 0 } &&
               hardwareMuteSummary.SupportedCount == hardwareMuteSummary.TotalCount &&
               hardwareMuteSummary.NotSupportedCount == 0 &&
               hardwareMuteSummary.UnknownCount == 0;
    }

    internal static AudioEndpointAvailability DetermineAudioEndpointAvailability(
        bool endpointQuerySucceeded,
        int endpointCount)
    {
        if (!endpointQuerySucceeded)
            return AudioEndpointAvailability.Unknown;

        return endpointCount > 0
            ? AudioEndpointAvailability.Available
            : AudioEndpointAvailability.NotDetected;
    }

    internal static TargetDeviceAvailability DetermineMicrophoneHardwareAvailability(
        int deviceCount,
        AudioEndpointAvailability endpointAvailability)
    {
        if (deviceCount > 0 || endpointAvailability == AudioEndpointAvailability.Available)
            return TargetDeviceAvailability.Available;

        if (endpointAvailability == AudioEndpointAvailability.NotDetected)
            return TargetDeviceAvailability.NotDetected;

        return TargetDeviceAvailability.Unknown;
    }

    internal static TargetDeviceAvailability DetermineMicrophoneAvailability(
        bool endpointQuerySucceeded,
        int endpointCount,
        int deviceCount)
    {
        var endpointAvailability = DetermineAudioEndpointAvailability(endpointQuerySucceeded, endpointCount);
        return DetermineMicrophoneHardwareAvailability(deviceCount, endpointAvailability);
    }

    internal static string? DetermineMicrophoneStatusMessage(
        TargetDeviceAvailability availability,
        AudioEndpointAvailability endpointAvailability,
        StandardProtectionState standardState,
        SecureProtectionState secureState,
        AudioHardwareMuteSummary? hardwareMuteSummary = null)
    {
        if (availability == TargetDeviceAvailability.NotDetected &&
            endpointAvailability == AudioEndpointAvailability.NotDetected)
            return "No input microphones detected.";

        if (availability == TargetDeviceAvailability.Available &&
            endpointAvailability == AudioEndpointAvailability.NotDetected)
            return "Microphone detected, but no active capture endpoint available.";

        if (endpointAvailability == AudioEndpointAvailability.Unknown ||
            availability == TargetDeviceAvailability.Unknown ||
            standardState == StandardProtectionState.Unknown ||
            secureState == SecureProtectionState.Unknown)
            return "Windows microphone protection state is partial or could not be fully verified.";

        if (secureState != SecureProtectionState.Active &&
            endpointAvailability == AudioEndpointAvailability.Available &&
            hardwareMuteSummary is { TotalCount: > 0 })
        {
            if (hardwareMuteSummary.UnknownCount > 0)
            {
                return "Windows microphone hardware mute support could not be verified for all capture endpoints; exclusive-mode protection is not verified.";
            }

            if (hardwareMuteSummary.NotSupportedCount > 0)
            {
                return "Software mute only: one or more capture endpoints do not support hardware mute (exclusive-mode streams may bypass software mute).";
            }
        }

        return null;
    }

    internal static string? DetermineMicrophoneStatusMessage(
        TargetDeviceAvailability availability,
        StandardProtectionState standardState,
        SecureProtectionState secureState) =>
        DetermineMicrophoneStatusMessage(
            availability,
            availability == TargetDeviceAvailability.NotDetected
                ? AudioEndpointAvailability.NotDetected
                : AudioEndpointAvailability.Available,
            standardState,
            secureState);

    private static StandardProtectionState ToStandardState(BlockStatus status) => status switch
    {
        BlockStatus.Blocked => StandardProtectionState.Active,
        BlockStatus.Allowed => StandardProtectionState.Inactive,
        _ => StandardProtectionState.Unknown
    };

    private static bool IsCleanOrExplicitlyDisabled(DeviceInfo device) =>
        device.IsStateKnown &&
        device.ConfigurationProblemCode is 0 or CfgMgrInterop.CM_PROB_DISABLED;

    internal static BlockStatus EvaluateMicrophoneStandardStatus(
        BlockStatus consentStatus,
        bool endpointQuerySucceeded,
        IReadOnlyList<WindowsAudioEndpointMuteState> endpoints)
    {
        if (consentStatus == BlockStatus.Unknown || !endpointQuerySucceeded || endpoints.Count == 0)
            return BlockStatus.Unknown;

        var allMuted = endpoints.All(endpoint => endpoint.IsMuted);
        var allUnmuted = endpoints.All(endpoint => !endpoint.IsMuted);
        if (consentStatus == BlockStatus.Blocked && allMuted)
            return BlockStatus.Blocked;
        if (consentStatus == BlockStatus.Allowed && allUnmuted)
            return BlockStatus.Allowed;
        return BlockStatus.Unknown;
    }

    internal static BlockStatus EvaluateStoreMicrophoneStandardStatus(
        bool endpointQuerySucceeded,
        IReadOnlyList<WindowsAudioEndpointMuteState> endpoints)
    {
        if (!endpointQuerySucceeded || endpoints.Count == 0)
            return BlockStatus.Unknown;

        if (endpoints.All(endpoint => endpoint.IsMuted))
            return BlockStatus.Blocked;
        if (endpoints.All(endpoint => !endpoint.IsMuted))
            return BlockStatus.Allowed;
        return BlockStatus.Unknown;
    }

    internal static (bool IsActive, bool IsUnknown) EvaluateSecureState(
        BlockStatus policyStatus,
        int detectedDeviceCount,
        bool deviceStateKnown,
        bool allDevicesDisabled,
        bool allDevicesEnabled)
    {
        if (policyStatus == BlockStatus.Unknown || !deviceStateKnown || detectedDeviceCount == 0)
            return (false, true);

        if (policyStatus == BlockStatus.Blocked && allDevicesDisabled)
            return (true, false);

        if (policyStatus == BlockStatus.Allowed && allDevicesEnabled)
            return (false, false);

        // One mechanism being active is only a partial block. Never expose it as verified Secure.
        return (false, true);
    }

    private static SecureProtectionState ToSecureState(
        bool isActive,
        bool isUnknown,
        StandardProtectionState standardState)
    {
        if (isActive)
            return SecureProtectionState.Active;
        if (isUnknown)
            return SecureProtectionState.Unknown;
        return standardState == StandardProtectionState.Active
            ? SecureProtectionState.Available
            : SecureProtectionState.Unavailable;
    }

    private OperationResult CompareAndMuteEndpoint(OriginalAudioEndpointState endpoint)
    {
        var current = _coreAudioController.GetMicrophoneMuteState(endpoint.EndpointId);
        if (!current.QuerySucceeded)
            return OperationResult.Fail(current.ErrorMessage ?? "Could not inspect the audio capture endpoint.");
        if (!current.IsPresent)
            return OperationResult.Fail("Audio capture endpoint disappeared after its snapshot was committed.");
        if (current.IsMuted == endpoint.ProtectedMutedState)
        {
            return endpoint.JournalState == PrivacyResourceJournalState.Applied &&
                   endpoint.ModifiedByPrivLock &&
                   !endpoint.OwnershipUncertain
                ? OperationResult.Ok()
                : OperationResult.Fail(
                    "Audio apply conflict: endpoint reached the protected state after snapshot and was preserved as externally owned.");
        }
        if (current.IsMuted != endpoint.OriginalMutedState)
            return OperationResult.Fail("Audio apply conflict: endpoint state changed after snapshot.");

        return _coreAudioController.SetMicrophoneMute(endpoint.EndpointId, endpoint.ProtectedMutedState);
    }

    private async Task<OperationResult> ExecuteSecureResourceAsync<T>(string command, T resource)
    {
        var payload = WindowsPrivacySessionPlatformAdapter.EncodePayload(resource);
        return await WindowsPrivilegedExecutor.InvokeOnDemandElevationAsync(command, payload);
    }

    private static DeviceOperationDetail ToResourceDetail(
        PrivacyResourceState resource,
        OperationResult result,
        string friendlyName) =>
        new()
        {
            DeviceId = resource.ResourceId,
            FriendlyName = friendlyName,
            Success = result.Success,
            OutcomeUncertain = result.OutcomeUncertain,
            ExecutionStillInFlight = result.ExecutionStillInFlight,
            ErrorMessage = result.ErrorMessage
        };

    private static OperationResult FailForResource(
        OperationResult result,
        IReadOnlyList<DeviceOperationDetail> details) =>
        OperationResult.Fail(
            result.ErrorMessage ?? "A Windows privacy resource could not be protected.",
            details,
            result.OutcomeUncertain,
            result.ExecutionStillInFlight);

    private PrivacySession LoadTrackedSession()
    {
        var session = _privacySessionStore.Load()
            ?? throw new InvalidOperationException("No durable privacy session exists for this Windows mutation.");
        if (!session.IsActive || string.IsNullOrWhiteSpace(session.LastOperationId))
            throw new InvalidOperationException("The privacy session is not active or has no prepared operation.");
        return session;
    }

    private static IReadOnlyList<T> GetTrackedResources<T>(
        PrivacySession session,
        ProtectionLayer layer,
        BlockTarget target)
        where T : PrivacyResourceState
    {
        return session.Resources
            .OfType<T>()
            .Where(resource => resource.Layer == layer)
            .Where(resource => target == BlockTarget.Both || resource.Target == target)
            .Where(resource => string.Equals(resource.OperationId, session.LastOperationId, StringComparison.Ordinal))
            .ToList();
    }
}

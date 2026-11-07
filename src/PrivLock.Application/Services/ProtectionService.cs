using System.Diagnostics;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Serilog;
using Serilog.Context;

namespace PrivLock.Application.Services;

/// <summary>
/// Single serialization point for block, unblock, recovery, and shutdown restoration.
/// Every supported Windows mutation is prepared in a durable journal before platform dispatch.
/// </summary>
public sealed class ProtectionService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ProtectionService>();

    private readonly IDeviceProtectionProvider _protectionProvider;
    private readonly IDeviceDetector _deviceDetector;
    private readonly IPlatformCapabilityProvider _capabilityProvider;
    private readonly IStateStore _stateStore;
    private readonly PrivacySessionService _privacySessions;
    private readonly ILegacyArtifactDetector _legacyDetector;
    private readonly IEditionProfileResolver? _profileResolver;
    private readonly IPrivacySettingsLauncher _privacySettingsLauncher;
    private readonly bool _allowUntrackedMutations;
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private int _shutdownStarted;

    public event Action<FullProtectionState>? StateChanged;

    public PlatformCapabilities Capabilities => _capabilityProvider.Capabilities;
    public PlatformInfo PlatformInfo => _capabilityProvider.PlatformInfo;
    public EditionProfile EditionProfile => _profileResolver?.Profile ?? EditionProfile.Standalone;
    public bool IsStoreEdition => EditionProfile == EditionProfile.Store;
    public bool IsUntrustedOrIndeterminate => EditionProfile == EditionProfile.UntrustedOrIndeterminate;
    public bool IsShutdownStarted => Volatile.Read(ref _shutdownStarted) != 0;
    internal bool HasPendingDesiredStateCleanup => false;
    public bool IsAdvancedProtectionEnabled => !IsStoreEdition && !IsUntrustedOrIndeterminate && _stateStore.Load().AdvancedProtectionEnabled;

    public ProtectionService(
        IDeviceProtectionProvider protectionProvider,
        IDeviceDetector deviceDetector,
        IPlatformCapabilityProvider capabilityProvider,
        IStateStore stateStore,
        PrivacySessionService privacySessions,
        IEditionProfileResolver? profileResolver = null,
        IPrivacySettingsLauncher? privacySettingsLauncher = null)
        : this(
            protectionProvider,
            deviceDetector,
            capabilityProvider,
            stateStore,
            privacySessions,
            legacyDetector: null,
            allowUntrackedMutations: false,
            profileResolver: profileResolver,
            privacySettingsLauncher: privacySettingsLauncher)
    {
    }

    public ProtectionService(
        IDeviceProtectionProvider protectionProvider,
        IDeviceDetector deviceDetector,
        IPlatformCapabilityProvider capabilityProvider,
        IStateStore stateStore,
        PrivacySessionService privacySessions,
        ILegacyArtifactDetector? legacyDetector,
        IEditionProfileResolver? profileResolver = null,
        IPrivacySettingsLauncher? privacySettingsLauncher = null)
        : this(
            protectionProvider,
            deviceDetector,
            capabilityProvider,
            stateStore,
            privacySessions,
            legacyDetector,
            allowUntrackedMutations: false,
            profileResolver: profileResolver,
            privacySettingsLauncher: privacySettingsLauncher)
    {
    }

    internal ProtectionService(
        IDeviceProtectionProvider protectionProvider,
        IDeviceDetector deviceDetector,
        IPlatformCapabilityProvider capabilityProvider,
        IStateStore stateStore,
        PrivacySessionService privacySessions,
        ILegacyArtifactDetector? legacyDetector,
        bool allowUntrackedMutations,
        IEditionProfileResolver? profileResolver = null,
        IPrivacySettingsLauncher? privacySettingsLauncher = null)
    {
        _protectionProvider = protectionProvider;
        _deviceDetector = deviceDetector;
        _capabilityProvider = capabilityProvider;
        _stateStore = stateStore;
        _privacySessions = privacySessions;
        _legacyDetector = legacyDetector ?? new PrivLock.Infrastructure.Common.Storage.DefaultLegacyArtifactDetector();
        _allowUntrackedMutations = allowUntrackedMutations;
        _profileResolver = profileResolver;
        _privacySettingsLauncher = privacySettingsLauncher ?? new PrivLock.Infrastructure.Common.Packaging.NoOpPrivacySettingsLauncher();
    }

    /// <summary>
    /// Compatibility constructor for platform-agnostic callers/tests that do not opt into an exact
    /// platform recovery adapter. Production DI uses the constructor with PrivacySessionService.
    /// </summary>
    public ProtectionService(
        IDeviceProtectionProvider protectionProvider,
        IDeviceDetector deviceDetector,
        IPlatformCapabilityProvider capabilityProvider,
        IStateStore stateStore,
        IEditionProfileResolver? profileResolver = null,
        IPrivacySettingsLauncher? privacySettingsLauncher = null)
        : this(
            protectionProvider,
            deviceDetector,
            capabilityProvider,
            stateStore,
            new PrivacySessionService(
                new VolatilePrivacySessionStore(),
                new UnsupportedPrivacySessionPlatformAdapter()),
            legacyDetector: null,
            allowUntrackedMutations: true,
            profileResolver: profileResolver,
            privacySettingsLauncher: privacySettingsLauncher)
    {
    }

    /// <summary>
    /// Opens the operating-system privacy settings page for the specified target (e.g. ms-settings:privacy-webcam)
    /// without mutating DesiredState or the recovery journal, then refreshes observed OS state.
    /// Opening Settings is never treated as proof that a permission changed.
    /// </summary>
    public async Task<OperationResult> OpenPrivacySettingsAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Opening Windows Settings is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        var launchResult = await _privacySettingsLauncher.OpenPrivacySettingsAsync(target, cancellationToken);
        await PublishVerifiedStateAsync(cancellationToken);

        return launchResult;
    }

    private static bool CanApplyStandardProtection(TargetProtectionStatus status)
    {
        if (status.StandardState == StandardProtectionState.Unknown)
            return false;

        if (status.StandardIsVerified)
            return true;

        // Software-only mute usability: when WASAPI capture endpoints are actively present and
        // their mute state is known, allow toggling software mute even if HardwareMuteSupport is
        // NotSupported or Unknown (StandardIsVerified remains false so Exclusive Mode protection is never claimed).
        return status.Target == BlockTarget.Microphone &&
               status.EndpointAvailability == AudioEndpointAvailability.Available &&
               status.HardwareMuteSummary is { TotalCount: > 0 };
    }

    public async Task<OperationResult> EnableStandardProtectionAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Standard Protection is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsStoreEdition && target is BlockTarget.Camera or BlockTarget.Both)
        {
            return OperationResult.Fail(
                "Direct camera ConsentStore modification is not supported in the Microsoft Store edition. Use Windows Settings (ms-settings:privacy-webcam) instead.");
        }

        var operationId = CreateOperationId("StdEn");
        using var context = LogContext.PushProperty("OperationId", operationId);
        return await RunSerializedUserOperationAsync(async () =>
        {
            var current = await _protectionProvider.GetProtectionStateAsync(cancellationToken);

            // Fail-closed verification guards: reject new Standard mutations if target state is unverified or unknown
            bool camUnverified = !CanApplyStandardProtection(current.Camera);
            bool micUnverified = !CanApplyStandardProtection(current.Microphone);

            if (target is BlockTarget.Camera or BlockTarget.Both && camUnverified)
            {
                return OperationResult.Fail(
                    "Standard protection for Camera cannot be enabled because its current state is unverified or unknown.");
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both && micUnverified)
            {
                return OperationResult.Fail(
                    "Standard protection for Microphone cannot be enabled because its current state is unverified or unknown.");
            }

            bool camNeedsApply = target is BlockTarget.Camera or BlockTarget.Both &&
                                 current.Camera.StandardState == StandardProtectionState.Inactive;
            bool micNeedsApply = target is BlockTarget.Microphone or BlockTarget.Both &&
                                 current.Microphone.StandardState == StandardProtectionState.Inactive;

            BlockTarget? effectiveTarget = (camNeedsApply, micNeedsApply) switch
            {
                (true, true) => BlockTarget.Both,
                (true, false) => BlockTarget.Camera,
                (false, true) => BlockTarget.Microphone,
                (false, false) => null
            };

            // Idempotent: already Active (when no persistent journal is used to validate ownership)
            if (effectiveTarget == null && !_privacySessions.SupportsPersistentRecovery)
            {
                UpdateDesiredAfterStandardEnable(target);
                await PublishVerifiedStateAsync(cancellationToken);
                return OperationResult.Ok();
            }

            Func<CancellationToken, Task<OperationResult>> applyFunc = effectiveTarget.HasValue
                ? ct => _protectionProvider.EnableStandardProtectionAsync(effectiveTarget.Value, ct)
                : _ => Task.FromResult(OperationResult.Ok());

            PrivacyBlockPreparation? preparation = null;
            var result = await PrepareApplyAndRecordAsync(
                ProtectionLayer.Standard,
                target,
                operationId,
                applyFunc,
                prepared => preparation = prepared,
                cancellationToken);
            if (!result.Success)
                return result;

            try
            {
                UpdateDesiredAfterStandardEnable(target);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Desired-state persistence failed after Standard protection; rolling back owned changes");
                var rollback = await RestorePreparedDeltaOrScopeAsync(
                    preparation,
                    ProtectionLayer.Standard,
                    target,
                    "DesiredStateSaveFailure",
                    CancellationToken.None);
                return OperationResult.Fail(
                    rollback.SafeToExit
                        ? $"Protection was rolled back because application state could not be saved: {ex.Message}"
                        : $"Application state could not be saved and rollback remains incomplete: {rollback.ErrorMessage}");
            }
            await PublishVerifiedStateAsync(cancellationToken);
            return OperationResult.Ok(result.Details);
        }, operationId, cancellationToken);
    }

    public async Task<OperationResult> DisableStandardProtectionAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Standard Protection teardown is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsStoreEdition && target is BlockTarget.Camera or BlockTarget.Both)
        {
            return OperationResult.Fail(
                "Direct camera ConsentStore modification is not supported in the Microsoft Store edition. Use Windows Settings (ms-settings:privacy-webcam) instead.");
        }

        var operationId = CreateOperationId("StdDis");
        using var context = LogContext.PushProperty("OperationId", operationId);
        return await RunSerializedUserOperationAsync(async () =>
        {
            // Standard teardown also owns Secure changes in Standalone. In Store edition, Secure
            // protection does not exist and must never be invoked.
            var secureResult = PrivacyRecoveryResult.NothingToRestore();
            if (!IsStoreEdition &&
                (_privacySessions.SupportsPersistentRecovery ||
                 await IsSecureActiveAsync(target, cancellationToken)))
            {
                secureResult = await RestoreScopeAsync(
                    ProtectionLayer.Secure,
                    target,
                    "UserDisableStandard-SecureFirst",
                    RestoreScopePolicy.OpportunisticPreTeardown,
                    cancellationToken);
            }
            var standardResult = await RestoreScopeAsync(
                ProtectionLayer.Standard,
                target,
                "UserDisableStandard",
                RestoreScopePolicy.Strict,
                cancellationToken);

            var combined = CombineRecoveryResults(secureResult, standardResult);
            if (combined.SafeToExit && combined.ConflictCount == 0 &&
                (combined.TrackingSupported || _allowUntrackedMutations))
            {
                UpdateDesiredAfterStandardDisable(target);
                await PublishVerifiedStateAsync(cancellationToken);
            }

            return ToOperationResult(combined);
        }, operationId, cancellationToken);
    }

    public async Task<OperationResult> EnableSecureProtectionAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Secure Protection is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsStoreEdition || (_profileResolver != null && !_profileResolver.SupportsPrivilegedOperations))
        {
            return OperationResult.Fail(
                "Secure Protection (PnP + HKLM Group Policy) is not supported in the Microsoft Store edition.");
        }

        var operationId = CreateOperationId("SecEn");
        using var context = LogContext.PushProperty("OperationId", operationId);
        return await RunSerializedUserOperationAsync(async () =>
        {
            var current = await _protectionProvider.GetProtectionStateAsync(cancellationToken);

            (bool canEnable, bool isAlreadyActive, string? error) EvaluateSecurePreconditions(
                TargetProtectionStatus status, string targetName)
            {
                if (status.StandardState != StandardProtectionState.Active)
                {
                    return (false, false,
                        $"You must enable Standard Protection before enabling Secure Protection for {targetName}.");
                }

                if (!CanApplyStandardProtection(status))
                {
                    return (false, false,
                        $"Secure protection for {targetName} cannot be enabled because Standard protection is unverified.");
                }

                if (!status.SecureIsVerified || status.SecureState == SecureProtectionState.Unknown)
                {
                    return (false, false,
                        $"Secure protection for {targetName} cannot be enabled because its current state is unverified or unknown.");
                }

                if (status.SecureState == SecureProtectionState.Unavailable)
                {
                    return (false, false,
                        $"Secure protection for {targetName} is unavailable.");
                }

                if (status.SecureState == SecureProtectionState.Active)
                {
                    return (false, true, null);
                }

                if (status.SecureState is SecureProtectionState.Available or SecureProtectionState.Failed)
                {
                    return (true, false, null);
                }

                return (false, false, $"Secure protection for {targetName} is in an invalid state: {status.SecureState}.");
            }

            BlockTarget? effectiveTarget;

            if (target == BlockTarget.Camera)
            {
                var (camCanEnable, camAlreadyActive, camError) = EvaluateSecurePreconditions(current.Camera, "Camera");
                if (camError != null)
                    return OperationResult.Fail(camError);

                if (camAlreadyActive)
                {
                    if (!_privacySessions.SupportsPersistentRecovery)
                    {
                        UpdateDesiredAfterSecureEnable(BlockTarget.Camera);
                        await PublishVerifiedStateAsync(cancellationToken);
                        return OperationResult.Ok();
                    }
                    effectiveTarget = null;
                }
                else
                {
                    effectiveTarget = BlockTarget.Camera;
                }
            }
            else if (target == BlockTarget.Microphone)
            {
                var (micCanEnable, micAlreadyActive, micError) = EvaluateSecurePreconditions(current.Microphone, "Microphone");
                if (micError != null)
                    return OperationResult.Fail(micError);

                if (micAlreadyActive)
                {
                    if (!_privacySessions.SupportsPersistentRecovery)
                    {
                        UpdateDesiredAfterSecureEnable(BlockTarget.Microphone);
                        await PublishVerifiedStateAsync(cancellationToken);
                        return OperationResult.Ok();
                    }
                    effectiveTarget = null;
                }
                else
                {
                    effectiveTarget = BlockTarget.Microphone;
                }
            }
            else // BlockTarget.Both
            {
                var (camCanEnable, camAlreadyActive, camError) = EvaluateSecurePreconditions(current.Camera, "Camera");
                if (camError != null)
                    return OperationResult.Fail(camError);

                var (micCanEnable, micAlreadyActive, micError) = EvaluateSecurePreconditions(current.Microphone, "Microphone");
                if (micError != null)
                    return OperationResult.Fail(micError);

                if (camAlreadyActive && micAlreadyActive)
                {
                    if (!_privacySessions.SupportsPersistentRecovery)
                    {
                        UpdateDesiredAfterSecureEnable(BlockTarget.Both);
                        await PublishVerifiedStateAsync(cancellationToken);
                        return OperationResult.Ok();
                    }
                    effectiveTarget = null;
                }
                else if (camCanEnable && micCanEnable)
                {
                    effectiveTarget = BlockTarget.Both;
                }
                else if (camCanEnable && micAlreadyActive)
                {
                    effectiveTarget = BlockTarget.Camera;
                }
                else if (micCanEnable && camAlreadyActive)
                {
                    effectiveTarget = BlockTarget.Microphone;
                }
                else
                {
                    return OperationResult.Fail("Neither target is eligible for Secure protection.");
                }
            }

            Func<CancellationToken, Task<OperationResult>> applyFunc = effectiveTarget.HasValue
                ? ct => _protectionProvider.EnableSecureProtectionAsync(effectiveTarget.Value, ct)
                : _ => Task.FromResult(OperationResult.Ok());

            PrivacyBlockPreparation? preparation = null;
            var result = await PrepareApplyAndRecordAsync(
                ProtectionLayer.Secure,
                target,
                operationId,
                applyFunc,
                prepared => preparation = prepared,
                cancellationToken);
            if (!result.Success)
                return result;

            try
            {
                UpdateDesiredAfterSecureEnable(target);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Desired-state persistence failed after Secure protection; rolling back owned changes");
                var rollback = await RestorePreparedDeltaOrScopeAsync(
                    preparation,
                    ProtectionLayer.Secure,
                    target,
                    "DesiredStateSaveFailure",
                    CancellationToken.None);
                return OperationResult.Fail(
                    rollback.SafeToExit
                        ? $"Secure protection was rolled back because application state could not be saved: {ex.Message}"
                        : $"Application state could not be saved and rollback remains incomplete: {rollback.ErrorMessage}");
            }
            await PublishVerifiedStateAsync(cancellationToken);
            return OperationResult.Ok(result.Details);
        }, operationId, cancellationToken);
    }

    public async Task<OperationResult> DisableSecureProtectionAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Secure Protection teardown is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsStoreEdition || (_profileResolver != null && !_profileResolver.SupportsPrivilegedOperations))
        {
            return OperationResult.Fail(
                "Secure Protection (PnP + HKLM Group Policy) is not supported in the Microsoft Store edition.");
        }

        var operationId = CreateOperationId("SecDis");
        using var context = LogContext.PushProperty("OperationId", operationId);
        return await RunSerializedUserOperationAsync(async () =>
        {
            var recovery = await RestoreScopeAsync(
                ProtectionLayer.Secure,
                target,
                "UserDisableSecure",
                RestoreScopePolicy.Strict,
                cancellationToken);
            if (recovery.SafeToExit && recovery.ConflictCount == 0 &&
                (recovery.TrackingSupported || _allowUntrackedMutations))
            {
                var desired = _stateStore.Load();
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    desired.CameraSecure = desired.CameraStandard == StandardProtectionState.Active
                        ? SecureProtectionState.Available
                        : SecureProtectionState.Unavailable;
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    desired.MicrophoneSecure = desired.MicrophoneStandard == StandardProtectionState.Active
                        ? SecureProtectionState.Available
                        : SecureProtectionState.Unavailable;
                _stateStore.Save(desired);
                await PublishVerifiedStateAsync(cancellationToken);
            }
            return ToOperationResult(recovery);
        }, operationId, cancellationToken);
    }

    /// <summary>
    /// Blocks the specified target. Applies standard protection, and if Advanced Protection is enabled globally,
    /// immediately reinforces the target with secure/hardware-level protection only if eligible and verified.
    /// If the secure layer cannot be verified, standard protection is preserved and secure reinforcement is deferred.
    /// </summary>
    public async Task<OperationResult> BlockDeviceAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        var stdResult = await EnableStandardProtectionAsync(target, cancellationToken);
        if (!stdResult.Success)
            return stdResult;

        if (IsAdvancedProtectionEnabled)
        {
            var currentState = await _protectionProvider.GetProtectionStateAsync(cancellationToken);

            bool CanSecure(TargetProtectionStatus status) =>
                status.StandardState == StandardProtectionState.Active &&
                CanApplyStandardProtection(status) &&
                status.SecureIsVerified &&
                status.SecureState is SecureProtectionState.Available or SecureProtectionState.Failed;

            bool IsSecureUnverified(TargetProtectionStatus status) =>
                !status.SecureIsVerified || status.SecureState == SecureProtectionState.Unknown;

            var details = new List<DeviceOperationDetail>(stdResult.Details);

            if (target == BlockTarget.Camera)
            {
                if (CanSecure(currentState.Camera))
                {
                    var advResult = await EnableSecureProtectionAsync(BlockTarget.Camera, cancellationToken);
                    if (!advResult.Success)
                        return advResult;
                    if (advResult.Details != null)
                        details.AddRange(advResult.Details);
                }
                else if (IsSecureUnverified(currentState.Camera))
                {
                    Log.Warning("BlockDeviceAsync: Camera Secure reinforcement deferred because state is unverified or unknown");
                    details.Add(new DeviceOperationDetail
                    {
                        DeviceId = "Secure-Camera",
                        FriendlyName = "Camera Secure Protection",
                        Success = false,
                        OutcomeUncertain = false,
                        ExecutionStillInFlight = false,
                        ErrorMessage = "Secure protection was deferred because Camera secure state is unverified or unknown."
                    });
                }
            }
            else if (target == BlockTarget.Microphone)
            {
                if (CanSecure(currentState.Microphone))
                {
                    var advResult = await EnableSecureProtectionAsync(BlockTarget.Microphone, cancellationToken);
                    if (!advResult.Success)
                        return advResult;
                    if (advResult.Details != null)
                        details.AddRange(advResult.Details);
                }
                else if (IsSecureUnverified(currentState.Microphone))
                {
                    Log.Warning("BlockDeviceAsync: Microphone Secure reinforcement deferred because state is unverified or unknown");
                    details.Add(new DeviceOperationDetail
                    {
                        DeviceId = "Secure-Microphone",
                        FriendlyName = "Microphone Secure Protection",
                        Success = false,
                        OutcomeUncertain = false,
                        ExecutionStillInFlight = false,
                        ErrorMessage = "Secure protection was deferred because Microphone secure state is unverified or unknown."
                    });
                }
            }
            else if (target == BlockTarget.Both)
            {
                var camEligible = CanSecure(currentState.Camera);
                var micEligible = CanSecure(currentState.Microphone);
                var camUnk = IsSecureUnverified(currentState.Camera);
                var micUnk = IsSecureUnverified(currentState.Microphone);

                if (camEligible && micEligible)
                {
                    var advResult = await EnableSecureProtectionAsync(BlockTarget.Both, cancellationToken);
                    if (!advResult.Success)
                        return advResult;
                    if (advResult.Details != null)
                        details.AddRange(advResult.Details);
                }
                else
                {
                    if (camEligible)
                    {
                        var advResult = await EnableSecureProtectionAsync(BlockTarget.Camera, cancellationToken);
                        if (!advResult.Success)
                            return advResult;
                        if (advResult.Details != null)
                            details.AddRange(advResult.Details);
                    }
                    else if (camUnk)
                    {
                        Log.Warning("BlockDeviceAsync: Camera Secure reinforcement deferred because state is unverified or unknown");
                        details.Add(new DeviceOperationDetail
                        {
                            DeviceId = "Secure-Camera",
                            FriendlyName = "Camera Secure Protection",
                            Success = false,
                            OutcomeUncertain = false,
                            ExecutionStillInFlight = false,
                            ErrorMessage = "Secure protection was deferred because Camera secure state is unverified or unknown."
                        });
                    }

                    if (micEligible)
                    {
                        var advResult = await EnableSecureProtectionAsync(BlockTarget.Microphone, cancellationToken);
                        if (!advResult.Success)
                            return advResult;
                        if (advResult.Details != null)
                            details.AddRange(advResult.Details);
                    }
                    else if (micUnk)
                    {
                        Log.Warning("BlockDeviceAsync: Microphone Secure reinforcement deferred because state is unverified or unknown");
                        details.Add(new DeviceOperationDetail
                        {
                            DeviceId = "Secure-Microphone",
                            FriendlyName = "Microphone Secure Protection",
                            Success = false,
                            OutcomeUncertain = false,
                            ExecutionStillInFlight = false,
                            ErrorMessage = "Secure protection was deferred because Microphone secure state is unverified or unknown."
                        });
                    }
                }
            }

            return OperationResult.Ok(details);
        }

        return stdResult;
    }

    /// <summary>
    /// Unblocks the specified target, tearing down both secure and standard layers for that target,
    /// while leaving global Advanced Protection and other targets unaffected.
    /// </summary>
    public Task<OperationResult> UnblockDeviceAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default) =>
        DisableStandardProtectionAsync(target, cancellationToken);

    /// <summary>
    /// Toggles global Advanced Protection without altering whether devices are blocked or unblocked.
    /// If enabled: reinforces currently blocked devices with the advanced layer only if verified and eligible.
    /// If disabled: strips the advanced layer from protected devices without unblocking them.
    /// Idempotent: repeated calls produce consistent state without redundant mutations.
    /// </summary>
    public async Task<OperationResult> SetAdvancedProtectionAsync(
        bool enable,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Advanced Protection is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsStoreEdition || (_profileResolver != null && !_profileResolver.SupportsPrivilegedOperations))
        {
            return OperationResult.Fail(
                "Advanced Protection (PnP + HKLM Group Policy) is not supported in the Microsoft Store edition.");
        }

        if (enable)
        {
            var currentState = await _protectionProvider.GetProtectionStateAsync(cancellationToken);

            bool CanSecure(TargetProtectionStatus status) =>
                status.StandardState == StandardProtectionState.Active &&
                CanApplyStandardProtection(status) &&
                status.SecureIsVerified &&
                status.SecureState is SecureProtectionState.Available or SecureProtectionState.Failed;

            var cameraCanSecure = CanSecure(currentState.Camera);
            var micCanSecure = CanSecure(currentState.Microphone);

            BlockTarget? targetToSecure = (cameraCanSecure, micCanSecure) switch
            {
                (true, true) => BlockTarget.Both,
                (true, false) => BlockTarget.Camera,
                (false, true) => BlockTarget.Microphone,
                (false, false) => null
            };

            var desired = _stateStore.Load();
            desired.AdvancedProtectionEnabled = true;
            _stateStore.Save(desired);

            var details = new List<DeviceOperationDetail>();

            if (targetToSecure.HasValue)
            {
                var result = await EnableSecureProtectionAsync(targetToSecure.Value, cancellationToken);
                if (!result.Success)
                {
                    var reverted = _stateStore.Load();
                    reverted.AdvancedProtectionEnabled = false;
                    _stateStore.Save(reverted);
                    await PublishVerifiedStateAsync(cancellationToken);
                    return result;
                }
                if (result.Details != null)
                {
                    details.AddRange(result.Details);
                }
            }

            if (currentState.Camera.StandardState == StandardProtectionState.Active &&
                (!currentState.Camera.SecureIsVerified || currentState.Camera.SecureState == SecureProtectionState.Unknown))
            {
                Log.Warning("SetAdvancedProtectionAsync: Camera Secure reinforcement deferred because state is unverified or unknown");
                details.Add(new DeviceOperationDetail
                {
                    DeviceId = "Secure-Camera",
                    FriendlyName = "Camera Secure Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Secure protection was deferred because Camera secure state is unverified or unknown."
                });
            }

            if (currentState.Microphone.StandardState == StandardProtectionState.Active &&
                (!currentState.Microphone.SecureIsVerified || currentState.Microphone.SecureState == SecureProtectionState.Unknown))
            {
                Log.Warning("SetAdvancedProtectionAsync: Microphone Secure reinforcement deferred because state is unverified or unknown");
                details.Add(new DeviceOperationDetail
                {
                    DeviceId = "Secure-Microphone",
                    FriendlyName = "Microphone Secure Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Secure protection was deferred because Microphone secure state is unverified or unknown."
                });
            }

            await PublishVerifiedStateAsync(cancellationToken);
            return OperationResult.Ok(details);
        }
        else
        {
            var currentState = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
            var cameraHasSecure = currentState.Camera.SecureState == SecureProtectionState.Active;
            var micHasSecure = currentState.Microphone.SecureState == SecureProtectionState.Active;

            BlockTarget? targetToDisable = _privacySessions.SupportsPersistentRecovery
                ? BlockTarget.Both
                : (cameraHasSecure, micHasSecure) switch
                {
                    (true, true) => BlockTarget.Both,
                    (true, false) => BlockTarget.Camera,
                    (false, true) => BlockTarget.Microphone,
                    (false, false) => null
                };

            if (targetToDisable.HasValue)
            {
                var result = await DisableSecureProtectionAsync(targetToDisable.Value, cancellationToken);
                if (!result.Success)
                {
                    await PublishVerifiedStateAsync(cancellationToken);
                    return result;
                }
            }

            var desired = _stateStore.Load();
            desired.AdvancedProtectionEnabled = false;
            _stateStore.Save(desired);

            await PublishVerifiedStateAsync(cancellationToken);
            return OperationResult.Ok();
        }
    }

    /// <summary>
    /// Begins a deferred/lazy platform elevation scope so multiple sequential operations
    /// share at most 1 elevated worker process (at most 1 UAC prompt).
    /// </summary>
    public IDisposable BeginDeferredElevationScope() =>
        _privacySessions.BeginDeferredPlatformScope();

    /// <summary>
    /// Startup recovery runs before the ViewModel/UI exists and before any new mutation is allowed.
    /// </summary>
    public async Task<PrivacyRecoveryResult> RecoverPreviousSessionAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return new PrivacyRecoveryResult
            {
                TrackingSupported = false,
                IsComplete = false,
                FailedCount = 1,
                ErrorMessage = "Session recovery is blocked because the runtime edition profile is untrusted or indeterminate."
            };
        }

        await _operationGate.WaitAsync(cancellationToken);
        try
        {
            var result = await _privacySessions.RecoverUnfinishedSessionAsync(cancellationToken);
            if (!_allowUntrackedMutations &&
                result.SafeToExit &&
                !result.HadPersistedSession &&
                _legacyDetector.HasLegacyEvidence())
            {
                return LegacyUntrackedRecoveryResult();
            }
            // DesiredState represents durable user intent across restarts and shutdowns;
            // startup recovery restores any unfinished runtime mutations to a safe baseline
            // without clearing what the user chose to protect.
            return result;
        }
        finally
        {
            _operationGate.Release();
        }
    }

    /// <summary>
    /// Reconciles the persistent DesiredState with the actual operating-system protection state.
    /// Reapplies only the protections that DesiredState requires, without treating previous
    /// PrivacySessions as desired state, never touching inactive scopes, independently reconciling
    /// Camera and Microphone, and establishing a new PrivacySession for newly applied runtime mutations.
    /// Aborts safely without mutating platform state if startup recovery was unsafe or incomplete.
    /// </summary>
    public async Task<OperationResult> ReconcileDesiredProtectionAsync(
        PrivacyRecoveryResult? recoveryResult = null,
        CancellationToken cancellationToken = default)
    {
        if (IsUntrustedOrIndeterminate)
        {
            return OperationResult.Fail(
                "Startup reconciliation is blocked because the runtime edition profile is untrusted or indeterminate.");
        }

        if (IsShutdownStarted)
            return OperationResult.Fail("PrivGvard is shutting down; startup reconciliation was not performed.");

        if (recoveryResult is { SafeToExit: false } or { ConflictCount: > 0 })
        {
            var reason = recoveryResult.ErrorMessage ??
                "A previous privacy session recovery was incomplete or conflicted; startup reconciliation was aborted for safety.";
            Log.Warning("Startup reconciliation aborted because recovery was incomplete: {Reason}", reason);
            return OperationResult.Fail(reason);
        }

        try
        {
            var desired = _stateStore.Load();
            var actual = await _protectionProvider.GetProtectionStateAsync(cancellationToken);

            var unverifiedOrUnknownFailures = new List<string>();
            var unverifiedDetails = new List<DeviceOperationDetail>();

            // Fail-closed reconciliation guards: verify target state explicitly before any mutation.
            // Do NOT convert Unknown into permission to mutate or assume it is Inactive/Available.
            if (!IsStoreEdition &&
                desired.CameraStandard == StandardProtectionState.Active &&
                !CanApplyStandardProtection(actual.Camera))
            {
                unverifiedOrUnknownFailures.Add("Camera standard protection could not be reconciled: state is unverified or unknown.");
                unverifiedDetails.Add(new DeviceOperationDetail
                {
                    DeviceId = "Reconcile-Standard-Camera",
                    FriendlyName = "Camera Standard Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Camera standard protection was omitted during reconciliation because its state is unverified or unknown."
                });
            }

            if (desired.MicrophoneStandard == StandardProtectionState.Active &&
                !CanApplyStandardProtection(actual.Microphone))
            {
                unverifiedOrUnknownFailures.Add("Microphone standard protection could not be reconciled: state is unverified or unknown.");
                unverifiedDetails.Add(new DeviceOperationDetail
                {
                    DeviceId = "Reconcile-Standard-Microphone",
                    FriendlyName = "Microphone Standard Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Microphone standard protection was omitted during reconciliation because its state is unverified or unknown."
                });
            }

            var cameraNeedsStandard = !IsStoreEdition &&
                                      desired.CameraStandard == StandardProtectionState.Active &&
                                      CanApplyStandardProtection(actual.Camera) &&
                                      actual.Camera.StandardState == StandardProtectionState.Inactive;

            var micNeedsStandard = desired.MicrophoneStandard == StandardProtectionState.Active &&
                                   CanApplyStandardProtection(actual.Microphone) &&
                                   actual.Microphone.StandardState == StandardProtectionState.Inactive;

            var cameraWantsSecure = !IsStoreEdition &&
                                    desired.CameraStandard == StandardProtectionState.Active &&
                                    (desired.AdvancedProtectionEnabled || desired.CameraSecure == SecureProtectionState.Active);

            var micWantsSecure = !IsStoreEdition &&
                                 desired.MicrophoneStandard == StandardProtectionState.Active &&
                                 (desired.AdvancedProtectionEnabled || desired.MicrophoneSecure == SecureProtectionState.Active);

            var cameraNeedsSecure = cameraWantsSecure &&
                                    !(actual.Camera.StandardState == StandardProtectionState.Active &&
                                      actual.Camera.SecureState == SecureProtectionState.Active);

            var micNeedsSecure = micWantsSecure &&
                                 !(actual.Microphone.StandardState == StandardProtectionState.Active &&
                                   actual.Microphone.SecureState == SecureProtectionState.Active);

            if (cameraWantsSecure && actual.Camera.StandardState == StandardProtectionState.Active &&
                (!actual.Camera.SecureIsVerified || actual.Camera.SecureState == SecureProtectionState.Unknown))
            {
                cameraNeedsSecure = false;
                unverifiedOrUnknownFailures.Add("Camera secure protection could not be reconciled: state is unverified or unknown.");
                unverifiedDetails.Add(new DeviceOperationDetail
                {
                    DeviceId = "Reconcile-Secure-Camera",
                    FriendlyName = "Camera Secure Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Camera secure protection was omitted during reconciliation because its state is unverified or unknown."
                });
            }

            if (micWantsSecure && actual.Microphone.StandardState == StandardProtectionState.Active &&
                (!actual.Microphone.SecureIsVerified || actual.Microphone.SecureState == SecureProtectionState.Unknown))
            {
                micNeedsSecure = false;
                unverifiedOrUnknownFailures.Add("Microphone secure protection could not be reconciled: state is unverified or unknown.");
                unverifiedDetails.Add(new DeviceOperationDetail
                {
                    DeviceId = "Reconcile-Secure-Microphone",
                    FriendlyName = "Microphone Secure Protection",
                    Success = false,
                    OutcomeUncertain = false,
                    ExecutionStillInFlight = false,
                    ErrorMessage = "Microphone secure protection was omitted during reconciliation because its state is unverified or unknown."
                });
            }

            if (!cameraNeedsStandard && !micNeedsStandard && !cameraNeedsSecure && !micNeedsSecure)
            {
                if (unverifiedOrUnknownFailures.Count > 0)
                {
                    Log.Warning("Startup reconciliation: Desired protections could not be reconciled because target state is unverified or unknown");
                    await PublishVerifiedStateAsync(cancellationToken);
                    return OperationResult.Fail(string.Join("; ", unverifiedOrUnknownFailures), unverifiedDetails);
                }

                Log.Information("Startup reconciliation: OS protection already matches desired state; no mutations required");
                await PublishVerifiedStateAsync(cancellationToken);
                return OperationResult.Ok();
            }

            var failures = new List<string>(unverifiedOrUnknownFailures);
            var details = new List<DeviceOperationDetail>(unverifiedDetails);

            using (_privacySessions.BeginDeferredPlatformScope())
            {
                if (cameraNeedsStandard)
                {
                    Log.Information("Startup reconciliation: Applying Camera standard protection");
                    var camResult = await EnableStandardProtectionAsync(BlockTarget.Camera, cancellationToken);
                    if (!camResult.Success)
                    {
                        failures.Add($"Camera standard protection failed: {camResult.ErrorMessage}");
                    }
                    else if (camResult.Details != null)
                    {
                        details.AddRange(camResult.Details);
                    }
                }

                var cameraStandardActive = cameraNeedsStandard
                    ? !failures.Any(f => f.Contains("Camera standard protection"))
                    : (CanApplyStandardProtection(actual.Camera) && actual.Camera.StandardState == StandardProtectionState.Active);

                var elevationDenied = false;

                if (cameraNeedsSecure && cameraStandardActive)
                {
                    var postCamState = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
                    if (!postCamState.Camera.SecureIsVerified || postCamState.Camera.SecureState == SecureProtectionState.Unknown)
                    {
                        Log.Warning("Startup reconciliation: Camera secure protection skipped because state is unverified or unknown");
                        failures.Add("Camera secure protection skipped: state is unverified or unknown.");
                        details.Add(new DeviceOperationDetail
                        {
                            DeviceId = "Reconcile-Secure-Camera",
                            FriendlyName = "Camera Secure Protection",
                            Success = false,
                            OutcomeUncertain = false,
                            ExecutionStillInFlight = false,
                            ErrorMessage = "Camera secure protection was omitted during reconciliation because its state is unverified or unknown."
                        });
                    }
                    else
                    {
                        Log.Information("Startup reconciliation: Applying Camera secure protection");
                        var camSecResult = await EnableSecureProtectionAsync(BlockTarget.Camera, cancellationToken);
                        if (!camSecResult.Success)
                        {
                            failures.Add($"Camera secure protection failed: {camSecResult.ErrorMessage}");
                            if (IsElevationDenied(camSecResult))
                            {
                                elevationDenied = true;
                            }
                        }
                        else if (camSecResult.Details != null)
                        {
                            details.AddRange(camSecResult.Details);
                        }
                    }
                }

                if (micNeedsStandard)
                {
                    Log.Information("Startup reconciliation: Applying Microphone standard protection");
                    var micResult = await EnableStandardProtectionAsync(BlockTarget.Microphone, cancellationToken);
                    if (!micResult.Success)
                    {
                        failures.Add($"Microphone standard protection failed: {micResult.ErrorMessage}");
                    }
                    else if (micResult.Details != null)
                    {
                        details.AddRange(micResult.Details);
                    }
                }

                var micStandardActive = micNeedsStandard
                    ? !failures.Any(f => f.Contains("Microphone standard protection"))
                    : (CanApplyStandardProtection(actual.Microphone) && actual.Microphone.StandardState == StandardProtectionState.Active);

                if (micNeedsSecure && micStandardActive)
                {
                    if (elevationDenied)
                    {
                        Log.Warning("Startup reconciliation: Skipping Microphone secure protection because elevation was previously denied");
                        failures.Add("Microphone secure protection skipped: Administrator permissions were denied.");
                    }
                    else
                    {
                        var postMicState = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
                        if (!postMicState.Microphone.SecureIsVerified || postMicState.Microphone.SecureState == SecureProtectionState.Unknown)
                        {
                            Log.Warning("Startup reconciliation: Microphone secure protection skipped because state is unverified or unknown");
                            failures.Add("Microphone secure protection skipped: state is unverified or unknown.");
                            details.Add(new DeviceOperationDetail
                            {
                                DeviceId = "Reconcile-Secure-Microphone",
                                FriendlyName = "Microphone Secure Protection",
                                Success = false,
                                OutcomeUncertain = false,
                                ExecutionStillInFlight = false,
                                ErrorMessage = "Microphone secure protection was omitted during reconciliation because its state is unverified or unknown."
                            });
                        }
                        else
                        {
                            Log.Information("Startup reconciliation: Applying Microphone secure protection");
                            var micSecResult = await EnableSecureProtectionAsync(BlockTarget.Microphone, cancellationToken);
                            if (!micSecResult.Success)
                            {
                                failures.Add($"Microphone secure protection failed: {micSecResult.ErrorMessage}");
                            }
                            else if (micSecResult.Details != null)
                            {
                                details.AddRange(micSecResult.Details);
                            }
                        }
                    }
                }
            }

            await PublishVerifiedStateAsync(cancellationToken);

            if (failures.Count > 0)
            {
                return OperationResult.Fail(string.Join("; ", failures), details);
            }

            return OperationResult.Ok(details);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error during startup reconciliation");
            return OperationResult.Fail($"Startup reconciliation encountered an error: {ex.Message}");
        }
    }

    private static bool IsElevationDenied(OperationResult result)
    {
        if (result.Success || string.IsNullOrEmpty(result.ErrorMessage))
            return false;

        return result.ErrorMessage.Contains("Administrator permissions were denied", StringComparison.OrdinalIgnoreCase) ||
               (result.ErrorMessage.Contains("elevation", StringComparison.OrdinalIgnoreCase) &&
                result.ErrorMessage.Contains("denied", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Marks the service as stopping before waiting on the operation gate. This ordering ensures a
    /// queued toggle cannot run after shutdown restoration has completed.
    /// </summary>
    public async Task<PrivacyRecoveryResult> BeginShutdownAndRestoreAsync(
        string reason,
        CancellationToken cancellationToken = default)
    {
        SignalShutdown();
        if (IsUntrustedOrIndeterminate)
        {
            return new PrivacyRecoveryResult
            {
                TrackingSupported = false,
                IsComplete = false,
                FailedCount = 1,
                ErrorMessage = "Shutdown restoration is blocked because the runtime edition profile is untrusted or indeterminate."
            };
        }

        await _operationGate.WaitAsync(cancellationToken);
        try
        {
            PrivacyRecoveryResult result;
            if (_privacySessions.SupportsPersistentRecovery)
            {
                result = await _privacySessions.RestoreAsync(null, null, reason, cancellationToken);
            }
            else
            {
                if (!_allowUntrackedMutations)
                {
                    // Production must never perform a broad "allow/unmute" without an exact
                    // snapshot proving ownership. Unsupported platforms reject mutation earlier.
                    result = PrivacyRecoveryResult.Unsupported();
                }
                else
                {
                    var secure = await _protectionProvider.DisableSecureProtectionAsync(BlockTarget.Both, cancellationToken);
                    var standard = await _protectionProvider.DisableStandardProtectionAsync(BlockTarget.Both, cancellationToken);
                    result = new PrivacyRecoveryResult
                    {
                        TrackingSupported = false,
                        IsComplete = secure.Success && standard.Success,
                        FailedCount = (secure.Success ? 0 : 1) + (standard.Success ? 0 : 1),
                        ErrorMessage = secure.ErrorMessage ?? standard.ErrorMessage
                    };
                }
            }

            if (!_allowUntrackedMutations &&
                result.SafeToExit &&
                !result.HadPersistedSession &&
                _legacyDetector.HasLegacyEvidence())
                result = LegacyUntrackedRecoveryResult();

            // Invariant: Restoring the runtime OS state during shutdown MUST NOT implicitly mean
            // that the user disabled protection. DesiredState survives normal application shutdown
            // and Windows shutdown. Only explicit user disable operations modify DesiredState.
            return result;
        }
        finally
        {
            _operationGate.Release();
        }
    }

    internal void SignalShutdown() => Interlocked.Exchange(ref _shutdownStarted, 1);

    public void AbortShutdown() => Interlocked.Exchange(ref _shutdownStarted, 0);

    public async Task<FullProtectionState> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        var state = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
        return state with { AdvancedProtectionEnabled = IsAdvancedProtectionEnabled };
    }

    public Task<IReadOnlyList<DeviceInfo>> GetDetectedDevicesAsync(CancellationToken cancellationToken = default) =>
        _deviceDetector.DetectAllAsync(cancellationToken);

    public object GetRecoveryDiagnosticSummary() => _privacySessions.GetDiagnosticSummary();

    private async Task<OperationResult> PrepareApplyAndRecordAsync(
        ProtectionLayer layer,
        BlockTarget target,
        string operationId,
        Func<CancellationToken, Task<OperationResult>> apply,
        Action<PrivacyBlockPreparation> capturePreparation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await PrepareApplyAndRecordCoreAsync(
                layer,
                target,
                operationId,
                apply,
                capturePreparation,
                cancellationToken);
        }
        finally
        {
            _privacySessions.CompletePlatformPass();
        }
    }

    private async Task<OperationResult> PrepareApplyAndRecordCoreAsync(
        ProtectionLayer layer,
        BlockTarget target,
        string operationId,
        Func<CancellationToken, Task<OperationResult>> apply,
        Action<PrivacyBlockPreparation> capturePreparation,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        PrivacyBlockPreparation preparation;
        try
        {
            preparation = await _privacySessions.PrepareBlockAsync(layer, target, operationId, cancellationToken);
            capturePreparation(preparation);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Snapshot persistence failed; platform mutation was not dispatched");
            return OperationResult.Fail($"Could not persist a recovery snapshot: {ex.Message}");
        }

        if (!preparation.TrackingEnabled && !_allowUntrackedMutations)
        {
            return OperationResult.Fail(
                "This platform cannot yet capture and restore exact per-resource privacy state; the protection change was not applied.");
        }

        var validation = await _privacySessions.ValidatePreparedResourcesAsync(preparation, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        OperationResult platformResult;
        try
        {
            platformResult = await apply(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Platform block operation threw after snapshot commit");
            platformResult = OperationResult.Fail(ex.Message, outcomeUncertain: true);
        }

        OperationResult journalResult;
        try
        {
            // Reconciliation and rollback must survive caller cancellation once native dispatch began.
            journalResult = await _privacySessions.RecordBlockOutcomeAsync(
                preparation,
                platformResult,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to record platform outcome; durable ApplyPending entries remain for startup recovery");
            var executionStillInFlight = platformResult.ExecutionStillInFlight;
            if (executionStillInFlight)
            {
                _privacySessions.RequireMutationQuiescence();
                var quiescence = await _privacySessions.EnsureMutationQuiescenceAsync(CancellationToken.None);
                executionStillInFlight = !quiescence.Success;
            }

            if (executionStillInFlight)
            {
                return OperationResult.Fail(
                    $"The platform outcome checkpoint failed and its native actor is not quiescent: {ex.Message}",
                    platformResult.Details,
                    outcomeUncertain: true,
                    executionStillInFlight: true);
            }

            var compensation = await _privacySessions.RollbackConfirmedApplyAfterCheckpointFailureAsync(
                preparation,
                platformResult,
                CancellationToken.None);
            var compensationMessage = compensation.SafeToExit
                ? " Confirmed changes were immediately restored."
                : $" Immediate restoration remains incomplete: {compensation.ErrorMessage}";
            return OperationResult.Fail(
                $"The platform outcome checkpoint failed: {ex.Message}.{compensationMessage}",
                platformResult.Details,
                outcomeUncertain: platformResult.OutcomeUncertain,
                executionStillInFlight: false);
        }

        var result = platformResult.Success ? journalResult : platformResult;
        if (!result.Success)
        {
            if (result.ExecutionStillInFlight)
            {
                return OperationResult.Fail(
                    (result.ErrorMessage ?? "Protection outcome is unknown.") +
                    " Recovery remains deferred until the privileged actor is proven stopped.",
                    result.Details,
                    outcomeUncertain: true,
                    executionStillInFlight: true);
            }

            var rollback = await RestorePreparedDeltaOrScopeAsync(
                preparation,
                layer,
                target,
                "BlockFailureRollback",
                CancellationToken.None);
            var rollbackSuffix = !rollback.SafeToExit
                ? $" Rollback remains incomplete: {rollback.ErrorMessage}"
                : rollback.ConflictCount > 0
                    ? $" External conflicts were preserved: {rollback.ErrorMessage}"
                    : " Partial changes were rolled back.";
            return OperationResult.Fail((result.ErrorMessage ?? "Protection operation failed.") + rollbackSuffix, result.Details);
        }

        stopwatch.Stop();
        Log.Information(
            "Protection applied and journaled: Layer={Layer}, Target={Target}, DurationMs={DurationMs}",
            layer,
            target,
            stopwatch.ElapsedMilliseconds);
        return result;
    }

    private Task<PrivacyRecoveryResult> RestorePreparedDeltaOrScopeAsync(
        PrivacyBlockPreparation? preparation,
        ProtectionLayer layer,
        BlockTarget target,
        string reason,
        CancellationToken cancellationToken) =>
        preparation is { TrackingEnabled: true }
            ? _privacySessions.RestorePreparedDeltaAsync(
                preparation,
                layer,
                target,
                reason,
                cancellationToken)
            : _privacySessions.RestoreAsync(layer, target, reason, cancellationToken);

    private async Task<PrivacyRecoveryResult> RestoreScopeAsync(
        ProtectionLayer layer,
        BlockTarget target,
        string reason,
        RestoreScopePolicy policy,
        CancellationToken cancellationToken)
    {
        if (_privacySessions.SupportsPersistentRecovery)
        {
            var recovery = await _privacySessions.RestoreAsync(layer, target, reason, cancellationToken);
            if (!recovery.SafeToExit)
                return recovery;
            if (!recovery.HadRecoveryWork &&
                !recovery.HadPersistedSession &&
                _legacyDetector.HasLegacyEvidence())
                return LegacyUntrackedRecoveryResult();
            if (!recovery.HadRecoveryWork)
            {
                var actual = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
                var active = IsActualScopeActive(actual, layer, target);
                if (active == true)
                {
                    return new PrivacyRecoveryResult
                    {
                        IsComplete = true,
                        ConflictCount = 1,
                        ErrorMessage =
                            "The requested privacy scope is protected, but no PrivLock ownership snapshot exists. " +
                            "The external or legacy state was preserved."
                    };
                }
                if (active == null)
                {
                    if (policy == RestoreScopePolicy.OpportunisticPreTeardown)
                    {
                        return PrivacyRecoveryResult.NothingToRestore();
                    }

                    return new PrivacyRecoveryResult
                    {
                        IsComplete = false,
                        FailedCount = 1,
                        ErrorMessage =
                            "The requested privacy scope has no ownership snapshot and its current state could not be verified."
                    };
                }
            }
            return recovery;
        }

        if (!_allowUntrackedMutations)
        {
            return _legacyDetector.HasLegacyEvidence()
                ? LegacyUntrackedRecoveryResult()
                : PrivacyRecoveryResult.Unsupported();
        }

        var result = layer == ProtectionLayer.Standard
            ? await _protectionProvider.DisableStandardProtectionAsync(target, cancellationToken)
            : await _protectionProvider.DisableSecureProtectionAsync(target, cancellationToken);
        return new PrivacyRecoveryResult
        {
            TrackingSupported = false,
            IsComplete = result.Success,
            FailedCount = result.Success ? 0 : 1,
            ErrorMessage = result.ErrorMessage
        };
    }

    private async Task<OperationResult> RunSerializedUserOperationAsync(
        Func<Task<OperationResult>> operation,
        string operationId,
        CancellationToken cancellationToken)
    {
        if (IsShutdownStarted)
            return OperationResult.Fail("PrivGvard is shutting down; new protection changes are not accepted.");

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsShutdownStarted)
                return OperationResult.Fail("PrivGvard is shutting down; new protection changes are not accepted.");
            // The whole serialized operation must be independent of a UI synchronization context.
            // Shutdown/logout is then free to return control to the dispatcher while this operation
            // reaches its durable checkpoint and releases the gate.
            return await Task.Run(operation, CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Log.Warning("Protection operation {OperationId} was cancelled", operationId);
            return OperationResult.Fail("The operation was cancelled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected protection operation failure");
            return OperationResult.Fail(ex.Message);
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private async Task PublishVerifiedStateAsync(CancellationToken cancellationToken)
    {
        var state = await GetCurrentStateAsync(cancellationToken);
        StateChanged?.Invoke(state);
    }

    private async Task<bool> IsSecureActiveAsync(BlockTarget target, CancellationToken cancellationToken)
    {
        var state = await _protectionProvider.GetProtectionStateAsync(cancellationToken);
        return target switch
        {
            BlockTarget.Camera => state.Camera.SecureState == SecureProtectionState.Active,
            BlockTarget.Microphone => state.Microphone.SecureState == SecureProtectionState.Active,
            BlockTarget.Both =>
                state.Camera.SecureState == SecureProtectionState.Active ||
                state.Microphone.SecureState == SecureProtectionState.Active,
            _ => false
        };
    }

    private void UpdateDesiredAfterStandardEnable(BlockTarget target)
    {
        var desired = _stateStore.Load();
        if (target is BlockTarget.Camera or BlockTarget.Both)
        {
            desired.CameraStandard = StandardProtectionState.Active;
            if (!IsStoreEdition && desired.CameraSecure == SecureProtectionState.Unavailable)
                desired.CameraSecure = SecureProtectionState.Available;
        }
        if (target is BlockTarget.Microphone or BlockTarget.Both)
        {
            desired.MicrophoneStandard = StandardProtectionState.Active;
            if (!IsStoreEdition && desired.MicrophoneSecure == SecureProtectionState.Unavailable)
                desired.MicrophoneSecure = SecureProtectionState.Available;
        }
        _stateStore.Save(desired);
    }

    private void UpdateDesiredAfterSecureEnable(BlockTarget target)
    {
        var desired = _stateStore.Load();
        if (target is BlockTarget.Camera or BlockTarget.Both)
            desired.CameraSecure = SecureProtectionState.Active;
        if (target is BlockTarget.Microphone or BlockTarget.Both)
            desired.MicrophoneSecure = SecureProtectionState.Active;
        _stateStore.Save(desired);
    }

    private void UpdateDesiredAfterStandardDisable(BlockTarget target)
    {
        var desired = _stateStore.Load();
        if (target is BlockTarget.Camera or BlockTarget.Both)
        {
            desired.CameraStandard = StandardProtectionState.Inactive;
            desired.CameraSecure = SecureProtectionState.Unavailable;
        }
        if (target is BlockTarget.Microphone or BlockTarget.Both)
        {
            desired.MicrophoneStandard = StandardProtectionState.Inactive;
            desired.MicrophoneSecure = SecureProtectionState.Unavailable;
        }
        _stateStore.Save(desired);
    }

    private OperationResult ToOperationResult(PrivacyRecoveryResult result)
    {
        if (!result.TrackingSupported && !_allowUntrackedMutations)
        {
            return OperationResult.Fail(
                "Exact reversible privacy-state tracking is not supported on this platform; no broad unblock was attempted.");
        }
        if (!result.SafeToExit)
            return OperationResult.Fail(result.ErrorMessage ?? "Restoration remains incomplete.");
        if (result.ConflictCount > 0)
            return OperationResult.Fail(result.ErrorMessage ?? "External state conflicts were preserved.");
        return OperationResult.Ok();
    }

    private static PrivacyRecoveryResult CombineRecoveryResults(params PrivacyRecoveryResult[] results) => new()
    {
        TrackingSupported = results.All(result => result.TrackingSupported),
        HadPersistedSession = results.Any(result => result.HadPersistedSession),
        HadRecoveryWork = results.Any(result => result.HadRecoveryWork),
        IsComplete = results.All(result => result.IsComplete),
        RestoredCount = results.Sum(result => result.RestoredCount),
        AlreadyRestoredCount = results.Sum(result => result.AlreadyRestoredCount),
        ConflictCount = results.Sum(result => result.ConflictCount),
        IrreducibleAmbiguityCount = results.Sum(result => result.IrreducibleAmbiguityCount),
        MissingCount = results.Sum(result => result.MissingCount),
        FailedCount = results.Sum(result => result.FailedCount),
        ErrorMessage = string.Join(" ", results.Select(result => result.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)))
    };

    private static PrivacyRecoveryResult LegacyUntrackedRecoveryResult() => new()
    {
        IsComplete = false,
        FailedCount = 1,
        ErrorMessage =
            "A protection state from an older PrivLock version was detected without an exact recovery snapshot. " +
            "It was preserved; automatic broad unblocking is unsafe. Review the affected operating-system privacy settings manually."
    };

    private static bool? IsActualScopeActive(
        FullProtectionState state,
        ProtectionLayer layer,
        BlockTarget target)
    {
        static bool? ForTarget(TargetProtectionStatus status, ProtectionLayer requestedLayer) =>
            requestedLayer == ProtectionLayer.Standard
                ? status.StandardState switch
                {
                    StandardProtectionState.Active => true,
                    StandardProtectionState.Inactive => false,
                    _ => null
                }
                : status.SecureState switch
                {
                    SecureProtectionState.Active => true,
                    SecureProtectionState.Unknown => null,
                    _ => false
                };

        var camera = ForTarget(state.Camera, layer);
        var microphone = ForTarget(state.Microphone, layer);
        return target switch
        {
            BlockTarget.Camera => camera,
            BlockTarget.Microphone => microphone,
            BlockTarget.Both when camera == true || microphone == true => true,
            BlockTarget.Both when camera == null || microphone == null => null,
            BlockTarget.Both => false,
            _ => null
        };
    }

    private static string CreateOperationId(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        return $"Op-{prefix}-{suffix}";
    }

    private sealed class VolatilePrivacySessionStore : IPrivacySessionStore
    {
        private PrivacySession? _session;
        public PrivacySession? Load() => _session;
        public void Save(PrivacySession session) => _session = session;
    }
}

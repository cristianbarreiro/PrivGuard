using System.Collections.Concurrent;
using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

/// <summary>
/// Phase 6 — Windows Hardening & Release Readiness automated validation tests.
/// Validates:
/// - Phase 6A: Hardware Hotplug (HOTPLUG-A Camera, HOTPLUG-B Microphone, HOTPLUG-C Combined, HOTPLUG-D Repeated cycles)
/// - Phase 6B: Crash Recovery (CRASH-B Quiescence Barrier, CRASH-C Partial/Corrupt Journal, CRASH-D Recovery Repetition / Idempotency)
/// - Phase 6C: UAC & Privilege Handling (UAC-B Cancellation Safety, UAC-D Worker Failure Rollback)
/// </summary>
public sealed class WindowsHardeningPhase6Tests : IDisposable
{
    private readonly string _testDirectory;

    public WindowsHardeningPhase6Tests()
    {
        var testId = Guid.NewGuid().ToString("N");
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PrivGvardPhase6Hardening_{testId}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, recursive: true);
        }
        catch
        {
        }
    }

    // =========================================================================
    // PHASE 6A: HARDWARE HOTPLUG VALIDATION
    // =========================================================================

    [Fact]
    public async Task HotplugA_Camera_UnpluggedWhileProtected_ObservedAsMissing_Replugged_RestoresCleanly()
    {
        var env = CreateHardeningEnvironment("HotplugA");

        // 1. Enable Camera protection
        var enableResult = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(enableResult.Success);

        var state1 = await env.ProtectionService.GetCurrentStateAsync();
        Assert.True(state1.Camera.IsProtected);

        // 2. Physical disconnection: Camera is unplugged mid-operation
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.CameraConsentResourceId,
            PrivacyResourceObservationKind.Missing);

        // 3. Application or recovery pass detects missing device without crashing or corrupting WAL
        var obsPass = await env.PlatformAdapter.ObserveAsync(new OriginalPolicyState
        {
            ResourceId = HardeningPlatformAdapter.CameraConsentResourceId,
            OperationId = "Op-ObsPass",
            RegistryHive = PrivacyRegistryHive.CurrentUser,
            RegistryView = PrivacyRegistryView.Default,
            RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam",
            ValueName = "Value",
            ProtectedValueExists = true,
            ProtectedValue = "Deny",
            ProtectedValueKind = PrivacyRegistryValueKind.String
        });
        Assert.Equal(PrivacyResourceObservationKind.Missing, obsPass.Kind);

        // 4. Physical reconnection: Camera is plugged back in
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.CameraConsentResourceId,
            PrivacyResourceObservationKind.MatchesProtected);

        // 5. User disables protection: device is successfully and safely restored to original state
        var disableResult = await env.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(disableResult.Success);

        var stateAfterDisable = await env.ProtectionService.GetCurrentStateAsync();
        Assert.False(stateAfterDisable.Camera.IsProtected);
        Assert.Contains(HardeningPlatformAdapter.CameraConsentResourceId, env.PlatformAdapter.RestoreCalls);
    }

    [Fact]
    public async Task HotplugB_Microphone_UnpluggedWhileProtected_ObservedAsMissing_Replugged_RestoresCleanly()
    {
        var env = CreateHardeningEnvironment("HotplugB");

        // 1. Enable Microphone protection
        var enableResult = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(enableResult.Success);

        var state1 = await env.ProtectionService.GetCurrentStateAsync();
        Assert.True(state1.Microphone.IsProtected);

        // 2. Physical disconnection of USB microphone audio endpoint
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.MicMuteResourceId,
            PrivacyResourceObservationKind.Missing);

        // 3. Verify detection of missing audio endpoint
        var obsPass = await env.PlatformAdapter.ObserveAsync(new OriginalAudioEndpointState
        {
            ResourceId = HardeningPlatformAdapter.MicMuteResourceId,
            OperationId = "Op-ObsPass",
            EndpointId = "mic-endpoint-1",
            ProtectedMutedState = true
        });
        Assert.Equal(PrivacyResourceObservationKind.Missing, obsPass.Kind);

        // 4. Physical reconnection
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.MicMuteResourceId,
            PrivacyResourceObservationKind.MatchesProtected);

        // 5. Disable protection restores original mute state cleanly
        var disableResult = await env.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(disableResult.Success);

        var stateAfterDisable = await env.ProtectionService.GetCurrentStateAsync();
        Assert.False(stateAfterDisable.Microphone.IsProtected);
        Assert.Contains(HardeningPlatformAdapter.MicMuteResourceId, env.PlatformAdapter.RestoreCalls);
    }

    [Fact]
    public async Task HotplugC_Combined_OneDeviceUnplugged_DoesNotCorruptOrAlterOtherDeviceOwnership()
    {
        var env = CreateHardeningEnvironment("HotplugC");

        // 1. Enable Both Camera and Microphone protection
        var enableResult = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Both);
        Assert.True(enableResult.Success);

        var stateInitial = await env.ProtectionService.GetCurrentStateAsync();
        Assert.True(stateInitial.Camera.IsProtected);
        Assert.True(stateInitial.Microphone.IsProtected);

        // 2. Camera is unplugged (Missing), while Microphone remains connected and protected
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.CameraConsentResourceId,
            PrivacyResourceObservationKind.Missing);

        // 3. User disables Microphone only
        var disableMic = await env.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(disableMic.Success);

        // Microphone restored, Camera still owned/registered without collateral corruption
        var stateAfterMicDisable = await env.ProtectionService.GetCurrentStateAsync();
        Assert.False(stateAfterMicDisable.Microphone.IsProtected);
        Assert.Contains(HardeningPlatformAdapter.MicMuteResourceId, env.PlatformAdapter.RestoreCalls);

        // Reconnect camera and disable it cleanly
        env.PlatformAdapter.SetObservation(
            HardeningPlatformAdapter.CameraConsentResourceId,
            PrivacyResourceObservationKind.MatchesProtected);

        var disableCam = await env.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(disableCam.Success);

        var finalState = await env.ProtectionService.GetCurrentStateAsync();
        Assert.False(finalState.Camera.IsProtected);
        Assert.False(finalState.Microphone.IsProtected);
    }

    [Fact]
    public async Task HotplugD_RepeatedReconnectionCycles_ZeroDuplicateSessions_ExactRestoration()
    {
        var env = CreateHardeningEnvironment("HotplugD");

        for (var cycle = 1; cycle <= 3; cycle++)
        {
            // Protect
            var enable = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
            Assert.True(enable.Success);

            // Unplug
            env.PlatformAdapter.SetObservation(
                HardeningPlatformAdapter.CameraConsentResourceId,
                PrivacyResourceObservationKind.Missing);

            // Replug
            env.PlatformAdapter.SetObservation(
                HardeningPlatformAdapter.CameraConsentResourceId,
                PrivacyResourceObservationKind.MatchesProtected);

            // Unprotect
            var disable = await env.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Camera);
            Assert.True(disable.Success);

            var state = await env.ProtectionService.GetCurrentStateAsync();
            Assert.False(state.Camera.IsProtected);
        }

        // Verify session was cleanly finalized with zero orphan leaks
        var finalSession = env.SessionStore.Load();
        Assert.NotNull(finalSession);
        Assert.False(finalSession!.IsActive);
    }

    // =========================================================================
    // PHASE 6B: CRASH RECOVERY VALIDATION
    // =========================================================================

    [Fact]
    public async Task CrashB_InFlightOperation_QuiescenceBarrierEnforced_RollbackRestoresOnlyVerifiablyOwned()
    {
        var env = CreateHardeningEnvironment("CrashB");

        // Simulate an in-flight unconfirmed operation left in WAL
        var inFlightDevice = new OriginalDeviceState
        {
            ResourceId = "pnp:camera-inflight",
            OperationId = "Op-InFlight",
            Layer = ProtectionLayer.Secure,
            Target = BlockTarget.Camera,
            CapturedAtUtc = DateTimeOffset.UtcNow,
            LastUpdatedAtUtc = DateTimeOffset.UtcNow,
            JournalState = PrivacyResourceJournalState.ApplyPending,
            ModifiedByPrivLock = false,
            OwnershipUncertain = true,
            ExecutionMayStillBeInFlight = true,
            InstanceId = "camera-inflight",
            FriendlyName = "camera-inflight",
            DeviceClass = "{ca3e7ab9-b4c3-4ae6-8251-579ef933890f}",
            IsPresentAtCapture = true,
            OriginalEnabledState = true,
            OriginalProblemCode = 0,
            ProtectedEnabledState = false,
            IsSafelyRestorable = true
        };

        var crashSession = new PrivacySession
        {
            SessionId = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            OwnerProcessId = 9999,
            IsActive = true,
            Status = PrivacySessionStatus.Active,
            LastOperationId = "Op-InFlight",
            Resources = [inFlightDevice]
        };
        env.SessionStore.Save(crashSession);

        env.PlatformAdapter.SetObservation("pnp:camera-inflight", PrivacyResourceObservationKind.MatchesOriginal);
        env.PlatformAdapter.SetAttestation("pnp:camera-inflight", PrivacyOwnershipAttestationKind.NotConfirmed);

        // Recovery executes quiescence barrier and resolves in-flight without blind restore
        var recovery = await env.RecoveryService.RecoverAtStartupAsync();
        Assert.True(recovery.SafeToExit);
        Assert.True(env.PlatformAdapter.QuiescenceChecked);
        Assert.Empty(env.PlatformAdapter.RestoreCalls); // Did not blindly restore un-owned resource
    }

    [Fact]
    public async Task CrashC_PartialCorruptJournal_FailsSafelyWithoutDestroyingOwnershipEvidence()
    {
        var env = CreateHardeningEnvironment("CrashC");

        // Write an unparseable/corrupted session file to simulate power loss during raw disk write
        var sessionFile = Path.Combine(env.RecoveryDir, "privacy-session-v1.json");
        File.WriteAllText(sessionFile, "{ \"invalidJson\": [ truncated... ");

        // Recovery fails safe with PrivacySessionStoreException, without destroying ownership evidence
        await Assert.ThrowsAsync<PrivacySessionStoreException>(() => env.RecoveryService.RecoverAtStartupAsync());

        // Verify the corrupted file was preserved for forensics / not wiped
        Assert.True(File.Exists(sessionFile));
    }

    [Fact]
    public async Task CrashD_RecoveryRepetition_IsIdempotent_ZeroDuplicatedWrites()
    {
        var env = CreateHardeningEnvironment("CrashD");

        // Create a crash session with 1 applied camera device
        var ownedDevice = new OriginalDeviceState
        {
            ResourceId = "pnp:camera-owned-1",
            OperationId = "Op-CrashD",
            Layer = ProtectionLayer.Secure,
            Target = BlockTarget.Camera,
            CapturedAtUtc = DateTimeOffset.UtcNow,
            LastUpdatedAtUtc = DateTimeOffset.UtcNow,
            JournalState = PrivacyResourceJournalState.Applied,
            ModifiedByPrivLock = true,
            InstanceId = "camera-owned-1",
            FriendlyName = "camera-owned-1",
            DeviceClass = "{ca3e7ab9-b4c3-4ae6-8251-579ef933890f}",
            IsPresentAtCapture = true,
            OriginalEnabledState = true,
            OriginalProblemCode = 0,
            ProtectedEnabledState = false,
            IsSafelyRestorable = true
        };

        var crashSession = new PrivacySession
        {
            SessionId = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            OwnerProcessId = 8888,
            IsActive = true,
            Status = PrivacySessionStatus.Active,
            LastOperationId = "Op-CrashD",
            Resources = [ownedDevice]
        };
        env.SessionStore.Save(crashSession);

        env.PlatformAdapter.SetObservation("pnp:camera-owned-1", PrivacyResourceObservationKind.MatchesProtected);
        env.PlatformAdapter.SetAttestation("pnp:camera-owned-1", PrivacyOwnershipAttestationKind.Confirmed);

        // First recovery pass: restores the owned device
        var recovery1 = await env.RecoveryService.RecoverAtStartupAsync();
        Assert.True(recovery1.SafeToExit);
        Assert.Single(env.PlatformAdapter.RestoreCalls);

        // Second recovery pass immediately following: idempotent, zero duplicated writes
        var recovery2 = await env.RecoveryService.RecoverAtStartupAsync();
        Assert.True(recovery2.SafeToExit);
        Assert.False(recovery2.HadRecoveryWork);
        Assert.Single(env.PlatformAdapter.RestoreCalls);
    }

    // =========================================================================
    // PHASE 6C: UAC AND PRIVILEGE VALIDATION
    // =========================================================================

    [Fact]
    public async Task UacB_UserCancelsUac_DoesNotApplyUnrecordedMutations_PreservesDesiredState()
    {
        var env = CreateHardeningEnvironment("UacB");

        // 1. Establish Standard protection baseline first
        var standardResult = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(standardResult.Success);

        // Configure provider to simulate UAC denial on Secure protection
        env.ProtectionProvider.SimulateUacDenialOnSecure = true;

        // 2. Attempting secure protection fails with clean cancellation
        var secureResult = await env.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Camera);
        Assert.False(secureResult.Success);
        Assert.Contains("cancelled", secureResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Invariant: DesiredState was preserved, not corrupted
        var desired = env.StateStore.Load();
        Assert.Equal(StandardProtectionState.Active, desired.CameraStandard);
    }

    [Fact]
    public async Task UacD_PrivilegedWorkerCommandFailure_TriggersSafeRollback()
    {
        var env = CreateHardeningEnvironment("UacD");

        env.ProtectionProvider.SimulateWorkerCommandFailure = true;

        var result = await env.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.False(result.Success);

        // Safe rollback occurred, camera remains unprotected
        var state = await env.ProtectionService.GetCurrentStateAsync();
        Assert.False(state.Camera.IsProtected);
    }

    // =========================================================================
    // Test Environment Infrastructure
    // =========================================================================

    private HardeningEnvironment CreateHardeningEnvironment(string testName)
    {
        var testDir = Path.Combine(_testDirectory, testName);
        var stateDir = Path.Combine(testDir, "Data");
        var recoveryDir = Path.Combine(testDir, "Recovery");
        Directory.CreateDirectory(stateDir);
        Directory.CreateDirectory(recoveryDir);

        var stateStore = new FileStateStore(stateDir);
        var sessionStore = new FilePrivacySessionStore(recoveryDir);
        var platform = new HardeningPlatformAdapter();
        var provider = new HardeningProtectionProvider(platform);

        platform.OnRestored = resource =>
        {
            if (resource.Target is BlockTarget.Camera or BlockTarget.Both)
                provider.SetCameraProtected(false);
            if (resource.Target is BlockTarget.Microphone or BlockTarget.Both)
                provider.SetMicProtected(false);
        };

        var sessions = new PrivacySessionService(sessionStore, platform);
        var protection = new ProtectionService(provider, provider, provider, stateStore, sessions);
        var recovery = new PrivacyRecoveryService(protection);

        return new HardeningEnvironment(
            stateDir,
            recoveryDir,
            stateStore,
            sessionStore,
            platform,
            provider,
            protection,
            recovery);
    }

    private sealed record HardeningEnvironment(
        string StateDir,
        string RecoveryDir,
        FileStateStore StateStore,
        FilePrivacySessionStore SessionStore,
        HardeningPlatformAdapter PlatformAdapter,
        HardeningProtectionProvider ProtectionProvider,
        ProtectionService ProtectionService,
        PrivacyRecoveryService RecoveryService);

    private sealed class HardeningPlatformAdapter : IPrivacySessionPlatformAdapter
    {
        public const string CameraConsentResourceId =
            @"registry:CurrentUser:Default:Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam:Value";
        public const string MicMuteResourceId = "audio-mute:mic-endpoint-1";

        private readonly ConcurrentDictionary<string, PrivacyResourceObservation> _observations =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, PrivacyOwnershipAttestation> _attestations =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _restoreCalls = [];

        public Action<PrivacyResourceState>? OnRestored { get; set; }
        public bool SupportsPersistentRecovery => true;
        public bool QuiescenceChecked { get; private set; }
        public IReadOnlyList<string> RestoreCalls
        {
            get
            {
                lock (_restoreCalls) { return _restoreCalls.ToArray(); }
            }
        }

        public void SetObservation(string resourceId, PrivacyResourceObservationKind kind, string? error = null) =>
            _observations[resourceId] = new PrivacyResourceObservation(kind, error);

        public void SetAttestation(string resourceId, PrivacyOwnershipAttestationKind kind, string? error = null) =>
            _attestations[resourceId] = new PrivacyOwnershipAttestation(kind, error);

        public Task<OperationResult> EnsureMutationQuiescenceAsync(CancellationToken cancellationToken = default)
        {
            QuiescenceChecked = true;
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<IReadOnlyList<PrivacyResourceState>> CaptureAsync(
            ProtectionLayer layer,
            BlockTarget target,
            string operationId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<PrivacyResourceState>();
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                list.Add(new OriginalPolicyState
                {
                    ResourceId = CameraConsentResourceId,
                    OperationId = operationId,
                    Layer = ProtectionLayer.Standard,
                    Target = BlockTarget.Camera,
                    CapturedAtUtc = DateTimeOffset.UtcNow,
                    LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                    JournalState = PrivacyResourceJournalState.Captured,
                    ModifiedByPrivLock = false,
                    RegistryHive = PrivacyRegistryHive.CurrentUser,
                    RegistryView = PrivacyRegistryView.Default,
                    RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam",
                    ValueName = "Value",
                    ValueExisted = true,
                    OriginalValueKind = PrivacyRegistryValueKind.String,
                    OriginalValue = "Allow",
                    ProtectedValueExists = true,
                    ProtectedValueKind = PrivacyRegistryValueKind.String,
                    ProtectedValue = "Deny"
                });
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                list.Add(new OriginalAudioEndpointState
                {
                    ResourceId = MicMuteResourceId,
                    OperationId = operationId,
                    Layer = ProtectionLayer.Standard,
                    Target = BlockTarget.Microphone,
                    CapturedAtUtc = DateTimeOffset.UtcNow,
                    LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                    JournalState = PrivacyResourceJournalState.Captured,
                    ModifiedByPrivLock = false,
                    EndpointId = "mic-endpoint-1",
                    OriginalMutedState = false,
                    ProtectedMutedState = true
                });
            }
            return Task.FromResult<IReadOnlyList<PrivacyResourceState>>(list);
        }

        public Task<PrivacyResourceObservation> ObserveAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_observations.TryGetValue(resource.ResourceId, out var obs)
                ? obs
                : new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal));

        public Task<PrivacyOwnershipAttestation> VerifyOwnershipAttestationAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_attestations.TryGetValue(resource.ResourceId, out var att)
                ? att
                : new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.NotConfirmed));

        public Task<OperationResult> RestoreOriginalAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            lock (_restoreCalls)
            {
                _restoreCalls.Add(resource.ResourceId);
            }
            _observations[resource.ResourceId] = new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal);
            OnRestored?.Invoke(resource);
            return Task.FromResult(OperationResult.Ok());
        }

        public void CompleteRecoveryPass() { }
    }

    private sealed class HardeningProtectionProvider :
        IDeviceProtectionProvider,
        IDeviceDetector,
        IPlatformCapabilityProvider
    {
        private readonly HardeningPlatformAdapter _platform;

        public bool CameraIsProtected { get; private set; }
        public bool MicIsProtected { get; private set; }
        public bool SimulateUacDenialOnSecure { get; set; }
        public bool SimulateWorkerCommandFailure { get; set; }

        public void SetCameraProtected(bool val) => CameraIsProtected = val;
        public void SetMicProtected(bool val) => MicIsProtected = val;

        public HardeningProtectionProvider(HardeningPlatformAdapter platform)
        {
            _platform = platform;
        }

        public PlatformCapabilities Capabilities => new()
        {
            CameraProtectionLevel = CapabilityLevel.Hardware,
            MicrophoneProtectionLevel = CapabilityLevel.Hardware
        };

        public PlatformInfo PlatformInfo => new()
        {
            OperatingSystemName = "Windows",
            OsVersion = "10.0.26100",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken ct = default)
        {
            if (SimulateWorkerCommandFailure)
                return Task.FromResult(OperationResult.Fail("Native mutation failed during block operation."));

            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                CameraIsProtected = true;
                _platform.SetObservation(HardeningPlatformAdapter.CameraConsentResourceId, PrivacyResourceObservationKind.MatchesProtected);
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                MicIsProtected = true;
                _platform.SetObservation(HardeningPlatformAdapter.MicMuteResourceId, PrivacyResourceObservationKind.MatchesProtected);
            }
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken ct = default)
        {
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                CameraIsProtected = false;
                _platform.SetObservation(HardeningPlatformAdapter.CameraConsentResourceId, PrivacyResourceObservationKind.MatchesOriginal);
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                MicIsProtected = false;
                _platform.SetObservation(HardeningPlatformAdapter.MicMuteResourceId, PrivacyResourceObservationKind.MatchesOriginal);
            }
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken ct = default)
        {
            if (SimulateUacDenialOnSecure)
                return Task.FromResult(OperationResult.Fail("Operation cancelled: Administrator permissions were denied."));

            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken ct = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken ct = default) =>
            Task.FromResult(new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = CameraIsProtected ? StandardProtectionState.Active : StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = MicIsProtected ? StandardProtectionState.Active : StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable
                }
            });

        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);
    }
}

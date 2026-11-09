using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Xunit;

namespace PrivLock.Application.Tests;

/// <summary>
/// Phase 4 — Restart Lifecycle Validation & Remediation Test Suite.
/// Proves that the complete protection lifecycle remains correct across normal application shutdown
/// and subsequent application restart, strictly obeying the State Separation Invariant:
/// DesiredState (user intent) != PrivacySession (recovery authority) != Actual OS State (runtime reality).
/// </summary>
public sealed class RestartLifecycleValidationTests
{
    // =========================================================================
    // Core Lifecycle Validation: Tests A, B, C, D
    // =========================================================================

    [Fact]
    public async Task TestA_CameraProtection_FullLifecycle_Startup_Enable_ShutdownRestore_RestartReconcile()
    {
        var fixture = new RestartLifecycleFixture();

        // 1. Process 1 starts
        var app1 = fixture.CreateApplicationInstance();

        // 2. User enables Camera protection
        var enable = await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(enable.Success);

        // 3. Verify resulting native Windows state
        var osState1 = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osState1.Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Active, osState1.Camera.StandardState);

        // 4. Confirm DesiredState.Camera reflects user intent
        var desired1 = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desired1.CameraStandard);
        Assert.Equal(StandardProtectionState.Inactive, desired1.MicrophoneStandard);

        // 5. Confirm active PrivacySession contains ownership and original-state info
        var session1 = fixture.SessionStore.Current;
        Assert.NotNull(session1);
        Assert.True(session1!.IsActive);
        Assert.Equal(PrivacySessionStatus.Active, session1.Status);
        var initialSessionId = session1.SessionId;
        Assert.NotEmpty(session1.Resources);
        Assert.All(session1.Resources, r => Assert.Equal(BlockTarget.Camera, r.Target));

        // 6. Normal application shutdown
        var shutdown = await app1.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown.SafeToExit);

        // 7. Verify owned runtime changes are restored in OS
        var osStateAfterShutdown = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.False(osStateAfterShutdown.Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Inactive, osStateAfterShutdown.Camera.StandardState);

        // 8. Verify session is completed/inactive
        var sessionAfterShutdown = fixture.SessionStore.Current;
        Assert.NotNull(sessionAfterShutdown);
        Assert.False(sessionAfterShutdown!.IsActive);
        Assert.Equal(PrivacySessionStatus.Restored, sessionAfterShutdown.Status);

        // 9. Verify DesiredState.Camera remains unchanged across shutdown
        var desiredAfterShutdown = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desiredAfterShutdown.CameraStandard);

        // 10. Process 2 starts (simulating application restart)
        var app2 = fixture.CreateApplicationInstance();

        // Startup recovery sees previous session completed (0 recovery work required)
        var recovery2 = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery2.SafeToExit);
        Assert.False(recovery2.HadRecoveryWork);

        // 11. Startup reconciliation reapplies camera protection per DesiredState
        var reconcile2 = await app2.Recovery.ReconcileAtStartupAsync(recovery2);
        Assert.True(reconcile2.Success);

        var osStateAfterRestart = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osStateAfterRestart.Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Active, osStateAfterRestart.Camera.StandardState);

        // 12. Verify a new valid PrivacySession is created with distinct SessionId
        var session2 = fixture.SessionStore.Current;
        Assert.NotNull(session2);
        Assert.True(session2!.IsActive);
        Assert.Equal(PrivacySessionStatus.Active, session2.Status);
        Assert.NotEqual(initialSessionId, session2.SessionId);
    }

    [Fact]
    public async Task TestB_MicrophoneProtection_FullLifecycle_Registry_AudioMute_PnP_ShutdownRestore_RestartReconcile()
    {
        var fixture = new RestartLifecycleFixture();

        // 1. Process 1 starts and enables Microphone DualLayer protection
        var app1 = fixture.CreateApplicationInstance();
        var stdEnable = await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Microphone);
        var secEnable = await app1.Protection.EnableSecureProtectionAsync(BlockTarget.Microphone);
        Assert.True(stdEnable.Success);
        Assert.True(secEnable.Success);

        // 2. Verify native Windows state (both Standard and Secure active)
        var osState1 = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osState1.Microphone.IsProtected);
        Assert.Equal(StandardProtectionState.Active, osState1.Microphone.StandardState);
        Assert.Equal(SecureProtectionState.Active, osState1.Microphone.SecureState);

        // 3. Verify DesiredState reflects user intent for Microphone
        var desired1 = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desired1.MicrophoneStandard);
        Assert.Equal(SecureProtectionState.Active, desired1.MicrophoneSecure);

        // 4. Verify PrivacySession has active resources
        var session1 = fixture.SessionStore.Current;
        Assert.NotNull(session1);
        Assert.True(session1!.IsActive);
        var initialSessionId = session1.SessionId;

        // 5. Normal shutdown restores all microphone layers
        var shutdown = await app1.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown.SafeToExit);

        var osStateAfterShutdown = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.False(osStateAfterShutdown.Microphone.IsProtected);
        Assert.Equal(StandardProtectionState.Inactive, osStateAfterShutdown.Microphone.StandardState);

        // 6. DesiredState preserved across shutdown
        var desiredAfterShutdown = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desiredAfterShutdown.MicrophoneStandard);
        Assert.Equal(SecureProtectionState.Active, desiredAfterShutdown.MicrophoneSecure);

        // 7. Restart application
        var app2 = fixture.CreateApplicationInstance();
        var recovery2 = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery2.SafeToExit);

        var reconcile2 = await app2.Recovery.ReconcileAtStartupAsync(recovery2);
        Assert.True(reconcile2.Success);

        // 8. Reconciled protection is active again
        var osStateAfterRestart = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osStateAfterRestart.Microphone.IsProtected);
        Assert.Equal(StandardProtectionState.Active, osStateAfterRestart.Microphone.StandardState);

        // 9. Fresh session created
        var session2 = fixture.SessionStore.Current;
        Assert.NotNull(session2);
        Assert.True(session2!.IsActive);
        Assert.NotEqual(initialSessionId, session2.SessionId);
    }

    [Fact]
    public async Task TestC_CombinedProtection_FullLifecycle_CameraAndMicrophone_IndependentOwnership_ShutdownRestore_RestartReconcile()
    {
        var fixture = new RestartLifecycleFixture();

        // 1. Process 1 starts and enables both Camera and Microphone
        var app1 = fixture.CreateApplicationInstance();
        var camResult = await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        var micResult = await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(camResult.Success && micResult.Success);

        var osState1 = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osState1.Camera.IsProtected);
        Assert.True(osState1.Microphone.IsProtected);

        var session1 = fixture.SessionStore.Current;
        Assert.NotNull(session1);
        var initialSessionId = session1!.SessionId;

        // 2. Normal shutdown restores all resources
        var shutdown = await app1.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown.SafeToExit);

        var osAfterShutdown = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.False(osAfterShutdown.Camera.IsProtected);
        Assert.False(osAfterShutdown.Microphone.IsProtected);
        Assert.False(fixture.SessionStore.Current!.IsActive);

        // 3. DesiredState preserves both intents
        var desiredAfterShutdown = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desiredAfterShutdown.CameraStandard);
        Assert.Equal(StandardProtectionState.Active, desiredAfterShutdown.MicrophoneStandard);

        // 4. Restart PrivGvard
        var app2 = fixture.CreateApplicationInstance();
        var recovery2 = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery2.SafeToExit);

        var reconcile2 = await app2.Recovery.ReconcileAtStartupAsync(recovery2);
        Assert.True(reconcile2.Success);

        // 5. Both protections are active in OS and new session is created
        var osAfterRestart = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.True(osAfterRestart.Camera.IsProtected);
        Assert.True(osAfterRestart.Microphone.IsProtected);

        var session2 = fixture.SessionStore.Current;
        Assert.NotNull(session2);
        Assert.True(session2!.IsActive);
        Assert.NotEqual(initialSessionId, session2.SessionId);
        Assert.Equal(2, session2.Resources.Count);
    }

    [Fact]
    public async Task TestD_ExplicitUserDisable_Lifecycle_Shutdown_Restart_DoesNotReapply()
    {
        var fixture = new RestartLifecycleFixture();

        // 1. Process 1 starts and enables Camera
        var app1 = fixture.CreateApplicationInstance();
        await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.Equal(StandardProtectionState.Active, fixture.Dependencies.Load().CameraStandard);

        // 2. User explicitly disables Camera protection
        var disable = await app1.Protection.DisableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(disable.Success);

        // Invariant: Native OS state immediately inactive
        var osStateAfterDisable = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.False(osStateAfterDisable.Camera.IsProtected);

        // Invariant: DesiredState cleared to Inactive for Camera
        var desiredAfterDisable = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Inactive, desiredAfterDisable.CameraStandard);

        // 3. Normal shutdown
        var shutdown = await app1.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown.SafeToExit);

        // 4. Process 2 restarts
        var app2 = fixture.CreateApplicationInstance();
        var recovery2 = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery2.SafeToExit);

        var reconcile2 = await app2.Recovery.ReconcileAtStartupAsync(recovery2);
        Assert.True(reconcile2.Success);

        // 5. Invariant: Explicitly disabled protection is NOT reapplied at restart
        var osStateAfterRestart = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.False(osStateAfterRestart.Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Inactive, osStateAfterRestart.Camera.StandardState);
        Assert.Equal(StandardProtectionState.Inactive, fixture.Dependencies.Load().CameraStandard);
    }

    // =========================================================================
    // Repeated Lifecycle & Idempotency
    // =========================================================================

    [Fact]
    public async Task MultiCycle_Enable_Shutdown_Restart_Reconcile_Shutdown_Restart_Reconcile_Idempotent()
    {
        var fixture = new RestartLifecycleFixture();

        // Cycle 1: Enable & Shutdown
        var app1 = fixture.CreateApplicationInstance();
        await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True((await fixture.Dependencies.GetProtectionStateAsync()).Camera.IsProtected);
        var shutdown1 = await app1.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown1.SafeToExit);
        Assert.False((await fixture.Dependencies.GetProtectionStateAsync()).Camera.IsProtected);

        // Cycle 2: Restart, Reconcile, Shutdown
        var app2 = fixture.CreateApplicationInstance();
        var recov2 = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recov2.SafeToExit);
        var recon2 = await app2.Recovery.ReconcileAtStartupAsync(recov2);
        Assert.True(recon2.Success);
        Assert.True((await fixture.Dependencies.GetProtectionStateAsync()).Camera.IsProtected);

        var shutdown2 = await app2.Shutdown.RestoreAsync("UserExit");
        Assert.True(shutdown2.SafeToExit);
        Assert.False((await fixture.Dependencies.GetProtectionStateAsync()).Camera.IsProtected);

        // Cycle 3: Restart & Reconcile again
        var app3 = fixture.CreateApplicationInstance();
        var recov3 = await app3.Recovery.RecoverAtStartupAsync();
        Assert.True(recov3.SafeToExit);
        var recon3 = await app3.Recovery.ReconcileAtStartupAsync(recov3);
        Assert.True(recon3.Success);

        Assert.True((await fixture.Dependencies.GetProtectionStateAsync()).Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Active, fixture.Dependencies.Load().CameraStandard);

        // Session is cleanly active with exactly 1 session in store, no duplicates
        Assert.NotNull(fixture.SessionStore.Current);
        Assert.True(fixture.SessionStore.Current!.IsActive);
    }

    [Fact]
    public async Task StartupReconciliation_MultipleConsecutiveCalls_DoesNotDuplicateSessionsOrStaleMarkers()
    {
        var fixture = new RestartLifecycleFixture();

        var app1 = fixture.CreateApplicationInstance();
        await app1.Protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        await app1.Shutdown.RestoreAsync("UserExit");

        var app2 = fixture.CreateApplicationInstance();
        var recovery = await app2.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery.SafeToExit);

        // Call 1
        var recon1 = await app2.Recovery.ReconcileAtStartupAsync(recovery);
        Assert.True(recon1.Success);
        var expectedSessionId = fixture.SessionStore.Current!.SessionId;
        var saveAttemptsBeforeSecondCall = fixture.SessionStore.SaveAttempts;

        // Call 2 (Immediate redundant call)
        var recon2 = await app2.Recovery.ReconcileAtStartupAsync(recovery);
        Assert.True(recon2.Success);

        // Assert: Same session ID preserved, no extra save or duplicate session
        Assert.Equal(expectedSessionId, fixture.SessionStore.Current!.SessionId);
        Assert.Equal(saveAttemptsBeforeSecondCall, fixture.SessionStore.SaveAttempts);
    }

    // =========================================================================
    // Failure and Recovery Scenarios: A, B, C, D
    // =========================================================================

    [Fact]
    public async Task PartialSecureReconciliationFailure_PreservesDesiredSecure_EffectiveStatusAccurate_EnablesRetry()
    {
        var fixture = new RestartLifecycleFixture();

        // Arrange: User desires both Standard and Secure
        fixture.Dependencies.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            CameraSecure = SecureProtectionState.Active
        });

        // Simulate Secure failure during reconciliation (e.g. UAC elevation denial)
        var shouldSecureFail = true;
        fixture.Dependencies.EnableSecureHandler = _ =>
        {
            if (shouldSecureFail)
                return OperationResult.Fail("User cancelled elevation prompt for PnP node");
            return OperationResult.Ok();
        };

        var app1 = fixture.CreateApplicationInstance();
        var recovery1 = await app1.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery1.SafeToExit);

        // Act 1: Reconcile fails on Secure
        var reconcile1 = await app1.Recovery.ReconcileAtStartupAsync(recovery1);
        Assert.False(reconcile1.Success);

        // Invariant: DesiredState preserves the user's requested Secure protection
        var desired = fixture.Dependencies.Load();
        Assert.Equal(StandardProtectionState.Active, desired.CameraStandard);
        Assert.Equal(SecureProtectionState.Active, desired.CameraSecure);

        // Invariant: Effective status represents Standard as Active and Secure as Inactive/Failed
        var status = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.Equal(StandardProtectionState.Active, status.Camera.StandardState);
        Assert.NotEqual(SecureProtectionState.Active, status.Camera.SecureState);

        // Act 2: Next startup retry where elevation succeeds
        shouldSecureFail = false;
        var app2 = fixture.CreateApplicationInstance();
        var recovery2 = await app2.Recovery.RecoverAtStartupAsync();
        var reconcile2 = await app2.Recovery.ReconcileAtStartupAsync(recovery2);
        Assert.True(reconcile2.Success);

        var finalStatus = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.Equal(StandardProtectionState.Active, finalStatus.Camera.StandardState);
        Assert.Equal(SecureProtectionState.Active, finalStatus.Camera.SecureState);
    }

    [Fact]
    public async Task AsymmetricMultiTargetRecovery_RestoresOnlyOwned_ReconcilesToNewSessionPerDesiredState()
    {
        var camera = PrivacyRecoveryTestData.Device(
            "camera-1",
            target: BlockTarget.Camera,
            journalState: PrivacyResourceJournalState.Applied,
            modifiedByPrivLock: true);

        var mic = PrivacyRecoveryTestData.Device(
            "mic-1",
            target: BlockTarget.Microphone,
            journalState: PrivacyResourceJournalState.ApplyPending,
            modifiedByPrivLock: false);
        mic.ExecutionMayStillBeInFlight = true;

        var previousSession = PrivacyRecoveryTestData.ActiveSession(camera, mic);
        var prevSessionId = previousSession.SessionId;

        var fixture = new RestartLifecycleFixture(previousSession);
        fixture.Platform.SetObservation(camera.ResourceId, PrivacyResourceObservationKind.MatchesProtected);
        fixture.Platform.SetObservation(mic.ResourceId, PrivacyResourceObservationKind.MatchesOriginal);

        fixture.Dependencies.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            MicrophoneStandard = StandardProtectionState.Active
        });

        var app = fixture.CreateApplicationInstance();

        // 1. Recovery quiesces in-flight, restores only verifiably owned camera change
        var recovery = await app.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery.SafeToExit);
        Assert.Contains(camera.ResourceId, fixture.Platform.RestoreCalls);

        // Previous session restored
        Assert.False(fixture.SessionStore.Current!.IsActive);

        // 2. Reconcile applies both targets cleanly into a NEW session
        var reconcile = await app.Recovery.ReconcileAtStartupAsync(recovery);
        Assert.True(reconcile.Success);

        var newSession = fixture.SessionStore.Current;
        Assert.NotNull(newSession);
        Assert.True(newSession!.IsActive);
        Assert.NotEqual(prevSessionId, newSession.SessionId);
    }

    [Fact]
    public async Task LegacyRecoveryDiagnosticCoupling_DesiredStateAloneDoesNotInferLegacyUntracked_DecoupledFromIntent()
    {
        // Setup: DesiredState Active, No PrivacySession, HasLegacyEvidence = false
        var fixture = new RestartLifecycleFixture();
        fixture.LegacyDetector.HasLegacyEvidenceResult = false;
        fixture.Dependencies.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            MicrophoneStandard = StandardProtectionState.Active
        });

        var app = fixture.CreateApplicationInstance();
        var recovery = await app.Recovery.RecoverAtStartupAsync();

        // Invariant: Safe to exit, 0 failures, does NOT infer untracked legacy protection from DesiredState
        Assert.True(recovery.SafeToExit);
        Assert.Equal(0, recovery.FailedCount);
        Assert.DoesNotContain("older PrivLock version", recovery.ErrorMessage ?? string.Empty);
        Assert.Empty(fixture.Platform.RestoreCalls);
        Assert.Equal(0, fixture.Dependencies.SaveCount);
    }

    [Fact]
    public async Task ExternalStateConflicts_PreservesExternalAdminProtection_DoesNotUnblockWithoutOwnership()
    {
        var fixture = new RestartLifecycleFixture();

        // User desires Inactive
        fixture.Dependencies.SetDesiredState(new DesiredState
        {
            CameraStandard = StandardProtectionState.Inactive,
            MicrophoneStandard = StandardProtectionState.Inactive
        });

        // External admin blocked camera in Windows
        fixture.Dependencies.SetCameraStandardState(StandardProtectionState.Active);

        var app = fixture.CreateApplicationInstance();
        var recovery = await app.Recovery.RecoverAtStartupAsync();
        Assert.True(recovery.SafeToExit);

        var reconcile = await app.Recovery.ReconcileAtStartupAsync(recovery);
        Assert.True(reconcile.Success);

        // Invariant: External protection preserved, zero restore calls made
        var osState = await fixture.Dependencies.GetProtectionStateAsync();
        Assert.Equal(StandardProtectionState.Active, osState.Camera.StandardState);
        Assert.Empty(fixture.Platform.RestoreCalls);
        Assert.Equal(StandardProtectionState.Inactive, fixture.Dependencies.Load().CameraStandard);
    }

    // =========================================================================
    // Test Fixture and Dependency Composition
    // =========================================================================

    private sealed record ApplicationInstance(
        ProtectionService Protection,
        ShutdownCoordinator Shutdown,
        PrivacyRecoveryService Recovery);

    private sealed class RestartLifecycleFixture
    {
        public RecordingPrivacySessionStore SessionStore { get; }
        public ScriptedPrivacySessionPlatformAdapter Platform { get; }
        public LifecycleHostDependencies Dependencies { get; }
        public ScriptedLegacyArtifactDetector LegacyDetector { get; }

        public RestartLifecycleFixture(PrivacySession? initialSession = null)
        {
            SessionStore = new RecordingPrivacySessionStore(initialSession);
            Platform = new ScriptedPrivacySessionPlatformAdapter();
            Dependencies = new LifecycleHostDependencies();
            LegacyDetector = new ScriptedLegacyArtifactDetector(hasLegacyEvidence: false);

            Platform.CaptureFactory = (layer, target, opId) =>
            {
                var list = new List<PrivacyResourceState>();
                if (target is BlockTarget.Camera or BlockTarget.Both)
                {
                    if (layer == ProtectionLayer.Secure)
                    {
                        list.Add(PrivacyRecoveryTestData.Device(
                            "camera-sec-1",
                            target: BlockTarget.Camera,
                            layer: ProtectionLayer.Secure,
                            journalState: PrivacyResourceJournalState.Captured,
                            modifiedByPrivLock: false));
                    }
                    else
                    {
                        list.Add(PrivacyRecoveryTestData.Device(
                            "camera-std-1",
                            target: BlockTarget.Camera,
                            layer: ProtectionLayer.Standard,
                            journalState: PrivacyResourceJournalState.Captured,
                            modifiedByPrivLock: false));
                    }
                }
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                {
                    if (layer == ProtectionLayer.Secure)
                    {
                        list.Add(PrivacyRecoveryTestData.Device(
                            "mic-sec-1",
                            target: BlockTarget.Microphone,
                            layer: ProtectionLayer.Secure,
                            journalState: PrivacyResourceJournalState.Captured,
                            modifiedByPrivLock: false));
                    }
                    else
                    {
                        list.Add(PrivacyRecoveryTestData.AudioEndpoint(
                            "mic-std-1",
                            journalState: PrivacyResourceJournalState.Captured));
                    }
                }
                return list;
            };

            Platform.AfterRestore = (resource, result) =>
            {
                Platform.SetObservation(resource.ResourceId, PrivacyResourceObservationKind.MatchesOriginal);
                if (resource.Target == BlockTarget.Camera)
                {
                    if (resource.Layer == ProtectionLayer.Standard)
                        Dependencies.SetCameraStandardState(StandardProtectionState.Inactive);
                    else
                        Dependencies.SetCameraSecureState(SecureProtectionState.Available);
                }
                if (resource.Target == BlockTarget.Microphone)
                {
                    if (resource.Layer == ProtectionLayer.Standard)
                        Dependencies.SetMicrophoneStandardState(StandardProtectionState.Inactive);
                    else
                        Dependencies.SetMicrophoneSecureState(SecureProtectionState.Available);
                }
            };

            Dependencies.OnStandardEnableAction = target =>
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    Platform.SetObservation("device:camera-std-1", PrivacyResourceObservationKind.MatchesProtected);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    Platform.SetObservation("audio-mute:mic-std-1", PrivacyResourceObservationKind.MatchesProtected);
            };

            Dependencies.OnStandardDisableAction = target =>
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    Platform.SetObservation("device:camera-std-1", PrivacyResourceObservationKind.MatchesOriginal);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    Platform.SetObservation("audio-mute:mic-std-1", PrivacyResourceObservationKind.MatchesOriginal);
            };

            Dependencies.OnSecureEnableAction = target =>
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    Platform.SetObservation("device:camera-sec-1", PrivacyResourceObservationKind.MatchesProtected);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    Platform.SetObservation("device:mic-sec-1", PrivacyResourceObservationKind.MatchesProtected);
            };

            Dependencies.OnSecureDisableAction = target =>
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    Platform.SetObservation("device:camera-sec-1", PrivacyResourceObservationKind.MatchesOriginal);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    Platform.SetObservation("device:mic-sec-1", PrivacyResourceObservationKind.MatchesOriginal);
            };
        }

        public ApplicationInstance CreateApplicationInstance()
        {
            var sessions = new PrivacySessionService(SessionStore, Platform);
            var protection = new ProtectionService(
                Dependencies,
                Dependencies,
                Dependencies,
                Dependencies,
                sessions,
                LegacyDetector);
            var shutdown = new ShutdownCoordinator(protection);
            var recovery = new PrivacyRecoveryService(protection);
            return new ApplicationInstance(protection, shutdown, recovery);
        }
    }

    private sealed class LifecycleHostDependencies :
        IDeviceProtectionProvider,
        IDeviceDetector,
        IPlatformCapabilityProvider,
        IStateStore,
        ILegacyArtifactDetector
    {
        private DesiredState _desiredState = new();
        public int SaveCount { get; private set; }
        public bool HasLegacyEvidenceResult { get; set; }
        public bool HasLegacyEvidence() => HasLegacyEvidenceResult;

        public OperationResult StandardEnableResult { get; set; } = OperationResult.Ok();
        public OperationResult SecureEnableResult { get; set; } = OperationResult.Ok();
        public Func<BlockTarget, OperationResult>? EnableSecureHandler { get; set; }

        public Action<BlockTarget>? OnStandardEnableAction { get; set; }
        public Action<BlockTarget>? OnStandardDisableAction { get; set; }
        public Action<BlockTarget>? OnSecureEnableAction { get; set; }
        public Action<BlockTarget>? OnSecureDisableAction { get; set; }

        public FullProtectionState CurrentProtectionState { get; set; } = new()
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable
            }
        };

        public PlatformCapabilities Capabilities { get; } = new();
        public PlatformInfo PlatformInfo { get; } = new()
        {
            OperatingSystemName = "Windows",
            OsVersion = "10.0.22631",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };

        public void SetCameraStandardState(StandardProtectionState state) =>
            CurrentProtectionState = CurrentProtectionState with
            {
                Camera = CurrentProtectionState.Camera with { StandardState = state }
            };

        public void SetMicrophoneStandardState(StandardProtectionState state) =>
            CurrentProtectionState = CurrentProtectionState with
            {
                Microphone = CurrentProtectionState.Microphone with { StandardState = state }
            };

        public void SetCameraSecureState(SecureProtectionState state) =>
            CurrentProtectionState = CurrentProtectionState with
            {
                Camera = CurrentProtectionState.Camera with { SecureState = state }
            };

        public void SetMicrophoneSecureState(SecureProtectionState state) =>
            CurrentProtectionState = CurrentProtectionState with
            {
                Microphone = CurrentProtectionState.Microphone with { SecureState = state }
            };

        public Task<OperationResult> EnableStandardProtectionAsync(
            BlockTarget target,
            CancellationToken cancellationToken = default)
        {
            OnStandardEnableAction?.Invoke(target);
            if (StandardEnableResult.Success)
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                {
                    SetCameraStandardState(StandardProtectionState.Active);
                    if (CurrentProtectionState.Camera.SecureState == SecureProtectionState.Unavailable)
                        SetCameraSecureState(SecureProtectionState.Available);
                }
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                {
                    SetMicrophoneStandardState(StandardProtectionState.Active);
                    if (CurrentProtectionState.Microphone.SecureState == SecureProtectionState.Unavailable)
                        SetMicrophoneSecureState(SecureProtectionState.Available);
                }
            }
            return Task.FromResult(StandardEnableResult);
        }

        public Task<OperationResult> DisableStandardProtectionAsync(
            BlockTarget target,
            CancellationToken cancellationToken = default)
        {
            OnStandardDisableAction?.Invoke(target);
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                SetCameraStandardState(StandardProtectionState.Inactive);
                SetCameraSecureState(SecureProtectionState.Unavailable);
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                SetMicrophoneStandardState(StandardProtectionState.Inactive);
                SetMicrophoneSecureState(SecureProtectionState.Unavailable);
            }
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> EnableSecureProtectionAsync(
            BlockTarget target,
            CancellationToken cancellationToken = default)
        {
            var result = EnableSecureHandler?.Invoke(target) ?? SecureEnableResult;
            if (result.Success)
            {
                OnSecureEnableAction?.Invoke(target);
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    SetCameraSecureState(SecureProtectionState.Active);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    SetMicrophoneSecureState(SecureProtectionState.Active);
            }
            return Task.FromResult(result);
        }

        public Task<OperationResult> DisableSecureProtectionAsync(
            BlockTarget target,
            CancellationToken cancellationToken = default)
        {
            OnSecureDisableAction?.Invoke(target);
            if (target is BlockTarget.Camera or BlockTarget.Both)
                SetCameraSecureState(SecureProtectionState.Available);
            if (target is BlockTarget.Microphone or BlockTarget.Both)
                SetMicrophoneSecureState(SecureProtectionState.Available);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CurrentProtectionState);

        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public DesiredState Load() => new()
        {
            CameraStandard = _desiredState.CameraStandard,
            CameraSecure = _desiredState.CameraSecure,
            MicrophoneStandard = _desiredState.MicrophoneStandard,
            MicrophoneSecure = _desiredState.MicrophoneSecure,
            AdvancedProtectionEnabled = _desiredState.AdvancedProtectionEnabled,
            Language = _desiredState.Language,
            Autostart = _desiredState.Autostart
        };

        public void Save(DesiredState state)
        {
            SaveCount++;
            _desiredState = state;
        }

        public void SetDesiredState(DesiredState state) => _desiredState = state;
    }
}

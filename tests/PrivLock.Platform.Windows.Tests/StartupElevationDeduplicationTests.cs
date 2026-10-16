using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.Privileged;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

/// <summary>
/// Verifies the startup elevation deduplication architecture (Tests 1 through 9).
/// Ensures that during startup, at most a single elevated worker process is launched
/// (ElevationLaunchCount &lt;= 1) and that Camera and Microphone share that single worker.
/// </summary>
public sealed class StartupElevationDeduplicationTests : IDisposable
{
    private readonly string _testRootDirectory;
    private readonly string _stateDirectory;
    private readonly string _recoveryDirectory;
    private readonly WindowsPrivilegedSession _testSession;

    public StartupElevationDeduplicationTests()
    {
        var testId = Guid.NewGuid().ToString("N");
        _testRootDirectory = Path.Combine(Path.GetTempPath(), $"PrivGvardElevationTests_{testId}");
        _stateDirectory = Path.Combine(_testRootDirectory, "Data");
        _recoveryDirectory = Path.Combine(_testRootDirectory, "Recovery");

        Directory.CreateDirectory(_stateDirectory);
        Directory.CreateDirectory(_recoveryDirectory);

        _testSession = new WindowsPrivilegedSession
        {
            CustomWorkerLauncher = () => Task.CompletedTask,
            CustomCommandExecutor = (cmd, arg) => Task.FromResult(OperationResult.Ok())
        };
        WindowsPrivilegedSession.SetTestOverrideInstance(_testSession);
    }

    public void Dispose()
    {
        WindowsPrivilegedSession.SetTestOverrideInstance(null);
        _testSession.Dispose();

        try
        {
            if (Directory.Exists(_testRootDirectory))
                Directory.Delete(_testRootDirectory, recursive: true);
        }
        catch { }
    }

    // =========================================================================
    // TEST 1: Startup sin Advanced Protection -> ElevationLaunchCount == 0
    // =========================================================================
    [Fact]
    public async Task Test1_StartupWithoutAdvancedProtection_ZeroElevationLaunches()
    {
        var env = CreateEnvironment();
        // Configure standard-only protection in DesiredState
        var desired = env.StateStore.Load();
        desired.CameraStandard = StandardProtectionState.Active;
        desired.MicrophoneStandard = StandardProtectionState.Active;
        desired.AdvancedProtectionEnabled = false;
        desired.CameraSecure = SecureProtectionState.Available;
        desired.MicrophoneSecure = SecureProtectionState.Available;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            Assert.True(recovery.SafeToExit);

            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);
        }

        Assert.Equal(0, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 2: Startup con Camera Advanced -> ElevationLaunchCount == 1
    // =========================================================================
    [Fact]
    public async Task Test2_StartupWithCameraAdvanced_SingleElevationLaunch()
    {
        var env = CreateEnvironment();
        var desired = env.StateStore.Load();
        desired.CameraStandard = StandardProtectionState.Active;
        desired.CameraSecure = SecureProtectionState.Active;
        desired.AdvancedProtectionEnabled = true;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            Assert.True(recovery.SafeToExit);

            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);
        }

        Assert.Equal(1, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 3: Startup con Microphone Advanced -> ElevationLaunchCount == 1
    // =========================================================================
    [Fact]
    public async Task Test3_StartupWithMicrophoneAdvanced_SingleElevationLaunch()
    {
        var env = CreateEnvironment();
        var desired = env.StateStore.Load();
        desired.MicrophoneStandard = StandardProtectionState.Active;
        desired.MicrophoneSecure = SecureProtectionState.Active;
        desired.AdvancedProtectionEnabled = true;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            Assert.True(recovery.SafeToExit);

            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);
        }

        Assert.Equal(1, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 4: Startup con Camera + Microphone Advanced -> ElevationLaunchCount == 1
    // =========================================================================
    [Fact]
    public async Task Test4_StartupWithCameraAndMicrophoneAdvanced_ReusesSingleWorker()
    {
        var env = CreateEnvironment();
        var desired = env.StateStore.Load();
        desired.CameraStandard = StandardProtectionState.Active;
        desired.CameraSecure = SecureProtectionState.Active;
        desired.MicrophoneStandard = StandardProtectionState.Active;
        desired.MicrophoneSecure = SecureProtectionState.Active;
        desired.AdvancedProtectionEnabled = true;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            Assert.True(recovery.SafeToExit);

            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);
        }

        // Camera Secure and Microphone Secure MUST have shared the exact same worker!
        Assert.Equal(1, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 5: Startup con recovery + reconciliation + Advanced -> máximo 1 worker
    // =========================================================================
    [Fact]
    public async Task Test5_StartupWithRecoveryAndReconciliation_SharesSingleWorker()
    {
        var env = CreateEnvironment();
        // Seed an unfinished active session in WAL containing secure resources
        var session = new PrivacySession
        {
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            OwnerProcessId = 99999, // From prior process before reboot
            IsActive = true,
            LastOperationId = "Op-Prior",
            Status = PrivacySessionStatus.Active,
            Resources =
            [
                new OriginalPolicyState
                {
                    ResourceId = DeduplicationPlatformAdapter.CameraPolicyResourceId,
                    RegistryHive = PrivacyRegistryHive.LocalMachine,
                    RegistryView = PrivacyRegistryView.Default,
                    RegistryPath = DeduplicationPlatformAdapter.CameraPolicyPath,
                    ValueName = "LetAppsAccessCamera",
                    OperationId = "Op-Prior",
                    Layer = ProtectionLayer.Secure,
                    Target = BlockTarget.Camera,
                    CapturedAtUtc = DateTimeOffset.UtcNow,
                    LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                    JournalState = PrivacyResourceJournalState.Applied,
                    ModifiedByPrivLock = true,
                    OwnershipUncertain = false,
                    ValueExisted = true,
                    OriginalValueKind = PrivacyRegistryValueKind.DWord,
                    OriginalValue = "0",
                    ProtectedValueExists = true,
                    ProtectedValueKind = PrivacyRegistryValueKind.DWord,
                    ProtectedValue = "2"
                },
                new OriginalPolicyState
                {
                    ResourceId = DeduplicationPlatformAdapter.MicPolicyResourceId,
                    RegistryHive = PrivacyRegistryHive.LocalMachine,
                    RegistryView = PrivacyRegistryView.Default,
                    RegistryPath = DeduplicationPlatformAdapter.CameraPolicyPath,
                    ValueName = "LetAppsAccessMicrophone",
                    OperationId = "Op-Prior",
                    Layer = ProtectionLayer.Secure,
                    Target = BlockTarget.Microphone,
                    CapturedAtUtc = DateTimeOffset.UtcNow,
                    LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                    JournalState = PrivacyResourceJournalState.Applied,
                    ModifiedByPrivLock = true,
                    OwnershipUncertain = false,
                    ValueExisted = true,
                    OriginalValueKind = PrivacyRegistryValueKind.DWord,
                    OriginalValue = "0",
                    ProtectedValueExists = true,
                    ProtectedValueKind = PrivacyRegistryValueKind.DWord,
                    ProtectedValue = "2"
                }
            ]
        };
        env.SessionStore.Save(session);
        env.PlatformAdapter.SetProtected(DeduplicationPlatformAdapter.CameraPolicyResourceId);
        env.PlatformAdapter.SetProtected(DeduplicationPlatformAdapter.MicPolicyResourceId);

        // DesiredState also requires Advanced protection
        var desired = env.StateStore.Load();
        desired.CameraStandard = StandardProtectionState.Active;
        desired.CameraSecure = SecureProtectionState.Active;
        desired.MicrophoneStandard = StandardProtectionState.Active;
        desired.MicrophoneSecure = SecureProtectionState.Active;
        desired.AdvancedProtectionEnabled = true;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            Assert.True(recovery.SafeToExit);
            Assert.True(recovery.HadRecoveryWork);

            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);
        }

        // Entire startup lifecycle (recovery of WAL + reconciliation of desired state)
        // must not exceed 1 elevated worker launch!
        Assert.Equal(1, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 6: Después de terminar startup -> IsSessionActive == false, worker cerrado
    // =========================================================================
    [Fact]
    public async Task Test6_AfterStartupCompletes_SessionIsInactiveAndWorkerClosed()
    {
        var env = CreateEnvironment();
        var desired = env.StateStore.Load();
        desired.CameraStandard = StandardProtectionState.Active;
        desired.CameraSecure = SecureProtectionState.Active;
        desired.AdvancedProtectionEnabled = true;
        env.StateStore.Save(desired);

        var app = env.CreateInstance();

        using (app.RecoveryService.BeginStartupPrivilegeScope())
        {
            var recovery = await app.RecoveryService.RecoverAtStartupAsync();
            var reconcile = await app.RecoveryService.ReconcileAtStartupAsync(recovery);
            Assert.True(reconcile.Success);

            // While within startup scope and after privileged work, worker is active
            Assert.True(_testSession.IsSessionActive);
        }

        // Immediately upon disposing the startup scope:
        Assert.False(_testSession.IsSessionActive);
        Assert.False(_testSession.IsDeferredScopeActive);
    }

    // =========================================================================
    // TEST 7: Operación manual normal -> crea worker bajo demanda, lo libera al terminar
    // =========================================================================
    [Fact]
    public async Task Test7_NormalManualOperation_ReleasesWorkerImmediatelyAfterPass()
    {
        var env = CreateEnvironment();
        var app = env.CreateInstance();

        // 1. Initially inactive
        Assert.False(_testSession.IsSessionActive);
        Assert.Equal(0, _testSession.ElevationLaunchCount);

        // 2. Enable standard first
        var stdCam = await app.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(stdCam.Success);
        Assert.Equal(0, _testSession.ElevationLaunchCount);

        // 3. User manually enables Secure protection (no startup scope active)
        var result = await app.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Camera);
        Assert.True(result.Success);

        // Worker was launched for this operation
        Assert.Equal(1, _testSession.ElevationLaunchCount);

        // And immediately released/closed upon operation pass completion!
        Assert.False(_testSession.IsSessionActive);

        // 4. Second manual operation for microphone
        var stdMic = await app.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(stdMic.Success);
        var resultMic = await app.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Microphone);
        Assert.True(resultMic.Success);

        // Second independent operation launched on demand
        Assert.Equal(2, _testSession.ElevationLaunchCount);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 8: Force quiescence -> cierra worker incluso dentro de un scope activo
    // =========================================================================
    [Fact]
    public async Task Test8_ForceQuiescence_TerminatesWorkerEvenInsideActiveScope()
    {
        using (var scope = _testSession.BeginDeferredScope())
        {
            Assert.True(_testSession.IsDeferredScopeActive);

            // Trigger a privileged command
            await _testSession.ExecuteCommandAsync("apply-policy", "camera");
            Assert.True(_testSession.IsSessionActive);
            Assert.Equal(1, _testSession.ElevationLaunchCount);

            // ReleaseSession() within scope retains the session
            _testSession.ReleaseSession();
            Assert.True(_testSession.IsSessionActive);

            // But CloseSession() (force quiescence) unconditionally closes the worker!
            _testSession.CloseSession();
            Assert.False(_testSession.IsSessionActive);

            // Scope is still active, but worker was killed for quiescence
            Assert.True(_testSession.IsDeferredScopeActive);
        }

        Assert.False(_testSession.IsDeferredScopeActive);
        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // TEST 9: Worker muere inesperadamente -> se recupera con nueva sesión elevada
    // =========================================================================
    [Fact]
    public async Task Test9_UnexpectedWorkerExit_RecoversWithNewElevatedSession()
    {
        using (var scope = _testSession.BeginDeferredScope())
        {
            // First privileged command launches worker 1
            await _testSession.ExecuteCommandAsync("apply-policy", "camera");
            Assert.Equal(1, _testSession.ElevationLaunchCount);
            Assert.True(_testSession.IsSessionActive);

            // Simulate sudden worker process exit (crash / pipe break)
            _testSession.SimulateWorkerExit();
            Assert.False(_testSession.IsSessionActive);

            // Next command recovers by launching a replacement worker
            await _testSession.ExecuteCommandAsync("apply-policy", "microphone");
            Assert.Equal(2, _testSession.ElevationLaunchCount);
            Assert.True(_testSession.IsSessionActive);
        }

        Assert.False(_testSession.IsSessionActive);
    }

    // =========================================================================
    // Test Environment Helpers
    // =========================================================================

    private TestEnvironment CreateEnvironment()
    {
        var stateStore = new FileStateStore(_stateDirectory);
        var sessionStore = new FilePrivacySessionStore(_recoveryDirectory);
        var adapter = new DeduplicationPlatformAdapter();
        var provider = new DeduplicationProtectionProvider(adapter);

        return new TestEnvironment(stateStore, sessionStore, adapter, provider);
    }

    private sealed record ApplicationInstance(
        ProtectionService ProtectionService,
        PrivacyRecoveryService RecoveryService);

    private sealed class TestEnvironment
    {
        public FileStateStore StateStore { get; }
        public FilePrivacySessionStore SessionStore { get; }
        public DeduplicationPlatformAdapter PlatformAdapter { get; }
        public DeduplicationProtectionProvider ProtectionProvider { get; }

        public TestEnvironment(
            FileStateStore stateStore,
            FilePrivacySessionStore sessionStore,
            DeduplicationPlatformAdapter platformAdapter,
            DeduplicationProtectionProvider protectionProvider)
        {
            StateStore = stateStore;
            SessionStore = sessionStore;
            PlatformAdapter = platformAdapter;
            ProtectionProvider = protectionProvider;
        }

        public ApplicationInstance CreateInstance()
        {
            var capabilityProvider = new DeduplicationCapabilityProvider();
            var detector = new DeduplicationDetector();
            var privacySessions = new PrivacySessionService(SessionStore, PlatformAdapter);
            var protection = new ProtectionService(
                ProtectionProvider,
                detector,
                capabilityProvider,
                StateStore,
                privacySessions);
            var recovery = new PrivacyRecoveryService(protection);
            return new ApplicationInstance(protection, recovery);
        }
    }

    public sealed class DeduplicationPlatformAdapter : IPrivacySessionPlatformAdapter
    {
        public const string CameraConsentPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam";
        public const string CameraConsentResourceId = "registry:CurrentUser:Default:Software\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\webcam:Value";
        public const string CameraPolicyPath = @"Software\Policies\Microsoft\Windows\AppPrivacy";
        public const string CameraPolicyResourceId = "registry:LocalMachine:Default:Software\\Policies\\Microsoft\\Windows\\AppPrivacy:LetAppsAccessCamera";
        public const string MicMuteResourceId = "audio-mute:mic-endpoint-1";
        public const string MicPolicyResourceId = "registry:LocalMachine:Default:Software\\Policies\\Microsoft\\Windows\\AppPrivacy:LetAppsAccessMicrophone";

        private readonly Dictionary<string, PrivacyResourceObservation> _observations = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new();

        public bool SupportsPersistentRecovery => true;

        public void SetObservation(string resourceId, PrivacyResourceObservationKind kind)
        {
            lock (_sync)
            {
                _observations[resourceId] = new PrivacyResourceObservation(kind);
            }
        }

        public void SetProtected(string resourceId) =>
            SetObservation(resourceId, PrivacyResourceObservationKind.MatchesProtected);

        public void SetOriginal(string resourceId) =>
            SetObservation(resourceId, PrivacyResourceObservationKind.MatchesOriginal);

        public Task<OperationResult> EnsureMutationQuiescenceAsync(CancellationToken cancellationToken = default)
        {
            WindowsPrivilegedSession.Instance.CloseSession();
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<IReadOnlyList<PrivacyResourceState>> CaptureAsync(
            ProtectionLayer layer,
            BlockTarget target,
            string operationId,
            CancellationToken cancellationToken = default)
        {
            var list = new List<PrivacyResourceState>();
            if (layer == ProtectionLayer.Standard)
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                {
                    list.Add(new OriginalPolicyState
                    {
                        ResourceId = CameraConsentResourceId,
                        RegistryHive = PrivacyRegistryHive.CurrentUser,
                        RegistryView = PrivacyRegistryView.Default,
                        RegistryPath = CameraConsentPath,
                        ValueName = "Value",
                        OperationId = operationId,
                        Layer = ProtectionLayer.Standard,
                        Target = BlockTarget.Camera,
                        CapturedAtUtc = DateTimeOffset.UtcNow,
                        LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                        JournalState = PrivacyResourceJournalState.Captured,
                        ModifiedByPrivLock = false,
                        OwnershipUncertain = false,
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
                        OwnershipUncertain = false,
                        EndpointId = "mic-endpoint-1",
                        OriginalMutedState = false,
                        ProtectedMutedState = true
                    });
                }
            }
            else if (layer == ProtectionLayer.Secure)
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                {
                    list.Add(new OriginalPolicyState
                    {
                        ResourceId = CameraPolicyResourceId,
                        RegistryHive = PrivacyRegistryHive.LocalMachine,
                        RegistryView = PrivacyRegistryView.Default,
                        RegistryPath = CameraPolicyPath,
                        ValueName = "LetAppsAccessCamera",
                        OperationId = operationId,
                        Layer = ProtectionLayer.Secure,
                        Target = BlockTarget.Camera,
                        CapturedAtUtc = DateTimeOffset.UtcNow,
                        LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                        JournalState = PrivacyResourceJournalState.Captured,
                        ModifiedByPrivLock = false,
                        OwnershipUncertain = false,
                        ValueExisted = true,
                        OriginalValueKind = PrivacyRegistryValueKind.DWord,
                        OriginalValue = "0",
                        ProtectedValueExists = true,
                        ProtectedValueKind = PrivacyRegistryValueKind.DWord,
                        ProtectedValue = "2"
                    });
                }
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                {
                    list.Add(new OriginalPolicyState
                    {
                        ResourceId = MicPolicyResourceId,
                        RegistryHive = PrivacyRegistryHive.LocalMachine,
                        RegistryView = PrivacyRegistryView.Default,
                        RegistryPath = CameraPolicyPath,
                        ValueName = "LetAppsAccessMicrophone",
                        OperationId = operationId,
                        Layer = ProtectionLayer.Secure,
                        Target = BlockTarget.Microphone,
                        CapturedAtUtc = DateTimeOffset.UtcNow,
                        LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                        JournalState = PrivacyResourceJournalState.Captured,
                        ModifiedByPrivLock = false,
                        OwnershipUncertain = false,
                        ValueExisted = true,
                        OriginalValueKind = PrivacyRegistryValueKind.DWord,
                        OriginalValue = "0",
                        ProtectedValueExists = true,
                        ProtectedValueKind = PrivacyRegistryValueKind.DWord,
                        ProtectedValue = "2"
                    });
                }
            }

            return Task.FromResult<IReadOnlyList<PrivacyResourceState>>(list);
        }

        public Task<PrivacyResourceObservation> ObserveAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var obs = _observations.TryGetValue(resource.ResourceId, out var custom)
                    ? custom
                    : new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal);
                return Task.FromResult(obs);
            }
        }

        public async Task<PrivacyOwnershipAttestation> VerifyOwnershipAttestationAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            var result = await WindowsPrivilegedSession.Instance.ExecuteCommandAsync("verify-policy-ownership", resource.ResourceId);
            return result.Success
                ? new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.Confirmed)
                : new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.NotConfirmed);
        }

        public async Task<OperationResult> RestoreOriginalAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            var result = await WindowsPrivilegedSession.Instance.ExecuteCommandAsync("restore-policy", resource.ResourceId);
            if (result.Success)
            {
                lock (_sync)
                {
                    _observations[resource.ResourceId] = new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal);
                }
            }
            return result;
        }

        public IDisposable BeginDeferredElevationScope() =>
            WindowsPrivilegedSession.Instance.BeginDeferredScope();

        public void CompleteRecoveryPass() =>
            WindowsPrivilegedSession.Instance.ReleaseSession();

        public void SuppressRecoveryPassCompletion() =>
            WindowsPrivilegedSession.Instance.BeginDeferredScope();

        public void ResumeRecoveryPassCompletion() =>
            WindowsPrivilegedSession.Instance.ReleaseSession();
    }

    public sealed class DeduplicationProtectionProvider : IDeviceProtectionProvider
    {
        private readonly DeduplicationPlatformAdapter _adapter;
        private StandardProtectionState _cameraStandard = StandardProtectionState.Inactive;
        private StandardProtectionState _micStandard = StandardProtectionState.Inactive;
        private SecureProtectionState _cameraSecure = SecureProtectionState.Available;
        private SecureProtectionState _micSecure = SecureProtectionState.Available;

        public DeduplicationProtectionProvider(DeduplicationPlatformAdapter adapter)
        {
            _adapter = adapter;
        }

        public void SetCameraStandard(StandardProtectionState state)
        {
            _cameraStandard = state;
            if (state == StandardProtectionState.Active)
                _adapter.SetProtected(DeduplicationPlatformAdapter.CameraConsentResourceId);
            else
                _adapter.SetOriginal(DeduplicationPlatformAdapter.CameraConsentResourceId);
        }

        public void SetMicrophoneStandard(StandardProtectionState state)
        {
            _micStandard = state;
            if (state == StandardProtectionState.Active)
                _adapter.SetProtected(DeduplicationPlatformAdapter.MicMuteResourceId);
            else
                _adapter.SetOriginal(DeduplicationPlatformAdapter.MicMuteResourceId);
        }

        public void SetCameraSecure(SecureProtectionState state)
        {
            _cameraSecure = state;
            if (state == SecureProtectionState.Active)
                _adapter.SetProtected(DeduplicationPlatformAdapter.CameraPolicyResourceId);
            else
                _adapter.SetOriginal(DeduplicationPlatformAdapter.CameraPolicyResourceId);
        }

        public void SetMicrophoneSecure(SecureProtectionState state)
        {
            _micSecure = state;
            if (state == SecureProtectionState.Active)
                _adapter.SetProtected(DeduplicationPlatformAdapter.MicPolicyResourceId);
            else
                _adapter.SetOriginal(DeduplicationPlatformAdapter.MicPolicyResourceId);
        }

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = _cameraStandard,
                    SecureState = _cameraSecure,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = _micStandard,
                    SecureState = _micSecure,
                    IsVerified = true
                }
            });

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            if (target is BlockTarget.Camera or BlockTarget.Both)
                SetCameraStandard(StandardProtectionState.Active);
            if (target is BlockTarget.Microphone or BlockTarget.Both)
                SetMicrophoneStandard(StandardProtectionState.Active);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            if (target is BlockTarget.Camera or BlockTarget.Both)
                SetCameraStandard(StandardProtectionState.Inactive);
            if (target is BlockTarget.Microphone or BlockTarget.Both)
                SetMicrophoneStandard(StandardProtectionState.Inactive);
            return Task.FromResult(OperationResult.Ok());
        }

        public async Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            var result = await WindowsPrivilegedSession.Instance.ExecuteCommandAsync("apply-policy", target.ToString());
            if (result.Success)
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    SetCameraSecure(SecureProtectionState.Active);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    SetMicrophoneSecure(SecureProtectionState.Active);
            }
            return result;
        }

        public async Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            var result = await WindowsPrivilegedSession.Instance.ExecuteCommandAsync("restore-policy", target.ToString());
            if (result.Success)
            {
                if (target is BlockTarget.Camera or BlockTarget.Both)
                    SetCameraSecure(SecureProtectionState.Available);
                if (target is BlockTarget.Microphone or BlockTarget.Both)
                    SetMicrophoneSecure(SecureProtectionState.Available);
            }
            return result;
        }
    }

    private sealed class DeduplicationCapabilityProvider : IPlatformCapabilityProvider
    {
        public PlatformCapabilities Capabilities => new()
        {
            CameraProtectionLevel = CapabilityLevel.Hardware,
            MicrophoneProtectionLevel = CapabilityLevel.Hardware,
            SupportsAutostart = true,
            SupportsHardwareDisable = true,
            SupportsSystemPolicy = true,
            RequiresElevationForBlock = true
        };

        public PlatformInfo PlatformInfo => new()
        {
            OperatingSystemName = "Windows 11",
            OsVersion = "10.0.22631",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };
    }

    private sealed class DeduplicationDetector : IDeviceDetector
    {
        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);

        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);
    }
}

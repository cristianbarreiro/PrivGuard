using System.Collections.Concurrent;
using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Localization;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using PrivLock.UI.ViewModels;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

/// <summary>
/// Phase 5 — Windows Autostart Lifecycle & Integration Tests.
/// Validates the 10 mandatory autostart scenarios (Test A through Test J):
/// - Test A: Enable Autostart (Authorization, effective readback, preference persistence)
/// - Test B: Disable Autostart (Disables Windows registration, preference update, no accidental re-registration)
/// - Test C: Windows User Disabled (Respects Windows user decision, no forced reactivation, UI diagnostic)
/// - Test D: Windows Policy Disabled (Respects policy restriction, no bypass, diagnostic reported)
/// - Test E: Already Enabled (Idempotent reconciliation, zero duplicate entries)
/// - Test F: Not Registered (Detected correctly, manual startup operates normally)
/// - Test G: Unsupported Environment (Degrades gracefully without crash, reports limitation)
/// - Test H: Single Instance (Autostart does not create duplicate instances, guard maintained)
/// - Test I: Startup Recovery Integration (Recovery precedes DesiredState reconcile, effective verified)
/// - Test J: Explicit User Disable (Simulated restart does not re-register autostart)
/// </summary>
public sealed class WindowsAutostartLifecycleTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _dummyExePath;

    public WindowsAutostartLifecycleTests()
    {
        var testId = Guid.NewGuid().ToString("N");
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PrivGuardAutostartLifecycle_{testId}");
        Directory.CreateDirectory(_testDirectory);

        _dummyExePath = Path.Combine(_testDirectory, "PrivGvard.exe");
        File.WriteAllBytes(_dummyExePath, [0x4D, 0x5A]); // Valid PE header stub
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
    // Test A — Enable Autostart
    // =========================================================================

    [Fact]
    public async Task TestA_EnableAutostart_RequestsAuthorization_InterpretsEffectiveResult_PersistsPreference()
    {
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Disabled)
        {
            StateAfterEnableRequest = WindowsStartupTaskState.Enabled
        };
        var store = new MemoryStateStore();
        var provider = CreatePackagedProvider(startupTask, store);
        var settingsService = new SettingsService(provider, store);

        // Act: User enables Autostart
        var result = await settingsService.SetAutostartAsync(true);

        // Assert: Authorization requested, verified Enabled, preference persisted
        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(1, startupTask.EnableRequestCount);

        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Enabled, status.State);
        Assert.True(status.IsEnabled);

        var persistedState = store.Load();
        Assert.True(persistedState.Autostart);
    }

    // =========================================================================
    // Test B — Disable Autostart
    // =========================================================================

    [Fact]
    public async Task TestB_DisableAutostart_DisablesWindowsRegistration_UpdatesPreference_NoAccidentalReRegistration()
    {
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Enabled)
        {
            StateAfterDisable = WindowsStartupTaskState.Disabled
        };
        var store = new MemoryStateStore(new DesiredState { Autostart = true });
        var provider = CreatePackagedProvider(startupTask, store);
        var settingsService = new SettingsService(provider, store);

        // Act: User disables Autostart
        var result = await settingsService.SetAutostartAsync(false);

        // Assert: Registration disabled, state store updated to false
        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(1, startupTask.DisableCount);

        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Disabled, status.State);
        Assert.False(status.IsEnabled);

        var persistedState = store.Load();
        Assert.False(persistedState.Autostart);

        // Verify calling status again does not accidentally re-register
        var readback = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Disabled, readback.State);
        Assert.Equal(0, startupTask.EnableRequestCount);
    }

    // =========================================================================
    // Test C — Windows User Disabled
    // =========================================================================

    [Fact]
    public async Task TestC_WindowsUserDisabled_RespectsCondition_DoesNotReactivateAutomatically_ReflectsInUI()
    {
        // Setup: Windows reports the startup task was disabled by the user in Windows Settings
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.DisabledByUser);
        var store = new MemoryStateStore(new DesiredState { Autostart = true });
        var provider = CreatePackagedProvider(startupTask, store);
        var settingsService = new SettingsService(provider, store);
        var observer = new FakeLifecycleChangeObserver(requiresVisiblePolling: true);
        var autostartState = new AutostartStateService(settingsService, observer);

        var loc = new LocalizationService(new MemoryStateStore());
        loc.SetLanguage("en");
        var protection = CreateDummyProtectionService();
        var vm = new SettingsViewModel(autostartState, loc, protection);
        vm.Initialize();

        await autostartState.StartAsync();

        // Assert: Condition respected, no automatic re-activation attempt
        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.DisabledByUser, status.State);
        Assert.Equal(0, startupTask.EnableRequestCount);

        // UI diagnosis reflects the real Windows restriction
        Assert.False(vm.IsAutostartEnabled);
        Assert.True(vm.HasAutostartStatusMessage);
        Assert.Contains("Windows has disabled PrivGuard startup", vm.AutostartStatusMessage, StringComparison.OrdinalIgnoreCase);

        await autostartState.StopAsync();
    }

    // =========================================================================
    // Test D — Windows Policy Disabled
    // =========================================================================

    [Fact]
    public async Task TestD_WindowsPolicyDisabled_RespectsPolicyRestriction_DoesNotBypass_ReportsDiagnostic()
    {
        // Setup: Group policy or MDM restricts startup tasks on this machine
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.DisabledByPolicy);
        var store = new MemoryStateStore(new DesiredState { Autostart = false });
        var provider = CreatePackagedProvider(startupTask, store);
        var settingsService = new SettingsService(provider, store);

        // Act: User tries to enable Autostart
        var result = await settingsService.SetAutostartAsync(true);

        // Assert: Operation fails cleanly, does not bypass Windows security
        Assert.False(result.Success);
        Assert.Contains("Windows policy", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.DisabledByPolicy, status.State);
        Assert.Equal(0, startupTask.EnableRequestCount);

        // Preference remains unchanged
        Assert.False(store.Load().Autostart);
    }

    // =========================================================================
    // Test E — Already Enabled (Idempotency)
    // =========================================================================

    [Fact]
    public async Task TestE_AlreadyEnabled_ReconciliationIsIdempotent_ZeroDuplicates()
    {
        var registry = new LifecycleMemoryRegistry();
        registry.RunValues["PrivGvard"] = $"\"{_dummyExePath}\" --minimized";

        var provider = new WindowsAutostartProvider(
            registry,
            new LifecycleFakeStartupTask(WindowsStartupTaskState.Enabled),
            () => false, // Win32 mode to verify registry idempotency
            () => _dummyExePath,
            _ => true,
            _ => true);

        // Status is already enabled
        var status1 = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Enabled, status1.State);

        // Repeated enable and migration calls
        var result1 = await provider.EnableAutostartAsync();
        var result2 = await provider.EnableAutostartAsync();
        var migrateResult = await provider.MigrateLegacyEntriesAsync();

        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.True(migrateResult.Success);

        // Invariant: Exactly 1 Run entry, exactly 1 PrivGuard registration, zero duplicate entries
        Assert.Single(registry.RunValues);
        Assert.True(registry.RunValues.ContainsKey("PrivGvard"));
        Assert.Equal($"\"{_dummyExePath}\" --minimized", registry.RunValues["PrivGvard"]);
    }

    // =========================================================================
    // Test F — Not Registered
    // =========================================================================

    [Fact]
    public async Task TestF_NotRegistered_DetectedCorrectly_ManualLaunchOperatesNormally()
    {
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Disabled);
        var store = new MemoryStateStore(new DesiredState { Autostart = false });
        var provider = CreatePackagedProvider(startupTask, store);

        // 1. Status indicates not registered
        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Disabled, status.State);
        Assert.False(status.IsEnabled);

        // 2. Manual application startup operates completely normally
        var protection = CreateDummyProtectionService();
        var camResult = await protection.EnableStandardProtectionAsync(BlockTarget.Camera);
        Assert.True(camResult.Success);

        var state = await protection.GetCurrentStateAsync();
        Assert.True(state.Camera.IsProtected);
    }

    // =========================================================================
    // Test G — Unsupported Environment
    // =========================================================================

    [Fact]
    public async Task TestG_UnsupportedEnvironment_MechanismUnavailable_DegradesGracefullyWithoutCrash()
    {
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Disabled)
        {
            GetStateFailure = new PlatformNotSupportedException("StartupTask API is not supported on this platform version")
        };
        var store = new MemoryStateStore();
        var provider = CreatePackagedProvider(startupTask, store);

        // Invariant: Does not throw or crash
        var status = await provider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Failed, status.State);
        Assert.Contains("could not be read", status.Message, StringComparison.OrdinalIgnoreCase);

        // Enable also returns clean failure without crash
        var enableResult = await provider.EnableAutostartAsync();
        Assert.False(enableResult.Success);
        Assert.Contains("could not enable", enableResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // Test H — Single Instance Under Autostart
    // =========================================================================

    [Fact]
    public async Task TestH_SingleInstance_AutostartDoesNotCreateDuplicates_SingleInstanceGuardMaintained()
    {
        var testMutex = $@"Local\PrivLock_Autostart_SingleInstance_{Guid.NewGuid():N}";
        var testGate = $@"Local\PrivLock_Autostart_UninstallGate_{Guid.NewGuid():N}";

        // 1. Windows starts instance 1 via autostart
        using var autostartInstance = new WindowsSingleInstanceGuard(testMutex, testGate);
        var admitted1 = autostartInstance.TryAcquireSingleInstance();
        Assert.True(admitted1);

        // 2. User concurrently clicks application shortcut to start instance 2 on a separate thread
        var admitted2 = await Task.Run(() =>
        {
            using var userInstance = new WindowsSingleInstanceGuard(testMutex, testGate);
            return userInstance.TryAcquireSingleInstance();
        });

        // Invariant: Second instance is cleanly denied and exits without changing state
        Assert.False(admitted2);

        // 3. When instance 1 exits, single instance is released
        autostartInstance.Release();

        var admitted3 = await Task.Run(() =>
        {
            using var thirdInstance = new WindowsSingleInstanceGuard(testMutex, testGate);
            return thirdInstance.TryAcquireSingleInstance();
        });
        Assert.True(admitted3);
    }

    // =========================================================================
    // Test I — Startup Recovery Integration
    // =========================================================================

    [Fact]
    public async Task TestI_StartupRecoveryIntegration_RecoveryPrecedesDesiredStateReconcile_EffectiveStatusVerified()
    {
        var dataDir = Path.Combine(_testDirectory, "Data_TestI");
        var recoveryDir = Path.Combine(_testDirectory, "Recovery_TestI");
        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(recoveryDir);

        var stateStore = new FileStateStore(dataDir);
        stateStore.Save(new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            MicrophoneStandard = StandardProtectionState.Active,
            Autostart = true
        });

        // Simulate crash leftover in session store
        var sessionStore = new FilePrivacySessionStore(recoveryDir);
        var previousResource = new OriginalDeviceState
        {
            ResourceId = "pnp:crash-camera",
            OperationId = "Op-Crash",
            Layer = ProtectionLayer.Secure,
            Target = BlockTarget.Camera,
            CapturedAtUtc = DateTimeOffset.UtcNow,
            LastUpdatedAtUtc = DateTimeOffset.UtcNow,
            JournalState = PrivacyResourceJournalState.Applied,
            ModifiedByPrivLock = true,
            InstanceId = "crash-camera",
            FriendlyName = "crash-camera",
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
            LastOperationId = "Op-Crash",
            Resources = [previousResource]
        };
        sessionStore.Save(crashSession);

        var platform = new LifecyclePlatformAdapter();
        platform.SetObservation("pnp:crash-camera", PrivacyResourceObservationKind.MatchesProtected);

        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Enabled);
        var autostartProvider = CreatePackagedProvider(startupTask, stateStore);

        var sessions = new PrivacySessionService(sessionStore, platform);
        var dummyDeps = new LifecycleDummyProtectionDeps(platform);
        var protection = new ProtectionService(dummyDeps, dummyDeps, dummyDeps, stateStore, sessions);
        var recoveryService = new PrivacyRecoveryService(protection);

        // Step 1: Autostart launches PrivGuard (--minimized)
        // Step 2: Recovery runs FIRST
        var recovery = await recoveryService.RecoverAtStartupAsync();
        Assert.True(recovery.SafeToExit);
        Assert.Contains("pnp:crash-camera", platform.RestoreCalls);

        // Previous crash session is restored
        Assert.False(sessionStore.Load()!.IsActive);

        // Step 3: Reconciliation runs SECOND and reapplies DesiredState
        var reconcile = await recoveryService.ReconcileAtStartupAsync(recovery);
        Assert.True(reconcile.Success, reconcile.ErrorMessage);

        // Step 4: Effective status verified in native platform
        Assert.True(dummyDeps.CameraIsProtected);
        Assert.True(dummyDeps.MicIsProtected);

        // Step 5: New session is created and active
        var activeSession = sessionStore.Load();
        Assert.NotNull(activeSession);
        Assert.True(activeSession!.IsActive);
        Assert.NotEqual(crashSession.SessionId, activeSession.SessionId);
    }

    // =========================================================================
    // Test J — Explicit User Disable
    // =========================================================================

    [Fact]
    public async Task TestJ_ExplicitUserDisable_SimulatedRestart_DoesNotReRegisterAutostart()
    {
        var startupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Enabled)
        {
            StateAfterDisable = WindowsStartupTaskState.Disabled
        };
        var store = new MemoryStateStore(new DesiredState { Autostart = true });
        var provider = CreatePackagedProvider(startupTask, store);
        var settingsService = new SettingsService(provider, store);

        // 1. User explicitly disables Autostart
        var disableResult = await settingsService.SetAutostartAsync(false);
        Assert.True(disableResult.Success);
        Assert.False(store.Load().Autostart);

        // 2. Simulate next Windows startup: application initializes and performs reconciliation
        var nextSessionStartupTask = new LifecycleFakeStartupTask(WindowsStartupTaskState.Disabled);
        var nextProvider = CreatePackagedProvider(nextSessionStartupTask, store);

        // Startup checks legacy entries migration
        var migrationResult = await nextProvider.MigrateLegacyEntriesAsync();
        Assert.True(migrationResult.Success);

        // Invariant: PrivGuard respects the user's explicit disable and does NOT re-register
        var readback = await nextProvider.GetAutostartStatusAsync();
        Assert.Equal(AutostartState.Disabled, readback.State);
        Assert.False(readback.IsEnabled);
        Assert.Equal(0, nextSessionStartupTask.EnableRequestCount);
    }

    // =========================================================================
    // Test Helpers and Doubles
    // =========================================================================

    private WindowsAutostartProvider CreatePackagedProvider(
        IWindowsStartupTaskAccessor startupTask,
        IStateStore stateStore) =>
        new(
            new LifecycleMemoryRegistry(),
            startupTask,
            () => true, // Packaged MSIX
            () => _dummyExePath,
            _ => true,
            _ => true);

    private static ProtectionService CreateDummyProtectionService()
    {
        var dummy = new LifecycleDummyProtectionDeps();
        var store = new MemoryStateStore();
        return new ProtectionService(dummy, dummy, dummy, store);
    }

    private sealed class LifecycleFakeStartupTask : IWindowsStartupTaskAccessor
    {
        private WindowsStartupTaskState _state;

        public LifecycleFakeStartupTask(WindowsStartupTaskState initialState)
        {
            _state = initialState;
        }

        public int EnableRequestCount { get; private set; }
        public int DisableCount { get; private set; }
        public WindowsStartupTaskState? StateAfterEnableRequest { get; set; }
        public WindowsStartupTaskState? StateAfterDisable { get; set; }
        public Exception? GetStateFailure { get; set; }

        public Task<WindowsStartupTaskState> GetStateAsync()
        {
            if (GetStateFailure != null)
                throw GetStateFailure;
            return Task.FromResult(_state);
        }

        public Task<WindowsStartupTaskState> RequestEnableAsync()
        {
            EnableRequestCount++;
            if (StateAfterEnableRequest.HasValue)
                _state = StateAfterEnableRequest.Value;
            return Task.FromResult(_state);
        }

        public Task DisableAsync()
        {
            DisableCount++;
            if (StateAfterDisable.HasValue)
                _state = StateAfterDisable.Value;
            return Task.CompletedTask;
        }
    }

    private sealed class LifecycleMemoryRegistry : IWindowsAutostartRegistry
    {
        public Dictionary<string, object> RunValues { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, byte[]> ApprovedValues { get; } = new(StringComparer.OrdinalIgnoreCase);

        public object? ReadRunValue(string name) =>
            RunValues.TryGetValue(name, out var val) ? val : null;

        public void WriteRunValue(string name, string value) =>
            RunValues[name] = value;

        public void DeleteRunValue(string name) =>
            RunValues.Remove(name);

        public byte[]? ReadStartupApprovedValue(string name) =>
            ApprovedValues.TryGetValue(name, out var val) ? val : null;

        public void WriteStartupApprovedValue(string name, byte[] value) =>
            ApprovedValues[name] = value;

        public void DeleteStartupApprovedValue(string name) =>
            ApprovedValues.Remove(name);

        public WindowsStartupApprovalSnapshot? ReadStartupApproval(string name) =>
            ApprovedValues.TryGetValue(name, out var val)
                ? new WindowsStartupApprovalSnapshot(val, WindowsStartupApprovalState.Enabled)
                : null;
    }

    private sealed class MemoryStateStore : IStateStore
    {
        private DesiredState _state;

        public MemoryStateStore(DesiredState? initial = null)
        {
            _state = initial ?? new DesiredState();
        }

        public DesiredState Load() => new()
        {
            CameraStandard = _state.CameraStandard,
            CameraSecure = _state.CameraSecure,
            MicrophoneStandard = _state.MicrophoneStandard,
            MicrophoneSecure = _state.MicrophoneSecure,
            AdvancedProtectionEnabled = _state.AdvancedProtectionEnabled,
            Language = _state.Language,
            Autostart = _state.Autostart
        };

        public void Save(DesiredState state) => _state = state;
    }

    private sealed class FakeLifecycleChangeObserver : IAutostartChangeObserver
    {
        public bool RequiresVisiblePolling { get; }

        public FakeLifecycleChangeObserver(bool requiresVisiblePolling)
        {
            RequiresVisiblePolling = requiresVisiblePolling;
        }

        public Task ObserveChangesAsync(
            Action onChanged,
            Action<Exception> onError,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class LifecyclePlatformAdapter : IPrivacySessionPlatformAdapter
    {
        private readonly Dictionary<string, PrivacyResourceObservation> _observations = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _restoreCalls = [];

        public bool SupportsPersistentRecovery => true;
        public IReadOnlyList<string> RestoreCalls => _restoreCalls;

        public void SetObservation(string resourceId, PrivacyResourceObservationKind kind) =>
            _observations[resourceId] = new PrivacyResourceObservation(kind);

        public Task<OperationResult> EnsureMutationQuiescenceAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

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
                    ResourceId = @"registry:CurrentUser:Default:Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam:Value",
                    OperationId = operationId,
                    Layer = layer,
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
                    ResourceId = "audio-mute:mic-std-1",
                    OperationId = operationId,
                    Layer = layer,
                    Target = BlockTarget.Microphone,
                    CapturedAtUtc = DateTimeOffset.UtcNow,
                    LastUpdatedAtUtc = DateTimeOffset.UtcNow,
                    JournalState = PrivacyResourceJournalState.Captured,
                    ModifiedByPrivLock = false,
                    EndpointId = "mic-std-1",
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
            Task.FromResult(new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.Confirmed));

        public Task<OperationResult> RestoreOriginalAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            _restoreCalls.Add(resource.ResourceId);
            _observations[resource.ResourceId] = new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal);
            return Task.FromResult(OperationResult.Ok());
        }

        public void CompleteRecoveryPass() { }
    }

    private sealed class LifecycleDummyProtectionDeps :
        IDeviceProtectionProvider,
        IDeviceDetector,
        IPlatformCapabilityProvider
    {
        private readonly LifecyclePlatformAdapter? _adapter;

        public LifecycleDummyProtectionDeps(LifecyclePlatformAdapter? adapter = null)
        {
            _adapter = adapter;
        }

        public bool CameraIsProtected { get; private set; }
        public bool MicIsProtected { get; private set; }

        public PlatformCapabilities Capabilities => new();
        public PlatformInfo PlatformInfo => new()
        {
            OperatingSystemName = "Windows",
            OsVersion = "10.0.22631",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken ct = default)
        {
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                CameraIsProtected = true;
                _adapter?.SetObservation(@"registry:CurrentUser:Default:Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam:Value", PrivacyResourceObservationKind.MatchesProtected);
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                MicIsProtected = true;
                _adapter?.SetObservation("audio-mute:mic-std-1", PrivacyResourceObservationKind.MatchesProtected);
            }
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken ct = default)
        {
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                CameraIsProtected = false;
                _adapter?.SetObservation(@"registry:CurrentUser:Default:Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam:Value", PrivacyResourceObservationKind.MatchesOriginal);
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                MicIsProtected = false;
                _adapter?.SetObservation("audio-mute:mic-std-1", PrivacyResourceObservationKind.MatchesOriginal);
            }
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken ct = default) =>
            Task.FromResult(OperationResult.Ok());

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

using Avalonia.Threading;
using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.UI.ViewModels;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

[Collection("AvaloniaUI")]
public sealed class StoreEditionViewModelSupervisionTests
{
    [Fact]
    public async Task MainViewModel_InStoreEdition_SetsStorePropertiesAndOpensSettings()
    {
        var storeDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector);
        var stateStore = new InMemoryStateStore();
        var capabilityProvider = new TestCapProvider();
        var protectionProvider = new FakeProtectionProvider();
        var launcher = new FakePrivacySettingsLauncher();

        var protectionService = new ProtectionService(
            protectionProvider,
            null!,
            capabilityProvider,
            stateStore,
            profileResolver: storeResolver,
            privacySettingsLauncher: launcher);

        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();
        var autostartState = new AutostartStateService(
            new SettingsService(new TestAutostartProvider(), stateStore),
            new NoOpAutostartChangeObserver());

        var viewModel = new MainViewModel(protectionService, autostartState, localizationService);
        viewModel.Initialize();
        await viewModel.RefreshStateAsync();
        Dispatcher.UIThread.RunJobs();

        Assert.True(viewModel.IsStoreEdition);
        Assert.False(viewModel.IsStandaloneEdition);
        Assert.Equal("Settings", viewModel.OpenCameraSettingsButtonText);

        // When toggling camera in Store edition, it routes to OpenCameraPrivacySettingsAsync without mutating ConsentStore
        await viewModel.OpenCameraPrivacySettingsAsync();
        Assert.Equal(BlockTarget.Camera, launcher.LastOpenedTarget);
    }

    [Fact]
    public async Task MainViewModel_WhenSupervisionOnlyReported_DisablesTogglesAndShowsBanner()
    {
        var storeDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector);
        var stateStore = new InMemoryStateStore();
        var capabilityProvider = new TestCapProvider();
        var protectionProvider = new FakeProtectionProvider();

        var coordinator = new FakeCrossEditionCoordinator(
            new CrossEditionInspectionResult(
                CrossEditionOwnershipState.OwnedByOtherEdition,
                new CrossEditionDescriptor
                {
                    SchemaVersion = 1,
                    OwnerEdition = EditionProfile.Standalone,
                    ProcessId = 1234,
                    SessionId = Guid.NewGuid(),
                    OperationStatus = "Active"
                },
                "Active Standalone edition process 1234 owns privacy session."));

        var protectionService = new ProtectionService(
            protectionProvider,
            new FakeDeviceDetector(),
            capabilityProvider,
            stateStore,
            profileResolver: storeResolver,
            crossEditionCoordinator: coordinator);

        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();
        var autostartState = new AutostartStateService(
            new SettingsService(new TestAutostartProvider(), stateStore),
            new NoOpAutostartChangeObserver());

        var viewModel = new MainViewModel(protectionService, autostartState, localizationService);
        viewModel.Initialize();
        await viewModel.RefreshStateAsync();
        Dispatcher.UIThread.RunJobs();

        Assert.True(viewModel.IsSupervisionOnly);
        Assert.Contains("Standalone", viewModel.SupervisionMessage);
        Assert.False(viewModel.IsCameraToggleEnabled);
        Assert.False(viewModel.IsMicToggleEnabled);
        Assert.False(viewModel.IsBothToggleEnabled);
        Assert.False(viewModel.IsAdvancedProtectionToggleEnabled);
    }

    [Fact]
    public async Task MainViewModel_WhenMicBlockedInStoreEdition_ShowsUninstallWarning()
    {
        var storeDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector);
        var stateStore = new InMemoryStateStore();
        var capabilityProvider = new TestCapProvider();
        var protectionProvider = new FakeProtectionProvider
        {
            CurrentState = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    Availability = TargetDeviceAvailability.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unavailable,
                    Availability = TargetDeviceAvailability.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = false,
                    IsVerified = true,
                    EndpointAvailability = AudioEndpointAvailability.Available
                }
            }
        };

        var protectionService = new ProtectionService(
            protectionProvider,
            new FakeDeviceDetector(),
            capabilityProvider,
            stateStore,
            profileResolver: storeResolver);

        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();
        var autostartState = new AutostartStateService(
            new SettingsService(new TestAutostartProvider(), stateStore),
            new NoOpAutostartChangeObserver());

        var viewModel = new MainViewModel(protectionService, autostartState, localizationService);
        viewModel.Initialize();
        await viewModel.RefreshStateAsync();
        Dispatcher.UIThread.RunJobs();

        Assert.True(viewModel.IsMicBlocked);
        Assert.True(viewModel.IsMicActiveStoreWarningVisible);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.MicActiveStoreWarningMessage));

        // Now simulate unblocking
        protectionProvider.CurrentState = new FullProtectionState
        {
            Camera = protectionProvider.CurrentState.Camera,
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = true,
                EndpointAvailability = AudioEndpointAvailability.Available
            }
        };

        await viewModel.RefreshStateAsync();
        Dispatcher.UIThread.RunJobs();

        Assert.False(viewModel.IsMicBlocked);
        Assert.False(viewModel.IsMicActiveStoreWarningVisible);
    }

    private sealed class FakeCrossEditionCoordinator : ICrossEditionCoordinator
    {
        private readonly CrossEditionInspectionResult _inspection;

        public FakeCrossEditionCoordinator(CrossEditionInspectionResult inspection)
        {
            _inspection = inspection;
        }

        public CrossEditionInspectionResult InspectOwnership() => _inspection;
        public CrossEditionAcquisitionResult TryAcquireOwnership(Guid sessionId) =>
            _inspection.RequiresSupervisionOnly
                ? CrossEditionAcquisitionResult.Failed(_inspection.State, _inspection.Message ?? "Denied")
                : CrossEditionAcquisitionResult.Acquired(new CrossEditionDescriptor { SessionId = sessionId });
        public bool TryReleaseOwnership(Guid sessionId) => true;
        public void UpdateHeartbeat(Guid sessionId) { }
    }

    private sealed class FakeProtectionProvider : IDeviceProtectionProvider
    {
        public FullProtectionState CurrentState { get; set; } = new()
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = true,
                EndpointAvailability = AudioEndpointAvailability.Available
            }
        };

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CurrentState);

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());
    }

    private sealed class FakeDeviceDetector : IDeviceDetector
    {
        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);
        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);
        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>([]);
    }

    private sealed class TestCapProvider : IPlatformCapabilityProvider
    {
        public PlatformCapabilities Capabilities { get; } = new();
        public PlatformInfo PlatformInfo { get; } = new()
        {
            OperatingSystemName = "Windows",
            OsVersion = "10.0",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };
    }

    private sealed class FakePrivacySettingsLauncher : IPrivacySettingsLauncher
    {
        public BlockTarget? LastOpenedTarget { get; private set; }
        public Task<OperationResult> OpenPrivacySettingsAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            LastOpenedTarget = target;
            return Task.FromResult(OperationResult.Ok());
        }
    }

    private sealed class FakePackageIdentityDetector : IPackageIdentityDetector
    {
        public FakePackageIdentityDetector(PackageIdentityKind kind) => Kind = kind;
        public PackageIdentityKind Kind { get; }
        public string? PackageFamilyName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_xxxx" : null;
        public string? PackageFullName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_1.0.0.0_x64__xxxx" : null;
        public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
    }

    private sealed class TestAutostartProvider : IAutostartProvider
    {
        public Task<AutostartStatus> GetAutostartStatusAsync() =>
            Task.FromResult(new AutostartStatus(AutostartState.Disabled));
        public Task<OperationResult> EnableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
        public Task<OperationResult> DisableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
        public Task<OperationResult> MigrateLegacyEntriesAsync() =>
            Task.FromResult(OperationResult.Ok());
    }

    private sealed class InMemoryStateStore : IStateStore
    {
        private DesiredState _state = new();
        public DesiredState Load() => _state;
        public void Save(DesiredState state) => _state = state;
    }
}

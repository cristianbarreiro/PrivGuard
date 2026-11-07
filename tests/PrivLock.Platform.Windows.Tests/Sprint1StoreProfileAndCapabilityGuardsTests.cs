using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PrivLock.Application.Services;
using PrivLock.Desktop;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.Devices;
using PrivLock.Platform.Windows.Policies;
using PrivLock.Platform.Windows.Privileged;
using PrivLock.Platform.Windows.System;
using PrivLock.UI.ViewModels;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

[Collection("WindowsPrivilegedSessionState")]
public sealed class Sprint1StoreProfileAndCapabilityGuardsTests : IDisposable
{
    private readonly string _tempDir;

    public Sprint1StoreProfileAndCapabilityGuardsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"PrivGuard_Sprint1Tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
    }

    public void Dispose()
    {
        WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    // =========================================================================
    // 7.1 Composition Root & Profile Resolution
    // =========================================================================

    [Fact]
    public void ProgramConfigureServices_RegistersCoherentEditionSnapshot_AndPreservesPreD05StoragePaths()
    {
        var storeDetector = new StubPackageIdentityDetector(
            PackageIdentityKind.AuthenticStorePackage,
            "cdevstudios.PrivGuard_tv7b4yqg10sw2",
            "cdevstudios.PrivGuard_2.0.0.0_x64__tv7b4yqg10sw2");
        var storeResolver = new DefaultEditionProfileResolver(storeDetector, _tempDir);

        var services = new ServiceCollection();
        try
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
            Program.ConfigureServices(services, storeDetector, storeResolver);

            using var provider = services.BuildServiceProvider();

            var resolvedDetector = provider.GetRequiredService<IPackageIdentityDetector>();
            var resolvedProfile = provider.GetRequiredService<IEditionProfileResolver>();
            var capabilityProvider = provider.GetRequiredService<IPlatformCapabilityProvider>();
            var settingsLauncher = provider.GetRequiredService<IPrivacySettingsLauncher>();
            var privacyReader = provider.GetRequiredService<IWindowsUserPrivacyReader>();
            var privacyWriter = provider.GetRequiredService<IWindowsUserPrivacyWriter>();
            var protectionService = provider.GetRequiredService<ProtectionService>();

            Assert.Same(storeDetector, resolvedDetector);
            Assert.Same(storeResolver, resolvedProfile);
            Assert.Same(storeResolver, WindowsPrivilegedSession.Instance.ProfileResolver);
            Assert.Equal(EditionProfile.Store, resolvedProfile.Profile);
            Assert.False(resolvedProfile.SupportsPrivilegedOperations);
            Assert.IsType<WindowsPrivacySettingsLauncher>(settingsLauncher);
            Assert.Same(privacyReader, privacyWriter);

            var capabilities = capabilityProvider.Capabilities;
            Assert.Equal(CapabilityLevel.Software, capabilities.CameraProtectionLevel);
            Assert.Equal(CapabilityLevel.Software, capabilities.MicrophoneProtectionLevel);
            Assert.False(capabilities.SupportsHardwareDisable);
            Assert.False(capabilities.SupportsSystemPolicy);
            Assert.False(capabilities.RequiresElevationForBlock);
            Assert.True(capabilities.SupportsAudioMuteLock);

            Assert.Equal(EditionProfile.Store, protectionService.EditionProfile);
            Assert.True(protectionService.IsStoreEdition);
            Assert.False(protectionService.IsUntrustedOrIndeterminate);
        }
        finally
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
        }
    }

    [Fact]
    public async Task WindowsPrivilegedSession_ProfileResolver_IsReadOnlyProperty_AndWriteOnceConfigured()
    {
        var prop = typeof(WindowsPrivilegedSession).GetProperty(nameof(WindowsPrivilegedSession.ProfileResolver));
        Assert.NotNull(prop);
        Assert.False(prop!.CanWrite);
        Assert.Null(prop.SetMethod);

        var storeDetector = new StubPackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector, _tempDir);
        var standaloneDetector = new StubPackageIdentityDetector(PackageIdentityKind.NotPackaged);
        var standaloneResolver = new DefaultEditionProfileResolver(standaloneDetector, _tempDir);

        try
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
            WindowsPrivilegedSession.ConfigureDefaultProfileResolver(storeResolver);

            // Idempotent repeat call with the exact same snapshot instance is allowed
            WindowsPrivilegedSession.ConfigureDefaultProfileResolver(storeResolver);
            Assert.Same(storeResolver, WindowsPrivilegedSession.Instance.ProfileResolver);
            Assert.Equal(EditionProfile.Store, WindowsPrivilegedSession.Instance.ProfileResolver.Profile);

            // Attempting to replace the configured resolver with a different instance must throw
            Assert.Throws<InvalidOperationException>(() =>
                WindowsPrivilegedSession.ConfigureDefaultProfileResolver(standaloneResolver));

            // Profile remains Store and continues blocking privileged worker execution
            Assert.Same(storeResolver, WindowsPrivilegedSession.Instance.ProfileResolver);
            var execResult = await WindowsPrivilegedSession.Instance.ExecuteCommandAsync("ping", "");
            Assert.False(execResult.Success);
            Assert.Equal(0, WindowsPrivilegedSession.Instance.ElevationLaunchCount);
            Assert.False(WindowsPrivilegedSession.Instance.IsSessionActive);
        }
        finally
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
        }
    }

    [Fact]
    public async Task WindowsPrivilegedSession_ActiveSession_CannotHaveProfileReplacedMidSession()
    {
        var standaloneDetector = new StubPackageIdentityDetector(PackageIdentityKind.NotPackaged);
        var standaloneResolver = new DefaultEditionProfileResolver(standaloneDetector, _tempDir);
        var storeDetector = new StubPackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector, _tempDir);

        try
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
            WindowsPrivilegedSession.ConfigureDefaultProfileResolver(standaloneResolver);

            var session = WindowsPrivilegedSession.Instance;
            session.CustomWorkerLauncher = () => Task.CompletedTask;
            session.CustomCommandExecutor = (_, _) => Task.FromResult(OperationResult.Ok());

            using (session.BeginDeferredScope())
            {
                var cmdResult = await session.ExecuteCommandAsync("apply-policy", "camera");
                Assert.True(cmdResult.Success);
                Assert.True(session.IsSessionActive);
                Assert.Equal(1, session.ElevationLaunchCount);

                Assert.Throws<InvalidOperationException>(() =>
                    WindowsPrivilegedSession.ConfigureDefaultProfileResolver(storeResolver));

                Assert.Same(standaloneResolver, session.ProfileResolver);
                Assert.Equal(EditionProfile.Standalone, session.ProfileResolver.Profile);
            }
        }
        finally
        {
            WindowsPrivilegedSession.ResetDefaultConfigurationForTesting();
        }
    }

    [Theory]
    [InlineData(PackageIdentityKind.AuthenticStorePackage, EditionProfile.Store, CapabilityLevel.Software, false)]
    [InlineData(PackageIdentityKind.NotPackaged, EditionProfile.Standalone, CapabilityLevel.DualLayer, true)]
    [InlineData(PackageIdentityKind.UnsignedDevPackage, EditionProfile.UntrustedOrIndeterminate, CapabilityLevel.None, false)]
    [InlineData(PackageIdentityKind.UnverifiedPublisherPackage, EditionProfile.UntrustedOrIndeterminate, CapabilityLevel.None, false)]
    [InlineData(PackageIdentityKind.UnexpectedPackage, EditionProfile.UntrustedOrIndeterminate, CapabilityLevel.None, false)]
    [InlineData(PackageIdentityKind.DetectionFailed, EditionProfile.UntrustedOrIndeterminate, CapabilityLevel.None, false)]
    public void WindowsCapabilityProvider_ReflectsResolvedEditionProfile(
        PackageIdentityKind identityKind,
        EditionProfile expectedProfile,
        CapabilityLevel expectedCapabilityLevel,
        bool expectedHardwareAndPolicySupport)
    {
        var detector = new StubPackageIdentityDetector(identityKind);
        var resolver = new DefaultEditionProfileResolver(detector, _tempDir);
        var capabilityProvider = new WindowsCapabilityProvider(new StubElevationProvider(), resolver);

        var caps = capabilityProvider.Capabilities;

        Assert.Equal(expectedProfile, resolver.Profile);
        Assert.Equal(expectedCapabilityLevel, caps.CameraProtectionLevel);
        Assert.Equal(expectedCapabilityLevel, caps.MicrophoneProtectionLevel);
        Assert.Equal(expectedHardwareAndPolicySupport, caps.SupportsHardwareDisable);
        Assert.Equal(expectedHardwareAndPolicySupport, caps.SupportsSystemPolicy);
        Assert.Equal(expectedHardwareAndPolicySupport, caps.RequiresElevationForBlock);
        Assert.Equal(expectedProfile != EditionProfile.UntrustedOrIndeterminate, caps.SupportsAudioMuteLock);
    }

    // =========================================================================
    // 7.2 Store & Untrusted Boundaries (Zero Forbidden Calls)
    // =========================================================================

    [Theory]
    [InlineData(PackageIdentityKind.AuthenticStorePackage)]
    [InlineData(PackageIdentityKind.UnsignedDevPackage)]
    [InlineData(PackageIdentityKind.DetectionFailed)]
    public async Task LowLevelComponents_InStoreOrUntrustedProfile_FailClosedWithZeroNativeMutations(
        PackageIdentityKind identityKind)
    {
        var harness = new WindowsBoundaryTestHarness(identityKind, _tempDir);

        // 1. ConsentStore writes must be blocked before touching registry
        var camConsentWrite = harness.UserPrivacyManager.SetCameraUserPrivacy(BlockStatus.Blocked);
        var micConsentWrite = harness.UserPrivacyManager.SetMicrophoneUserPrivacy(BlockStatus.Blocked);
        Assert.False(camConsentWrite.Success);
        Assert.False(micConsentWrite.Success);
        Assert.Equal(0, harness.ConsentStoreWriteCount);

        // 2. HKLM Policy writes must be blocked without launching worker
        var camPolicyWrite = harness.RealPolicyManager.SetPolicy(BlockTarget.Camera, BlockStatus.Blocked);
        var micPolicyWrite = harness.RealPolicyManager.SetPolicy(BlockTarget.Microphone, BlockStatus.Blocked);
        Assert.False(camPolicyWrite.Success);
        Assert.False(micPolicyWrite.Success);
        Assert.Equal(0, harness.ElevationLaunchCount);

        // 3. PnP CfgMgr32 mutations must be blocked without launching worker
        var fakeDevices = new List<DeviceInfo>
        {
            new() { Id = @"USB\VID_1234&PID_5678\5&11111111&0&1", FriendlyName = "Camera", DeviceType = DeviceType.Camera }
        };
        var pnpDisable = await harness.RealDeviceController.DisableDevicesAsync(fakeDevices);
        var pnpEnable = await harness.RealDeviceController.EnableDevicesAsync(fakeDevices);
        Assert.False(pnpDisable.Success);
        Assert.False(pnpEnable.Success);
        Assert.Equal(0, harness.ElevationLaunchCount);

        // 4. Privileged Session, Worker, and Executor must fail closed
        var sessionExec = await harness.PrivilegedSession.ExecuteCommandAsync("ping", "");
        var workerExitCode = WindowsPrivilegedWorker.Run(
            "testpipe",
            1234,
            "0123456789abcdef0123456789abcdef",
            harness.ProfileResolver);
        var executorResult = WindowsPrivilegedExecutor.ExecuteWorkerCommand(
            "ping",
            "",
            "S-1-5-21-123456-7890-1234-1001",
            harness.ProfileResolver);

        Assert.False(sessionExec.Success);
        Assert.False(harness.PrivilegedSession.IsSessionActive);
        Assert.Equal(1, workerExitCode);
        Assert.False(executorResult.Success);
        Assert.Equal(0, harness.ElevationLaunchCount);
    }

    [Fact]
    public async Task StoreProfile_SecureProtectionRequests_FailClosedWithZeroPrivilegedOrRegistryCalls()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);

        var enableCamSecure = await harness.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Camera);
        var enableMicSecure = await harness.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Microphone);
        var enableBothSecure = await harness.ProtectionService.EnableSecureProtectionAsync(BlockTarget.Both);
        var disableBothSecure = await harness.ProtectionService.DisableSecureProtectionAsync(BlockTarget.Both);
        var toggleAdvOn = await harness.ProtectionService.SetAdvancedProtectionAsync(true);
        var toggleAdvOff = await harness.ProtectionService.SetAdvancedProtectionAsync(false);

        Assert.False(enableCamSecure.Success);
        Assert.False(enableMicSecure.Success);
        Assert.False(enableBothSecure.Success);
        Assert.False(disableBothSecure.Success);
        Assert.False(toggleAdvOn.Success);
        Assert.False(toggleAdvOff.Success);

        Assert.Equal(0, harness.ElevationLaunchCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
    }

    [Fact]
    public async Task StoreProfile_CameraStandardMutation_FailsClosedWithoutTouchingConsentStore_WhileSettingsLauncherNavigatesToMsSettings()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);
        harness.SpyPrivacyReader.CameraStatus = BlockStatus.Allowed;

        // Direct camera Standard block/unblock must fail closed in Store without writing to ConsentStore
        var blockCam = await harness.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        var unblockCam = await harness.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Camera);

        Assert.False(blockCam.Success);
        Assert.False(unblockCam.Success);
        Assert.Equal(0, harness.ConsentStoreWriteCount);

        // Opening Windows Settings navigates to ms-settings:privacy-webcam without mutating ConsentStore or DesiredState
        var openSettings = await harness.ProtectionService.OpenPrivacySettingsAsync(BlockTarget.Camera);
        Assert.True(openSettings.Success);
        Assert.Single(harness.LaunchedUris);
        Assert.Equal(IPrivacySettingsLauncher.WindowsCameraSettingsUri, harness.LaunchedUris[0]);
        Assert.Equal(0, harness.ConsentStoreWriteCount);

        // Simply opening ms-settings: must NOT count as permission changed while OS still reports Allowed
        var stateBeforeUserChange = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.False(stateBeforeUserChange.Camera.IsProtected);
        Assert.Equal(StandardProtectionState.Inactive, stateBeforeUserChange.Camera.StandardState);

        // Only after the user changes the toggle in Windows Settings (reflected by OS read) does Store report Active
        harness.SpyPrivacyReader.CameraStatus = BlockStatus.Blocked;
        var stateAfterUserChange = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.True(stateAfterUserChange.Camera.IsProtected);
        Assert.True(stateAfterUserChange.Camera.StandardIsVerified);
        Assert.Equal(StandardProtectionState.Active, stateAfterUserChange.Camera.StandardState);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
    }

    [Fact]
    public async Task StoreProfile_LowLevelProviderAndAdapter_RejectCameraAndBoth_WithZeroPartialExecution()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);
        harness.FakeCoreAudio.SetSimulatedEndpoints(
            isMuted: false,
            AudioEndpointHardwareMuteSupport.Supported);

        // 1. Direct WindowsPrivacySessionPlatformAdapter.CaptureAsync checks for Camera and Both in Store
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.SessionAdapter.CaptureAsync(ProtectionLayer.Standard, BlockTarget.Camera, "op-cam"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.SessionAdapter.CaptureAsync(ProtectionLayer.Standard, BlockTarget.Both, "op-both"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.SessionAdapter.CaptureAsync(ProtectionLayer.Secure, BlockTarget.Both, "op-sec-both"));

        // Seed a valid active session containing a microphone endpoint resource so that if
        // WindowsProtectionProvider.EnableStandardProtectionAsync(BlockTarget.Both) did not reject Both
        // upfront, it would partially execute the microphone WASAPI mute and return Ok.
        var micResources = await harness.SessionAdapter.CaptureAsync(
            ProtectionLayer.Standard,
            BlockTarget.Microphone,
            "op-seeded");
        Assert.Single(micResources);
        harness.SessionStore.Save(new PrivacySession
        {
            IsActive = true,
            Status = PrivacySessionStatus.Active,
            OwnerProcessId = Environment.ProcessId,
            LastOperationId = "op-seeded",
            Resources = micResources.ToList()
        });

        // 2. Direct WindowsProtectionProvider checks for Camera and Both in Store
        var providerEnableCam = await harness.ProtectionProvider.EnableStandardProtectionAsync(BlockTarget.Camera);
        var providerEnableBoth = await harness.ProtectionProvider.EnableStandardProtectionAsync(BlockTarget.Both);
        var providerDisableCam = await harness.ProtectionProvider.DisableStandardProtectionAsync(BlockTarget.Camera);
        var providerDisableBoth = await harness.ProtectionProvider.DisableStandardProtectionAsync(BlockTarget.Both);

        Assert.False(providerEnableCam.Success);
        Assert.False(providerEnableBoth.Success);
        Assert.False(providerDisableCam.Success);
        Assert.False(providerDisableBoth.Success);
        Assert.Contains("ms-settings:privacy-webcam", providerEnableCam.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ms-settings:privacy-webcam", providerEnableBoth.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ms-settings:privacy-webcam", providerDisableCam.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ms-settings:privacy-webcam", providerDisableBoth.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // Verify zero partial microphone WASAPI execution and zero forbidden calls
        Assert.Equal(0, harness.FakeCoreAudio.MuteWriteCount);
        Assert.False(harness.FakeCoreAudio.IsMuted);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);

        // 3. Direct ProtectionService checks for Both in Store
        var serviceEnableBoth = await harness.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Both);
        var serviceDisableBoth = await harness.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Both);
        var serviceBlockBoth = await harness.ProtectionService.BlockDeviceAsync(BlockTarget.Both);
        var serviceUnblockBoth = await harness.ProtectionService.UnblockDeviceAsync(BlockTarget.Both);

        Assert.False(serviceEnableBoth.Success);
        Assert.False(serviceDisableBoth.Success);
        Assert.False(serviceBlockBoth.Success);
        Assert.False(serviceUnblockBoth.Success);

        Assert.Equal(0, harness.FakeCoreAudio.MuteWriteCount);
        Assert.False(harness.FakeCoreAudio.IsMuted);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);

        // 4. Confirm direct Microphone Standard on WindowsProtectionProvider still succeeds via WASAPI only
        var providerEnableMic = await harness.ProtectionProvider.EnableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(providerEnableMic.Success);
        Assert.Equal(1, harness.FakeCoreAudio.MuteWriteCount);
        Assert.True(harness.FakeCoreAudio.IsMuted);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);
    }

    [Fact]
    public async Task StoreProfile_MicrophoneStandardProtection_UsesCoreAudioOnly_AndNeverWritesOrRestoresConsentStore()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);
        harness.FakeCoreAudio.SetSimulatedEndpoints(
            isMuted: false,
            AudioEndpointHardwareMuteSupport.Supported);

        // Apply Standard Microphone protection in Store
        var blockMic = await harness.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(blockMic.Success);
        Assert.True(harness.FakeCoreAudio.MuteWriteCount > 0);
        Assert.True(harness.FakeCoreAudio.IsMuted);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);

        // Disable Standard Microphone protection in Store (restores via WAL session adapter)
        var unblockMic = await harness.ProtectionService.DisableStandardProtectionAsync(BlockTarget.Microphone);
        Assert.True(unblockMic.Success);
        Assert.False(harness.FakeCoreAudio.IsMuted);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);
    }

    [Fact]
    public async Task StoreProfile_PrivacySessionAdapter_RejectsForgedPolicyAndDeviceEntries_NeverWritesConsentStoreOrLaunchesWorker()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);

        var forgedConsentEntry = new OriginalPolicyState
        {
            ResourceId = @"registry:HKCU:Default:Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam:Value",
            OperationId = "forged-consent",
            Layer = ProtectionLayer.Standard,
            Target = BlockTarget.Camera,
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
        };

        var forgedHklmEntry = new OriginalPolicyState
        {
            ResourceId = @"registry:HKLM:Registry64:SOFTWARE\Policies\Microsoft\Windows\AppPrivacy:LetAppsAccessCamera",
            OperationId = "forged-hklm",
            Layer = ProtectionLayer.Secure,
            Target = BlockTarget.Camera,
            RegistryHive = PrivacyRegistryHive.LocalMachine,
            RegistryView = PrivacyRegistryView.Registry64,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy",
            ValueName = "LetAppsAccessCamera",
            ValueExisted = false,
            OriginalValueKind = PrivacyRegistryValueKind.None,
            OriginalValue = null,
            ProtectedValueExists = true,
            ProtectedValueKind = PrivacyRegistryValueKind.DWord,
            ProtectedValue = "2"
        };

        var forgedPnpEntry = new OriginalDeviceState
        {
            ResourceId = @"device:USB\VID_1234&PID_5678\5&11111111&0&1",
            OperationId = "forged-pnp",
            Layer = ProtectionLayer.Secure,
            Target = BlockTarget.Camera,
            InstanceId = @"USB\VID_1234&PID_5678\5&11111111&0&1",
            FriendlyName = "Camera",
            DeviceClass = "Camera",
            IsPresentAtCapture = true,
            OriginalEnabledState = true,
            OriginalProblemCode = 0,
            ProtectedEnabledState = false,
            IsSafelyRestorable = true
        };

        var restoreConsent = await harness.SessionAdapter.RestoreOriginalAsync(forgedConsentEntry);
        var restoreHklm = await harness.SessionAdapter.RestoreOriginalAsync(forgedHklmEntry);
        var restorePnp = await harness.SessionAdapter.RestoreOriginalAsync(forgedPnpEntry);

        Assert.False(restoreConsent.Success);
        Assert.False(restoreHklm.Success);
        Assert.False(restorePnp.Success);

        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.HklmWriteCount);
        Assert.Equal(0, harness.PnpMutationCount);
        Assert.Equal(0, harness.ElevationLaunchCount);
    }

    [Fact]
    public async Task UntrustedOrIndeterminateProfile_FailsClosedOnAllOperations_IncludingCoreAudioAndSettingsLauncher()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.UnsignedDevPackage, _tempDir);

        Assert.True(harness.ProtectionService.IsUntrustedOrIndeterminate);
        Assert.False(harness.SessionAdapter.SupportsPersistentRecovery);

        var status = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.Equal(StandardProtectionState.Unknown, status.Camera.StandardState);
        Assert.Equal(StandardProtectionState.Unknown, status.Microphone.StandardState);

        var enableMic = await harness.ProtectionService.EnableStandardProtectionAsync(BlockTarget.Microphone);
        var openSettings = await harness.ProtectionService.OpenPrivacySettingsAsync(BlockTarget.Camera);
        var realCoreAudioGet = harness.RealCoreAudioController.GetMicrophoneMuteStates();
        var realCoreAudioSet = harness.RealCoreAudioController.SetMicrophoneMute("endpoint-1", true);

        Assert.False(enableMic.Success);
        Assert.False(openSettings.Success);
        Assert.False(realCoreAudioGet.Success);
        Assert.False(realCoreAudioSet.Success);
        Assert.Empty(harness.LaunchedUris);
        Assert.Equal(0, harness.FakeCoreAudio.MuteWriteCount);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
        Assert.Equal(0, harness.ElevationLaunchCount);
    }

    // =========================================================================
    // 7.3 Standalone Non-Regression
    // =========================================================================

    [Fact]
    public async Task StandaloneProfile_PreservesStandardCapabilities_AndCapabilityMatrix()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.NotPackaged, _tempDir);
        harness.SpyPrivacyReader.CameraStatus = BlockStatus.Allowed;
        harness.SpyPrivacyReader.MicrophoneStatus = BlockStatus.Allowed;
        harness.FakeCoreAudio.SetSimulatedEndpoints(isMuted: false, AudioEndpointHardwareMuteSupport.Supported);

        Assert.Equal(EditionProfile.Standalone, harness.ProtectionService.EditionProfile);
        Assert.False(harness.ProtectionService.IsStoreEdition);
        Assert.True(harness.ProtectionService.Capabilities.SupportsHardwareDisable);
        Assert.True(harness.ProtectionService.Capabilities.SupportsSystemPolicy);
        Assert.True(harness.ProtectionService.Capabilities.RequiresElevationForBlock);

        // Verify WindowsUserPrivacyManager allows ConsentStore writes in Standalone (via in-memory writer delegate)
        var capturedConsentWrites = new List<(string SubKey, string ValueName, string Value)>();
        var standalonePrivacyManager = new WindowsUserPrivacyManager(
            harness.ProfileResolver,
            (subKey, valueName, value) => capturedConsentWrites.Add((subKey, valueName, value)));

        var camWriteResult = standalonePrivacyManager.SetCameraUserPrivacy(BlockStatus.Blocked);
        var micWriteResult = standalonePrivacyManager.SetMicrophoneUserPrivacy(BlockStatus.Blocked);
        Assert.True(camWriteResult.Success);
        Assert.True(micWriteResult.Success);
        Assert.Equal(4, capturedConsentWrites.Count);
        Assert.All(capturedConsentWrites, w => Assert.Equal("Deny", w.Value));

        // Verify Standalone Standard & Secure protection via isolated in-memory provider/adapter (0 host OS mutations)
        var standaloneSessionStore = new InMemoryPrivacySessionStore();
        var standaloneSim = new SimulatedStandalonePlatform(harness.ProfileResolver, standaloneSessionStore);
        var standaloneSessionService = new PrivacySessionService(standaloneSessionStore, standaloneSim);
        var standaloneProtectionService = new ProtectionService(
            standaloneSim,
            new StubDeviceDetector(),
            new WindowsCapabilityProvider(new StubElevationProvider(), harness.ProfileResolver),
            new InMemoryStateStore(),
            standaloneSessionService,
            harness.ProfileResolver,
            harness.SettingsLauncher);

        var blockCamStd = await standaloneProtectionService.EnableStandardProtectionAsync(BlockTarget.Camera);
        var blockMicStd = await standaloneProtectionService.EnableStandardProtectionAsync(BlockTarget.Microphone);
        var blockCamSec = await standaloneProtectionService.EnableSecureProtectionAsync(BlockTarget.Camera);
        var unblockBothStd = await standaloneProtectionService.DisableStandardProtectionAsync(BlockTarget.Both);
        var blockBothStd = await standaloneProtectionService.EnableStandardProtectionAsync(BlockTarget.Both);

        Assert.True(blockCamStd.Success);
        Assert.True(blockMicStd.Success);
        Assert.True(blockCamSec.Success);
        Assert.True(unblockBothStd.Success);
        Assert.True(blockBothStd.Success);
        Assert.Equal(3, standaloneSim.StandardEnableCalls);
        Assert.Equal(1, standaloneSim.SecureEnableCalls);
    }

    // =========================================================================
    // 7.4 Core Audio Hardware Mute Verification & UI Honesty
    // =========================================================================

    [Fact]
    public void EvaluateMicrophoneStandardVerification_RequiresAllActiveEndpointsToSupportHardwareMute()
    {
        var singleSupported = new AudioHardwareMuteSummary
        {
            TotalCount = 1,
            SupportedCount = 1,
            NotSupportedCount = 0,
            UnknownCount = 0
        };
        Assert.True(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            singleSupported));

        var singleSoftwareOnly = new AudioHardwareMuteSummary
        {
            TotalCount = 1,
            SupportedCount = 0,
            NotSupportedCount = 1,
            UnknownCount = 0
        };
        Assert.False(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            singleSoftwareOnly));

        var singleUnknown = new AudioHardwareMuteSummary
        {
            TotalCount = 1,
            SupportedCount = 0,
            NotSupportedCount = 0,
            UnknownCount = 1
        };
        Assert.False(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            singleUnknown));

        // Multi-endpoint: 1 Supported + 1 NotSupported MUST return false (cannot claim full hardware verification)
        var multiMixedNotSupported = new AudioHardwareMuteSummary
        {
            TotalCount = 2,
            SupportedCount = 1,
            NotSupportedCount = 1,
            UnknownCount = 0
        };
        Assert.False(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            multiMixedNotSupported));

        // Multi-endpoint: 1 Supported + 1 Unknown MUST return false
        var multiMixedUnknown = new AudioHardwareMuteSummary
        {
            TotalCount = 2,
            SupportedCount = 1,
            NotSupportedCount = 0,
            UnknownCount = 1
        };
        Assert.False(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            multiMixedUnknown));

        // Multi-endpoint: 2 Supported MUST return true
        var multiAllSupported = new AudioHardwareMuteSummary
        {
            TotalCount = 2,
            SupportedCount = 2,
            NotSupportedCount = 0,
            UnknownCount = 0
        };
        Assert.True(WindowsProtectionProvider.EvaluateMicrophoneStandardVerification(
            StandardProtectionState.Active,
            AudioEndpointAvailability.Available,
            multiAllSupported));
    }

    [Fact]
    public async Task MainViewModel_InStoreEdition_DifferentiatesHardwareMuteFromSoftwareOnlyAndUnknown_WhileKeepingSoftwareMuteUsable()
    {
        var harness = new WindowsBoundaryTestHarness(PackageIdentityKind.AuthenticStorePackage, _tempDir);
        var stateStore = new InMemoryStateStore();
        var localization = new LocalizationService(stateStore);
        localization.SetLanguage("en");

        var settingsService = new SettingsService(new StubAutostartProvider(), stateStore);
        var autostartService = new AutostartStateService(
            settingsService,
            new NoOpAutostartChangeObserver());

        var vm = new MainViewModel(
            harness.ProtectionService,
            autostartService,
            localization);

        Assert.True(vm.IsStoreEdition);
        Assert.False(vm.IsStandaloneEdition);
        Assert.False(vm.IsSecureProtectionSupported);

        var devices = await harness.ProtectionService.GetDetectedDevicesAsync();

        // Case 1: Software-only mute (HardwareMuteSupport.NotSupported)
        harness.FakeCoreAudio.SetSimulatedEndpoints(
            isMuted: true,
            AudioEndpointHardwareMuteSupport.NotSupported);

        var stateSoftwareOnly = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.True(stateSoftwareOnly.Microphone.IsProtected);
        Assert.False(stateSoftwareOnly.Microphone.StandardIsVerified);

        vm.UpdateUi(stateSoftwareOnly, devices);

        // StandardIsVerified is false, so UI displays Amber warning "⚠️ Partially Protected (Software Mute Only)"
        // yet the user can still toggle software mute on/off (IsMicToggleEnabled == true, IsMicBlocked == true)
        Assert.False(vm.IsAdvancedProtectionToggleEnabled);
        Assert.Equal("#FFA726", vm.MicStatusColor);
        Assert.Equal(localization.GetString("StatusBlockedSoftwareOnly"), vm.MicStatusText);
        Assert.True(vm.IsMicToggleEnabled);
        Assert.True(vm.IsMicBlocked);

        // Case 2: HardwareMuteSupport.Unknown (QueryHardwareSupport failed)
        harness.FakeCoreAudio.SetSimulatedEndpoints(
            isMuted: true,
            AudioEndpointHardwareMuteSupport.Unknown);

        var stateUnknownHw = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.True(stateUnknownHw.Microphone.IsProtected);
        Assert.False(stateUnknownHw.Microphone.StandardIsVerified);

        vm.UpdateUi(stateUnknownHw, devices);

        Assert.Equal("#FFA726", vm.MicStatusColor);
        Assert.Equal(localization.GetString("StatusPartiallyUnverifiable"), vm.MicStatusText);
        Assert.True(vm.IsMicToggleEnabled);

        // Case 3: HardwareMuteSupport.Supported across all endpoints
        harness.FakeCoreAudio.SetSimulatedEndpoints(
            isMuted: true,
            AudioEndpointHardwareMuteSupport.Supported);

        var stateSupportedHw = await harness.ProtectionService.GetCurrentStateAsync();
        Assert.True(stateSupportedHw.Microphone.IsProtected);
        Assert.True(stateSupportedHw.Microphone.StandardIsVerified);

        vm.UpdateUi(stateSupportedHw, devices);

        Assert.Equal("#E57373", vm.MicStatusColor);
        Assert.Equal(localization.GetString("StatusBlockedStandard"), vm.MicStatusText);
        Assert.True(vm.IsMicToggleEnabled);
        Assert.True(vm.IsMicBlocked);

        // Case 4: Camera privacy settings command in Store opens ms-settings:privacy-webcam and never writes to ConsentStore
        await vm.OpenCameraPrivacySettingsCommand.ExecuteAsync(null);

        Assert.Contains(IPrivacySettingsLauncher.WindowsCameraSettingsUri, harness.LaunchedUris);
        Assert.Equal(0, harness.ConsentStoreWriteCount);
    }

    // =========================================================================
    // 7.5 MSIX Manifest Verification (D-01)
    // =========================================================================

    [Fact]
    public void PackageAppxManifest_DeclaresRunFullTrustSolely_AndContainsNoAllowElevationOrUnvirtualizedResources()
    {
        var repoRoot = FindRepoRoot();
        var manifestPath = Path.Combine(repoRoot, "packaging", "Package.appxmanifest");
        Assert.True(File.Exists(manifestPath), $"Expected manifest at {manifestPath}");

        var rawXml = File.ReadAllText(manifestPath);
        Assert.DoesNotContain("allowElevation", rawXml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("unvirtualizedResources", rawXml, StringComparison.OrdinalIgnoreCase);

        var doc = XDocument.Parse(rawXml);
        var capabilitiesElement = doc.Root?.Elements()
            .FirstOrDefault(e => string.Equals(e.Name.LocalName, "Capabilities", StringComparison.Ordinal));

        Assert.NotNull(capabilitiesElement);

        var capabilityElements = capabilitiesElement!.Elements().ToList();
        Assert.Single(capabilityElements);
        Assert.Equal("Capability", capabilityElements[0].Name.LocalName);
        Assert.Equal("runFullTrust", capabilityElements[0].Attribute("Name")?.Value);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "PrivGvard.sln")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing PrivGvard.sln.");
    }

    // =========================================================================
    // Test Harness & Doubles (100% Isolated from Host OS)
    // =========================================================================

    private sealed class WindowsBoundaryTestHarness
    {
        public StubPackageIdentityDetector IdentityDetector { get; }
        public DefaultEditionProfileResolver ProfileResolver { get; }
        public SpyUserPrivacyManager UserPrivacyManager { get; }
        public SpyUserPrivacyManager SpyPrivacyReader => UserPrivacyManager;
        public FakeCoreAudioController FakeCoreAudio { get; }
        public WindowsCoreAudioController RealCoreAudioController { get; }
        public SpyPolicyManager SpyPolicyManager { get; }
        public WindowsPolicyManager RealPolicyManager { get; }
        public SpyDeviceController SpyDeviceController { get; }
        public WindowsDeviceController RealDeviceController { get; }
        public WindowsPrivilegedSession PrivilegedSession { get; }
        public InMemoryPrivacySessionStore SessionStore { get; }
        public WindowsPrivacySessionPlatformAdapter SessionAdapter { get; }
        public WindowsProtectionProvider ProtectionProvider { get; }
        public WindowsPrivacySettingsLauncher SettingsLauncher { get; }
        public ProtectionService ProtectionService { get; }
        public List<string> LaunchedUris { get; } = [];

        public int ElevationLaunchCount { get; private set; }
        public int ConsentStoreWriteCount => UserPrivacyManager.ConsentStoreWriteCount;
        public int HklmWriteCount => SpyPolicyManager.HklmWriteCount;
        public int PnpMutationCount => SpyDeviceController.PnpMutationCount;

        public WindowsBoundaryTestHarness(PackageIdentityKind identityKind, string baseRootDir)
        {
            var subDir = Path.Combine(baseRootDir, $"run_{Guid.NewGuid():N}");
            Directory.CreateDirectory(subDir);

            IdentityDetector = new StubPackageIdentityDetector(identityKind);
            ProfileResolver = new DefaultEditionProfileResolver(IdentityDetector, subDir);

            PrivilegedSession = new WindowsPrivilegedSession(ProfileResolver)
            {
                CustomWorkerLauncher = () =>
                {
                    ElevationLaunchCount++;
                    return Task.CompletedTask;
                }
            };

            UserPrivacyManager = new SpyUserPrivacyManager(ProfileResolver);
            FakeCoreAudio = new FakeCoreAudioController(ProfileResolver);
            RealCoreAudioController = new WindowsCoreAudioController(ProfileResolver);
            SpyPolicyManager = new SpyPolicyManager(ProfileResolver);
            RealPolicyManager = new WindowsPolicyManager(ProfileResolver);
            SpyDeviceController = new SpyDeviceController(ProfileResolver);
            RealDeviceController = new WindowsDeviceController(ProfileResolver);

            var detector = new StubDeviceDetector();
            var elevationProvider = new StubElevationProvider();
            var capabilityProvider = new WindowsCapabilityProvider(elevationProvider, ProfileResolver);
            SessionStore = new InMemoryPrivacySessionStore();

            ProtectionProvider = new WindowsProtectionProvider(
                detector,
                SpyDeviceController,
                FakeCoreAudio,
                SpyPolicyManager,
                UserPrivacyManager,
                elevationProvider,
                SessionStore,
                ProfileResolver);

            SessionAdapter = new WindowsPrivacySessionPlatformAdapter(
                detector,
                SpyDeviceController,
                FakeCoreAudio,
                ProfileResolver);

            var sessionService = new PrivacySessionService(SessionStore, SessionAdapter);
            var stateStore = new InMemoryStateStore();
            SettingsLauncher = new WindowsPrivacySettingsLauncher(
                ProfileResolver,
                uri =>
                {
                    LaunchedUris.Add(uri);
                    return true;
                });

            ProtectionService = new ProtectionService(
                ProtectionProvider,
                detector,
                capabilityProvider,
                stateStore,
                sessionService,
                ProfileResolver,
                SettingsLauncher);
        }
    }

    private sealed class StubPackageIdentityDetector : IPackageIdentityDetector
    {
        public StubPackageIdentityDetector(
            PackageIdentityKind kind,
            string? familyName = "cdevstudios.PrivGuard_tv7b4yqg10sw2",
            string? fullName = "cdevstudios.PrivGuard_2.0.0.0_x64__tv7b4yqg10sw2")
        {
            Kind = kind;
            PackageFamilyName = kind == PackageIdentityKind.NotPackaged ? null : familyName;
            PackageFullName = kind == PackageIdentityKind.NotPackaged ? null : fullName;
        }

        public PackageIdentityKind Kind { get; }
        public string? PackageFullName { get; }
        public string? PackageFamilyName { get; }
        public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
    }

    private sealed class SpyUserPrivacyManager : WindowsUserPrivacyManager
    {
        private readonly IEditionProfileResolver _profileResolver;
        public int ConsentStoreWriteCount { get; private set; }
        public BlockStatus CameraStatus { get; set; } = BlockStatus.Allowed;
        public BlockStatus MicrophoneStatus { get; set; } = BlockStatus.Allowed;

        public SpyUserPrivacyManager(IEditionProfileResolver profileResolver)
            : base(profileResolver, (_, _, _) => throw new InvalidOperationException("Direct registry writer should never be reached in spy"))
        {
            _profileResolver = profileResolver;
        }

        public override BlockStatus GetCameraUserPrivacyStatus() =>
            _profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate
                ? BlockStatus.Unknown
                : CameraStatus;

        public override BlockStatus GetMicrophoneUserPrivacyStatus() =>
            _profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate
                ? BlockStatus.Unknown
                : MicrophoneStatus;

        public override OperationResult SetCameraUserPrivacy(BlockStatus status)
        {
            if (_profileResolver.Profile != EditionProfile.Standalone)
            {
                return base.SetCameraUserPrivacy(status);
            }

            ConsentStoreWriteCount++;
            CameraStatus = status;
            return OperationResult.Ok();
        }

        public override OperationResult SetMicrophoneUserPrivacy(BlockStatus status)
        {
            if (_profileResolver.Profile != EditionProfile.Standalone)
            {
                return base.SetMicrophoneUserPrivacy(status);
            }

            ConsentStoreWriteCount++;
            MicrophoneStatus = status;
            return OperationResult.Ok();
        }
    }

    private sealed class FakeCoreAudioController : WindowsCoreAudioController
    {
        private readonly IEditionProfileResolver _profileResolver;
        public bool IsMuted { get; private set; }
        public int MuteWriteCount { get; private set; }
        private AudioEndpointHardwareMuteSupport _hardwareSupport = AudioEndpointHardwareMuteSupport.Supported;
        private List<WindowsAudioEndpointMuteState> _endpoints = [];

        public FakeCoreAudioController(IEditionProfileResolver profileResolver)
            : base(profileResolver)
        {
            _profileResolver = profileResolver;
            SetSimulatedEndpoints(false, AudioEndpointHardwareMuteSupport.Supported);
        }

        public void SetSimulatedEndpoints(bool isMuted, AudioEndpointHardwareMuteSupport hardwareSupport)
        {
            IsMuted = isMuted;
            _hardwareSupport = hardwareSupport;
            _endpoints =
            [
                new WindowsAudioEndpointMuteState(
                    "{0.0.1.00000000}.{11111111-2222-3333-4444-555555555555}",
                    isMuted,
                    hardwareSupport)
            ];
        }

        public override WindowsAudioEndpointQueryResult GetMicrophoneMuteStates()
        {
            if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            {
                return base.GetMicrophoneMuteStates();
            }

            return WindowsAudioEndpointQueryResult.Ok(_endpoints.ToList());
        }

        public override WindowsAudioEndpointLookupResult GetMicrophoneMuteState(string endpointId)
        {
            if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            {
                return base.GetMicrophoneMuteState(endpointId);
            }

            var match = _endpoints.FirstOrDefault(e => string.Equals(e.EndpointId, endpointId, StringComparison.OrdinalIgnoreCase));
            return match == null
                ? WindowsAudioEndpointLookupResult.Missing()
                : WindowsAudioEndpointLookupResult.Found(match.IsMuted, match.HardwareMuteSupport);
        }

        public override OperationResult SetMicrophoneMute(string endpointId, bool mute)
        {
            if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            {
                return base.SetMicrophoneMute(endpointId, mute);
            }

            MuteWriteCount++;
            IsMuted = mute;
            _endpoints =
            [
                new WindowsAudioEndpointMuteState(endpointId, mute, _hardwareSupport)
            ];
            return OperationResult.Ok();
        }
    }

    private sealed class SpyPolicyManager : WindowsPolicyManager
    {
        private readonly IEditionProfileResolver _profileResolver;
        public int HklmWriteCount { get; private set; }
        public BlockStatus CameraPolicyStatus { get; private set; } = BlockStatus.Allowed;
        public BlockStatus MicrophonePolicyStatus { get; private set; } = BlockStatus.Allowed;

        public SpyPolicyManager(IEditionProfileResolver profileResolver)
            : base(profileResolver)
        {
            _profileResolver = profileResolver;
        }

        public override BlockStatus GetCameraPolicyStatus() =>
            _profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate
                ? BlockStatus.Unknown
                : CameraPolicyStatus;

        public override BlockStatus GetMicrophonePolicyStatus() =>
            _profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate
                ? BlockStatus.Unknown
                : MicrophonePolicyStatus;

        public override OperationResult SetPolicy(BlockTarget target, BlockStatus status)
        {
            if (_profileResolver.Profile != EditionProfile.Standalone)
            {
                return base.SetPolicy(target, status);
            }

            HklmWriteCount++;
            if (target is BlockTarget.Camera or BlockTarget.Both)
            {
                CameraPolicyStatus = status;
            }
            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                MicrophonePolicyStatus = status;
            }

            return OperationResult.Ok();
        }
    }

    private sealed class SpyDeviceController : WindowsDeviceController
    {
        private readonly IEditionProfileResolver _profileResolver;
        public int PnpMutationCount { get; private set; }
        private bool _cameraEnabled = true;

        public SpyDeviceController(IEditionProfileResolver profileResolver)
            : base(profileResolver)
        {
            _profileResolver = profileResolver;
        }

        public override WindowsDeviceNodeState GetDeviceNodeState(string instanceId)
        {
            if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
            {
                return base.GetDeviceNodeState(instanceId);
            }

            return WindowsDeviceNodeState.Present(0, _cameraEnabled ? 0u : 22u);
        }

        public override Task<OperationResult> DisableDevicesAsync(IEnumerable<DeviceInfo> devices)
        {
            if (_profileResolver.Profile != EditionProfile.Standalone)
            {
                return base.DisableDevicesAsync(devices);
            }

            PnpMutationCount++;
            _cameraEnabled = false;
            return Task.FromResult(OperationResult.Ok());
        }

        public override Task<OperationResult> EnableDevicesAsync(IEnumerable<DeviceInfo> devices)
        {
            if (_profileResolver.Profile != EditionProfile.Standalone)
            {
                return base.EnableDevicesAsync(devices);
            }

            PnpMutationCount++;
            _cameraEnabled = true;
            return Task.FromResult(OperationResult.Ok());
        }
    }

    private sealed class StubDeviceDetector : IDeviceDetector
    {
        private static readonly IReadOnlyList<DeviceInfo> Cameras =
        [
            new DeviceInfo
            {
                Id = @"USB\VID_1234&PID_5678\5&11111111&0&1",
                FriendlyName = "Simulated USB Camera",
                DeviceType = DeviceType.Camera,
                IsEnabled = true
            }
        ];

        private static readonly IReadOnlyList<DeviceInfo> Microphones =
        [
            new DeviceInfo
            {
                Id = @"SWD\MMDEVAPI\{0.0.1.00000000}.{11111111-2222-3333-4444-555555555555}",
                FriendlyName = "Simulated USB Microphone",
                DeviceType = DeviceType.Microphone,
                IsEnabled = true
            }
        ];

        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Cameras);

        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Microphones);

        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>(Cameras.Concat(Microphones).ToList());
    }

    private sealed class StubElevationProvider : IElevationProvider
    {
        public bool IsElevated => false;
        public Task<ElevationResult> RequestElevationAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(ElevationResult.Fail("Elevation disabled in test harness."));
    }

    private sealed class StubAutostartProvider : IAutostartProvider
    {
        public Task<AutostartStatus> GetAutostartStatusAsync() =>
            Task.FromResult(new AutostartStatus(AutostartState.Disabled));
        public Task<OperationResult> EnableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
        public Task<OperationResult> DisableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
    }

    private sealed class InMemoryStateStore : IStateStore
    {
        private DesiredState _state = new();
        public DesiredState Load() => _state;
        public void Save(DesiredState state) => _state = state;
    }

    private sealed class InMemoryPrivacySessionStore : IPrivacySessionStore
    {
        private PrivacySession? _session;
        public PrivacySession? Load() => _session;
        public void Save(PrivacySession session) => _session = session;
    }

    private sealed class SimulatedStandalonePlatform : IDeviceProtectionProvider, IPrivacySessionPlatformAdapter
    {
        private readonly IEditionProfileResolver _profileResolver;
        private readonly IPrivacySessionStore _sessionStore;
        private bool _cameraStandardActive;
        private bool _micStandardActive;
        private bool _cameraSecureActive;

        public int StandardEnableCalls { get; private set; }
        public int SecureEnableCalls { get; private set; }

        public SimulatedStandalonePlatform(IEditionProfileResolver profileResolver, IPrivacySessionStore sessionStore)
        {
            _profileResolver = profileResolver;
            _sessionStore = sessionStore;
        }

        public bool SupportsPersistentRecovery => _profileResolver.Profile == EditionProfile.Standalone;

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
                list.Add(new OriginalAudioEndpointState
                {
                    ResourceId = $"sim:{layer}:{BlockTarget.Camera}",
                    OperationId = operationId,
                    Layer = layer,
                    Target = BlockTarget.Camera,
                    EndpointId = $"ep-{BlockTarget.Camera}",
                    OriginalMutedState = false,
                    ProtectedMutedState = true
                });
            }

            if (target is BlockTarget.Microphone or BlockTarget.Both)
            {
                list.Add(new OriginalAudioEndpointState
                {
                    ResourceId = $"sim:{layer}:{BlockTarget.Microphone}",
                    OperationId = operationId,
                    Layer = layer,
                    Target = BlockTarget.Microphone,
                    EndpointId = $"ep-{BlockTarget.Microphone}",
                    OriginalMutedState = false,
                    ProtectedMutedState = true
                });
            }

            return Task.FromResult<IReadOnlyList<PrivacyResourceState>>(list);
        }

        public Task<PrivacyResourceObservation> ObserveAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            var isProtected = resource.Layer == ProtectionLayer.Secure
                ? _cameraSecureActive
                : (resource.Target == BlockTarget.Camera ? _cameraStandardActive : _micStandardActive);

            return Task.FromResult(new PrivacyResourceObservation(
                isProtected
                    ? PrivacyResourceObservationKind.MatchesProtected
                    : PrivacyResourceObservationKind.MatchesOriginal));
        }

        public Task<PrivacyOwnershipAttestation> VerifyOwnershipAttestationAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.Confirmed));

        public Task<OperationResult> RestoreOriginalAsync(
            PrivacyResourceState resource,
            CancellationToken cancellationToken = default)
        {
            if (resource.Layer == ProtectionLayer.Secure)
                _cameraSecureActive = false;
            else if (resource.Target == BlockTarget.Camera)
                _cameraStandardActive = false;
            else
                _micStandardActive = false;

            return Task.FromResult(OperationResult.Ok());
        }

        public void CompleteRecoveryPass() { }

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            StandardEnableCalls++;
            if (target is BlockTarget.Camera or BlockTarget.Both)
                _cameraStandardActive = true;
            if (target is BlockTarget.Microphone or BlockTarget.Both)
                _micStandardActive = true;

            var session = _sessionStore.Load();
            var details = session?.Resources
                .Where(r => r.Layer == ProtectionLayer.Standard)
                .Select(r => new DeviceOperationDetail { DeviceId = r.ResourceId, FriendlyName = r.ResourceId, Success = true })
                .ToList() ?? [];

            return Task.FromResult(OperationResult.Ok(details));
        }

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default)
        {
            SecureEnableCalls++;
            _cameraSecureActive = true;
            var session = _sessionStore.Load();
            var details = session?.Resources
                .Where(r => r.Layer == ProtectionLayer.Secure)
                .Select(r => new DeviceOperationDetail { DeviceId = r.ResourceId, FriendlyName = r.ResourceId, Success = true })
                .ToList() ?? [];

            return Task.FromResult(OperationResult.Ok(details));
        }

        public Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<BlockState> GetBlockStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new BlockState());

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    Availability = TargetDeviceAvailability.Available,
                    StandardState = _cameraStandardActive ? StandardProtectionState.Active : StandardProtectionState.Inactive,
                    SecureState = _cameraSecureActive
                        ? SecureProtectionState.Active
                        : (_cameraStandardActive ? SecureProtectionState.Available : SecureProtectionState.Unavailable),
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    Availability = TargetDeviceAvailability.Available,
                    EndpointAvailability = AudioEndpointAvailability.Available,
                    StandardState = _micStandardActive ? StandardProtectionState.Active : StandardProtectionState.Inactive,
                    SecureState = _micStandardActive ? SecureProtectionState.Available : SecureProtectionState.Unavailable,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            });
    }
}

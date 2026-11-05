using Avalonia.Threading;
using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Localization;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows;
using PrivLock.Platform.Windows.Devices;
using PrivLock.Platform.Windows.Policies;
using PrivLock.UI.ViewModels;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

[Collection("AvaloniaUI")]
public sealed class LayerVerificationAndRecoveryUiTests
{
    private static MainViewModel CreateTestViewModel(
        ProtectionService? protectionService = null,
        string language = "en")
    {
        var stateStore = new TestStateStore();
        var autostartProvider = new FakeAutostartProvider();
        protectionService ??= new ProtectionService(
            null!,
            null!,
            new TestCapabilityProvider(),
            stateStore);
        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();
        localizationService.SetLanguage(language);

        var sharedState = new AutostartStateService(
            new SettingsService(autostartProvider, stateStore),
            new NoOpAutostartChangeObserver());

        return new MainViewModel(
            protectionService,
            sharedState,
            localizationService);
    }

    // =========================================================================
    // SECTION 7: ACCEPTANCE MATRIX TESTS (Cases A through M)
    // =========================================================================

    // --- Case A: Standard Active verificado, Secure Unknown, WASAPI Available, DevNode Detectado ---
    [Fact]
    public void CaseA_StandardActive_SecureUnknown_PreservesStandardActive_AndEnablesSafeRestoration()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // Standard Active is preserved (NOT displayed as Standard Unverifiable)
        Assert.Contains("Standard Active", vm.MicStandardText);
        Assert.Equal("#E57373", vm.MicStandardColor);

        // Overall status does not claim full protection or total verification
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.Equal("#FFA726", vm.MicStatusColor);

        // Safe restoration route: User CAN toggle off to unblock/restore!
        Assert.True(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled);

        // Secure action is disabled because Secure is Unknown
        Assert.False(vm.IsMicSecureButtonEnabled);
    }

    // --- Case B: Standard Inactive verificado, Secure Unknown, WASAPI Available, DevNode Detectado ---
    [Fact]
    public void CaseB_StandardInactive_SecureUnknown_PreservesStandardInactive_NeverShowsAllowedTotal_DisablesSecureAction()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // Preserves Standard Inactive
        Assert.Contains("Standard Inactive", vm.MicStandardText);
        Assert.Equal("#81C784", vm.MicStandardColor);

        // NEVER claims Allowed total because Secure is Unknown
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.Equal("#FFA726", vm.MicStatusColor);

        // User can enable Standard Protection
        Assert.False(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled);

        // New Secure action is disabled
        Assert.False(vm.IsMicSecureButtonEnabled);
    }

    // --- Case C: Standard Active verificado, Secure Unknown, WASAPI Available, DevNode No detectado ---
    [Fact]
    public void CaseC_StandardActive_SecureUnknown_ZeroDevNodes_PreservesSafeRestorationRoute()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available, // endpoint evidence keeps availability
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // Does not leave Standard without a safe restoration route
        Assert.True(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled);
        Assert.Contains("Standard Active", vm.MicStandardText);
    }

    // --- Case D: Standard Unknown, Secure Unknown, WASAPI Query failed, DevNode Detectado ---
    [Fact]
    public void CaseD_StandardUnknown_SecureUnknown_QueryFailed_PnpDetected_DisablesAllMutations()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Unknown,
                StandardIsVerified = false,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // Disables new mutations
        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        // Neutral diagnostic, never claims Allowed nor confirmed absent
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.DoesNotContain("No input microphones detected", vm.MicStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
    }

    // --- Case E: Standard Unknown, Secure Unknown, WASAPI Query failed, DevNode No detectado ---
    [Fact]
    public void CaseE_StandardUnknown_SecureUnknown_QueryFailed_PnpEmpty_NeverConfirmedAbsentNorAllowed()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Unknown, // query failed, cannot confirm absence
                EndpointAvailability = AudioEndpointAvailability.Unknown,
                StandardIsVerified = false,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsMicToggleEnabled);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.DoesNotContain("No input microphones detected", vm.MicStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
    }

    // --- Case F: Standard Active verificado, Secure Active verificado, WASAPI Endpoint desaparecido, DevNode Detectado ---
    [Fact]
    public void CaseF_StandardActive_SecureActive_EndpointMissing_DevNodeDisabled_RestorationAvailable()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Active,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.NotDetected, // endpoint disappeared while blocked
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            }
        };

        vm.UpdateUi(state, []);

        Assert.True(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled); // Restoration route available
        Assert.True(vm.IsMicSecureButtonEnabled);
        Assert.Contains("Blocked (Advanced)", vm.MicStatusText);
    }

    // --- Case G: Standard Active verificado, Secure Unknown, Zero endpoints, DevNode Detectado ---
    [Fact]
    public void CaseG_StandardActive_SecureUnknown_ZeroEndpoints_DevNodeDetected_RestorationNotBlocked()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.NotDetected, // zero endpoints
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // WASAPI uncertainty must not silently block WAL-backed restoration route
        Assert.True(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled);
        Assert.Contains("Standard Active", vm.MicStandardText);
    }

    // --- Case H: Standard Inactive verificado, Secure Unknown, WASAPI Available, DevNode Query WMI fallida ---
    [Fact]
    public void CaseH_StandardInactive_SecureUnknown_DevNodeQueryFailed_PreservesStandardInactive()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Unknown,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.Contains("Standard Inactive", vm.MicStandardText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.False(vm.IsMicSecureButtonEnabled);
    }

    // --- Case I: Camera Active verificado, Camera Secure Unknown, Camera Detectada ---
    [Fact]
    public void CaseI_CameraStandardActive_CameraSecureUnknown_PreservesStandard_AllowsRestoration()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            }
        };

        vm.UpdateUi(state, []);

        // Preserves Standard Active on Camera
        Assert.Contains("Standard Active", vm.CameraStandardText);
        Assert.Equal("#E57373", vm.CameraStandardColor);
        Assert.Contains("Unverifiable", vm.CameraStatusText);

        // Allows safe restoration
        Assert.True(vm.IsCameraBlocked);
        Assert.True(vm.IsCameraToggleEnabled);
        Assert.False(vm.IsCameraSecureButtonEnabled);
    }

    // --- Case J: Ambos conocidos y verificados (Regresión) ---
    [Fact]
    public void CaseJ_BothKnownAndVerified_AllowedAndBlockedRegression()
    {
        var vm = CreateTestViewModel(language: "en");

        // Allowed case
        var allowedState = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            }
        };

        vm.UpdateUi(allowedState, []);
        Assert.True(vm.IsBothToggleEnabled);
        Assert.False(vm.IsBothBlocked);
        Assert.Contains("Allowed", vm.BothStatusText);

        // Blocked case
        var blockedState = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            }
        };

        vm.UpdateUi(blockedState, []);
        Assert.True(vm.IsBothToggleEnabled);
        Assert.True(vm.IsBothBlocked);
        Assert.Contains("Blocked", vm.BothStatusText);
    }

    // --- Case K: Una capa protegida, otra desconocida (Mixto) ---
    [Fact]
    public void CaseK_MixedOneProtectedOneUnknown_BothReflectsPartial_DoesNotBlockIndividualRestoration()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // Both reflects partially unverifiable
        Assert.Contains("Unverifiable", vm.BothStatusText);
        Assert.False(vm.IsBothToggleEnabled); // Mismatched blocked states disable Both toggle

        // Does NOT block individual safe restoration of the protected resource
        Assert.True(vm.IsCameraToggleEnabled); // Camera is active and can be unblocked!
        Assert.True(vm.IsMicToggleEnabled);    // Mic is inactive and can be blocked!
    }

    // --- Case M: WAL sin ownership activo, observación Unknown, query failed, no detectado ---
    [Fact]
    public void CaseM_NoActiveOwnership_BothUnknown_NoDestructiveMutationOffered()
    {
        var vm = CreateTestViewModel(language: "en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Unknown,
                StandardIsVerified = false,
                SecureIsVerified = false,
                IsVerified = false
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
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        // No destructive mutations offered
        Assert.False(vm.IsCameraToggleEnabled);
        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsBothToggleEnabled);
        Assert.Contains("Unverifiable", vm.CameraStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
    }

    // =========================================================================
    // SECTION 8: REAL BEHAVIOR & INTEGRATION TESTS
    // =========================================================================

    [Fact]
    public async Task WindowsProtectionProvider_GetProtectionStateAsync_ComposesCaseF_WasapiPresentWithZeroPnpDevices()
    {
        var deviceDetector = new FakeDeviceDetector
        {
            Cameras = [new DeviceInfo { Id = "cam-1", FriendlyName = "Integrated Camera", DeviceType = DeviceType.Camera, IsPresent = true, IsEnabled = true }],
            Microphones = [] // 0 WMI/PnP DevNodes
        };

        var coreAudio = new FakeCoreAudioController
        {
            QueryResult = WindowsAudioEndpointQueryResult.Ok(
            [
                new WindowsAudioEndpointMuteState("endpoint-1", false, AudioEndpointHardwareMuteSupport.Supported)
            ])
        };

        var policyManager = new FakePolicyManager
        {
            CameraPolicy = BlockStatus.Allowed,
            MicrophonePolicy = BlockStatus.Allowed
        };

        var userPrivacyManager = new FakeUserPrivacyManager
        {
            CameraPrivacy = BlockStatus.Allowed,
            MicrophonePrivacy = BlockStatus.Allowed
        };

        var elevationProvider = new FakeElevationProvider();
        var sessionStore = new InMemoryPrivacySessionStore();

        var provider = new WindowsProtectionProvider(
            deviceDetector,
            new WindowsDeviceController(),
            coreAudio,
            policyManager,
            userPrivacyManager,
            elevationProvider,
            sessionStore);

        var state = await provider.GetProtectionStateAsync();

        // Microphone should have EndpointAvailable and therefore Hardware Available (Case F!)
        Assert.Equal(AudioEndpointAvailability.Available, state.Microphone.EndpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Available, state.Microphone.Availability);
        Assert.True(state.Microphone.StandardIsVerified);
    }

    [Fact]
    public async Task WindowsProtectionProvider_GetProtectionStateAsync_SeparatesStandardAndSecureVerifications()
    {
        var deviceDetector = new FakeDeviceDetector
        {
            Cameras = [new DeviceInfo { Id = "cam-1", FriendlyName = "Integrated Camera", DeviceType = DeviceType.Camera, IsPresent = true, IsEnabled = true }],
            Microphones = [new DeviceInfo { Id = "mic-1", FriendlyName = "Microphone", DeviceType = DeviceType.Microphone, IsPresent = true, IsEnabled = true }]
        };

        var coreAudio = new FakeCoreAudioController
        {
            QueryResult = WindowsAudioEndpointQueryResult.Ok(
            [
                new WindowsAudioEndpointMuteState("endpoint-1", false, AudioEndpointHardwareMuteSupport.Supported)
            ])
        };

        var policyManager = new FakePolicyManager
        {
            CameraPolicy = BlockStatus.Unknown, // Secure policy Unknown!
            MicrophonePolicy = BlockStatus.Unknown // Secure policy Unknown!
        };

        var userPrivacyManager = new FakeUserPrivacyManager
        {
            CameraPrivacy = BlockStatus.Allowed, // Standard Known Allowed
            MicrophonePrivacy = BlockStatus.Allowed // Standard Known Allowed
        };

        var elevationProvider = new FakeElevationProvider();
        var sessionStore = new InMemoryPrivacySessionStore();

        var provider = new WindowsProtectionProvider(
            deviceDetector,
            new WindowsDeviceController(),
            coreAudio,
            policyManager,
            userPrivacyManager,
            elevationProvider,
            sessionStore);

        var state = await provider.GetProtectionStateAsync();

        // Standard is verified, but Secure is NOT verified!
        Assert.True(state.Microphone.StandardIsVerified);
        Assert.False(state.Microphone.SecureIsVerified);
        Assert.False(state.Microphone.IsVerified); // Aggregated is false

        Assert.True(state.Camera.StandardIsVerified);
        Assert.False(state.Camera.SecureIsVerified);
        Assert.False(state.Camera.IsVerified); // Aggregated is false
    }

    [Fact]
    public async Task MainViewModel_TogglingOffBlockedMicrophone_CallsProtectionServiceAndReachesPrivacySessionRestore()
    {
        var sessionStore = new InMemoryPrivacySessionStore();
        var platformAdapter = new FakePrivacySessionPlatformAdapter();

        // Prepare an active session in WAL representing a blocked microphone
        var sessionId = Guid.NewGuid();
        var activeSession = new PrivacySession
        {
            SessionId = sessionId,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Resources =
            [
                new OriginalPolicyState
                {
                    ResourceId = "mic-consent-1",
                    OperationId = "op-1",
                    Layer = ProtectionLayer.Standard,
                    Target = BlockTarget.Microphone,
                    RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone",
                    ValueName = "Value",
                    OriginalValue = "Allow",
                    ProtectedValue = "Deny",
                    JournalState = PrivacyResourceJournalState.Applied,
                    ModifiedByPrivLock = true
                }
            ]
        };
        sessionStore.Save(activeSession);

        var privacySessionService = new PrivacySessionService(sessionStore, platformAdapter);

        var provider = new FakeDeviceProtectionProvider();
        var stateStore = new TestStateStore();
        var capabilityProvider = new TestCapabilityProvider();
        var deviceDetector = new FakeDeviceDetector();

        var protectionService = new ProtectionService(
            provider,
            deviceDetector,
            capabilityProvider,
            stateStore,
            privacySessionService);

        var autostartProvider = new FakeAutostartProvider();
        var localizationService = new LocalizationService(stateStore);
        localizationService.Initialize();

        var sharedState = new AutostartStateService(
            new SettingsService(autostartProvider, stateStore),
            new NoOpAutostartChangeObserver());

        var vm = new MainViewModel(
            protectionService,
            sharedState,
            localizationService);

        // Setup UI with blocked microphone but Secure unknown (Case A)
        var protectionState = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = false,
                IsVerified = false
            }
        };

        vm.UpdateUi(protectionState, []);

        Assert.True(vm.IsMicBlocked);
        Assert.True(vm.IsMicToggleEnabled);

        // User unblocks by flipping the toggle
        vm.IsMicBlocked = false;

        // Allow async dispatcher jobs / tasks to execute
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (platformAdapter.RestoredResourceIds.Count == 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
            Dispatcher.UIThread.RunJobs();
        }

        // Verify that platformAdapter.RestoreOriginalAsync was invoked for the owned resource!
        Assert.Contains("mic-consent-1", platformAdapter.RestoredResourceIds);

        // Verify that the WAL session on store was updated
        var loadedSession = sessionStore.Load();
        Assert.NotNull(loadedSession);
        var res = loadedSession.Resources.First(r => r.ResourceId == "mic-consent-1");
        Assert.Equal(PrivacyResourceJournalState.Restored, res.JournalState);
    }

    [Fact]
    public async Task MainViewModel_RecoveryConflict_DoesNotUpdateDesiredStateToInactive()
    {
        var sessionStore = new InMemoryPrivacySessionStore();
        // No session in WAL: microphone was blocked externally or by an untracked process
        var provider = new FakeDeviceProtectionProvider
        {
            StateToReturn = new FullProtectionState
            {
                Camera = new TargetProtectionStatus
                {
                    Target = BlockTarget.Camera,
                    StandardState = StandardProtectionState.Inactive,
                    SecureState = SecureProtectionState.Unavailable,
                    Availability = TargetDeviceAvailability.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                },
                Microphone = new TargetProtectionStatus
                {
                    Target = BlockTarget.Microphone,
                    StandardState = StandardProtectionState.Active,
                    SecureState = SecureProtectionState.Unavailable,
                    Availability = TargetDeviceAvailability.Available,
                    EndpointAvailability = AudioEndpointAvailability.Available,
                    StandardIsVerified = true,
                    SecureIsVerified = true,
                    IsVerified = true
                }
            }
        };

        var platformAdapter = new FakePrivacySessionPlatformAdapter();
        var privacySessionService = new PrivacySessionService(sessionStore, platformAdapter);
        var stateStore = new TestStateStore();
        stateStore.Save(new DesiredState
        {
            MicrophoneStandard = StandardProtectionState.Active
        });

        var protectionService = new ProtectionService(
            provider,
            new FakeDeviceDetector(),
            new TestCapabilityProvider(),
            stateStore,
            privacySessionService);

        var result = await protectionService.UnblockDeviceAsync(BlockTarget.Microphone);

        // Unblock should fail due to conflict (external state without PrivLock ownership snapshot)
        Assert.False(result.Success);
        Assert.Contains("ownership snapshot", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // DesiredState must NOT be silently cleared or converted to Inactive on conflict!
        var desired = stateStore.Load();
        Assert.Equal(StandardProtectionState.Active, desired.MicrophoneStandard);
    }

    // =========================================================================
    // TEST DOUBLES
    // =========================================================================

    private sealed class TestStateStore : IStateStore
    {
        public DesiredState State { get; private set; } = new();
        public DesiredState Load() => State;
        public void Save(DesiredState state) => State = state;
    }

    private sealed class InMemoryPrivacySessionStore : IPrivacySessionStore
    {
        private PrivacySession? _session;
        public PrivacySession? Load() => _session;
        public void Save(PrivacySession session) => _session = session;
    }

    private sealed class FakePrivacySessionPlatformAdapter : IPrivacySessionPlatformAdapter
    {
        public bool SupportsPersistentRecovery => true;
        public List<string> RestoredResourceIds { get; } = [];
        public PrivacyResourceObservation? ObservationToReturn { get; set; }

        public Task<OperationResult> EnsureMutationQuiescenceAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<IReadOnlyList<PrivacyResourceState>> CaptureAsync(
            ProtectionLayer layer, BlockTarget target, string operationId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PrivacyResourceState>>([]);

        public Task<PrivacyResourceObservation> ObserveAsync(
            PrivacyResourceState resource, CancellationToken cancellationToken = default)
        {
            if (ObservationToReturn != null)
                return Task.FromResult(ObservationToReturn);

            if (RestoredResourceIds.Contains(resource.ResourceId))
                return Task.FromResult(new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesOriginal));

            return Task.FromResult(new PrivacyResourceObservation(PrivacyResourceObservationKind.MatchesProtected));
        }

        public Task<OperationResult> RestoreOriginalAsync(
            PrivacyResourceState resource, CancellationToken cancellationToken = default)
        {
            RestoredResourceIds.Add(resource.ResourceId);
            return Task.FromResult(OperationResult.Ok());
        }

        public Task<PrivacyOwnershipAttestation> VerifyOwnershipAttestationAsync(
            PrivacyResourceState resource, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PrivacyOwnershipAttestation(PrivacyOwnershipAttestationKind.Confirmed));

        public void CompleteRecoveryPass() { }
    }

    private sealed class FakeDeviceDetector : IDeviceDetector
    {
        public List<DeviceInfo> Cameras { get; set; } = [];
        public List<DeviceInfo> Microphones { get; set; } = [];

        public Task<IReadOnlyList<DeviceInfo>> DetectCamerasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>(Cameras);

        public Task<IReadOnlyList<DeviceInfo>> DetectMicrophonesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DeviceInfo>>(Microphones);

        public Task<IReadOnlyList<DeviceInfo>> DetectAllAsync(CancellationToken cancellationToken = default)
        {
            var all = new List<DeviceInfo>(Cameras);
            all.AddRange(Microphones);
            return Task.FromResult<IReadOnlyList<DeviceInfo>>(all);
        }
    }

    private sealed class FakeCoreAudioController : WindowsCoreAudioController
    {
        public WindowsAudioEndpointQueryResult QueryResult { get; set; } = WindowsAudioEndpointQueryResult.Ok([]);
        public override WindowsAudioEndpointQueryResult GetMicrophoneMuteStates() => QueryResult;
    }

    private sealed class FakePolicyManager : WindowsPolicyManager
    {
        public BlockStatus CameraPolicy { get; set; } = BlockStatus.Allowed;
        public BlockStatus MicrophonePolicy { get; set; } = BlockStatus.Allowed;

        public override BlockStatus GetCameraPolicyStatus() => CameraPolicy;
        public override BlockStatus GetMicrophonePolicyStatus() => MicrophonePolicy;
    }

    private sealed class FakeUserPrivacyManager : WindowsUserPrivacyManager
    {
        public BlockStatus CameraPrivacy { get; set; } = BlockStatus.Allowed;
        public BlockStatus MicrophonePrivacy { get; set; } = BlockStatus.Allowed;

        public override BlockStatus GetCameraUserPrivacyStatus() => CameraPrivacy;
        public override BlockStatus GetMicrophoneUserPrivacyStatus() => MicrophonePrivacy;
    }

    private sealed class FakeElevationProvider : IElevationProvider
    {
        public bool IsElevated => false;
        public Task<ElevationResult> RequestElevationAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(ElevationResult.Cancelled());
    }

    private sealed class FakeDeviceProtectionProvider : IDeviceProtectionProvider
    {
        public FullProtectionState StateToReturn { get; set; } = new()
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                StandardIsVerified = true,
                SecureIsVerified = true,
                IsVerified = true
            }
        };

        public Task<OperationResult> EnableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> DisableStandardProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> EnableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<OperationResult> DisableSecureProtectionAsync(BlockTarget target, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Ok());

        public Task<FullProtectionState> GetProtectionStateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(StateToReturn);
    }

    private sealed class FakeAutostartProvider : IAutostartProvider
    {
        public AutostartStatus EffectiveStatus { get; set; } = new(AutostartState.Disabled);
        public Task<AutostartStatus> GetAutostartStatusAsync() =>
            Task.FromResult(EffectiveStatus);
        public Task<OperationResult> EnableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
        public Task<OperationResult> DisableAutostartAsync() =>
            Task.FromResult(OperationResult.Ok());
    }

    private sealed class TestCapabilityProvider : IPlatformCapabilityProvider
    {
        public PlatformCapabilities Capabilities { get; } = new();
        public PlatformInfo PlatformInfo { get; } = new()
        {
            OperatingSystemName = "Windows",
            OsVersion = "Windows 11",
            Architecture = "x64",
            Is64Bit = true,
            IsElevated = false
        };
    }

    private sealed class NoOpAutostartChangeObserver : IAutostartChangeObserver
    {
        public bool RequiresVisiblePolling => false;
        public Task ObserveChangesAsync(
            Action onChanged, Action<Exception> onError, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}

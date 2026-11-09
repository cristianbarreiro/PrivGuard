using Avalonia.Threading;
using PrivLock.Application.Services;
using PrivLock.Domain.Capabilities;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Localization;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.Devices;
using PrivLock.UI.ViewModels;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

[Collection("AvaloniaUI")]
public sealed class MicrophoneAvailabilityAndProtectionMatrixTests
{
    private static MainViewModel CreateTestViewModel(string language = "en")
    {
        var stateStore = new TestStateStore();
        var autostartProvider = new FakeAutostartProvider();
        var protectionService = new ProtectionService(
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

    // --- Case A: PnP with device and WASAPI with endpoint ---
    [Fact]
    public void CaseA_PnpDetected_And_WasapiEndpointAvailable_EvaluatedNormally()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 1);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 1,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.Available, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Available, hardwareAvailability);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = hardwareAvailability,
                EndpointAvailability = endpointAvailability,
                IsVerified = true
            }
        };

        vm.UpdateUi(state, []);

        Assert.True(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        Assert.Contains("Allowed", vm.MicStatusText);
        Assert.Equal("#81C784", vm.MicStatusColor);
    }

    // --- Case B: PnP with device; WASAPI with zero endpoints ---
    [Fact]
    public void CaseB_PnpDetected_And_ZeroWasapiEndpoints_DisablesToggle_NeverShowsAllowed()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 0);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 1,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.NotDetected, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Available, hardwareAvailability);

        var statusMessage = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            hardwareAvailability,
            endpointAvailability,
            StandardProtectionState.Unknown,
            SecureProtectionState.Unknown);
        Assert.Equal("Microphone detected, but no active capture endpoint available.", statusMessage);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unavailable,
                Availability = hardwareAvailability,
                EndpointAvailability = endpointAvailability,
                IsVerified = false,
                StatusMessage = statusMessage
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.Contains("Microphone detected, but no active capture endpoint", vm.MicStatusText);
        Assert.Equal("#FFA726", vm.MicStatusColor);
    }

    // --- Case C: PnP empty; WASAPI empty ---
    [Fact]
    public void CaseC_PnpEmpty_And_WasapiEmpty_NeutralAbsenceMessage_DisablesActions()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 0);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 0,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.NotDetected, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.NotDetected, hardwareAvailability);

        var statusMessage = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            hardwareAvailability,
            endpointAvailability,
            StandardProtectionState.Inactive,
            SecureProtectionState.Unavailable);
        Assert.Equal("No input microphones detected.", statusMessage);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = hardwareAvailability,
                EndpointAvailability = endpointAvailability,
                IsVerified = true,
                StatusMessage = statusMessage
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.Equal("No input microphones detected", vm.MicStatusText);
        Assert.Equal("#9E9E9E", vm.MicStatusColor);
    }

    // --- Case D: WASAPI query failed; PnP detects device ---
    [Fact]
    public void CaseD_WasapiQueryFailed_And_PnpDetected_EndpointUnknown_DisablesControls()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: false,
            endpointCount: 0);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 1,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.Unknown, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Available, hardwareAvailability);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = hardwareAvailability,
                EndpointAvailability = endpointAvailability,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.Equal("#FFA726", vm.MicStatusColor);
    }

    // --- Case E: WASAPI query failed; PnP empty ---
    [Fact]
    public void CaseE_WasapiQueryFailed_And_PnpEmpty_GlobalUnknown_NeverConfirmedAbsentNorAllowed()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: false,
            endpointCount: 0);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 0,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.Unknown, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Unknown, hardwareAvailability);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = hardwareAvailability,
                EndpointAvailability = endpointAvailability,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsMicToggleEnabled);
        Assert.False(vm.IsMicBlocked);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
        Assert.DoesNotContain("No input microphones detected", vm.MicStatusText);
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.Equal("#FFA726", vm.MicStatusColor);
    }

    // --- Case F: WASAPI OK with endpoint; PnP empty ---
    [Fact]
    public void CaseF_WasapiEndpointsPresent_And_PnpEmpty_InputEvidenceExists_NotAbsent()
    {
        var endpointAvailability = WindowsProtectionProvider.DetermineAudioEndpointAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 1);
        var hardwareAvailability = WindowsProtectionProvider.DetermineMicrophoneHardwareAvailability(
            deviceCount: 0,
            endpointAvailability: endpointAvailability);

        Assert.Equal(AudioEndpointAvailability.Available, endpointAvailability);
        Assert.Equal(TargetDeviceAvailability.Available, hardwareAvailability);
    }

    // --- Case G: State transition from Available to Failed/Unknown ---
    [Fact]
    public void CaseG_Transition_FromAvailable_To_FailedQuery_ClearsStaleTelemetryAndDisablesControls()
    {
        var vm = CreateTestViewModel("en");

        // Initial state: Available with hardware telemetry
        var initial = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                HardwareMuteSummary = new AudioHardwareMuteSummary
                {
                    SupportedCount = 1,
                    NotSupportedCount = 0,
                    UnknownCount = 0,
                    TotalCount = 1
                },
                IsVerified = true
            }
        };

        vm.UpdateUi(initial, []);
        Assert.True(vm.HasMicHardwareCapability);
        Assert.NotEmpty(vm.MicHardwareCapabilityText);
        Assert.True(vm.IsMicToggleEnabled);

        // Transition: Query failed
        var failed = new FullProtectionState
        {
            Camera = initial.Camera,
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Unknown,
                HardwareMuteSummary = null,
                IsVerified = false
            }
        };

        vm.UpdateUi(failed, []);

        Assert.False(vm.HasMicHardwareCapability);
        Assert.Empty(vm.MicHardwareCapabilityText);
        Assert.False(vm.IsMicToggleEnabled);
        Assert.Contains("Unverifiable", vm.MicStatusText);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
    }

    // --- Case H: State transition from Available to 0 endpoints ---
    [Fact]
    public void CaseH_Transition_FromAvailable_To_ZeroEndpoints_HidesTelemetry_ShowsAppropriateMessage()
    {
        var vm = CreateTestViewModel("en");

        var initial = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                HardwareMuteSummary = new AudioHardwareMuteSummary
                {
                    SupportedCount = 1,
                    NotSupportedCount = 0,
                    UnknownCount = 0,
                    TotalCount = 1
                },
                IsVerified = true
            }
        };

        vm.UpdateUi(initial, []);
        Assert.True(vm.HasMicHardwareCapability);

        // Transition: Endpoints vanished (PnP device still exists)
        var zeroEndpoints = new FullProtectionState
        {
            Camera = initial.Camera,
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.NotDetected,
                HardwareMuteSummary = null,
                IsVerified = false
            }
        };

        vm.UpdateUi(zeroEndpoints, []);

        Assert.False(vm.HasMicHardwareCapability);
        Assert.Empty(vm.MicHardwareCapabilityText);
        Assert.False(vm.IsMicToggleEnabled);
        Assert.Contains("Microphone detected, but no active capture endpoint", vm.MicStatusText);
        Assert.DoesNotContain("Allowed", vm.MicStatusText);
    }

    // --- Case I: ConsentStore Allowed but endpoint cannot be queried ---
    [Fact]
    public void CaseI_ConsentStoreAllowed_ButEndpointQueryFailed_ReturnsUnknown_NeverAllowed()
    {
        var result = WindowsProtectionProvider.EvaluateMicrophoneStandardStatus(
            consentStatus: BlockStatus.Allowed,
            endpointQuerySucceeded: false,
            endpoints: []);

        Assert.Equal(BlockStatus.Unknown, result);
    }

    // --- Case J: Standard active (ConsentStore Blocked) but 0 endpoints ---
    [Fact]
    public void CaseJ_StandardActiveConsent_WithZeroEndpoints_ReturnsUnknown_NotMuted()
    {
        var result = WindowsProtectionProvider.EvaluateMicrophoneStandardStatus(
            consentStatus: BlockStatus.Blocked,
            endpointQuerySucceeded: true,
            endpoints: []);

        Assert.Equal(BlockStatus.Unknown, result);
    }

    // --- Case K: Secure unknown or unverifiable ---
    [Fact]
    public void CaseK_SecureUnknown_DoesNotClaimPermitted_DisablesSecureAction()
    {
        var (isActive, isUnknown) = WindowsProtectionProvider.EvaluateSecureState(
            policyStatus: BlockStatus.Unknown,
            detectedDeviceCount: 1,
            deviceStateKnown: true,
            allDevicesDisabled: false,
            allDevicesEnabled: true);

        Assert.False(isActive);
        Assert.True(isUnknown);

        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = false
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsCameraSecureButtonEnabled);
        Assert.False(vm.IsMicSecureButtonEnabled);
        Assert.Contains("Unverifiable", vm.CameraSecureText);
        Assert.Contains("Unverifiable", vm.MicSecureText);
    }

    // --- Case L: Camera Allowed (verified), Microphone Unknown ---
    [Fact]
    public void CaseL_CameraAllowed_And_MicrophoneUnknown_BothNeverPresentsAllowed()
    {
        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Unknown,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsBothBlocked);
        Assert.False(vm.IsBothToggleEnabled);
        Assert.DoesNotContain("Allowed", vm.BothStatusText);
        Assert.Contains("Unverifiable", vm.BothStatusText);
        Assert.Equal("#FFA726", vm.BothStatusColor);
    }

    // --- Case M: Camera Blocked (verified), Microphone Unknown ---
    [Fact]
    public void CaseM_CameraBlocked_And_MicrophoneUnknown_DoesNotAffirmFullProtectionOfBoth()
    {
        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Unknown,
                SecureState = SecureProtectionState.Unknown,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Unknown,
                IsVerified = false
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsBothBlocked);
        Assert.False(vm.IsBothToggleEnabled);
        Assert.DoesNotContain("Blocked (Standard)", vm.BothStatusText);
        Assert.DoesNotContain("Allowed", vm.BothStatusText);
        Assert.Contains("Partially", vm.BothStatusText);
        Assert.Equal("#FFA726", vm.BothStatusColor);
    }

    // --- Case N: Both queried successfully and allowed ---
    [Fact]
    public void CaseN_BothVerifiedAllowed_CombinedStatusIsAllowed_ControlsEnabled()
    {
        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Inactive,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                IsVerified = true
            }
        };

        vm.UpdateUi(state, []);

        Assert.False(vm.IsBothBlocked);
        Assert.True(vm.IsBothToggleEnabled);
        Assert.True(vm.IsCameraToggleEnabled);
        Assert.True(vm.IsMicToggleEnabled);
        Assert.Contains("Allowed", vm.BothStatusText);
        Assert.Equal("#81C784", vm.BothStatusColor);
    }

    // --- Case O: Both protected and verified ---
    [Fact]
    public void CaseO_BothProtectedAndVerified_CombinedStatusIsBlocked()
    {
        var vm = CreateTestViewModel("en");
        var state = new FullProtectionState
        {
            Camera = new TargetProtectionStatus
            {
                Target = BlockTarget.Camera,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                IsVerified = true
            },
            Microphone = new TargetProtectionStatus
            {
                Target = BlockTarget.Microphone,
                StandardState = StandardProtectionState.Active,
                SecureState = SecureProtectionState.Unavailable,
                Availability = TargetDeviceAvailability.Available,
                EndpointAvailability = AudioEndpointAvailability.Available,
                IsVerified = true
            }
        };

        vm.UpdateUi(state, []);

        Assert.True(vm.IsBothBlocked);
        Assert.Contains("Blocked", vm.BothStatusText);
        Assert.Equal("#E57373", vm.BothStatusColor);
    }

    // --- Localization coverage for EN and ES ---
    [Theory]
    [InlineData("es", "MicrophoneDetectedNoEndpoints", "Micrófono detectado")]
    [InlineData("en", "MicrophoneDetectedNoEndpoints", "Microphone detected")]
    [InlineData("es", "StatusUnverifiable", "No verificable")]
    [InlineData("en", "StatusUnverifiable", "Unverifiable")]
    [InlineData("es", "StatusStandardUnknown", "Estándar No verificable")]
    [InlineData("en", "StatusStandardUnknown", "Standard Unverifiable")]
    [InlineData("es", "BadgeUnverifiable", "No verificable")]
    [InlineData("en", "BadgeUnverifiable", "Unverifiable")]
    public void LocalizationCatalog_ContainsExpectedTranslations(string lang, string key, string expectedSubstring)
    {
        var text = LocalizationCatalog.Get(key, lang);
        Assert.Contains(expectedSubstring, text);
    }

    // --- COM ParseHardwareMuteSupport safety regression test ---
    [Theory]
    [InlineData(0, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE, AudioEndpointHardwareMuteSupport.Supported)]
    [InlineData(1, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE, AudioEndpointHardwareMuteSupport.Supported)]
    [InlineData(0, 0u, AudioEndpointHardwareMuteSupport.NotSupported)]
    [InlineData(1, 0u, AudioEndpointHardwareMuteSupport.NotSupported)]
    [InlineData(-2147467259, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE, AudioEndpointHardwareMuteSupport.Unknown)]
    public void ParseHardwareMuteSupport_ComHresultSemantics_Preserved(int hr, uint mask, AudioEndpointHardwareMuteSupport expected)
    {
        var result = WindowsCoreAudioController.ParseHardwareMuteSupport(hr, mask);
        Assert.Equal(expected, result);
    }

    private sealed class TestStateStore : IStateStore
    {
        public DesiredState State { get; private set; } = new();

        public DesiredState Load() => State;

        public void Save(DesiredState state) => State = state;
    }

    private sealed class FakeAutostartProvider : IAutostartProvider
    {
        public AutostartStatus EffectiveStatus { get; set; } =
            new(AutostartState.Disabled);

        public Task<AutostartStatus> GetAutostartStatusAsync() =>
            Task.FromResult(EffectiveStatus);

        public Task<OperationResult> EnableAutostartAsync() =>
            Task.FromResult(EffectiveStatus.State == AutostartState.DisabledByUser
                ? OperationResult.Fail(EffectiveStatus.Message ?? "Windows disabled startup.")
                : OperationResult.Ok());

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
}

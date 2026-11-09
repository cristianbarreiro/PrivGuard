using PrivLock.Domain.Models;
using PrivLock.Platform.Windows.Devices;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsCoreAudioTelemetryTests
{
    [Fact]
    public void ParseHardwareMuteSupport_WhenMaskContainsMuteBitAndHrZero_ReturnsSupported()
    {
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.Supported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(0, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE));

        // Combined bits: mute bit (0x2) + volume bit (0x1)
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.Supported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(0, 0x00000003));
    }

    [Fact]
    public void ParseHardwareMuteSupport_WhenMaskLacksMuteBitAndHrZero_ReturnsNotSupported()
    {
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.NotSupported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(0, 0x00000000));

        // Other hardware bits without mute bit
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.NotSupported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(0, 0x00000001));

        Assert.Equal(
            AudioEndpointHardwareMuteSupport.NotSupported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(0, 0x00000004));
    }

    [Fact]
    public void ParseHardwareMuteSupport_WhenComSucceededWithSFalse_ParsesMaskAccurately()
    {
        // COM S_FALSE = 1 is a non-negative success code (SUCCEEDED(hr) == true)
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.Supported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(1, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE));

        Assert.Equal(
            AudioEndpointHardwareMuteSupport.NotSupported,
            WindowsCoreAudioController.ParseHardwareMuteSupport(1, 0x00000000));
    }

    [Theory]
    [InlineData(unchecked((int)0x80004005))] // E_FAIL
    [InlineData(unchecked((int)0x80070005))] // E_ACCESSDENIED
    [InlineData(unchecked((int)0x80004001))] // E_NOTIMPL
    [InlineData(-1)]                         // Generic negative failure
    public void ParseHardwareMuteSupport_WhenHResultFailed_ReturnsUnknownRegardlessOfMask(int hresult)
    {
        Assert.Equal(
            AudioEndpointHardwareMuteSupport.Unknown,
            WindowsCoreAudioController.ParseHardwareMuteSupport(hresult, WindowsCoreAudioController.ENDPOINT_HARDWARE_SUPPORT_MUTE));

        Assert.Equal(
            AudioEndpointHardwareMuteSupport.Unknown,
            WindowsCoreAudioController.ParseHardwareMuteSupport(hresult, 0));
    }

    [Fact]
    public void AudioEndpointMuteState_PreservesHardwareMuteSupportAndMuteState()
    {
        var supported = new WindowsAudioEndpointMuteState(
            "endpoint-hw",
            IsMuted: true,
            HardwareMuteSupport: AudioEndpointHardwareMuteSupport.Supported);
        Assert.Equal("endpoint-hw", supported.EndpointId);
        Assert.True(supported.IsMuted);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Supported, supported.HardwareMuteSupport);

        var notSupported = new WindowsAudioEndpointMuteState(
            "endpoint-sw",
            IsMuted: false,
            HardwareMuteSupport: AudioEndpointHardwareMuteSupport.NotSupported);
        Assert.Equal("endpoint-sw", notSupported.EndpointId);
        Assert.False(notSupported.IsMuted);
        Assert.Equal(AudioEndpointHardwareMuteSupport.NotSupported, notSupported.HardwareMuteSupport);

        // Default backward-compatible constructor parameter
        var legacyCall = new WindowsAudioEndpointMuteState("endpoint-legacy", true);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Unknown, legacyCall.HardwareMuteSupport);
    }

    [Fact]
    public void AudioEndpointLookupResult_PreservesHardwareMuteSupportAndMuteState()
    {
        var foundSupported = WindowsAudioEndpointLookupResult.Found(
            isMuted: true,
            hardwareMuteSupport: AudioEndpointHardwareMuteSupport.Supported);
        Assert.True(foundSupported.QuerySucceeded);
        Assert.True(foundSupported.IsPresent);
        Assert.True(foundSupported.IsMuted);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Supported, foundSupported.HardwareMuteSupport);

        var missing = WindowsAudioEndpointLookupResult.Missing();
        Assert.True(missing.QuerySucceeded);
        Assert.False(missing.IsPresent);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Unknown, missing.HardwareMuteSupport);

        var failed = WindowsAudioEndpointLookupResult.Failed("Error");
        Assert.False(failed.QuerySucceeded);
        Assert.False(failed.IsPresent);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Unknown, failed.HardwareMuteSupport);
    }

    [Fact]
    public void MultipleEndpoints_MaintainIndividualHardwareCapabilitiesWithoutAggregation()
    {
        var endpoint1 = new WindowsAudioEndpointMuteState("ep-1", true, AudioEndpointHardwareMuteSupport.Supported);
        var endpoint2 = new WindowsAudioEndpointMuteState("ep-2", false, AudioEndpointHardwareMuteSupport.NotSupported);
        var endpoint3 = new WindowsAudioEndpointMuteState("ep-3", true, AudioEndpointHardwareMuteSupport.Unknown);

        var states = new[] { endpoint1, endpoint2, endpoint3 };

        // Ensure capabilities are individually preserved
        Assert.Equal(AudioEndpointHardwareMuteSupport.Supported, states[0].HardwareMuteSupport);
        Assert.Equal(AudioEndpointHardwareMuteSupport.NotSupported, states[1].HardwareMuteSupport);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Unknown, states[2].HardwareMuteSupport);

        // No individual endpoint's capability represents the whole system
        Assert.False(states.All(s => s.HardwareMuteSupport == AudioEndpointHardwareMuteSupport.Supported));
    }

    [Fact]
    public void DetermineMicrophoneAvailability_WhenQuerySucceededAndNoEndpointsOrDevices_ReturnsNotDetected()
    {
        var availability = WindowsProtectionProvider.DetermineMicrophoneAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 0,
            deviceCount: 0);

        Assert.Equal(TargetDeviceAvailability.NotDetected, availability);
    }

    [Fact]
    public void DetermineMicrophoneAvailability_WhenQueryFailed_ReturnsUnknown()
    {
        var availability = WindowsProtectionProvider.DetermineMicrophoneAvailability(
            endpointQuerySucceeded: false,
            endpointCount: 0,
            deviceCount: 0);

        Assert.Equal(TargetDeviceAvailability.Unknown, availability);
    }

    [Fact]
    public void DetermineMicrophoneAvailability_WhenEndpointsOrDevicesPresent_ReturnsAvailable()
    {
        var availabilityWithEndpoints = WindowsProtectionProvider.DetermineMicrophoneAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 1,
            deviceCount: 0);
        Assert.Equal(TargetDeviceAvailability.Available, availabilityWithEndpoints);

        var availabilityWithDevices = WindowsProtectionProvider.DetermineMicrophoneAvailability(
            endpointQuerySucceeded: true,
            endpointCount: 0,
            deviceCount: 1);
        Assert.Equal(TargetDeviceAvailability.Available, availabilityWithDevices);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenNotDetected_ReturnsNeutralAbsenceMessage()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            TargetDeviceAvailability.NotDetected,
            StandardProtectionState.Unknown,
            SecureProtectionState.Unknown);

        Assert.Equal("No input microphones detected.", message);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenAvailableAndUnknown_ReturnsPartialWarning()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            TargetDeviceAvailability.Available,
            StandardProtectionState.Unknown,
            SecureProtectionState.Unknown);

        Assert.Equal("Windows microphone protection state is partial or could not be fully verified.", message);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenAvailableAndFullyVerified_ReturnsNull()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            TargetDeviceAvailability.Available,
            StandardProtectionState.Active,
            SecureProtectionState.Active);

        Assert.Null(message);
    }
}

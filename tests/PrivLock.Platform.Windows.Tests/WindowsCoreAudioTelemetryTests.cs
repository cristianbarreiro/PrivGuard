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

    [Theory]
    [InlineData(unchecked((int)0x80004005))] // E_FAIL
    [InlineData(1)]                          // S_FALSE
    [InlineData(-1)]                         // Generic failure
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
            isMuted: true,
            AudioEndpointHardwareMuteSupport.Supported);
        Assert.Equal("endpoint-hw", supported.EndpointId);
        Assert.True(supported.IsMuted);
        Assert.Equal(AudioEndpointHardwareMuteSupport.Supported, supported.HardwareMuteSupport);

        var notSupported = new WindowsAudioEndpointMuteState(
            "endpoint-sw",
            isMuted: false,
            AudioEndpointHardwareMuteSupport.NotSupported);
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
            AudioEndpointHardwareMuteSupport.Supported);
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
    public void DetermineMicrophoneStatusMessage_WhenZeroEndpointsAndZeroDevices_ReturnsNeutralAbsenceMessage()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            endpointQuerySucceeded: true,
            endpointCount: 0,
            deviceCount: 0,
            standardState: StandardProtectionState.Unknown,
            secureState: SecureProtectionState.Unknown);

        Assert.Equal("No input microphones detected.", message);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenQueryFailed_ReturnsPartialVerificationWarning()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            endpointQuerySucceeded: false,
            endpointCount: 0,
            deviceCount: 0,
            standardState: StandardProtectionState.Unknown,
            secureState: SecureProtectionState.Unknown);

        Assert.Equal("Windows microphone protection state is partial or could not be fully verified.", message);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenEndpointsPresentAndUnknown_ReturnsPartialWarning()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            endpointQuerySucceeded: true,
            endpointCount: 1,
            deviceCount: 1,
            standardState: StandardProtectionState.Unknown,
            secureState: SecureProtectionState.Unknown);

        Assert.Equal("Windows microphone protection state is partial or could not be fully verified.", message);
    }

    [Fact]
    public void DetermineMicrophoneStatusMessage_WhenFullyVerified_ReturnsNull()
    {
        var message = WindowsProtectionProvider.DetermineMicrophoneStatusMessage(
            endpointQuerySucceeded: true,
            endpointCount: 1,
            deviceCount: 1,
            standardState: StandardProtectionState.Active,
            secureState: SecureProtectionState.Active);

        Assert.Null(message);
    }
}

using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

[Collection("AvaloniaUI")]
public sealed class WindowsPackageIdentityDetectorTests
{
    [Fact]
    public void Detector_WhenPackagedWithAuthenticIdentity_ReturnsExpectedValues()
    {
        const string expectedFullName = "cdevstudios.PrivGuard_1.0.0.0_x64__tv7b4yqg10sw2";
        var detector = new WindowsPackageIdentityDetector(() => (true, expectedFullName));

        Assert.True(detector.IsRunningAsPackaged);
        Assert.Equal(expectedFullName, detector.PackageFullName);
        Assert.True(detector.HasExpectedPackageIdentity());
        Assert.True(detector.MatchesPackageName("cdevstudios.PrivGuard"));
        Assert.True(detector.MatchesPackageName("cdevstudios.PrivGuard_"));
        Assert.False(detector.MatchesPackageName("unrelated.package"));
    }

    [Fact]
    public void Detector_WhenPackagedWithSpoofedOrUnexpectedIdentity_ReturnsFalseForExpected()
    {
        const string spoofedFullName = "malicious.privguard_1.0.0.0_x64__badpublisher";
        var detector = new WindowsPackageIdentityDetector(() => (true, spoofedFullName));

        Assert.True(detector.IsRunningAsPackaged);
        Assert.Equal(spoofedFullName, detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
        Assert.False(detector.MatchesPackageName("cdevstudios.PrivGuard"));
    }

    [Fact]
    public void Detector_WhenUnpackaged_ReturnsUnpackagedAndFalse()
    {
        var detector = new WindowsPackageIdentityDetector(() => (false, null));

        Assert.False(detector.IsRunningAsPackaged);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
        Assert.False(detector.MatchesPackageName("cdevstudios.PrivGuard"));
    }

    [Fact]
    public void Detector_WhenNativeQueryThrowsException_HandlesGracefullyWithoutCrashing()
    {
        var detector = new WindowsPackageIdentityDetector(() =>
            throw new InvalidOperationException("Simulated Win32 GetCurrentPackageFullName failure"));

        Assert.False(detector.IsRunningAsPackaged);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
        Assert.False(detector.MatchesPackageName("cdevstudios.PrivGuard"));
    }

    [Fact]
    public void Detector_DefaultConstructor_InTestHost_ExecutesCleanly()
    {
        var detector = new WindowsPackageIdentityDetector();

        // Under standard unit test runner, process runs unpackaged
        Assert.False(detector.IsRunningAsPackaged);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }
}

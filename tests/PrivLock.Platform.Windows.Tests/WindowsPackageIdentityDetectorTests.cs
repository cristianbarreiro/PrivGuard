using System.Text;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsPackageIdentityDetectorTests
{
    private const int ERROR_SUCCESS = 0;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int APPMODEL_ERROR_NO_PACKAGE = 15700;
    private const int ERROR_ACCESS_DENIED = 5;

    [Fact]
    public void Detector_WhenAuthenticStorePackage_ReturnsAuthenticStorePackageKind()
    {
        const string expectedFamily = "cdevstudios.PrivGuard_tv7b4yqg10sw2";
        const string expectedFull = "cdevstudios.PrivGuard_1.0.0.0_x64__tv7b4yqg10sw2";

        var detector = new WindowsPackageIdentityDetector(
            CreateNativeInvoker(expectedFamily),
            CreateNativeInvoker(expectedFull));

        Assert.Equal(PackageIdentityKind.AuthenticStorePackage, detector.Kind);
        Assert.Equal(expectedFamily, detector.PackageFamilyName);
        Assert.Equal(expectedFull, detector.PackageFullName);
        Assert.True(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenAuthenticStorePackage_CaseInsensitiveMatching_Succeeds()
    {
        const string uppercaseFamily = "CDEVSTUDIOS.PRIVGUARD_TV7B4YQG10SW2";
        const string uppercaseFull = "CDEVSTUDIOS.PRIVGUARD_1.0.0.0_X64__TV7B4YQG10SW2";

        var detector = new WindowsPackageIdentityDetector(
            CreateNativeInvoker(uppercaseFamily),
            CreateNativeInvoker(uppercaseFull));

        Assert.Equal(PackageIdentityKind.AuthenticStorePackage, detector.Kind);
        Assert.Equal(uppercaseFamily, detector.PackageFamilyName);
        Assert.Equal(uppercaseFull, detector.PackageFullName);
        Assert.True(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenUnexpectedPackage_ReturnsUnexpectedPackageKind()
    {
        const string unexpectedFamily = "malicious.privguard_badpublisher123";
        const string unexpectedFull = "malicious.privguard_1.0.0.0_x64__badpublisher123";

        var detector = new WindowsPackageIdentityDetector(
            CreateNativeInvoker(unexpectedFamily),
            CreateNativeInvoker(unexpectedFull));

        Assert.Equal(PackageIdentityKind.UnexpectedPackage, detector.Kind);
        Assert.Equal(unexpectedFamily, detector.PackageFamilyName);
        Assert.Equal(unexpectedFull, detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenPartialMatchOrPrefixOnly_ClassifiesAsUnexpectedPackage()
    {
        const string prefixOnlyFamily = "cdevstudios.PrivGuard_impostorpublisher";
        const string prefixOnlyFull = "cdevstudios.PrivGuard_1.0.0.0_x64__impostorpublisher";

        var detector = new WindowsPackageIdentityDetector(
            CreateNativeInvoker(prefixOnlyFamily),
            CreateNativeInvoker(prefixOnlyFull));

        Assert.Equal(PackageIdentityKind.UnexpectedPackage, detector.Kind);
        Assert.Equal(prefixOnlyFamily, detector.PackageFamilyName);
        Assert.Equal(prefixOnlyFull, detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenUnpackaged_AppModelErrorNoPackage_ReturnsNotPackaged()
    {
        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker unpackagedInvoker =
            (ref int length, StringBuilder? _) => APPMODEL_ERROR_NO_PACKAGE;

        var detector = new WindowsPackageIdentityDetector(unpackagedInvoker, unpackagedInvoker);

        Assert.Equal(PackageIdentityKind.NotPackaged, detector.Kind);
        Assert.Null(detector.PackageFamilyName);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenNativeApiReturnsError_ReturnsDetectionFailed()
    {
        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker failingInvoker =
            (ref int length, StringBuilder? _) => ERROR_ACCESS_DENIED;

        var detector = new WindowsPackageIdentityDetector(failingInvoker, failingInvoker);

        Assert.Equal(PackageIdentityKind.DetectionFailed, detector.Kind);
        Assert.Null(detector.PackageFamilyName);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenNativeApiReturnsInvalidBufferLength_ReturnsDetectionFailed()
    {
        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker invalidLengthInvoker =
            (ref int length, StringBuilder? _) =>
            {
                length = -1;
                return ERROR_INSUFFICIENT_BUFFER;
            };

        var detector = new WindowsPackageIdentityDetector(invalidLengthInvoker, invalidLengthInvoker);

        Assert.Equal(PackageIdentityKind.DetectionFailed, detector.Kind);
    }

    [Fact]
    public void Detector_WhenNativeApiThrowsException_FailsClosedToDetectionFailed()
    {
        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker throwingInvoker =
            (ref int length, StringBuilder? _) => throw new InvalidOperationException("Simulated Win32 crash");

        var detector = new WindowsPackageIdentityDetector(throwingInvoker, throwingInvoker);

        Assert.Equal(PackageIdentityKind.DetectionFailed, detector.Kind);
        Assert.Null(detector.PackageFamilyName);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    [Fact]
    public void Detector_WhenInconsistentResults_ReturnsDetectionFailed()
    {
        const string validFamily = "cdevstudios.PrivGuard_tv7b4yqg10sw2";

        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker familyInvoker =
            CreateNativeInvoker(validFamily);

        WindowsPackageIdentityDetector.NativeGetPackageNameInvoker inconsistentFullInvoker =
            (ref int length, StringBuilder? _) => APPMODEL_ERROR_NO_PACKAGE;

        var detector = new WindowsPackageIdentityDetector(familyInvoker, inconsistentFullInvoker);

        Assert.Equal(PackageIdentityKind.DetectionFailed, detector.Kind);
    }

    [Fact]
    public void Detector_DefaultConstructor_InTestHost_ExecutesCleanly()
    {
        var detector = new WindowsPackageIdentityDetector();

        // Under standard unit test runner, process runs unpackaged
        Assert.Equal(PackageIdentityKind.NotPackaged, detector.Kind);
        Assert.Null(detector.PackageFamilyName);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
    }

    private static WindowsPackageIdentityDetector.NativeGetPackageNameInvoker CreateNativeInvoker(string? returnedValue)
    {
        return (ref int length, StringBuilder? buffer) =>
        {
            if (string.IsNullOrEmpty(returnedValue))
            {
                return APPMODEL_ERROR_NO_PACKAGE;
            }

            if (buffer == null)
            {
                length = returnedValue.Length + 1;
                return ERROR_INSUFFICIENT_BUFFER;
            }

            buffer.Append(returnedValue);
            return ERROR_SUCCESS;
        };
    }
}

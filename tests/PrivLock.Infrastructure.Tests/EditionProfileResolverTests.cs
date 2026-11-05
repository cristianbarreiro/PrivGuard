using PrivLock.Infrastructure.Common.Packaging;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using Xunit;

namespace PrivLock.Infrastructure.Tests;

public sealed class EditionProfileResolverTests : IDisposable
{
    private readonly string _testRoot;

    public EditionProfileResolverTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), $"PrivGuard_ProfileResolverTests_{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    [Fact]
    public void Resolve_WhenAuthenticStorePackage_ReturnsStoreProfileAndIsolatedPaths()
    {
        var detector = new FakePackageIdentityDetector
        {
            Kind = PackageIdentityKind.AuthenticStorePackage,
            PackageFamilyName = "cdevstudios.PrivGuard_tv7b4yqg10sw2",
            PackageFullName = "cdevstudios.PrivGuard_1.0.0.0_x64__tv7b4yqg10sw2"
        };

        var resolver = new DefaultEditionProfileResolver(detector, _testRoot);

        Assert.Equal(EditionProfile.Store, resolver.Profile);
        Assert.Equal("Store", resolver.ProfileName);
        Assert.Equal(Path.Combine(_testRoot, "Store"), resolver.DataDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Store", "Recovery"), resolver.RecoveryDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Store", "Logs"), resolver.LogsDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Store", "CrashReports"), resolver.CrashReportsDirectory);
        Assert.Equal(@"Global\PrivGuard_Store_SingleInstance", resolver.SingleInstanceMutexName);
        Assert.False(resolver.SupportsPrivilegedOperations);
    }

    [Fact]
    public void Resolve_WhenNotPackaged_ReturnsStandaloneProfileAndIsolatedPaths()
    {
        var detector = new FakePackageIdentityDetector
        {
            Kind = PackageIdentityKind.NotPackaged,
            PackageFamilyName = null,
            PackageFullName = null
        };

        var resolver = new DefaultEditionProfileResolver(detector, _testRoot);

        Assert.Equal(EditionProfile.Standalone, resolver.Profile);
        Assert.Equal("Standalone", resolver.ProfileName);
        Assert.Equal(Path.Combine(_testRoot, "Standalone"), resolver.DataDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Standalone", "Recovery"), resolver.RecoveryDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Standalone", "Logs"), resolver.LogsDirectory);
        Assert.Equal(Path.Combine(_testRoot, "Standalone", "CrashReports"), resolver.CrashReportsDirectory);
        Assert.Equal(@"Global\PrivGuard_Standalone_SingleInstance", resolver.SingleInstanceMutexName);
        Assert.True(resolver.SupportsPrivilegedOperations);
    }

    [Fact]
    public void Resolve_WhenUnexpectedPackage_ReturnsUntrustedProfileAndRejectsPathsAndPrivileges()
    {
        var detector = new FakePackageIdentityDetector
        {
            Kind = PackageIdentityKind.UnexpectedPackage,
            PackageFamilyName = "unrelated.application_1234567890abc",
            PackageFullName = "unrelated.application_1.0.0.0_x64__1234567890abc"
        };

        var resolver = new DefaultEditionProfileResolver(detector, _testRoot);

        Assert.Equal(EditionProfile.UntrustedOrIndeterminate, resolver.Profile);
        Assert.Equal("UntrustedOrIndeterminate", resolver.ProfileName);
        Assert.False(resolver.SupportsPrivilegedOperations);

        Assert.Throws<InvalidOperationException>(() => resolver.DataDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.RecoveryDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.LogsDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.CrashReportsDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.SingleInstanceMutexName);
    }

    [Fact]
    public void Resolve_WhenDetectionFailed_ReturnsUntrustedProfileAndRejectsPathsAndPrivileges()
    {
        var detector = new FakePackageIdentityDetector
        {
            Kind = PackageIdentityKind.DetectionFailed,
            PackageFamilyName = null,
            PackageFullName = null
        };

        var resolver = new DefaultEditionProfileResolver(detector, _testRoot);

        Assert.Equal(EditionProfile.UntrustedOrIndeterminate, resolver.Profile);
        Assert.Equal("UntrustedOrIndeterminate", resolver.ProfileName);
        Assert.False(resolver.SupportsPrivilegedOperations);

        Assert.Throws<InvalidOperationException>(() => resolver.DataDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.RecoveryDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.LogsDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.CrashReportsDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.SingleInstanceMutexName);
    }

    [Fact]
    public void NoOpPackageIdentityDetector_DefaultsToDetectionFailed_FailsClosed()
    {
        var detector = NoOpPackageIdentityDetector.Instance;

        Assert.Equal(PackageIdentityKind.DetectionFailed, detector.Kind);
        Assert.Null(detector.PackageFullName);
        Assert.Null(detector.PackageFamilyName);
        Assert.False(detector.HasExpectedPackageIdentity());

        var resolver = new DefaultEditionProfileResolver(detector, _testRoot);
        Assert.Equal(EditionProfile.UntrustedOrIndeterminate, resolver.Profile);
        Assert.False(resolver.SupportsPrivilegedOperations);
        Assert.Throws<InvalidOperationException>(() => resolver.DataDirectory);
        Assert.Throws<InvalidOperationException>(() => resolver.SingleInstanceMutexName);
    }

    [Fact]
    public void Resolve_DefaultConstructor_UsesCanonicalPrivGuardFolder()
    {
        var detector = new FakePackageIdentityDetector
        {
            Kind = PackageIdentityKind.NotPackaged
        };

        var resolver = new DefaultEditionProfileResolver(detector);

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedRoot = Path.Combine(localAppData, "PrivGuard", "Standalone");

        Assert.Equal(expectedRoot, resolver.DataDirectory);
    }

    private sealed class FakePackageIdentityDetector : IPackageIdentityDetector
    {
        public PackageIdentityKind Kind { get; set; } = PackageIdentityKind.NotPackaged;
        public string? PackageFullName { get; set; }
        public string? PackageFamilyName { get; set; }

        public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
    }
}

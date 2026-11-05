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
    public void Resolve_WhenPackaged_ReturnsStoreProfileAndIsolatedPaths()
    {
        var detector = new FakePackageIdentityDetector
        {
            IsRunningAsPackaged = true,
            PackageFullName = "cdevstudios.PrivGuard_1.0.0.0_x64__tv7b4yqg10sw2",
            HasExpectedIdentity = true
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
    public void Resolve_WhenUnpackaged_ReturnsStandaloneProfileAndIsolatedPaths()
    {
        var detector = new FakePackageIdentityDetector
        {
            IsRunningAsPackaged = false,
            PackageFullName = null,
            HasExpectedIdentity = false
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
    public void Resolve_DefaultConstructor_UsesCanonicalPrivGuardFolder()
    {
        var detector = new FakePackageIdentityDetector
        {
            IsRunningAsPackaged = false
        };

        var resolver = new DefaultEditionProfileResolver(detector);

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedRoot = Path.Combine(localAppData, "PrivGuard", "Standalone");

        Assert.Equal(expectedRoot, resolver.DataDirectory);
    }

    [Fact]
    public void NoOpPackageIdentityDetector_AlwaysReportsUnpackaged()
    {
        var detector = NoOpPackageIdentityDetector.Instance;

        Assert.False(detector.IsRunningAsPackaged);
        Assert.Null(detector.PackageFullName);
        Assert.False(detector.HasExpectedPackageIdentity());
        Assert.False(detector.MatchesPackageName("cdevstudios.PrivGuard"));
    }

    private sealed class FakePackageIdentityDetector : IPackageIdentityDetector
    {
        public bool IsRunningAsPackaged { get; set; }
        public string? PackageFullName { get; set; }
        public bool HasExpectedIdentity { get; set; }

        public bool HasExpectedPackageIdentity() => HasExpectedIdentity;

        public bool MatchesPackageName(string expectedPrefixOrName)
        {
            return PackageFullName != null && PackageFullName.StartsWith(expectedPrefixOrName, StringComparison.OrdinalIgnoreCase);
        }
    }
}

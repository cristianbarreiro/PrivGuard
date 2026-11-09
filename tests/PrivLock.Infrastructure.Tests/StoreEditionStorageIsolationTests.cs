using PrivLock.Domain.Models;
using PrivLock.Infrastructure.Common.Logging;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using Xunit;

namespace PrivLock.Infrastructure.Tests;

public sealed class StoreEditionStorageIsolationTests : IDisposable
{
    private readonly string _testBaseDir;

    public StoreEditionStorageIsolationTests()
    {
        _testBaseDir = Path.Combine(Path.GetTempPath(), "PrivGuard_StorageIsolationTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testBaseDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testBaseDir))
                Directory.Delete(_testBaseDir, recursive: true);
        }
        catch { }
    }

    [Fact]
    public void FileStateStore_WithStoreProfileResolver_SavesInStoreDataDirectory()
    {
        var mockDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var resolver = new DefaultEditionProfileResolver(mockDetector, _testBaseDir);

        var stateStore = new FileStateStore(resolver);
        var state = new DesiredState
        {
            CameraStandard = StandardProtectionState.Active,
            MicrophoneStandard = StandardProtectionState.Active
        };
        stateStore.Save(state);

        var expectedFile = Path.Combine(resolver.DataDirectory, "state.json");
        Assert.True(File.Exists(expectedFile), $"State file should exist at {expectedFile}");

        var loaded = stateStore.Load();
        Assert.Equal(StandardProtectionState.Active, loaded.CameraStandard);
        Assert.Equal(StandardProtectionState.Active, loaded.MicrophoneStandard);
    }

    [Fact]
    public void FilePrivacySessionStore_WithStoreProfileResolver_SavesInStoreRecoveryDirectory()
    {
        var mockDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var resolver = new DefaultEditionProfileResolver(mockDetector, _testBaseDir);
        var marker = new FileActivePrivacySessionMarker(resolver.RecoveryDirectory);

        var sessionStore = new FilePrivacySessionStore(resolver, marker);
        var sessionId = Guid.NewGuid();
        var session = new PrivacySession
        {
            SessionId = sessionId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            OwnerProcessId = Environment.ProcessId,
            IsActive = true,
            Status = PrivacySessionStatus.Active
        };

        sessionStore.Save(session);

        var expectedJournal = Path.Combine(resolver.RecoveryDirectory, "privacy-session-v1.json");
        Assert.True(File.Exists(expectedJournal), $"Journal file should exist at {expectedJournal}");

        var loaded = sessionStore.Load();
        Assert.NotNull(loaded);
        Assert.Equal(sessionId, loaded.SessionId);
    }

    [Fact]
    public void DefaultLegacyArtifactDetector_WithStoreProfile_ReturnsFalseEvenIfLegacyArtifactsExist()
    {
        var storeDetector = new FakePackageIdentityDetector(PackageIdentityKind.AuthenticStorePackage);
        var storeResolver = new DefaultEditionProfileResolver(storeDetector, _testBaseDir);

        // Create a legacy marker in the data directory
        var recoveryDir = Path.Combine(storeResolver.DataDirectory, "Recovery");
        Directory.CreateDirectory(recoveryDir);
        File.WriteAllText(Path.Combine(recoveryDir, "legacy-untracked.marker"), "legacy");

        var detector = new DefaultLegacyArtifactDetector(storeResolver);
        Assert.False(detector.HasLegacyEvidence(), "Store profile must never report legacy evidence or trigger migration.");
    }

    [Fact]
    public void DefaultLegacyArtifactDetector_WithStandaloneProfile_DetectsLegacyArtifacts()
    {
        var standaloneDetector = new FakePackageIdentityDetector(PackageIdentityKind.NotPackaged);
        var standaloneResolver = new DefaultEditionProfileResolver(standaloneDetector, _testBaseDir);

        var recoveryDir = Path.Combine(standaloneResolver.DataDirectory, "Recovery");
        Directory.CreateDirectory(recoveryDir);
        File.WriteAllText(Path.Combine(recoveryDir, "legacy-untracked.marker"), "legacy");

        var detector = new DefaultLegacyArtifactDetector(standaloneResolver);
        Assert.True(detector.HasLegacyEvidence(), "Standalone profile should detect legacy evidence when present.");
    }

    [Fact]
    public void CrashReporter_WithConfiguredDefaultDirectory_WritesToConfiguredDirectory()
    {
        var customCrashDir = Path.Combine(_testBaseDir, "CustomCrashDir");
        CrashReporter.ConfigureDefaultDirectory(customCrashDir);

        var reportPath = CrashReporter.GenerateCrashReport(
            new InvalidOperationException("Test crash isolation"),
            "StoreEditionStorageIsolationTests");

        Assert.False(string.IsNullOrWhiteSpace(reportPath));
        Assert.True(File.Exists(reportPath));
        Assert.StartsWith(customCrashDir, reportPath, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakePackageIdentityDetector : IPackageIdentityDetector
    {
        public FakePackageIdentityDetector(PackageIdentityKind kind)
        {
            Kind = kind;
        }

        public PackageIdentityKind Kind { get; }
        public string? PackageFamilyName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_xxxx" : null;
        public string? PackageFullName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_1.0.0.0_x64__xxxx" : null;
        public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
    }
}

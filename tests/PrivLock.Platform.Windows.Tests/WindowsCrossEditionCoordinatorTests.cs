using System.Diagnostics;
using System.Text.Json;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using PrivLock.Platform.Windows.System;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class WindowsCrossEditionCoordinatorTests : IDisposable
{
    private readonly string _testBaseDir;
    private readonly string _testMutexName;

    public WindowsCrossEditionCoordinatorTests()
    {
        _testBaseDir = Path.Combine(Path.GetTempPath(), "PrivGuard_CrossEditionTests_" + Guid.NewGuid().ToString("N"));
        _testMutexName = @"Local\PrivGuard_TestLock_" + Guid.NewGuid().ToString("N");
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
    public void InspectOwnership_WhenNoDescriptorsExist_ReturnsFree()
    {
        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var coordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);

        var result = coordinator.InspectOwnership();

        Assert.Equal(CrossEditionOwnershipState.Free, result.State);
        Assert.False(result.RequiresSupervisionOnly);
        Assert.Null(result.Descriptor);
    }

    [Fact]
    public void TryAcquireOwnership_WritesDescriptorAndReturnsAcquired()
    {
        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var coordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);
        var sessionId = Guid.NewGuid();

        var acquisition = coordinator.TryAcquireOwnership(sessionId);

        Assert.True(acquisition.Success);
        Assert.Equal(CrossEditionOwnershipState.OwnedByCurrentEdition, acquisition.State);
        Assert.NotNull(acquisition.Descriptor);
        Assert.Equal(sessionId, acquisition.Descriptor.SessionId);
        Assert.Equal(EditionProfile.Store, acquisition.Descriptor.OwnerEdition);

        var descriptorPath = Path.Combine(_testBaseDir, "Store", WindowsCrossEditionCoordinator.LatchFileName);
        Assert.True(File.Exists(descriptorPath), "Descriptor file must exist on disk");

        var inspection = coordinator.InspectOwnership();
        Assert.Equal(CrossEditionOwnershipState.OwnedByCurrentEdition, inspection.State);
        Assert.False(inspection.RequiresSupervisionOnly);
    }

    [Fact]
    public void TryAcquireOwnership_WhenOpposingEditionActive_ReturnsFailedWithOwnedByOtherEdition()
    {
        // 1. Simulate Standalone edition owning an active session with the current live process
        var standaloneDir = Path.Combine(_testBaseDir, "Standalone");
        Directory.CreateDirectory(standaloneDir);
        var standaloneLatchPath = Path.Combine(standaloneDir, WindowsCrossEditionCoordinator.LatchFileName);

        using var curProc = Process.GetCurrentProcess();
        var standaloneDescriptor = new CrossEditionDescriptor
        {
            SchemaVersion = 1,
            OwnerEdition = EditionProfile.Standalone,
            UserSid = "S-1-5-21-test",
            ProcessId = curProc.Id,
            ProcessStartTimeUtc = curProc.StartTime.ToUniversalTime(),
            SessionId = Guid.NewGuid(),
            AcquiredAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow,
            OperationStatus = "Active"
        };
        File.WriteAllText(standaloneLatchPath, JsonSerializer.Serialize(standaloneDescriptor));

        // 2. Store edition attempts to inspect and acquire
        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var storeCoordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);

        var inspection = storeCoordinator.InspectOwnership();
        Assert.Equal(CrossEditionOwnershipState.OwnedByOtherEdition, inspection.State);
        Assert.True(inspection.RequiresSupervisionOnly);
        Assert.NotNull(inspection.Descriptor);

        var acquisition = storeCoordinator.TryAcquireOwnership(Guid.NewGuid());
        Assert.False(acquisition.Success);
        Assert.Equal(CrossEditionOwnershipState.OwnedByOtherEdition, acquisition.State);
    }

    [Fact]
    public void InspectOwnership_WhenOpposingEditionProcessTerminated_ReturnsAbandonedByOtherEdition()
    {
        // Simulate Standalone edition with a non-existent dead PID (e.g. 999999)
        var standaloneDir = Path.Combine(_testBaseDir, "Standalone");
        Directory.CreateDirectory(standaloneDir);
        var standaloneLatchPath = Path.Combine(standaloneDir, WindowsCrossEditionCoordinator.LatchFileName);

        var deadDescriptor = new CrossEditionDescriptor
        {
            SchemaVersion = 1,
            OwnerEdition = EditionProfile.Standalone,
            UserSid = "S-1-5-21-test",
            ProcessId = 999999,
            ProcessStartTimeUtc = DateTime.UtcNow.AddHours(-1),
            SessionId = Guid.NewGuid(),
            AcquiredAtUtc = DateTime.UtcNow.AddHours(-1),
            LastHeartbeatUtc = DateTime.UtcNow.AddHours(-1),
            OperationStatus = "Active"
        };
        File.WriteAllText(standaloneLatchPath, JsonSerializer.Serialize(deadDescriptor));

        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var storeCoordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);

        var inspection = storeCoordinator.InspectOwnership();
        Assert.Equal(CrossEditionOwnershipState.AbandonedByOtherEdition, inspection.State);
        Assert.True(inspection.RequiresSupervisionOnly);
    }

    [Fact]
    public void InspectOwnership_WithRecycledPid_DetectsTimeMismatchAndReturnsAbandonedByOtherEdition()
    {
        // Simulate Standalone latch with current PID, but with StartTime from 10 hours ago (simulating recycled PID)
        using var curProc = Process.GetCurrentProcess();
        var mismatchedStartTime = curProc.StartTime.ToUniversalTime().AddHours(-10);

        var standaloneDir = Path.Combine(_testBaseDir, "Standalone");
        Directory.CreateDirectory(standaloneDir);
        var standaloneLatchPath = Path.Combine(standaloneDir, WindowsCrossEditionCoordinator.LatchFileName);

        var recycledDescriptor = new CrossEditionDescriptor
        {
            SchemaVersion = 1,
            OwnerEdition = EditionProfile.Standalone,
            UserSid = "S-1-5-21-test",
            ProcessId = curProc.Id,
            ProcessStartTimeUtc = mismatchedStartTime,
            SessionId = Guid.NewGuid(),
            AcquiredAtUtc = DateTime.UtcNow.AddHours(-10),
            LastHeartbeatUtc = DateTime.UtcNow.AddHours(-10),
            OperationStatus = "Active"
        };
        File.WriteAllText(standaloneLatchPath, JsonSerializer.Serialize(recycledDescriptor));

        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var storeCoordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);

        var inspection = storeCoordinator.InspectOwnership();
        Assert.Equal(CrossEditionOwnershipState.AbandonedByOtherEdition, inspection.State);
        Assert.True(inspection.RequiresSupervisionOnly);
    }

    [Fact]
    public void InspectOwnership_WhenOpposingDescriptorCorrupt_ReturnsCorruptedOrIncomplete()
    {
        var standaloneDir = Path.Combine(_testBaseDir, "Standalone");
        Directory.CreateDirectory(standaloneDir);
        var standaloneLatchPath = Path.Combine(standaloneDir, WindowsCrossEditionCoordinator.LatchFileName);

        File.WriteAllText(standaloneLatchPath, "{ \"invalid\": \"truncated json");

        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var storeCoordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);

        var inspection = storeCoordinator.InspectOwnership();
        Assert.Equal(CrossEditionOwnershipState.CorruptedOrIncomplete, inspection.State);
        Assert.True(inspection.RequiresSupervisionOnly);

        var acquisition = storeCoordinator.TryAcquireOwnership(Guid.NewGuid());
        Assert.False(acquisition.Success);
        Assert.Equal(CrossEditionOwnershipState.CorruptedOrIncomplete, acquisition.State);
    }

    [Fact]
    public void TryReleaseOwnership_DeletesOnlyCurrentEditionDescriptor()
    {
        // Create descriptors for both editions
        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        var storeCoordinator = new WindowsCrossEditionCoordinator(storeResolver, _testBaseDir, _testMutexName);
        var storeSessionId = Guid.NewGuid();
        storeCoordinator.TryAcquireOwnership(storeSessionId);

        var standaloneDir = Path.Combine(_testBaseDir, "Standalone");
        Directory.CreateDirectory(standaloneDir);
        var standaloneLatchPath = Path.Combine(standaloneDir, WindowsCrossEditionCoordinator.LatchFileName);
        File.WriteAllText(standaloneLatchPath, "{\"schemaVersion\":1}");

        var storeLatchPath = Path.Combine(_testBaseDir, "Store", WindowsCrossEditionCoordinator.LatchFileName);
        Assert.True(File.Exists(storeLatchPath));
        Assert.True(File.Exists(standaloneLatchPath));

        // Release Store ownership
        var released = storeCoordinator.TryReleaseOwnership(storeSessionId);
        Assert.True(released);

        // Store latch must be deleted; Standalone latch must NOT be touched
        Assert.False(File.Exists(storeLatchPath), "Store descriptor must be deleted on release");
        Assert.True(File.Exists(standaloneLatchPath), "Standalone descriptor must never be touched or deleted by Store");
    }

    [Fact]
    public void WindowsSingleInstanceGuard_ResolvesStoreMutexNameFromProfile()
    {
        var storeResolver = CreateProfileResolver(EditionProfile.Store);
        Assert.Equal(DefaultEditionProfileResolver.StoreMutexName, storeResolver.SingleInstanceMutexName);

        var standaloneResolver = CreateProfileResolver(EditionProfile.Standalone);
        Assert.Equal(DefaultEditionProfileResolver.StandaloneMutexName, standaloneResolver.SingleInstanceMutexName);

        var testMutex = $@"Local\PrivGuard_Store_Test_{Guid.NewGuid():N}";
        var testGate = $@"Local\PrivGuard_Gate_Test_{Guid.NewGuid():N}";
        using var guard = new WindowsSingleInstanceGuard(testMutex, testGate);
        Assert.True(guard.TryAcquireSingleInstance());
        guard.Release();
    }

    private static IEditionProfileResolver CreateProfileResolver(EditionProfile profile)
    {
        var kind = profile == EditionProfile.Store
            ? PackageIdentityKind.AuthenticStorePackage
            : PackageIdentityKind.NotPackaged;
        var detector = new FakeDetector(kind);
        return new DefaultEditionProfileResolver(detector);
    }

    private sealed class FakeDetector : IPackageIdentityDetector
    {
        public FakeDetector(PackageIdentityKind kind) => Kind = kind;
        public PackageIdentityKind Kind { get; }
        public string? PackageFamilyName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_xxxx" : null;
        public string? PackageFullName => Kind == PackageIdentityKind.AuthenticStorePackage ? "cdevstudios.PrivGuard_1.0.0.0_x64__xxxx" : null;
        public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;
    }
}

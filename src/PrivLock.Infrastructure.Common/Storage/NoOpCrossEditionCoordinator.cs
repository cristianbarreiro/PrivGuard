using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Storage;

/// <summary>
/// No-op implementation of ICrossEditionCoordinator for non-Windows platforms or test scaffolds.
/// </summary>
public sealed class NoOpCrossEditionCoordinator : ICrossEditionCoordinator
{
    public CrossEditionInspectionResult InspectOwnership() =>
        new(CrossEditionOwnershipState.Free, null, "Cross-edition coordination is not required on this platform.");

    public CrossEditionAcquisitionResult TryAcquireOwnership(Guid sessionId) =>
        CrossEditionAcquisitionResult.Acquired(new CrossEditionDescriptor
        {
            SchemaVersion = 1,
            OwnerEdition = EditionProfile.Standalone,
            SessionId = sessionId,
            ProcessId = Environment.ProcessId,
            ProcessStartTimeUtc = DateTime.UtcNow,
            AcquiredAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow,
            OperationStatus = "Active"
        });

    public bool TryReleaseOwnership(Guid sessionId) => true;

    public void UpdateHeartbeat(Guid sessionId) { }
}

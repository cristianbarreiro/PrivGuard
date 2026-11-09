namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Status of ownership across PrivGuard editions (Store vs Standalone).
/// </summary>
public enum CrossEditionOwnershipState
{
    Free,
    OwnedByCurrentEdition,
    OwnedByOtherEdition,
    AbandonedByOtherEdition,
    CorruptedOrIncomplete,
    Indeterminate
}

/// <summary>
/// Machine/user-scoped metadata recording active session ownership of an edition.
/// </summary>
public sealed record CrossEditionDescriptor
{
    public int SchemaVersion { get; init; } = 1;
    public EditionProfile OwnerEdition { get; init; }
    public string UserSid { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public DateTime ProcessStartTimeUtc { get; init; }
    public Guid SessionId { get; init; }
    public DateTime AcquiredAtUtc { get; init; }
    public DateTime LastHeartbeatUtc { get; init; }
    public string OperationStatus { get; init; } = "Active";
}

/// <summary>
/// Result of inspecting cross-edition session ownership.
/// </summary>
public sealed record CrossEditionInspectionResult(
    CrossEditionOwnershipState State,
    CrossEditionDescriptor? Descriptor,
    string? Message)
{
    public bool RequiresSupervisionOnly =>
        State is CrossEditionOwnershipState.OwnedByOtherEdition
              or CrossEditionOwnershipState.AbandonedByOtherEdition
              or CrossEditionOwnershipState.CorruptedOrIncomplete;
}

/// <summary>
/// Result of attempting to acquire cross-edition ownership.
/// </summary>
public sealed record CrossEditionAcquisitionResult(
    bool Success,
    CrossEditionOwnershipState State,
    CrossEditionDescriptor? Descriptor,
    string? ErrorMessage)
{
    public static CrossEditionAcquisitionResult Acquired(CrossEditionDescriptor descriptor) =>
        new(true, CrossEditionOwnershipState.OwnedByCurrentEdition, descriptor, null);

    public static CrossEditionAcquisitionResult Failed(CrossEditionOwnershipState state, string errorMessage) =>
        new(false, state, null, errorMessage);
}

/// <summary>
/// Coordinates session ownership and mutual supervision between Store and Standalone editions of PrivGuard.
/// </summary>
public interface ICrossEditionCoordinator
{
    CrossEditionInspectionResult InspectOwnership();
    CrossEditionAcquisitionResult TryAcquireOwnership(Guid sessionId);
    bool TryReleaseOwnership(Guid sessionId);
    void UpdateHeartbeat(Guid sessionId);
}

namespace PrivLock.Platform.Abstractions;

public enum AutostartState
{
    Enabled,
    Disabled,
    DisabledByUser,
    DisabledByPolicy,
    InvalidRegistration,
    MissingExecutable,
    UnstablePath,
    MigrationRequired,
    Failed,
    Unsupported
}

public sealed record AutostartStatus(AutostartState State, string? Message = null)
{
    public bool IsEnabled => State == AutostartState.Enabled;
}

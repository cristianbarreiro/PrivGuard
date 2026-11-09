namespace PrivLock.Domain.Models;

/// <summary>
/// Specifies the policy applied when restoring a protection scope that has no active ownership snapshot.
/// </summary>
public enum RestoreScopePolicy
{
    /// <summary>
    /// Strict policy: if the current state cannot be verified (unknown) and there is no ownership snapshot,
    /// report failure so unverified scopes are not assumed safe.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Opportunistic pre-teardown policy (used e.g. when tearing down Secure before Standard):
    /// if PrivGuard does not own the Secure scope and its state is unknown, treat as nothing to restore
    /// so the verified Standard restoration can proceed unblocked.
    /// </summary>
    OpportunisticPreTeardown = 1
}

using PrivLock.Domain.Results;
using Serilog;

namespace PrivLock.Application.Services;

/// <summary>
/// Startup-only facade that makes recovery ordering explicit in the composition root.
/// </summary>
public sealed class PrivacyRecoveryService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PrivacyRecoveryService>();
    private readonly ProtectionService _protectionService;

    public PrivacyRecoveryService(ProtectionService protectionService)
    {
        _protectionService = protectionService;
    }

    /// <summary>
    /// Begins a deferred/lazy elevation scope spanning both startup recovery and reconciliation.
    /// Entering the scope does not launch an elevated process; if either phase requires elevation,
    /// a single elevated worker process is launched on demand and reused across all recovery and
    /// reconciliation operations. Disposing the scope cleanly closes the worker.
    /// </summary>
    public IDisposable BeginStartupPrivilegeScope()
    {
        Log.Information("Startup privilege scope starting");
        return _protectionService.BeginDeferredElevationScope();
    }

    public async Task<PrivacyRecoveryResult> RecoverAtStartupAsync(
        CancellationToken cancellationToken = default)
    {
        Log.Information("Startup recovery beginning: Checking for an unfinished reversible privacy session");
        var result = await _protectionService.RecoverPreviousSessionAsync(cancellationToken);
        Log.Information(
            "Startup recovery finished: Supported={Supported}, Complete={Complete}, Conflicts={Conflicts}, IrreducibleAmbiguities={IrreducibleAmbiguities}, Missing={Missing}, Failed={Failed}",
            result.TrackingSupported,
            result.IsComplete,
            result.ConflictCount,
            result.IrreducibleAmbiguityCount,
            result.MissingCount,
            result.FailedCount);
        return result;
    }

    public async Task<OperationResult> ReconcileAtStartupAsync(
        PrivacyRecoveryResult? recoveryResult = null,
        CancellationToken cancellationToken = default)
    {
        Log.Information("Startup reconciliation beginning: Reconciling persistent desired protection state with actual OS state");
        var result = await _protectionService.ReconcileDesiredProtectionAsync(recoveryResult, cancellationToken);
        Log.Information(
            "Startup reconciliation finished: Success={Success}, Error={Error}",
            result.Success,
            result.ErrorMessage);
        return result;
    }
}

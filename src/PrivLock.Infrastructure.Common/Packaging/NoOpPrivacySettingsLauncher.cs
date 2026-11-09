using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;

namespace PrivLock.Infrastructure.Common.Packaging;

/// <summary>
/// Fail-closed no-op implementation of <see cref="IPrivacySettingsLauncher"/> for platforms
/// or test contexts where OS privacy settings URI navigation is not available.
/// </summary>
public sealed class NoOpPrivacySettingsLauncher : IPrivacySettingsLauncher
{
    public static NoOpPrivacySettingsLauncher Instance { get; } = new();

    public Task<OperationResult> OpenPrivacySettingsAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(OperationResult.Fail(
            "Native OS privacy settings navigation is not supported on this platform."));
    }
}

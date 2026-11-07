using PrivLock.Domain.Models;
using PrivLock.Domain.Results;

namespace PrivLock.Platform.Abstractions;

/// <summary>
/// Launches native operating system privacy settings pages (e.g., ms-settings: URIs on Windows).
/// Opening settings is strictly a user navigation helper and never constitutes proof that a
/// permission was changed; callers must re-query actual OS state after navigation.
/// </summary>
public interface IPrivacySettingsLauncher
{
    public const string WindowsCameraSettingsUri = "ms-settings:privacy-webcam";
    public const string WindowsMicrophoneSettingsUri = "ms-settings:privacy-microphone";

    /// <summary>
    /// Opens the operating system privacy settings page for the requested target.
    /// </summary>
    Task<OperationResult> OpenPrivacySettingsAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default);
}

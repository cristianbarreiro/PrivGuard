using System.Diagnostics;
using PrivLock.Domain.Models;
using PrivLock.Domain.Results;
using PrivLock.Infrastructure.Common.Storage;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

/// <summary>
/// Windows implementation of <see cref="IPrivacySettingsLauncher"/> that navigates to official
/// Windows privacy settings URIs (ms-settings:privacy-webcam and ms-settings:privacy-microphone).
/// Strictly testable via an injectable URI launcher delegate and fail-closed for untrusted profiles.
/// </summary>
public class WindowsPrivacySettingsLauncher : IPrivacySettingsLauncher
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsPrivacySettingsLauncher>();

    private readonly IEditionProfileResolver _editionProfileResolver;
    private readonly Func<string, bool> _uriStarter;

    public WindowsPrivacySettingsLauncher()
        : this(new DefaultEditionProfileResolver(new WindowsPackageIdentityDetector()), null)
    {
    }

    public WindowsPrivacySettingsLauncher(
        IEditionProfileResolver editionProfileResolver,
        Func<string, bool>? uriStarter = null)
    {
        ArgumentNullException.ThrowIfNull(editionProfileResolver);
        _editionProfileResolver = editionProfileResolver;
        _uriStarter = uriStarter ?? DefaultStartUri;
    }

    public virtual Task<OperationResult> OpenPrivacySettingsAsync(
        BlockTarget target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_editionProfileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            Log.Error("Rejected privacy settings navigation because edition profile is untrusted or indeterminate");
            return Task.FromResult(OperationResult.Fail(
                "Cannot open Windows privacy settings: application package identity is untrusted or indeterminate."));
        }

        var uri = ResolveSettingsUri(target);
        if (uri == null)
        {
            return Task.FromResult(OperationResult.Fail($"Unsupported privacy settings target: '{target}'."));
        }

        try
        {
            Log.Information("Opening Windows privacy settings URI '{Uri}' for target {Target}", uri, target);
            var launched = _uriStarter(uri);
            if (!launched)
            {
                return Task.FromResult(OperationResult.Fail(
                    $"Windows could not open the privacy settings page ({uri})."));
            }

            return Task.FromResult(OperationResult.Ok());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open Windows privacy settings URI '{Uri}'", uri);
            return Task.FromResult(OperationResult.Fail(
                $"Failed to open Windows privacy settings ({uri}): {ex.Message}"));
        }
    }

    public static string? ResolveSettingsUri(BlockTarget target) => target switch
    {
        BlockTarget.Camera => IPrivacySettingsLauncher.WindowsCameraSettingsUri,
        BlockTarget.Microphone => IPrivacySettingsLauncher.WindowsMicrophoneSettingsUri,
        BlockTarget.Both => IPrivacySettingsLauncher.WindowsCameraSettingsUri,
        _ => null
    };

    private static bool DefaultStartUri(string uri)
    {
        // Only allow official ms-settings: privacy URIs
        if (!string.Equals(uri, IPrivacySettingsLauncher.WindowsCameraSettingsUri, StringComparison.Ordinal) &&
            !string.Equals(uri, IPrivacySettingsLauncher.WindowsMicrophoneSettingsUri, StringComparison.Ordinal))
        {
            return false;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = uri,
            UseShellExecute = true
        };
        using var process = Process.Start(startInfo);
        return true;
    }
}

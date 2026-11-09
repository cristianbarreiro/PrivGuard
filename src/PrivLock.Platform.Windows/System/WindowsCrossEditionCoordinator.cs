using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

/// <summary>
/// Implements cross-edition coordination between PrivGuard Store and Standalone editions on Windows.
/// Uses a user-scoped Mutex and edition-isolated descriptor files in LocalAppData to prevent
/// concurrent or conflicting privacy mutations. Fails closed on corrupted descriptors or abandoned sessions.
/// Each edition writes and deletes ONLY its own descriptor.
/// </summary>
public sealed class WindowsCrossEditionCoordinator : ICrossEditionCoordinator
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsCrossEditionCoordinator>();

    public const string DefaultMutexName = @"Local\PrivGuard_CrossEdition_Lock";
    public const string LatchFileName = "cross-edition-latch.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IEditionProfileResolver _profileResolver;
    private readonly string _mutexName;
    private readonly string _currentDescriptorPath;
    private readonly string _opposingDescriptorPath;
    private readonly EditionProfile _opposingProfile;

    public WindowsCrossEditionCoordinator(
        IEditionProfileResolver profileResolver,
        string? customBaseAppDataDirectory = null)
        : this(profileResolver, customBaseAppDataDirectory, customMutexName: null)
    {
    }

    internal WindowsCrossEditionCoordinator(
        IEditionProfileResolver profileResolver,
        string? customBaseAppDataDirectory,
        string? customMutexName)
    {
        _profileResolver = profileResolver ?? throw new ArgumentNullException(nameof(profileResolver));
        _mutexName = customMutexName ?? DefaultMutexName;

        var baseDir = customBaseAppDataDirectory ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivGuard");

        _opposingProfile = _profileResolver.Profile switch
        {
            EditionProfile.Store => EditionProfile.Standalone,
            EditionProfile.Standalone => EditionProfile.Store,
            _ => EditionProfile.UntrustedOrIndeterminate
        };

        var currentProfileName = _profileResolver.Profile switch
        {
            EditionProfile.Store => "Store",
            EditionProfile.Standalone => "Standalone",
            _ => "UntrustedOrIndeterminate"
        };

        var opposingProfileName = _opposingProfile switch
        {
            EditionProfile.Store => "Store",
            EditionProfile.Standalone => "Standalone",
            _ => "UntrustedOrIndeterminate"
        };

        _currentDescriptorPath = Path.Combine(baseDir, currentProfileName, LatchFileName);
        _opposingDescriptorPath = Path.Combine(baseDir, opposingProfileName, LatchFileName);
    }

    public CrossEditionInspectionResult InspectOwnership()
    {
        if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            return new CrossEditionInspectionResult(
                CrossEditionOwnershipState.Indeterminate,
                null,
                "Cannot inspect cross-edition state in untrusted or indeterminate profile.");
        }

        try
        {
            // 1. Inspect opposing edition
            if (File.Exists(_opposingDescriptorPath))
            {
                var (opposingDesc, corrupt) = ReadDescriptor(_opposingDescriptorPath);
                if (corrupt)
                {
                    Log.Warning("Cross-edition opposing descriptor at {Path} is corrupted or incomplete", _opposingDescriptorPath);
                    return new CrossEditionInspectionResult(
                        CrossEditionOwnershipState.CorruptedOrIncomplete,
                        null,
                        "Opposing edition coordination descriptor is corrupted or incomplete.");
                }

                if (opposingDesc != null && string.Equals(opposingDesc.OperationStatus, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsProcessAliveAndMatching(opposingDesc.ProcessId, opposingDesc.ProcessStartTimeUtc))
                    {
                        Log.Information("Active session owned by opposing edition {Edition} (PID: {Pid})",
                            opposingDesc.OwnerEdition, opposingDesc.ProcessId);
                        return new CrossEditionInspectionResult(
                            CrossEditionOwnershipState.OwnedByOtherEdition,
                            opposingDesc,
                            $"Session is currently owned by active {opposingDesc.OwnerEdition} edition (Process {opposingDesc.ProcessId}).");
                    }

                    Log.Warning("Opposing edition {Edition} terminated leaving active session (PID: {Pid})",
                        opposingDesc.OwnerEdition, opposingDesc.ProcessId);
                    return new CrossEditionInspectionResult(
                        CrossEditionOwnershipState.AbandonedByOtherEdition,
                        opposingDesc,
                        $"Session was abandoned by {opposingDesc.OwnerEdition} edition (Process {opposingDesc.ProcessId} terminated without restoration).");
                }
            }

            // 2. Inspect current edition
            if (File.Exists(_currentDescriptorPath))
            {
                var (currentDesc, corrupt) = ReadDescriptor(_currentDescriptorPath);
                if (corrupt)
                {
                    Log.Warning("Current edition coordination descriptor at {Path} is corrupted or incomplete", _currentDescriptorPath);
                    return new CrossEditionInspectionResult(
                        CrossEditionOwnershipState.CorruptedOrIncomplete,
                        null,
                        "Current edition coordination descriptor is corrupted or incomplete.");
                }

                if (currentDesc != null && string.Equals(currentDesc.OperationStatus, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentDesc.ProcessId == Environment.ProcessId)
                    {
                        return new CrossEditionInspectionResult(
                            CrossEditionOwnershipState.OwnedByCurrentEdition,
                            currentDesc,
                            "Session is owned by the current process.");
                    }

                    if (IsProcessAliveAndMatching(currentDesc.ProcessId, currentDesc.ProcessStartTimeUtc))
                    {
                        return new CrossEditionInspectionResult(
                            CrossEditionOwnershipState.OwnedByCurrentEdition,
                            currentDesc,
                            $"Session is owned by running instance of {_profileResolver.Profile} (Process {currentDesc.ProcessId}).");
                    }
                }
            }

            return new CrossEditionInspectionResult(
                CrossEditionOwnershipState.Free,
                null,
                "No active session ownership held across editions.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error inspecting cross-edition ownership; failing closed to Indeterminate");
            return new CrossEditionInspectionResult(
                CrossEditionOwnershipState.Indeterminate,
                null,
                $"Cross-edition inspection failed: {ex.Message}");
        }
    }

    public CrossEditionAcquisitionResult TryAcquireOwnership(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session ID must not be empty.", nameof(sessionId));

        if (_profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate)
        {
            return CrossEditionAcquisitionResult.Failed(
                CrossEditionOwnershipState.Indeterminate,
                "Cannot acquire cross-edition ownership in untrusted or indeterminate profile.");
        }

        Mutex? mutex = null;
        var hasLock = false;
        try
        {
            mutex = new Mutex(initiallyOwned: false, _mutexName);
            try
            {
                hasLock = mutex.WaitOne(3000);
            }
            catch (AbandonedMutexException)
            {
                hasLock = true;
            }

            if (!hasLock)
            {
                Log.Warning("Timeout waiting for cross-edition mutex {Mutex}", _mutexName);
                return CrossEditionAcquisitionResult.Failed(
                    CrossEditionOwnershipState.Indeterminate,
                    "Timed out acquiring cross-edition coordination lock.");
            }

            // Inspect opposing edition state under lock
            var inspection = InspectOwnership();
            if (inspection.RequiresSupervisionOnly)
            {
                Log.Warning("Refusing to acquire ownership: Opposing edition state is {State}", inspection.State);
                return CrossEditionAcquisitionResult.Failed(
                    inspection.State,
                    inspection.Message ?? "Cannot acquire ownership while opposing edition holds an active or unrecovered session.");
            }

            // Write current edition descriptor
            var now = DateTime.UtcNow;
            DateTime processStartTimeUtc;
            try
            {
                using var curProc = Process.GetCurrentProcess();
                processStartTimeUtc = curProc.StartTime.ToUniversalTime();
            }
            catch
            {
                processStartTimeUtc = now;
            }

            string userSid;
            try
            {
                userSid = WindowsIdentity.GetCurrent().User?.Value ?? string.Empty;
            }
            catch
            {
                userSid = string.Empty;
            }

            var descriptor = new CrossEditionDescriptor
            {
                SchemaVersion = 1,
                OwnerEdition = _profileResolver.Profile,
                UserSid = userSid,
                ProcessId = Environment.ProcessId,
                ProcessStartTimeUtc = processStartTimeUtc,
                SessionId = sessionId,
                AcquiredAtUtc = now,
                LastHeartbeatUtc = now,
                OperationStatus = "Active"
            };

            WriteDurableDescriptor(_currentDescriptorPath, descriptor);
            Log.Information("Acquired cross-edition ownership for session {SessionId} (Edition: {Edition})",
                sessionId, _profileResolver.Profile);

            return CrossEditionAcquisitionResult.Acquired(descriptor);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to acquire cross-edition ownership for session {SessionId}", sessionId);
            return CrossEditionAcquisitionResult.Failed(
                CrossEditionOwnershipState.Indeterminate,
                $"Cross-edition acquisition failed: {ex.Message}");
        }
        finally
        {
            if (hasLock && mutex != null)
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch { }
            }
            mutex?.Dispose();
        }
    }

    public bool TryReleaseOwnership(Guid sessionId)
    {
        Mutex? mutex = null;
        var hasLock = false;
        try
        {
            mutex = new Mutex(initiallyOwned: false, _mutexName);
            try
            {
                hasLock = mutex.WaitOne(3000);
            }
            catch (AbandonedMutexException)
            {
                hasLock = true;
            }

            if (!File.Exists(_currentDescriptorPath))
                return true;

            var (descriptor, _) = ReadDescriptor(_currentDescriptorPath);
            if (descriptor == null || descriptor.SessionId == sessionId || descriptor.ProcessId == Environment.ProcessId)
            {
                File.Delete(_currentDescriptorPath);
                Log.Information("Released cross-edition ownership for session {SessionId}", sessionId);
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error releasing cross-edition ownership for session {SessionId}", sessionId);
            return false;
        }
        finally
        {
            if (hasLock && mutex != null)
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch { }
            }
            mutex?.Dispose();
        }
    }

    public void UpdateHeartbeat(Guid sessionId)
    {
        try
        {
            if (!File.Exists(_currentDescriptorPath))
                return;

            var (descriptor, corrupt) = ReadDescriptor(_currentDescriptorPath);
            if (corrupt || descriptor == null || descriptor.SessionId != sessionId)
                return;

            var updated = descriptor with { LastHeartbeatUtc = DateTime.UtcNow };
            WriteDurableDescriptor(_currentDescriptorPath, updated);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to update cross-edition heartbeat for session {SessionId}", sessionId);
        }
    }

    internal static bool IsProcessAliveAndMatching(int pid, DateTime expectedStartTimeUtc)
    {
        if (pid <= 0)
            return false;

        try
        {
            using var process = Process.GetProcessById(pid);
            if (process.HasExited)
                return false;

            var actualStartTimeUtc = process.StartTime.ToUniversalTime();
            return Math.Abs((actualStartTimeUtc - expectedStartTimeUtc).TotalSeconds) <= 2.0;
        }
        catch (ArgumentException)
        {
            return false; // Process does not exist
        }
        catch (InvalidOperationException)
        {
            return false; // Process exited
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Could not verify process {Pid} status for cross-edition check; assuming not matching", pid);
            return false;
        }
    }

    private static (CrossEditionDescriptor? Descriptor, bool Corrupt) ReadDescriptor(string path)
    {
        if (!File.Exists(path))
            return (null, false);

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return (null, true);

            var descriptor = JsonSerializer.Deserialize<CrossEditionDescriptor>(json, JsonOptions);
            if (descriptor == null ||
                descriptor.SchemaVersion != 1 ||
                descriptor.SessionId == Guid.Empty ||
                descriptor.ProcessId <= 0 ||
                descriptor.ProcessStartTimeUtc == default)
            {
                return (null, true);
            }

            return (descriptor, false);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error reading cross-edition descriptor from {Path}", path);
            return (null, true);
        }
    }

    private static void WriteDurableDescriptor(string targetPath, CrossEditionDescriptor descriptor)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = targetPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            var json = JsonSerializer.Serialize(descriptor, JsonOptions);
            using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(tempPath, targetPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch { }
            }
        }
    }
}

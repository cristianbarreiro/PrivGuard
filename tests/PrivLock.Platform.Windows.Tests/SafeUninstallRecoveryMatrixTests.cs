using System.Text;
using System.Text.Json;
using PrivLock.Domain.Models;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public enum PathInspectionResult
{
    NotFound,
    AccessDenied,
    ReparsePoint,
    WrongEntryType,
    ValidDirectory,
    ValidFile,
    OtherError
}

public interface IPathInspector
{
    PathInspectionResult InspectPath(string path, bool isDirectory);
    long GetFileSize(string path);
    string ReadFileText(string path);
}

/// <summary>
/// Exact C# replica of Inno Setup Pascal rules in installer/setup.iss
/// (ReadJsonValue, JournalMayBeActive, RecoveryDirectoryAncestorsSafe, ProfileMayHaveActiveSession).
/// </summary>
public sealed class InnoSetupRulesSimulator
{
    private const int MaximumJournalBytes = 2097152;
    private readonly IPathInspector _inspector;

    public InnoSetupRulesSimulator(IPathInspector inspector)
    {
        _inspector = inspector;
    }

    public bool TryInspectPath(string path, bool isDirectory, out bool exists)
    {
        exists = false;
        var res = _inspector.InspectPath(path, isDirectory);
        if (res == PathInspectionResult.NotFound)
        {
            exists = false;
            return true;
        }

        if (res == (isDirectory ? PathInspectionResult.ValidDirectory : PathInspectionResult.ValidFile))
        {
            exists = true;
            return true;
        }

        // Fail-closed on AccessDenied, ReparsePoint, WrongEntryType, OtherError
        return false;
    }

    public bool JournalMayBeActive(string journalPath)
    {
        if (!TryInspectPath(journalPath, isDirectory: false, out var exists))
            return true; // Fail-closed

        if (!exists)
            return false;

        var size = _inspector.GetFileSize(journalPath);
        if (size <= 0 || size > MaximumJournalBytes)
            return true;

        string contents;
        try
        {
            contents = _inspector.ReadFileText(journalPath);
        }
        catch
        {
            return true;
        }

        if (string.IsNullOrEmpty(contents) || contents.Length > MaximumJournalBytes)
            return true;

        return ParseJournalJsonMayBeActive(contents);
    }

    /// <summary>
    /// Exact parser matching Pascal ReadJsonValue (setup.iss lines 220-362).
    /// Returns true if the journal may still be active (or corrupt / needs recovery).
    /// Returns false only if confirmed safely inactive.
    /// </summary>
    public static bool ParseJournalJsonMayBeActive(string json)
    {
        var position = 0;
        var conflictCount = 0;
        var scalar = string.Empty;

        // Context 1 = Root Object
        if (!ReadJsonValue(json, ref position, depth: 1, context: 1, ref scalar, ref conflictCount))
            return true; // Fail-closed

        SkipWhitespace(json, ref position);

        // Position <= Length(Json) in Pascal means trailing content exists after root object
        var trailingContentExists = position < json.Length;
        return trailingContentExists;
    }

    private static bool ReadJsonValue(
        string value,
        ref int position,
        int depth,
        int context,
        ref string scalar,
        ref int conflictCount)
    {
        if (depth > 64) return false;
        SkipWhitespace(value, ref position);
        if (position >= value.Length) return false;

        var opening = value[position];

        if (((context == 1 || context == 3) && opening != '{') ||
            (context == 2 && opening != '['))
            return false;

        if (opening == '{' || opening == '[')
        {
            var closing = opening == '{' ? '}' : ']';
            position++;
            var requiredFields = 0;
            var itemCount = 0;
            var sessionStatus = string.Empty;
            var restoredFlag = string.Empty;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            SkipWhitespace(value, ref position);
            if (position >= value.Length) return false;

            if (value[position] != closing)
            {
                while (true)
                {
                    var childContext = 0;
                    var key = string.Empty;

                    if (opening == '{')
                    {
                        if (!ReadJsonString(value, ref position, isKey: true, out key))
                            return false;

                        var normalizedKey = key.Trim('"').ToLowerInvariant();
                        if (seenKeys.Contains(normalizedKey))
                            return false; // Reject duplicate keys

                        seenKeys.Add(normalizedKey);
                        SkipWhitespace(value, ref position);
                        if (position >= value.Length || value[position] != ':')
                            return false;

                        position++;
                        if (context == 1 && normalizedKey == "resources")
                            childContext = 2; // Context 2 = resources array
                    }
                    else if (context == 2)
                    {
                        childContext = 3; // Context 3 = resource object
                    }

                    var childScalar = string.Empty;
                    if (!ReadJsonValue(value, ref position, depth + 1, childContext, ref childScalar, ref conflictCount))
                        return false;

                    itemCount++;
                    if (itemCount > 1024) return false;

                    if (context == 1) // Root Object
                    {
                        var normKey = key.Trim('"').ToLowerInvariant();
                        if (normKey == "schemaversion")
                        {
                            if (childScalar != "1") return false;
                            requiredFields |= 1;
                        }
                        else if (normKey == "isactive")
                        {
                            if (childScalar != "false") return false;
                            requiredFields |= 2;
                        }
                        else if (normKey == "status")
                        {
                            if (childScalar != "2" && childScalar != "3") return false;
                            sessionStatus = childScalar;
                            requiredFields |= 4;
                        }
                        else if (normKey == "wasrestored")
                        {
                            if (childScalar != "true" && childScalar != "false") return false;
                            restoredFlag = childScalar;
                            requiredFields |= 8;
                        }
                        else if (normKey == "resources")
                        {
                            requiredFields |= 16;
                        }
                    }
                    else if (context == 3) // Resource Object
                    {
                        var normKey = key.Trim('"').ToLowerInvariant();
                        if (normKey == "journalstate")
                        {
                            // Unchanged(3), ApplyFailed(4), Restored(6), Conflict(8)
                            if (childScalar != "3" && childScalar != "4" &&
                                childScalar != "6" && childScalar != "8")
                                return false;

                            if (childScalar == "8")
                                conflictCount++;

                            requiredFields |= 1;
                        }
                        else if (normKey == "modifiedbyprivlock")
                        {
                            if (childScalar != "false") return false;
                            requiredFields |= 2;
                        }
                        else if (normKey == "ownershipuncertain")
                        {
                            if (childScalar != "false") return false;
                            requiredFields |= 4;
                        }
                        else if (normKey == "executionmaystillbeinflight")
                        {
                            if (childScalar != "false") return false;
                            requiredFields |= 8;
                        }
                    }

                    SkipWhitespace(value, ref position);
                    if (position >= value.Length) return false;
                    if (value[position] == closing) break;
                    if (value[position] != ',') return false;
                    position++;
                    SkipWhitespace(value, ref position);
                }
            }

            position++; // Skip closing char

            if (context == 1)
            {
                if (requiredFields != 31) return false;
                if (sessionStatus == "2" && (restoredFlag != "true" || conflictCount != 0)) return false;
                if (sessionStatus == "3" && (restoredFlag != "false" || conflictCount == 0)) return false;
            }
            else if (context == 3)
            {
                if (requiredFields != 15) return false;
            }

            return true;
        }

        if (opening == '"')
        {
            return ReadJsonString(value, ref position, isKey: false, out scalar);
        }

        var start = position;
        while (position < value.Length)
        {
            var ch = value[position];
            if (ch == ',' || ch == ']' || ch == '}' || ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
                break;
            position++;
        }

        scalar = value[start..position];
        if (scalar is "true" or "false" or "null")
            return true;

        if (scalar.Length == 0) return false;
        var idx = 0;
        if (scalar[idx] == '-') idx++;
        if (idx >= scalar.Length) return false;
        if (scalar[idx] == '0' && idx < scalar.Length - 1) return false; // Non-canonical leading zero
        while (idx < scalar.Length)
        {
            if (scalar[idx] is < '0' or > '9') return false;
            idx++;
        }

        return true;
    }

    private static bool ReadJsonString(string value, ref int position, bool isKey, out string result)
    {
        result = string.Empty;
        if (position >= value.Length || value[position] != '"') return false;
        var start = position;
        position++;
        var escaped = false;
        while (position < value.Length)
        {
            var ch = value[position];
            if (escaped)
            {
                escaped = false;
            }
            else if (ch == '\\')
            {
                escaped = true;
            }
            else if (ch == '"')
            {
                position++;
                result = value[start..position];
                return true;
            }
            position++;
        }
        return false;
    }

    private static void SkipWhitespace(string value, ref int position)
    {
        while (position < value.Length)
        {
            var ch = value[position];
            if (ch is ' ' or '\t' or '\r' or '\n')
                position++;
            else
                break;
        }
    }

    public bool RecoveryDirectoryAncestorsSafe(string profilePath, string relativeDir, out bool directoryExists)
    {
        directoryExists = false;
        var fullPath = Path.Combine(profilePath, relativeDir);
        if (!fullPath.EndsWith('\\')) fullPath += "\\";

        for (var index = 4; index <= fullPath.Length; index++)
        {
            if (index == fullPath.Length || fullPath[index - 1] == '\\')
            {
                var parentPath = fullPath[..(index - 1)];
                if (!TryInspectPath(parentPath, isDirectory: true, out var exists))
                    return false; // Fail-closed on error or reparse point

                if (!exists)
                {
                    // If ancestor at or above profilePath is missing -> fail-closed
                    if (parentPath.Length <= profilePath.TrimEnd('\\').Length)
                        return false;

                    directoryExists = false;
                    return true;
                }
            }
        }

        directoryExists = true;
        return true;
    }

    public bool ProfilePathMayHaveActiveMarkerInDir(string profilePath, string relativeDir)
    {
        if (!RecoveryDirectoryAncestorsSafe(profilePath, relativeDir, out var dirExists))
            return true; // Fail-closed

        if (!dirExists)
            return false;

        var markerPath = Path.Combine(profilePath, relativeDir, "active-session-v1.marker");
        if (!TryInspectPath(markerPath, isDirectory: false, out var exists))
            return true; // Fail-closed

        return exists;
    }

    public bool ProfilePathMayHaveActiveJournalInDir(string profilePath, string relativeDir)
    {
        if (!RecoveryDirectoryAncestorsSafe(profilePath, relativeDir, out var dirExists))
            return true; // Fail-closed

        if (!dirExists)
            return false;

        var primary = Path.Combine(profilePath, relativeDir, "privacy-session-v1.json");
        return JournalMayBeActive(primary) || JournalMayBeActive(primary + ".bak");
    }

    public bool ProfileMayHaveActiveSession(string profilePath)
    {
        const string relGvard = @"AppData\Local\PrivGvard\Recovery\";
        const string relLock = @"AppData\Local\PrivLock\Recovery\";
        const string relGuard = @"AppData\Local\PrivGuard\Recovery\";

        return ProfilePathMayHaveActiveMarkerInDir(profilePath, relGvard) ||
               ProfilePathMayHaveActiveMarkerInDir(profilePath, relLock) ||
               ProfilePathMayHaveActiveJournalInDir(profilePath, relGvard) ||
               ProfilePathMayHaveActiveJournalInDir(profilePath, relLock) ||
               ProfilePathMayHaveActiveJournalInDir(profilePath, relGuard);
    }

    public bool EvaluateUninstallGate(string profilePath, bool hasHklmClaim)
    {
        return ProfileMayHaveActiveSession(profilePath) || hasHklmClaim;
    }

    public bool EvaluateAllProfilesUninstallGate(
        IReadOnlyList<string> profilePaths,
        bool hasHklmClaim,
        bool profileListEnumerationSucceeded = true)
    {
        if (!profileListEnumerationSucceeded)
            return true; // Fail-closed if profile enumeration fails

        if (hasHklmClaim)
            return true;

        foreach (var p in profilePaths)
        {
            // Reject unresolvable or dangerous profile paths
            if (string.IsNullOrWhiteSpace(p) || p.Contains('%') || p.Contains('/') ||
                p.Contains(@"\..") || p.Contains(@"\."))
                return true; // Fail-closed

            if (ProfileMayHaveActiveSession(p))
                return true;
        }

        return false;
    }
}

public sealed class RealFileSystemPathInspector : IPathInspector
{
    private readonly Dictionary<string, PathInspectionResult> _overrides = new(StringComparer.OrdinalIgnoreCase);

    public void SetOverride(string path, PathInspectionResult result)
    {
        _overrides[path] = result;
    }

    public PathInspectionResult InspectPath(string path, bool isDirectory)
    {
        if (_overrides.TryGetValue(path, out var ov))
            return ov;

        if (isDirectory)
        {
            if (Directory.Exists(path))
                return PathInspectionResult.ValidDirectory;
            if (File.Exists(path))
                return PathInspectionResult.WrongEntryType;
            return PathInspectionResult.NotFound;
        }

        if (File.Exists(path))
            return PathInspectionResult.ValidFile;
        if (Directory.Exists(path))
            return PathInspectionResult.WrongEntryType;
        return PathInspectionResult.NotFound;
    }

    public long GetFileSize(string path)
    {
        return new FileInfo(path).Length;
    }

    public string ReadFileText(string path)
    {
        return File.ReadAllText(path, Encoding.UTF8);
    }
}

public sealed class SafeUninstallRecoveryMatrixTests : IDisposable
{
    private readonly string _testRoot;
    private readonly RealFileSystemPathInspector _inspector;
    private readonly InnoSetupRulesSimulator _simulator;

    private static readonly JsonSerializerOptions ProductionJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        MaxDepth = 64
    };

    public SafeUninstallRecoveryMatrixTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "PrivGuard_UninstallMatrixHardening_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);
        _inspector = new RealFileSystemPathInspector();
        _simulator = new InnoSetupRulesSimulator(_inspector);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, true);
        }
        catch
        {
            // Cleanup
        }
    }

    private string CreateProfile(string name)
    {
        var path = Path.Combine(_testRoot, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private void SetupRecoveryFiles(
        string profilePath,
        string folderName,
        bool createMarker,
        string? primaryJournalContent,
        string? backupJournalContent)
    {
        var dir = Path.Combine(profilePath, "AppData", "Local", folderName, "Recovery");
        Directory.CreateDirectory(dir);

        if (createMarker)
        {
            File.WriteAllText(Path.Combine(dir, "active-session-v1.marker"), "active");
        }

        if (primaryJournalContent != null)
        {
            File.WriteAllText(Path.Combine(dir, "privacy-session-v1.json"), primaryJournalContent);
        }

        if (backupJournalContent != null)
        {
            File.WriteAllText(Path.Combine(dir, "privacy-session-v1.json.bak"), backupJournalContent);
        }
    }

    #region WAL Production Model Fixtures

    private static string CreateProductionRestoredSessionJson()
    {
        var session = new PrivacySession
        {
            SchemaVersion = 1,
            IsActive = false,
            Status = PrivacySessionStatus.Restored,
            WasRestored = true,
            Resources =
            [
                new OriginalDeviceState
                {
                    ResourceId = "dev-1",
                    OperationId = "op-1",
                    InstanceId = @"USB\VID_1234&PID_5678\0",
                    FriendlyName = "Test Camera",
                    DeviceClass = "Camera",
                    IsSafelyRestorable = true,
                    JournalState = PrivacyResourceJournalState.Restored,
                    ModifiedByPrivLock = false,
                    OwnershipUncertain = false,
                    ExecutionMayStillBeInFlight = false
                }
            ]
        };
        return JsonSerializer.Serialize(session, ProductionJsonOptions);
    }

    private static string CreateProductionCompletedWithConflictsSessionJson()
    {
        var session = new PrivacySession
        {
            SchemaVersion = 1,
            IsActive = false,
            Status = PrivacySessionStatus.CompletedWithConflicts,
            WasRestored = false,
            Resources =
            [
                new OriginalPolicyState
                {
                    ResourceId = "reg-1",
                    OperationId = "op-1",
                    RegistryHive = PrivacyRegistryHive.LocalMachine,
                    RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy",
                    ValueName = "LetAppsAccessCamera",
                    JournalState = PrivacyResourceJournalState.Conflict,
                    ModifiedByPrivLock = false,
                    OwnershipUncertain = false,
                    ExecutionMayStillBeInFlight = false
                }
            ]
        };
        return JsonSerializer.Serialize(session, ProductionJsonOptions);
    }

    private static string CreateProductionActiveSessionJson()
    {
        var session = new PrivacySession
        {
            SchemaVersion = 1,
            IsActive = true,
            Status = PrivacySessionStatus.Active,
            WasRestored = false,
            Resources = []
        };
        return JsonSerializer.Serialize(session, ProductionJsonOptions);
    }

    private static string CreateProductionSessionWithResource(
        PrivacyResourceJournalState resourceState,
        bool modified = false,
        bool uncertain = false,
        bool inFlight = false)
    {
        var session = new PrivacySession
        {
            SchemaVersion = 1,
            IsActive = false,
            Status = PrivacySessionStatus.Restored,
            WasRestored = true,
            Resources =
            [
                new OriginalAudioEndpointState
                {
                    ResourceId = "audio-1",
                    OperationId = "op-1",
                    EndpointId = "{0.0.1.00000000}.{abc}",
                    JournalState = resourceState,
                    ModifiedByPrivLock = modified,
                    OwnershipUncertain = uncertain,
                    ExecutionMayStillBeInFlight = inFlight
                }
            ]
        };
        return JsonSerializer.Serialize(session, ProductionJsonOptions);
    }

    #endregion

    #region Matrix Cases A through Z

    [Fact]
    public void CaseA_ActivePrivGvardMarker_BlocksUninstall()
    {
        var profile = CreateProfile("CaseA");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case A: Active PrivGvard marker must block uninstall.");
    }

    [Fact]
    public void CaseB_ActivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseB");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: CreateProductionActiveSessionJson(), backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case B: Active PrivGuard journal must block uninstall.");
    }

    [Fact]
    public void CaseC_MissingPrimaryWithActivePrivGuardBackupJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseC");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: null, backupJournalContent: CreateProductionActiveSessionJson());

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case C: Missing primary with active backup in PrivGuard must block uninstall.");
    }

    [Fact]
    public void CaseD_CorruptOrUnreadablePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseD");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: "{ invalid json corrupt ", backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case D: Corrupt PrivGuard journal must block uninstall.");
    }

    [Fact]
    public void CaseE_AllJournalsInactiveAndMarkersAbsent_AllowsUninstall()
    {
        var profile = CreateProfile("CaseE");
        var restoredJson = CreateProductionRestoredSessionJson();
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.False(blocked, "Case E: Inactive production journals without markers must allow uninstall.");
    }

    [Fact]
    public void CaseF_AllJournalsAbsentWithHklmOwnershipClaim_BlocksUninstall()
    {
        var profile = CreateProfile("CaseF");

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: true);
        Assert.True(blocked, "Case F: HKLM ownership claim without journals must block uninstall.");
    }

    [Fact]
    public void CaseG_ActiveMarkerAndHklmClaim_BlocksUninstall()
    {
        var profile = CreateProfile("CaseG");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: true);
        Assert.True(blocked, "Case G: Active marker + HKLM claim must block uninstall.");
    }

    [Fact]
    public void CaseH_ActivePrivLockJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseH");
        SetupRecoveryFiles(profile, "PrivLock", createMarker: false, primaryJournalContent: CreateProductionActiveSessionJson(), backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case H: Active PrivLock journal must block uninstall.");
    }

    [Fact]
    public void CaseI_CorruptPrivGvardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseI");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: "{ incomplete corrupt", backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case I: Corrupt PrivGvard journal must block uninstall.");
    }

    [Fact]
    public void CaseJ_ReparsePointOnRecoveryDirectoryOrAncestor_BlocksUninstall()
    {
        var profile = CreateProfile("CaseJ");
        var recoveryDir = Path.Combine(profile, @"AppData\Local\PrivGvard\Recovery");
        Directory.CreateDirectory(recoveryDir);
        _inspector.SetOverride(recoveryDir, PathInspectionResult.ReparsePoint);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case J: Reparse point in recovery path must fail-closed and block uninstall.");
    }

    [Fact]
    public void CaseK_CopiedMarkerUnderPrivGuardWithoutActiveJournal_DoesNotBlockUninstall()
    {
        var profile = CreateProfile("CaseK");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.False(blocked, "Case K: Copied marker in PrivGuard alone must NOT block uninstall.");
    }

    [Fact]
    public void CaseL_ActivePrimaryPrivGvardJournalWithInactivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseL");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: CreateProductionActiveSessionJson(), backupJournalContent: null);
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: CreateProductionRestoredSessionJson(), backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case L: Active primary PrivGvard journal must block uninstall.");
    }

    [Fact]
    public void CaseM_ActiveBackupPrivGvardJournalWithInactivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseM");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: CreateProductionRestoredSessionJson(), backupJournalContent: CreateProductionActiveSessionJson());
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: CreateProductionRestoredSessionJson(), backupJournalContent: null);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case M: Active backup PrivGvard journal must block uninstall.");
    }

    [Fact]
    public void CaseN_PrimaryJournalUnreadableByAcl_BlocksUninstall()
    {
        var profile = CreateProfile("CaseN");
        var dir = Path.Combine(profile, @"AppData\Local\PrivGuard\Recovery");
        Directory.CreateDirectory(dir);
        var primary = Path.Combine(dir, "privacy-session-v1.json");
        File.WriteAllText(primary, CreateProductionRestoredSessionJson());
        _inspector.SetOverride(primary, PathInspectionResult.AccessDenied);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case N: Access denied on primary journal must fail-closed and block uninstall.");
    }

    [Fact]
    public void CaseO_BackupJournalUnreadableByAcl_BlocksUninstall()
    {
        var profile = CreateProfile("CaseO");
        var dir = Path.Combine(profile, @"AppData\Local\PrivGuard\Recovery");
        Directory.CreateDirectory(dir);
        var backup = Path.Combine(dir, "privacy-session-v1.json.bak");
        File.WriteAllText(backup, CreateProductionRestoredSessionJson());
        _inspector.SetOverride(backup, PathInspectionResult.AccessDenied);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case O: Access denied on backup journal must fail-closed and block uninstall.");
    }

    [Fact]
    public void CaseP_PrimaryCorruptAndBackupApparentlyInactive_BlocksUninstall()
    {
        var profile = CreateProfile("CaseP");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false,
            primaryJournalContent: "{ corrupt primary",
            backupJournalContent: CreateProductionRestoredSessionJson());

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case P: Corrupt primary journal must block even if backup is inactive.");
    }

    [Fact]
    public void CaseQ_DuplicateKeysInRootJson_BlocksUninstall()
    {
        const string jsonWithDuplicateKeys =
            """
            {
              "schemaversion": 1,
              "isactive": false,
              "status": 2,
              "wasrestored": true,
              "schemaversion": 1,
              "resources": []
            }
            """;
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(jsonWithDuplicateKeys);
        Assert.True(mayBeActive, "Case Q: Duplicate keys in root must fail-closed and be treated as may-be-active.");
    }

    [Fact]
    public void CaseR_MissingRequiredRootField_BlocksUninstall()
    {
        // Missing "wasrestored"
        const string jsonMissingField =
            """
            {
              "schemaversion": 1,
              "isactive": false,
              "status": 2,
              "resources": []
            }
            """;
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(jsonMissingField);
        Assert.True(mayBeActive, "Case R: Missing required root field must fail-closed.");
    }

    [Fact]
    public void CaseS_StatusRestoredWithWasRestoredFalse_BlocksUninstall()
    {
        const string json =
            """
            {
              "schemaversion": 1,
              "isactive": false,
              "status": 2,
              "wasrestored": false,
              "resources": []
            }
            """;
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "Case S: status=Restored (2) with wasrestored=false must fail-closed.");
    }

    [Fact]
    public void CaseT_StatusCompletedWithConflictsWithoutConflicts_BlocksUninstall()
    {
        // Status 3 requires conflictCount > 0
        const string json =
            """
            {
              "schemaversion": 1,
              "isactive": false,
              "status": 3,
              "wasrestored": false,
              "resources": []
            }
            """;
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "Case T: status=CompletedWithConflicts (3) without conflict resources must fail-closed.");
    }

    [Fact]
    public void CaseU_StatusRestoredWithConflictResource_BlocksUninstall()
    {
        // Status 2 requires conflictCount == 0, but here resource has journalstate 8 (Conflict)
        const string json =
            """
            {
              "schemaversion": 1,
              "isactive": false,
              "status": 2,
              "wasrestored": true,
              "resources": [
                {
                  "journalstate": 8,
                  "modifiedbyprivlock": false,
                  "ownershipuncertain": false,
                  "executionmaystillbeinflight": false
                }
              ]
            }
            """;
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "Case U: status=Restored with a conflict resource must fail-closed.");
    }

    [Fact]
    public void CaseV_CompletedWithConflictsWithConflictAndSafeFlags_AllowsUninstall()
    {
        var json = CreateProductionCompletedWithConflictsSessionJson();
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.False(mayBeActive, "Case V: Valid CompletedWithConflicts session must be recognized as safely inactive.");
    }

    [Fact]
    public void CaseW_MoreThan1024Elements_BlocksUninstall()
    {
        var sb = new StringBuilder();
        sb.Append("{\"schemaversion\":1,\"isactive\":false,\"status\":2,\"wasrestored\":true,\"resources\":[");
        for (var i = 0; i < 1030; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("{\"journalstate\":6,\"modifiedbyprivlock\":false,\"ownershipuncertain\":false,\"executionmaystillbeinflight\":false}");
        }
        sb.Append("]}");

        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(sb.ToString());
        Assert.True(mayBeActive, "Case W: More than 1024 elements must exceed limits and fail-closed.");
    }

    [Fact]
    public void CaseX_JournalLargerThan2MiB_BlocksUninstall()
    {
        var profile = CreateProfile("CaseX");
        var dir = Path.Combine(profile, @"AppData\Local\PrivGuard\Recovery");
        Directory.CreateDirectory(dir);
        var primary = Path.Combine(dir, "privacy-session-v1.json");

        // Create file exceeding 2MB (2,097,152 bytes)
        var buffer = new byte[2097153];
        File.WriteAllBytes(primary, buffer);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case X: Journal exceeding 2 MiB must fail-closed.");
    }

    [Fact]
    public void CaseY_ValidJsonFollowedByTrailingTextOrSecondDocument_BlocksUninstall()
    {
        var validJson = CreateProductionRestoredSessionJson();
        var jsonWithTrailingText = validJson + " ExtraTrailingNonWhitespaceContent";

        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(jsonWithTrailingText);
        Assert.True(mayBeActive, "Case Y: Trailing content after root object must fail-closed.");
    }

    [Fact]
    public void CaseZ_ProfilePathInaccessibleOrUnresolvable_BlocksUninstall()
    {
        var profile = CreateProfile("CaseZ");
        _inspector.SetOverride(profile, PathInspectionResult.AccessDenied);

        var blocked = _simulator.EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case Z: Inaccessible profile path must fail-closed.");
    }

    #endregion

    #region Multi-Profile Scenarios

    [Fact]
    public void MultiProfile_OneActiveSessionAmongCleanProfiles_BlocksUninstall()
    {
        var profileClean1 = CreateProfile("UserClean1");
        var profileClean2 = CreateProfile("UserClean2");
        var profileActive = CreateProfile("UserActive");

        var restoredJson = CreateProductionRestoredSessionJson();
        SetupRecoveryFiles(profileClean1, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);
        SetupRecoveryFiles(profileClean2, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);
        SetupRecoveryFiles(profileActive, "PrivGvard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = _simulator.EvaluateAllProfilesUninstallGate(
            [profileClean1, profileClean2, profileActive],
            hasHklmClaim: false);

        Assert.True(blocked, "Multi-profile: A single active session across profiles must block uninstall.");
    }

    [Fact]
    public void MultiProfile_AllCleanProfiles_AllowsUninstall()
    {
        var profile1 = CreateProfile("MultiUser1");
        var profile2 = CreateProfile("MultiUser2");

        var restoredJson = CreateProductionRestoredSessionJson();
        SetupRecoveryFiles(profile1, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);
        SetupRecoveryFiles(profile2, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);

        var blocked = _simulator.EvaluateAllProfilesUninstallGate(
            [profile1, profile2],
            hasHklmClaim: false);

        Assert.False(blocked, "Multi-profile: When all profiles are inactive, uninstall is allowed.");
    }

    [Fact]
    public void MultiProfile_UnresolvableOrTraversingPath_BlocksUninstall()
    {
        var validProfile = CreateProfile("ValidUser");
        var restoredJson = CreateProductionRestoredSessionJson();
        SetupRecoveryFiles(validProfile, "PrivGuard", createMarker: false, primaryJournalContent: restoredJson, backupJournalContent: null);

        var blocked = _simulator.EvaluateAllProfilesUninstallGate(
            [validProfile, @"C:\Users\BadUser\..\Traversal"],
            hasHklmClaim: false);

        Assert.True(blocked, "Multi-profile: Path traversal or unresolvable profile must fail-closed.");
    }

    [Fact]
    public void MultiProfile_EnumerationFailure_BlocksUninstall()
    {
        var blocked = _simulator.EvaluateAllProfilesUninstallGate(
            [],
            hasHklmClaim: false,
            profileListEnumerationSucceeded: false);

        Assert.True(blocked, "Multi-profile: Failure to enumerate ProfileList registry must fail-closed.");
    }

    #endregion

    #region WAL State Fixtures Tests

    [Theory]
    [InlineData(PrivacyResourceJournalState.ApplyPending)]
    [InlineData(PrivacyResourceJournalState.RestorePending)]
    [InlineData(PrivacyResourceJournalState.RestoreFailed)]
    [InlineData(PrivacyResourceJournalState.Captured)]
    [InlineData(PrivacyResourceJournalState.Applied)]
    [InlineData(PrivacyResourceJournalState.Missing)]
    public void WALFixtures_NonTerminalStates_BlockUninstall(PrivacyResourceJournalState nonTerminalState)
    {
        var json = CreateProductionSessionWithResource(nonTerminalState);
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, $"WAL Fixture: Resource in non-terminal state {nonTerminalState} must require recovery and block uninstall.");
    }

    [Fact]
    public void WALFixtures_OwnershipUncertain_BlocksUninstall()
    {
        var json = CreateProductionSessionWithResource(PrivacyResourceJournalState.Restored, uncertain: true);
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "WAL Fixture: Resource with OwnershipUncertain=true must block uninstall.");
    }

    [Fact]
    public void WALFixtures_ExecutionInFlight_BlocksUninstall()
    {
        var json = CreateProductionSessionWithResource(PrivacyResourceJournalState.Restored, inFlight: true);
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "WAL Fixture: Resource with ExecutionMayStillBeInFlight=true must block uninstall.");
    }

    [Fact]
    public void WALFixtures_ModifiedByPrivLockStillTrue_BlocksUninstall()
    {
        var json = CreateProductionSessionWithResource(PrivacyResourceJournalState.Restored, modified: true);
        var mayBeActive = InnoSetupRulesSimulator.ParseJournalJsonMayBeActive(json);
        Assert.True(mayBeActive, "WAL Fixture: Resource with ModifiedByPrivLock=true must block uninstall.");
    }

    #endregion
}

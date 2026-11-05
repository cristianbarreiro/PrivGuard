using System.Text.Json;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class SafeUninstallRecoveryMatrixTests : IDisposable
{
    private readonly string _testRoot;

    public SafeUninstallRecoveryMatrixTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "PrivGuard_UninstallMatrix_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);
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
            // Best-effort cleanup of temporary test root
        }
    }

    private string CreateProfile(string name)
    {
        var profilePath = Path.Combine(_testRoot, name);
        Directory.CreateDirectory(profilePath);
        return profilePath;
    }

    private static string InactiveJournalJson =>
        """
        {
          "schemaversion": 1,
          "isactive": false,
          "status": 2,
          "wasrestored": true,
          "resources": [
            {
              "journalstate": 6,
              "modifiedbyprivlock": false,
              "ownershipuncertain": false,
              "executionmaystillbeinflight": false
            }
          ]
        }
        """;

    private static string ActiveJournalJson =>
        """
        {
          "schemaversion": 1,
          "isactive": true,
          "status": 1,
          "wasrestored": false,
          "resources": []
        }
        """;

    /// <summary>
    /// Simulates Inno Setup's JournalMayBeActive logic.
    /// Returns false only if the journal is verified to be durably inactive.
    /// Missing journal returns false. Corrupt or active returns true.
    /// </summary>
    private static bool InnoSimulatedJournalMayBeActive(string journalPath)
    {
        if (!File.Exists(journalPath))
            return false;

        try
        {
            var fi = new FileInfo(journalPath);
            if (fi.Length == 0 || fi.Length > 2097152)
                return true; // Corrupt/empty/too large -> Fail-closed

            var json = File.ReadAllText(journalPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetProperty("schemaversion").GetInt32() != 1) return true;
            if (root.GetProperty("isactive").GetBoolean()) return true;

            var status = root.GetProperty("status").GetInt32();
            var wasRestored = root.GetProperty("wasrestored").GetBoolean();

            if (status != 2 && status != 3) return true;
            if (status == 2 && !wasRestored) return true;

            if (root.TryGetProperty("resources", out var resources))
            {
                foreach (var res in resources.EnumerateArray())
                {
                    var js = res.GetProperty("journalstate").GetInt32();
                    if (js != 3 && js != 4 && js != 6 && js != 8) return true;
                    if (res.GetProperty("modifiedbyprivlock").GetBoolean()) return true;
                    if (res.GetProperty("ownershipuncertain").GetBoolean()) return true;
                    if (res.GetProperty("executionmaystillbeinflight").GetBoolean()) return true;
                }
            }

            return false;
        }
        catch
        {
            return true; // Fail-closed on parse error
        }
    }

    /// <summary>
    /// Simulates Inno Setup ProfilePathMayHaveActiveMarkerInDir
    /// </summary>
    private static bool InnoSimulatedProfilePathMayHaveActiveMarkerInDir(string profilePath, string relativeDir)
    {
        var recoveryPath = Path.Combine(profilePath, relativeDir);
        if (!Directory.Exists(recoveryPath))
            return false;

        var markerPath = Path.Combine(recoveryPath, "active-session-v1.marker");
        return File.Exists(markerPath);
    }

    /// <summary>
    /// Simulates Inno Setup ProfilePathMayHaveActiveJournalInDir
    /// </summary>
    private static bool InnoSimulatedProfilePathMayHaveActiveJournalInDir(string profilePath, string relativeDir)
    {
        var recoveryPath = Path.Combine(profilePath, relativeDir);
        if (!Directory.Exists(recoveryPath))
            return false;

        var primary = Path.Combine(recoveryPath, "privacy-session-v1.json");
        var backup = primary + ".bak";

        return InnoSimulatedJournalMayBeActive(primary) || InnoSimulatedJournalMayBeActive(backup);
    }

    /// <summary>
    /// Simulates Inno Setup ProfileMayHaveActiveSession with updated 8B.0.1 rules:
    /// Markers inspected in PrivGvard and PrivLock.
    /// Journals inspected in PrivGvard, PrivLock, and PrivGuard.
    /// Copied marker under PrivGuard is NOT inspected.
    /// </summary>
    private static bool InnoSimulatedProfileMayHaveActiveSession(string profilePath)
    {
        const string relGvard = @"AppData\Local\PrivGvard\Recovery\";
        const string relLock = @"AppData\Local\PrivLock\Recovery\";
        const string relGuard = @"AppData\Local\PrivGuard\Recovery\";

        return InnoSimulatedProfilePathMayHaveActiveMarkerInDir(profilePath, relGvard) ||
               InnoSimulatedProfilePathMayHaveActiveMarkerInDir(profilePath, relLock) ||
               InnoSimulatedProfilePathMayHaveActiveJournalInDir(profilePath, relGvard) ||
               InnoSimulatedProfilePathMayHaveActiveJournalInDir(profilePath, relLock) ||
               InnoSimulatedProfilePathMayHaveActiveJournalInDir(profilePath, relGuard);
    }

    private static bool EvaluateUninstallGate(string profilePath, bool hasHklmClaim)
    {
        return InnoSimulatedProfileMayHaveActiveSession(profilePath) || hasHklmClaim;
    }

    private static void SetupRecoveryFiles(
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
            File.WriteAllText(Path.Combine(dir, "active-session-v1.marker"), "marker");
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

    [Fact]
    public void CaseA_ActivePrivGvardMarker_BlocksUninstall()
    {
        var profile = CreateProfile("CaseA");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case A: Active PrivGvard marker must block uninstall.");
    }

    [Fact]
    public void CaseB_ActivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseB");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: ActiveJournalJson, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case B: Active PrivGuard journal must block uninstall.");
    }

    [Fact]
    public void CaseC_MissingPrimaryWithActivePrivGuardBackupJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseC");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: null, backupJournalContent: ActiveJournalJson);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case C: Missing primary with active backup in PrivGuard must block uninstall.");
    }

    [Fact]
    public void CaseD_CorruptOrUnreadablePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseD");
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: "{ invalid json corrupt ", backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case D: Corrupt PrivGuard journal must block uninstall.");
    }

    [Fact]
    public void CaseE_AllJournalsInactiveAndMarkersAbsent_AllowsUninstall()
    {
        var profile = CreateProfile("CaseE");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: InactiveJournalJson, backupJournalContent: null);
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: InactiveJournalJson, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.False(blocked, "Case E: Inactive journals without markers must allow uninstall.");
    }

    [Fact]
    public void CaseF_AllJournalsAbsentWithHklmOwnershipClaim_BlocksUninstall()
    {
        var profile = CreateProfile("CaseF");

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: true);
        Assert.True(blocked, "Case F: HKLM ownership claim without journals must block uninstall.");
    }

    [Fact]
    public void CaseG_ActiveMarkerAndHklmClaim_BlocksUninstall()
    {
        var profile = CreateProfile("CaseG");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: true);
        Assert.True(blocked, "Case G: Active marker + HKLM claim must block uninstall.");
    }

    [Fact]
    public void CaseH_ActivePrivLockJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseH");
        SetupRecoveryFiles(profile, "PrivLock", createMarker: false, primaryJournalContent: ActiveJournalJson, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case H: Active PrivLock journal must block uninstall.");
    }

    [Fact]
    public void CaseI_CorruptPrivGvardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseI");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: "{ corrupt content", backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case I: Corrupt PrivGvard journal must block uninstall.");
    }

    [Fact]
    public void CaseK_CopiedMarkerUnderPrivGuardWithoutActiveJournal_DoesNotBlockUninstall()
    {
        var profile = CreateProfile("CaseK");
        // Marker exists in PrivGuard (migrated artifact), but no marker in PrivGvard/PrivLock and journals are absent
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: true, primaryJournalContent: null, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.False(blocked, "Case K: Copied marker in PrivGuard alone must NOT block uninstall.");
    }

    [Fact]
    public void CaseL_ActivePrimaryPrivGvardJournalWithInactivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseL");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: ActiveJournalJson, backupJournalContent: null);
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: InactiveJournalJson, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case L: Active primary PrivGvard journal must block uninstall.");
    }

    [Fact]
    public void CaseM_ActiveBackupPrivGvardJournalWithInactivePrivGuardJournal_BlocksUninstall()
    {
        var profile = CreateProfile("CaseM");
        SetupRecoveryFiles(profile, "PrivGvard", createMarker: false, primaryJournalContent: InactiveJournalJson, backupJournalContent: ActiveJournalJson);
        SetupRecoveryFiles(profile, "PrivGuard", createMarker: false, primaryJournalContent: InactiveJournalJson, backupJournalContent: null);

        var blocked = EvaluateUninstallGate(profile, hasHklmClaim: false);
        Assert.True(blocked, "Case M: Active backup PrivGvard journal must block uninstall.");
    }
}

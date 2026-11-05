using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class InstallerUninstallRegistrationTests
{
    [Fact]
    public void InnoRegistersSafetyWrapperWithItsActualUninstallerPathAndStableAppId()
    {
        var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "setup.iss"));

        Assert.Contains(
            "#define MyAppId \"{{8E0F7A12-BFB3-4FE8-B9A5-48FD50A15A9A}\"",
            script,
            StringComparison.Ordinal);
        Assert.Contains("AppId={#MyAppId}", script, StringComparison.Ordinal);
        Assert.Contains(
            "WrapperPath := ExpandConstant('{app}\\{#MyAppExeName}');",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "UninstallerPath := ExpandConstant('{uninstallexe}');",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "UninstallCommand := Quoted(WrapperPath) + ' --safe-uninstall ' + Quoted(UninstallerPath);",
            script,
            StringComparison.Ordinal);
        Assert.Contains("QuietUninstallCommand := UninstallCommand + ' --quiet';", script, StringComparison.Ordinal);
        Assert.Contains("'UninstallString', UninstallCommand", script, StringComparison.Ordinal);
        Assert.Contains("'QuietUninstallString', QuietUninstallCommand", script, StringComparison.Ordinal);
    }

    [Fact]
    public void InnoDefinesModernRecoveryRelativePathAndSeparatesMarkerFromJournalInspection()
    {
        var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "setup.iss"));

        Assert.Contains(
            "#define ModernRecoveryRelativePath \"AppData\\Local\\PrivGuard\\Recovery\\\"",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "#define RecoveryRelativePath \"AppData\\Local\\PrivGvard\\Recovery\\\"",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "#define LegacyRecoveryRelativePath \"AppData\\Local\\PrivLock\\Recovery\\\"",
            script,
            StringComparison.Ordinal);

        Assert.Contains("function RecoveryDirectoryAncestorsSafe", script, StringComparison.Ordinal);
        Assert.Contains("function ProfilePathMayHaveActiveMarkerInDir", script, StringComparison.Ordinal);
        Assert.Contains("function ProfilePathMayHaveActiveJournalInDir", script, StringComparison.Ordinal);
        Assert.Contains("function ProfilePathMayHaveActiveSessionInDir", script, StringComparison.Ordinal);

        // Verify ProfileMayHaveActiveSession checks journals under PrivGuard but markers only under PrivGvard and PrivLock
        Assert.Contains(
            "ProfilePathMayHaveActiveSessionInDir(ProfilePath, '{#RecoveryRelativePath}')",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "ProfilePathMayHaveActiveSessionInDir(ProfilePath, '{#LegacyRecoveryRelativePath}')",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "ProfilePathMayHaveActiveJournalInDir(ProfilePath, '{#ModernRecoveryRelativePath}')",
            script,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ProfilePathMayHaveActiveSessionInDir(ProfilePath, '{#ModernRecoveryRelativePath}')",
            script,
            StringComparison.Ordinal);
    }
}

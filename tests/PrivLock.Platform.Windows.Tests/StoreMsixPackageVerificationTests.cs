using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using PrivLock.Platform.Windows.Packaging;
using Xunit;

namespace PrivLock.Platform.Windows.Tests;

public sealed class StoreMsixPackageVerificationTests : IDisposable
{
    private readonly string _tempDir;

    public StoreMsixPackageVerificationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"PrivGuard_StoreVerifierTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "PrivGvard.sln")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing PrivGvard.sln.");
    }

    private static string CreateManifestXml(
        string name = StoreMsixManifestVerifier.ExpectedPackageName,
        string publisher = StoreMsixManifestVerifier.ExpectedPublisher,
        string publisherDisplayName = StoreMsixManifestVerifier.ExpectedPublisherDisplayName,
        IEnumerable<string>? restrictedCapabilities = null)
    {
        var caps = restrictedCapabilities ?? ["runFullTrust"];
        var capElements = string.Join("\n    ", caps.Select(c => $"<rescap:Capability Name=\"{c}\" />"));

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
  xmlns=""http://schemas.microsoft.com/appx/manifest/foundation/windows10""
  xmlns:uap=""http://schemas.microsoft.com/appx/manifest/uap/windows10""
  xmlns:rescap=""http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities""
  IgnorableNamespaces=""uap rescap"">
  <Identity
    Name=""{name}""
    Publisher=""{publisher}""
    Version=""2.0.0.0""
    ProcessorArchitecture=""x64"" />
  <Properties>
    <DisplayName>PrivGuard</DisplayName>
    <PublisherDisplayName>{publisherDisplayName}</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Applications>
    <Application Id=""PrivGuardApp"" Executable=""PrivGvard.exe"" EntryPoint=""Windows.FullTrustApplication"">
      <uap:VisualElements DisplayName=""PrivGuard"" Description=""Privacy Guard"" BackgroundColor=""transparent"" Square150x150Logo=""Assets\Square150x150Logo.png"" Square44x44Logo=""Assets\Square44x44Logo.png"" />
    </Application>
  </Applications>
  <Capabilities>
    {capElements}
  </Capabilities>
</Package>";
    }

    private string CreateFakeMsixPackage(string fileName, string? manifestXml = null)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        using (var zipStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            if (manifestXml != null)
            {
                var entry = archive.CreateEntry("AppxManifest.xml");
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(manifestXml);
            }
        }
        return filePath;
    }

    // =========================================================================
    // 1. C# Manifest Verifier: Positive & Official Template Tests
    // =========================================================================

    [Fact]
    public void VerifyManifest_OfficialStoreManifestTemplate_ReturnsSuccess()
    {
        var repoRoot = FindRepoRoot();
        var manifestPath = Path.Combine(repoRoot, "packaging", "Package.appxmanifest");
        Assert.True(File.Exists(manifestPath));

        var xml = File.ReadAllText(manifestPath);
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPackageName, result.PackageName);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPublisher, result.Publisher);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPublisherDisplayName, result.PublisherDisplayName);
        Assert.Equal(["runFullTrust"], result.RestrictedCapabilities);
    }

    [Fact]
    public void VerifyManifest_GeneratedValidManifestXml_ReturnsSuccess()
    {
        var xml = CreateManifestXml();
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPackageName, result.PackageName);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPublisher, result.Publisher);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPublisherDisplayName, result.PublisherDisplayName);
        Assert.Equal(["runFullTrust"], result.RestrictedCapabilities);
    }

    // =========================================================================
    // 2. C# Manifest Verifier: Negative Cases (Identity, Publisher, DisplayName)
    // =========================================================================

    [Fact]
    public void VerifyManifest_DifferentPackageName_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(name: "cdevstudios.PrivGuard.Other");
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("Identity/Name mismatch", result.ErrorMessage);
    }

    [Fact]
    public void VerifyManifest_DifferentPublisher_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(publisher: "CN=UntrustedPublisher");
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("Identity/Publisher mismatch", result.ErrorMessage);
    }

    [Fact]
    public void VerifyManifest_DifferentPublisherDisplayName_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(publisherDisplayName: "Other Studios");
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("Properties/PublisherDisplayName mismatch", result.ErrorMessage);
    }

    [Fact]
    public void VerifyManifest_AccidentalDevLabManifest_FailsExplicitly()
    {
        var repoRoot = FindRepoRoot();
        var devLabManifestPath = Path.Combine(repoRoot, "packaging", "Package.DevLab.appxmanifest");
        Assert.True(File.Exists(devLabManifestPath));

        var xml = File.ReadAllText(devLabManifestPath);
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("DevLab", result.ErrorMessage);
    }

    // =========================================================================
    // 3. C# Manifest Verifier: Negative Cases (Capabilities & D-01 Guard)
    // =========================================================================

    [Fact]
    public void VerifyManifest_MissingRunFullTrust_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(restrictedCapabilities: []);
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("runFullTrust", result.ErrorMessage);
    }

    [Fact]
    public void VerifyManifest_PresenceOfAllowElevation_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(restrictedCapabilities: ["runFullTrust", "allowElevation"]);
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("allowElevation", result.ErrorMessage);
        Assert.Contains("violates D-01", result.ErrorMessage);
    }

    [Fact]
    public void VerifyManifest_PresenceOfUnvirtualizedResources_FailsWithAppropriateError()
    {
        var xml = CreateManifestXml(restrictedCapabilities: ["runFullTrust", "unvirtualizedResources"]);
        var result = StoreMsixManifestVerifier.VerifyManifest(xml);

        Assert.False(result.IsValid);
        Assert.Contains("unvirtualizedResources", result.ErrorMessage);
        Assert.Contains("violates D-01", result.ErrorMessage);
    }

    // =========================================================================
    // 4. C# Package Archive & Directory Verifier Tests
    // =========================================================================

    [Fact]
    public void VerifyPackageFile_ValidMsix_ReturnsSuccess()
    {
        var validXml = CreateManifestXml();
        var packagePath = CreateFakeMsixPackage("PrivGuard-Valid.msix", validXml);

        var result = StoreMsixManifestVerifier.VerifyPackageFile(packagePath);

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(StoreMsixManifestVerifier.ExpectedPackageName, result.PackageName);
    }

    [Fact]
    public void VerifyPackageFile_MissingAppxManifestXmlInZip_Fails()
    {
        var packagePath = CreateFakeMsixPackage("PrivGuard-NoManifest.msix", manifestXml: null);

        var result = StoreMsixManifestVerifier.VerifyPackageFile(packagePath);

        Assert.False(result.IsValid);
        Assert.Contains("AppxManifest.xml was not found", result.ErrorMessage);
    }

    [Fact]
    public void VerifyPackageDirectory_ZeroMsixCandidates_Fails()
    {
        var emptySubDir = Path.Combine(_tempDir, "EmptyDir");
        Directory.CreateDirectory(emptySubDir);

        var result = StoreMsixManifestVerifier.VerifyPackageDirectory(emptySubDir);

        Assert.False(result.IsValid);
        Assert.Contains("No .msix packages found", result.ErrorMessage);
    }

    [Fact]
    public void VerifyPackageDirectory_MultipleMsixCandidates_Fails()
    {
        var multiSubDir = Path.Combine(_tempDir, "MultiDir");
        Directory.CreateDirectory(multiSubDir);

        var validXml = CreateManifestXml();
        CreateFakeMsixPackage(Path.Combine(multiSubDir, "Pkg1.msix"), validXml);
        CreateFakeMsixPackage(Path.Combine(multiSubDir, "Pkg2.msix"), validXml);

        var result = StoreMsixManifestVerifier.VerifyPackageDirectory(multiSubDir);

        Assert.False(result.IsValid);
        Assert.Contains("Expected exactly 1 .msix package", result.ErrorMessage);
    }

    // =========================================================================
    // 5. PowerShell Verification Script Execution Tests (Positive & Negative)
    // =========================================================================

    [Fact]
    public void PowerShellVerifyScript_OnValidPackage_ExitsZero()
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "scripts", "verify-store-msix.ps1");
        Assert.True(File.Exists(scriptPath));

        var validXml = CreateManifestXml();
        var packagePath = CreateFakeMsixPackage("PrivGuard-ScriptTest-Valid.msix", validXml);

        var (exitCode, stdout, stderr) = RunPowerShellScript(scriptPath, $"-PackagePath \"{packagePath}\"");

        Assert.Equal(0, exitCode);
        Assert.Contains("Store MSIX Verification: PASS", stdout);
    }

    [Fact]
    public void PowerShellVerifyScript_OnForbiddenAllowElevation_ExitsOne()
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "scripts", "verify-store-msix.ps1");
        Assert.True(File.Exists(scriptPath));

        var badXml = CreateManifestXml(restrictedCapabilities: ["runFullTrust", "allowElevation"]);
        var packagePath = CreateFakeMsixPackage("PrivGuard-ScriptTest-AllowElev.msix", badXml);

        var (exitCode, stdout, stderr) = RunPowerShellScript(scriptPath, $"-PackagePath \"{packagePath}\"");

        Assert.Equal(1, exitCode);
        Assert.True(stdout.Contains("allowElevation") || stderr.Contains("allowElevation"));
    }

    [Fact]
    public void PowerShellVerifyScript_OnEmptyDirectory_ExitsOne()
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "scripts", "verify-store-msix.ps1");
        Assert.True(File.Exists(scriptPath));

        var emptySubDir = Path.Combine(_tempDir, "ScriptEmptyDir");
        Directory.CreateDirectory(emptySubDir);

        var (exitCode, stdout, stderr) = RunPowerShellScript(scriptPath, $"-PackageDir \"{emptySubDir}\"");

        Assert.Equal(1, exitCode);
        Assert.True(stdout.Contains("Expected exactly 1 .msix package") || stderr.Contains("Expected exactly 1 .msix package"));
    }

    private static (int ExitCode, string Stdout, string Stderr) RunPowerShellScript(string scriptPath, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to launch PowerShell process.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(15000);

        return (process.ExitCode, stdout, stderr);
    }
}

using System.Runtime.InteropServices;
using System.Text;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

/// <summary>
/// Windows-specific package identity detector implementing IPackageIdentityDetector.
/// Uses native kernel32 GetCurrentPackageFamilyName and GetCurrentPackageFullName,
/// validating against the authentic PrivGuard Store package identity with fail-closed semantics.
/// </summary>
public sealed class WindowsPackageIdentityDetector : IPackageIdentityDetector
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsPackageIdentityDetector>();

    public const string ExpectedPackageName = "cdevstudios.PrivGuard";
    public const string ExpectedPublisherCn = "CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD";
    public const string ExpectedPackageFamilyName = "cdevstudios.PrivGuard_tv7b4yqg10sw2";
    public const string ExpectedPublisherId = "tv7b4yqg10sw2";
    public const string ExpectedStoreId = "9NQ56N45XH5N";

    private const int ERROR_SUCCESS = 0;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int APPMODEL_ERROR_NO_PACKAGE = 15700;

    internal delegate int NativeGetPackageNameInvoker(ref int length, StringBuilder? buffer);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFamilyName(ref int packageFamilyNameLength, StringBuilder? packageFamilyName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    public PackageIdentityKind Kind { get; }

    public string? PackageFullName { get; }

    public string? PackageFamilyName { get; }

    public WindowsPackageIdentityDetector()
        : this(GetCurrentPackageFamilyName, GetCurrentPackageFullName)
    {
    }

    internal WindowsPackageIdentityDetector(
        NativeGetPackageNameInvoker getFamilyNameInvoker,
        NativeGetPackageNameInvoker getFullNameInvoker)
    {
        var (kind, fullName, familyName) = DetectIdentity(getFamilyNameInvoker, getFullNameInvoker);
        Kind = kind;
        PackageFullName = fullName;
        PackageFamilyName = familyName;
    }

    internal WindowsPackageIdentityDetector(
        PackageIdentityKind kind,
        string? packageFullName = null,
        string? packageFamilyName = null)
    {
        Kind = kind;
        PackageFullName = packageFullName;
        PackageFamilyName = packageFamilyName;
    }

    public bool HasExpectedPackageIdentity() => Kind == PackageIdentityKind.AuthenticStorePackage;

    private static (PackageIdentityKind Kind, string? FullName, string? FamilyName) DetectIdentity(
        NativeGetPackageNameInvoker getFamilyNameInvoker,
        NativeGetPackageNameInvoker getFullNameInvoker)
    {
        try
        {
            if (getFamilyNameInvoker == null || getFullNameInvoker == null)
            {
                Log.Warning("Native package invokers are null; failing closed.");
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            // 1. Probe for package family name buffer length
            int familyLength = 0;
            int familyResult = getFamilyNameInvoker(ref familyLength, null);

            // Explicit unpackaged process (standard Win32)
            if (familyResult == APPMODEL_ERROR_NO_PACKAGE)
            {
                Log.Debug("Process is not running within a package container (APPMODEL_ERROR_NO_PACKAGE).");
                return (PackageIdentityKind.NotPackaged, null, null);
            }

            // If not APPMODEL_ERROR_NO_PACKAGE and not ERROR_INSUFFICIENT_BUFFER (or ERROR_SUCCESS with length > 0),
            // it's an unexpected native error
            if (familyResult != ERROR_INSUFFICIENT_BUFFER && (familyResult != ERROR_SUCCESS || familyLength <= 0))
            {
                Log.Warning("Unexpected native error probing package family name: Win32 error {ErrorCode}", familyResult);
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            if (familyLength <= 0)
            {
                Log.Warning("Package family name returned invalid buffer length: {Length}", familyLength);
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            // 2. Fetch the package family name
            var familyBuffer = new StringBuilder(familyLength);
            familyResult = getFamilyNameInvoker(ref familyLength, familyBuffer);
            if (familyResult != ERROR_SUCCESS)
            {
                Log.Warning("Failed to retrieve package family name: Win32 error {ErrorCode}", familyResult);
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            string familyName = familyBuffer.ToString().Trim();
            if (string.IsNullOrEmpty(familyName))
            {
                Log.Warning("Package family name is empty after successful native query.");
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            // 3. Retrieve package full name for diagnostic logging
            string? fullName = null;
            int fullLength = 0;
            int fullResult = getFullNameInvoker(ref fullLength, null);
            if (fullResult == ERROR_INSUFFICIENT_BUFFER && fullLength > 0)
            {
                var fullBuffer = new StringBuilder(fullLength);
                if (getFullNameInvoker(ref fullLength, fullBuffer) == ERROR_SUCCESS)
                {
                    fullName = fullBuffer.ToString().Trim();
                }
                else
                {
                    Log.Warning("Failed to retrieve package full name string even though family name succeeded.");
                }
            }
            else if (fullResult == APPMODEL_ERROR_NO_PACKAGE)
            {
                Log.Warning("Inconsistent package identity: family name query succeeded ({FamilyName}) but full name returned APPMODEL_ERROR_NO_PACKAGE.", familyName);
                return (PackageIdentityKind.DetectionFailed, null, null);
            }

            // 4. Exact comparison against canonical ExpectedPackageFamilyName
            if (string.Equals(familyName, ExpectedPackageFamilyName, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("Process verified as AuthenticStorePackage ({PackageFamilyName})", familyName);
                return (PackageIdentityKind.AuthenticStorePackage, fullName, familyName);
            }

            Log.Warning("Process is running within unexpected package identity: {PackageFamilyName} (Expected: {Expected})",
                familyName, ExpectedPackageFamilyName);
            return (PackageIdentityKind.UnexpectedPackage, fullName, familyName);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Exception querying Windows package identity; failing closed to DetectionFailed.");
            return (PackageIdentityKind.DetectionFailed, null, null);
        }
    }
}

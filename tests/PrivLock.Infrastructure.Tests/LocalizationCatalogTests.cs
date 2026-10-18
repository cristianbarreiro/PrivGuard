using PrivLock.Infrastructure.Common.Localization;
using Xunit;

namespace PrivLock.Infrastructure.Tests;

public class LocalizationCatalogTests
{
    [Theory]
    [InlineData("es", "🔒 Bloqueado")]
    [InlineData("en", "🔒 Blocked")]
    public void Get_StatusBlocked_ReturnsCorrectTranslation(string lang, string expected)
    {
        var result = LocalizationCatalog.Get("StatusBlocked", lang);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Get_UnknownKey_ReturnsFallbackOrKey()
    {
        var result = LocalizationCatalog.Get("NonExistentKey", "es", "FallbackVal");
        Assert.Equal("FallbackVal", result);
    }

    [Fact]
    public void HelpDocumentationKeys_ArePresentInSpanishAndEnglish()
    {
        var dictEs = LocalizationCatalog.GetAll("es");
        var dictEn = LocalizationCatalog.GetAll("en");
        var helpKeys = new[]
        {
            "HelpHowItWorksDesc", "HelpCameraDesc", "HelpMicDesc", "HelpAdvancedDesc",
            "HelpUnifiedDesc", "HelpFaq1Q", "HelpFaq1A",
            "HelpFaq2Q", "HelpFaq2A", "HelpFaq3Q", "HelpFaq3A", "HelpFaq4Q", "HelpFaq4A"
        };

        foreach (var key in helpKeys)
        {
            Assert.True(dictEs.TryGetValue(key, out var spanish), $"Missing Spanish help key: {key}");
            Assert.True(dictEn.TryGetValue(key, out var english), $"Missing English help key: {key}");
            Assert.False(string.IsNullOrWhiteSpace(spanish), $"Empty Spanish help value: {key}");
            Assert.False(string.IsNullOrWhiteSpace(english), $"Empty English help value: {key}");
        }
    }

    [Theory]
    [InlineData("es", "HelpFaq1A", "estado deseado")]
    [InlineData("en", "HelpFaq1A", "desired state")]
    [InlineData("es", "HelpFaq3A", "bandeja")]
    [InlineData("en", "HelpFaq3A", "system tray")]
    public void HelpDocumentation_DescribesRecoveryAndWindowLifecycle(string lang, string key, string expectedText)
    {
        var result = LocalizationCatalog.Get(key, lang);

        Assert.Contains(expectedText, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAll_ReturnsCompleteDictionary()
    {
        var dictEs = LocalizationCatalog.GetAll("es");
        var dictEn = LocalizationCatalog.GetAll("en");

        Assert.NotEmpty(dictEs);
        Assert.NotEmpty(dictEn);
        Assert.Equal(dictEs.Count, dictEn.Count);
    }

    [Theory]
    [InlineData("es", "BadgeUnprotected", "Sin protección")]
    [InlineData("en", "BadgeUnprotected", "Unprotected")]
    [InlineData("es", "BadgeProtected", "Protegido")]
    [InlineData("en", "BadgeProtected", "Protected")]
    [InlineData("es", "UserStandard", "Usuario estándar")]
    [InlineData("en", "UserStandard", "Standard User")]
    [InlineData("es", "UserAdmin", "Administrador")]
    [InlineData("en", "UserAdmin", "Administrator")]
    [InlineData("es", "DeviceTypeCamera", "Cámara")]
    [InlineData("en", "DeviceTypeCamera", "Camera")]
    [InlineData("es", "DeviceTypeMicrophone", "Micrófono")]
    [InlineData("en", "DeviceTypeMicrophone", "Microphone")]
    [InlineData("es", "DeviceEnabled", "Habilitado")]
    [InlineData("en", "DeviceEnabled", "Enabled")]
    [InlineData("es", "DeviceDisabled", "Deshabilitado")]
    [InlineData("en", "DeviceDisabled", "Disabled")]
    [InlineData("es", "TrayOpen", "Abrir PrivGvard")]
    [InlineData("en", "TrayOpen", "Open PrivGvard")]
    [InlineData("es", "TrayExit", "Salir")]
    [InlineData("en", "TrayExit", "Exit")]
    [InlineData("es", "Tray.Open", "Abrir PrivGvard")]
    [InlineData("en", "Tray.Open", "Open PrivGvard")]
    [InlineData("es", "Tray.Exit", "Salir")]
    [InlineData("en", "Tray.Exit", "Exit")]
    [InlineData("es", "Tray.ToolTip", "PrivGvard - Bloqueador de Cámara y Micrófono")]
    [InlineData("en", "Tray.ToolTip", "PrivGvard - Camera & Microphone Blocker")]
    public void Get_RequiredSemanticKeys_ReturnCorrectTranslation(string lang, string key, string expected)
    {
        var result = LocalizationCatalog.Get(key, lang);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("es", "TrayOpen", "Abrir PrivGvard")]
    [InlineData("en", "TrayOpen", "Open PrivGvard")]
    [InlineData("es", "Tray.Open", "Abrir PrivGvard")]
    [InlineData("en", "Tray.Open", "Open PrivGvard")]
    [InlineData("es", "TrayExit", "Salir")]
    [InlineData("en", "TrayExit", "Exit")]
    [InlineData("es", "Tray.Exit", "Salir")]
    [InlineData("en", "Tray.Exit", "Exit")]
    public void Get_TrayMenuKeys_ReturnCorrectTranslation(string lang, string key, string expected)
    {
        var result = LocalizationCatalog.Get(key, lang);
        Assert.Equal(expected, result);
        Assert.NotEqual("Tray.Open", result);
        Assert.NotEqual("Tray.Exit", result);
    }
}

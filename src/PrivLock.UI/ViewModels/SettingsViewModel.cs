using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrivLock.Application.Services;
using PrivLock.Domain.Models;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.UI.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private static readonly ILogger Log = Serilog.Log.ForContext<SettingsViewModel>();

    private readonly AutostartStateService _autostartStateService;
    private readonly LocalizationService _localizationService;
    private readonly ProtectionService _protectionService;
    private bool _disposed;
    private bool _initialized;

    [ObservableProperty]
    private SettingsSection _selectedSection = SettingsSection.General;

    public bool IsGeneralSelected => SelectedSection == SettingsSection.General;
    public bool IsHelpSelected => SelectedSection == SettingsSection.Help;
    public bool IsDiagnosticsSelected => SelectedSection == SettingsSection.Diagnostics;
    public bool IsAboutSelected => SelectedSection == SettingsSection.About;

    // --- General Section Properties ---
    [ObservableProperty]
    private bool? _isAutostartEnabled;

    [ObservableProperty]
    private bool _isAutostartStatusKnown;

    [ObservableProperty]
    private bool _isAutostartBusy;

    [ObservableProperty]
    private string _autostartStatusMessage = "";

    public bool HasAutostartStatusMessage => !string.IsNullOrWhiteSpace(AutostartStatusMessage);
    public bool IsAutostartToggleEnabled => IsAutostartStatusKnown && !IsAutostartBusy;
    public bool RequiresVisibleAutostartPolling => _autostartStateService.RequiresVisiblePolling;

    [ObservableProperty]
    private bool _isSpanishSelected;

    [ObservableProperty]
    private bool _isEnglishSelected;

    // --- Diagnostics Section Properties ---
    [ObservableProperty]
    private string _osName = "";

    [ObservableProperty]
    private string _architecture = "";

    [ObservableProperty]
    private string _privilegeLevel = "";

    [ObservableProperty]
    private string _cameraStatus = "";

    [ObservableProperty]
    private string _microphoneStatus = "";

    [ObservableProperty]
    private string _advancedStatus = "";

    [ObservableProperty]
    private string _capabilitiesSummary = "";

    [ObservableProperty]
    private int _devicesCount;

    [ObservableProperty]
    private string _devicesCountSummary = "";

    public ObservableCollection<string> DetectedDevicesList { get; } = [];

    // --- About Section Properties ---
    [ObservableProperty]
    private string _appName = "PrivGvard";

    [ObservableProperty]
    private string _appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "2.0.0";

    // --- Localized Titles & Labels ---
    public string WindowTitle => _localizationService.GetString("SettingsWindowTitle", "Configuración");
    public string HeaderTitle => _localizationService.GetString("SettingsHeaderTitle", "Configuración");
    public string NavGeneral => _localizationService.GetString("SettingsNavGeneral", "Ajustes");
    public string NavHelp => _localizationService.GetString("SettingsNavHelp", "Ayuda y Documentación");
    public string NavDiagnostics => _localizationService.GetString("SettingsNavDiagnostics", "Diagnóstico del Sistema");
    public string NavAbout => _localizationService.GetString("SettingsNavAbout", "Acerca de PrivGvard");

    // General
    public string AppearanceTitle => _localizationService.GetString("AppearanceTitle", "Apariencia");
    public string ThemeLabel => _localizationService.GetString("ThemeLabel", "Tema de la interfaz");
    public string ThemeDark => _localizationService.GetString("ThemeDark", "Oscuro (Predeterminado)");
    public string ThemeDescription => _localizationService.GetString("ThemeDescription", "El tema oscuro de alto contraste está optimizado para la interfaz de PrivGvard.");
    public string StartupTitle => _localizationService.GetString("StartupTitle", "Inicio del Sistema");
    public string StartupDescription => _localizationService.GetString("StartupDescription", "Iniciar PrivGvard automáticamente al iniciar sesión en el equipo.");
    public string StartWithSystemLabel => _localizationService.GetString("StartWithSystem", "Iniciar con el sistema");
    public string LanguageTitle => _localizationService.GetString("LanguageTitle", "Idioma de la interfaz");
    public string LanguageDescription => _localizationService.GetString("LanguageDescription", "Selecciona el idioma principal de la aplicación.");

    // Help
    public string HelpTitle => _localizationService.GetString("HelpTitle", "Ayuda y Documentación");
    public string HelpSubtitle => _localizationService.GetString("HelpSubtitle", "Guía de referencia rápida y funcionamiento de PrivGvard");
    public string HelpHowItWorksTitle => _localizationService.GetString("HelpHowItWorksTitle", "Cómo funciona PrivGvard");
    public string HelpHowItWorksDesc => _localizationService.GetString("HelpHowItWorksDesc", "En Windows, PrivGvard combina controles de privacidad del sistema con administración de dispositivos. El resultado depende del modo elegido, el hardware, el controlador, los permisos y la configuración; la aplicación verifica el estado efectivo cuando el sistema lo permite.");
    public string HelpCameraTitle => _localizationService.GetString("HelpCameraTitle", "Protección de Cámara");
    public string HelpCameraDesc => _localizationService.GetString("HelpCameraDesc", "Según el modo y el dispositivo, puede aplicar la directiva de privacidad de cámara de Windows y administrar nodos de dispositivo compatibles. Esto no desconecta físicamente la cámara y no todos los modelos o controladores admiten cada medida.");
    public string HelpMicTitle => _localizationService.GetString("HelpMicTitle", "Protección de Micrófono");
    public string HelpMicDesc => _localizationService.GetString("HelpMicDesc", "Según el modo y el dispositivo, puede aplicar la directiva de privacidad de micrófono, administrar dispositivos compatibles y silenciar puntos de conexión de captura de audio. Silenciar un punto de conexión no equivale a desconectarlo físicamente.");
    public string HelpAdvancedTitle => _localizationService.GetString("HelpAdvancedTitle", "Protección Avanzada");
    public string HelpAdvancedDesc => _localizationService.GetString("HelpAdvancedDesc", "La protección avanzada puede combinar directivas de privacidad de Windows y administración de dispositivos PnP (Plug and Play). La compatibilidad varía según el dispositivo y el controlador, y algunas operaciones requieren permisos de administrador.");
    public string HelpUnifiedTitle => _localizationService.GetString("HelpUnifiedTitle", "Control Unificado");
    public string HelpUnifiedDesc => _localizationService.GetString("HelpUnifiedDesc", "Solicita el cambio para cámara y micrófono con un solo control. Cada uno se procesa y verifica por separado, por lo que pueden quedar con resultados distintos si una operación falla o un dispositivo no es compatible.");
    public string HelpFaqTitle => _localizationService.GetString("HelpFaqTitle", "Preguntas Frecuentes");
    public string HelpFaq1Q => _localizationService.GetString("HelpFaq1Q", "¿Se mantienen los bloqueos tras reiniciar el equipo?");
    public string HelpFaq1A => _localizationService.GetString("HelpFaq1A", "Al salir normalmente, la aplicación intenta restaurar los cambios que administra. Al iniciar, intenta recuperar operaciones interrumpidas y reconciliar el sistema con el estado deseado guardado. El diario no garantiza protección ininterrumpida durante el reinicio; comprueba el estado después de iniciar sesión.");
    public string HelpFaq2Q => _localizationService.GetString("HelpFaq2Q", "¿Por qué aparece UAC y qué pasa si cancelo?");
    public string HelpFaq2A => _localizationService.GetString("HelpFaq2A", "Algunas operaciones de Windows requieren permisos de administrador y solicitan autorización en ese momento. Si cancelas el aviso, esa operación falla y no debe considerarse protección confirmada; revisa el estado y el mensaje de error.");
    public string HelpFaq3Q => _localizationService.GetString("HelpFaq3Q", "¿Cerrar la ventana es lo mismo que elegir «Salir»?");
    public string HelpFaq3A => _localizationService.GetString("HelpFaq3A", "Con la bandeja disponible, cerrar la ventana o usar Alt+F4 la oculta y la aplicación sigue en ejecución. «Salir» desde la bandeja inicia el cierre e intenta restaurar los cambios que PrivGvard administra. Si la bandeja no está disponible, cerrar la ventana inicia el cierre coordinado.");
    public string HelpFaq4Q => _localizationService.GetString("HelpFaq4Q", "¿Qué hago si un dispositivo no es compatible o falla?");
    public string HelpFaq4A => _localizationService.GetString("HelpFaq4A", "Actualiza el diagnóstico y revisa el estado efectivo, los permisos y el controlador; consulta el mensaje de error. Un estado deseado guardado no confirma que la protección se haya aplicado. Si la compatibilidad figura como no disponible o el estado no se verifica, no lo consideres protegido.");

    // Diagnostics
    public string DiagTitle => _localizationService.GetString("DiagTitle", "Diagnóstico del Sistema");
    public string DiagSubtitle => _localizationService.GetString("DiagSubtitle", "Telemetría y estado operativo de privacidad");
    public string DiagOsLabel => _localizationService.GetString("DiagOsLabel", "Sistema Operativo");
    public string DiagArchLabel => _localizationService.GetString("DiagArchLabel", "Arquitectura");
    public string DiagPrivilegesLabel => _localizationService.GetString("DiagPrivilegesLabel", "Nivel de Privilegios");
    public string DiagCamStatusLabel => _localizationService.GetString("DiagCamStatusLabel", "Estado de Cámara");
    public string DiagMicStatusLabel => _localizationService.GetString("DiagMicStatusLabel", "Estado de Micrófono");
    public string DiagAdvancedLabel => _localizationService.GetString("DiagAdvancedLabel", "Protección Avanzada");
    public string DiagDevicesCountLabel => _localizationService.GetString("DiagDevicesCountLabel", "Dispositivos Detectados");
    public string DetectedDevicesLabel => _localizationService.GetString("DetectedDevices", "Dispositivos Detectados");
    public string DiagCapabilitiesLabel => _localizationService.GetString("DiagCapabilitiesLabel", "Nivel de Capacidades");
    public string DiagRefreshButton => _localizationService.GetString("DiagRefreshButton", "Actualizar Diagnóstico");

    // About
    public string AboutTitle => _localizationService.GetString("AboutTitle", "Acerca de PrivGvard");
    public string AboutVersionLabel => _localizationService.GetString("AboutVersionLabel", "Versión");
    public string AboutDeveloperLabel => _localizationService.GetString("AboutDeveloperLabel", "Desarrollador");
    public string AboutDeveloperValue => _localizationService.GetString("AboutDeveloperValue", "cdev Studio");
    public string AboutDescription => _localizationService.GetString("AboutDescription", "Solución de privacidad y control de hardware de cámara y micrófono para Windows.");
    public string AboutLicenseLabel => _localizationService.GetString("AboutLicenseLabel", "Licencia");
    public string AboutLicenseValue => _localizationService.GetString("AboutLicenseValue", "Licencia MIT");
    public string AboutCopyright => _localizationService.GetString("AboutCopyright", "© 2026 cdev Studio. Todos los derechos reservados.");

    public SettingsViewModel(
        AutostartStateService autostartStateService,
        LocalizationService localizationService,
        ProtectionService protectionService)
    {
        _autostartStateService = autostartStateService;
        _localizationService = localizationService;
        _protectionService = protectionService;

        ApplyAutostartStatus(_autostartStateService.Status);

        _isSpanishSelected = _localizationService.CurrentLanguage == "es";
        _isEnglishSelected = !_isSpanishSelected;

        var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var ver = entryAssembly.GetName().Version;
        AppVersion = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
    }

    /// <summary>
    /// Starts UI-bound observation only after Avalonia has initialized its platform dispatcher.
    /// </summary>
    public void Initialize()
    {
        if (_initialized || _disposed)
            return;

        _initialized = true;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _protectionService.StateChanged += OnProtectionStateChanged;
        _autostartStateService.StatusChanged += OnAutostartStatusChanged;

        IsSpanishSelected = _localizationService.CurrentLanguage == "es";
        IsEnglishSelected = !IsSpanishSelected;
        ApplyAutostartStatus(_autostartStateService.Status);
        _ = RefreshDiagnosticsAsync();
    }

    partial void OnSelectedSectionChanged(SettingsSection value)
    {
        OnPropertyChanged(nameof(IsGeneralSelected));
        OnPropertyChanged(nameof(IsHelpSelected));
        OnPropertyChanged(nameof(IsDiagnosticsSelected));
        OnPropertyChanged(nameof(IsAboutSelected));
    }

    [RelayCommand]
    public void SelectSection(string sectionName)
    {
        if (Enum.TryParse<SettingsSection>(sectionName, true, out var section))
        {
            SelectedSection = section;
        }
    }

    [RelayCommand]
    private void SelectLanguage(string lang)
    {
        var result = _localizationService.SetLanguage(lang);
        IsSpanishSelected = lang == "es";
        IsEnglishSelected = lang == "en";
        if (!result.Success)
        {
            Log.Warning("Failed to save language preference from settings: {Error}", result.ErrorMessage);
        }
    }

    [RelayCommand]
    private async Task ToggleAutostart(bool? enable)
    {
        if (enable is not bool requestedState || IsAutostartBusy)
            return;

        IsAutostartBusy = true;
        try
        {
            var result = await _autostartStateService.SetAutostartAsync(requestedState);
            // Reconcile the optimistic checkbox change even when Windows reports the same
            // effective state as before (for example, StartupApproved is DisabledByUser).
            OnAutostartStatusChanged(_autostartStateService.Status);
            if (!result.Success)
            {
                Log.Warning("Failed to update autostart from settings: {Error}", result.ErrorMessage);
                if (string.IsNullOrWhiteSpace(AutostartStatusMessage))
                {
                    AutostartStatusMessage = result.ErrorMessage ??
                        _localizationService.GetString("ErrorAutostart", "Failed to update autostart setting");
                }
            }
        }
        finally
        {
            IsAutostartBusy = false;
        }
    }

    public async Task RefreshAutostartStatusAsync()
    {
        await _autostartStateService.RefreshAsync();
    }

    private void OnAutostartStatusChanged(AutostartStatus status)
    {
        if (_disposed)
            return;

        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            ApplyAutostartStatus(status);
        else
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (!_disposed)
                    ApplyAutostartStatus(status);
            });
    }

    private void ApplyAutostartStatus(AutostartStatus status)
    {
        IsAutostartStatusKnown = status.State is AutostartState.Enabled
            or AutostartState.Disabled
            or AutostartState.DisabledByUser
            or AutostartState.DisabledByPolicy;
        IsAutostartEnabled = IsAutostartStatusKnown ? status.IsEnabled : null;
        AutostartStatusMessage = GetAutostartStatusMessage(status);
    }

    private string GetAutostartStatusMessage(AutostartStatus status) =>
        status.State switch
        {
            AutostartState.Enabled or AutostartState.Disabled => "",
            AutostartState.DisabledByUser => _localizationService.GetString(
                "AutostartDisabledByUser",
                "Windows has disabled startup for PrivGvard. Enable it in Windows Settings or Task Manager."),
            AutostartState.DisabledByPolicy => _localizationService.GetString(
                "AutostartDisabledByPolicy",
                "Windows policy prevents PrivGvard from starting automatically."),
            _ => string.IsNullOrWhiteSpace(status.Message)
                ? _localizationService.GetString(
                    "AutostartStateUnknown",
                    "The effective Windows startup state could not be verified.")
                : $"{_localizationService.GetString(
                    "AutostartStateUnknown",
                    "The effective Windows startup state could not be verified.")} {status.Message}"
        };

    partial void OnAutostartStatusMessageChanged(string value) =>
        OnPropertyChanged(nameof(HasAutostartStatusMessage));

    partial void OnIsAutostartStatusKnownChanged(bool value) =>
        OnPropertyChanged(nameof(IsAutostartToggleEnabled));

    partial void OnIsAutostartBusyChanged(bool value) =>
        OnPropertyChanged(nameof(IsAutostartToggleEnabled));

    [RelayCommand]
    public async Task RefreshDiagnosticsAsync()
    {
        try
        {
            var info = _protectionService.PlatformInfo;
            OsName = info.OsVersion.Contains(info.OperatingSystemName, StringComparison.OrdinalIgnoreCase)
                ? info.OsVersion
                : $"{info.OperatingSystemName} {info.OsVersion}";
            Architecture = $"{info.Architecture} ({(info.Is64Bit ? "64-bit" : "32-bit")})";
            PrivilegeLevel = info.IsElevated
                ? _localizationService.GetString("DiagPrivilegeAdmin", "Administrador (Elevado)")
                : _localizationService.GetString("DiagPrivilegeStandard", "Usuario estándar (asInvoker)");

            var caps = _protectionService.Capabilities;
            var camPrefix = _localizationService.GetString("SummaryCameraLabel", "Cámara");
            var micPrefix = _localizationService.GetString("SummaryMicrophoneLabel", "Micrófono");
            var hwYesNo = caps.SupportsHardwareDisable
                ? _localizationService.GetString("Yes", "Sí")
                : _localizationService.GetString("No", "No");
            CapabilitiesSummary = $"{camPrefix}: {caps.CameraProtectionLevel} | {micPrefix}: {caps.MicrophoneProtectionLevel} | Hardware: {hwYesNo}";

            var state = await _protectionService.GetCurrentStateAsync();
            CameraStatus = state.Camera.IsProtected
                ? (state.Camera.SecureState == SecureProtectionState.Active
                    ? _localizationService.GetString("StatusBlockedAdvanced", "🛡️ Bloqueado (Avanzado)")
                    : _localizationService.GetString("StatusBlockedStandard", "🔒 Bloqueado (Estándar)"))
                : _localizationService.GetString("StatusUnblocked", "✅ Permitido");
            MicrophoneStatus = state.Microphone.IsProtected
                ? (state.Microphone.SecureState == SecureProtectionState.Active
                    ? _localizationService.GetString("StatusBlockedAdvanced", "🛡️ Bloqueado (Avanzado)")
                    : _localizationService.GetString("StatusBlockedStandard", "🔒 Bloqueado (Estándar)"))
                : _localizationService.GetString("StatusUnblocked", "✅ Permitido");
            AdvancedStatus = state.AdvancedProtectionEnabled
                ? _localizationService.GetString("StatusActive", "🛡️ Activa")
                : _localizationService.GetString("StatusInactive", "○ Inactiva");

            var devices = await _protectionService.GetDetectedDevicesAsync();
            DevicesCount = devices.Count;
            DevicesCountSummary = string.Format(
                _localizationService.GetString("DevicesCountFormat", "{0} dispositivos detectados en total"),
                devices.Count);

            DetectedDevicesList.Clear();
            var enabledStatus = _localizationService.GetString("DeviceEnabled", "Habilitado");
            var disabledStatus = _localizationService.GetString("DeviceDisabled", "Deshabilitado");
            foreach (var d in devices)
            {
                var typeIcon = d.DeviceType == DeviceType.Camera ? "📷" : "🎙️";
                var statusStr = d.IsEnabled ? enabledStatus : disabledStatus;
                DetectedDevicesList.Add($"{typeIcon} {d.FriendlyName} [{statusStr}]");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh system diagnostics in SettingsViewModel");
        }
    }

    private void OnProtectionStateChanged(FullProtectionState state)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => _ = RefreshDiagnosticsAsync());
    }

    private void OnLanguageChanged(string lang)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            IsSpanishSelected = lang == "es";
            IsEnglishSelected = lang == "en";

            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(NavGeneral));
            OnPropertyChanged(nameof(NavHelp));
            OnPropertyChanged(nameof(NavDiagnostics));
            OnPropertyChanged(nameof(NavAbout));

            OnPropertyChanged(nameof(AppearanceTitle));
            OnPropertyChanged(nameof(ThemeLabel));
            OnPropertyChanged(nameof(ThemeDark));
            OnPropertyChanged(nameof(ThemeDescription));
            OnPropertyChanged(nameof(StartupTitle));
            OnPropertyChanged(nameof(StartupDescription));
            OnPropertyChanged(nameof(StartWithSystemLabel));
            OnPropertyChanged(nameof(AutostartStatusMessage));
            ApplyAutostartStatus(_autostartStateService.Status);
            OnPropertyChanged(nameof(LanguageTitle));
            OnPropertyChanged(nameof(LanguageDescription));

            OnPropertyChanged(nameof(HelpTitle));
            OnPropertyChanged(nameof(HelpSubtitle));
            OnPropertyChanged(nameof(HelpHowItWorksTitle));
            OnPropertyChanged(nameof(HelpHowItWorksDesc));
            OnPropertyChanged(nameof(HelpCameraTitle));
            OnPropertyChanged(nameof(HelpCameraDesc));
            OnPropertyChanged(nameof(HelpMicTitle));
            OnPropertyChanged(nameof(HelpMicDesc));
            OnPropertyChanged(nameof(HelpAdvancedTitle));
            OnPropertyChanged(nameof(HelpAdvancedDesc));
            OnPropertyChanged(nameof(HelpUnifiedTitle));
            OnPropertyChanged(nameof(HelpUnifiedDesc));
            OnPropertyChanged(nameof(HelpFaqTitle));
            OnPropertyChanged(nameof(HelpFaq1Q));
            OnPropertyChanged(nameof(HelpFaq1A));
            OnPropertyChanged(nameof(HelpFaq2Q));
            OnPropertyChanged(nameof(HelpFaq2A));
            OnPropertyChanged(nameof(HelpFaq3Q));
            OnPropertyChanged(nameof(HelpFaq3A));
            OnPropertyChanged(nameof(HelpFaq4Q));
            OnPropertyChanged(nameof(HelpFaq4A));

            OnPropertyChanged(nameof(DiagTitle));
            OnPropertyChanged(nameof(DiagSubtitle));
            OnPropertyChanged(nameof(DiagOsLabel));
            OnPropertyChanged(nameof(DiagArchLabel));
            OnPropertyChanged(nameof(DiagPrivilegesLabel));
            OnPropertyChanged(nameof(DiagCamStatusLabel));
            OnPropertyChanged(nameof(DiagMicStatusLabel));
            OnPropertyChanged(nameof(DiagAdvancedLabel));
            OnPropertyChanged(nameof(DiagDevicesCountLabel));
            OnPropertyChanged(nameof(DetectedDevicesLabel));
            OnPropertyChanged(nameof(DiagCapabilitiesLabel));
            OnPropertyChanged(nameof(DiagRefreshButton));

            OnPropertyChanged(nameof(AboutTitle));
            OnPropertyChanged(nameof(AboutVersionLabel));
            OnPropertyChanged(nameof(AboutDeveloperLabel));
            OnPropertyChanged(nameof(AboutDeveloperValue));
            OnPropertyChanged(nameof(AboutDescription));
            OnPropertyChanged(nameof(AboutLicenseLabel));
            OnPropertyChanged(nameof(AboutLicenseValue));
            OnPropertyChanged(nameof(AboutCopyright));

            _ = RefreshDiagnosticsAsync();
        });
    }

    public void Dispose()
    {
        _disposed = true;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _protectionService.StateChanged -= OnProtectionStateChanged;
        _autostartStateService.StatusChanged -= OnAutostartStatusChanged;
    }
}

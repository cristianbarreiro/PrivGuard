using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrivLock.Application.Services;
using PrivLock.Domain.Models;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.UI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MainViewModel>();

    private readonly ProtectionService _protectionService;
    private readonly AutostartStateService _autostartStateService;
    private readonly LocalizationService _localizationService;

    private bool _isUpdating;
    private bool _disposed;
    private bool _initialized;

    [ObservableProperty]
    private string _appTitle = "PrivGvard";

    [ObservableProperty]
    private string _appSubtitle = "Camera & Microphone Blocker";

    // --- Edition & Capability Profile ---
    public EditionProfile EditionProfile => _protectionService.EditionProfile;
    public bool IsStoreEdition => _protectionService.IsStoreEdition;
    public bool IsStandaloneEdition => _protectionService.EditionProfile == EditionProfile.Standalone;
    public bool IsSecureProtectionSupported =>
        _protectionService.EditionProfile == EditionProfile.Standalone &&
        _protectionService.Capabilities.SupportsHardwareDisable &&
        _protectionService.Capabilities.SupportsSystemPolicy;

    // --- Advanced Protection (Global Extension) ---
    [ObservableProperty]
    private bool _isAdvancedProtectionEnabled;

    [ObservableProperty]
    private bool _isAdvancedProtectionToggleEnabled = true;

    [ObservableProperty]
    private string _advancedProtectionStatusText = "";

    [ObservableProperty]
    private string _advancedProtectionStatusColor = "#888888";

    // --- Camera States ---
    [ObservableProperty]
    private bool _isCameraBlocked;

    [ObservableProperty]
    private string _cameraStatusText = "";

    [ObservableProperty]
    private string _cameraStatusColor = "#81C784";

    [ObservableProperty]
    private bool _isCameraStandardActive;

    [ObservableProperty]
    private string _cameraStandardText = "";

    [ObservableProperty]
    private string _cameraStandardColor = "#81C784";

    [ObservableProperty]
    private bool _isCameraToggleEnabled = true;

    [ObservableProperty]
    private string _cameraSecureText = "";

    [ObservableProperty]
    private string _cameraSecureBadgeColor = "#555555";

    [ObservableProperty]
    private string _cameraSecureButtonText = "";

    [ObservableProperty]
    private bool _isCameraSecureButtonEnabled;

    [ObservableProperty]
    private string _cameraSecureHint = "";

    [ObservableProperty]
    private bool _isCameraSecureHintVisible;

    // --- Microphone States ---
    [ObservableProperty]
    private bool _isMicBlocked;

    [ObservableProperty]
    private string _micStatusText = "";

    [ObservableProperty]
    private string _micStatusColor = "#81C784";

    [ObservableProperty]
    private bool _isMicStandardActive;

    [ObservableProperty]
    private string _micStandardText = "";

    [ObservableProperty]
    private string _micStandardColor = "#81C784";

    [ObservableProperty]
    private string _micSecureText = "";

    [ObservableProperty]
    private string _micSecureBadgeColor = "#555555";

    [ObservableProperty]
    private bool _isMicToggleEnabled = true;

    [ObservableProperty]
    private string _micHardwareCapabilityText = "";

    [ObservableProperty]
    private bool _hasMicHardwareCapability;

    [ObservableProperty]
    private string _micSecureButtonText = "";

    [ObservableProperty]
    private bool _isMicSecureButtonEnabled;

    [ObservableProperty]
    private string _micSecureHint = "";

    [ObservableProperty]
    private bool _isMicSecureHintVisible;

    // --- Unified (Both) States ---
    [ObservableProperty]
    private bool _isBothBlocked;

    [ObservableProperty]
    private string _bothStatusText = "";

    [ObservableProperty]
    private string _bothStatusColor = "#81C784";

    [ObservableProperty]
    private bool _isBothStandardActive;

    [ObservableProperty]
    private string _bothStandardText = "";

    [ObservableProperty]
    private string _bothStandardColor = "#81C784";

    [ObservableProperty]
    private bool _isBothToggleEnabled = true;

    [ObservableProperty]
    private string _bothSecureText = "";

    [ObservableProperty]
    private string _bothSecureBadgeColor = "#555555";

    [ObservableProperty]
    private string _bothSecureButtonText = "";

    [ObservableProperty]
    private bool _isBothSecureButtonEnabled;

    [ObservableProperty]
    private string _bothSecureHint = "";

    [ObservableProperty]
    private bool _isBothSecureHintVisible;

    // --- Common & Settings ---
    [ObservableProperty]
    private bool _isSpanishSelected = true;

    [ObservableProperty]
    private bool _isEnglishSelected;

    [ObservableProperty]
    private bool? _isAutostartEnabled;

    [ObservableProperty]
    private string _autostartStatusMessage = "";

    [ObservableProperty]
    private string _platformName = "";

    [ObservableProperty]
    private string _capabilitiesSummary = "";

    [ObservableProperty]
    private string _securityBadgeText = "Standard";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSupervisionOnly;

    [ObservableProperty]
    private string _supervisionMessage = "";

    [ObservableProperty]
    private bool _isMicActiveStoreWarningVisible;

    [ObservableProperty]
    private string _micActiveStoreWarningMessage = "";

    [ObservableProperty]
    private string _openCameraSettingsButtonText = "Settings";

    public ObservableCollection<DeviceItemViewModel> Devices { get; } = [];

    // Localized Labels
    public string CameraTitle => _localizationService.GetString("CameraTitle");
    public string CameraSubtitle => _localizationService.GetString("CameraSubtitle");
    public string MicrophoneTitle => _localizationService.GetString("MicrophoneTitle");
    public string MicrophoneSubtitle => _localizationService.GetString("MicrophoneSubtitle");
    public string StandardProtectionTitle => _localizationService.GetString("StandardProtectionTitle");
    public string StandardProtectionDesc => _localizationService.GetString("StandardProtectionDesc");
    public string SecureProtectionTitle => _localizationService.GetString("SecureProtectionTitle");
    public string SecureProtectionDesc => _localizationService.GetString("SecureProtectionDesc");
    public string AdvancedProtectionTitle => _localizationService.GetString("AdvancedProtectionTitle");
    public string AdvancedProtectionSubtitle => _localizationService.GetString("AdvancedProtectionSubtitle");
    public string AdvancedProtectionDesc => _localizationService.GetString("AdvancedProtectionDesc");
    public string DetectedDevicesLabel => _localizationService.GetString("DetectedDevices");
    public string StartWithSystemLabel => _localizationService.GetString("StartWithSystem");
    public string LanguageLabel => _localizationService.GetString("Language");
    public string CapabilitiesTitle => _localizationService.GetString("CapabilitiesTitle");
    public string UnifiedTitle => _localizationService.GetString("UnifiedTitle");
    public string UnifiedSubtitle => _localizationService.GetString("UnifiedSubtitle");
    public string UnifiedStandardDesc => _localizationService.GetString("UnifiedStandardDesc");
    public string UnifiedSecureDesc => _localizationService.GetString("UnifiedSecureDesc");

    // Main Menu Flyout
    public string MenuToolTip => _localizationService.GetString("MenuToolTip", "Menú y Ajustes");
    public string MenuGeneral => _localizationService.GetString("MenuGeneral", "⚙️  Ajustes");
    public string MenuHelp => _localizationService.GetString("MenuHelp", "📖  Ayuda y Documentación");
    public string MenuDiagnostics => _localizationService.GetString("MenuDiagnostics", "🛡️  Diagnóstico del Sistema");
    public string MenuAbout => _localizationService.GetString("MenuAbout", "ℹ️  Acerca de PrivGvard");

    public MainViewModel(
        ProtectionService protectionService,
        AutostartStateService autostartStateService,
        LocalizationService localizationService)
    {
        _protectionService = protectionService;
        _autostartStateService = autostartStateService;
        _localizationService = localizationService;

        _isSpanishSelected = _localizationService.CurrentLanguage == "es";
        _isEnglishSelected = !_isSpanishSelected;
        ApplyAutostartStatus(_autostartStateService.Status);

        UpdatePlatformAndCapabilities();
    }

    /// <summary>
    /// Starts UI-bound observation only after Avalonia has initialized its platform dispatcher.
    /// </summary>
    public void Initialize()
    {
        if (_initialized || _disposed)
            return;

        _initialized = true;
        _protectionService.StateChanged += OnProtectionStateChanged;
        _autostartStateService.StatusChanged += OnAutostartStatusChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;

        IsSpanishSelected = _localizationService.CurrentLanguage == "es";
        IsEnglishSelected = !IsSpanishSelected;
        ApplyAutostartStatus(_autostartStateService.Status);
        _ = RefreshStateAsync();
    }

    private void UpdatePlatformAndCapabilities()
    {
        var info = _protectionService.PlatformInfo;
        var userRole = info.IsElevated
            ? _localizationService.GetString("UserAdmin", "Administrador")
            : _localizationService.GetString("UserStandard", "Usuario estándar");
        PlatformName = $"{info.OperatingSystemName} ({info.Architecture}) - {userRole}";

        var camLabel = _localizationService.GetString("SummaryCameraLabel", "Cámara");
        var micLabel = _localizationService.GetString("SummaryMicrophoneLabel", "Micrófono");
        CapabilitiesSummary = $"{camLabel}: {_protectionService.Capabilities.CameraProtectionLevel} | {micLabel}: {_protectionService.Capabilities.MicrophoneProtectionLevel}";

        AppSubtitle = _localizationService.GetString("AppSubtitle", "Bloqueador de Cámara y Micrófono");
    }

    public async Task RefreshStateAsync()
    {
        try
        {
            var state = await _protectionService.GetCurrentStateAsync();
            var devices = await _protectionService.GetDetectedDevicesAsync();
            UpdateUi(state, devices);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh UI state");
        }
    }

    private void OnProtectionStateChanged(FullProtectionState state)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var devices = await _protectionService.GetDetectedDevicesAsync();
                UpdateUi(state, devices);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to refresh detected devices after a protection-state change");
                ShowError($"Could not verify the current device state: {ex.Message}");
            }
        });
    }

    private void OnLanguageChanged(string lang)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            IsSpanishSelected = lang == "es";
            IsEnglishSelected = lang == "en";

            OnPropertyChanged(nameof(CameraTitle));
            OnPropertyChanged(nameof(CameraSubtitle));
            OnPropertyChanged(nameof(MicrophoneTitle));
            OnPropertyChanged(nameof(MicrophoneSubtitle));
            OnPropertyChanged(nameof(StandardProtectionTitle));
            OnPropertyChanged(nameof(StandardProtectionDesc));
            OnPropertyChanged(nameof(SecureProtectionTitle));
            OnPropertyChanged(nameof(SecureProtectionDesc));
            OnPropertyChanged(nameof(AdvancedProtectionTitle));
            OnPropertyChanged(nameof(AdvancedProtectionSubtitle));
            OnPropertyChanged(nameof(AdvancedProtectionDesc));
            OnPropertyChanged(nameof(DetectedDevicesLabel));
            OnPropertyChanged(nameof(StartWithSystemLabel));
            OnPropertyChanged(nameof(LanguageLabel));
            OnPropertyChanged(nameof(CapabilitiesTitle));
            OnPropertyChanged(nameof(UnifiedTitle));
            OnPropertyChanged(nameof(UnifiedSubtitle));
            OnPropertyChanged(nameof(UnifiedStandardDesc));
            OnPropertyChanged(nameof(UnifiedSecureDesc));

            OnPropertyChanged(nameof(MenuToolTip));
            OnPropertyChanged(nameof(MenuGeneral));
            OnPropertyChanged(nameof(MenuHelp));
            OnPropertyChanged(nameof(MenuDiagnostics));
            OnPropertyChanged(nameof(MenuAbout));
            UpdatePlatformAndCapabilities();
            _ = RefreshStateAsync();
        });
    }

    internal void UpdateUi(FullProtectionState state, IReadOnlyList<DeviceInfo> devices)
    {
        _isUpdating = true;
        try
        {
            var isStore = _protectionService.IsStoreEdition;
            var isUntrusted = _protectionService.IsUntrustedOrIndeterminate;

            // === 1. Advanced Protection (Global Extension) ===
            IsAdvancedProtectionToggleEnabled = IsSecureProtectionSupported && !isUntrusted;
            IsAdvancedProtectionEnabled = !isStore && !isUntrusted && state.AdvancedProtectionEnabled;
            AdvancedProtectionStatusText = isStore
                ? _localizationService.GetString("StoreEditionSecureNotSupported", "Unavailable in Microsoft Store edition")
                : (IsAdvancedProtectionEnabled
                    ? _localizationService.GetString("StatusAdvancedActive", "🛡️ Hardened (Advanced)")
                    : _localizationService.GetString("StatusAdvancedInactive", "○ Inactive"));
            AdvancedProtectionStatusColor = IsAdvancedProtectionEnabled ? "#007ACC" : "#888888";

            // === 2. Camera UI ===
            var camBlocked = state.Camera.IsProtected;
            var camAbsent = state.Camera.Availability == TargetDeviceAvailability.NotDetected;
            var camStandardUnverifiable = state.Camera.StandardState == StandardProtectionState.Unknown || !state.Camera.StandardIsVerified;
            var camSecureUnverifiable = state.Camera.SecureState == SecureProtectionState.Unknown || !state.Camera.SecureIsVerified;
            var camAllowedVerified = !camBlocked && !camAbsent && !camStandardUnverifiable && !camSecureUnverifiable &&
                                     state.Camera.StandardState == StandardProtectionState.Inactive;

            IsCameraBlocked = camBlocked;
            IsCameraStandardActive = camBlocked;

            if (isUntrusted)
            {
                IsCameraToggleEnabled = false;
            }
            else if (camBlocked)
            {
                // Safe unblock/restoration: user can restore if Standard or Secure is Active and verified
                IsCameraToggleEnabled = (state.Camera.StandardState == StandardProtectionState.Active && state.Camera.StandardIsVerified) ||
                                        (state.Camera.SecureState == SecureProtectionState.Active && state.Camera.SecureIsVerified);
            }
            else
            {
                // Safe mutation: user can enable Standard if camera is present and Standard is Inactive and verifiable
                IsCameraToggleEnabled = !camAbsent && !camStandardUnverifiable &&
                                        state.Camera.StandardState == StandardProtectionState.Inactive;
            }

            if (camBlocked)
            {
                if (state.Camera.SecureState == SecureProtectionState.Active && state.Camera.SecureIsVerified)
                {
                    CameraStatusText = _localizationService.GetString("StatusBlockedAdvanced", "🛡️ Blocked (Advanced)");
                    CameraStatusColor = "#D32F2F";
                    CameraStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    CameraStandardColor = "#E57373";
                }
                else if (state.Camera.StandardState == StandardProtectionState.Active && state.Camera.StandardIsVerified)
                {
                    if (camSecureUnverifiable)
                    {
                        CameraStatusText = _localizationService.GetString("StatusPartiallyUnverifiable", "⚠️ Parcialmente protegido / No verificable");
                        CameraStatusColor = "#FFA726";
                    }
                    else
                    {
                        CameraStatusText = _localizationService.GetString("StatusBlockedStandard", "🔒 Blocked (Standard)");
                        CameraStatusColor = "#E57373";
                    }
                    CameraStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    CameraStandardColor = "#E57373";
                }
                else
                {
                    CameraStatusText = _localizationService.GetString("StatusPartiallyUnverifiable", "⚠️ Parcialmente protegido / No verificable");
                    CameraStatusColor = "#FFA726";
                    CameraStandardText = _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable");
                    CameraStandardColor = "#FFA726";
                }
            }
            else if (camAbsent)
            {
                CameraStatusText = _localizationService.GetString("NoCameraDetected", "No camera detected");
                CameraStatusColor = "#9E9E9E";
                CameraStandardText = _localizationService.GetString("NoCameraDetected", "No camera detected");
                CameraStandardColor = "#9E9E9E";
            }
            else if (camStandardUnverifiable || camSecureUnverifiable)
            {
                CameraStatusText = _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                CameraStatusColor = "#FFA726";
                CameraStandardText = camStandardUnverifiable
                    ? _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable")
                    : _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                CameraStandardColor = camStandardUnverifiable ? "#FFA726" : "#81C784";
            }
            else
            {
                CameraStatusText = _localizationService.GetString("StatusUnblocked", "✅ Allowed");
                CameraStatusColor = "#81C784";
                CameraStandardText = _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                CameraStandardColor = "#81C784";
            }

            // Camera Secure Compatibility fields
            if (isStore || isUntrusted)
            {
                CameraSecureText = isStore
                    ? _localizationService.GetString("StoreEditionSecureNotSupported", "Unavailable in Microsoft Store edition")
                    : _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                CameraSecureBadgeColor = "#555555";
                CameraSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                IsCameraSecureButtonEnabled = false;
                IsCameraSecureHintVisible = isStore;
                CameraSecureHint = isStore
                    ? _localizationService.GetString("StoreEditionCameraSettingsHint", "In the Microsoft Store edition, manage camera permissions in Windows Settings")
                    : "";
            }
            else
            {
                switch (state.Camera.SecureState)
                {
                    case SecureProtectionState.Active:
                        CameraSecureText = _localizationService.GetString("StatusSecureActive", "🛡️ Secure Active (Hardened)");
                        CameraSecureBadgeColor = "#D32F2F";
                        CameraSecureButtonText = _localizationService.GetString("DisableSecure", "Disable Secure");
                        IsCameraSecureButtonEnabled = true;
                        IsCameraSecureHintVisible = false;
                        CameraSecureHint = "";
                        break;
                    case SecureProtectionState.Available:
                        CameraSecureText = _localizationService.GetString("StatusSecureAvailable", "○ Available to enable");
                        CameraSecureBadgeColor = "#F57C00";
                        CameraSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                        IsCameraSecureButtonEnabled = true;
                        IsCameraSecureHintVisible = false;
                        CameraSecureHint = "";
                        break;
                    default:
                        CameraSecureText = state.Camera.SecureState == SecureProtectionState.Unknown
                            ? _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable")
                            : _localizationService.GetString("StatusSecureUnavailable", "🔒 Unavailable");
                        CameraSecureBadgeColor = "#555555";
                        CameraSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                        IsCameraSecureButtonEnabled = false;
                        IsCameraSecureHintVisible = !camBlocked || state.Camera.SecureState == SecureProtectionState.Unknown;
                        CameraSecureHint = camAbsent
                            ? _localizationService.GetString("NoCameraDetected", "No camera detected")
                            : (camStandardUnverifiable || state.Camera.SecureState == SecureProtectionState.Unknown
                                ? _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable")
                                : _localizationService.GetString("SecureRequirementHint", "Enable Standard Protection first"));
                        break;
                }
            }

            // === 3. Microphone UI ===
            var hwSummary = state.Microphone.HardwareMuteSummary;
            var micSoftwareOnlyMute = state.Microphone.EndpointAvailability == AudioEndpointAvailability.Available &&
                                      hwSummary is { TotalCount: > 0 } &&
                                      hwSummary.NotSupportedCount > 0 &&
                                      hwSummary.UnknownCount == 0;
            var micCanToggleStandard = state.Microphone.StandardState != StandardProtectionState.Unknown &&
                                       (state.Microphone.StandardIsVerified ||
                                        (state.Microphone.EndpointAvailability == AudioEndpointAvailability.Available &&
                                         hwSummary is { TotalCount: > 0 }));

            var micBlocked = state.Microphone.IsProtected;
            var micAbsent = state.Microphone.Availability == TargetDeviceAvailability.NotDetected &&
                            state.Microphone.EndpointAvailability == AudioEndpointAvailability.NotDetected;
            var micHardwareWithoutEndpoints = state.Microphone.Availability == TargetDeviceAvailability.Available &&
                                              state.Microphone.EndpointAvailability == AudioEndpointAvailability.NotDetected;
            var micStandardUnverifiable = state.Microphone.StandardState == StandardProtectionState.Unknown ||
                                          !state.Microphone.StandardIsVerified ||
                                          state.Microphone.EndpointAvailability == AudioEndpointAvailability.Unknown;
            var micSecureUnverifiable = state.Microphone.SecureState == SecureProtectionState.Unknown ||
                                        !state.Microphone.SecureIsVerified;
            var micAllowedVerified = !micBlocked && !micAbsent && !micHardwareWithoutEndpoints &&
                                     !micStandardUnverifiable && !micSecureUnverifiable &&
                                     state.Microphone.StandardState == StandardProtectionState.Inactive &&
                                     state.Microphone.EndpointAvailability == AudioEndpointAvailability.Available;

            IsMicBlocked = micBlocked;
            IsMicStandardActive = micBlocked;

            if (isUntrusted)
            {
                IsMicToggleEnabled = false;
            }
            else if (micBlocked)
            {
                // Safe unblock/restoration: user can restore if Standard is Active (including software-only mute) or Secure is Active and verified
                IsMicToggleEnabled = (state.Microphone.StandardState == StandardProtectionState.Active && micCanToggleStandard) ||
                                     (state.Microphone.SecureState == SecureProtectionState.Active && state.Microphone.SecureIsVerified);
            }
            else
            {
                // Safe mutation: user can enable Standard if endpoints are available and Standard is Inactive (including software-only mute)
                IsMicToggleEnabled = !micAbsent && !micHardwareWithoutEndpoints && micCanToggleStandard &&
                                     state.Microphone.EndpointAvailability == AudioEndpointAvailability.Available &&
                                     state.Microphone.StandardState == StandardProtectionState.Inactive;
            }

            if (micBlocked)
            {
                if (state.Microphone.SecureState == SecureProtectionState.Active && state.Microphone.SecureIsVerified)
                {
                    MicStatusText = _localizationService.GetString("StatusBlockedAdvanced", "🛡️ Blocked (Advanced)");
                    MicStatusColor = "#D32F2F";
                    MicStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    MicStandardColor = "#E57373";
                }
                else if (state.Microphone.StandardState == StandardProtectionState.Active && state.Microphone.StandardIsVerified)
                {
                    if (micSecureUnverifiable)
                    {
                        MicStatusText = _localizationService.GetString("StatusPartiallyUnverifiable", "⚠️ Parcialmente protegido / No verificable");
                        MicStatusColor = "#FFA726";
                    }
                    else
                    {
                        MicStatusText = _localizationService.GetString("StatusBlockedStandard", "🔒 Blocked (Standard)");
                        MicStatusColor = "#E57373";
                    }
                    MicStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    MicStandardColor = "#E57373";
                }
                else if (state.Microphone.StandardState == StandardProtectionState.Active && micSoftwareOnlyMute)
                {
                    MicStatusText = _localizationService.GetString("StatusBlockedSoftwareOnly", "⚠️ Partially Protected (Software Mute Only)");
                    MicStatusColor = "#FFA726";
                    MicStandardText = _localizationService.GetString("StatusStandardActiveSoftwareOnly", "● Standard Active (Software Mute Only)");
                    MicStandardColor = "#FFA726";
                }
                else
                {
                    MicStatusText = _localizationService.GetString("StatusPartiallyUnverifiable", "⚠️ Parcialmente protegido / No verificable");
                    MicStatusColor = "#FFA726";
                    MicStandardText = _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable");
                    MicStandardColor = "#FFA726";
                }
            }
            else if (micAbsent)
            {
                MicStatusText = _localizationService.GetString("NoMicrophonesDetected", "No input microphones detected");
                MicStatusColor = "#9E9E9E";
                MicStandardText = _localizationService.GetString("NoMicrophonesDetected", "No input microphones detected");
                MicStandardColor = "#9E9E9E";
            }
            else if (micHardwareWithoutEndpoints)
            {
                MicStatusText = _localizationService.GetString("MicrophoneDetectedNoEndpoints", "Microphone detected, but no active capture endpoint available");
                MicStatusColor = "#FFA726";
                MicStandardText = _localizationService.GetString("MicrophoneDetectedNoEndpoints", "Microphone detected, but no active capture endpoint available");
                MicStandardColor = "#FFA726";
            }
            else if (micStandardUnverifiable || micSecureUnverifiable)
            {
                MicStatusText = _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                MicStatusColor = "#FFA726";
                MicStandardText = micStandardUnverifiable
                    ? _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable")
                    : _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                MicStandardColor = micStandardUnverifiable ? "#FFA726" : "#81C784";
            }
            else
            {
                MicStatusText = _localizationService.GetString("StatusUnblocked", "✅ Allowed");
                MicStatusColor = "#81C784";
                MicStandardText = _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                MicStandardColor = "#81C784";
            }

            // Microphone Secure Compatibility fields
            if (isStore || isUntrusted)
            {
                MicSecureText = isStore
                    ? _localizationService.GetString("StoreEditionSecureNotSupported", "Unavailable in Microsoft Store edition")
                    : _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                MicSecureBadgeColor = "#555555";
                MicSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                IsMicSecureButtonEnabled = false;
                IsMicSecureHintVisible = false;
                MicSecureHint = "";
            }
            else
            {
                switch (state.Microphone.SecureState)
                {
                    case SecureProtectionState.Active:
                        MicSecureText = _localizationService.GetString("StatusSecureActive", "🛡️ Secure Active (Hardened)");
                        MicSecureBadgeColor = "#D32F2F";
                        MicSecureButtonText = _localizationService.GetString("DisableSecure", "Disable Secure");
                        IsMicSecureButtonEnabled = true;
                        IsMicSecureHintVisible = false;
                        MicSecureHint = "";
                        break;
                    case SecureProtectionState.Available:
                        MicSecureText = _localizationService.GetString("StatusSecureAvailable", "○ Available to enable");
                        MicSecureBadgeColor = "#F57C00";
                        MicSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                        IsMicSecureButtonEnabled = true;
                        IsMicSecureHintVisible = false;
                        MicSecureHint = "";
                        break;
                    default:
                        MicSecureText = state.Microphone.SecureState == SecureProtectionState.Unknown
                            ? _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable")
                            : _localizationService.GetString("StatusSecureUnavailable", "🔒 Unavailable");
                        MicSecureBadgeColor = "#555555";
                        MicSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                        IsMicSecureButtonEnabled = false;
                        IsMicSecureHintVisible = !micBlocked || state.Microphone.SecureState == SecureProtectionState.Unknown;
                        if (micAbsent)
                            MicSecureHint = _localizationService.GetString("NoMicrophonesDetected", "No input microphones detected");
                        else if (micHardwareWithoutEndpoints)
                            MicSecureHint = _localizationService.GetString("MicrophoneDetectedNoEndpoints", "Microphone detected, but no active capture endpoint available");
                        else if (micStandardUnverifiable || state.Microphone.SecureState == SecureProtectionState.Unknown)
                            MicSecureHint = _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                        else
                            MicSecureHint = _localizationService.GetString("SecureRequirementHint", "Enable Standard Protection first");
                        break;
                }
            }

            // Clean telemetry and reset if endpoints unavailable/unknown
            if (hwSummary != null && hwSummary.TotalCount > 0 &&
                state.Microphone.EndpointAvailability == AudioEndpointAvailability.Available)
            {
                HasMicHardwareCapability = true;
                if (hwSummary.TotalCount == 1)
                {
                    if (hwSummary.SupportedCount == 1)
                        MicHardwareCapabilityText = _localizationService.GetString("HardwareMuteSupported", "Hardware mute support available");
                    else if (hwSummary.NotSupportedCount == 1)
                        MicHardwareCapabilityText = _localizationService.GetString("HardwareMuteNotSupported", "Hardware mute support not reported");
                    else
                        MicHardwareCapabilityText = _localizationService.GetString("HardwareMuteUnknown", "Hardware mute support unknown");
                }
                else
                {
                    var formatStr = _localizationService.GetString("HardwareMuteSummaryFormat", "Hardware mute: {0} of {1} supported, {2} not reported, {3} unknown");
                    MicHardwareCapabilityText = string.Format(formatStr, hwSummary.SupportedCount, hwSummary.TotalCount, hwSummary.NotSupportedCount, hwSummary.UnknownCount);
                }
            }
            else
            {
                HasMicHardwareCapability = false;
                MicHardwareCapabilityText = string.Empty;
            }

            // === 4. Unified (Both) UI ===
            var camProtectedVerified = camBlocked && !camStandardUnverifiable &&
                                       (state.Camera.SecureState != SecureProtectionState.Active || state.Camera.SecureIsVerified);
            var micProtectedVerified = micBlocked && !micStandardUnverifiable &&
                                       (state.Microphone.SecureState != SecureProtectionState.Active || state.Microphone.SecureIsVerified);
            var bothBlockedVerified = camProtectedVerified && micProtectedVerified;

            IsBothBlocked = bothBlockedVerified;
            IsBothStandardActive = bothBlockedVerified;
            IsBothToggleEnabled = !isStore && !isUntrusted &&
                                  IsCameraToggleEnabled && IsMicToggleEnabled && (IsCameraBlocked == IsMicBlocked);

            if (bothBlockedVerified)
            {
                if (state.BothSecure)
                {
                    BothStatusText = _localizationService.GetString("StatusBlockedAdvanced", "🛡️ Blocked (Advanced)");
                    BothStatusColor = "#D32F2F";
                    BothStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    BothStandardColor = "#E57373";
                }
                else
                {
                    BothStatusText = _localizationService.GetString("StatusBlockedStandard", "🔒 Blocked (Standard)");
                    BothStatusColor = "#E57373";
                    BothStandardText = _localizationService.GetString("StatusStandardActive", "● Standard Active");
                    BothStandardColor = "#E57373";
                }
            }
            else if (camAllowedVerified && micAllowedVerified)
            {
                BothStatusText = _localizationService.GetString("StatusUnblocked", "✅ Allowed");
                BothStatusColor = "#81C784";
                BothStandardText = _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                BothStandardColor = "#81C784";
            }
            else if ((camProtectedVerified && micAllowedVerified) || (micProtectedVerified && camAllowedVerified))
            {
                BothStatusText = _localizationService.GetString("StatusPartiallyBlocked", "🔒 Parcialmente bloqueado");
                BothStatusColor = "#FFA726";
                BothStandardText = _localizationService.GetString("StatusStandardInactive", "○ Standard Inactive");
                BothStandardColor = "#FFA726";
            }
            else if (camAbsent && micAbsent)
            {
                BothStatusText = _localizationService.GetString("NoDevicesDetected", "No devices detected");
                BothStatusColor = "#9E9E9E";
                BothStandardText = _localizationService.GetString("NoDevicesDetected", "No devices detected");
                BothStandardColor = "#9E9E9E";
            }
            else if (camBlocked || micBlocked)
            {
                BothStatusText = _localizationService.GetString("StatusPartiallyUnverifiable", "⚠️ Parcialmente protegido / No verificable");
                BothStatusColor = "#FFA726";
                BothStandardText = _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable");
                BothStandardColor = "#FFA726";
            }
            else
            {
                BothStatusText = _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                BothStatusColor = "#FFA726";
                BothStandardText = _localizationService.GetString("StatusStandardUnknown", "○ Standard Unverifiable");
                BothStandardColor = "#FFA726";
            }

            if (isStore || isUntrusted)
            {
                BothSecureText = isStore
                    ? _localizationService.GetString("StoreEditionSecureNotSupported", "Unavailable in Microsoft Store edition")
                    : _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable");
                BothSecureBadgeColor = "#555555";
                BothSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                IsBothSecureButtonEnabled = false;
                IsBothSecureHintVisible = false;
                BothSecureHint = "";
            }
            else
            {
                var camSecure = state.Camera.SecureState;
                var micSecure = state.Microphone.SecureState;
                var bothSecureActive = camSecure == SecureProtectionState.Active && micSecure == SecureProtectionState.Active;
                var bothSecureAvailable = bothBlockedVerified &&
                    (camSecure is SecureProtectionState.Available or SecureProtectionState.Active) &&
                    (micSecure is SecureProtectionState.Available or SecureProtectionState.Active);

                if (bothSecureActive)
                {
                    BothSecureText = _localizationService.GetString("StatusSecureActive", "🛡️ Secure Active (Hardened)");
                    BothSecureBadgeColor = "#D32F2F";
                    BothSecureButtonText = _localizationService.GetString("DisableSecure", "Disable Secure");
                    IsBothSecureButtonEnabled = true;
                    IsBothSecureHintVisible = false;
                    BothSecureHint = "";
                }
                else if (bothSecureAvailable)
                {
                    BothSecureText = _localizationService.GetString("StatusSecureAvailable", "○ Available to enable");
                    BothSecureBadgeColor = "#F57C00";
                    BothSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                    IsBothSecureButtonEnabled = true;
                    IsBothSecureHintVisible = false;
                    BothSecureHint = "";
                }
                else
                {
                    BothSecureText = (camSecureUnverifiable || micSecureUnverifiable)
                        ? _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable")
                        : _localizationService.GetString("StatusSecureUnavailable", "🔒 Unavailable");
                    BothSecureBadgeColor = "#555555";
                    BothSecureButtonText = _localizationService.GetString("EnableSecure", "🔒 Enable Secure Protection");
                    IsBothSecureButtonEnabled = false;
                    IsBothSecureHintVisible = !bothBlockedVerified || camSecureUnverifiable || micSecureUnverifiable;
                    BothSecureHint = (camSecureUnverifiable || micSecureUnverifiable)
                        ? _localizationService.GetString("StatusUnverifiable", "⚠️ Unverifiable")
                        : _localizationService.GetString("SecureRequirementHint", "Enable Standard Protection first");
                }
            }

            // Overall Badge
            if (state.BothSecure)
                SecurityBadgeText = _localizationService.GetString("BadgeHardened", "Reforzado");
            else if (bothBlockedVerified)
                SecurityBadgeText = _localizationService.GetString("BadgeProtected", "Protegido");
            else if (camBlocked || micBlocked)
                SecurityBadgeText = _localizationService.GetString("BadgePartiallyProtected", "Parcialmente protegido");
            else if (camStandardUnverifiable || camSecureUnverifiable || micStandardUnverifiable || micSecureUnverifiable)
                SecurityBadgeText = _localizationService.GetString("BadgeUnverifiable", "No verificable");
            else
                SecurityBadgeText = _localizationService.GetString("BadgeUnprotected", "Sin protección");

            // Devices list
            var enabledStr = _localizationService.GetString("DeviceEnabled", "Habilitado");
            var disabledStr = _localizationService.GetString("DeviceDisabled", "Deshabilitado");
            var camTypeLabel = _localizationService.GetString("DeviceTypeCamera", "Cámara");
            var micTypeLabel = _localizationService.GetString("DeviceTypeMicrophone", "Micrófono");

            Devices.Clear();
            foreach (var d in devices)
            {
                var category = d.DeviceType == DeviceType.Camera ? camTypeLabel : micTypeLabel;
                Devices.Add(new DeviceItemViewModel(d, category, enabledStr, disabledStr));
            }

            // Cross-Edition Inspection & Supervision Only Mode
            var crossInspection = _protectionService.CrossEditionCoordinator.InspectOwnership();
            if (crossInspection.RequiresSupervisionOnly)
            {
                IsSupervisionOnly = true;
                SupervisionMessage = crossInspection.Message ?? _localizationService.GetString("SupervisionModeActive", "Supervision Only Mode");
                IsBothToggleEnabled = false;
                IsCameraToggleEnabled = false;
                IsMicToggleEnabled = false;
                IsAdvancedProtectionToggleEnabled = false;
                IsCameraSecureButtonEnabled = false;
                IsMicSecureButtonEnabled = false;
                IsBothSecureButtonEnabled = false;
            }
            else
            {
                IsSupervisionOnly = false;
                SupervisionMessage = string.Empty;
            }

            // Store Edition specific hints & warnings
            OpenCameraSettingsButtonText = _localizationService.GetString("OpenCameraSettingsButtonText", "Settings");
            if (isStore && micBlocked)
            {
                IsMicActiveStoreWarningVisible = true;
                MicActiveStoreWarningMessage = _localizationService.GetString(
                    "StoreMicUninstallWarning",
                    "Notice: If you uninstall the app while the microphone is muted, Windows will not automatically unmute it. Please unmute before uninstalling.");
            }
            else
            {
                IsMicActiveStoreWarningVisible = false;
                MicActiveStoreWarningMessage = string.Empty;
            }

            UpdatePlatformAndCapabilities();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    // --- Property Changed Event Handlers for ToggleSwitches ---

    partial void OnIsCameraBlockedChanged(bool value)
    {
        if (_isUpdating) return;
        _ = ExecuteCameraToggleAsync(value);
    }

    private async Task ExecuteCameraToggleAsync(bool enable)
    {
        ClearError();
        if (IsSupervisionOnly)
        {
            ShowError(SupervisionMessage);
            await RefreshStateAsync();
            return;
        }
        Log.Information("User toggled Camera to {Enable} (IsStoreEdition={IsStore})", enable, _protectionService.IsStoreEdition);

        if (_protectionService.IsStoreEdition)
        {
            await OpenCameraPrivacySettingsAsync();
            return;
        }

        var result = enable
            ? await _protectionService.BlockDeviceAsync(BlockTarget.Camera)
            : await _protectionService.UnblockDeviceAsync(BlockTarget.Camera);

        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateCamera", "Failed to update Camera protection"));
            await RefreshStateAsync();
        }
    }

    [RelayCommand]
    public async Task OpenCameraPrivacySettingsAsync()
    {
        ClearError();
        var result = await _protectionService.OpenPrivacySettingsAsync(BlockTarget.Camera);
        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateCamera", "Failed to open Camera privacy settings"));
        }
        await RefreshStateAsync();
    }

    [RelayCommand]
    public async Task OpenMicrophonePrivacySettingsAsync()
    {
        ClearError();
        var result = await _protectionService.OpenPrivacySettingsAsync(BlockTarget.Microphone);
        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateMic", "Failed to open Microphone privacy settings"));
        }
        await RefreshStateAsync();
    }

    partial void OnIsCameraStandardActiveChanged(bool value)
    {
        if (_isUpdating) return;
        IsCameraBlocked = value;
    }

    partial void OnIsMicBlockedChanged(bool value)
    {
        if (_isUpdating) return;
        _ = ExecuteMicToggleAsync(value);
    }

    private async Task ExecuteMicToggleAsync(bool enable)
    {
        ClearError();
        if (IsSupervisionOnly)
        {
            ShowError(SupervisionMessage);
            await RefreshStateAsync();
            return;
        }
        Log.Information("User toggled Microphone to {Enable}", enable);

        var result = enable
            ? await _protectionService.BlockDeviceAsync(BlockTarget.Microphone)
            : await _protectionService.UnblockDeviceAsync(BlockTarget.Microphone);

        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateMic", "Failed to update Microphone protection"));
            await RefreshStateAsync();
        }
    }

    partial void OnIsMicStandardActiveChanged(bool value)
    {
        if (_isUpdating) return;
        IsMicBlocked = value;
    }

    partial void OnIsBothBlockedChanged(bool value)
    {
        if (_isUpdating) return;
        _ = ExecuteBothToggleAsync(value);
    }

    private async Task ExecuteBothToggleAsync(bool enable)
    {
        ClearError();
        if (IsSupervisionOnly)
        {
            ShowError(SupervisionMessage);
            await RefreshStateAsync();
            return;
        }
        Log.Information("User toggled Unified Control to {Enable}", enable);

        var result = enable
            ? await _protectionService.BlockDeviceAsync(BlockTarget.Both)
            : await _protectionService.UnblockDeviceAsync(BlockTarget.Both);

        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateUnified", "Failed to update Unified protection"));
            await RefreshStateAsync();
        }
    }

    partial void OnIsBothStandardActiveChanged(bool value)
    {
        if (_isUpdating) return;
        IsBothBlocked = value;
    }

    partial void OnIsAdvancedProtectionEnabledChanged(bool value)
    {
        if (_isUpdating) return;
        _ = ExecuteAdvancedProtectionToggleAsync(value);
    }

    private async Task ExecuteAdvancedProtectionToggleAsync(bool enable)
    {
        ClearError();
        Log.Information("User toggled Advanced Protection to {Enable}", enable);

        var result = await _protectionService.SetAdvancedProtectionAsync(enable);
        if (!result.Success)
        {
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorUpdateAdvanced", "Failed to update Advanced Protection"));
            await RefreshStateAsync();
        }
    }

    // --- Legacy Relay Commands for compatibility ---

    [RelayCommand]
    private async Task ToggleCameraSecureAsync()
    {
        if (_isUpdating) return;
        await ToggleSecureAsync(
            BlockTarget.Camera,
            state => state.Camera.SecureState == SecureProtectionState.Active,
            _localizationService.GetString("ErrorCameraSecure", "Camera Secure Protection error"));
    }

    [RelayCommand]
    private async Task ToggleMicSecureAsync()
    {
        if (_isUpdating) return;
        await ToggleSecureAsync(
            BlockTarget.Microphone,
            state => state.Microphone.SecureState == SecureProtectionState.Active,
            _localizationService.GetString("ErrorMicSecure", "Microphone Secure Protection error"));
    }

    [RelayCommand]
    private async Task ToggleBothSecureAsync()
    {
        if (_isUpdating) return;
        await ToggleSecureAsync(
            BlockTarget.Both,
            state => state.Camera.SecureState == SecureProtectionState.Active &&
                     state.Microphone.SecureState == SecureProtectionState.Active,
            _localizationService.GetString("ErrorBothSecure", "Both Secure Protection error"));
    }

    private async Task ToggleSecureAsync(
        BlockTarget target,
        Func<FullProtectionState, bool> isActive,
        string fallbackError)
    {
        ClearError();
        try
        {
            var state = await _protectionService.GetCurrentStateAsync();
            var result = isActive(state)
                ? await _protectionService.DisableSecureProtectionAsync(target)
                : await _protectionService.EnableSecureProtectionAsync(target);
            if (!result.Success)
                ShowError(result.ErrorMessage ?? fallbackError);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Secure protection toggle failed before state could be verified");
            ShowError($"{fallbackError}: {ex.Message}");
        }

        await RefreshStateAsync();
    }

    [RelayCommand]
    private void SelectLanguage(string lang)
    {
        if (_isUpdating) return;
        var result = _localizationService.SetLanguage(lang);
        IsSpanishSelected = lang == "es";
        IsEnglishSelected = lang == "en";
        if (!result.Success)
            ShowError(result.ErrorMessage ?? _localizationService.GetString("ErrorLanguagePreference", "Failed to save language preference"));
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
        IsAutostartEnabled = IsAutostartStateKnown(status.State) ? status.IsEnabled : null;
        AutostartStatusMessage = status.Message ?? "";
    }

    private static bool IsAutostartStateKnown(AutostartState state) =>
        state is AutostartState.Enabled
            or AutostartState.Disabled
            or AutostartState.DisabledByUser
            or AutostartState.DisabledByPolicy;

    public void Dispose()
    {
        _disposed = true;
        _protectionService.StateChanged -= OnProtectionStateChanged;
        _autostartStateService.StatusChanged -= OnAutostartStatusChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void ShowError(string msg)
    {
        ErrorMessage = msg;
        HasError = true;
    }

    public void ReportExternalError(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => ShowError(message));
    }

    public event Action<SettingsSection>? OpenSettingsRequested;

    [RelayCommand]
    public void OpenSettings(string? sectionName)
    {
        var section = SettingsSection.General;
        if (!string.IsNullOrWhiteSpace(sectionName) &&
            Enum.TryParse<SettingsSection>(sectionName, true, out var parsed))
        {
            section = parsed;
        }

        OpenSettingsRequested?.Invoke(section);
    }

    [RelayCommand]
    private void ClearError()
    {
        ErrorMessage = "";
        HasError = false;
    }
}

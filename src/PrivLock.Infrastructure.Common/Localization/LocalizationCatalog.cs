namespace PrivLock.Infrastructure.Common.Localization;

/// <summary>
/// Cross-platform in-memory dictionary catalogs for UI localization (ES and EN).
/// Completely decoupled from WPF ResourceDictionaries or OS-specific formats.
/// </summary>
public static class LocalizationCatalog
{
    public static readonly IReadOnlyDictionary<string, string> StringsEs = new Dictionary<string, string>
    {
        ["AppTitle"] = "PrivGvard",
        ["AppSubtitle"] = "Bloqueador de Cámara y Micrófono",

        // Sections
        ["CameraTitle"] = "Cámara",
        ["CameraSubtitle"] = "Protección de webcam y sensores de video",
        ["MicrophoneTitle"] = "Micrófono",
        ["MicrophoneSubtitle"] = "Protección de micrófonos y captura de audio",

        // Status
        ["StatusBlocked"] = "🔒 Bloqueado",
        ["StatusAllowed"] = "✅ Permitido",

        // Standard Protection
        ["StandardProtectionTitle"] = "Protección Estándar",
        ["StandardProtectionDesc"] = "Protección cotidiana sin permisos elevados",
        ["EnableStandard"] = "Activar Estándar",
        ["DisableStandard"] = "Desactivar Estándar",
        ["StatusStandardActive"] = "● Estándar Activa",
        ["StatusStandardInactive"] = "○ Estándar Inactiva",

        // Secure Protection
        ["SecureProtectionTitle"] = "Protección Segura (Admin)",
        ["SecureProtectionDesc"] = "Aislamiento físico y directivas de sistema de bajo nivel",
        ["EnableSecure"] = "🔒 Activar Protección Segura",
        ["DisableSecure"] = "Desactivar Segura",
        ["StatusSecureUnavailable"] = "🔒 No disponible (Activa estándar primero)",
        ["StatusSecureAvailable"] = "○ Disponible para activar",
        ["StatusSecureActive"] = "🛡️ Segura Activa (Reforzada)",
        ["SecureRequirementHint"] = "Activa primero la Protección Estándar",

        // Devices
        ["DetectedDevices"] = "Dispositivos de Hardware Detectados",
        ["DeviceEnabled"] = "Habilitado",
        ["DeviceDisabled"] = "Deshabilitado",

        // Settings & Footers
        ["StartWithSystem"] = "Iniciar con el sistema",
        ["Language"] = "Idioma",
        ["CapabilitiesTitle"] = "Capacidades del Sistema",

        // Unified Control
        ["UnifiedTitle"] = "Control Unificado",
        ["UnifiedSubtitle"] = "Controlar cámara y micrófono simultáneamente",
        ["UnifiedStandardDesc"] = "Proteger ambos dispositivos a la vez",
        ["UnifiedSecureDesc"] = "Reforzar protección en ambos dispositivos",

        // Advanced Protection (Global Extension)
        ["AdvancedProtectionTitle"] = "Protección Avanzada",
        ["AdvancedProtectionSubtitle"] = "Refuerzo físico y directivas para dispositivos bloqueados",
        ["AdvancedProtectionDesc"] = "Aplica aislamiento de hardware automáticamente a los dispositivos bloqueados",
        ["StatusAdvancedActive"] = "🛡️ Reforzada (Avanzada)",
        ["StatusAdvancedInactive"] = "○ Inactiva",
        ["StatusBlockedAdvanced"] = "🛡️ Bloqueado (Avanzado)",
        ["StatusBlockedStandard"] = "🔒 Bloqueado (Estándar)",
        ["StatusUnblocked"] = "✅ Permitido",

        // Notifications & Errors
        ["ElevationCancelled"] = "Operación cancelada: Se denegaron los permisos de administrador.",
        ["StandardRequiredFirst"] = "Debes activar la Protección Estándar antes de activar la Protección Segura.",

        // Settings Window Navigation
        ["SettingsWindowTitle"] = "Configuración",
        ["SettingsHeaderTitle"] = "Configuración",
        ["SettingsNavGeneral"] = "Ajustes",
        ["SettingsNavHelp"] = "Ayuda y Documentación",
        ["SettingsNavDiagnostics"] = "Diagnóstico del Sistema",
        ["SettingsNavAbout"] = "Acerca de PrivGvard",

        // Section: General Settings
        ["AppearanceTitle"] = "Apariencia",
        ["ThemeLabel"] = "Tema de la interfaz",
        ["ThemeDark"] = "Oscuro (Predeterminado)",
        ["ThemeDescription"] = "El tema oscuro de alto contraste está optimizado para la interfaz de PrivGvard.",
        ["StartupTitle"] = "Inicio del Sistema",
        ["StartupDescription"] = "Iniciar PrivGvard automáticamente al iniciar sesión en el equipo.",
        ["AutostartStateUnknown"] = "No se pudo verificar el estado real del inicio automático.",
        ["AutostartDisabledByUser"] = "Windows ha desactivado el inicio de PrivGvard. Actívalo en Configuración de Windows o en el Administrador de tareas.",
        ["AutostartDisabledByPolicy"] = "Una directiva de Windows impide que PrivGvard se inicie automáticamente.",
        ["AutostartMigrationFailed"] = "No se pudo migrar la configuración de inicio de PrivGvard: {0}",
        ["LanguageTitle"] = "Idioma de la interfaz",
        ["LanguageDescription"] = "Selecciona el idioma principal de la aplicación.",

        // Section: Help & Documentation
        ["HelpTitle"] = "Ayuda y Documentación",
        ["HelpSubtitle"] = "Guía de referencia rápida y funcionamiento de PrivGvard",
        ["HelpHowItWorksTitle"] = "Cómo funciona PrivGvard",
        ["HelpHowItWorksDesc"] = "En Windows, PrivGvard combina controles de privacidad del sistema con administración de dispositivos. El resultado depende del modo elegido, el hardware, el controlador, los permisos y la configuración; la aplicación verifica el estado efectivo cuando el sistema lo permite.",
        ["HelpCameraTitle"] = "Protección de Cámara",
        ["HelpCameraDesc"] = "Según el modo y el dispositivo, puede aplicar la directiva de privacidad de cámara de Windows y administrar nodos de dispositivo compatibles. Esto no desconecta físicamente la cámara y no todos los modelos o controladores admiten cada medida.",
        ["HelpMicTitle"] = "Protección de Micrófono",
        ["HelpMicDesc"] = "Según el modo y el dispositivo, puede aplicar la directiva de privacidad de micrófono, administrar dispositivos compatibles y silenciar puntos de conexión de captura de audio. Silenciar un punto de conexión no equivale a desconectarlo físicamente.",
        ["HelpAdvancedTitle"] = "Protección Avanzada",
        ["HelpAdvancedDesc"] = "La protección avanzada puede combinar directivas de privacidad de Windows y administración de dispositivos PnP (Plug and Play). La compatibilidad varía según el dispositivo y el controlador, y algunas operaciones requieren permisos de administrador.",
        ["HelpUnifiedTitle"] = "Control Unificado",
        ["HelpUnifiedDesc"] = "Solicita el cambio para cámara y micrófono con un solo control. Cada uno se procesa y verifica por separado, por lo que pueden quedar con resultados distintos si una operación falla o un dispositivo no es compatible.",
        ["HelpFaqTitle"] = "Preguntas Frecuentes",
        ["HelpFaq1Q"] = "¿Se mantienen los bloqueos tras reiniciar el equipo?",
        ["HelpFaq1A"] = "Al salir normalmente, la aplicación intenta restaurar los cambios que administra. Al iniciar, intenta recuperar operaciones interrumpidas y reconciliar el sistema con el estado deseado guardado. El diario no garantiza protección ininterrumpida durante el reinicio; comprueba el estado después de iniciar sesión.",
        ["HelpFaq2Q"] = "¿Por qué aparece UAC y qué pasa si cancelo?",
        ["HelpFaq2A"] = "Algunas operaciones de Windows requieren permisos de administrador y solicitan autorización en ese momento. Si cancelas el aviso, esa operación falla y no debe considerarse protección confirmada; revisa el estado y el mensaje de error.",
        ["HelpFaq3Q"] = "¿Cerrar la ventana es lo mismo que elegir «Salir»?",
        ["HelpFaq3A"] = "Con la bandeja disponible, cerrar la ventana o usar Alt+F4 la oculta y la aplicación sigue en ejecución. «Salir» desde la bandeja inicia el cierre e intenta restaurar los cambios que PrivGvard administra. Si la bandeja no está disponible, cerrar la ventana inicia el cierre coordinado.",
        ["HelpFaq4Q"] = "¿Qué hago si un dispositivo no es compatible o falla?",
        ["HelpFaq4A"] = "Actualiza el diagnóstico y revisa el estado efectivo, los permisos y el controlador; consulta el mensaje de error. Un estado deseado guardado no confirma que la protección se haya aplicado. Si la compatibilidad figura como no disponible o el estado no se verifica, no lo consideres protegido.",

        // Section: System Diagnostics
        ["DiagTitle"] = "Diagnóstico del Sistema",
        ["DiagSubtitle"] = "Telemetría y estado operativo de privacidad",
        ["DiagOsLabel"] = "Sistema Operativo",
        ["DiagArchLabel"] = "Arquitectura",
        ["DiagPrivilegesLabel"] = "Nivel de Privilegios",
        ["DiagCamStatusLabel"] = "Estado de Cámara",
        ["DiagMicStatusLabel"] = "Estado de Micrófono",
        ["DiagAdvancedLabel"] = "Protección Avanzada",
        ["DiagDevicesCountLabel"] = "Dispositivos Detectados",
        ["DiagCapabilitiesLabel"] = "Nivel de Capacidades",
        ["DiagRefreshButton"] = "Actualizar Diagnóstico",

        // Section: About
        ["AboutTitle"] = "Acerca de PrivGvard",
        ["AboutVersionLabel"] = "Versión",
        ["AboutDeveloperLabel"] = "Desarrollador",
        ["AboutDeveloperValue"] = "cdev Studio",
        ["AboutDescription"] = "Solución de privacidad y control de hardware de cámara y micrófono para Windows.",
        ["AboutLicenseLabel"] = "Licencia",
        ["AboutLicenseValue"] = "Licencia MIT",
        ["AboutCopyright"] = "© 2026 cdev Studio. Todos los derechos reservados.",

        // Device Types & Labels
        ["DeviceTypeCamera"] = "Cámara",
        ["DeviceTypeMicrophone"] = "Micrófono",
        ["SummaryCameraLabel"] = "Cámara",
        ["SummaryMicrophoneLabel"] = "Micrófono",

        // User & Privilege Levels
        ["UserStandard"] = "Usuario estándar",
        ["UserAdmin"] = "Administrador",
        ["DiagPrivilegeStandard"] = "Usuario estándar (asInvoker)",
        ["DiagPrivilegeAdmin"] = "Administrador (Elevado)",

        // Overall Badges & Protection Statuses
        ["BadgeHardened"] = "Reforzado",
        ["BadgeProtected"] = "Protegido",
        ["BadgePartiallyProtected"] = "Parcialmente protegido",
        ["BadgeUnprotected"] = "Sin protección",
        ["StatusPartiallyBlocked"] = "🔒 Parcialmente bloqueado",
        ["StatusActive"] = "🛡️ Activa",
        ["StatusInactive"] = "○ Inactiva",

        // Canonical Semantic Statuses
        ["Enabled"] = "Habilitado",
        ["Disabled"] = "Deshabilitado",
        ["Protected"] = "Protegido",
        ["Unprotected"] = "Sin protección",
        ["Active"] = "Activo",
        ["Inactive"] = "Inactivo",
        ["Allowed"] = "Permitido",
        ["Blocked"] = "Bloqueado",

        // Diagnostics Formatting & Hardware States
        ["DevicesCountFormat"] = "{0} dispositivos detectados en total",
        ["Yes"] = "Sí",
        ["No"] = "No",

        // Main Window Menu
        ["MenuToolTip"] = "Menú y Ajustes",
        ["MenuGeneral"] = "⚙️  Ajustes",
        ["MenuHelp"] = "📖  Ayuda y Documentación",
        ["MenuDiagnostics"] = "🛡️  Diagnóstico del Sistema",
        ["MenuAbout"] = "ℹ️  Acerca de PrivGvard",

        // System Tray Menu & Tooltip
        ["TrayOpen"] = "Abrir PrivGvard",
        ["Tray.Open"] = "Abrir PrivGvard",
        ["TrayExit"] = "Salir",
        ["Tray.Exit"] = "Salir",
        ["Tray.ToolTip"] = "PrivGvard - Bloqueador de Cámara y Micrófono",
        ["AppSubtitleLong"] = "PrivGvard - Bloqueador de Cámara y Micrófono",

        // Common Error Fallbacks
        ["ErrorUpdateCamera"] = "No se pudo actualizar la protección de la cámara",
        ["ErrorUpdateMic"] = "No se pudo actualizar la protección del micrófono",
        ["ErrorUpdateUnified"] = "No se pudo actualizar la protección unificada",
        ["ErrorUpdateAdvanced"] = "No se pudo actualizar la Protección Avanzada",
        ["ErrorCameraSecure"] = "Error de Protección Segura de Cámara",
        ["ErrorMicSecure"] = "Error de Protección Segura de Micrófono",
        ["ErrorBothSecure"] = "Error de Protección Segura Unificada",
        ["ErrorLanguagePreference"] = "No se pudo guardar la preferencia de idioma",
        ["ErrorAutostart"] = "No se pudo actualizar la configuración de inicio automático",
        ["StartupRecoveryFailed"] = "No se pudo restaurar completamente una sesión de privacidad anterior.",
        ["StartupReconcileFailed"] = "No se pudo aplicar la protección deseada al iniciar el sistema.",
        ["ErrorShutdownRestore"] = "PrivGvard no pudo restaurar con seguridad todos los cambios. Se canceló la salida.",
        ["ErrorShutdownFailed"] = "Falló la restauración de cierre: {0}"
    };

    public static readonly IReadOnlyDictionary<string, string> StringsEn = new Dictionary<string, string>
    {
        ["AppTitle"] = "PrivGvard",
        ["AppSubtitle"] = "Camera & Microphone Blocker",

        // Sections
        ["CameraTitle"] = "Camera",
        ["CameraSubtitle"] = "Webcam and video capture protection",
        ["MicrophoneTitle"] = "Microphone",
        ["MicrophoneSubtitle"] = "Microphone and audio capture protection",

        // Status
        ["StatusBlocked"] = "🔒 Blocked",
        ["StatusAllowed"] = "✅ Allowed",

        // Standard Protection
        ["StandardProtectionTitle"] = "Standard Protection",
        ["StandardProtectionDesc"] = "Everyday protection without elevated permissions",
        ["EnableStandard"] = "Enable Standard",
        ["DisableStandard"] = "Disable Standard",
        ["StatusStandardActive"] = "● Standard Active",
        ["StatusStandardInactive"] = "○ Standard Inactive",

        // Secure Protection
        ["SecureProtectionTitle"] = "Secure Protection (Admin)",
        ["SecureProtectionDesc"] = "Physical isolation and low-level system policies",
        ["EnableSecure"] = "🔒 Enable Secure Protection",
        ["DisableSecure"] = "Disable Secure",
        ["StatusSecureUnavailable"] = "🔒 Unavailable (Enable standard first)",
        ["StatusSecureAvailable"] = "○ Available to enable",
        ["StatusSecureActive"] = "🛡️ Secure Active (Hardened)",
        ["SecureRequirementHint"] = "Enable Standard Protection first",

        // Devices
        ["DetectedDevices"] = "Detected Hardware Devices",
        ["DeviceEnabled"] = "Enabled",
        ["DeviceDisabled"] = "Disabled",

        // Settings & Footers
        ["StartWithSystem"] = "Start with system",
        ["Language"] = "Language",
        ["CapabilitiesTitle"] = "Platform Capabilities",

        // Unified Control
        ["UnifiedTitle"] = "Unified Control",
        ["UnifiedSubtitle"] = "Control camera and microphone simultaneously",
        ["UnifiedStandardDesc"] = "Protect both devices at once",
        ["UnifiedSecureDesc"] = "Harden protection on both devices",

        // Advanced Protection (Global Extension)
        ["AdvancedProtectionTitle"] = "Advanced Protection",
        ["AdvancedProtectionSubtitle"] = "Hardware isolation and system policies for blocked devices",
        ["AdvancedProtectionDesc"] = "Automatically applies hardware-level isolation to blocked devices",
        ["StatusAdvancedActive"] = "🛡️ Hardened (Advanced)",
        ["StatusAdvancedInactive"] = "○ Inactive",
        ["StatusBlockedAdvanced"] = "🛡️ Blocked (Advanced)",
        ["StatusBlockedStandard"] = "🔒 Blocked (Standard)",
        ["StatusUnblocked"] = "✅ Allowed",

        // Notifications & Errors
        ["ElevationCancelled"] = "Operation cancelled: Administrator permissions were denied.",
        ["StandardRequiredFirst"] = "You must enable Standard Protection before enabling Secure Protection.",

        // Settings Window Navigation
        ["SettingsWindowTitle"] = "Settings",
        ["SettingsHeaderTitle"] = "Settings",
        ["SettingsNavGeneral"] = "General",
        ["SettingsNavHelp"] = "Help & Documentation",
        ["SettingsNavDiagnostics"] = "System Diagnostics",
        ["SettingsNavAbout"] = "About PrivGvard",

        // Section: General Settings
        ["AppearanceTitle"] = "Appearance",
        ["ThemeLabel"] = "Interface Theme",
        ["ThemeDark"] = "Dark (Default)",
        ["ThemeDescription"] = "High-contrast dark theme optimized for the PrivGvard interface.",
        ["StartupTitle"] = "System Startup",
        ["StartupDescription"] = "Automatically start PrivGvard when logging into your computer.",
        ["AutostartStateUnknown"] = "The effective Windows startup state could not be verified.",
        ["AutostartDisabledByUser"] = "Windows has disabled PrivGvard startup. Enable it in Windows Settings or Task Manager.",
        ["AutostartDisabledByPolicy"] = "A Windows policy prevents PrivGvard from starting automatically.",
        ["AutostartMigrationFailed"] = "PrivGvard startup settings could not be migrated: {0}",
        ["LanguageTitle"] = "Interface Language",
        ["LanguageDescription"] = "Select the primary application language.",

        // Section: Help & Documentation
        ["HelpTitle"] = "Help & Documentation",
        ["HelpSubtitle"] = "Quick reference and operational guide for PrivGvard",
        ["HelpHowItWorksTitle"] = "How PrivGvard Works",
        ["HelpHowItWorksDesc"] = "On Windows, PrivGvard combines system privacy controls with device management. Results depend on the selected mode, hardware, driver, permissions, and configuration; the app verifies effective state when the system allows it.",
        ["HelpCameraTitle"] = "Camera Protection",
        ["HelpCameraDesc"] = "Depending on the mode and device, it can apply the Windows camera privacy policy and manage compatible device nodes. This does not physically disconnect the camera, and not every model or driver supports each measure.",
        ["HelpMicTitle"] = "Microphone Protection",
        ["HelpMicDesc"] = "Depending on the mode and device, it can apply the Windows microphone privacy policy, manage compatible devices, and mute audio capture endpoints. Muting an endpoint is not the same as physically disconnecting it.",
        ["HelpAdvancedTitle"] = "Advanced Protection",
        ["HelpAdvancedDesc"] = "Advanced protection can combine Windows privacy policies with PnP (Plug and Play) device management. Compatibility varies by device and driver, and some operations require administrator permissions.",
        ["HelpUnifiedTitle"] = "Unified Control",
        ["HelpUnifiedDesc"] = "Requests a camera and microphone change from one control. Each is processed and verified separately, so they can end up with different results if an operation fails or a device is unsupported.",
        ["HelpFaqTitle"] = "Frequently Asked Questions",
        ["HelpFaq1Q"] = "Are device blocks retained after rebooting?",
        ["HelpFaq1A"] = "When you exit normally, the app attempts to restore the changes it manages. At startup, it attempts to recover interrupted operations and reconcile the system with the saved desired state. The journal does not guarantee uninterrupted protection during reboot; check the status after signing in.",
        ["HelpFaq2Q"] = "Why does UAC appear, and what if I cancel?",
        ["HelpFaq2A"] = "Some Windows operations require administrator permissions and request authorization at that time. If you cancel the prompt, that operation fails and must not be treated as confirmed protection; check the status and error message.",
        ["HelpFaq3Q"] = "Is closing the window the same as choosing “Exit”?",
        ["HelpFaq3A"] = "When the system tray is available, closing the window or pressing Alt+F4 hides it while the app keeps running. “Exit” from the tray menu starts shutdown and attempts to restore changes managed by PrivGvard. If the tray is unavailable, closing the window starts coordinated shutdown.",
        ["HelpFaq4Q"] = "What should I do if a device is unsupported or an operation fails?",
        ["HelpFaq4A"] = "Refresh diagnostics and check the effective status, permissions, and driver; review the error message. A saved desired state does not confirm that protection was applied. If compatibility is unavailable or the state cannot be verified, do not treat the device as protected.",

        // Section: System Diagnostics
        ["DiagTitle"] = "System Diagnostics",
        ["DiagSubtitle"] = "Telemetry and operational privacy status",
        ["DiagOsLabel"] = "Operating System",
        ["DiagArchLabel"] = "Architecture",
        ["DiagPrivilegesLabel"] = "Privilege Level",
        ["DiagCamStatusLabel"] = "Camera Status",
        ["DiagMicStatusLabel"] = "Microphone Status",
        ["DiagAdvancedLabel"] = "Advanced Protection",
        ["DiagDevicesCountLabel"] = "Detected Devices",
        ["DiagCapabilitiesLabel"] = "Capability Level",
        ["DiagRefreshButton"] = "Refresh Diagnostics",

        // Section: About
        ["AboutTitle"] = "About PrivGvard",
        ["AboutVersionLabel"] = "Version",
        ["AboutDeveloperLabel"] = "Developer",
        ["AboutDeveloperValue"] = "cdev Studio",
        ["AboutDescription"] = "Hardware privacy and camera/microphone security solution for Windows.",
        ["AboutLicenseLabel"] = "License",
        ["AboutLicenseValue"] = "MIT License",
        ["AboutCopyright"] = "© 2026 cdev Studio. All rights reserved.",

        // Device Types & Labels
        ["DeviceTypeCamera"] = "Camera",
        ["DeviceTypeMicrophone"] = "Microphone",
        ["SummaryCameraLabel"] = "Cam",
        ["SummaryMicrophoneLabel"] = "Mic",

        // User & Privilege Levels
        ["UserStandard"] = "Standard User",
        ["UserAdmin"] = "Administrator",
        ["DiagPrivilegeStandard"] = "Standard User (asInvoker)",
        ["DiagPrivilegeAdmin"] = "Administrator (Elevated)",

        // Overall Badges & Protection Statuses
        ["BadgeHardened"] = "Hardened",
        ["BadgeProtected"] = "Protected",
        ["BadgePartiallyProtected"] = "Partially Protected",
        ["BadgeUnprotected"] = "Unprotected",
        ["StatusPartiallyBlocked"] = "🔒 Partially Blocked",
        ["StatusActive"] = "🛡️ Active",
        ["StatusInactive"] = "○ Inactive",

        // Canonical Semantic Statuses
        ["Enabled"] = "Enabled",
        ["Disabled"] = "Disabled",
        ["Protected"] = "Protected",
        ["Unprotected"] = "Unprotected",
        ["Active"] = "Active",
        ["Inactive"] = "Inactive",
        ["Allowed"] = "Allowed",
        ["Blocked"] = "Blocked",

        // Diagnostics Formatting & Hardware States
        ["DevicesCountFormat"] = "{0} devices detected in total",
        ["Yes"] = "Yes",
        ["No"] = "No",

        // Main Window Menu
        ["MenuToolTip"] = "Menu and Settings",
        ["MenuGeneral"] = "⚙️  General Settings",
        ["MenuHelp"] = "📖  Help & Documentation",
        ["MenuDiagnostics"] = "🛡️  System Diagnostics",
        ["MenuAbout"] = "ℹ️  About PrivGvard",

        // System Tray Menu & Tooltip
        ["TrayOpen"] = "Open PrivGvard",
        ["Tray.Open"] = "Open PrivGvard",
        ["TrayExit"] = "Exit",
        ["Tray.Exit"] = "Exit",
        ["Tray.ToolTip"] = "PrivGvard - Camera & Microphone Blocker",
        ["AppSubtitleLong"] = "PrivGvard - Camera & Microphone Blocker",

        // Common Error Fallbacks
        ["ErrorUpdateCamera"] = "Failed to update Camera protection",
        ["ErrorUpdateMic"] = "Failed to update Microphone protection",
        ["ErrorUpdateUnified"] = "Failed to update Unified protection",
        ["ErrorUpdateAdvanced"] = "Failed to update Advanced Protection",
        ["ErrorCameraSecure"] = "Camera Secure Protection error",
        ["ErrorMicSecure"] = "Microphone Secure Protection error",
        ["ErrorBothSecure"] = "Both Secure Protection error",
        ["ErrorLanguagePreference"] = "Failed to save language preference",
        ["ErrorAutostart"] = "Failed to update autostart setting",
        ["StartupRecoveryFailed"] = "A previous privacy session could not be fully restored.",
        ["StartupReconcileFailed"] = "Desired privacy protection could not be applied at startup.",
        ["ErrorShutdownRestore"] = "PrivGvard could not safely restore every owned change. Exit was cancelled.",
        ["ErrorShutdownFailed"] = "Shutdown restoration failed: {0}"
    };

    public static string Get(string key, string culture = "es", string fallback = "")
    {
        var dict = culture.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? StringsEn : StringsEs;
        if (dict.TryGetValue(key, out var val))
            return val;

        if (key.Equals("Tray.Open", StringComparison.OrdinalIgnoreCase) && dict.TryGetValue("TrayOpen", out val))
            return val;
        if (key.Equals("Tray.Exit", StringComparison.OrdinalIgnoreCase) && dict.TryGetValue("TrayExit", out val))
            return val;
        if ((key.Equals("Tray.ToolTip", StringComparison.OrdinalIgnoreCase) || key.Equals("TrayToolTip", StringComparison.OrdinalIgnoreCase))
            && dict.TryGetValue("AppSubtitleLong", out val))
            return val;

        return string.IsNullOrEmpty(fallback) ? key : fallback;
    }

    public static string GetString(string key, string culture = "es", string fallback = "") =>
        Get(key, culture, fallback);

    public static IReadOnlyDictionary<string, string> GetAll(string culture = "es") =>
        culture.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? StringsEn : StringsEs;
}

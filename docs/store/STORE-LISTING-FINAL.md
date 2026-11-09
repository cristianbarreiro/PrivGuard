# PrivGuard 2.0 — Final Microsoft Store Listing Content (`STORE-LISTING-FINAL.md`)

This document provides ready-to-copy text blocks in English (`en-us`) and Spanish (`es-es`), keyword listings, screenshot captions, and license disclosures for direct entry into **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard`).

---

## 1. Product Metadata Summary

| Field | Partner Center Value | Notes |
|---|---|---|
| **Product Title** | `PrivGuard` | Matches reserved product name |
| **Store Product ID** | `9NQ56N45XH5N` | Confirm in Partner Center dashboard |
| **Package Identity** | `cdevstudios.PrivGuard` | Verified in MSIX manifest |
| **Publisher** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` | cdev studios |
| **Version** | `2.0.0.0` | Semantic Version 2.0.0.0 |
| **Primary Category** | `Security` > `Personal security` | Or closest available in portal |
| **Secondary Category** | `Utilities & tools` | |
| **Pricing** | `Free` ($0.00) | Recommended commercial price |
| **Publishing Option** | `Manual Publishing (Hold)` | Strongly recommended to inspect Microsoft report before release |
| **License** | `GNU General Public License v3.0 (GPL-3.0)` | Open source repository license |

---

## 2. English Listing Content (`en-us`)

### Product Title
```text
PrivGuard
```

### Subtitle / Short Description (under 100 characters)
```text
Manage and verify native Windows camera and microphone privacy controls.
```

### Full Description (Copy & Paste)
```text
PrivGuard is a lightweight, local-first privacy utility designed to give you clear, transparent control over your microphone and camera privacy state on Windows 10 and Windows 11.

Many desktop applications can access your audio recording devices in the background. PrivGuard helps safeguard your privacy by controlling native Windows Core Audio capture endpoints and providing one-click access to system privacy settings—always verifying actual operating system status before reporting device state.

Core Privacy Features:
• Microphone Privacy Muting (Core Audio WASAPI): Silences active recording endpoints using official Windows Core Audio APIs (IAudioEndpointVolume). Designed for rapid, day-to-day privacy control without requiring administrative elevation or intrusive UAC prompts.
• Camera Privacy Navigation: Provides one-click access to Windows Camera Privacy Settings (ms-settings:privacy-webcam) so you can review and manage app permissions directly in Windows.
• Observable Status Verification: Inspects live endpoint status from Windows before displaying state, ensuring the interface reflects verified system observations.
• Standard User Execution: Executes purely as a standard user (asInvoker). Zero administrative elevation, zero kernel drivers, and zero system file tampering.
• System Tray Integration: Operates quietly in the notification area with minimal resource usage. The application stays accessible from the tray while keeping your workspace clean.
• Startup Integration: Optional launch on Windows logon minimized directly to the notification area.
• Pure Local Privacy: Operates completely offline with zero network transmission, zero telemetry, and zero remote analytics SDKs.

Important Limitations & Honest Privacy Disclosures:
• Microphone Muting Scope: Muting is performed via software at the Windows Core Audio endpoint level for shared-mode streams. Certain specialized hardware drivers or software utilizing exclusive access mode may bypass standard Windows endpoint muting. PrivGuard is not a physical hardware disconnect switch.
• Camera Control Scope: In this Microsoft Store edition, camera privacy is managed through native Windows Settings; PrivGuard does not manipulate camera hardware drivers directly.
• Local Data Processing: PrivGuard operates offline. It persists limited operational data strictly on your local PC (user preferences, a write-ahead recovery journal to restore audio state, rotating technical logs kept for up to 7 days, and structured local crash dumps). None of this data is transmitted off your machine.
• Not Antivirus Software: PrivGuard is a privacy convenience tool. It is not an antivirus, anti-spyware, or anti-malware system.
```

### Key Features (Bullet Points)
```text
• Microphone Privacy Muting: Silence active recording endpoints using native Windows Core Audio APIs.
• Live Observable State: Real-time status reporting based on verified Windows endpoint mute state.
• Camera Privacy Navigation: One-click shortcut to native Windows Camera Privacy Settings.
• Standard User Execution: Runs strictly as a standard user (asInvoker) with zero elevation prompts.
• System Tray Integration: Minimizes cleanly to the notification area with a dedicated tray menu.
• Windows Startup Support: Optional startup entry to launch minimized upon user logon.
• Zero Network Transmission: Operates completely offline with zero telemetry or tracking SDKs.
• High-DPI Aware: Crisp Fluent Dark interface built with Avalonia 11 supporting 100% to 400% display scaling.
```

### Search Keywords (7 Max)
```text
privacy guard, microphone mute, camera privacy, mic mute, privacy tool, device privacy, audio mute
```

---

## 3. Spanish Listing Content (`es-es`)

### Título del Producto
```text
PrivGuard
```

### Subtítulo / Descripción Corta (menos de 100 caracteres)
```text
Administra y verifica controles nativos de privacidad de cámara y micrófono en Windows.
```

### Descripción Completa (Copiar y Pegar)
```text
PrivGuard es una utilidad de privacidad ligera, nativa y de procesamiento local diseñada para ofrecerte un control claro y transparente sobre el estado de tu micrófono y cámara en Windows 10 y Windows 11.

Múltiples aplicaciones pueden acceder a tus dispositivos de captura de audio en segundo plano. PrivGuard te ayuda a proteger tu privacidad gestionando los puntos de captura de audio mediante Windows Core Audio y facilitando acceso inmediato a los ajustes de privacidad del sistema, verificando siempre el estado real del sistema operativo antes de reflejarlo en pantalla.

Funcionalidades Principales de Privacidad:
• Silenciado de Micrófono (Core Audio WASAPI): Silencia los puntos de grabación activos utilizando las APIs oficiales de Windows Core Audio (IAudioEndpointVolume). Diseñado para un control ágil y cotidiano de la privacidad sin requerir elevación de administrador ni molestos diálogos UAC.
• Acceso a Privacidad de Cámara: Ofrece acceso directo con un solo clic a la configuración de cámara de Windows (ms-settings:privacy-webcam) para revisar y administrar permisos de aplicaciones directamente en el sistema operativo.
• Verificación de Estado Observable: Consulta el estado real del punto de captura en Windows antes de mostrarlo, garantizando que la interfaz refleja observaciones verificadas.
• Ejecución como Usuario Estándar: Se ejecuta estrictamente con permisos estándar (asInvoker). Cero elevación de privilegios, cero controladores de kernel y cero alteración de subsistemas de seguridad.
• Integración con la Bandeja del Sistema (System Tray): Funciona discretamente en el área de notificación con un consumo mínimo de recursos, manteniendo despejado tu espacio de trabajo.
• Inicio Automático Opcional: Compatible con el inicio de sesión de Windows para arrancar minimizado en la bandeja del sistema.
• Privacidad Estrictamente Local: Funciona totalmente desconectado, sin transmisión de red, sin telemetría remota y sin bibliotecas analíticas de terceros.

Límites Técnicos y Transparencia de Privacidad:
• Alcance del Silenciado de Micrófono: El silenciado se aplica por software a nivel de punto de conexión de Windows Core Audio para flujos compartidos. Ciertos controladores especializados o aplicaciones con acceso en modo exclusivo podrían eludir el silenciado estándar. PrivGuard no es un interruptor físico de corte de hardware.
• Alcance del Control de Cámara: En esta edición de Microsoft Store, la privacidad de la cámara se gestiona derivando al usuario a la configuración nativa de Windows; la app no modifica controladores de cámara de bajo nivel.
• Datos Locales Procesados: PrivGuard opera sin conexión. Persiste datos operativos mínimos estrictamente en tu equipo local (preferencias de usuario, un diario de recuperación WAL para restablecer el audio, registros técnicos rotativos con retención máxima de 7 días y reportes de fallo locales). Ningún dato sale de tu equipo.
• No es Software Antivirus: PrivGuard es una utilidad de gestión de privacidad, no un antivirus, antispyware ni herramienta de desinfección de malware.
```

### Características Principales (Bullet Points)
```text
• Silenciado Rápido de Micrófono: Silencia dispositivos de grabación mediante APIs nativas de Windows Core Audio.
• Estado Observable en Tiempo Real: Comprobación basada en el estado real del punto de audio en Windows.
• Acceso Rápido a Ajustes de Cámara: Enlace directo a la configuración de privacidad de cámara de Windows.
• Ejecución con Mínimo Privilegio: Funciona estrictamente como usuario estándar (asInvoker) sin avisos UAC.
• Bandeja del Sistema (System Tray): Se minimiza discretamente al área de notificación con menú contextual completo.
• Inicio Automático con Windows: Permite iniciar minimizado al iniciar sesión en el equipo.
• Cero Transmisión de Red: Funciona 100% desconectado, sin telemetría ni recopilación remota de datos.
• Soporte para Alta Densidad (DPI): Interfaz moderna Fluent Dark en Avalonia 11 adaptable de 100% a 400%.
```

### Palabras Clave de Búsqueda (7 Máximo)
```text
privacidad microfono, silenciar microfono, privacidad camara, mute audio, control privacidad, privacidad windows, privguard
```

---

## 4. Screenshots and Captions / Capturas de Pantalla

Upload the 4 verified screenshots from `release/store/`:

| File | Resolution | Recommended English Caption | Pie de Foto Recomendado en Español |
|---|---|---|---|
| `release/store/main-window.png` | 593 x 625 | Main application window showing device status in Fluent Dark theme. | Ventana principal mostrando el estado de los dispositivos con tema Fluent Dark. |
| `release/store/mainwindow_dark.png` | 563 x 788 | Comprehensive view of PrivGuard interface with device status indicators. | Vista completa de la interfaz de PrivGuard con indicadores de estado de privacidad. |
| `release/store/protection_overview.png` | 563 x 356 | Protection status overview and one-click privacy actions. | Vista resumida del estado de protección y acciones de privacidad inmediatas. |
| `release/store/tray_menu.png` | 465 x 190 | System tray notification area context menu for background operation and clean exit. | Menú contextual de la bandeja del sistema para operación en segundo plano y salida limpia. |

### Store Listing App Tile Icon (1:1 300x300)
- **Asset:** `release/store/StoreListing-300x300.png`
- **Resolution:** 300 x 300 pixels (32bpp ARGB PNG)
- **Source:** Downscaled with high-quality bicubic resampling from the official 1024x1024 vector/raster master branding logo (`assets/logo/privgvard_logo.png`).
- **Portal Location:** Upload in Store Listings under **App tile icon** (recommended by Microsoft for Store search results and collections).

---

## 5. Software License Block

| Field | Canonical Text |
|---|---|
| **License Name** | GNU General Public License v3.0 (GPL-3.0) |
| **License URL** | `https://github.com/cristianbarreiro/PrivGuard/blob/main/LICENSE` |
| **Terms Summary** | PrivGuard is free and open-source software licensed under the GNU General Public License v3.0. You may run, inspect, share, and modify the software subject to the terms of the GPL-3.0. Full source code is available at https://github.com/cristianbarreiro/PrivGuard. |
| **Commercial Price** | Free ($0.00). Open source distribution on the Microsoft Store. |

---

## 6. Technical Justification for `runFullTrust`

Copy and paste this exact block when Partner Center prompts for restricted capability justification:

```text
Technical Justification for runFullTrust:
PrivGuard is a native .NET desktop privacy utility packaged via the Desktop Bridge (MSIX) for Windows 10 and Windows 11.
The runFullTrust capability is strictly required because the application interacts with native Windows COM interfaces and Win32 APIs that are unavailable within an AppContainer sandbox:
1. Windows Core Audio APIs (WASAPI IAudioEndpointVolume and IMMDeviceEnumerator) to inspect microphone volume states and apply software privacy muting.
2. Windows Management Instrumentation (WMI Win32_PnPEntity) to detect connected camera and microphone hardware and correlate friendly names.
3. Native Windows Notification Area (System Tray) lifecycle integration and custom Avalonia Fluent Dark desktop windowing.

In accordance with least-privilege principles (Architectural Decision D-01):
- The application executes exclusively as a standard user (requestedExecutionLevel="asInvoker").
- It does NOT declare allowElevation (no UAC prompts, no administrative helpers).
- It does NOT declare unvirtualizedResources (no registry virtualization overrides).
- Camera controls direct the user to Windows native Settings (ms-settings:privacy-webcam) rather than attempting elevated driver manipulation.
- The application operates 100% locally with zero telemetry, zero analytics, and zero network transmission.
```

---

## 7. Notes for Certification (Instructions for Microsoft Certification Testers)

Copy and paste this exact block into the **Notes for certification** field:

```text
PrivGuard Store Edition — Certification Reviewer Instructions

Application Architecture & Least-Privilege Design:
- Single-binary Win32 desktop application built with .NET 10 and packaged via MSIX Desktop Bridge.
- Runs purely as a standard unprivileged user (asInvoker). It does NOT trigger UAC elevation prompts and contains zero background services.
- Operates completely offline: no network connectivity is requested (internetClient is omitted), zero telemetry SDKs are bundled, and no remote requests are made.

How to Test the Core Functionality:
1. Launch PrivGuard from the Start Menu. The application opens with a Fluent Dark window showing Camera and Microphone sections.
2. Microphone Muting (Core Audio WASAPI):
   - Click the "Mute" button under Microphone.
   - PrivGuard toggles the Windows master mute switch for active audio recording endpoints using Windows Core Audio APIs.
   - Verify: Open Windows Sound Settings or check the microphone icon on the Windows taskbar; the recording endpoint will reflect the muted state.
   - Click "Unmute" to restore the microphone to its normal state.
3. Camera Privacy (Native Settings Navigation):
   - Click the Camera privacy button / link.
   - PrivGuard launches the native Windows Camera Privacy Settings page (ms-settings:privacy-webcam), allowing the user to configure system-level camera access permissions directly in Windows.
4. System Tray Lifecycle:
   - Closing the window (via the 'X' button or Alt+F4) minimizes the application to the Windows Notification Area (System Tray).
   - To fully exit the application, right-click the PrivGuard shield icon in the System Tray and select "Exit" (or "Salir"). This triggers a clean shutdown and verifies that previous audio mute states are cleanly restored.

Contact & Source Code:
- Repository: https://github.com/cristianbarreiro/PrivGuard
- Issue Tracker: https://github.com/cristianbarreiro/PrivGuard/issues
```

# PrivGvard 2.0 — Microsoft Store Listing Content (Ficha Comercial)

**Application Title:** PrivGvard  
**Subtitle:** Camera & Microphone Privacy Guard for Windows  
**Subtítulo (ES):** Bloqueador y Administrador de Privacidad de Cámara y Micrófono  
**Default Language:** English (United States) — `en-us`  
**Additional Language:** Spanish (Spain) — `es-es`  
**Category:** Utilities & tools  
**Subcategory:** Security & privacy  

---

## 1. Store Product Title & Subtitle

- **Product Title:** `PrivGvard`
- **Short Description / Subtitle (EN):** Protect your webcam and microphone from unauthorized access with native Windows controls.
- **Short Description / Subtitle (ES):** Protege tu cámara y micrófono de accesos no autorizados mediante controles nativos de Windows.

---

## 2. English Listing Content (`en-us`)

### Description
> PrivGvard is a lightweight, local-first privacy utility designed to give you clear control over your camera and microphone on Windows 10 and Windows 11.
>
> Many desktop and background applications can access your webcam or microphone without your knowledge. PrivGvard safeguards your privacy by managing native Windows software policies and hardware device states, ensuring that sensors remain blocked when you need privacy and can be safely restored when you are ready to make a call.
>
> **Dual-Layer Protection Architecture:**
> - **Standard Protection (No Admin Rights Needed):** Disables camera consent permissions for apps and mutes active microphone capture endpoints using official Windows Core Audio and Capability Access Manager APIs. Perfect for day-to-day privacy without elevation prompts.
> - **Secure Protection (Enhanced Hardware Lock):** Leverages Windows Configuration Manager (PnP) to disable device hardware nodes at the driver level and enforces system-wide Windows AppPrivacy Group Policies. Requests standard Windows User Account Control (UAC) elevation strictly on-demand.
>
> **Durable Recovery & Reliability:**
> PrivGvard features an atomic Write-Ahead Log (WAL) session journal. When you close the application or restart your PC, PrivGvard automatically and cleanly restores your devices to their original state. If a crash or power loss occurs, the startup reconciler safely restores hardware without leaving devices stranded.
>
> **Designed for Windows:**
> - Modern Fluent Dark interface with high contrast.
> - Runs quietly in the System Tray with zero background resource drain.
> - Optional Windows Autostart: starts minimized directly to the notification area upon logon.
> - **100% Local & Private:** No telemetry, no remote analytics, and zero video/audio streaming or recording.

### Key Features (Product Bullet Points)
- **One-Click Camera & Mic Privacy:** Block webcam video, mute microphone audio, or secure both simultaneously.
- **Two Protection Modes:** Standard mode for everyday use; Secure mode for deep driver-level PnP node isolation and Group Policies.
- **Atomic State Recovery:** Automatic rollback of device settings on app exit, with fail-safe recovery across system restarts.
- **Verified Effective State:** Shows live, confirmed hardware and policy status—never assumes a device is safe without verifying actual Windows state.
- **Windows Autostart Integration:** Integrates with Windows Startup Apps to launch minimized to the System Tray on logon.
- **Zero-Telemetry Policy:** Operates entirely offline; your data, device identifiers, and settings never leave your computer.
- **Multi-Monitor & DPI Aware:** Clean dark theme built with Avalonia 11, crisp on 100% to 400% display scaling.

### Search Keywords (7 Allowed Terms)
1. `privacy guard`
2. `webcam blocker`
3. `camera privacy`
4. `microphone mute`
5. `mic blocker`
6. `device security`
7. `privacy tool`

---

## 3. Spanish Listing Content (`es-es`)

### Descripción
> PrivGvard es una utilidad de privacidad nativa y de procesamiento local diseñada para darte control absoluto sobre tu cámara y micrófono en Windows 10 y Windows 11.
>
> Diversas aplicaciones y procesos en segundo plano pueden acceder a tu webcam o micrófono sin tu consentimiento explícito. PrivGvard protege tu privacidad administrando directivas de software y estados de dispositivos nativos de Windows, garantizando que los sensores permanezcan bloqueados cuando necesitas privacidad y se restauren de forma segura cuando desees utilizarlos.
>
> **Arquitectura de Protección en Dos Capas:**
> - **Protección Estándar (Sin Permisos de Administrador):** Bloquea los permisos de consentimiento de cámara para aplicaciones y silencia los puntos de captura de audio activos mediante las APIs oficiales de Windows Core Audio y Capability Access Manager. Ideal para privacidad diaria sin avisos de elevación.
> - **Protección Segura (Aislamiento Reforzado):** Emplea el Administrador de Configuración de Windows (PnP) para deshabilitar los nodos de hardware a nivel de controlador y aplica directivas de grupo de Windows (AppPrivacy). Solicita elevación UAC estándar exclusivamente bajo demanda.
>
> **Recuperación y Resiliencia Atómica:**
> PrivGvard incorpora un diario de sesión Write-Ahead Log (WAL). Al cerrar la aplicación o apagar el equipo, PrivGvard restaura automáticamente tus periféricos a su estado original exacto. Ante caídas imprevistas o cortes de energía, el reconciliador de arranque restaura el hardware de forma segura.
>
> **Optimizado para Windows:**
> - Interfaz Fluent Dark moderna de alto contraste.
> - Operación silenciosa en la bandeja del sistema (System Tray) con consumo mínimo de recursos.
> - Inicio automático opcional: inicia minimizado en la bandeja del sistema al iniciar sesión.
> - **100% Local y Privado:** Sin telemetría, sin analíticas remotas y sin captura ni transmisión de audio o video.

### Características Principales
- **Control Inmediato de Privacidad:** Bloquea la cámara, silencia el micrófono o protege ambos dispositivos a la vez.
- **Dos Niveles de Protección:** Modo Estándar para uso cotidiano; Modo Seguro para aislamiento PnP y directivas de sistema.
- **Recuperación Atómica de Estado:** Reversión automática del hardware al salir y recuperación segura tras reinicios.
- **Estado Efectivo Verificado:** Muestra el estado nativo real confirmado por Windows; nunca asume protección sin consultar el hardware.
- **Inicio Automático con Windows:** Compatible con Aplicaciones de Inicio de Windows para arrancar minimizado en el System Tray.
- **Política de Cero Telemetría:** Funciona 100% sin conexión; tus preferencias e identificadores jamás salen de tu equipo.
- **Adaptabilidad DPI:** Interfaz oscura nítida en factores de escala desde el 100% hasta el 400%.

### Palabras Clave de Búsqueda (7 Términos)
1. `bloquear camara`
2. `bloquear microfono`
3. `privacidad webcam`
4. `silenciar microfono`
5. `seguridad camara`
6. `privacidad windows`
7. `bloqueador dispositivos`

---

## 4. Visual Assets & Screenshot Specifications

Staged in `release/store/`:

| Asset File | Target Context | Required Dimensions | Format |
|---|---|---|:---:|
| `StoreLogo.png` | Store Listing / Search Icon | 50x50 px (Scales 100% to 400%) | PNG (Transparent) |
| `Square150x150Logo.png` | Medium Start Tile / App Detail | 150x150 px (Scales 100% to 400%) | PNG |
| `Wide310x150Logo.png` | Wide Tile | 310x150 px (Scales 100% to 200%) | PNG |
| `Square44x44Logo.png` | App List / Taskbar / Notification | 44x44 px (Target sizes 16 to 256) | PNG |
| `SplashScreen.png` | Package Startup Screen | 620x300 px (Scales 100% to 200%) | PNG |
| `mainwindow_dark.png` | Screenshot 1: Main Window Protection Overview | 1920x1080 px or 1366x768 px | PNG |
| `protection_overview.png` | Screenshot 2: Device Hardware Cards & Status | 1920x1080 px | PNG |
| `tray_menu.png` | Screenshot 3: System Tray Menu & Notification | 1920x1080 px | PNG |

---

## 5. System Requirements & Compatibility

- **Operating System:** Windows 10 (Version 2004, Build 19041 or higher) or Windows 11 (all versions: 21H2, 22H2, 23H2, 24H2, 25H2, 26H1).
- **Architecture:** x64 (AMD64 / Intel 64), ARM64 (supported via Windows 11 ARM64 emulation / native packaging).
- **Memory:** Minimum 512 MB RAM (Recommended: 1 GB RAM).
- **Storage:** ~120 MB available disk space for installation.
- **Hardware:** Compatible with built-in webcams, USB cameras, integrated microphones, and USB audio capture devices.

---

## 6. Copyright & Publisher Information

- **Publisher Name:** cdev studios
- **Legal Entity / Developer:** Cristian Barreiro
- **Copyright:** Copyright © 2026 cdev studios. All rights reserved.
- **License:** MIT License (Open Source)
- **Public URL:** [https://github.com/cristianbarreiro/PrivGuard](https://github.com/cristianbarreiro/PrivGuard)
- **Support Contact:** [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues)

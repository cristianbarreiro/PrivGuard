# PrivGuard 2.0 â€” Microsoft Store Listing Content (Ficha Comercial)

**Application Title:** PrivGuard
**Subtitle (EN):** Camera & Microphone Privacy Manager for Windows
**SubtÃ­tulo (ES):** Administrador y Bloqueador de Privacidad de CÃ¡mara y MicrÃ³fono
**Default Language:** English (United States) â€” `en-us`
**Additional Language:** Spanish (Spain) â€” `es-es`
**Category:** Security
**Subcategory:** Personal security
**Secondary Category:** Utilities & tools
**Target Architecture:** x64
**Supported OS:** Windows 10 (version 2004 / build 19041.0+) and Windows 11 (x64)

---

## 1. Store Product Title & Subtitle

- **Product Title:** `PrivGuard`
- **Short Description / Subtitle (EN):** Manage and verify native Windows camera and microphone privacy controls.
- **Short Description / Subtitle (ES):** Administra y verifica controles nativos de privacidad de cÃ¡mara y micrÃ³fono en Windows.

---

## 2. English Listing Content (`en-us`)

### Description
> PrivGuard is a lightweight, local-first privacy utility designed to give you clear, transparent control over your camera and microphone privacy on Windows 10 and Windows 11 (x64).
>
> Many desktop and background applications can access your webcam or microphone without your active awareness. PrivGuard helps safeguard your privacy by managing native Windows software policies, Core Audio endpoints, and hardware device statesâ€”always verifying observable effective status against the operating system before reporting protection.
>
> **Dual-Layer Protection Architecture:**
> - **Standard Protection (No Admin Rights Needed):** Mutes active microphone capture endpoints using the official Windows Core Audio COM API and adjusts camera consent settings. Designed for day-to-day privacy without elevation prompts.
> - **Secure Protection (Enhanced Hardware Node Isolation):** Leverages Windows Configuration Manager (PnP) APIs (`CfgMgr32.dll`) to disable hardware device nodes at the driver level and applies system-wide Windows AppPrivacy Group Policies (`HKLM`). Requests standard Windows User Account Control (UAC) elevation strictly on-demand.
>
> **Durable Recovery & Reliability:**
> PrivGuard features an atomic Write-Ahead Log (WAL) session journal. When you close the application or restart your PC, PrivGuard attempts to cleanly restore changes that it verifies it owns. If an unexpected power loss occurs, the startup reconciler safely checks device state upon relaunch without leaving hardware stranded.
>
> **Designed for Windows:**
> - Modern Fluent Dark interface with high contrast.
> - Runs quietly in the System Tray with minimal background resource usage.
> - Optional Windows Autostart: starts minimized directly to the notification area upon logon.
> - **100% Local & Private:** No telemetry, no remote analytics, and zero video/audio streaming or recording.
>
> *Important Note on System Behavior:* Protection outcomes and recovery depend on your specific hardware, device drivers, Windows version, permissions, and system policies. Secure Protection operations require user consent via the standard Windows UAC prompt.

### Key Features (Product Bullet Points)
- **Camera & Mic Privacy Controls:** Block webcam access, mute microphone capture, or apply both simultaneously.
- **Two Protection Modes:** Standard mode for everyday un-elevated privacy; Secure mode for driver-level PnP hardware node disabling and Group Policies.
- **Verified Effective State:** Shows confirmed, live hardware and policy statusâ€”never assumes a sensor is safe without verifying actual Windows state.
- **Atomic Session Journal:** Keeps a Write-Ahead Log to safely restore device settings on exit and recover state after unexpected restarts.
- **Least-Privilege Security:** The main window runs as a standard user (`asInvoker`). Administrative elevation is requested strictly on-demand via standard UAC.
- **Windows Autostart Integration:** Integrates with Windows Startup Apps to launch minimized to the System Tray on user logon.
- **Zero-Telemetry Policy:** Operates 100% offline; your device identifiers and preferences never leave your computer.
- **High-DPI Aware:** Clean Fluent Dark interface built with Avalonia 11, crisp across 100% to 400% display scaling.

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

### DescripciÃ³n
> PrivGuard es una utilidad de privacidad nativa y de procesamiento local diseÃ±ada para darte un control claro y transparente sobre el estado de privacidad de tu cÃ¡mara y micrÃ³fono en Windows 10 y Windows 11 (x64).
>
> Diversas aplicaciones y procesos en segundo plano pueden acceder a tu webcam o micrÃ³fono sin tu consentimiento activo. PrivGuard te ayuda a proteger tu privacidad administrando directivas de software, puntos de captura de audio y estados de dispositivos nativos de Windows, verificando siempre la observaciÃ³n nativa efectiva antes de mostrar el estado protegido.
>
> **Arquitectura de ProtecciÃ³n en Dos Capas:**
> - **ProtecciÃ³n EstÃ¡ndar (Sin Permisos de Administrador):** Silencia los puntos de captura de audio activos mediante la API oficial de Windows Core Audio y gestiona directivas de consentimiento. Ideal para privacidad diaria sin avisos de elevaciÃ³n.
> - **ProtecciÃ³n Segura (Aislamiento a Nivel de Controlador):** Utiliza el Administrador de ConfiguraciÃ³n de Windows (PnP) mediante `CfgMgr32.dll` para deshabilitar los nodos de hardware de los dispositivos y aplica directivas de grupo del equipo (`HKLM`). Solicita elevaciÃ³n UAC estÃ¡ndar exclusivamente bajo demanda.
>
> **RecuperaciÃ³n y Resiliencia AtÃ³mica:**
> PrivGuard incorpora un diario de sesiÃ³n Write-Ahead Log (WAL). Al cerrar la aplicaciÃ³n o reiniciar el equipo, PrivGuard intenta restaurar de forma limpia los cambios que puede verificar que le pertenecen. Ante un corte de energÃ­a o fallo imprevisto, el reconciliador de inicio verifica y recupera el estado de los perifÃ©ricos.
>
> **Optimizado para Windows:**
> - Interfaz Fluent Dark moderna de alto contraste.
> - OperaciÃ³n silenciosa en la bandeja del sistema (System Tray) con consumo mÃ­nimo de recursos.
> - Inicio automÃ¡tico opcional: inicia minimizado en la bandeja del sistema al iniciar sesiÃ³n en Windows.
> - **100% Local y Privado:** Sin telemetrÃ­a, sin analÃ­ticas remotas y sin captura ni transmisiÃ³n de audio o video.
>
> *Nota sobre el Comportamiento del Sistema:* La efectividad de la protecciÃ³n y la reversibilidad dependen del hardware especÃ­fico, controladores, ediciÃ³n de Windows, permisos y directivas del sistema. Las operaciones de ProtecciÃ³n Segura requieren confirmaciÃ³n explÃ­cita mediante el diÃ¡logo UAC de Windows.

### CaracterÃ­sticas Principales
- **Control de Privacidad de CÃ¡mara y MicrÃ³fono:** Bloquea el acceso a la webcam, silencia micrÃ³fonos o asegura ambos a la vez.
- **Dos Niveles de ProtecciÃ³n:** Modo EstÃ¡ndar para uso cotidiano sin permisos elevados; Modo Seguro para deshabilitaciÃ³n de nodos PnP y directivas de grupo.
- **Estado Efectivo Verificado:** Muestra el estado real y confirmado por el sistema operativo, sin asumir que un dispositivo estÃ¡ protegido sin observaciÃ³n nativa.
- **Diario de SesiÃ³n AtÃ³mico:** Registro Write-Ahead Log (WAL) para restaurar la configuraciÃ³n al salir y recuperar el estado tras reinicios.
- **MÃ­nimo Privilegio:** La aplicaciÃ³n principal se ejecuta como usuario estÃ¡ndar (`asInvoker`). La elevaciÃ³n administrativa se solicita Ãºnicamente bajo demanda mediante UAC.
- **IntegraciÃ³n con Inicio de Windows:** Compatible con Aplicaciones de Inicio para iniciar minimizado en la bandeja del sistema.
- **Cero TelemetrÃ­a:** Funciona 100% desconectado; tus datos, identificadores y preferencias nunca salen de tu equipo.
- **Adaptable a Pantallas de Alta Densidad (DPI):** Interfaz Fluent Dark nÃ­tida en factores de escala del 100% al 400%.

### Palabras Clave de BÃºsqueda (7 TÃ©rminos Permitidos)
1. `privacidad camara`
2. `bloqueador microfono`
3. `bloquear webcam`
4. `seguridad privacidad`
5. `silenciar microfono`
6. `privacidad windows`
7. `control camara`

---

## 4. Submission Notes & Visual Assets Checklist

- **Store Logo / App Tile:** `release\store\StoreLogo.png` (300x300, 50KB)
- **Square 150x150 Logo:** `release\store\Square150x150Logo.png`
- **Square 44x44 Logo:** `release\store\Square44x44Logo.png`
- **Splash Screen:** `release\store\SplashScreen.png`
- **Desktop Screenshots (Minimum 1, Recommended 3):**
  1. `release\store\mainwindow_dark.png` (Main window Fluent Dark interface)
  2. `release\store\protection_overview.png` (Active protection indicators)
  3. `release\store\tray_menu.png` (System Tray background operation)

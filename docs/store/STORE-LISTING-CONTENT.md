# PrivGuard 2.0 — Microsoft Store Listing Content (Ficha Comercial)

**Application Title:** PrivGuard
**Subtitle (EN):** Camera & Microphone Privacy Manager for Windows
**Subtítulo (ES):** Administrador de Privacidad de Cámara y Micrófono para Windows
**Default Language:** English (United States) — `en-us`
**Additional Language:** Spanish (Spain) — `es-es`
**Category:** Security
**Subcategory:** Personal security
**Secondary Category:** Utilities & tools
**Target Architecture:** x64
**Supported OS:** Windows 10 (version 2004 / build 19041.0+) and Windows 11 (x64)

---

## 1. Store Product Title & Subtitle

- **Product Title:** `PrivGuard`
- **Short Description / Subtitle (EN):** Manage and verify native Windows camera and microphone privacy controls.
- **Short Description / Subtitle (ES):** Administra y verifica controles nativos de privacidad de cámara y micrófono en Windows.

---

## 2. English Listing Content (`en-us`)

### Description
> PrivGuard is a lightweight, local-first privacy utility designed to give you clear, transparent control over your microphone and camera privacy state on Windows 10 and Windows 11.
>
> Many desktop and background applications can access your audio capture devices without your active awareness. PrivGuard helps safeguard your privacy by managing native Windows Core Audio capture endpoints and providing one-click access to system privacy settings—always verifying observable effective status before reporting device state.
>
> **Core Privacy Capabilities:**
> - **Microphone Privacy Mute (Core Audio WASAPI):** Silences active microphone recording endpoints using the official Windows Core Audio API (`IAudioEndpointVolume`). Designed for convenient, day-to-day privacy protection without requiring administrative elevation or intrusive prompts.
> - **System Privacy Integration:** Provides direct navigation to Windows Privacy & Security settings (`ms-settings:privacy-webcam` and `ms-settings:privacy-microphone`), making it easy to review and manage app-level access permissions.
> - **Observable Status Verification:** Inspects live endpoint status from Windows before displaying state, ensuring the interface reflects actual operating system observations.
>
> **Designed for Windows:**
> - Clean Fluent Dark interface with high contrast and smooth responsiveness.
> - Runs quietly in the Windows Notification Area (System Tray) with minimal background resource usage.
> - Optional Windows Startup integration: starts minimized directly to the notification area upon user logon.
> - **100% Local & Private:** No telemetry, no remote analytics, and zero video/audio streaming or recording.
>
> *Important Note on Audio Subsystem Behavior:* Muting operates at the Windows Audio subsystem level for active shared-mode audio streams. The muting mechanism and availability depend on your audio hardware and manufacturer drivers. Flujos in exclusive mode or specialized audio equipment may interact differently depending on whether the driver implements hardware mute controls. Advanced driver-level PnP node management is part of the open-source Standalone distribution.

### Key Features (Product Bullet Points)
- **Microphone Privacy Muting:** Quickly silence active recording endpoints using native Windows Core Audio APIs.
- **Observable Status Verification:** Live reporting based on actual Windows endpoint volume and mute state.
- **Quick Privacy Settings Access:** One-click shortcuts to Windows Camera and Microphone system privacy settings.
- **Standard User Execution:** Runs completely as a standard user (`asInvoker`) with zero administrative elevation prompts.
- **System Tray Integration:** Minimizes cleanly to the notification area to keep your desktop uncluttered.
- **Windows Autostart Integration:** Compatible with Windows Startup Apps to launch minimized on logon.
- **Zero-Telemetry Policy:** Operates 100% offline; your device identifiers and preferences never leave your PC.
- **High-DPI Aware:** Crisp Fluent Dark interface built with Avalonia 11, supporting scaling from 100% to 400%.

### Search Keywords (7 Allowed Terms)
1. `privacy guard`
2. `microphone mute`
3. `camera privacy`
4. `mic mute`
5. `privacy tool`
6. `device privacy`
7. `audio mute`

---

## 3. Spanish Listing Content (`es-es`)

### Descripción
> PrivGuard es una utilidad de privacidad nativa y de procesamiento local diseñada para ofrecerte un control claro y transparente sobre el estado de privacidad de tu micrófono y cámara en Windows 10 y Windows 11.
>
> Diversas aplicaciones en segundo plano pueden acceder a tus dispositivos de captura de audio sin tu conocimiento activo. PrivGuard te ayuda a proteger tu privacidad gestionando los puntos de captura de audio mediante Windows Core Audio y ofreciendo acceso directo a la configuración nativa de privacidad del sistema, verificando siempre el estado efectivo observado antes de mostrarlo en pantalla.
>
> **Capacidades Principales de Privacidad:**
> - **Silenciado de Micrófono (Core Audio WASAPI):** Silencia los puntos de captura de audio activos utilizando la API oficial de Windows Core Audio (`IAudioEndpointVolume`). Diseñado para un uso diario ágil y sin necesidad de permisos de administrador ni diálogos UAC.
> - **Integración con Ajustes de Privacidad del Sistema:** Proporciona accesos directos inmediatos a las páginas de privacidad y seguridad de Windows (`ms-settings:privacy-webcam` y `ms-settings:privacy-microphone`) para revisar y gestionar los permisos de las aplicaciones.
> - **Verificación de Estado Observable:** Consulta y valida el estado real del sistema operativo antes de reflejar el estado en la interfaz, garantizando información verídica y comprobada.
>
> **Optimizado para Windows:**
> - Interfaz Fluent Dark moderna de alto contraste y diseño adaptable.
> - Operación silenciosa en el área de notificación (System Tray) con consumo mínimo de recursos.
> - Inicio automático opcional con Windows: arranca minimizado en la bandeja del sistema al iniciar sesión.
> - **100% Local y Privado:** Sin telemetría, sin analíticas remotas y sin captura ni transmisión de audio o video.
>
> *Nota sobre el Comportamiento del Subsistema de Audio:* El silenciado actúa a nivel del motor de audio de Windows para flujos en modo compartido. La efectividad y el mecanismo de mute dependen del hardware y controladores instalados. Aplicaciones con flujos en modo exclusivo o equipamiento especializado de sonido pueden tener un comportamiento distinto si el controlador no implementa control de mute por hardware. Las funciones avanzadas de aislamiento de controladores PnP forman parte de la edición Standalone de código abierto.

### Características Principales
- **Silenciado Rápido de Micrófono:** Silencia puntos de captura de audio activos mediante la API nativa Core Audio.
- **Estado Efectivo Verificado:** Comprueba el estado real del audio en Windows sin asumir estados no verificados.
- **Acceso Rápido a Ajustes de Windows:** Enlaces directos a las opciones de privacidad de cámara y micrófono del sistema.
- **Mínimo Privilegio:** Se ejecuta completamente como usuario estándar (`asInvoker`) sin avisos de elevación administrativa.
- **Integración con Bandeja del Sistema:** Se minimiza discretamente al área de notificación para no saturar tu escritorio.
- **Inicio Automático con Windows:** Compatible con Aplicaciones de Inicio para arrancar minimizado al iniciar sesión.
- **Cero Telemetría:** Funciona 100% desconectado; tus preferencias e identificadores jamás salen de tu equipo.
- **Compatible con Pantallas de Alta Densidad (DPI):** Interfaz nítida Fluent Dark construida con Avalonia 11 (escalado 100% al 400%).

### Palabras Clave de Búsqueda (7 Términos Permitidos)
1. `privacidad microfono`
2. `silenciar microfono`
3. `privacidad camara`
4. `mute audio`
5. `control privacidad`
6. `seguridad microfono`
7. `privacidad windows`

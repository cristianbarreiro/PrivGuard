# PrivGuard — Privacy Policy / Política de Privacidad (Final — Store Edition)

**Effective Date / Fecha de Entrada en Vigor:** November 2026 / Noviembre de 2026  
**Application / Aplicación:** PrivGuard 2.0 (Store Edition)  
**Package Identity:** `cdevstudios.PrivGuard` (`cdevstudios.PrivGuard_tv7b4yqg10sw2`)  
**Publisher:** cdev studios (`CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`)  
**License:** GNU General Public License v3.0 (GPL-3.0)  
**Public Support Channel (Subject to Owner Approval):** https://github.com/cristianbarreiro/PrivGuard/issues  

---

## 1. English Version (`en-us`)

### Summary & Core Commitment
PrivGuard Store Edition is an offline, local-first desktop privacy utility for Windows 10 and Windows 11. It helps users manage microphone muting via native Windows Core Audio APIs and provides direct access to Windows Camera Privacy Settings. 

**PrivGuard does not transmit data over the network, does not collect personal identity information, does not record audio or video streams, and does not include any third-party analytics or telemetry SDKs.**

PrivGuard does persist limited operational and diagnostic data **strictly on your local device** to maintain your settings, protect audio state across restarts, coordinate edition execution, and allow local troubleshooting of technical errors.

---

### 1. Audio and Video Processing Boundaries
- **Zero Audio Stream Recording or Buffering:** PrivGuard interacts exclusively with the Windows Core Audio endpoint volume interface (`IAudioEndpointVolume` via WASAPI) to query and toggle the master software mute switch on active recording endpoints. PrivGuard never creates audio capture clients, never reads audio buffer samples, and never records, stores, or transmits voice or ambient sounds.
- **Microphone Driver & Exclusive Mode Limitation:** The microphone muting performed by PrivGuard operates at the Windows Core Audio software endpoint level. Specialized hardware drivers or applications operating in exclusive stream mode may bypass standard Windows endpoint muting. PrivGuard is a software privacy utility, not a hardware-level physical disconnect switch.
- **Zero Camera Stream Capture:** PrivGuard Store Edition does not access, intercept, render, or capture video frames. For webcam privacy, PrivGuard launches the native Windows Camera Privacy Settings page (`ms-settings:privacy-webcam`), allowing the user to configure system-level camera access permissions directly in Windows.
- **Not Security or Antivirus Software:** PrivGuard is a privacy convenience utility. It is not an antivirus, anti-spyware, or anti-malware system, and does not monitor running processes for malicious activity.

---

### 2. Network Transmission & Telemetry Audit
- **Zero Network Transmission:** PrivGuard does not initiate, listen for, or transmit data over local networks or the Internet. 
- **Package Manifest Isolation:** The MSIX package manifest (`AppxManifest.xml`) explicitly omits the `internetClient` and `internetClientServer` network capabilities.
- **No Third-Party Telemetry:** PrivGuard contains zero analytics libraries, crash-upload SDKs (such as Sentry or AppCenter), advertisement trackers, or remote telemetry frameworks.
- **No Automatic Updates or Version Checks:** PrivGuard Store Edition relies entirely on the Microsoft Store client for package distribution and updates; it performs no independent remote update queries.

---

### 3. Local Operational Data Inventory
PrivGuard persists limited operational data strictly within your local Windows user profile directory (`%LOCALAPPDATA%\PrivGuard\Store\`):

| Artifact / File | Relative Location | Exact Data Fields Stored | Purpose | Retention Policy | Leaves Device? |
|---|---|---|---|---|---|
| **Preferences State** (`state.json`) | `%LOCALAPPDATA%\PrivGuard\Store\state.json` | Desired microphone/camera states (`cameraStandard`, `cameraSecure`, `microphoneStandard`, `microphoneSecure`), UI language (`language`: "en" / "es"), autostart preference (`autostart`), advanced protection flag (`advancedProtectionEnabled`). | Preserves user preferences and requested privacy state across application launches. | Persists until modified by the user or deleted manually. | **No.** Stored locally only. |
| **Recovery Journal (WAL)** (`privacy-session-v1.json`) | `%LOCALAPPDATA%\PrivGuard\Store\Recovery\privacy-session-v1.json` | Session metadata (`schemaVersion`, `sessionId`, `createdAtUtc`, `updatedAtUtc`, `completedAtUtc`, `ownerProcessId`, `isActive`, `wasRestored`, `status`, `lastOperationId`) and audio resource state (`resourceId`, `operationId`, `endpointId`, `originalMutedState`, `protectedMutedState`, `journalState`, `restoreAttempts`, `lastError`). | Durable write-ahead recovery journal ensuring previous microphone mute states can be restored reliably upon application shutdown or crash recovery. | Retained until all modified resources are restored or unblocked; updated during runtime. | **No.** Stored locally only. |
| **Coordination Latch** (`cross-edition-latch.json`) | `%LOCALAPPDATA%\PrivGuard\Store\cross-edition-latch.json` | Coordination descriptor (`schemaVersion`, `ownerEdition`, `userSid`, `processId`, `processStartTimeUtc`, `sessionId`, `acquiredAtUtc`, `lastHeartbeatUtc`, `operationStatus`). | Local user-scoped descriptor preventing concurrency conflicts between Store and Standalone editions on the same PC. | Created when active session starts; removed on clean exit. Reconciled on startup. | **No.** Stored locally only. |
| **Diagnostic Logs** (`PrivGvard-*.log`) | `%LOCALAPPDATA%\PrivGuard\Store\Logs\` | Technical timestamp, log severity, process ID, thread ID, source component, technical event description, and exceptions; startup banner logs OS description, CPU architecture, .NET runtime version, and log directory path. | Enables local diagnosis and troubleshooting of audio device enumeration or state synchronization failures. | Rolling daily text files retained up to a maximum of **7 days** (`retainedFileCountLimit: 7`) with a 10 MB per-file cap. | **No.** Stored locally only. |
| **Crash Reports** (`crash-*.json`) | `%LOCALAPPDATA%\PrivGuard\Store\CrashReports\` | UTC timestamp, source context, exception type, exception message, stack trace, inner exception details, and environment metadata (OS description, architecture, runtime version, process ID, 64-bit flags). | Provides structured diagnostic dumps when an unhandled software exception occurs. | Retained locally on disk until manually inspected and deleted by the user. | **No.** Stored locally only. |

---

### 4. Data Retention, Uninstallation & User Deletion Instructions
- **Custom Directory Retention:** Because PrivGuard Store Edition stores its operational files in `%LOCALAPPDATA%\PrivGuard\Store\` rather than the Windows-managed package sandbox folder (`%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState`), the Windows MSIX uninstaller **may not automatically delete** this folder when the app is uninstalled.
- **Safe Uninstallation Sequence:**
  1. Open PrivGuard and ensure the microphone is **unmuted** (allowed) if you wish to restore default Windows audio behavior.
  2. Right-click the PrivGuard System Tray icon and select **Exit / Salir** to allow the recovery journal to close cleanly.
  3. Uninstall PrivGuard via Windows Settings (`Settings > Apps > Installed apps > PrivGuard > Uninstall`).
  4. To completely remove all remaining local operational data, logs, and crash reports, manually delete the folder:  
     `%LOCALAPPDATA%\PrivGuard\Store\`  
     (Press `Win + R`, paste `%LOCALAPPDATA%\PrivGuard\Store`, press Enter, and delete the contents).

---

### 5. Third-Party Services and External Links
PrivGuard contains no advertising services, tracking cookies, or commercial integrations. The only external link provided within the application is a direct navigation link to the open-source GitHub repository and issue tracker for support purposes.

---

### 6. Contact and Support
For privacy questions, issue reports, or support, please submit an issue to the public issue tracker:
- **Repository Support:** https://github.com/cristianbarreiro/PrivGuard/issues
- **Publisher:** cdev studios

*(Note: The contact channel is subject to final review and approval by the application owner).*

---
---

## 2. Versión en Español (`es-es`)

### Resumen y Compromiso Fundamental
PrivGuard Store Edition es una utilidad de escritorio local y desconectada para Windows 10 y Windows 11. Permite gestionar el silenciado del micrófono mediante las APIs nativas de Windows Core Audio y ofrece acceso directo a la configuración nativa de privacidad de cámara de Windows.

**PrivGuard no transmite datos a través de la red, no recopila datos de identidad personal, no graba flujos de audio ni secuencias de video, y no incluye bibliotecas analíticas o de telemetría de terceros.**

PrivGuard almacena datos operativos y de diagnóstico limitados **exclusivamente en tu equipo local** con el fin de recordar tus preferencias, preservar el estado del audio entre reinicios, coordinar la ejecución entre ediciones y facilitar el diagnóstico de errores técnicos en local.

---

### 1. Alcance y Límites del Procesamiento de Audio y Video
- **Cero Grabación o Almacenamiento de Audio:** PrivGuard interactúa exclusivamente con la interfaz de volumen de Windows Core Audio (`IAudioEndpointVolume` mediante WASAPI) para consultar y conmutar el interruptor maestro de silenciado en los dispositivos de grabación activos. PrivGuard jamás inicializa clientes de captura de sonido, no lee muestras de búfer de audio y no graba, almacena ni transmite voz o sonido ambiente.
- **Límites de Controladores y Modo Exclusivo:** El silenciado aplicado por PrivGuard opera a nivel de software en los puntos finales de Windows Core Audio. Ciertos controladores de hardware especializados o aplicaciones configuradas en modo de acceso exclusivo podrían ignorar el silenciado estándar de Windows. PrivGuard es una utilidad de privacidad por software, no un interruptor de desconexión física.
- **Cero Captura de Cámara:** PrivGuard Store Edition no accede, no intercepta ni captura imágenes o secuencias de cámara. Para la privacidad de la cámara, PrivGuard abre la página nativa de configuración de Windows (`ms-settings:privacy-webcam`), permitiendo al usuario configurar directamente los permisos del sistema operativo.
- **No es Software de Seguridad ni Antivirus:** PrivGuard es una herramienta de privacidad y conveniencia. No constituye un software antivirus, antispyware ni antimalware, y no monitoriza procesos del sistema en busca de amenazas.

---

### 2. Auditoría de Red y Telemetría
- **Cero Transmisión de Red:** PrivGuard no inicia, no escucha ni transmite datos por redes locales ni por Internet.
- **Aislamiento en Manifiesto:** El manifiesto del paquete MSIX (`AppxManifest.xml`) omite expresamente las capacidades de red `internetClient` e `internetClientServer`.
- **Cero Telemetría de Terceros:** PrivGuard no contiene bibliotecas de analítica, herramientas de subida de fallos (como Sentry o AppCenter), rastreadores de publicidad ni módulos de telemetría remota.
- **Sin Comprobación Remota de Actualizaciones:** PrivGuard Store Edition depende enteramente de Microsoft Store para su distribución y actualización; no efectúa consultas remotas independientes.

---

### 3. Inventario de Datos Operativos Locales
PrivGuard almacena datos técnicos y de preferencia estrictamente dentro del directorio de perfil del usuario local (`%LOCALAPPDATA%\PrivGuard\Store\`):

| Artefacto / Archivo | Ubicación Relativa | Campos de Datos Almacenados | Finalidad Técnica | Política de Retención | ¿Sale del Dispositivo? |
|---|---|---|---|---|---|
| **Estado y Preferencias** (`state.json`) | `%LOCALAPPDATA%\PrivGuard\Store\state.json` | Estados deseados (`cameraStandard`, `cameraSecure`, `microphoneStandard`, `microphoneSecure`), idioma de interfaz (`language`: "es" / "en"), preferencia de autostart (`autostart`), bandera de protección avanzada (`advancedProtectionEnabled`). | Conserva la configuración elegida por el usuario entre ejecuciones del programa. | Se conserva hasta que el usuario lo modifique o elimine la carpeta manualmente. | **No.** Estrictamente local. |
| **Diario de Sesión (WAL)** (`privacy-session-v1.json`) | `%LOCALAPPDATA%\PrivGuard\Store\Recovery\privacy-session-v1.json` | Metadatos de sesión (`schemaVersion`, `sessionId`, `createdAtUtc`, `updatedAtUtc`, `completedAtUtc`, `ownerProcessId`, `isActive`, `wasRestored`, `status`, `lastOperationId`) y estado del recurso de audio (`resourceId`, `operationId`, `endpointId`, `originalMutedState`, `protectedMutedState`, `journalState`, `restoreAttempts`, `lastError`). | Registro de recuperación duradera (WAL) que garantiza la reversión y restauración exacta de los estados previos de silenciado de audio al salir o tras un fallo. | Se conserva mientras existan recursos pendientes de restaurar; se actualiza en tiempo de ejecución. | **No.** Estrictamente local. |
| **Descriptor de Coordinación** (`cross-edition-latch.json`) | `%LOCALAPPDATA%\PrivGuard\Store\cross-edition-latch.json` | Descriptor (`schemaVersion`, `ownerEdition`, `userSid`, `processId`, `processStartTimeUtc`, `sessionId`, `acquiredAtUtc`, `lastHeartbeatUtc`, `operationStatus`). | Coordinación local por usuario para impedir colisiones o modificaciones simultáneas entre la edición Store y Standalone en un mismo equipo. | Se genera al iniciar la sesión y se elimina al salir limpiamente. Se reconcilia en el arranque. | **No.** Estrictamente local. |
| **Registros Locales (Logs)** (`PrivGvard-*.log`) | `%LOCALAPPDATA%\PrivGuard\Store\Logs\` | Marca de tiempo, nivel de log, PID, ID de subproceso, contexto del componente, mensaje del evento y excepciones; cabecera inicial incluye versión del SO, arquitectura de CPU, versión de .NET y ruta de logs. | Diagnóstico técnico local ante problemas de detección de audio o sincronización de estados. | Archivos diarios rotativos con retención máxima de **7 días** (`retainedFileCountLimit: 7`) y tope de 10 MB por archivo. | **No.** Estrictamente local. |
| **Reportes de Caída** (`crash-*.json`) | `%LOCALAPPDATA%\PrivGuard\Store\CrashReports\` | Marca de tiempo UTC, contexto de origen, tipo de excepción, mensaje, traza de pila (stack trace), detalles de excepción interna e información de entorno (descripción del SO, arquitectura, versión de runtime, PID, banderas de 64 bits). | Volcado de diagnóstico estructurado ante excepciones no controladas. | Se conserva en disco local hasta su eliminación manual por parte del usuario. | **No.** Estrictamente local. |

---

### 4. Retención, Desinstalación y Eliminación por el Usuario
- **Retención en Directorio Personalizado:** Debido a que PrivGuard Store Edition utiliza la carpeta `%LOCALAPPDATA%\PrivGuard\Store\` y no el contenedor estándar del sandbox de paquete (`%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState`), el desinstalador de MSIX de Windows **podría no eliminar automáticamente** esta carpeta al desinstalar.
- **Instrucciones para Desinstalación Limpia:**
  1. Abre PrivGuard y asegúrate de que el micrófono esté **desilenciado** (permitido) si deseas dejar el sistema en su estado habitual.
  2. Haz clic derecho en el icono de PrivGuard en la bandeja del sistema (System Tray) y selecciona **Salir** para asegurar el cierre limpio del diario de recuperación.
  3. Desinstala PrivGuard desde Configuración de Windows (`Configuración > Aplicaciones > Aplicaciones instaladas > PrivGuard > Desinstalar`).
  4. Para eliminar de forma total y manual todos los datos residuales, logs y reportes de fallo, borra la carpeta:  
     `%LOCALAPPDATA%\PrivGuard\Store\`  
     (Pulsa `Win + R`, introduce `%LOCALAPPDATA%\PrivGuard\Store`, pulsa Enter y elimina su contenido).

---

### 5. Servicios de Terceros y Enlaces Externos
PrivGuard no integra publicidad, cookies ni servicios comerciales externos. El único enlace presente en la interfaz dirige al repositorio oficial de código abierto en GitHub para soporte técnico.

---

### 6. Canal de Contacto y Soporte
Para consultas sobre esta política de privacidad, incidencias técnicas o soporte:
- **Incidencias y Soporte en GitHub:** https://github.com/cristianbarreiro/PrivGuard/issues
- **Publicador:** cdev studios

*(Nota: Este canal de contacto está sujeto a la validación y autorización final del propietario de la aplicación).*

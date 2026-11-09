# PrivGuard 2.0 — Store Listing & Visual Assets Review (Fase 8B.1E)

Este documento audita y consolida la revisión técnica de los activos visuales, las fichas comerciales (inglés y español) y las declaraciones de capacidades para la publicación de PrivGuard Store Edition en Microsoft Store.

---

## 1. Matriz de Activos Gráficos y Empaquetado

Todos los activos han sido auditados directamente en disco a partir de sus dimensiones reales y formatos:

| Ruta del Activo | Tipo de Activo | Dimensiones (px) | Relación de Aspecto | Formato / Tamaño | Estado en Partner Center | Requisito que Cubre |
|---|---|---|---|---|---|---|
| `release/store/StoreLogo.png` | Store Logo (Ficha) | 50 x 50 | 1:1 | PNG (5.688 B) | **VALID** | Icono de tienda / listado básico |
| `release/store/Square150x150Logo.png` | App Icon (Mediano) | 150 x 150 | 1:1 | PNG (39.717 B) | **VALID** | Icono cuadrado del listado |
| `release/store/Square44x44Logo.png` | App Icon (Pequeño) | 44 x 44 | 1:1 | PNG (4.549 B) | **VALID** | Icono de barra de tareas / lista |
| `release/store/Wide310x150Logo.png` | Tile Ancho | 310 x 150 | ~2:1 | PNG (31.346 B) | **VALID** | Tile panorámico de Windows |
| `release/store/SplashScreen.png` | Pantalla de Carga | 620 x 300 | ~2:1 | PNG (108.542 B) | **VALID** | Splash screen de la aplicación |
| `release/store/main-window.png` | Captura Desktop #1 | 593 x 625 | ~1:1 | PNG (60.041 B) | **VALID** | Captura real: interfaz Fluent Dark principal |
| `release/store/mainwindow_dark.png` | Captura Desktop #2 | 563 x 788 | ~1:1.4 | PNG (145.176 B) | **VALID** | Captura real: vista extendida con diagnósticos |
| `release/store/protection_overview.png`| Captura Desktop #3 | 563 x 356 | ~1.58:1 | PNG (28.764 B) | **VALID** | Captura real: tarjetas de cámara y micrófono |
| `release/store/tray_menu.png` | Captura Desktop #4 | 465 x 190 | ~2.45:1 | PNG (118.936 B) | **VALID** | Captura real: menú contextual de bandeja |
| `packaging/Assets/` (Matriz Completa)| Activos Internos PRI | Escalas 100%-400%, targetsize 16-256 | Diversas | Monolítico en `resources.pri` | **VALID** | Recursos integrados en el paquete MSIX |

### Observaciones de Cumplimiento:
1. Las 4 capturas de pantalla son capturas reales de la interfaz Avalonia 11 ejecutándose en Windows, sin simulaciones engañosas, sin marcas de agua de debug y sin exponer credenciales, rutas privadas ni información personal.
2. Microsoft Store recomienda un mínimo de 1 captura y sugiere 4 o más para optimizar la ficha en PCs de escritorio. PrivGuard dispone de 4 capturas genuinas.
3. El paquete no declara compatibilidad con Xbox, HoloLens ni Surface Hub, enfocándose exclusivamente en Windows 10 y 11 Desktop (x64).

---

## 2. Auditoría de Declaraciones Comerciales (Honesty & Accuracy)

Se revisaron exhaustivamente los textos del listing en inglés (`en-us`) y español (`es-es`) para garantizar que no existan afirmaciones exageradas o falsas:

| Declaración / Claim Analizado | Postura en el Listing | Verificación con Código Fuente | Veredicto |
|---|---|---|---|
| **Silenciado de Micrófono** | Silencia puntos de captura de audio mediante la API oficial Core Audio WASAPI (`IAudioEndpointVolume`). | Se realiza a nivel de software en el motor de audio de Windows. No promete un bloqueo físico irrevocable de hardware. | **CONFORME** |
| **Límites de Modo Exclusivo** | Se advierte expresamente que flujos en modo exclusivo o drivers especializados pueden comportarse de forma distinta si no admiten hardware mute. | `LocalizationCatalog` y `MainViewModel` implementan advertencias explícitas sobre soporte de hardware y modo exclusivo. | **CONFORME** |
| **Control de Cámara** | Ofrece acceso directo a la configuración nativa de privacidad de Windows (`ms-settings:privacy-webcam`). | En la edición Store no se intenta deshabilitar dispositivos PnP ni manipular el registro HKLM. No afirma bloqueo por hardware de cámara. | **CONFORME** |
| **Elevación y Privilegios** | Se ejecuta completamente como usuario estándar (`asInvoker`) sin requerir elevación administrativa. | Manifiesto MSIX declara solo `runFullTrust`, con 0 `allowElevation` y 0 `unvirtualizedResources`. | **CONFORME** |
| **Aviso de Desinstalación** | La interfaz muestra un aviso persistente aconsejando desilenciar antes de desinstalar. | `MainViewModel` activa `IsMicActiveStoreWarningVisible` al detectar el micrófono silenciado en el perfil Store. | **CONFORME** |
| **Privacidad y Telemetría** | Operación 100% local, sin telemetría remota, sin analíticas y sin captura ni transmisión de audio/video. | Inspección de dependencias y código confirma 0 bibliotecas de red, 0 `HttpClient`, 0 SDKs de analítica. | **CONFORME** |
| **Antivirus / Anti-spyware** | No promete protección contra malware, hackers o troyanos de acceso remoto. | El producto se describe honestamente como una utilidad de control y verificación de estado de privacidad. | **CONFORME** |

---

## 3. Contenido Definitivo de la Ficha Comercial

### Ficha en Inglés (`en-us`) — Idioma Principal
- **Title:** `PrivGuard`
- **Short Description / Subtitle:** `Manage and verify native Windows camera and microphone privacy controls.`
- **Category:** `Security` > `Personal security` (Secundaria: `Utilities & tools`).
- **Features:**
  1. *Microphone Privacy Muting:* Quickly silence active recording endpoints using native Windows Core Audio APIs.
  2. *Observable Status Verification:* Live reporting based on actual Windows endpoint volume and mute state.
  3. *Quick Privacy Settings Access:* One-click shortcuts to Windows Camera and Microphone system privacy settings.
  4. *Standard User Execution:* Runs completely as a standard user (`asInvoker`) with zero administrative elevation prompts.
  5. *System Tray Integration:* Minimizes cleanly to the notification area to keep your desktop uncluttered.
  6. *Windows Startup Integration:* Optional startup app support to start minimized upon logon.
  7. *Zero-Telemetry Policy:* Operates 100% offline; device preferences never leave your PC.
- **Search Keywords:** `privacy guard`, `microphone mute`, `camera privacy`, `mic mute`, `privacy tool`, `device privacy`, `audio mute`.

### Ficha en Español (`es-es`) — Idioma Adicional
- **Título:** `PrivGuard`
- **Subtítulo / Descripción Corta:** `Administra y verifica controles nativos de privacidad de cámara y micrófono en Windows.`
- **Características:**
  1. *Silenciado Rápido de Micrófono:* Silencia puntos de captura de audio activos mediante la API nativa Core Audio.
  2. *Estado Efectivo Verificado:* Comprueba el estado real del audio en Windows sin asumir estados no verificados.
  3. *Acceso Rápido a Ajustes de Windows:* Enlaces directos a las opciones de privacidad de cámara y micrófono del sistema.
  4. *Mínimo Privilegio:* Se ejecuta completamente como usuario estándar (`asInvoker`) sin avisos de elevación administrativa.
  5. *Integración con Bandeja del Sistema:* Se minimiza discretamente al área de notificación para no saturar tu escritorio.
  6. *Inicio Automático con Windows:* Compatible con Aplicaciones de Inicio para arrancar minimizado al iniciar sesión.
  7. *Cero Telemetría:* Funciona 100% desconectado; tus preferencias e identificadores jamás salen de tu equipo.
- **Palabras Clave:** `privacidad microfono`, `silenciar microfono`, `privacidad camara`, `mute audio`, `control privacidad`, `privacidad windows`, `privguard`.

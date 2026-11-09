# PrivGuard — Privacy Policy Draft / Borrador de Política de Privacidad (Fase 8B.1E)

**Fecha:** Noviembre de 2026  
**Versión del Producto:** PrivGuard 2.0 (Store Edition)  
**Alcance:** Paquete Microsoft Store (`cdevstudios.PrivGuard`)

> *Nota Informativa:* Este documento constituye un borrador técnico riguroso basado en el código fuente verificado de PrivGuard Store Edition. Los campos señalados como `[OWNER INPUT REQUIRED]` deben ser completados por el propietario de la aplicación antes de publicar la política en una URL pública accesible.

---

## 1. English Version (`en-us`)

### Summary
PrivGuard is an offline, local-first privacy utility designed to help users manage Windows microphone muting and access native camera privacy settings. **PrivGuard does not collect, record, process, or transmit any personal data, audio streams, or video feeds.**

### 1. Information We Do Not Collect
- **Zero Audio/Video Recording:** PrivGuard interacts with Windows Core Audio APIs solely to toggle the master mute switch of active audio recording endpoints. It does not access audio buffer streams, capture audio, or record conversations.
- **Zero Video Capture:** PrivGuard does not access, intercept, or process camera frames or image feeds. Camera management directs the user to the native Windows Camera Privacy Settings (`ms-settings:privacy-webcam`).
- **Zero Telemetry or Analytics:** PrivGuard does not include any analytics SDKs, telemetry libraries, tracking pixels, or remote error reporting services.
- **Zero Network Transmission:** PrivGuard does not declare network capabilities in its package manifest (`internetClient` is absent). It makes zero HTTP/HTTPS requests, opens zero network sockets, and operates completely offline.

### 2. Local Data Storage & Processing
PrivGuard stores limited operational data strictly on your local computer within your user profile directory (`%LOCALAPPDATA%\PrivGuard\Store\`):
- **User Preferences:** Local configuration state (such as UI language preference).
- **Session Recovery Journal:** Local write-ahead recovery file (`privacy-session-v1.json`) used to track session muting states and ensure proper release.
- **Local Diagnostic Logs:** Rotating local log files containing technical execution timestamps and device operational status for troubleshooting. These files never leave your device.
- **Local Crash Reports:** If an unexpected software fault occurs, a diagnostic JSON dump is written strictly to your local drive. No automatic cloud uploads occur.

### 3. Third-Party Services
PrivGuard contains zero third-party tracking, advertising, or data-sharing services.

### 4. Data Retention and Deletion
All local operational data is stored solely on your local drive and can be deleted at any time by deleting the `%LOCALAPPDATA%\PrivGuard\Store\` directory or uninstalling the application via Windows Settings.

### 5. Contact Information
For questions or support regarding this privacy policy, please contact:
- **Developer / Publisher:** cdev studios
- **Contact Email / URL:** `[OWNER INPUT REQUIRED — Insert public contact email or GitHub Issues link]`
- **Repository:** https://github.com/cristianbarreiro/PrivGuard

---

## 2. Versión en Español (`es-es`)

### Resumen
PrivGuard es una utilidad de privacidad local y desconectada diseñada para ayudar a los usuarios a silenciar el micrófono y acceder a los ajustes nativos de privacidad de la cámara en Windows. **PrivGuard no recopila, graba, procesa ni transmite ningún dato personal, flujo de audio o captura de video.**

### 1. Información que NO Recopilamos
- **Cero Grabación de Audio o Video:** PrivGuard interactúa con la API de Windows Core Audio únicamente para activar o desactivar el silenciado de los puntos de captura de audio. No accede a búferes de sonido, no graba audio ni escucha conversaciones.
- **Cero Captura de Cámara:** PrivGuard no intercepta, graba ni procesa imágenes o secuencias de cámara. Para la cámara, la aplicación deriva directamente al usuario a la página oficial de privacidad de Windows (`ms-settings:privacy-webcam`).
- **Cero Telemetría y Analítica:** PrivGuard no incluye bibliotecas de telemetría, SDKs analíticos ni herramientas de seguimiento de terceros.
- **Cero Transmisión de Red:** El manifiesto del paquete no declara capacidades de red (`internetClient` está ausente). La aplicación no realiza peticiones HTTP/HTTPS, no abre sockets de red y funciona 100% desconectada.

### 2. Procesamiento y Almacenamiento Local
PrivGuard almacena datos operativos mínimos exclusivamente en tu equipo local dentro de tu perfil de usuario (`%LOCALAPPDATA%\PrivGuard\Store\`):
- **Preferencias de Usuario:** Configuración local (ej. selección de idioma en la interfaz).
- **Diario de Sesión de Privacidad:** Archivo local de recuperación (`privacy-session-v1.json`) para registrar los estados de silenciado y garantizar su liberación.
- **Registros Locales (Logs):** Archivos de registro rotativos con marcas de tiempo y estados operativos para diagnóstico. Jamás se envían fuera del equipo.
- **Reportes de Caída Locales:** Ante un error inesperado, se genera un volcado JSON únicamente en disco local. No existe envío automático a la nube.

### 3. Servicios de Terceros
PrivGuard no integra herramientas de publicidad, seguimiento ni intercambio de datos con terceros.

### 4. Retención y Eliminación de Datos
Todos los datos operativos se conservan exclusivamente en tu disco local y pueden ser eliminados en cualquier momento borrando el directorio `%LOCALAPPDATA%\PrivGuard\Store\` o desinstalando la aplicación desde la Configuración de Windows.

### 5. Contacto
Para consultas sobre esta política de privacidad, puedes contactar al desarrollador en:
- **Desarrollador / Publicador:** cdev studios
- **Correo o Canal de Contacto:** `[OWNER INPUT REQUIRED — Insertar correo público o enlace de GitHub Issues]`
- **Repositorio Oficial:** https://github.com/cristianbarreiro/PrivGuard

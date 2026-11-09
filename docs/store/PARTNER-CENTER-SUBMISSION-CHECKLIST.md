# PrivGuard 2.0 — Partner Center Submission Checklist (Fase 8B.1D)

Este documento contiene la lista de verificación paso a paso, los metadatos exactos del paquete candidato y los campos que el propietario del producto debe completar o confirmar manualmente en el portal de **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard`).

---

## 1. Identidad y Propiedades del Paquete

Verificados directamente desde el manifiesto `AppxManifest.xml` empaquetado en el artefacto final:

| Campo en Partner Center | Valor Canónico Requerido | Estado / Validación |
|---|---|---|
| **Package Identity Name** | `cdevstudios.PrivGuard` | Coincidencia exacta con la reserva de nombre |
| **Publisher** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` | Sujeto oficial del Partner Center |
| **Publisher Display Name** | `cdev studios` | Nombre comercial registrado |
| **Package Family Name (PFN)** | `cdevstudios.PrivGuard_tv7b4yqg10sw2` | Calculado por Windows AppModel |
| **Store Product ID** | `9NQ56N45XH5N` | **PENDIENTE DE CONFIRMACIÓN EN PARTNER CENTER** |
| **Package Version** | `2.0.0.0` | Versión semántica 2.0.0.0 |
| **Architecture** | `x64` | Windows x64 (Intel/AMD) |
| **Target OS / Min Version** | Windows 10 build 19041+ / Windows 11 | `10.0.19041.0` a `10.0.26100.0` |

---

## 2. Artefacto de Envío (Paquete Binario)

- **Archivo a Cargar:** `release/store/PrivGuard-2.0.0.0-x64.msix`
- **Tamaño:** `58.573.769 bytes`
- **Hash SHA-256:** `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`
- **Nota sobre el formato (`.msix` vs `.msixupload`):**
  Microsoft Partner Center acepta directamente paquetes `.msix` individuales para aplicaciones de escritorio Win32 / Desktop Bridge. No es obligatorio generar un archivo `.msixupload` si no se dispone de un servidor de símbolos corporativo. No se deben empaquetar símbolos falsos o vacíos.

---

## 3. Justificación de Capacidad Restringida (`runFullTrust`)

En la sección **Submission > Product declarations / Capabilities** de Partner Center, cuando el portal pregunte por la justificación de la capacidad restringida `runFullTrust`, ingresar el siguiente texto:

> **Technical Justification for `runFullTrust`:**
> PrivGuard is a native .NET desktop privacy utility packaged via the Desktop Bridge (MSIX) for Windows 10 and Windows 11.
> The `runFullTrust` capability is strictly required because the application interacts with native Windows COM interfaces and Win32 APIs that are unavailable within an AppContainer sandbox:
> 1. Windows Core Audio APIs (WASAPI `IAudioEndpointVolume` and `IMMDeviceEnumerator`) to inspect microphone volume states and apply privacy muting.
> 2. Windows Management Instrumentation (WMI `Win32_PnPEntity`) to detect connected camera and microphone hardware and correlate friendly names.
> 3. Native Windows Notification Area (System Tray) lifecycle integration and custom Avalonia Fluent Dark desktop windowing.
>
> In accordance with least-privilege principles (Architectural Decision D-01):
> - The application executes exclusively as a standard user (`requestedExecutionLevel="asInvoker"`).
> - It does NOT declare `allowElevation` (no UAC prompts, no administrative helpers).
> - It does NOT declare `unvirtualizedResources` (no registry virtualization overrides).
> - Camera controls direct the user to Windows native Settings (`ms-settings:privacy-webcam`) rather than attempting elevated driver manipulation.
> - The application operates 100% locally with zero telemetry, zero analytics, and zero network transmission.

---

## 4. Ficha Comercial y Contenido del Listing

### Idioma Principal: Inglés (`en-us`)
- **Product Title:** `PrivGuard`
- **Subtitle / Short Description:** `Manage and verify native Windows camera and microphone privacy controls.`
- **Description:** Disponible completa en [`docs/store/STORE-LISTING-CONTENT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md#L26-L43).
- **Key Features:**
  - `Microphone Privacy Muting: Quickly silence active recording endpoints using native Windows Core Audio APIs.`
  - `Observable Status Verification: Live reporting based on actual Windows endpoint volume and mute state.`
  - `Quick Privacy Settings Access: One-click shortcuts to Windows Camera and Microphone system privacy settings.`
  - `Standard User Execution: Runs completely as a standard user (asInvoker) with zero administrative elevation prompts.`
  - `System Tray Integration: Minimizes cleanly to the notification area to keep your desktop uncluttered.`
  - `Zero-Telemetry Policy: Operates 100% offline; your preferences never leave your PC.`
- **Search Keywords (7 permitidas):**
  `privacy guard`, `microphone mute`, `camera privacy`, `mic mute`, `privacy tool`, `device privacy`, `audio mute`.

### Idioma Adicional: Español (`es-es`)
- **Título del Producto:** `PrivGuard`
- **Subtítulo / Descripción Corta:** `Administra y verifica controles nativos de privacidad de cámara y micrófono en Windows.`
- **Descripción:** Disponible completa en [`docs/store/STORE-LISTING-CONTENT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/STORE-LISTING-CONTENT.md#L67-L84).
- **Palabras Clave (7 permitidas):**
  `privacidad microfono`, `silenciar microfono`, `privacidad camara`, `mute audio`, `control privacidad`, `privacidad windows`, `privguard`.

---

## 5. Activos Gráficos y Capturas de Pantalla

Los activos requeridos por Partner Center se encuentran generados en `release/store/` y `packaging/Assets/`:

- **Iconos del Paquete y Tienda:**
  - `Square150x150Logo.png` (`release/store/Square150x150Logo.png`)
  - `Square44x44Logo.png` (`release/store/Square44x44Logo.png`)
  - `StoreLogo.png` (`release/store/StoreLogo.png`)
  - `Wide310x150Logo.png` (`release/store/Wide310x150Logo.png`)
  - `SplashScreen.png` (`release/store/SplashScreen.png`)
- **Capturas de Pantalla (Screenshots para Desktop):**
  - `release/store/main-window.png` (Ventana principal con tema Fluent Dark)
  - `release/store/protection_overview.png` (Detalle de estado de protección)
  - `release/store/tray_menu.png` (Menú contextual de bandeja del sistema)

---

## 6. Parámetros que Requieren Decisión del Propietario

Los siguientes puntos dependen exclusivamente de la titularidad y preferencias del propietario y deben definirse en el portal:

1. **Precio y Disponibilidad:**
   - **Precio:** Gratuito (Recomendado) o de pago (a criterio del propietario).
   - **Mercados:** Todos los mercados mundiales o restringido a países específicos.
   - **Fecha de Publicación:** Publicación automática inmediata tras la certificación o retención manual para publicación programada.
2. **Clasificación por Edad (Cuestionario IARC):**
   - Completar el cuestionario IARC en Partner Center (categoría Utilidad / Seguridad Personal: no contiene violencia, contenido para adultos, lenguaje inapropiado ni compras dentro de la app; suele resultar en clasificación 3+ / Todos).
3. **Enlaces Obligatorios de Soporte y Privacidad:**
   - **URL de Política de Privacidad:** URL pública donde se describa que PrivGuard no recopila datos (ej. enlace al archivo `PRIVACY.md` o sitio web del proyecto).
   - **URL o Correo de Soporte:** Canal de contacto del desarrollador (ej. página de GitHub Issues o correo oficial).
4. **Notas para Certificación:**
   - Pegar el contenido de [`PARTNER-CENTER-CERTIFICATION-NOTES.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md).

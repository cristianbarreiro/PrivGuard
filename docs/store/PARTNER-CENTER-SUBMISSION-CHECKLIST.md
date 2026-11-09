# PrivGuard 2.0 — Partner Center Submission Checklist (Fase 8B.1F)

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
| **Store Product ID** | `9NQ56N45XH5N` | Confirmar en dashboard de Partner Center |
| **Package Version** | `2.0.0.0` | Versión semántica 2.0.0.0 |
| **Architecture** | `x64` | Windows x64 (Intel/AMD) |
| **Target OS / Min Version** | Windows 10 build 19041+ / Windows 11 | `10.0.19041.0` a `10.0.26100.0` |

---

## 2. Artefacto de Envío (Paquete Binario)

- **Archivo a Cargar:** `release/store/PrivGuard-2.0.0.0-x64.msix`
- **Tamaño:** `58.573.769 bytes`
- **Hash SHA-256:** `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`
- **Distinción de Formato de Carga (`.msix` vs `.msixupload`):**
  - Microsoft **recomienda** el formato `.msixupload` para Windows 10+ cuando el pipeline genera automáticamente un archivo de símbolos públicos (`.appxsym`).
  - La subida directa del archivo individual `.msix` es un **formato oficialmente admitido y un fallback plenamente válido** en Partner Center.
  - *Estado en PrivGuard:* El pipeline de empaquetado `scripts/build-msix.ps1` utiliza las herramientas del Windows SDK (`MakeAppx.exe`) para compilar directamente paquetes `.msix` y `.msixbundle`. Conforme a la política de no fabricar contenedores ZIP arbitrarios ni símbolos ficticios vacíos, se utiliza la subida directa del `.msix` validado (`MSIXUPLOAD NOT GENERATED — DIRECT MSIX SUPPORTED FALLBACK`).

---

## 3. Justificación de Capacidad Restringida (`runFullTrust`)

En la sección **Submission > Product declarations / Capabilities** de Partner Center, cuando el portal solicite la justificación de la capacidad restringida `runFullTrust`, ingresar el siguiente texto oficial:

> **Technical Justification for `runFullTrust`:**  
> PrivGuard is a native .NET desktop privacy utility packaged via the Desktop Bridge (MSIX) for Windows 10 and Windows 11.  
> The `runFullTrust` capability is strictly required because the application interacts with native Windows COM interfaces and Win32 APIs that are unavailable within an AppContainer sandbox:  
> 1. Windows Core Audio APIs (WASAPI `IAudioEndpointVolume` and `IMMDeviceEnumerator`) to inspect microphone volume states and apply software privacy muting.  
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

Los textos completos, características, palabras clave y notas de certificación bilingües están disponibles para copiar y pegar en:
- **Archivo Canónico:** [`docs/store/STORE-LISTING-FINAL.md`](STORE-LISTING-FINAL.md)

### Resumen Rápido por Idioma:
- **English (`en-us`):**
  - **Product Title:** `PrivGuard`
  - **Subtitle:** `Manage and verify native Windows camera and microphone privacy controls.`
  - **Keywords:** `privacy guard`, `microphone mute`, `camera privacy`, `mic mute`, `privacy tool`, `device privacy`, `audio mute`.
- **Español (`es-es`):**
  - **Título del Producto:** `PrivGuard`
  - **Subtítulo:** `Administra y verifica controles nativos de privacidad de cámara y micrófono en Windows.`
  - **Palabras Clave:** `privacidad microfono`, `silenciar microfono`, `privacidad camara`, `mute audio`, `control privacidad`, `privacidad windows`, `privguard`.

---

## 5. Activos Gráficos y Capturas de Pantalla

Todos los activos auditados se encuentran en `release/store/`:

### Capturas de Pantalla (Screenshots para Desktop):
1. `release/store/main-window.png` (593 x 625) — Ventana principal con tema Fluent Dark.
2. `release/store/mainwindow_dark.png` (563 x 788) — Vista completa de la interfaz en modo oscuro.
3. `release/store/protection_overview.png` (563 x 356) — Detalle de estado de protección y accesos directos.
4. `release/store/tray_menu.png` (465 x 190) — Menú contextual de la bandeja del sistema (System Tray).

### Icono de Ficha de Aplicación (1:1 300x300):
- `release/store/StoreListing-300x300.png` (300 x 300) — Icono oficial de alta resolución generado a partir del logotipo maestro de 1024x1024 px para mostrar en búsquedas y colecciones de la Tienda.

### Iconos del Paquete (referenciados internamente en el manifiesto):
- `Square150x150Logo.png`
- `Square44x44Logo.png`
- `StoreLogo.png`
- `Wide310x150Logo.png`
- `SplashScreen.png`

---

## 6. Parámetros que Requieren Decisión del Propietario

Consultar [`docs/store/OWNER-DECISIONS.md`](OWNER-DECISIONS.md) para la lista completa y detallada:

1. **Precios y Disponibilidad:** Confirmar precio (**Free** recomendado), mercados de distribución y fecha/modo de publicación (**Manual Hold** recomendado).
2. **Cuestionario IARC (Edad):** Responder a las preguntas del cuestionario en Partner Center con la realidad del producto (0 violencia, 0 lenguaje ofensivo, 0 compras dentro de la app, 0 transmisión de red, etc.).
3. **URL Pública de Política de Privacidad:** Alojar [`docs/store/privacy-policy/index.html`](privacy-policy/index.html) en un servidor o dominio HTTPS público y suministrar la URL en el portal.
4. **Canal de Soporte:** Confirmar el canal público (`https://github.com/cristianbarreiro/PrivGuard/issues` propuesto).
5. **Notas de Certificación:** Copiar el bloque de notas para los revisores técnicos disponible en [`docs/store/STORE-LISTING-FINAL.md`](STORE-LISTING-FINAL.md).

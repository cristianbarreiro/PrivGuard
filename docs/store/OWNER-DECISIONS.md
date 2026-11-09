# PrivGuard 2.0 — Matriz de Decisiones Exclusivas del Propietario (`OWNER-DECISIONS.md`)

Este documento consolida las decisiones comerciales, de cuenta y de configuración en **Microsoft Partner Center** que corresponden exclusivamente al propietario del producto. No contiene valores ficticios ni decisiones asumidas como definitivas sin autorización expresa.

> **Recomendación Técnica de Publicación:**  
> En la sección **Submission options**, seleccionar **Manual Publishing (Hold)** (`Don't publish this submission until I select Publish now`).  
> *Razón:* Microsoft no permite cancelar ni retener una publicación automática una vez iniciada la certificación. La opción manual permite al propietario inspeccionar el informe emitido por Microsoft antes de que la aplicación sea pública.

---

## 1. Tabla de Decisiones y Confirmaciones Pendientes

| Decisión / Campo | Valor Preparado / Recomendación Técnica | Confirmación Pendiente del Propietario |
|---|---|---|
| **Store Product ID** | `9NQ56N45XH5N` | Confirmar que coincide con el ID asignado en el dashboard de Partner Center. |
| **Identidad del Paquete** | `cdevstudios.PrivGuard` | Confirmar coincidencia exacta con el nombre de producto reservado en la cuenta. |
| **Publisher Subject** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` (`cdev studios`) | Confirmar coincidencia con el perfil de editor de la cuenta de desarrollador. |
| **Precio Base** | Recomendación: **Free** ($0.00 USD) | El propietario decide el precio comercial (gratuito o de pago). |
| **Mercados de Distribución** | Todos los mercados (o excluir regiones específicas) | El propietario decide los países de distribución. |
| **Visibilidad / Discoverability** | Recomendación: **Público** (visible en la Tienda) o **Hidden** (acceso solo con enlace directo) | El propietario decide el tipo de visibilidad para esta primera versión. |
| **Publicación tras Certificación** | Recomendación: **Manual hold** (`Don't publish until I select Publish now`) | Confirmar si desea retener la publicación para revisar el informe de Microsoft. |
| **URL de Política de Privacidad** | Documento preparado en `docs/store/privacy-policy/index.html` | El propietario aloja la página en un servidor/hosting HTTPS público y autoriza la URL. |
| **Canal de Soporte al Usuario** | Propuesta: `https://github.com/cristianbarreiro/PrivGuard/issues` | El propietario confirma si utilizará este enlace de GitHub o un correo oficial. |
| **Cuestionario IARC (Edad)** | Sin predicción automática; completar cada pregunta en el portal según comportamiento real | El propietario responde directamente a las preguntas del cuestionario de IARC en el portal. |
| **Categoría del Producto** | Propuesta: Primaria `Security > Personal security`, Secundaria `Utilities & tools` | El propietario confirma la categoría según las opciones vigentes del portal. |
| **Capacidad `runFullTrust`** | Justificación técnica redactada en `docs/store/STORE-LISTING-FINAL.md` | El propietario copia y pega el texto en el formulario del portal. |
| **Hardware RT-05** | Prueba física en micrófono real (silenciado por software WASAPI) | El propietario ejecuta la prueba en un PC con micrófono físico cuando esté disponible. |

---

## 2. Puntos Clave para el Cuestionario IARC

El cuestionario de la International Age Rating Coalition (IARC) en Partner Center formula preguntas directas sobre el contenido y funcionamiento de la aplicación:

1. **Información técnica a tener a mano:**
   - La aplicación no recopila ubicación física ni datos personales identificables.
   - La aplicación no permite compras dentro de la aplicación ni monedas virtuales.
   - La aplicación no permite compartir contenido generado por usuarios ni comunicarse con otros usuarios.
   - La aplicación no contiene violencia, lenguaje vulgar, referencias a drogas ni contenido para adultos.
   - La aplicación interactúa exclusivamente con el silenciado de puntos de conexión de audio de Windows y no transmite sonido ni video.
2. **Acción requerida:** El propietario debe responder a cada pregunta según la realidad de la aplicación. No se debe predecir ni asumir una calificación fija de forma anticipada.

---

## 3. Lista de Archivos que el Propietario Debe Tener Listos para la Sesión

1. Paquete binario verificado: `release/store/PrivGuard-2.0.0.0-x64.msix`
   - SHA-256: `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`
2. Capturas de pantalla (4 archivos PNG):
   - `release/store/main-window.png`
   - `release/store/mainwindow_dark.png`
   - `release/store/protection_overview.png`
   - `release/store/tray_menu.png`
3. Icono de ficha de aplicación (1:1 300x300 PNG):
   - `release/store/StoreListing-300x300.png`
4. Textos para copiar y pegar:
   - `docs/store/STORE-LISTING-FINAL.md` (textos bilingües, justificación `runFullTrust` y notas de certificación).
5. URL pública HTTPS de la política de privacidad (alojar `docs/store/privacy-policy/index.html`).

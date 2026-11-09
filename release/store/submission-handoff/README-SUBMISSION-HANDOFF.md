# PrivGuard 2.0 — Submission Handoff README (Fase 8B.1F)

Este directorio contiene el expediente formal, congelado y determinista para realizar la presentación y carga de PrivGuard Store Edition en **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard`).

---

## 1. Artefacto Canónico para Subir al Portal

| Parámetro | Valor Verificado |
|---|---|
| **Archivo Binario** | `release/store/PrivGuard-2.0.0.0-x64.msix` |
| **Tamaño Exacto** | `58.573.769 bytes` |
| **Hash SHA-256** | `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7` |
| **Identity Name** | `cdevstudios.PrivGuard` |
| **Publisher** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` |
| **Publisher Display Name** | `cdev studios` |
| **Versión** | `2.0.0.0` |
| **Arquitectura** | `x64` |
| **Capacidades Declaradas** | `rescap:runFullTrust` (única capability) |
| **Capacidades Prohibidas** | 0 (`allowElevation` ausente, `unvirtualizedResources` ausente) |
| **Verificación Local** | `scripts/verify-store-msix.ps1` -> **PASS (Exit code 0)** |

---

## 2. Formato de Carga: Direct MSIX Supported Fallback

- **Recomendación de Microsoft:** Microsoft documenta `.msixupload` como el formato recomendado para Windows 10 y posteriores cuando el entorno de compilación genera automáticamente paquetes de símbolos públicos (`.appxsym`).
- **Soporte de Carga Directa:** Partner Center admite y valida de forma nativa la carga directa de archivos `.msix` individuales para aplicaciones de escritorio empaquetadas (Desktop Bridge).
- **Dictamen del Expediente:** Dado que el pipeline `scripts/build-msix.ps1` compila directamente el paquete MSIX mediante `MakeAppx.exe`, y conforme a la norma de seguridad que prohíbe fabricar archivos ZIP arbitrarios o crear símbolos ficticios vacíos, se establece la subida directa del archivo `.msix` verificado (`MSIXUPLOAD NOT GENERATED — DIRECT MSIX SUPPORTED FALLBACK`).

---

## 3. Guía de Archivos del Expediente

### En este directorio (`release/store/submission-handoff/`):
- **`ARTIFACT-MANIFEST.json`**: Metadatos estructurados del paquete, capacidades, procedencia y sumas criptográficas.
- **`SHA256SUMS.txt`**: Sumas de comprobación SHA-256 de todos los artefactos incluidos en la entrega.
- **`PARTNER-CENTER-STEP-BY-STEP.md`**: Runbook con la secuencia detallada de pasos en el portal de Microsoft.

### En la carpeta de distribución (`release/store/`):
- **`PrivGuard-2.0.0.0-x64.msix`**: Binario oficial para carga.
- **`BUILD-PROVENANCE.json`**: Metadatos de procedencia de compilación y entorno.
- **`StoreListing-300x300.png`**: Icono de ficha de aplicación (1:1 300x300) para búsquedas de la Tienda.
- **Capturas de Pantalla (4 imágenes):**
  - `main-window.png` (593 x 625)
  - `mainwindow_dark.png` (563 x 788)
  - `protection_overview.png` (563 x 356)
  - `tray_menu.png` (465 x 190)

### En la carpeta de documentación (`docs/store/`):
- **`docs/store/STORE-LISTING-FINAL.md`**: Ficha comercial bilingüe completa (EN/ES), palabras clave, licencia y justificaciones.
- **`docs/store/PRIVACY-POLICY-FINAL.md`**: Política de privacidad bilingüe auditada basada en el inventario real de datos.
- **`docs/store/privacy-policy/index.html`**: Página web estática, responsive y autónoma, lista para publicar.
- **`docs/store/OWNER-DECISIONS.md`**: Matriz con las decisiones exclusivas que debe introducir el propietario en el portal.

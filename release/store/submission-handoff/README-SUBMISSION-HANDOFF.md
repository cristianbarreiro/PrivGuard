# PrivGuard 2.0 — Submission Handoff README (Fase 8B.1E)

Este directorio contiene el expediente formal y determinista para realizar la presentación y carga de PrivGuard Store Edition en **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard`).

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

## 2. Formato de Carga: Subida Directa de `.msix`

Microsoft Partner Center admite la carga directa de archivos `.msix` para aplicaciones de escritorio empaquetadas (Desktop Bridge). No es necesario ni recomendable generar un archivo `.msixupload` artificial si no se cuenta con un pipeline de compilación de símbolos corporativo. El archivo `PrivGuard-2.0.0.0-x64.msix` es el artefacto que debe subirse en la sección **Packages** del portal.

---

## 3. Guía de Archivos del Expediente

- **`ARTIFACT-MANIFEST.json`**: Metadatos estructurados del paquete y firmas criptográficas.
- **`SHA256SUMS.txt`**: Sumas de comprobación SHA-256 del binario y de los activos.
- **`PARTNER-CENTER-STEP-BY-STEP.md`**: Guía paso a paso para navegar y completar el portal de Microsoft.
- **`docs/store/OWNER-DECISIONS.md`**: Lista de decisiones comerciales que debe ingresar el propietario.
- **`docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`**: Checklist con todos los textos comerciales (EN/ES).
- **`docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md`**: Notas en inglés para los revisores de Microsoft.
- **`docs/store/PRIVACY-POLICY-DRAFT.md`**: Texto de política de privacidad listo para ser publicado en la web.
- **`docs/store/REAL-HARDWARE-RT05-TEST.md`**: Prueba física rápida del silenciado de micrófono en PC real.

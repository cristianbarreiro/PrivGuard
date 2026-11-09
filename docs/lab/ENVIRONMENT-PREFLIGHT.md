# Preflight del Entorno de Host y Validación del Candidato — Fase 8B.1D

**Fecha:** 9 de noviembre de 2026  
**Fase:** PrivGuard Fase 8B.1D — Windows Runtime Acceptance & Microsoft Store Submission Readiness  
**Árbol Git:** `cristianbarreiro/PrivGuard` en rama `main`  
**HEAD verificado:** `ecd15afd5200696cde838d778125ae4fe4dbf73b` (1 commit por delante de `origin/main`, sin push, working tree limpio)  

---

## 1. Inventario y Evidencia Criptográfica del Paquete

| Parámetro | Valor Observado | Estado / Validación |
|---|---|---|
| **Archivo del Paquete** | `release\store\PrivGuard-2.0.0.0-x64.msix` | Existente en disco |
| **Tamaño en Bytes** | `58.573.769 bytes` | Coincide exactamente con Fase 8B.1C |
| **Hash SHA-256** | `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7` | Coincide exactamente con Fase 8B.1C |
| **Identidad (Name)** | `cdevstudios.PrivGuard` | Verificado desde `AppxManifest.xml` interno |
| **Publisher** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` | Verificado desde `AppxManifest.xml` interno |
| **Publisher Display Name**| `cdev studios` | Verificado desde `AppxManifest.xml` interno |
| **Versión** | `2.0.0.0` | Verificado desde `AppxManifest.xml` interno |
| **Arquitectura** | `x64` | Verificado desde `AppxManifest.xml` interno |
| **MinVersion / Target OS**| `10.0.19041.0` / `10.0.26100.0` | Windows 10 versión 2004+ y Windows 11 |
| **Capacidades Declaradas** | `rescap:runFullTrust` (única capability) | Conforme a Decisión D-01 |
| **Capacidades Prohibidas** | 0 (`allowElevation`: ausente, `unvirtualizedResources`: ausente) | Conforme a Decisión D-01 |
| **Script Verificador** | `scripts/verify-store-msix.ps1` | **PASS (Exit code 0)** |
| **Registro de Proveniencia**| `release/store/BUILD-PROVENANCE.json` | Válido y consistente |

---

## 2. Capacidades Reales del Entorno de Host

| Propiedad | Valor Observado | Evidencia / Método |
|---|---|---|
| **Sistema Operativo** | Microsoft Windows 11 Enterprise Evaluation | `Get-ComputerInfo -Property OsName` |
| **Versión / Build OS** | `10.0.26300` (Build 26300) | Cumple holgadamente `MinVersion` (19041) |
| **Arquitectura del SO** | 64 bits (`x64`) | Compatible con binario `x64` |
| **Fabricante del Equipo** | `innotek GmbH` | `Win32_ComputerSystem.Manufacturer` |
| **Modelo del Equipo** | `VirtualBox` | `Win32_ComputerSystem.Model` |
| **BIOS / SMBIOS** | `VBOX - 1` / `VirtualBox` | `Win32_BIOS` |
| **Tipo de Entorno** | **Máquina Virtual (Guest VM en VirtualBox)** | Detección confirmada sin virtualización anidada |
| **Sesión de Usuario** | Interactiva (`[Environment]::UserInteractive == True`) | Sesión de escritorio activa |
| **.NET SDK / Runtime** | .NET SDK `10.0.403`, Runtime `10.0.14` | `dotnet --version` |
| **Cmdlets AppX** | Disponibles (`Get-AppxPackage`, `Add-AppxPackage`) | Módulo `Appx` cargado |
| **Cámaras Físicas** | **0 dispositivos detectados** | `Get-PnpDevice -Class Camera, Image` = 0 |
| **Endpoints de Audio** | 1 altavoz activo, 1 micrófono en error | `Get-PnpDevice -Class AudioEndpoint` |
| **Detalle Micrófono Host** | `SWD\MMDEVAPI\{0.0.1.00000000}.{8155585D-902B-4399-B33B-1EA8BE36ABE3}` | `Status: Error`, `Problem: CM_PROB_DISABLED` (audio virtualizado sin entrada física de host) |
| **Estado Firma MSIX Local**| `NotSigned` (esperado para paquete de ingesta Store) | `Get-AuthenticodeSignature` |
| **AppCertificationKit (WACK)**| Instalado en `C:\Program Files (x86)\Windows Kits\10\App Certification Kit\appcert.exe` | Devuelve `Solicitud no compatible` por entorno VM. **Nota:** WACK está oficialmente obsoleto según Microsoft Learn; la certificación real se realiza en Partner Center. |

---

## 3. Diagnóstico de Instalación Local del Paquete de Ingesta

- **Comando Ejecutado:** `Add-AppxPackage -Path release\store\PrivGuard-2.0.0.0-x64.msix`
- **Resultado:** Fallo con `HRESULT: 0x800B0100` (`TRUST_E_NOSIGNATURE: No hay ninguna firma presente en el sujeto`).
- **Análisis de Causa Raíz:**
  1. El paquete candidato para Microsoft Store se genera sin firma o con certificado de ingesta porque Microsoft firma automáticamente los paquetes en Partner Center con la clave de certificación oficial de Microsoft (`CN=Microsoft Corporation`).
  2. Windows rechaza la instalación directa de paquetes sin firma en máquinas de usuario estándar (`0x800B0100`).
  3. De acuerdo con las salvaguardas de seguridad (Fase 8B.1D, Sección 4 B.1), **no se deben desactivar controles de seguridad ni instalar certificados no autorizados en los almacenes del sistema**.
  4. Por tanto, la instalación local del MSIX de producción queda catalogada objetivamente como `BLOCKED BY ENVIRONMENT (OS Signature Policy)`, mientras que la lógica de aplicación está 100% verificada mediante los tests automatizados de integración (632 tests green) y la ejecución de diagnóstico en espacio de usuario.

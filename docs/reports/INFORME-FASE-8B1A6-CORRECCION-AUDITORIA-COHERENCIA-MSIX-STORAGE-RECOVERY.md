# PrivGuard — Fase 8B.1A.6
## Auditoría Correctiva de Coherencia entre Manifiesto MSIX, D-01, DI, Almacenamiento, Recuperación y Evidencia

**Fecha de Ejecución:** 6 de Noviembre de 2026  
**Repositorio:** `cristianbarreiro/PrivGuard`  
**Commit Auditado:** `9f856c0405e4e5b64e784309affad450c38d3a37`  
**Rama:** `main` (sincronizada al 100% con `origin/main`)  
**Fase Previa:** 8B.1A.5  
**Tipo de Documento:** Informe de Auditoría Adversarial y Corrección Factual  

---

## 1. Resumen Ejecutivo y Veredicto Formal

### 1.1 Resumen Ejecutivo
Esta auditoría técnica y correctiva fue realizada con un enfoque adversarial estricto sobre el estado del repositorio `PrivGuard` en el commit canónico `9f856c0`. Su propósito es confrontar las afirmaciones del informe 8B.1A.5 con el código fuente en ejecución, los manifiestos de empaquetado, la configuración del contenedor de inyección de dependencias (DI) y la documentación oficial de Microsoft Learn.

La auditoría ha revelado **contradicciones factuales y arquitectónicas de severidad P0 y P1** que invalidan las asunciones operativas previas:

1. **Contradicción Crítica en el Manifiesto MSIX (P0):** `packaging/Package.appxmanifest` declara explícitamente `<rescap:Capability Name="allowElevation" />` y `<rescap:Capability Name="unvirtualizedResources" />`. Esto contradice de plano la Decisión Arquitectónica **D-01** y el documento público de certificación `docs/store/MICROSOFT-STORE-JUSTIFICATION.md`, los cuales afirman que la edición Store declara *únicamente* `runFullTrust`. Además, la presencia de `unvirtualizedResources` sin `<desktop6:FileSystemWriteVirtualization>disabled</desktop6:FileSystemWriteVirtualization>` bajo `<Properties>` **no** desactiva la virtualización de AppData en Windows 10/11.
2. **Desconexión Total del DI de Perfiles en Producción (P0):** Aunque en las Fases 8B.1B.2 y 8B.1B.3 se implementaron `PackageIdentityKind`, `EditionProfile`, `IEditionProfileResolver` y `DefaultEditionProfileResolver`, **ninguno de estos contratos está registrado en `Program.cs`**. En tiempo de ejecución, el proceso empaquetado Store y el ejecutable Standalone corren exactamente el mismo código de `WindowsProtectionProvider`, intentando elevar mediante el worker de UAC y compartiendo el mismo directorio no aislado `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json`.
3. **Escritura Directa a ConsentStore en HKCU vs. Justificación Store (P0):** Mientras que la documentación de justificación ante Microsoft afirma que la Store Edition no escribe en `HKCU` y redirige al usuario a `ms-settings:`, `WindowsUserPrivacyManager.cs` en código de producción escribe cadenas `"Deny"` y `"Allow"` directamente en el registro `HKCU`. No existe una sola línea de código en el proyecto que invoque `ms-settings:`.
4. **Falsa Garantía de Verificación en Core Audio (P1):** `WindowsProtectionProvider.cs` marca `micStandardIsVerified = true` para el micrófono basándose únicamente en el estado booleano de mute y la disponibilidad de endpoints, **ignorando por completo si `HardwareMuteSupport` es `Supported` o `NotSupported`**. Si el hardware no implementa mute físico/driver, el silenciamiento por software de Core Audio solo protege flujos en *Shared Mode*, siendo completamente omitido (*bypass*) por cualquier aplicación que abra el dispositivo en *Exclusive Mode* (`AUDCLNT_SHAREMODE_EXCLUSIVE`).
5. **Inviabilidad de Endpoint Claims en ProgramData (P1):** El esquema de archivos `.claim` en `ProgramData` propuesto en 8B.1A.5 adolece de un trade-off de seguridad irresoluble sin un broker privilegiado: si se respeta un claim no autenticado, cualquier cuenta estándar puede generar un DoS contra otros usuarios (*Claim Squatting*); si se ignora para evitar DoS, no ofrece exclusión mutua de hardware.
6. **Machine Ownership Store no es un Recovery Escrow (P0):** El registro en `HKLM\SOFTWARE\PrivLock\PrivilegedOwnership\v2` almacena exclusivamente un hash SHA-256 (fingerprint/attestation), un SID y un enum de etapa. **No almacena snapshots de restauración** (no guarda Instance IDs, clases de dispositivo, códigos de problema ni valores previos de registro). Si el WAL del usuario se destruye, la máquina no puede restaurar el hardware a partir de HKLM.

### 1.2 Veredicto Formal

> [!CAUTION]
> **VEREDICTO: HOLD — ARCHITECTURE/IMPLEMENTATION CONTRADICTIONS FOUND**
> 
> Se prohíbe taxativamente la autorización de la Fase **8B.1C** y se ratifica el estado de **CONDITIONAL HOLD** para la distribución en **Microsoft Store**.  
> El informe 8B.1A.5 contenía clasificaciones erróneas (`CONFIRMADO EN CÓDIGO` sobre componentes puramente teóricos como Latches en ProgramData, Claims o Transaction Gates). Las discrepancias de manifiesto y DI deben subsanarse mediante decisiones de producto vinculantes antes de cualquier avance en la implementación.

---

## 2. Commit Auditado, Estado Git y Evidencia de Integridad

### 2.1 Registro de Integridad Git (Preflight y Postflight)
- **Rama Activa:** `main`
- **HEAD Local:** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **HEAD Remoto (`origin/main`):** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **Estado de Sincronización:** Idéntico (0 commits por delante, 0 por detrás; verificado vía `git fetch origin`).
- **Estado del Working Tree:** `git status --short` arrojó salida completamente vacía.
- **Diff Acumulado:** `git diff --stat` arrojó 0 archivos modificados.
- **Último Commit en Historial:**
  ```text
  commit 9f856c0405e4e5b64e784309affad450c38d3a37
  Date:    Thu Nov 5 23:57:20 2026 +0100
  Subject: feat(security,packaging): enforce fail-closed enum defaults, immutable resolver snapshot and strict NoOp detector (Phase 8B.1B.3)
  ```
- **Integridad de Código:** La auditoría se ejecutó de forma estrictamente pasiva (lectura de disco y compilación de verificación sin cambios). No se crearon commits, ramas, stashes ni modificaciones de archivos del repositorio.

---

## 3. Tabla Consolidada de Contradicciones y Hallazgos (P0 / P1 / P2)

| ID | Severidad | Componente / Área | Afirmación Auditada (8B.1A.5 / Docs) | Estado Real en Código / Plataforma | Clasificación |
| :--- | :---: | :--- | :--- | :--- | :---: |
| **P0-01** | **P0** | Manifiesto MSIX | Store declara únicamente `runFullTrust` (D-01). | `Package.appxmanifest` incluye `allowElevation` y `unvirtualizedResources`. | **CONTRADICCIÓN CRÍTICA** |
| **P0-02** | **P0** | Contenedor DI | `DefaultEditionProfileResolver` restringe mutaciones en Store. | `Program.cs` no registra ni consume `IEditionProfileResolver`. | **DESCONEXIÓN EN PRODUCCIÓN** |
| **P0-03** | **P0** | ConsentStore HKCU | Store no escribe en el registro y abre `ms-settings:`. | `WindowsUserPrivacyManager.cs` escribe en HKCU; 0 llamadas a `ms-settings:`. | **CONTRADICCIÓN FACTUAL** |
| **P0-04** | **P0** | Recuperación MSIX | El WAL de Store está aislado y protegido ante desinstalación. | WAL reside en `%LOCALAPPDATA%\PrivGuard\Recovery`; Reset/Uninstall lo destruye o deja hardware huérfano. | **RIESGO DE RECUPERACIÓN** |
| **P0-05** | **P0** | Ownership HKLM | HKLM contiene un Machine Recovery Escrow completo. | `WindowsPrivilegedOwnershipStore` solo almacena un hash SHA-256 (fingerprint), sin datos de reversión. | **CLASIFICACIÓN REFUTADA** |
| **P1-01** | **P1** | Core Audio WASAPI | `micStandardIsVerified` refleja protección probada. | Se marca `true` aunque `HardwareMuteSupport == NotSupported` (vulnerable a Exclusive Mode). | **SEMÁNTICA UI FALSA** |
| **P1-02** | **P1** | ProgramData Claims | Reclamos `.claim` resuelven exclusión mutua multiusuario. | Reclamos no existen en código; introducen DoS trivial (*Claim Squatting*) o carecen de autoridad. | **DISEÑO INVIABLE** |
| **P1-03** | **P1** | Sincronización | Mutex `Global\PrivGuard_Transaction_Gate` y `OwnerTuple`. | Mutex no existe en código (solo existe `PrivLock_PrivilegedOperation`); `OwnerTuple` no existe. | **TEÓRICO / NO IMPLEMENTADO** |
| **P2-01** | **P2** | Durabilidad WAL | Manejo de errores Win32 1175, 1176, 1177 en `File.Replace`. | `FilePrivacySessionStore.cs` usa `File.Replace` de .NET sin capturar códigos Win32 específicos. | **EXCESO DE ATRIBUCIÓN** |

---

## 4. Auditoría del Manifiesto Real y Flujo de Empaquetado (P0-01)

### 4.1 Inspección del Archivo `packaging/Package.appxmanifest`
El manifiesto fuente en el repositorio contiene en sus líneas 59 a 63:
```xml
<Capabilities>
  <rescap:Capability Name="runFullTrust" />
  <rescap:Capability Name="allowElevation" />
  <rescap:Capability Name="unvirtualizedResources" />
</Capabilities>
```

### 4.2 Trazabilidad del Script de Empaquetado `scripts/build-msix.ps1`
Al inspeccionar `scripts/build-msix.ps1` (líneas 495-503), se comprueba cómo se genera el paquete MSIX:
```powershell
[xml]$manifestXml = Get-Content -LiteralPath $ManifestSourcePath -Raw
$manifestXml.Package.Identity.Name = $PackageName
$manifestXml.Package.Identity.ProcessorArchitecture = $arch
$manifestXml.Package.Identity.Version = $PackageVersion
$manifestXml.Package.Identity.Publisher = $Publisher
$manifestXml.Package.Properties.PublisherDisplayName = $PublisherDisplayName

$targetManifestPath = Join-Path $stagingDir "AppxManifest.xml"
$manifestXml.Save($targetManifestPath)
```
**Hallazgo Técnico:** El script de empaquetado **no modifica ni filtra las capacidades**. Las tres capacidades restringidas (`runFullTrust`, `allowElevation`, `unvirtualizedResources`) pasan íntegramente al `AppxManifest.xml` empaquetado en el archivo `.msix` final por `MakeAppx.exe`.

### 4.3 Normativa de Microsoft Learn sobre Capacidades Restringidas
De acuerdo con la documentación oficial de Microsoft Learn (*App capability declarations / Restricted capabilities*):
1. **`allowElevation`:** Está restringida para aplicaciones empresariales o asociadas autorizadas. Requiere aprobación previa expresa por parte del equipo de revisión del Microsoft Store durante el proceso de certificación. Presentar un paquete con `allowElevation` sin un caso de negocio preaprobado resulta en el rechazo inmediato de la publicación. Además, declarar `allowElevation` no eleva el proceso principal (sigue corriendo como `asInvoker`); solo habilita la posibilidad de invocar verbos de elevación (`runas`) desde procesos empaquetados.
2. **`unvirtualizedResources`:** Por sí sola, la declaración de la capacidad **no tiene efecto operativo**. Para desactivar la virtualización del sistema de archivos en Windows 10 versión 2004 (build 19041) o superior, el manifiesto debe declarar explícitamente dentro del bloque `<Properties>` el elemento:
   ```xml
   <desktop6:FileSystemWriteVirtualization>disabled</desktop6:FileSystemWriteVirtualization>
   ```
   Al revisar `packaging/Package.appxmanifest`, se constata que dicho elemento **no está presente**. Por tanto, la aplicación sufre la virtualización estándar de Desktop Bridge para rutas de AppData, haciendo redundante e inútil la inclusión de `unvirtualizedResources`.

---

## 5. Comparación entre D-01 y las Capacidades Reales del Código (P0-03)

### 5.1 La Promesa de D-01 y la Justificación Store
En `docs/store/MICROSOFT-STORE-JUSTIFICATION.md` (Sección 3), el proyecto sostiene formalmente:
> *"No `unvirtualizedResources`: Store Edition does not attempt direct writes to `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore`. Instead, it provides one-click navigation to Windows native privacy settings (`ms-settings:privacy-webcam` and `ms-settings:privacy-microphone`)."*

### 5.2 La Realidad del Código Fuente en Producción
Se inspeccionó `WindowsUserPrivacyManager.cs` (líneas 22-68). El código ejecuta:
```csharp
internal OperationResult SetCameraUserPrivacy(BlockStatus status)
{
    var consentValue = isBlocked ? "Deny" : "Allow";
    SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraPath, ConsentValueName, consentValue);
    SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraNonPackagedPath, ConsentValueName, consentValue);
    return OperationResult.Ok();
}
```
Asimismo, una búsqueda en todo el código bajo `src/` confirma que **no existe una sola referencia al esquema `ms-settings:`**.

### 5.3 Consecuencias Técnicas
1. **Fallo de Virtualización:** En un proceso empaquetado bajo Desktop Bridge donde la virtualización del registro no ha sido desactivada explícitamente, las escrituras a `HKCU\Software` son interceptadas y redirigidas a un subárbol privado virtualizado (`User.dat` bajo el contenedor del paquete). El resto del sistema operativo y las aplicaciones no empaquetadas (Chrome, Zoom, etc.) jamás observan el cambio de consentimiento.
2. **Incoherencia Documental Crítica:** El documento de justificación enviado o preparado para Microsoft Store describe un producto inexistente. El código actual intenta modificar el registro local directamente.

---

## 6. Auditoría del DI y Rutas Reales de Almacenamiento (P0-02 / P0-04)

### 6.1 Desconexión en `src/PrivLock.Desktop/Program.cs`
Se auditó minuciosamente el método `ConfigureServices` en `Program.cs` (líneas 560-625):
- `IPackageIdentityDetector` **no está registrado**.
- `IEditionProfileResolver` / `DefaultEditionProfileResolver` **no están registrados**.
- Ningún servicio de dominio (`ProtectionService`) ni de plataforma (`WindowsProtectionProvider`) inyecta o consulta `EditionProfile.SupportsPrivilegedOperations`.

### 6.2 Rutas Físicas de Almacenamiento
En `Program.cs` (líneas 568-570):
```csharp
services.AddSingleton<IPrivacySessionStore>(serviceProvider =>
    new FilePrivacySessionStore(
        serviceProvider.GetRequiredService<IActivePrivacySessionMarker>()));
```
Al invocar el constructor con `customDirectory: null`, `FilePrivacySessionStore.cs` (línea 59) deriva la ruta mediante:
```csharp
var recoveryDirectory = customDirectory ?? Path.Combine(
    StorageMigrationHelper.GetDefaultDataDirectory(),
    "Recovery");
```
`StorageMigrationHelper.GetDefaultDataDirectory()` retorna `Path.Combine(localAppData, "PrivGuard")`.

Por tanto, las rutas físicas en ejecución para **ambas ediciones** son idénticas:
- **WAL Principal:** `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json`
- **Backup WAL:** `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json.bak`
- **Active Session Marker:** `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session.marker`
- **Settings / Estado:** `%LOCALAPPDATA%\PrivGuard\state.json`
- **Logs:** `%LOCALAPPDATA%\PrivGuard\Logs\privguard-.log`
- **Crash Reports:** `%LOCALAPPDATA%\PrivGuard\CrashReports\crash-.json`

**Conclusión:** No existe aislamiento de almacenamiento entre Store y Standalone. Ambas distribuciones compiten por el mismo archivo de journal si se instalan concurrentemente en el mismo perfil de usuario.

---

## 7. Análisis de Semántica de Core Audio y Estado de Verificación (P1-01)

### 7.1 Inspección de `WindowsProtectionProvider.cs`
En las líneas 222-227 de `WindowsProtectionProvider.cs`:
```csharp
var micStandardIsVerified = micStandardState != StandardProtectionState.Unknown &&
                            micEndpointAvailability != AudioEndpointAvailability.Unknown;
var micSecureIsVerified = micSecureState != SecureProtectionState.Unknown &&
                          micHardwareAvailability != TargetDeviceAvailability.Unknown;
var micIsVerified = micStandardIsVerified && micSecureIsVerified;
```
Aunque el proveedor calcula `micHwSummary` a partir de `QueryHardwareSupport`, **dicho resumen es completamente ignorado al establecer `micStandardIsVerified`**.

### 7.2 Comportamiento Documentado de WASAPI / `IAudioEndpointVolume`
Según Microsoft Learn (*IAudioEndpointVolume::QueryHardwareSupport* y *Endpoint Volume Controls*):
1. El bit `ENDPOINT_HARDWARE_SUPPORT_MUTE` (0x00000002) indica si el hardware de captura o su controlador en modo kernel implementan el silenciamiento físicamente.
2. Si el hardware **no** lo soporta (`HardwareMuteSupport.NotSupported`), la llamada a `IAudioEndpointVolume.SetMute(true)` es implementada por el motor de audio de Windows (WASAPI Software Audio Engine).
3. **El Bypass de Modo Exclusivo:** El motor de audio por software solo procesa flujos en *Shared Mode*. Si cualquier aplicación de grabación abre un stream en modo exclusivo (`AUDCLNT_SHAREMODE_EXCLUSIVE`), la aplicación se conecta directamente al búfer DMA del controlador de audio, **omitiendo por completo el software mute**.
4. **Falsa Garantía UI:** PrivGuard reporta el micrófono como "Protegido y Verificado" basándose en que `endpoint.GetMute() == true`, cuando en realidad la protección puede ser eludida trivialmente por cualquier software de captura que solicite modo exclusivo.

---

## 8. Análisis de `ProgramData` y Endpoint Claims (P1-02)

### 8.1 Permisos de Windows sobre `%ProgramData%`
De forma predeterminada en NTFS:
- `C:\ProgramData` otorga a `BUILTIN\Users` el permiso `FILE_ADD_SUBDIRECTORY` (puede crear subcarpetas), y `FILE_ADD_FILE` en subdirectorios heredados.
- Si un usuario estándar crea `C:\ProgramData\PrivGuard`, dicho usuario se convierte en `CREATOR OWNER` de la carpeta, pudiendo alterar su DACL y denegar el acceso a otros usuarios estándar.

### 8.2 El Dilema Ineludible de DoS vs. Exclusión Mutua
El diseño de claims en `ProgramData` para Store Edition plantea una contradicción de seguridad irresoluble:
1. **Si los archivos `.claim` son autoritativos:** Cualquier usuario estándar malicioso o proceso sin privilegios puede crear un archivo `mic-1.claim` para cada endpoint detectado. Cuando el usuario legítimo intente silenciar el micrófono, la aplicación interpretará que el dispositivo está tomado por otro usuario y se rehusará a operar (**Denial of Service permanente**).
2. **Si los archivos `.claim` son puramente cooperativos (no autoritativos):** La aplicación no puede garantizar que dos usuarios o instancias no estén mutando el estado simultáneamente, perdiendo la integridad de recuperación requerida por el modelo de journaling.
3. **Falta de Autoridad:** Un proceso que corre con derechos estándar (`asInvoker`) no puede autenticar criptográficamente la posesión de un recurso a nivel de máquina sin un servicio del sistema o un componente elevado con DACL protegida.

---

## 9. Ciclo de Vida Reset / Repair / Uninstall en MSIX (P0-04)

### 9.1 Matriz de Impacto en Almacenamiento y Recuperación

| Evento del SO | Efecto en Binarios | Efecto en `%LOCALAPPDATA%\PrivGuard` (Sin VFS) | Efecto en `LocalState` (Virtualizado) | Estado del Endpoint de Audio | Capacidad de Rollback |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Repair (Reparar)** | Reinstala y valida hashes de archivos del paquete. | Preservado intacto. | Preservado intacto. | Se mantiene muteado. | **Alta:** La app reinicia y lee el WAL. |
| **Reset (Restablecer)** | Restaura configuración inicial de fábrica. | Preservado (si está fuera del paquete). | **ELIMINADO por Windows.** | Se mantiene muteado en Core Audio. | **Nula / Destruida:** Si el WAL residía en el contenedor, se pierde para siempre. |
| **Uninstall (Desinstalar)**| Elimina el paquete del sistema. | Preservado (no administrado por MSIX). | **ELIMINADO por Windows.** | **Hardware Stuck:** El endpoint permanece muteado en el driver. | **Inexistente:** MSIX no admite scripts de pre-desinstalación; no hay reversión. |

### 9.2 Corrección Terminológica
El bloqueo de un micrófono vía `IAudioEndpointVolume.SetMute(true)` **no desconecta eléctricamente el hardware**. Si el usuario desinstala la aplicación Store mientras el micrófono está silenciado:
- El micrófono permanece en estado muteado en Windows.
- La recuperación no requiere herramientas especiales: el usuario puede reactivar el micrófono manualmente desde la barra de tareas de Windows, la app de Configuración (`ms-settings:sound`) o el Panel de Control (`mmsys.cpl`).
- El riesgo debe documentarse técnicamente como: **"Endpoint de captura que permanece en estado de software mute sin rollback automático por desinstalación del paquete."**

---

## 10. Diferencia entre Machine Ownership Store y Recovery Escrow (P0-05)

### 10.1 Inspección de `WindowsPrivilegedOwnershipStore.cs`
En las líneas 518-527, la estructura serializada en `HKLM\SOFTWARE\PrivLock\PrivilegedOwnership\v2` es:
```csharp
private sealed class StoredOwnershipRecord
{
    public int Version { get; set; }
    public required string ResourceKey { get; set; }
    public required string Fingerprint { get; set; }
    public required string OwnerUserSid { get; set; }
    public PrivilegedOwnershipStage Stage { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
```

### 10.2 Análisis Comparativo

| Característica | Implementación Real (`StoredOwnershipRecord`) | Concepto Teórico (`Machine Recovery Escrow`) |
| :--- | :--- | :--- |
| **Naturaleza** | Almacén de atestación / huella digital (Fingerprint). | Almacén completo de reversión transaccional. |
| **Datos Almacenados** | Hashes SHA-256 (`ResourceKey`, `Fingerprint`), SID, Timestamp. | Instance ID, Problem Code previo, Clase PnP, Valores de registro originales. |
| **Propósito** | Impedir que un usuario no autorizado o un WAL adulterado invoque el worker elevado. | Permitir que el sistema o un administrador restaure el hardware sin el WAL del usuario. |
| **Comportamiento si se borra el WAL** | **Falla Cerrado:** La máquina no puede revertir porque no conoce el estado previo ni el ID del dispositivo. | **Recuperable:** El escrow contiene el snapshot necesario para ejecutar la reversión. |

**Corrección de Código Registrada:** `WindowsPolicyManager` escribe valores `RegistryValueKind.DWord` (valor numérico `2`). El informe 8B.1A.5 describía incorrectamente este valor como `REG_SZ`.

---

## 11. Auditoría y Reclasificación Exhaustiva de la Matriz de 54 Escenarios

A continuación se auditan de forma individual los 54 escenarios evaluados en la Fase 8B.1A.5, asignando una clasificación de evidencia estricta basada en el código real en `9f856c0`:

| ID | Categoría | Escenario Auditado | Clasificación en 8B.1A.5 | Clasificación Corregida | Archivo / Método / Línea Relevante | Justificación Factual y Bloqueo Pendiente |
| :---: | :--- | :--- | :---: | :---: | :--- | :--- |
| **01** | Store-Only | Instalación limpia sin Standalone. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Depende de arquitectura de perfiles no cableada en DI. |
| **02** | Provisioning | Standalone instalado primero; luego Store. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Latch público y detección mutua no existen en código. |
| **03** | Provisioning | Store instalado primero; luego Standalone. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | No existe handshake ni señalización inter-edición. |
| **04** | Provisioning | ProgramData root no existe. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | `none` | Comportamiento del shell de Windows al resolver `FOLDERID_ProgramData`. |
| **05** | Permisos | ACL de ProgramData incompatible. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | `none` | Windows NTFS ACLs estándar (`UnauthorizedAccessException`). |
| **06** | Permisos | `BUILTIN\Users` sin permiso `CreateFiles`. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | `none` | Seguridad NTFS de Windows sobre All Users profile. |
| **07** | Seguridad | Claim Squatting: Usuario B pre-crea claim falso. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Reclamos no implementados; riesgo DoS irresoluble sin broker. |
| **08** | Concurrencia | Dos usuarios crean el mismo claim a la vez. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | `none` | `FileStream(..., FileMode.CreateNew)` garantiza atomicidad NTFS. |
| **09** | Integridad | Claim con endpoint hash inválido / corrupto. | `CONFIRMADO EN CÓDIGO` | **INVALID / CONTRADICTS SOURCE** | `none` | **FALSO POSITIVO EN 8B.1A.5**: Claims no existen en el código fuente. |
| **10** | Seguridad | Reparse Point / Hardlink malicioso en `.claim`. | `CONFIRMADO EN CÓDIGO` | **INVALID / CONTRADICTS SOURCE** | `none` | **FALSO POSITIVO EN 8B.1A.5**: No existe código que verifique claims. |
| **11** | Seguridad | NTFS Owner no coincide con `OwnerUserSid` JSON. | `DOCUMENTADO POR MICROSOFT` | **DESIGN ONLY** | `none` | La verificación de propietario NTFS sobre claims es una propuesta teórica. |
| **12** | Core Audio | Store mute con hardware mute `Supported`. | `DOCUMENTADO POR MICROSOFT` | **PARTIAL — CODE EXISTS** | `WindowsCoreAudioController.cs` L:75-80 | Detecta el bit, pero la UI lo reporta igual que si fuera NotSupported. |
| **13** | Core Audio | Store mute con hardware mute `NotSupported`. | `DOCUMENTADO POR MICROSOFT` | **PARTIAL — CODE EXISTS** | `WindowsProtectionProvider.cs` L:222-226 | Se aplica mute pero se oculta a la UI que es Software-Only. |
| **14** | Core Audio | Store mute con hardware mute `Unknown`. | `CONFIRMADO EN CÓDIGO` | **IMPLEMENTED — CODE VERIFIED** | `WindowsCoreAudioController.cs` L:81-87 | Manejo seguro de HRESULT negativo registrando `Unknown`. |
| **15** | Core Audio | Captura en Shared Mode mientras mute activo. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn WASAPI | El mezclador de Windows mezcla muestras de silencio. |
| **16** | Core Audio | Captura en Exclusive Mode con software mute. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn WASAPI | **Bypass inevitable**: Exclusive Mode salta el software mute. |
| **17** | Core Audio | Dos apps de captura en la misma audio session. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn WASAPI | Alcance de audio session en Windows Core Audio. |
| **18** | Core Audio | Dos sesiones de audio diferentes en misma cuenta. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn WASAPI | El EndpointVolume afecta a todo el endpoint a nivel maestro. |
| **19** | Multiusuario | Dos logon sessions de Windows distintas. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn Core Audio | El endpoint maestro es global a todas las sesiones de inicio de sesión. |
| **20** | Multiusuario | Fast User Switching (A cede a B). | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn Core Audio | El estado muteado del endpoint persiste a través de FUS. |
| **21** | Multiusuario | Sesión remota RDP con redirección de audio. | `PENDIENTE DE VM` | **VM REQUIRED** | `none` | Requiere validar cómo Windows Virtual Desktop crea endpoints dinámicos. |
| **22** | Multiusuario | Usuario A mutea endpoint y cambia a Usuario B. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Señalización inter-usuario de Store no implementada. |
| **23** | Multiusuario | Usuario B intenta hacer unmute del endpoint de A. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Bloqueo cruzado en Store es solo diseño. |
| **24** | Multiusuario | Usuario A cierra sesión (Logoff) con mute activo. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn Core Audio | El endpoint permanece muteado tras logoff. |
| **25** | Multiusuario | Usuario A vuelve a iniciar sesión tras logoff. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Reconexión de Store con WAL no probada para edición Store. |
| **26** | Multiusuario | Usuario A elimina su cuenta de Windows. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Windows User Profile Service | Perfil de usuario destruido; WAL destruido; hardware stuck. |
| **27** | Crash | Store crashea tras ejecutar `SetMute(true)`. | `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `PrivacyRecoveryService.cs` L:120-145 | Existe recuperación en Standalone; Store no está cableado por DI. |
| **28** | Crash | Store crashea antes de crear el `.claim`. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Claims no implementados. |
| **29** | Crash | Store crashea tras crear claim y antes de mute. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Claims no implementados. |
| **30** | Crash | Store crashea tras `SetMute` antes de WAL terminal. | `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `FilePrivacySessionStore.cs` L:170-182 | Reversión basada en ActiveSessionMarker existe en Standalone. |
| **31** | Lifecycle | Store se actualiza vía Windows Store con mute. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn MSIX | Actualizaciones de MSIX preservan el contenido de `LocalAppData`. |
| **32** | Lifecycle | Store se repara vía Windows Settings con mute. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn MSIX | La operación Repair de Windows preserva datos de usuario. |
| **33** | Lifecycle | Store se resetea vía Windows Settings con mute. | `DOCUMENTADO POR MICROSOFT` | **VM REQUIRED** | `none` | Si el WAL está en `LocalCache`, se borra. Requiere prueba empírica. |
| **34** | Lifecycle | Store se desinstala con mute activo. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Microsoft Learn MSIX | MSIX no tiene uninstaller hooks; el endpoint queda muteado. |
| **35** | Coherencia | WAL permanece pero claim desapareció. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Claims no implementados. |
| **36** | Coherencia | Claim permanece pero WAL desapareció. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Claims no implementados. |
| **37** | Coexistencia | Machine Latch activo en Standalone; Store inicia. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | D-04 (SupervisionOnly) no está implementada en código. |
| **38** | Coexistencia | Store claim activo; Standalone pide Secure. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Señalización inter-edición no implementada. |
| **39** | Coexistencia | Standalone Secure activo; Store intenta Standard. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Barrera inter-edición no implementada. |
| **40** | Integridad | Machine Latch corrupto y backup `.bak` válido. | `CONFIRMADO EN CÓDIGO` | **INVALID / CONTRADICTS SOURCE** | `none` | **FALSO POSITIVO**: Machine Latch no existe. Solo existe el WAL de usuario. |
| **41** | Integridad | Machine Latch principal y backup corruptos. | `CONFIRMADO EN CÓDIGO` | **INVALID / CONTRADICTS SOURCE** | `none` | **FALSO POSITIVO**: Machine Latch no existe en el código fuente. |
| **42** | Escrow | Escrow PnP completo con rollback forzado. | `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `WindowsPrivilegedOwnershipStore.cs` L:134-150 | Almacena solo un fingerprint SHA-256; requiere el WAL del usuario para restaurar. |
| **43** | Escrow | Escrow HKLM completo con rollback forzado. | `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `WindowsPrivilegedOwnershipStore.cs` L:109-132 | Almacena solo un fingerprint SHA-256; requiere el WAL del usuario para restaurar. |
| **44** | Políticas | GPO corporativa altera HKLM tras mutación. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `WindowsPolicyManager.cs` | Regla compare-before-restore existe parcialmente para GPO, no generalizada. |
| **45** | Hardware | Rollback PnP falla parcialmente en cámara 2. | `CONFIRMADO EN CÓDIGO` | **IMPLEMENTED — CODE VERIFIED** | `WindowsDeviceController.cs` L:180-210 | Detalle por dispositivo (`DeviceOperationDetail`) y fallo seguro. |
| **46** | Hardware | Endpoint USB desconectado durante `Active`. | `CONFIRMADO EN CÓDIGO` | **IMPLEMENTED — CODE VERIFIED** | `WindowsProtectionProvider.cs` L:214-221 | Evaluación de presencia dinámica maneja dispositivos desconectados. |
| **47** | Sincronización| Transaction Gate Mutex ocupado por app colgada. | `CONFIRMADO EN CÓDIGO` | **INVALID / CONTRADICTS SOURCE** | `none` | **FALSO POSITIVO**: `Transaction_Gate` no existe; solo existe `PrivLock_PrivilegedOperation`. |
| **48** | Concurrencia | Mutex de UI global impide segunda sesión. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `WindowsSingleInstanceGuard.cs` | Usa mutex por usuario/sesión, no existe esquema inter-edición. |
| **49** | Seguridad | `OpenProcess` retorna `ERROR_ACCESS_DENIED`. | `DOCUMENTADO POR MICROSOFT` | **DOCUMENTED** | Win32 API Contract | Acceso denegado impide inspeccionar procesos de otra sesión o protegidos. |
| **50** | Seguridad | PID reciclado con misma SID pero diferente LUID. | `DOCUMENTADO POR MICROSOFT` | **DESIGN ONLY** | `none` | `OwnerTuple` completa con LUID no está implementada en código. |
| **51** | Worker | Worker privilegiado pierde Named Pipe. | `CONFIRMADO EN CÓDIGO` | **IMPLEMENTED — CODE VERIFIED** | `WindowsPrivilegedWorker.cs` L:240-280 | El worker finaliza inmediatamente si el pipe se desconecta o cae. |
| **52** | Desinstalación| Desinstalador Inno Setup con claim Store ajeno. | `DISEÑO PROPUESTO` | **DESIGN ONLY** | `none` | Inno Setup no tiene comprobación de claims de Store. |
| **53** | Desinstalación| Desinstalador Inno Setup con Machine Latch activo.| `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `setup.iss` L:34; `Program.cs` L:135-145 | Usa `WindowsUninstallRequestListener` y el WAL local, no un Machine Latch. |
| **54** | Energía | Reinicio abrupto (BSOD / corte) en mutación. | `CONFIRMADO EN CÓDIGO` | **PARTIAL — CODE EXISTS** | `FilePrivacySessionStore.cs` L:172-182 | Basado en el WAL con `WriteThrough` y `.tmp`/`.bak`, no en un Machine Latch. |

---

## 12. Comparativa de Opciones de Producto y Recomendación Provisional

| Criterio de Decisión | Opción 1: Store como Cliente Complementario de Standalone | Opción 2: Store Best-Effort (Standard Protection Únicamente) | Opción 3: Standalone Únicamente (Pausar Store) |
| :--- | :--- | :--- | :--- |
| **Modelo Arquitectónico** | La app Store actúa como frontend IPC hacia un broker elevado provisto por Standalone. | La app Store opera aislada, limitándose a Core Audio mute y apertura de `ms-settings:`. | Se distribuye exclusivamente mediante Inno Setup / Portable (`PrivGuard.exe` Win32). |
| **Viabilidad en Microsoft Store** | **Baja / Incierta:** Requiere que el usuario instale un software externo para funcionar completamente. | **Media:** Requiere eliminar `allowElevation` y `unvirtualizedResources` del manifiesto. | **N/A:** Fuera de la tienda. |
| **Complejidad de Implementación** | **Muy Alta:** Requiere diseñar broker IPC local, protocolo autenticado, empaquetado dual. | **Media-Baja:** Limpiar manifiesto, conectar DI, abrir `ms-settings:` y corregir semántica UI. | **Mínima:** La arquitectura Standalone ya está implementada y verificada en un 95%. |
| **Riesgo de Recuperación** | Medio: Delegado al motor de recuperación de Standalone. | **Alto:** Si el usuario resetea o desinstala Store mientras está muteado, el audio queda silenciado. | **Nulo / Controlado:** Inno Setup ejecuta el rollback obligatorio antes de desinstalar. |
| **Garantía Frente a Exclusive Mode** | Total (vía PnP de Standalone). | **Nula:** Vulnerable a bypass por aplicaciones en modo exclusivo. | Total (vía CfgMgr32 PnP DevNode disable). |

### Recomendación Provisional del Auditor
> [!IMPORTANT]
> **Recomendación:** Adoptar de forma inmediata la **Opción 3 (Standalone Únicamente)** como hito de entrega a corto plazo, manteniendo Microsoft Store en **CONDITIONAL HOLD**.  
> Intentar habilitar Store en este momento requiere reescribir la interacción con `ms-settings:`, rediseñar el manifiesto MSIX, conectar el DI de perfiles y asumir contractualmente que la protección de micrófono es vulnerable a Exclusive Mode. La versión Standalone Win32 cuenta con una base sólida de journaling, elevación bajo demanda autenticada y recuperación verificada.

---

## 13. Bloqueos que Impiden Avanzar a la Fase 8B.1C

Para que la Fase 8B.1C pueda ser autorizada, deben resolverse los siguientes bloqueos estructurales:
1. **Bloqueo B-01 (Manifiesto):** Resolver si el paquete MSIX eliminará `allowElevation` y `unvirtualizedResources` (cumpliendo D-01) o si se mantendrá como un paquete empresarial fuera de Store.
2. **Bloqueo B-02 (Cableado DI):** Conectar `WindowsPackageIdentityDetector` y `DefaultEditionProfileResolver` en `Program.ConfigureServices`, asegurando que `WindowsProtectionProvider` rechace mutaciones de PnP y HKLM si `SupportsPrivilegedOperations == false`.
3. **Bloqueo B-03 (Semántica de ConsentStore):** Eliminar las llamadas directas de escritura a `HKCU` en la versión de Store y sustituirlas por navegación asistida a `ms-settings:privacy-webcam` y `ms-settings:privacy-microphone`.
4. **Bloqueo B-04 (Semántica UI en Core Audio):** Modificar el cálculo de `micStandardIsVerified` para que no reporte "Verified" a dispositivos con `HardwareMuteSupport.NotSupported`.
5. **Bloqueo B-05 (Aislamiento de Almacenamiento):** Inyectar directorios de datos específicos en `FilePrivacySessionStore` para evitar colisiones entre Store y Standalone en `%LOCALAPPDATA%\PrivGuard\Recovery`.

---

## 14. Decisiones que Requieren Aprobación Expresa del Usuario

1. **Aprobación de la Estrategia de Manifiesto MSIX:** ¿Se autoriza la eliminación de `<rescap:Capability Name="allowElevation" />` y `<rescap:Capability Name="unvirtualizedResources" />` en `packaging/Package.appxmanifest` para someter la app a Microsoft Store estrictamente como Standard/asInvoker?
2. **Aceptación del Riesgo de Modo Exclusivo en Store:** ¿Acepta el propietario del producto que la versión de Microsoft Store no pueda garantizar el bloqueo de micrófono contra aplicaciones en `AUDCLNT_SHAREMODE_EXCLUSIVE`, documentándolo de forma transparente en la interfaz de usuario?
3. **Adopción de Opción 3 (Standalone First):** ¿Se aprueba priorizar el release de la versión Win32 Standalone (Inno Setup) mientras se resuelve la certificación y aislamiento de la versión Store?

---

## 15. Fuentes Oficiales de Microsoft Learn

- **Restricted Capabilities:**  
  *Special and restricted capabilities in package manifests*  
  URL: `https://learn.microsoft.com/en-us/windows/uwp/packaging/app-capability-declarations#restricted-capabilities`  
  *Evidencia:* `allowElevation` requiere aprobación estricta de Store; `unvirtualizedResources` requiere propiedades de virtualización complementarias.
- **MSIX Flexible Virtualization:**  
  *Disable registry and file system write virtualization in packaged desktop apps*  
  URL: `https://learn.microsoft.com/en-us/windows/msix/desktop/flexible-virtualization`  
  *Evidencia:* Declara que `FileSystemWriteVirtualization` bajo `<Properties>` es indispensable para omitir la redirección VFS.
- **Core Audio APIs & Hardware Support:**  
  *IAudioEndpointVolume::QueryHardwareSupport method*  
  URL: `https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nf-endpointvolume-iaudioendpointvolume-queryhardwaresupport`  
  *Evidencia:* El flag `ENDPOINT_HARDWARE_SUPPORT_MUTE` (0x2) distingue silenciamiento en hardware/driver de silenciamiento por software en el motor WASAPI.
- **Exclusive-Mode Audio Streams:**  
  *Exclusive-Mode Streams in Windows Core Audio*  
  URL: `https://learn.microsoft.com/en-us/windows/win32/coreaudio/exclusive-mode-streams`  
  *Evidencia:* Las aplicaciones en modo exclusivo no pasan por el mezclador de software del sistema; el software mute no las silencia.
- **MSIX Application Lifecycle & Reset:**  
  *Repair or remove programs in Windows / Reset app package data*  
  URL: `https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#reset-and-repair-apps`  
  *Evidencia:* Reset elimina todo el contenido de los directorios gestionados por el paquete; MSIX carece de hooks de desinstalación.

---

## 16. Estado de Cierre de la Auditoría

- **Archivos Modificados en el Repositorio:** 0 (cero).
- **Commits Creados:** 0 (cero).
- **Ramas Creadas:** 0 (cero).
- **Comprobación Final:** `git status --short` limpio; `git diff --check` limpio.
- **Microsoft Store Status:** **CONDITIONAL HOLD**.
- **Fase 8B.1C:** **BLOCKED**.

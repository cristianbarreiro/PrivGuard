# PrivGuard — Fase 8B.1A.6.1
## Revisión Correctiva del Informe 8B.1A.6 y Validación de Evidencia MSIX, Almacenamiento, Recuperación y Contrato de Protección

**Fecha de Ejecución:** 6 de Noviembre de 2026  
**Auditor Principal:** Auditor Senior Adversarial de Plataforma Windows, MSIX y Seguridad de Sistemas  
**Repositorio:** `cristianbarreiro/PrivGuard`  
**Commit Auditado:** `9f856c0405e4e5b64e784309affad450c38d3a37` (HEAD de `main`, en sincronía con `origin/main`)  
**Fase Previa:** 8B.1A.6  
**Fase Siguiente:** 8B.1C — **ESTRICTAMENTE BLOQUEADA / NO AUTORIZADA**  
**Estado de Microsoft Store:** **`CONDITIONAL HOLD`**  
**Ubicación del Artefacto:** Fuera del repositorio (Área de Artefactos de Antigravity)  

---

## 1. Resumen Ejecutivo y Veredicto Formal

### 1.1 Resumen Ejecutivo
Esta fase de auditoría técnica y documental correctiva (Fase **8B.1A.6.1**) reexamina con rigor adversarial las conclusiones, afirmaciones y categorizaciones del informe **8B.1A.6**, contrastándolas directamente contra:
1. El código fuente real en el commit `9f856c0`.
2. El manifiesto fuente (`packaging/Package.appxmanifest`) y los artefactos `.msix` efectivamente generados (`publish_out/msix/PrivGvard-2.0.0.0-x64.msix`).
3. La documentación oficial vigente de Microsoft Learn sobre capacidades restringidas, Desktop Bridge, virtualización flexible y Core Audio (WASAPI).
4. El script del desinstalador Standalone (`installer/setup.iss`) y el flujo de `WindowsUninstallRequestListener`.

#### Conclusiones Principales de la Revisión:
- **Confirmación de Contradicciones P0:** Se ratifican como hechos probados en código y manifiesto las contradicciones críticas:
  - `packaging/Package.appxmanifest` declara `allowElevation` y `unvirtualizedResources`, lo cual contradice la Decisión de Arquitectura **D-01** y la justificación pública de Store.
  - La inspección directa del binario empaquetado `.msix` demuestra que ambas capacidades restringidas se incluyen en el paquete final distribuible.
  - El contenedor de DI en `Program.cs` **no registra** `IEditionProfileResolver` ni `IPackageIdentityDetector`, provocando que Store y Standalone compartan código y rutas de almacenamiento no aisladas.
  - `WindowsUserPrivacyManager.cs` escribe directamente en `HKCU`, sin invocar `ms-settings:`.
- **Correcciones Críticas al Informe 8B.1A.6:**
  - *Partner Center y `allowElevation`:* El informe 8B.1A.6 sugería que la presencia de `allowElevation` causa un "rechazo automático inmediato". La documentación oficial de Microsoft indica que las capacidades restringidas están sujetas a un proceso de revisión y justificación; no se rechazan automáticamente de plano, sino durante la evaluación si no existe justificación aceptable.
  - *Virtualización de AppData por Versión de Windows:* La regla de virtualización depende críticamente de la versión de Windows. En Windows 10 versión 1903/19041+, los archivos creados de novo se virtualizan en `Packages\<PFN>\LocalCache\Local`, pero las modificaciones a archivos preexistentes en `AppData` no virtualizado pueden impactar directamente la ruta nativa. Por ende, la ruta física exacta del WAL no puede deducirse por pura lógica de .NET sin una prueba en VM.
  - *Desinstalador Standalone (Inno Setup):* El informe 8B.1A.6 sobrestimó la garantía del rollback en Standalone llamándola "riesgo nulo / 95% implementado". El análisis de `setup.iss` demuestra que si otra cuenta de Windows tiene una sesión activa, el desinstalador **aborta la desinstalación** mediante mensaje de error crítico, requiriendo intervención manual del usuario. No existe "rollback automático universal".
  - *Core Audio y Exclusive Mode:* La documentación oficial de WASAPI establece que el silenciamiento de software a nivel de endpoint (`IAudioEndpointVolume`) es implementado por el motor de audio de Windows (audio engine). Los flujos en `AUDCLNT_SHAREMODE_EXCLUSIVE` omiten el motor de audio y, por tanto, omiten el software mute. No debe afirmarse "acceso directo al DMA" sin citar la especificación: la formulación rigurosa es que el modo exclusivo **omite los controles de software del endpoint**.
  - *Machine Ownership Store vs Escrow:* Se ratifica que `WindowsPrivilegedOwnershipStore` es exclusivamente un almacén de atestación (fingerprint SHA-256) y no un Recovery Escrow, pues no guarda los datos originales necesarios para revertir PnP ni HKLM sin el WAL del usuario.

### 1.2 Veredicto Formal

> [!CAUTION]
> **VEREDICTO: HOLD — CONTRADICCIONES P0 PERSISTENTES Y EVIDENCIA FÍSICA PENDIENTE DE VM**
> 
> 1. Se prohíbe el paso a la Fase **8B.1C**.
> 2. Se ratifica el estado de **`CONDITIONAL HOLD`** para la publicación en Microsoft Store.
> 3. Las contradicciones entre manifiesto, D-01 y DI deben resolverse formalmente antes de cualquier implementación.
> 4. Toda afirmación relativa al comportamiento físico de Reset/Uninstall, redirección de AppData en runtime y aislamiento de almacenamiento entre ediciones queda clasificada como **`VM REQUIRED` / `NOT VERIFIED`** hasta disponer de una ejecución en VM desechable aislada.

---

## 2. Commit, Rama, Estado Git Real y Alcance de la Inspección

### 2.1 Registro de Integridad Git (Preflight y Postflight)
- **Repositorio:** `c:\Users\Cristian\Documents\GitHub\PrivGuard`
- **Proyecto Canónico:** `cristianbarreiro/PrivGuard`
- **Rama Activa:** `main`
- **HEAD Local:** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **HEAD Remoto (`origin/main`):** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **Estado de Sincronización:** Idéntico (0 commits adelante, 0 atrás; comprobado vía `git fetch origin`).
- **Working Tree:** Completamente limpio (`git status --short` vacío, `git diff --stat` vacío, `git diff --check` limpio).
- **Último Commit:**
  ```text
  commit 9f856c0405e4e5b64e784309affad450c38d3a37
  Date: Thu Nov 5 23:57:20 2026 +0100
  Subject: feat(security,packaging): enforce fail-closed enum defaults, immutable resolver snapshot and strict NoOp detector (Phase 8B.1B.3)
  ```
- **Alcance de la Inspección:** Análisis estático de solo lectura del código fuente, inspección binaria de archivos MSIX existentes, lectura de scripts Inno Setup y contraste con documentación oficial de Microsoft Learn. Cero modificaciones en el repositorio.

---

## 3. Inventario de Fuentes de Entrada

| Documento / Fuente | Ubicación | Estado | Observación |
| :--- | :--- | :---: | :--- |
| `INFORME-FASE-8B1A6-CORRECCION-...` | Artifacts Directory (`.../brain/1e56214b.../`) | **DISPONIBLE** | Leído íntegramente; objeto de esta revisión. |
| `INFORME-FASE-8B1A5-VIABILIDAD-...` | Artifacts Directory (`.../brain/1e56214b.../`) | **DISPONIBLE** | Leído íntegramente; matriz de 54 escenarios base. |
| `packaging/Package.appxmanifest` | Repositorio (`packaging/`) | **DISPONIBLE** | Manifiesto fuente inspeccionado. |
| `scripts/build-msix.ps1` | Repositorio (`scripts/`) | **DISPONIBLE** | Pipeline de packaging inspeccionado. |
| `publish_out/msix/PrivGvard-2.0.0.0-x64.msix` | Repositorio (`publish_out/msix/`) | **DISPONIBLE** | Artefacto real inspeccionado internamente. |
| `docs/store/MICROSOFT-STORE-JUSTIFICATION.md` | Repositorio (`docs/store/`) | **DISPONIBLE** | Contiene D-01 y D-02 aprobadas. |
| `INFORME-FASE-8B1A1-CIERRE-DISENO-...` | Artifacts Directory (`.../brain/1e56214b.../`) | **DISPONIBLE** | Contiene definiciones de D-04 y D-05. |
| `installer/setup.iss` | Repositorio (`installer/`) | **DISPONIBLE** | Script Inno Setup de Standalone inspeccionado. |
| Entorno de VM desechable | N/A | **NO DISPONIBLE** | No se ejecutaron pruebas en el host. Pruebas dinámicas marcadas `VM REQUIRED`. |

---

## 4. Matriz de Afirmaciones Auditadas de la Fase 8B.1A.6

A continuación se evalúa cada hallazgo del informe anterior bajo el criterio adversarial:

### P0-01: Manifiesto MSIX (`allowElevation` y `unvirtualizedResources`) vs. D-01
- **Afirmación Auditada:** `packaging/Package.appxmanifest` incluye `allowElevation` y `unvirtualizedResources`, contradiciendo D-01 y causando rechazo automático en Store.
- **Resultado:** **PARCIALMENTE CONFIRMADA / CORREGIDA**.
- **Evidencia de Código:**
  - `packaging/Package.appxmanifest` (líneas 59-63).
  - Inspección binaria de `publish_out\msix\PrivGvard-2.0.0.0-x64.msix`: su `AppxManifest.xml` interno contiene exactamente `runFullTrust`, `allowElevation`, `unvirtualizedResources`.
- **Evidencia de Windows:** Microsoft Learn (*App capability declarations*). Las capacidades restringidas requieren proceso de revisión de Partner Center y justificación. No existe rechazo *automático* ciego, sino *rechazo durante la certificación* si la justificación no es aceptada.
- **Evidencia de Ejecución:** `NOT AVAILABLE` (sin VM).
- **Impacto:** Si se sube a Store, fallará la revisión técnica porque no hay caso de negocio para elevación en la Store Edition bajo D-01.
- **Corrección:** Sustituir la afirmación de "rechazo automático en el upload" por "rechazo en la fase de revisión de capacidades restringidas de Store por incumplimiento de justificación según D-01".
- **Acción Pendiente:** Modificar `Package.appxmanifest` eliminando ambas capacidades en una fase autorizada futura.

### P0-02: Desconexión del Contenedor DI en `Program.cs`
- **Afirmación Auditada:** `IEditionProfileResolver` y `IPackageIdentityDetector` no están registrados en `ConfigureServices`; Store y Standalone corren el mismo código y comparten el WAL.
- **Resultado:** **CONFIRMADA**.
- **Evidencia de Código:** `src/PrivLock.Desktop/Program.cs` (líneas 560-625). `ConfigureServices` carece de registros para perfiles. `FilePrivacySessionStore` se instancia sin `customDirectory`, derivando la ruta de `StorageMigrationHelper`.
- **Evidencia de Windows:** N/A (específico de arquitectura .NET).
- **Evidencia de Ejecución:** `NOT AVAILABLE`.
- **Impacto:** El aislamiento de perfiles de la Fase 8B.1B.3 no está operativo en la aplicación de producción.
- **Corrección:** Ninguna; la afirmación de 8B.1A.6 es exacta.
- **Acción Pendiente:** Registrar los servicios y conectar la barrera de mutación en `WindowsProtectionProvider`.

### P0-03: Escritura Directa a ConsentStore en HKCU vs. Justificación Store
- **Afirmación Auditada:** La justificación afirma que Store no escribe en el registro y abre `ms-settings:`, pero el código escribe directamente en `HKCU` y carece de llamadas a `ms-settings:`.
- **Resultado:** **CONFIRMADA**.
- **Evidencia de Código:** `WindowsUserPrivacyManager.cs` (líneas 22-68). Búsqueda de `ms-settings:` en `src/` arrojó 0 coincidencias.
- **Evidencia de Windows:** Microsoft Learn (*Flexible Virtualization*). Las escrituras de FullTrust sin `RegistryWriteVirtualization=disabled` son redirigidas a la hive privada del paquete (`User.dat`).
- **Evidencia de Ejecución:** `NOT AVAILABLE`.
- **Impacto:** La justificación ante Microsoft es falsa frente al código; en ejecución empaquetada, las escrituras a ConsentStore no afectan a aplicaciones externas.
- **Corrección:** Ninguna; afirmación exacta.
- **Acción Pendiente:** Implementar `Process.Start("ms-settings:privacy-webcam")` y eliminar escrituras a registro en Store.

### P0-04: Destrucción de WAL en Ciclo de Vida MSIX (Reset / Uninstall)
- **Afirmación Auditada:** El WAL se destruye en Reset/Uninstall dejando el micrófono silenciado sin posibilidad de rollback ("Hardware Stuck").
- **Resultado:** **PARCIAL / PENDIENTE DE VM**.
- **Evidencia de Código:** `FilePrivacySessionStore.cs` (línea 59).
- **Evidencia de Windows:** Microsoft Learn (*Reset and repair apps*). Reset borra los datos administrados por el paquete (`LocalState` / `LocalCache`). Uninstall elimina el paquete. Si el WAL residiera en AppData no virtualizado externo, Reset podría no borrarlo.
- **Evidencia de Ejecución:** `VM REQUIRED`. No se puede confirmar físicamente qué directorio exacto borra Windows sin una prueba en VM limpia.
- **Impacto:** Riesgo severo de dejar endpoints muteados si el WAL es destruido por el SO.
- **Corrección:** Matizar que la destrucción del WAL depende de si la ruta física efectiva quedó virtualizada bajo `LocalCache` o en AppData nativo. Mantener como `VM REQUIRED`.
- **Acción Pendiente:** Ejecutar protocolo en VM desechable.

### P0-05: Machine Ownership Store es Fingerprint, no Recovery Escrow
- **Afirmación Auditada:** HKLM contiene únicamente un hash SHA-256 (`Fingerprint`), no un Recovery Escrow capaz de revertir el hardware si se borra el WAL.
- **Resultado:** **CONFIRMADA**.
- **Evidencia de Código:** `WindowsPrivilegedOwnershipStore.cs` (líneas 518-527, `StoredOwnershipRecord`).
- **Evidencia de Windows:** N/A.
- **Evidencia de Ejecución:** `NOT AVAILABLE`.
- **Impacto:** No es posible un rollback a nivel de máquina sin el archivo de journal del usuario.
- **Corrección:** Ninguna; afirmación exacta.
- **Acción Pendiente:** Diseñar un escrow completo en HKLM si se aprueba el soporte multiusuario desatendido.

### P1-01: Semántica UI en Core Audio y Exclusive Mode Bypass
- **Afirmación Auditada:** `micStandardIsVerified = true` se activa ignorando si el hardware soporta mute, exponiendo al usuario a capturas no silenciadas en Exclusive Mode.
- **Resultado:** **CONFIRMADA**.
- **Evidencia de Código:** `WindowsProtectionProvider.cs` (líneas 222-226).
- **Evidencia de Windows:** Microsoft Learn (*Exclusive-Mode Streams* e *IAudioEndpointVolume::QueryHardwareSupport*). Flujos exclusivos omiten el motor de audio por software.
- **Evidencia de Ejecución:** `NOT AVAILABLE`.
- **Impacto:** Falsa sensación de privacidad; la UI promete "Verified" en escenarios eludibles por software de audio profesional.
- **Corrección:** Mantener la afirmación; corregir la redacción para indicar que el bypass ocurre porque los flujos exclusivos omiten el motor de audio de Windows.
- **Acción Pendiente:** Degradar el estado en UI a `Partial — software mute (shared mode)` cuando `HardwareMuteSupport == NotSupported`.

### P1-02: ProgramData Claims y el Dilema de DoS
- **Afirmación Auditada:** Claims en `ProgramData` son inviables para cuentas estándar sin un broker elevado debido al riesgo de DoS (*Claim Squatting*).
- **Resultado:** **CONFIRMADA / MATIZADA**.
- **Evidencia de Código:** Ninguna (los claims no existen en código).
- **Evidencia de Windows:** Permisos NTFS de `C:\ProgramData`. `Users` pueden crear carpetas y convertirse en `CREATOR OWNER`.
- **Evidencia de Ejecución:** `NOT AVAILABLE`.
- **Impacto:** El diseño de claims en espacio de usuario propuesto en 8B.1A.5 es inseguro.
- **Corrección:** Clarificar que no es un problema "irresoluble en la informática", sino irresoluble *sin un componente elevado que aprovisione y proteja la DACL del directorio*.
- **Acción Pendiente:** Descartar claims basados en archivos de usuario; adoptar broker o mutexes de sincronización.

---

## 5. Manifiesto Fuente vs. Manifiesto Empaquetado y Pipeline de Build

### 5.1 Comparativa Factual de Manifiestos

| Elemento XML | Manifiesto Fuente (`Package.appxmanifest`) | Manifiesto en `publish_out/.../staging-x64/` | Manifiesto Dentro del `.msix` Compilado | Estado de Coherencia |
| :--- | :--- | :--- | :--- | :---: |
| `EntryPoint` | `Windows.FullTrustApplication` | `Windows.FullTrustApplication` | `Windows.FullTrustApplication` | **IDÉNTICO** |
| `Identity/Name` | `cdevstudios.PrivGuard` | `cdevstudios.PrivGvard` | `cdevstudios.PrivGvard` | Divergencia histórica (u vs v) |
| `<rescap:runFullTrust>` | Presente | Presente | Presente | **IDÉNTICO** |
| `<rescap:allowElevation>` | Presente | Presente | Presente | **IDÉNTICO** |
| `<rescap:unvirtualizedResources>` | Presente | Presente | Presente | **IDÉNTICO** |
| `FileSystemWriteVirtualization` | **Ausente** | **Ausente** | **Ausente** | **IDÉNTICO (NO DESACTIVADA)** |
| `RegistryWriteVirtualization` | **Ausente** | **Ausente** | **Ausente** | **IDÉNTICO (NO DESACTIVADA)** |

**Conclusión:** Se comprobó mediante lectura de archivo ZIP que el paquete `.msix` contiene exactamente las tres capacidades restringidas y **no** desactiva la virtualización de archivos ni de registro.

---

## 6. Reglas de Virtualización por Versión de Windows y Aplicabilidad

De acuerdo con la documentación canónica de Microsoft Learn (*Flexible virtualization / AppData virtualization*):

1. **Windows 10 <= 1809 (Redstone 5):**
   - Cualquier intento de escritura bajo `%LOCALAPPDATA%` se realiza con semántica *copy-on-write* hacia `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local`.
2. **Windows 10 >= 1903 / 2004 (Build 19041+ — Mínimo de PrivGuard):**
   - **Creación de nuevos archivos/carpetas:** Se desvían hacia la ubicación privada `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local`.
   - **Modificación de archivos preexistentes en AppData:** Si una carpeta o archivo ya existía fuera del contenedor antes de la ejecución del paquete empaquetado (por ejemplo, creado por Standalone o por el usuario), Windows permite que las modificaciones se apliquen directamente al archivo no virtualizado.
   - **Lectura:** El sistema busca primero en la ubicación privada; si no existe, recurre a la ubicación no virtualizada externa.
3. **Persistencia al Desinstalar:**
   - Todo lo que reside en `%LOCALAPPDATA%\Packages\<PFN>` es eliminado por el sistema operativo al desinstalar.
   - Los archivos en la ubicación nativa `%LOCALAPPDATA%\PrivGuard` fuera del contenedor que no fueron creados por la sesión virtualizada sobreviven.

> [!IMPORTANT]
> **Implicación para el WAL de PrivGuard:**  
> Si PrivGuard Store se ejecuta en un sistema limpio donde `%LOCALAPPDATA%\PrivGuard\Recovery` no existe, Windows creará la carpeta y el archivo `privacy-session-v1.json` **dentro de `LocalCache`**. Por ende, al desinstalar la app, **el WAL se destruye**.  
> Si Standalone se instaló primero y ya creó el directorio, la Store Edition podría interactuar con el archivo externo. **Esto requiere verificación obligatoria en VM.**

---

## 7. Tabla de Rutas Lógicas vs. Físicas de Almacenamiento

| Artefacto | Ruta Lógica (.NET) | Ubicación Física Estimada (MSIX) | Ubicación Física (Standalone) | Visibilidad Fuera del Paquete | Persistencia tras Reset/Uninstall | Evidencia |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **WAL Principal** | `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json` | `...\Packages\<PFN>\LocalCache\Local\PrivGuard\Recovery\` | `%LOCALAPPDATA%\PrivGuard\Recovery\` | Invisible para Win32 nativo si está en LocalCache | Se destruye al desinstalar MSIX | `VM REQUIRED` |
| **Backup WAL** | `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json.bak` | `...\Packages\<PFN>\LocalCache\Local\PrivGuard\Recovery\` | `%LOCALAPPDATA%\PrivGuard\Recovery\` | Invisible para Win32 nativo si está en LocalCache | Se destruye al desinstalar MSIX | `VM REQUIRED` |
| **Active Marker** | `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session.marker` | Depende de si `PrivGvard` preexistía | `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\` | Visible si no fue virtualizado | Variable | `VM REQUIRED` |
| **Settings** | `%LOCALAPPDATA%\PrivGuard\state.json` | `...\Packages\<PFN>\LocalCache\Local\PrivGuard\` | `%LOCALAPPDATA%\PrivGuard\` | Invisible si está en LocalCache | Se destruye al desinstalar MSIX | `VM REQUIRED` |
| **Logs** | `%LOCALAPPDATA%\PrivGuard\Logs\privguard-.log` | `...\Packages\<PFN>\LocalCache\Local\PrivGuard\Logs\` | `%LOCALAPPDATA%\PrivGuard\Logs\` | Invisible si está en LocalCache | Se destruye al desinstalar MSIX | `VM REQUIRED` |
| **Crash Reports** | `%LOCALAPPDATA%\PrivGuard\CrashReports\crash-.json` | `...\Packages\<PFN>\LocalCache\Local\PrivGuard\CrashReports\` | `%LOCALAPPDATA%\PrivGuard\CrashReports\` | Invisible si está en LocalCache | Se destruye al desinstalar MSIX | `VM REQUIRED` |

---

## 8. DI y Activación Real de Perfiles y Barreras de Mutación

Al auditar `Program.cs` en `9f856c0`:
- **Pregunta 1: ¿Se construye `WindowsPackageIdentityDetector` en producción?**  
  *Respuesta:* **NO**. No está registrado en DI ni se instancia manualmente en `Program.cs`.
- **Pregunta 2: ¿Se construye `DefaultEditionProfileResolver` en producción?**  
  *Respuesta:* **NO**. Ningún servicio lo resuelve ni lo solicita.
- **Pregunta 3: ¿Llega el perfil resuelto a `WindowsProtectionProvider`?**  
  *Respuesta:* **NO**. `WindowsProtectionProvider` tiene un constructor sin parámetros relacionados con `EditionProfile`.
- **Pregunta 4: ¿Existe alguna barrera efectiva en runtime que impida operaciones Secure/PnP a Store?**  
  *Respuesta:* **NO**. Si un usuario lanza la app desde el paquete MSIX y solicita protección Secure, la app intentará invocar el worker elevado mediante `ProcessStartInfo` con `Verb="runas"`. Como el paquete MSIX declara `allowElevation`, Windows mostrará el prompt de UAC.
- **Pregunta 5: ¿Comparten el mismo código binario?**  
  *Respuesta:* **SÍ**. `scripts/build-msix.ps1` compila `PrivLock.Desktop.csproj` con `dotnet publish`, generando exactamente el mismo ensamblado que utiliza el instalador Standalone.

---

## 9. ConsentStore Frente a D-01 y Justificación Store

- **D-01 / Justificación:** Promete que Store Edition es 100% no privilegiada, que no modifica claves del sistema y que guía al usuario a `ms-settings:privacy-webcam`.
- **Código en Producción:** `WindowsUserPrivacyManager.cs` líneas 32-35:
  ```csharp
  SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraPath, ConsentValueName, consentValue);
  SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraNonPackagedPath, ConsentValueName, consentValue);
  ```
- **Virtualización de Registro de Windows:** Según Microsoft Learn, las escrituras en `HKCU\Software` de un proceso Desktop Bridge son virtualizadas en el hive privado del paquete salvo declaración de excepción. Por ende, la llamada `SetRegistryStringValue` tiene éxito dentro de la burbuja del paquete pero **no cambia el permiso real** de las aplicaciones externas (Chrome, Zoom, etc.).
- **Resultado:** La implementación actual de Standard Protection en MSIX es **disfuncional e inconsistente** con la documentación de Store.

---

## 10. Core Audio: Alcance Software vs. Hardware y Semántica de UI

### 10.1 Comportamiento Documentado de `IAudioEndpointVolume`
1. **QueryHardwareSupport:** Devuelve un bitmask:
   - `0x1`: `ENDPOINT_HARDWARE_SUPPORT_VOLUME`
   - `0x2`: `ENDPOINT_HARDWARE_SUPPORT_MUTE`
   - `0x4`: `ENDPOINT_HARDWARE_SUPPORT_METER`
2. **Software Mute:** Cuando el bit `0x2` está ausente, el silenciamiento es puramente una operación de software dentro del motor de mezcla de audio de Windows.
3. **Bypass de Exclusive Mode:** Microsoft documenta explícitamente que los flujos abiertos con `AUDCLNT_SHAREMODE_EXCLUSIVE` omiten el motor de audio y se comunican directamente con el controlador. Por consiguiente, el silenciamiento de software **no silencia flujos en modo exclusivo**.
4. **Semántica de UI:** `WindowsProtectionProvider.cs` calcula `micStandardIsVerified = true` sin verificar el soporte de hardware. Esto induce al usuario al error, haciéndole creer que el micrófono está protegido de forma verificada cuando un software de grabación en modo exclusivo puede capturar audio sin restricciones.

---

## 11. Auditoría del Desinstalador Standalone y Mitigación de Riesgos

Se auditó minuciosamente `installer/setup.iss` para verificar si es lícito afirmar que Standalone tiene "riesgo cero" o "rollback garantizado":

1. **Gate Mutex:** El instalador adquiere `Global\PrivLock_UninstallGate` (línea 598).
2. **Chequeo de Sesiones Activas:** En la función `InitializeUninstall` (líneas 753-759):
   ```pascal
   if AnyProfileMayHaveActiveSession or AnyPrivilegedOwnershipClaim then
   begin
     MsgBox('PrivGvard found privacy state that still requires recovery. Sign in to each affected account and start PrivGvard before uninstalling.',
       mbCriticalError, MB_OK);
     ReleaseUninstallGate;
     exit; // ABORTA LA DESINSTALACIÓN
   end;
   ```
3. **Hallazgos Críticos sobre Standalone:**
   - **No hay rollback automático desatendido:** Si un usuario común mutó el hardware y cerró sesión, el desinstalador **no revierte el estado por sí mismo**; se niega a desinstalar hasta que ese usuario inicie sesión y recupere su estado.
   - **Fallo de la app ejecutándose:** Si la app en ejecución se cuelga y no responde al evento de handoff en 30 segundos, el desinstalador aborta con error.
   - **Conclusión:** Standalone tiene una arquitectura de contención de fallos robusta (*fail-closed*), pero **no tiene riesgo cero** ni garantiza rollback bajo cualquier circunstancia multiusuario sin interacción.

---

## 12. ProgramData, ACL y Estado de Claims

- **Implementación:** `CODE VERIFIED = FALSE`. No existen archivos `.claim`, clases de claims ni directorios en `ProgramData` dentro del repositorio.
- **Análisis de Seguridad NTFS:**
  - `C:\ProgramData` concede permisos de creación a `BUILTIN\Users`.
  - Si un usuario estándar crea una carpeta, se convierte en `CREATOR OWNER`.
  - Sin un servicio o instalador elevado que establezca una DACL explícita que restrinja la modificación a `SYSTEM` y `Administrators`, cualquier diseño basado en archivos compartidos en `ProgramData` es vulnerable a ataques de denegación de servicio (*Claim Squatting*) o manipulación entre usuarios.

---

## 13. Reclasificación Exhaustiva de la Matriz de 54 Escenarios

Se presenta la matriz completa de los 54 escenarios, confrontando la clasificación original de 8B.1A.5, la revisión de 8B.1A.6 y la clasificación final y rigurosa de esta auditoría (**8B.1A.6.1**):

| ID | Escenario Concreto | Clasificación 8B.1A.5 | Clasificación 8B.1A.6 | Clasificación 8B.1A.6.1 | Fuente / Código / Evidencia | Justificación de la Clasificación |
| :---: | :--- | :---: | :---: | :---: | :--- | :--- |
| **01** | Instalación limpia sin Standalone. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Arquitectura D-01 | Depende de perfiles no cableados en DI. |
| **02** | Standalone primero; luego Store. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Propuesta D-04 | No existe detección inter-edición en código. |
| **03** | Store primero; luego Standalone. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Propuesta D-04 | No existe detección mutua en código. |
| **04** | ProgramData root no existe. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Shell Folders | Contrato de `FOLDERID_ProgramData`. |
| **05** | ACL de ProgramData incompatible. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn NTFS Security | `UnauthorizedAccessException` en Win32. |
| **06** | `BUILTIN\Users` sin `CreateFiles`. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn NTFS Security | Comportamiento estándar de NTFS. |
| **07** | Claim Squatting: Usuario B claim falso. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Análisis de Seguridad | Claims no implementados. |
| **08** | Dos usuarios crean claim a la vez. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn FileStream | `FileMode.CreateNew` es atómico en NTFS. |
| **09** | Claim con hash corrupto. | `CONFIRMADO...` | `INVALID...` | **INVALID / CONTRADICTS SOURCE** | Ninguna | Claims no existen en código fuente. |
| **10** | Reparse Point en `.claim`. | `CONFIRMADO...` | `INVALID...` | **INVALID / CONTRADICTS SOURCE** | Ninguna | Claims no existen en código fuente. |
| **11** | NTFS Owner discrepa con `OwnerUserSid`. | `DOCUMENTADO...` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Verificación NTFS sobre claims es diseño. |
| **12** | Store mute con hardware mute `Supported`.| `DOCUMENTADO...` | `PARTIAL...` | **PARTIAL** | `WindowsCoreAudioController.cs` L:75 | Código detecta bit, pero UI lo ignora. |
| **13** | Store mute con hardware mute `NotSupported`.| `DOCUMENTADO...` | `PARTIAL...` | **PARTIAL** | `WindowsProtectionProvider.cs` L:222 | Mute aplica, pero UI oculta que es Software-Only. |
| **14** | Store mute con hardware mute `Unknown`. | `CONFIRMADO...` | `IMPLEMENTED...` | **CODE VERIFIED** | `WindowsCoreAudioController.cs` L:81 | HR < 0 asigna `Unknown` limpiamente. |
| **15** | Captura Shared Mode mientras mute activo. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn WASAPI | El audio engine mezcla silencio. |
| **16** | Captura Exclusive Mode con software mute. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Exclusive Mode | Bypass documentado: elude el software mixer. |
| **17** | Dos apps captura en misma audio session. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Audio Sessions | Sesión compartida recibe silencio. |
| **18** | Dos sesiones audio en misma cuenta. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Audio Endpoints| El endpoint mute afecta a nivel maestro. |
| **19** | Dos logon sessions Windows distintas. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Audio Endpoints| El endpoint es dispositivo de máquina. |
| **20** | Fast User Switching (A cede a B). | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Audio Endpoints| El endpoint continúa en estado muteado. |
| **21** | Sesión remota RDP con audio. | `PENDIENTE...` | `VM REQUIRED` | **VM REQUIRED** | Audio Virtualization | Endpoints virtuales RDP requieren VM. |
| **22** | Usuario A mutea; B abre Store. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Señalización inter-usuario no implementada. |
| **23** | Usuario B intenta unmute de endpoint A. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Bloqueo cruzado en Store es diseño. |
| **24** | Usuario A logoff con mute activo. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn Audio Endpoints| El endpoint permanece muteado en driver. |
| **25** | Usuario A logon tras logoff. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Reconexión Store con WAL no probada. |
| **26** | Usuario A elimina su cuenta Windows. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn User Profiles | Perfil destruido; WAL destruido. |
| **27** | Store crashea tras `SetMute(true)`. | `CONFIRMADO...` | `PARTIAL...` | **PARTIAL** | `PrivacyRecoveryService.cs` | Recuperación existe en Win32; no en Store. |
| **28** | Store crashea antes de crear claim. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Claims no implementados. |
| **29** | Store crashea tras claim y antes mute. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Claims no implementados. |
| **30** | Store crashea tras mute antes de WAL. | `CONFIRMADO...` | `PARTIAL...` | **PARTIAL** | `FilePrivacySessionStore.cs` L:170 | ActiveMarker protege en Win32; no en Store. |
| **31** | Store se actualiza vía Windows Store. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn MSIX Updates | Actualizaciones MSIX preservan AppData. |
| **32** | Store se repara vía Windows Settings. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn MSIX Repair | Repair preserva datos de usuario. |
| **33** | Store se resetea vía Windows Settings. | `DOCUMENTADO...` | `VM REQUIRED` | **VM REQUIRED** | MS Learn MSIX Reset | Requiere VM para ver si borra WAL externo. |
| **34** | Store se desinstala con mute activo. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | MS Learn MSIX Lifecycle | MSIX carece de hooks; endpoint queda muteado. |
| **35** | WAL en disco pero claim desapareció. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Claims no implementados. |
| **36** | Claim en disco pero WAL desapareció. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Claims no implementados. |
| **37** | Machine Latch en Standalone; Store inicia.| `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Propuesta D-04 | D-04 (SupervisionOnly) no existe en código. |
| **38** | Store claim activo; Standalone pide Secure.| `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Interacción inter-edición no implementada. |
| **39** | Standalone Secure; Store pide Standard. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Barrera inter-edición no implementada. |
| **40** | Machine Latch corrupto y `.bak` válido. | `CONFIRMADO...` | `INVALID...` | **INVALID / CONTRADICTS SOURCE** | Ninguna | Machine Latch en ProgramData no existe. |
| **41** | Latch principal y backup corruptos. | `CONFIRMADO...` | `INVALID...` | **INVALID / CONTRADICTS SOURCE** | Ninguna | Machine Latch no existe en el código fuente. |
| **42** | Escrow PnP completo con rollback. | `CONFIRMADO...` | `PARTIAL...` | **PARTIAL** | `WindowsPrivilegedOwnershipStore` L:134| Almacena fingerprint, no snapshot restaurable.|
| **43** | Escrow HKLM completo con rollback. | `CONFIRMADO...` | `PARTIAL...` | **PARTIAL** | `WindowsPrivilegedOwnershipStore` L:109| Almacena fingerprint, no snapshot restaurable.|
| **44** | GPO corporativa altera HKLM. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **PARTIAL** | `WindowsPolicyManager.cs` L:90 | Regla compare-before-restore básica existe. |
| **45** | Rollback PnP falla en cámara 2. | `CONFIRMADO...` | `IMPLEMENTED...` | **CODE VERIFIED** | `WindowsDeviceController.cs` L:180 | `DeviceOperationDetail` reporta fallos parciales.|
| **46** | Endpoint USB desconectado durante Active. | `CONFIRMADO...` | `IMPLEMENTED...` | **CODE VERIFIED** | `WindowsProtectionProvider.cs` L:214 | Manejo dinámico de dispositivos desconectados. |
| **47** | Transaction Gate Mutex ocupado. | `CONFIRMADO...` | `INVALID...` | **INVALID / CONTRADICTS SOURCE** | Ninguna | `Transaction_Gate` no existe; es `PrivLock_...` |
| **48** | Mutex UI global impide segunda sesión. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **PARTIAL** | `WindowsSingleInstanceGuard.cs` | Mutex por sesión existe; inter-edición no. |
| **49** | `OpenProcess` retorna `ACCESS_DENIED`. | `DOCUMENTADO...` | `DOCUMENTED` | **WINDOWS DOCUMENTED** | Win32 API Contract | Acceso denegado impide inspeccionar procesos. |
| **50** | PID reciclado con misma SID pero otro LUID.| `DOCUMENTADO...` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | `OwnerTuple` completa con LUID no implementada. |
| **51** | Worker privilegiado pierde Named Pipe. | `CONFIRMADO...` | `IMPLEMENTED...` | **CODE VERIFIED** | `WindowsPrivilegedWorker.cs` L:250 | Desconexión del pipe aborta worker al instante. |
| **52** | Desinstalador Inno Setup con claim Store. | `DISEÑO PROPUESTO` | `DESIGN ONLY` | **DESIGN ONLY** | Ninguna | Inno Setup no comprueba claims de Store. |
| **53** | Desinstalador Inno Setup con sesión activa.| `CONFIRMADO...` | `PARTIAL...` | **CODE VERIFIED** | `setup.iss` L:753; `Program.cs` L:135 | Inno Setup aborta si detecta sesión activa. |
| **54** | Reinicio abrupto (BSOD / corte) en mutación.| `CONFIRMADO...` | `PARTIAL...` | **CODE VERIFIED** | `FilePrivacySessionStore.cs` L:172 | `WriteThrough` + `.bak` garantizan WAL en disco. |

---

## 14. Comparativa Rigurosa de Opciones de Producto

| Criterio | Opción 1: Store como Cliente de Broker Standalone | Opción 2: Store Best-Effort (Standard Únicamente) | Opción 3: Standalone First (Pausar Store) |
| :--- | :--- | :--- | :--- |
| **Complejidad Arquitectónica** | **Muy Alta:** Requiere desarrollar un broker de sistema Windows, IPC bidireccional seguro y manejo de permisos cross-session. | **Media:** Requiere limpiar el manifiesto, desconectar escrituras HKCU y corregir semántica UI. | **Baja:** La arquitectura Win32 actual está consolidada y funcional. |
| **Alineación con Microsoft Store** | **Incierta / Cuestionable:** Las apps que dependen de instaladores externos suelen ser observadas o rechazadas en certificación. | **Alta:** Compatible con políticas si declara solo `runFullTrust` y no eleva. | **N/A:** Distribución directa independiente. |
| **Garantía Frente a Exclusive Mode** | Total (delegada a PnP en el broker). | **Nula:** Vulnerable a software de captura en modo exclusivo. | Total (vía desactivación de devnodes PnP). |
| **Recuperación tras Desinstalación** | Controlada por el broker de Standalone. | **Inexistente:** El endpoint queda silenciado si se desinstala la app. | Controlada por el gate de `setup.iss`. |
| **Recomendación Provisional** | Descartar a corto plazo. | Viable solo si el negocio acepta un producto de privacidad meramente básico. | **RECOMENDADA:** Release prioritario de Standalone; Store en hold. |

---

## 15. Bloqueos Priorizados y Pruebas Requeridas

### Prioridad P0 (Bloqueos Críticos de Arquitectura)
1. **B0-01 (Manifiesto vs. D-01):** Decidir si se elimina `allowElevation` y `unvirtualizedResources` de `packaging/Package.appxmanifest`.
2. **B0-02 (Cableado DI):** Conectar `IEditionProfileResolver` en `Program.cs` e implementar una barrera en `WindowsProtectionProvider` que impida elevación en Store.
3. **B0-03 (ConsentStore Disfuncional):** Eliminar la mutación de registro en `WindowsUserPrivacyManager` y reemplazarla por invocación a `ms-settings:privacy-webcam`.

### Prioridad P1 (Garantías de Protección y Recuperación)
1. **B1-01 (Falsa Verificación en Core Audio):** Ajustar el cálculo de `micStandardIsVerified` para requerir `HardwareMuteSupport.Supported` antes de marcar la protección como verificada.
2. **B1-02 (Aislamiento de Storage):** Parametrizar `FilePrivacySessionStore` para utilizar subdirectorios separados (`PrivGuard\Store` vs `PrivGuard\Standalone`).

### Pruebas Requeridas en VM Desechable (Pendientes)
1. **Test-VM-01 (Redirección VFS):** Instalar el paquete MSIX y monitorizar con ProcMon si `FilePrivacySessionStore` escribe en `AppData\Local\PrivGuard` o en `Packages\<PFN>\LocalCache`.
2. **Test-VM-02 (Reset y Uninstall):** Muteador de prueba activo -> Reset de la app -> Verificar si `privacy-session-v1.json` sobrevive o es eliminado.

---

## 16. Fuentes Oficiales de Microsoft Learn Contrastadas

1. **Flexible Virtualization in MSIX:**  
   URL: `https://learn.microsoft.com/en-us/windows/msix/desktop/flexible-virtualization`  
   *Afirmación respaldada:* Describe la virtualización de AppData por copia al escribir y la hive privada de registro para Desktop Bridge.
2. **App Capability Declarations:**  
   URL: `https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations#restricted-capabilities`  
   *Afirmación respaldada:* `allowElevation` y `unvirtualizedResources` son restringidas y están sujetas a revisión de Store.
3. **Reset and Repair MSIX Apps:**  
   URL: `https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#reset-and-repair-apps`  
   *Afirmación respaldada:* Reset elimina los datos de aplicación del paquete; Repair los conserva.
4. **IAudioEndpointVolume::QueryHardwareSupport:**  
   URL: `https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nf-endpointvolume-iaudioendpointvolume-queryhardwaresupport`  
   *Afirmación respaldada:* El bitmask `ENDPOINT_HARDWARE_SUPPORT_MUTE` (0x2) define soporte de mute en hardware.
5. **Exclusive-Mode Audio Streams:**  
   URL: `https://learn.microsoft.com/en-us/windows/win32/coreaudio/exclusive-mode-streams`  
   *Afirmación respaldada:* Flujos exclusivos omiten el mezclador del motor de audio de Windows.
6. **Desktop6 UninstallActions Schema:**  
   URL: `https://learn.microsoft.com/en-us/uwp/schemas/appxpackage/uapmanifestschema/element-desktop6-uninstallactions`  
   *Afirmación respaldada:* `UninstallActions` está restringido a paquetes con capacidades personalizadas específicas (ej. juegos MSIXVC).

---

## 17. Respuestas a Preguntas de Cierre y Postflight

1. **¿Qué afirmaciones críticas de 8B.1A.6 se sostienen?** Se sostienen plenamente las contradicciones de manifiesto, desconexión de DI, escritura directa en HKCU, ausencia de escrow en HKLM y la vulnerabilidad de software mute frente a Exclusive Mode.
2. **¿Cuáles se corrigen o retiran?** Se corrige la afirmación de que `allowElevation` se rechaza "automáticamente" en la subida (es rechazado en revisión si no se justifica); se retira la afirmación de que Standalone tiene "riesgo cero" o "rollback automático universal"; y se matiza la certeza física sobre el destino del WAL, marcándola como dependiente de VM.
3. **¿Qué se comprobó por código?** La ausencia de registros en DI, el contenido de `StoredOwnershipRecord`, el código de `WindowsUserPrivacyManager`, la evaluación de `micStandardIsVerified` y el script `setup.iss`.
4. **¿Qué se comprobó únicamente por documentación Microsoft?** El comportamiento del bypass en Exclusive Mode, los límites de virtualización VFS y el ciclo de vida de Reset/Uninstall.
5. **¿Qué exige VM?** La ubicación física final de los archivos de Store y el comportamiento de persistencia tras Reset y Uninstall.
6. **¿Qué bloquea aún el diseño Store?** Manifiesto, DI, ConsentStore, falta de rollback en desinstalación y bypass de Exclusive Mode.
7. **¿Qué nivel de madurez puede afirmarse de Standalone?** Alta solidez técnica y journaling probado, pero con aborto manual en caso de conflicto multiusuario o crash severo.
8. **¿Qué decisiones requieren aprobación del propietario?** Aprobación de la limpieza del manifiesto, aceptación de limitaciones de Store y priorización de Standalone First.
9. **¿Puede autorizarse 8B.1C?** **NO**. 8B.1C permanece terminantemente bloqueada.

### Comprobación Postflight Git
- **Rama:** `main`
- **HEAD:** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **`git status --short`:** Salida vacía (0 cambios).
- **`git diff --check`:** Salida vacía (sin errores de formato ni cambios pendientes).
- **Archivos modificados en el repositorio:** **0 (cero)**.
- **Microsoft Store Status:** **`CONDITIONAL HOLD`**.
- **Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`**.

# PrivGuard — Fase 8B.1A.6.2
## Cierre de Contradicciones, Trazabilidad de Recuperación y Matriz de Evidencia MSIX / Standalone

**Fecha de Ejecución:** 6 de Noviembre de 2026  
**Auditor Principal:** Auditor Senior Adversarial de Plataforma Windows, MSIX/Desktop Bridge, Core Audio y Seguridad de Sistemas  
**Repositorio:** `cristianbarreiro/PrivGuard`  
**Commit Auditado:** `9f856c0405e4e5b64e784309affad450c38d3a37` (HEAD de `main`, en sincronía con `origin/main`)  
**Fase de Entrada:** 8B.1A.6.1  
**Fase Siguiente:** 8B.1C — **ESTRICTAMENTE BLOQUEADA / NO AUTORIZADA**  
**Estado de Microsoft Store:** **`CONDITIONAL HOLD`**  
**Veredicto Formal de Cierre:** **`DOCUMENTARY AUDIT CLOSED — VM VALIDATION PENDING`**  
**Ubicación del Artefacto:** Fuera del repositorio (Directorio de Artefactos de Antigravity)  

---

## 1. Resumen Ejecutivo y Veredicto Formal

### 1.1 Resumen Ejecutivo
La Fase **8B.1A.6.2** constituye el cierre adversarial, riguroso y documental de la auditoría técnica sobre la arquitectura de empaquetado MSIX, persistencia transaccional (WAL), contenedor de inyección de dependencias (DI), mutaciones de Core Audio (WASAPI), permisos en `ProgramData` y garantías del desinstalador Standalone de PrivGuard.

Esta auditoría elimina ambigüedades, resuelve contradicciones previas y establece una línea base factual verificable:

1. **Resolución de la Discrepancia de Identidad del Manifiesto:**  
   Se descubrió que la divergencia entre `cdevstudios.PrivGuard` y `cdevstudios.PrivGvard` detectada en 8B.1A.6.1 se debía a la presencia de un directorio staging residual no limpiado del 2 de noviembre de 2026. La inspección del paquete oficial `release/store/PrivGuard-2.0.0.0-x64.msix` (generado el 3 de noviembre tras el commit `b59b4bc`) y del manifiesto fuente confirma que la identidad oficial es unívocamente **`cdevstudios.PrivGuard`**, la cual coincide exactamente con las constantes esperadas por [`WindowsPackageIdentityDetector.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsPackageIdentityDetector.cs#L17-L21).
2. **Capacidades Restringidas y Partner Center:**  
   Se confirma que el `.msix` empaqueta `runFullTrust`, `allowElevation` y `unvirtualizedResources` sin desactivar la virtualización del sistema de archivos ni del registro (carece de `<desktop6:FileSystemWriteVirtualization>disabled</desktop6:FileSystemWriteVirtualization>` bajo `<Properties>`). Se aclara la normativa de Microsoft: `allowElevation` no causa rechazo automático en la carga del paquete, sino durante la revisión humana/técnica de certificación si no se aporta un caso de negocio justificado bajo la política Store.
3. **Desmitificación de la "Ausencia de Recuperación en Store":**  
   Se corrige una inferencia errónea previa: el hecho de que `IEditionProfileResolver` no esté registrado en `Program.cs` **no significa que Store carezca de recuperación**. El pipeline de arranque en [`Program.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L229-L236) ejecuta incondicionalmente `PrivacyRecoveryService.RecoverAtStartupAsync()`. Tanto Store como Standalone compilan y ejecutan el mismo motor de recuperación. El defecto crítico no es la ausencia de código, sino que ambas ediciones comparten el mismo almacén no aislado y que la versión Store intentará elevar si encuentra recursos Secure en el diario.
4. **Análisis Físico de Virtualización de AppData (Casos A a F):**  
   Bajo Windows 10 versión 1903/19041+ (TargetDeviceFamily de PrivGuard), los archivos nuevos creados por un proceso FullTrust se desvían a `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local`. Dado que `FilePrivacySessionStore.Save` utiliza `FileMode.CreateNew` sobre un archivo temporal `.tmp`, dicho archivo se crea en `LocalCache`. El comportamiento de `File.Replace` reemplazando un archivo nativo externo con un archivo virtualizado es un caso no resuelto documentalmente que **exige validación en VM**.
5. **Gate de Desinstalador Standalone vs. Rollback Real:**  
   [`installer/setup.iss`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/installer/setup.iss#L753-L759) implementa una barrera de seguridad (*gate*), no un rollback automático desatendido. Si se detectan sesiones activas o reclamos en cualquier perfil de Windows, el desinstalador **aborta la operación** mediante error crítico. No existe "riesgo cero" ni "rollback universal garantizado" en escenarios multiusuario sin interacción.
6. **Core Audio y Exclusive Mode:**  
   Se ratifica que el silenciamiento de software a nivel de endpoint (`IAudioEndpointVolume`) es implementado por el motor de audio de Windows (WASAPI audio engine). Los flujos que solicitan `AUDCLNT_SHAREMODE_EXCLUSIVE` omiten el motor de audio y acceden al controlador sin ser silenciados. La UI comete un defecto semántico al reportar `StandardIsVerified = true` para dispositivos sin soporte de hardware (`HardwareMuteSupport.NotSupported`).

### 1.2 Veredicto Formal

> [!IMPORTANT]
> **VEREDICTO: DOCUMENTARY AUDIT CLOSED — VM VALIDATION PENDING**
> 
> La auditoría documental, estática y de código queda formalmente **CERRADA**. Las contradicciones internas han sido subsanadas y fundamentadas en código y documentación oficial.  
> Se declara **PENDIENTE DE VALIDACIÓN EN VM** el comportamiento dinámico de virtualización de AppData, la persistencia del WAL tras Reset/Uninstall y el comportamiento multiusuario.  
> - **Microsoft Store:** Permanece en **`CONDITIONAL HOLD`**.  
> - **Fase 8B.1C:** Permanece **`BLOCKED / NOT AUTHORIZED`**.

---

## 2. Estado Git Preflight y Postflight

- **Repositorio:** `c:\Users\Cristian\Documents\GitHub\PrivGuard`
- **Proyecto Canónico:** `cristianbarreiro/PrivGuard`
- **Rama:** `main`
- **HEAD Auditado:** `9f856c0405e4e5b64e784309affad450c38d3a37`
- **HEAD Remoto (`origin/main`):** `9f856c0405e4e5b64e784309affad450c38d3a37` (100% sincronizado vía `git fetch origin`).
- **Estado del Working Tree:** `git status --short` arrojó 0 líneas; `git diff --stat` arrojó 0 modificaciones; `git diff --check` limpio.
- **Archivos Modificados en el Repositorio:** **0 (cero)**. Prohibición estricta cumplida.

---

## 3. Matriz de Revalidación de Hallazgos de la Fase 8B.1A.6.1

| ID | Severidad | Afirmación Auditada en 8B.1A.6.1 | Estado Final 8B.1A.6.2 | Evidencia Concreta de Código / Documentación | Consecuencia Práctica |
| :---: | :---: | :--- | :---: | :--- | :--- |
| **P0-01** | **P0** | `allowElevation` se rechaza automáticamente al subir a Store. | **CORREGIDA (PARTIAL)** | MS Learn *App capability declarations*. Requiere justificación en Partner Center; rechazo en revisión humana, no automático en upload. | Store certification fallará si no hay caso de negocio D-01. |
| **P0-02** | **P0** | Divergencia de identidad (`cdevstudios.PrivGvard` en binario). | **REFUTADA (EXPLICADA)** | `release/store/PrivGuard-2.0.0.0-x64.msix` contiene `cdevstudios.PrivGuard`. El error en 8B.1A.6.1 vino de staging obsoleto del 2-Nov. | Identidad empaquetada coincide con detector de 8B.1B.3. |
| **P0-03** | **P0** | DI no conecta resolver; Store "carece de recuperación". | **CORREGIDA (PARTIAL)** | [`Program.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L229) ejecuta recuperación para todos. Store sí tiene recovery; el fallo es que comparte WAL y ruta con Standalone. | Colisión de WAL si coexisten; elevación en Store. |
| **P0-04** | **P0** | AppData se comparte si Standalone creó la carpeta. | **CORREGIDA (VM REQUIRED)** | MS Learn *Flexible Virtualization*. Crear nuevos archivos (`.tmp`) desvía a `LocalCache` aún si la carpeta padre preexistía. | Imposible asegurar ruta física sin ProcMon en VM. |
| **P0-05** | **P0** | ConsentStore escribe en HKCU; 0 referencias a `ms-settings:`. | **CONFIRMED** | [`WindowsUserPrivacyManager.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/Policies/WindowsUserPrivacyManager.cs#L32-L35). Búsqueda de `ms-settings:` en `src/` arrojó 0 resultados. | Contradicción frontal con justificación de Store. |
| **P1-01** | **P1** | Exclusive Mode elude software mute; UI engaña con "Verified". | **CONFIRMED** | [`WindowsProtectionProvider.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L222-L226). MS Learn *Exclusive-Mode Streams*. Omite mixer de Windows. | La UI reporta Verified en flujos eludibles. |
| **P1-02** | **P1** | Desinstalador Standalone ejecuta rollback obligatorio (riesgo 0).| **REFUTADA** | [`setup.iss`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/installer/setup.iss#L753-L759). `InitializeUninstall` aborta con `exit;` si hay sesiones activas; no ejecuta rollback. | Standalone bloquea desinstalación; no revierte solo. |
| **P1-03** | **P1** | Reclamos `.claim` en ProgramData resuelven multiusuario. | **REFUTADA** | No existen en código. En NTFS, `BUILTIN\Users` introduce riesgo DoS (*Claim Squatting*) sin un broker elevado. | Diseño de claims descartado sin servicio mediador. |

---

## 4. Manifiesto, Capacidades Restringidas, TrustLevel e Identidad Real

### 4.1 Trazabilidad del Pipeline de Empaquetado
Se inspeccionaron las tres capas del artefacto:
1. **Manifiesto Fuente ([`packaging/Package.appxmanifest`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest)):**
   - `Identity Name="cdevstudios.PrivGuard"`
   - `Publisher="CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD"`
   - `EntryPoint="Windows.FullTrustApplication"`
   - Capacidades: `runFullTrust`, `allowElevation`, `unvirtualizedResources`.
   - Propiedades: Carece de `desktop6:FileSystemWriteVirtualization` y de `desktop6:RegistryWriteVirtualization`.
2. **Artefacto Compilado Real ([`release/store/PrivGuard-2.0.0.0-x64.msix`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/release/store/PrivGuard-2.0.0.0-x64.msix)):**
   - Se leyó su `AppxManifest.xml` interno mediante inspección de archivo ZIP.
   - `Identity Name="cdevstudios.PrivGuard"`, `Version="2.0.0.0"`, `ProcessorArchitecture="x64"`.
   - **Confirma:** El paquete distribuible real tiene la identidad canónica oficial `cdevstudios.PrivGuard`.
3. **Residuo Histórico Explicado:**
   - La carpeta `publish_out/msix/staging-x64/` contenía un manifiesto con fecha 02/11/2026 generado con la identidad previa `cdevstudios.PrivGvard`. El commit `b59b4bc` del 03/11/2026 actualizó formalmente la identidad a `cdevstudios.PrivGuard`.

### 4.2 Modelo de Confianza Efectivo
- **`EntryPoint="Windows.FullTrustApplication"`:** El proceso empaquetado se ejecuta como una aplicación desktop de confianza total en el nivel de integridad medio (**Medium IL**). **No corre en AppContainer**.
- **`asInvoker` vs `allowElevation`:** El proceso principal arranca estrictamente sin privilegios (`asInvoker`). La presencia de `allowElevation` no lo eleva automáticamente; solo permite que el proceso pueda lanzar procesos hijos solicitando elevación UAC (`Verb="runas"`).

---

## 5. Tabla de Rutas Lógicas vs. Físicas y Análisis de Virtualización (Casos A a F)

### 5.1 Reglas de Virtualización de AppData (Windows 10 Build 19041+)
De acuerdo con Microsoft Learn (*Flexible Virtualization / AppData Operations*), el comportamiento físico exacto ante las operaciones del código de PrivGuard se desglosa en los siguientes escenarios:

| Caso | Condición Previa del Sistema de Archivos | Operación del Código ([`FilePrivacySessionStore.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Infrastructure.Common/Storage/FilePrivacySessionStore.cs)) | Ubicación Física Estimada | Comportamiento Documentado vs Evidencia Pendiente |
| :---: | :--- | :--- | :--- | :--- |
| **A** | **Perfil Limpio:** No existe `%LOCALAPPDATA%\PrivGuard`. | `Directory.CreateDirectory` y `FileStream(..., FileMode.CreateNew)` de `.tmp`. | `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local\PrivGuard\Recovery\` | **DOCUMENTED:** Todo el árbol nuevo se crea bajo `LocalCache`. Invisible para Win32 nativo. Destruido en Uninstall. |
| **B** | Existe `PrivGuard`, pero **no** `Recovery`. | `Directory.CreateDirectory` crea la subcarpeta `Recovery`. | `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local\PrivGuard\Recovery\` | **DOCUMENTED:** Al ser subdirectorio nuevo, se desvía a `LocalCache`. Invisible para Standalone. |
| **C** | Existe `PrivGuard\Recovery`, pero **no** `privacy-session-v1.json`. | Crea `.tmp` con `FileMode.CreateNew`, luego `File.Move(tmp, wal)`. | `%LOCALAPPDATA%\Packages\<PFN>\LocalCache\Local\PrivGuard\Recovery\*.tmp` | **DOCUMENTED:** La creación de archivo nuevo va a `LocalCache`. El WAL queda en el contenedor privado. |
| **D** | **WAL Principal Preexistente:** Creado previamente por Standalone en AppData nativo. | Abre `.tmp` nuevo (`CreateNew`), serializa y ejecuta `File.Replace(tmp, wal, bak)`. | **INDETERMINADA / CONFLICTIVA** | **VM REQUIRED:** El `.tmp` nace en `LocalCache`, pero el `wal` de destino reside en AppData nativo. ¿Permite el VFS filter driver un `ReplaceFileW` cross-view atómico? |
| **E** | Existe únicamente el archivo `.bak` en AppData nativo. | `Load()` lee `.bak` (lectura busca primero local, luego nativo). Luego `Save()` crea `.tmp`. | Lectura desde nativo; nueva escritura en `LocalCache`. | **VM REQUIRED:** Puede bifurcar el estado: el proceso lee el backup nativo pero persiste el nuevo WAL en `LocalCache`. |
| **F** | Existen WAL, `.bak`, `.tmp` y `active-session.marker` en estados combinados. | `ReconcileMarker` y rotación de copias terminales. | `active-session.marker` usa ruta `PrivGvard` (con v); WAL usa `PrivGuard` (con u). | **VM REQUIRED:** Riesgo severo de desincronización física entre el marker y el diario transaccional. |

---

## 6. DI, Recuperación de Inicio y Trazabilidad de Mutaciones

### 6.1 Trazabilidad del Pipeline de Inicio
Al auditar [`src/PrivLock.Desktop/Program.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L229-L236):
1. El contenedor DI se inicializa en `ConfigureServices(services)`.
2. Se resuelve `var recoveryService = serviceProvider.GetRequiredService<PrivacyRecoveryService>();`.
3. Se invoca:
   ```csharp
   using (recoveryService.BeginStartupPrivilegeScope())
   {
       startupRecovery = recoveryService.RecoverAtStartupAsync().GetAwaiter().GetResult();
       ...
       reconcileResult = recoveryService.ReconcileAtStartupAsync(startupRecovery).GetAwaiter().GetResult();
   }
   ```
4. **Hallazgo:** Este código se ejecuta **exactamente igual** si PrivGuard se inicia como Standalone Win32 o como paquete MSIX Store.
5. **Corrección de Concepto:** No es verdad que Store "carezca de recuperación". Store ejecuta el mismo motor. Sin embargo:
   - Si el WAL de Store fue virtualizado en `LocalCache` y el usuario desinstala la app, Windows borra `LocalCache`, por lo que en el siguiente arranque (o reinstalación) no habrá WAL que recuperar.
   - Si el WAL contiene recursos Secure (PnP/HKLM), Store intentará lanzar el worker elevado (`--privileged-worker`), provocando un prompt UAC que viola la promesa de D-01.

### 6.2 Hashes SHA-256 de Ensamblados
- No fue posible comparar directamente los hashes binarios entre `publish_out/win-x64` y el `.msix` debido a que la carpeta `publish_out/win-x64` no estaba compilada en el árbol actual de solo lectura.
- Se clasifica la identidad binaria exacta bit-a-bit como **`NOT VERIFIED`**, aunque la inspección de proyectos (`PrivLock.Desktop.csproj`) demuestra que ambos objetivos compilan el mismo código fuente C#.

---

## 7. ConsentStore: Virtualización Esperada y Realidad del Código

1. **El Código Real ([`WindowsUserPrivacyManager.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/Policies/WindowsUserPrivacyManager.cs#L32-L35)):**
   ```csharp
   SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraPath, ConsentValueName, consentValue);
   SetRegistryStringValue(Registry.CurrentUser, ConsentStoreCameraNonPackagedPath, ConsentValueName, consentValue);
   ```
2. **La Promesa de Store ([`MICROSOFT-STORE-JUSTIFICATION.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-STORE-JUSTIFICATION.md#L57)):**
   Promete formalmente no escribir en el registro y guiar al usuario a `ms-settings:privacy-webcam`.
3. **Comportamiento en Windows Desktop Bridge:**
   Como el manifiesto no declara `<desktop6:RegistryWriteVirtualization>disabled</desktop6:RegistryWriteVirtualization>`, Windows redirige estas escrituras a `\Registry\User\<SID>_Classes\AppX...\User.dat`.
4. **Efecto Práctico:** La aplicación cree haber cambiado el consentimiento a `"Deny"`, pero Windows y las apps de terceros (Chrome, Teams) leen la clave nativa y continúan accediendo a la cámara. El estado de protección es **ficticio en tiempo de ejecución**.

---

## 8. Core Audio: Contrato Verificable y Semántica UI

### 8.1 Capacidades Reales de `IAudioEndpointVolume`
- **Consulta de Soporte:** `QueryHardwareSupport(out uint mask)`.
  - Bit `0x2` (`ENDPOINT_HARDWARE_SUPPORT_MUTE`): Indica soporte en driver/hardware.
- **Software Mute:** Si el bit `0x2` es 0, `SetMute(true)` es implementado por el motor de mezcla de audio de Windows (WASAPI mixer).
- **Exclusive Mode Bypass:** Microsoft documenta explícitamente en *Exclusive-Mode Streams* que los flujos con `AUDCLNT_SHAREMODE_EXCLUSIVE` omiten el motor de audio y acceden directamente al driver. Por tanto, el software mute **no detiene la captura en modo exclusivo**.
- **Defecto Semántico en UI:** [`WindowsProtectionProvider.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L222-L226) asigna `micStandardIsVerified = true` sin comprobar `HardwareMuteSupport`. Reportar "Verified" al usuario en un micrófono con software mute constituye una afirmación de seguridad no veraz.

---

## 9. Desinstalador Standalone: Gate de Seguridad vs. Rollback Real

Al inspeccionar [`installer/setup.iss`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/installer/setup.iss#L743-L779):
1. **Adquisición del Gate:** Adquiere `Global\PrivLock_UninstallGate`.
2. **Evaluación de Sesiones:**
   ```pascal
   if AnyProfileMayHaveActiveSession or AnyPrivilegedOwnershipClaim then
   begin
     MsgBox('PrivGvard found privacy state that still requires recovery. Sign in to each affected account and start PrivGvard before uninstalling.',
       mbCriticalError, MB_OK);
     ReleaseUninstallGate;
     exit; // Aborta la desinstalación; NO restaura el hardware
   end;
   ```
3. **Conclusión Crítica:**
   El desinstalador de Inno Setup **no es un agente de rollback universal**. Si otro usuario de Windows bloqueó una cámara o un micrófono y cerró sesión, el desinstalador **se rehúsa a desinstalar** para evitar dejar hardware huérfano. La recuperación requiere que el usuario afectado inicie sesión o que un administrador intervenga manualmente. No existe "riesgo cero".

---

## 10. ProgramData, Permisos ACL y Reclamos (.claim)

1. **Estado de Código:** Los reclamos `.claim` **no existen en el código fuente** (`CODE VERIFIED = FALSE`).
2. **Seguridad NTFS en `C:\ProgramData`:**
   - La carpeta `C:\ProgramData` otorga por defecto permisos a `BUILTIN\Users` para crear carpetas.
   - Si un usuario estándar crea un archivo o carpeta, adquiere derechos de `CREATOR OWNER`.
   - **El Dilema de Seguridad:** Si los claims son vinculantes, cualquier proceso estándar puede crear claims falsos y provocar un Denial of Service (DoS) contra otros usuarios. Si son ignorados, no previenen colisiones. Sin un servicio elevado o broker con DACL restringida a `SYSTEM` y `Administrators`, los claims multiusuario en `ProgramData` son inviables.

---

## 11. Matriz Final de los 54 Escenarios Reauditados

Se presenta la matriz completa de los 54 escenarios, con separación estricta entre la presencia de código, el contrato de Microsoft, los tests automatizados, la prueba de VM, la clasificación de implementación y el resultado del escenario:

| ID | Escenario Concreto | Código Presente | Contrato Microsoft (URL / Norma) | Test Automatizado | Prueba VM | Clasificación Implementación | Resultado Escenario | Riesgo / Bloqueo Pendiente |
| :---: | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **01** | Instalación limpia sin Standalone. | Parcial (`Program.cs`) | MS Learn Desktop Bridge | `EditionProfileResolverTests` | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | No hay aislamiento de carpetas en DI. |
| **02** | Standalone primero; luego Store. | No (D-04 ausente) | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | D-04 no aprobada ni implementada. |
| **03** | Store primero; luego Standalone. | No (D-04 ausente) | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | D-04 no aprobada ni implementada. |
| **04** | ProgramData root no existe. | No | MS Learn Shell Folders | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Falla de sistema operativo anómala. |
| **05** | ACL de ProgramData incompatible. | No | MS Learn NTFS Security | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Claims no implementados. |
| **06** | `BUILTIN\Users` sin `CreateFiles`. | No | MS Learn NTFS Security | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Claims no implementados. |
| **07** | Claim Squatting: Usuario B claim falso. | No | Proceso Security Analysis | None | NOT RUN | **DESIGN ONLY** | **FAIL** | Diseño de claims vulnerable a DoS. |
| **08** | Dos usuarios crean claim a la vez. | No | MS Learn FileStream.CreateNew | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Atomicidad NTFS garantizada. |
| **09** | Claim con hash corrupto. | No | N/A | None | NOT RUN | **INVALID / CONTRADICTS SOURCE** | **FAIL** | Falso positivo: claims no existen. |
| **10** | Reparse Point en `.claim`. | No | MS Learn File Attributes | None | NOT RUN | **INVALID / CONTRADICTS SOURCE** | **FAIL** | Falso positivo: claims no existen. |
| **11** | NTFS Owner discrepa con `OwnerUserSid`. | No | MS Learn Security Descriptors | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Verificación NTFS sobre claims es diseño. |
| **12** | Store mute con hardware mute `Supported`.| Parcial (`WindowsCoreAudioController.cs` L:75)| MS Learn IAudioEndpointVolume | `WindowsCoreAudioTelemetryTests` | NOT RUN | **PARTIAL** | **PARTIAL** | Detecta bit pero UI lo ignora. |
| **13** | Store mute con hardware mute `NotSupported`.| Parcial (`WindowsProtectionProvider.cs` L:222)| MS Learn IAudioEndpointVolume | `WindowsCoreAudioTelemetryTests` | NOT RUN | **PARTIAL** | **PARTIAL** | Mute aplica; UI oculta que es software. |
| **14** | Store mute con hardware mute `Unknown`. | Sí (`WindowsCoreAudioController.cs` L:81) | COM HRESULT Contract | `WindowsCoreAudioTelemetryTests` | NOT RUN | **CODE VERIFIED** | **PASS** | Asigna Unknown limpiamente ante error. |
| **15** | Captura Shared Mode con mute activo. | Sí (`WindowsCoreAudioController.cs` L:151) | MS Learn WASAPI Mixer | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Requiere prueba física de audio en VM. |
| **16** | Captura Exclusive Mode con software mute. | Sí (`WindowsCoreAudioController.cs`) | MS Learn Exclusive Mode Streams | None | NOT RUN | **WINDOWS DOCUMENTED** | **PASS** | Bypass probado por documentación MS. |
| **17** | Dos apps captura en misma audio session. | No (OS mixer) | MS Learn Audio Sessions | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Alcance de sesión requiere VM. |
| **18** | Dos sesiones audio en misma cuenta. | No (OS mixer) | MS Learn IAudioEndpointVolume | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Endpoint afecta a nivel maestro. |
| **19** | Dos logon sessions Windows distintas. | No (OS mixer) | MS Learn Core Audio Architecture | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Multiusuario real requiere VM. |
| **20** | Fast User Switching (FUS). | No (OS driver) | MS Learn Audio Endpoints | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Persistencia a través de FUS requiere VM. |
| **21** | Sesión remota RDP con audio. | No | MS Learn Terminal Services Audio | None | NOT RUN | **VM REQUIRED** | **NOT VERIFIED** | Endpoints virtuales RDP requieren VM. |
| **22** | Usuario A mutea; B abre Store. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Señalización inter-usuario ausente. |
| **23** | Usuario B intenta unmute de endpoint A. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | En Core Audio nativo, B puede desmutear. |
| **24** | Usuario A logoff con mute activo. | No (OS driver) | MS Learn Audio Endpoint State | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Endpoint queda muteado en sistema. |
| **25** | Usuario A logon tras logoff. | Parcial (`Program.cs` startup) | N/A | `FilePrivacySessionStoreTests` | NOT RUN | **PARTIAL** | **NOT VERIFIED** | Reconexión de Store con WAL requiere VM. |
| **26** | Usuario A elimina su cuenta Windows. | No | MS Learn User Profile Service | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Perfil borrado; hardware stuck. |
| **27** | Store crashea tras `SetMute(true)`. | Parcial (`PrivacyRecoveryService.cs`) | N/A | `SafeUninstallRecoveryMatrixTests` | NOT RUN | **PARTIAL** | **NOT VERIFIED** | Recuperación existe; aislamiento no. |
| **28** | Store crashea antes de crear claim. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Claims no implementados. |
| **29** | Store crashea tras claim y antes mute. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Claims no implementados. |
| **30** | Store crashea tras mute antes de WAL. | Parcial (`FilePrivacySessionStore.cs` L:170) | N/A | `FileActivePrivacySessionMarkerTests` | NOT RUN | **PARTIAL** | **NOT VERIFIED** | Marker protege en Standalone; Store no probado. |
| **31** | Store se actualiza vía Windows Store. | No (OS updater) | MS Learn MSIX Updates | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Actualización preserva AppData. |
| **32** | Store se repara vía Windows Settings. | No (OS repair) | MS Learn MSIX Repair | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Repair preserva datos de usuario. |
| **33** | Store se resetea vía Windows Settings. | No (OS reset) | MS Learn MSIX Reset | None | NOT RUN | **VM REQUIRED** | **NOT VERIFIED** | Destruye datos en `LocalCache`. Requiere VM. |
| **34** | Store se desinstala con mute activo. | No (OS uninstaller) | MS Learn MSIX Lifecycle | None | NOT RUN | **WINDOWS DOCUMENTED** | **NOT VERIFIED** | Sin hooks; endpoint permanece muteado. |
| **35** | WAL en disco pero claim desapareció. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Claims no implementados. |
| **36** | Claim en disco pero WAL desapareció. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Claims no implementados. |
| **37** | Machine Latch en Standalone; Store inicia.| No | Propuesta D-04 | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | D-04 pendiente de aprobación. |
| **38** | Store claim activo; Standalone pide Secure.| No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Claims no implementados. |
| **39** | Standalone Secure; Store pide Standard. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Barrera inter-edición no implementada. |
| **40** | Machine Latch corrupto y `.bak` válido. | No | N/A | None | NOT RUN | **INVALID / CONTRADICTS SOURCE** | **FAIL** | Latch en ProgramData no existe. |
| **41** | Latch principal y backup corruptos. | No | N/A | None | NOT RUN | **INVALID / CONTRADICTS SOURCE** | **FAIL** | Latch en ProgramData no existe. |
| **42** | Escrow PnP completo con rollback. | Parcial (`WindowsPrivilegedOwnershipStore.cs`) | N/A | `PrivilegedOwnershipFingerprintTests` | NOT RUN | **PARTIAL** | **PARTIAL** | Solo guarda fingerprint SHA-256. |
| **43** | Escrow HKLM completo con rollback. | Parcial (`WindowsPrivilegedOwnershipStore.cs`) | N/A | `PrivilegedOwnershipFingerprintTests` | NOT RUN | **PARTIAL** | **PARTIAL** | Solo guarda fingerprint SHA-256. |
| **44** | GPO corporativa altera HKLM. | Parcial (`WindowsPolicyManager.cs` L:90) | MS Learn Group Policy Processing | None | NOT RUN | **PARTIAL** | **PARTIAL** | Regla compare-before-restore existe. |
| **45** | Rollback PnP falla en cámara 2. | Sí (`WindowsDeviceController.cs` L:180) | CfgMgr32 API | None | NOT RUN | **CODE VERIFIED** | **PARTIAL** | Falla cerrado; error nativo requiere hardware. |
| **46** | Endpoint USB desconectado durante Active. | Sí (`WindowsProtectionProvider.cs` L:214)| PnP Hotplug Notification | `MicrophoneAvailabilityAndProtectionMatrixTests` | NOT RUN | **CODE VERIFIED** | **PASS** | Manejo de presencia dinámico verificado. |
| **47** | Transaction Gate Mutex ocupado. | Parcial (`WindowsPrivilegedExecutor.cs` L:28) | MS Learn Mutex Class | `PrivilegedProtocolTests` | NOT RUN | **INVALID / CONTRADICTS SOURCE** | **FAIL** | El mutex real es `PrivLock_PrivilegedOperation`. |
| **48** | Mutex UI global impide segunda sesión. | Parcial (`WindowsSingleInstanceGuard.cs`) | MS Learn Named Objects | `WindowsSingleInstanceStartupTests` | NOT RUN | **PARTIAL** | **PARTIAL** | Mutex local existe; inter-edición no. |
| **49** | `OpenProcess` retorna `ACCESS_DENIED`. | Sí (`SafeUninstallLauncher.cs` L:110) | Win32 Process Security | `SafeUninstallLauncherTests` | NOT RUN | **WINDOWS DOCUMENTED** | **PASS** | Manejo conservador sin asumir liveness. |
| **50** | PID reciclado con misma SID pero otro LUID.| No | Token Statistics LUID | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | `OwnerTuple` con LUID no implementada. |
| **51** | Worker privilegiado pierde Named Pipe. | Sí (`WindowsPrivilegedWorker.cs` L:250) | MS Learn Named Pipes | `PrivilegedProtocolTests` | NOT RUN | **CODE VERIFIED** | **PASS** | Cierre abrupto del pipe aborta worker. |
| **52** | Desinstalador Inno Setup con claim Store. | No | N/A | None | NOT RUN | **DESIGN ONLY** | **NOT VERIFIED** | Inno Setup no conoce Store. |
| **53** | Desinstalador Inno Setup con sesión activa.| Sí (`setup.iss` L:753; `Program.cs` L:135) | Inno Setup Scripting | `InstallerUninstallRegistrationTests` | NOT RUN | **CODE VERIFIED** | **PASS** | Inno Setup aborta desinstalación limpiamente. |
| **54** | Reinicio abrupto (BSOD / corte) en mutación.| Parcial (`FilePrivacySessionStore.cs` L:172)| MS Learn NTFS Flush | `FilePrivacySessionStoreTests` | NOT RUN | **CODE VERIFIED (Storage)** | **NOT VERIFIED (Power)** | `WriteThrough` implementado; BSOD requiere VM. |

---

## 12. Comparativa Rigurosa de Opciones de Producto

| Criterio Técnico | Opción 1: Store como Cliente de Broker Standalone | Opción 2: Store Best-Effort (Standard Protection Únicamente) | Opción 3: Standalone First (Pausar Store) |
| :--- | :--- | :--- | :--- |
| **Capacidades Declaradas** | `runFullTrust` + IPC local hacia broker. | Requiere eliminar `allowElevation` y `unvirtualizedResources`. Solo `runFullTrust`. | N/A (Win32 ejecutable tradicional). |
| **Operación Permitida** | Standard y Secure (delegada a broker). | Únicamente Standard (Core Audio mute + navegación a Settings). | Standard y Secure completos (PnP DevNodes y HKLM). |
| **Exposición a Exclusive Mode** | Protegido mediante desactivación PnP en el broker. | **Expuesto:** Flujos en modo exclusivo eluden el software mute. | Protegido mediante desactivación PnP en el driver. |
| **Recuperación tras Crash** | Gestionada por el servicio broker centralizado. | Basada en el WAL local (si sobrevive en disco). | Basada en WAL local + ActiveSessionMarker. |
| **Recuperación tras Desinstalación** | Controlada por el broker de Standalone. | **Ninguna:** Endpoint puede quedar muteado sin rollback automático. | **Gate de Seguridad:** El instalador aborta si hay sesiones activas. |
| **Complejidad de Entrega** | **Muy Alta:** Requiere diseñar e implementar un broker Windows y protocolo IPC seguro. | **Media:** Requiere limpiar manifiesto, conectar DI y corregir UI. | **Baja:** Código consolidado, suite de tests verde y listo para empaque. |
| **Evidencia Faltante** | Arquitectura IPC, servicio NT y autenticación. | Prueba en VM de `LocalCache` y persistencia en Reset. | Validación final en VM limpia con múltiples usuarios. |
| **Recomendación Técnica** | Descartar a corto plazo. | Evaluar solo si el negocio acepta un producto básico transparente. | **RECOMENDADA:** Priorizar lanzamiento Standalone; mantener Store en hold. |

---

## 13. Protocolo de Pruebas Requeridas en VM Desechable (Pendiente)

Para cerrar definitivamente las clasificaciones `VM REQUIRED`, se establece el siguiente protocolo reproducible para ejecutar en una máquina virtual aislada con Windows 10/11:

1. **Parámetros del Entorno:**
   - Windows 11 23H2 (Build 22631) o Windows 10 22H2 (Build 19045) en VM aislada.
   - Dos usuarios estándar (`UserA`, `UserB`) y una cuenta Administrador.
   - Endpoint de audio virtual (ej. Virtual Audio Cable o driver emulado) para evitar alterar hardware real.
2. **Prueba 1: Trazabilidad Física de Storage (ProcMon):**
   - Iniciar Process Monitor con filtro: `ProcessName is PrivGvard.exe` y `Path contains PrivGuard`.
   - Ejecutar la versión MSIX e iniciar una mutación de Standard Protection.
   - Registrar la ruta física exacta de creación de `privacy-session-v1.json.tmp`, `privacy-session-v1.json`, `.bak` y `active-session.marker`.
   - **Criterio PASS:** Determinar si los archivos se crearon bajo `%LOCALAPPDATA%\Packages\<PFN>\LocalCache` o en `%LOCALAPPDATA%\PrivGuard`.
3. **Prueba 2: Ciclo de Vida Reset y Uninstall:**
   - Con el micrófono silenciado mediante MSIX, cerrar la aplicación.
   - Ejecutar en Windows Settings: *Aplicaciones instaladas > PrivGuard > Opciones avanzadas > Restablecer (Reset)*.
   - Inspeccionar si el WAL sobrevivió o fue borrado.
   - Repetir con *Desinstalar (Uninstall)*.
   - **Criterio PASS/FAIL:** Registrar si el endpoint de audio quedó silenciado en Windows sin aplicación instalada.
4. **Prueba 3: Coexistencia con Standalone:**
   - Ejecutar los escenarios A, B, C y D de la Sección 5.
   - Verificar si Standalone y Store leen o sobrescriben el mismo archivo o si operan en vistas aisladas.

---

## 14. Decisiones Pendientes del Propietario del Producto

Las siguientes decisiones arquitectónicas y de negocio quedan formalmente catalogadas como **`PENDING OWNER APPROVAL`**:

1. **Limpieza del Manifiesto Store:**  
   ¿Aprueba el propietario eliminar `<rescap:Capability Name="allowElevation" />` y `<rescap:Capability Name="unvirtualizedResources" />` de [`packaging/Package.appxmanifest`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest) para someter la app a revisión en Microsoft Store estrictamente como `runFullTrust` sin privilegios (`asInvoker`)?
2. **Aceptación del Contrato Store Standard Best-Effort:**  
   ¿Acepta el propietario del producto que la versión de Microsoft Store ofrezca únicamente silenciamiento de software por Core Audio, reconociendo transparentemente en la UI que los flujos en `AUDCLNT_SHAREMODE_EXCLUSIVE` pueden eludir el silenciamiento?
3. **Aprobación de Decisiones D-04 y D-05:**  
   - **D-04:** ¿Se aprueba que Store Edition adopte un "Modo Supervisión Informativo" (bloqueo pasivo sin mutación) si detecta que Standalone está activo?
   - **D-05:** ¿Se aprueba separar el almacenamiento en perfiles 100% independientes (`%LOCALAPPDATA%\PrivGuard\Store` vs `%LOCALAPPDATA%\PrivGuard\Standalone`), designando a Standalone como el único receptor de migraciones legacy?
4. **Estrategia de Release (Standalone First):**  
   ¿Aprueba el propietario del producto congelar temporalmente el pipeline de Microsoft Store y avanzar con el empaquetado y distribución de **PrivGuard Standalone (Inno Setup)**?

---

## 15. Bloqueos Reales para la Fase 8B.1C

Para que la Fase **8B.1C** pueda ser autorizada, deben resolverse los siguientes bloqueos de implementación:

1. **Bloqueo B-01 (Alineación de Manifiesto):** Modificar el manifiesto MSIX para cumplir con D-01 (requiere aprobación de Decisión 1).
2. **Bloqueo B-02 (Cableado en DI):** Registrar `IPackageIdentityDetector` e `IEditionProfileResolver` en [`Program.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L560), e inyectar el perfil en `WindowsProtectionProvider` para bloquear mutaciones Secure en Store.
3. **Bloqueo B-03 (Corrección de ConsentStore):** Reemplazar las escrituras directas a `HKCU` en `WindowsUserPrivacyManager` por navegación a `ms-settings:privacy-webcam` en la edición Store.
4. **Bloqueo B-04 (Semántica UI en Core Audio):** Condicionar `micStandardIsVerified` a la presencia de `HardwareMuteSupport.Supported` para reflejar la realidad técnica frente a Exclusive Mode.
5. **Bloqueo B-05 (Aislamiento de WAL):** Parametrizar `FilePrivacySessionStore` para utilizar subdirectorios aislados por edición.

---

## 16. Fuentes Oficiales de Microsoft Learn

- **Flexible Virtualization in MSIX:**  
  *Disable registry and file system write virtualization in packaged desktop apps*  
  URL: `https://learn.microsoft.com/en-us/windows/msix/desktop/flexible-virtualization`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* Describe que las escrituras a AppData y Registry se virtualizan a carpetas privadas salvo que se declaren explícitamente las propiedades `desktop6:FileSystemWriteVirtualization` y `RegistryWriteVirtualization`.
- **Behind the Scenes: Desktop to UWP:**  
  *Understanding how packaged desktop apps run on Windows*  
  URL: `https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-behind-the-scenes`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* Detalla la regla de AppData en Windows 10 versión 1903+, donde los archivos nuevos van a `LocalCache` y los existentes en la raíz pueden ser modificados in situ.
- **App Capability Declarations:**  
  *Restricted capabilities and partner review*  
  URL: `https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations#restricted-capabilities`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* `allowElevation` y `unvirtualizedResources` son restringidas y requieren envío de justificación durante la certificación de Partner Center.
- **Reset and Repair MSIX Apps:**  
  *Managing your MSIX deployment: Reset and Repair*  
  URL: `https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps#reset-and-repair-apps`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* Reset elimina los datos de aplicación de la app; Repair reinstala preservando los datos.
- **IAudioEndpointVolume Interface:**  
  *Windows Core Audio endpoint volume management*  
  URL: `https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nn-endpointvolume-iaudioendpointvolume`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* Describe el alcance maestro a nivel de endpoint físico de audio.
- **IAudioEndpointVolume::QueryHardwareSupport Method:**  
  *Querying hardware mute and volume capabilities*  
  URL: `https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nf-endpointvolume-iaudioendpointvolume-queryhardwaresupport`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* El flag `ENDPOINT_HARDWARE_SUPPORT_MUTE` (0x2) define si el mute está soportado en hardware/driver.
- **Exclusive-Mode Audio Streams:**  
  *Direct hardware access in Core Audio*  
  URL: `https://learn.microsoft.com/en-us/windows/win32/coreaudio/exclusive-mode-streams`  
  *Fecha de Consulta:* 6 de Noviembre de 2026.  
  *Afirmación Respaldada:* Los flujos exclusivos omiten el motor de audio de Windows (mixer) y los controles de software asociados.

---

## 17. Conclusión y Cierre

1. **Estado del Repositorio:** Limpio, inalterado, sin commits, ramas ni archivos modificados.
2. **Microsoft Store:** **`CONDITIONAL HOLD`**.
3. **Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`**.
4. **Veredicto Final:** **`DOCUMENTARY AUDIT CLOSED — VM VALIDATION PENDING`**.

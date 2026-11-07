# PrivGuard — Fase 8B.1A.7.2: Rebaseline de Supuestos y Readiness del Laboratorio VM (Revisión Factual y Corrección Estática)

**Fecha de emisión:** 2026-11-07<br />
**Fecha de revisión factual:** 2026-11-07<br />
**Autor:** Principal .NET / Windows Security Engineer & Transactional Recovery Auditor<br />
**Rama activa de trabajo:** `docs/8b1a7-2-final-evidence-reconciliation`<br />
**Baseline de código auditado:** `653be31040da53bea72a540ddb2a111f16b35d50`<br />
**HEAD documental remoto (`origin/main`):** `fc345ebd06239e8b86c0c502a4b289052e988a8c`<br />
**Parent directo en `main`:** `aedd19c819ddeaf4e482576fb973f490980b7aa5`<br />
**Parent funcional anterior (Baseline auditado):** `653be31040da53bea72a540ddb2a111f16b35d50`<br />
**Último workflow CI:** Run #95 (ID: `42420318570`, commit `fc345eb`, evento `push`, **7/7 jobs `success`**)<br />
**Workflows CI previos relevantes:**<br />
- Run #94 (ID: `42417187681`, commit `aedd19c`, evento `push`, **7/7 jobs `success`**)<br />
- Run #93 (ID: `42410227273`, commit `54fb446`, evento `push`, **7/7 jobs `success`**)<br />
- Run #92 (ID: `42396473407`, commit `653be31`, evento `push`, **7/7 jobs `success`**)<br />
- Runs #87 y #86 (IDs: `42346415046` y `42342850877`, commits `391d79e` y `0a270a1`, evento `pull_request` en PR #11, **7/7 jobs `success`**)<br />
**Veredicto de Rebaseline:** **PASS — Auditoría estática y conciliación factual completadas**<br />
**Readiness del Laboratorio VM:** **HOLD — Requiere aprovisionamiento/acción del propietario en Hipervisor L0**<br />
**Estado Microsoft Store:** **CONDITIONAL HOLD**<br />
**Estado Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**

---

## 1. Resumen Ejecutivo y Veredicto Formal

### 1.1 Resumen Ejecutivo
El presente documento constituye la **revisión factual, adversarial y correctiva final** del rebaseline técnico de la Fase 8B.1A.7.2 (preparación para pruebas dinámicas de almacenamiento MSIX y recuperación transaccional en VM aislada).

Los informes y runbooks previos de virtualización (Fases 8B.1A.5, 8B.1A.6 y 8B.1A.7.1) fueron redactados tomando como base el commit histórico `9f856c0405e4e5b64e784309affad450c38d3a37`. Desde dicho punto, la historia del repositorio avanzó mediante 5 commits en el rango `9f856c0..d8ca7acc` (Sprint 1.0, 1.1 y 1.2) seguidos de **8 commits exclusivamente documentales en `d8ca7acc..fc345eb`**:
- Cuatro commits documentales previos: `dded99c`, `b9d4b8f`, `07e4188` y `653be31` (este último fijando el baseline del código auditado).
- Cuatro commits documentales correctivos posteriores: `3f08f12`, `54fb446`, `aedd19c` y `fc345eb`.

**Reconciliación Post-Push y Trazabilidad:** El commit `fc345eb` (`docs: correct mutex DACL and FUS handoff analysis`), formulado inicialmente para reconciliar la DACL del mutex global y el handoff en FUS, fue integrado en `origin/main` mediante push directo. La verificación remota en GitHub confirma que `origin/main` se encuentra en `fc345eb` y que el workflow de CI Run #95 (ID: `42420318570`, evento `push`) concluyó con 7/7 jobs en `success`.

La comparación Git confirma que los commits posteriores a `653be31` no modificaron código funcional ni de configuración (`0 líneas de código alteradas`). Por tanto, **`653be31` permanece como el baseline del código auditado estáticamente**, mientras que **`fc345eb` representa el HEAD documental remoto canónico**.

Asimismo, **CI no publica un paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`** (los 4 artefactos publicados en los Runs #89–#95 corresponden exclusivamente a ejecutables single-file: `win-x64`, `win-arm64`, `linux-x64` y `osx-arm64`). El empaquetado Release-Store MSIX se ejecuta de forma efímera en `RUNNER_TEMP` para verificación estática de manifiesto, pero no se conserva. La prueba en laboratorio requiere generar un nuevo paquete MSIX y un manifiesto de proveniencia explícito desde el HEAD canónico a evaluar.

**Comportamiento de Eventos en CI (`push` vs. `pull_request`):** Se precisa que el trigger `pull_request` fue verificado empíricamente y superado con éxito en los Runs #86 y #87 durante la tramitación del PR #11 (`feature/sprint-1-store-profile-guards-d01-d02`). No obstante, todos los runs subsiguientes (#89 al #95) correspondieron a eventos de tipo `push` directo sobre la rama `main`, por lo que los ajustes documentales posteriores al merge del PR #11 no han transitado por un flujo de Pull Request.

Esta revisión introduce precisiones técnicas indispensables alineadas con el código real y la documentación oficial de Microsoft Learn:

1. **Full Trust no equivale a AppContainer (Comportamiento de AppData):**
   - **Evidencia del Manifiesto:** En `packaging/Package.appxmanifest`, tanto la aplicación principal como la extensión de inicio declaran explícitamente `EntryPoint="Windows.FullTrustApplication"`, y el paquete declara `<rescap:Capability Name="runFullTrust" />`. No se declara un proceso AppContainer.
   - **Contrato de Microsoft Learn:** De acuerdo con la documentación oficial ([*MSIX Containerization Overview*](https://learn.microsoft.com/en-us/windows/msix/msix-containerization-overview) y [*Understanding how packaged desktop apps run on Windows*](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-behind-the-scenes)), las aplicaciones Full Trust (medium integrity) no están sujetas a la redirección de AppData por paquete característica de los procesos AppContainer. Sus escrituras en AppData pasan directamente a la ruta normal (*pass-through*).
   - **Evidencia del Código:** `FileStateStore`, `FilePrivacySessionStore` y `FileActivePrivacySessionMarker` solicitan rutas canónicas ordinarias de `%LOCALAPPDATA%`, sin inyectar las rutas aisladas calculadas por `DefaultEditionProfileResolver`. Por ende, **D-05 no está integrada en la composición de DI de los stores**.
   - **Expectativa Técnica Correcta:** La expectativa más respaldada por el manifiesto y la arquitectura documentada por Microsoft es que **Store y Standalone utilicen las mismas ubicaciones físicas en `%LOCALAPPDATA%` dentro del mismo perfil de usuario de Windows**, y no una ubicación privada en `LocalCache` por defecto. La colisión física o compartición de estado es la hipótesis primaria más sólida, cuya confirmación empírica en el ejecutable instalado corresponde a las pruebas con ProcMon en VM (donde `Packages\<PFN>\LocalCache` se inspeccionará para descartar anomalías de plataforma).
2. **Alcance Real de la Comprobación del Desinstalador Inno Setup (`setup.iss`):**
   - El marcador activo se construye mediante `GetWindowsActiveMarkerDirectory()` en `Program.cs` bajo `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker`.
   - En `installer/setup.iss`, la función `ProfileMayHaveActiveSession` comprueba:
     - `PrivGvard\Recovery`: busca `active-session-v1.marker` y diario/backup (`privacy-session-v1.json`). **Esta ubicación cubre el marcador activo del código vigente**.
     - `PrivLock\Recovery`: busca marcador y diario/backup.
     - `PrivGuard\Recovery`: busca diario y backup, **no el marcador activo**.
   - `setup.iss` enumera perfiles mediante `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList` y bloquea la desinstalación (*fail-closed*) si detecta un diario o marcador activo.
   - **Límite de Autoridad:** Esta salvaguarda pertenece exclusivamente al desinstalador Standalone de Inno Setup. **No tiene capacidad alguna para impedir que Windows desinstale el paquete MSIX de Store** (a través de Configuración de Windows o `Remove-AppxPackage`).
3. **Seguridad y Accesibilidad Real del Mutex Global (DACL vs. Namespace):**
   - Target framework: `net10.0-windows10.0.19041.0`. El código invoca `new Mutex(initiallyOwned: false, _name)`.
   - `Global\PrivLock_SingleInstance` y `Global\PrivLock_UninstallGate` utilizan el namespace global de Windows (`\BaseNamedObjects`).
   - La visibilidad global del nombre **no equivale a permisos irrestrictos de acceso**. Sin embargo, la ausencia de una ACL explícita **tampoco significa que otro usuario estándar siempre vaya a ser denegado**.
   - De acuerdo con la documentación oficial de .NET para el constructor nombrado de `Mutex`, los objetos nombrados del sistema pueden ser accedidos por otros usuarios a menos que se configure seguridad explícita (`MutexSecurity` / `MutexAcl.Create`). A su vez, Win32 `CreateMutexW` solicita `MUTEX_ALL_ACCESS` cuando el objeto ya existe y utiliza el descriptor de seguridad predeterminado del token del creador si `lpMutexAttributes` es `NULL`.
   - Por tanto, la denegación o concesión efectiva de acceso a un segundo usuario (UserB) depende de la DACL concreta que el token del creador (UserA) aplique en el sistema. Debe distinguirse si un fallo deriva de permisos (`UnauthorizedAccessException`), contención (`WaitOne(0) == false`), o mutex abandonado (`AbandonedMutexException`).
   - El mutex actúa como una guarda de admisión de instancia única; **no es un coordinador transaccional de recursos, no provee aislamiento ni arbitraje de almacenamiento, no administra leases de diario y no integra D-03/D-05**. Su comportamiento multiusuario efectivo debe ser verificado en TEST-09.
4. **Disparidad de Ámbitos entre Mutex Global y Evento Local de Desinstalación:**
   - Mientras los mutexes de admisión utilizan el namespace global (`Global\PrivLock_SingleInstance`, `Global\PrivLock_UninstallGate`), el listener de desinstalación declara `private const string RequestEventName = @"Local\PrivLock_UninstallRequested"`.
   - El prefijo `Local\` confina el evento al namespace de la sesión Terminal Services del creador (`\Sessions\<SessionId>\BaseNamedObjects`).
   - En Fast User Switching (FUS), UserA y UserB operan en sesiones Terminal Services distintas. El evento de UserA en la Sesión 1 **no es visible para el proceso de UserB en la Sesión 2**.
   - En consecuencia, en un escenario FUS cruzado, `TryRequestRunningInstanceAndAcquire()` no puede señalizar el listener de UserA (`requestSent = false`), experimentando un timeout hasta fallar con `"No running PrivGvard instance accepted the uninstall shutdown request."` En la misma sesión, el protocolo opera según su diseño original.
5. **Alcance Real de los Tests Unitarios Actuales:**
   - `WindowsSingleInstanceStartupTests.cs` evalúa dos tareas `Task.Run` concurrentes dentro del **mismo proceso OS** y utilizando nombres `Local\..._Test_<GUID>`.
   - Demuestra exclusión concurrente entre tareas de un mismo proceso/sesión.
   - **No demuestra:** procesos OS independientes, usuarios con SIDs distintos, sesiones Terminal Services separadas, FUS, DACL efectiva global ni handoff de desinstalación entre sesiones.
6. **Significado Exacto de `HypervisorPresent = True`:**
   - La consulta WMI/CIM se ejecutó desde dentro de un Guest de VirtualBox (`innotek GmbH / VirtualBox`). La propiedad `HypervisorPresent = True` indica únicamente que el Windows invitado detecta un hipervisor expuesto a él.
   - El estado real de la CPU física, BIOS/UEFI, memoria RAM total y capacidades de virtualización del host físico L0 son **desconocidos desde el interior del guest** y deben ser verificados por el propietario directamente en el host físico.
7. **Topologías de Virtualización y Estimaciones de Recursos:**
   - Se distinguen los requerimientos reales del sistema operativo invitado (Windows 11: 4 GB vRAM, 2 vCPU, 64 GB almacenamiento, TPM 2.0 y Secure Boot) de las **estimaciones y recomendaciones de planificación** para el host.
   - Se mantiene como alternativa recomendada y más simple la provisión de una VM directa en el host físico (Topología B, ~16 GB RAM física total estimada), frente al overhead y complejidad de tres capas (Topología A: L0 físico -> L1 VirtualBox -> L2 VM anidada).
8. **Cadena de Custodia Criptográfica del Artefacto MSIX:**
   - Se formaliza que el SHA-256 del paquete unsigned generado en build y el SHA-256 del paquete firmado tras test-signing dentro de la VM son distintos y deben registrarse de forma diferenciada en la proveniencia del laboratorio.

### 1.2 Veredicto Oficial

$$\mathbf{VEREDICTO:\ REBASELINE\ PASS\ —\ LAB\ HOLD\ (REQUIRES\ OWNER\ ACTION)}$$

- **Rebaseline de Supuestos:** **PASS**. Se reconcilia la DACL del mutex global con la documentación oficial de .NET y Win32, se identifica la frontera de sesión del evento `Local\`, se precisan los límites de los tests unitarios y se sincroniza el estado post-push.
- **Readiness del Laboratorio VM:** **HOLD**. El entorno local actual es un guest sin virtualización anidada expuesta y recursos insuficientes. Se requiere acción del propietario en el host físico.
- **Salvaguardas Invariantes:** Microsoft Store en `CONDITIONAL HOLD`, Fase 8B.1C en `BLOCKED / NOT AUTHORIZED`.

---

## 2. Baseline de Control de Versiones e Integración Factual

### 2.1 Verificación de Git y Remoto
- **Repositorio:** `cristianbarreiro/PrivGuard`
- **Rama activa de trabajo:** `docs/8b1a7-2-final-evidence-reconciliation`
- **HEAD Remoto Canónico (`origin/main`):** `fc345ebd06239e8b86c0c502a4b289052e988a8c`
- **Parent directo en `main`:** `aedd19c819ddeaf4e482576fb973f490980b7aa5`
- **Parent funcional anterior (Baseline auditado):** `653be31040da53bea72a540ddb2a111f16b35d50`
- **Historial `d8ca7acc..fc345eb` (8 commits exclusivamente documentales):**
  1. `dded99c` — Documental: corrección de evidencia Sprint 1.2 postflight.
  2. `b9d4b8f` — Documental: ajuste de afirmaciones Sprint 1.2 postflight.
  3. `07e4188` — Documental: reconciliación Sprint 1.2 con estado remoto.
  4. `653be31` — Documental: rebaseline 8B.1A.7.2 contra código vigente (baseline de auditoría estática).
  5. `3f08f12` — Documental: corrección factual 8B.1A.7.2.
  6. `54fb446` — Documental: análisis Full Trust AppData y desinstalación.
  7. `aedd19c` — Documental: alineación post-push de baseline y análisis de mutex FUS.
  8. `fc345eb` — Documental: corrección de análisis de DACL de mutex y handoff en FUS (HEAD documental canónico).
- **Forma Real de Integración:** Los commits sobre `main` fueron integrados directamente mediante push sin Pull Request. La consulta a la API de GitHub (`/branches/main`) confirma `protected: false` (la consulta específica a `/protection` devolvió 401/403 por alcance del token; no hay required status checks activos que restrinjan push directo). No existen Pull Requests abiertas actualmente.
- **Estado del Árbol de Trabajo:** Limpio (`nothing to commit, working tree clean`).
- **Código Funcional Modificado:** **0 líneas**. Todos los commits posteriores a `d8ca7ac` son estrictamente documentales.

### 2.2 Evidencia de GitHub Actions (Runs #86, #87 y #89 al #95)
- **Workflow Run más reciente:** Run #95
  - **Run ID:** `42420318570`
  - **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42420318570](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42420318570)
  - **Commit:** `fc345ebd06239e8b86c0c502a4b289052e988a8c`
  - **Rama:** `main` | **Evento:** `push`
  - **Estado / Conclusión:** `completed / success`
  - **Jobs:** **7/7 jobs finalizados en `success`** (Build & Test en windows-latest, ubuntu-latest, macos-latest; y Publish Single-File para osx-arm64, win-x64, linux-x64, win-arm64). En Windows se ejecutó con éxito el paso de empaquetado y verificación MSIX Store.
  - **Artefactos publicados:** 4 ejecutables single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicó ningún paquete MSIX descargable ni `BUILD-PROVENANCE.json`**.
- **Workflow Runs previos:**
  - **Run #94:** ID `42417187681`, commit `aedd19c`, evento `push`, conclusión `success` (7/7 jobs `success`, 4 ejecutables single-file).
  - **Run #93:** ID `42410227273`, commit `54fb446`, evento `push`, conclusión `success` (7/7 jobs `success`, 4 ejecutables single-file).
  - **Run #92:** ID `42396473407`, commit `653be31` (baseline de código auditado), evento `push`, conclusión `success` (7/7 jobs `success`).
  - **Runs #87 y #86 (Validación de evento `pull_request`):** IDs `42346415046` y `42342850877`, commits `391d79e` y `0a270a1`, rama `main`, evento `pull_request` (PR #11), ambos con **7/7 jobs en `success`**.

> [!NOTE]
> **Clarificación de Cobertura de Eventos en CI:** El evento `pull_request` **sí fue verificado empíricamente y superado con éxito** en los Runs #86 y #87 durante la tramitación y merge de la PR #11 (`feature/sprint-1-store-profile-guards-d01-d02`). Lo que no ha corrido bajo `pull_request` son los 8 commits documentales posteriores (`d8ca7acc..fc345eb`), los cuales fueron integrados mediante eventos de tipo `push` directo sobre la rama `main` (Runs #89 al #95).

---

## 3. Matriz de Reconciliación Corregida de Supuestos Históricos

La siguiente tabla confronta las afirmaciones de informes históricos frente a la realidad del código en `653be31`, delimitando qué constituye código implementado y qué es una hipótesis que requiere validación en VM:

| Documento Histórico | Afirmación Histórica | Estado Real en Código Actual (`653be31`) | Dictamen de Vigencia | Evidencia / Ubicación en Código |
|---|---|---|---|---|
| **8B.1A.5 / 8B.1A.6** (P0-01) | El manifiesto MSIX declara `allowElevation` y `unvirtualizedResources`, violando D-01. | El manifiesto `packaging/Package.appxmanifest` declara **únicamente** `<rescap:Capability Name="runFullTrust" />`. Las capacidades `allowElevation` y `unvirtualizedResources` fueron eliminadas y su ausencia está protegida por 16 tests unitarios y el CI gate. | **SUPERSEDIDO** (Resuelto por D-01) | [`packaging/Package.appxmanifest#L59-L61`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest#L59-L61)<br />[`StoreMsixManifestVerifier.cs#L95-L105`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/Packaging/StoreMsixManifestVerifier.cs#L95-L105) |
| **8B.1A.5 / 8B.1A.6** (P0-02) | `IPackageIdentityDetector` e `IEditionProfileResolver` no están registrados en el contenedor de DI en `Program.cs`. | `Program.Main` resuelve el snapshot inmutable y `ConfigureServices` **registra como singletons en DI** tanto `IPackageIdentityDetector` como `IEditionProfileResolver`. También se configura en `WindowsPrivilegedSession`. | **SUPERSEDIDO** (Resuelto por Sprint 1.0) | [`Program.cs#L25`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L25)<br />[`Program.cs#L628-L630`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L628-L630)<br />[`Program.cs#L682`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L682) |
| **8B.1A.5 / 8B.1A.6** (P0-03) | `WindowsUserPrivacyManager` escribe directamente en HKCU ConsentStore para Store Edition y no existe uso de `ms-settings:`. | `WindowsUserPrivacyManager` divide sus contratos en Reader y Writer. `SetCameraUserPrivacy` y `SetMicrophoneUserPrivacy` **rechazan explícitamente cualquier intento de escritura si el perfil no es `Standalone`** (D-02). En Store Edition, la mutación de cámara devuelve error dirigiendo a `ms-settings:privacy-webcam`. | **SUPERSEDIDO** (Resuelto por Sprint 1.0 / D-02) | [`WindowsUserPrivacyManager.cs#L62-L71`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/Policies/WindowsUserPrivacyManager.cs#L62-L71)<br />[`WindowsProtectionProvider.cs#L66-L74`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L66-L74) |
| **8B.1A.5 / 8B.1A.6** (P0-04) | El almacenamiento de Store está aislado por perfil y protegido frente a desinstalación de Windows. | **Precisión Full Trust:** El paquete declara `Windows.FullTrustApplication` y `runFullTrust`. Según Microsoft Learn, las escrituras de Full Trust en AppData no se redirigen por paquete (pass-through). En código, los stores no consumen el resolver (D-05 no integrada en DI). Por ende, **la expectativa técnica documentada es que ambas ediciones compartan las mismas rutas físicas en `%LOCALAPPDATA%`**. La confirmación empírica corresponde a ProcMon en VM. | **PARCIALMENTE VÁLIDO / EXPECTATIVA PASS-THROUGH (REQUIERE VM)** | [`Program.cs#L632-L643`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L632-L643)<br />[`FileStateStore.cs#L24-L30`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Infrastructure.Common/Storage/FileStateStore.cs#L24-L30)<br />[`FilePrivacySessionStore.cs#L59-L62`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Infrastructure.Common/Storage/FilePrivacySessionStore.cs#L59-L62) |
| **8B.1A.5 / 8B.1A.6** (P0-05) | `HKLM\SOFTWARE\PrivLock\PrivilegedOwnership\v2` actúa como un Machine Recovery Escrow completo. | Se confirma la refutación: HKLM almacena únicamente un fingerprint criptográfico SHA-256, un SID y una etapa. **No almacena datos de reversión de hardware ni snapshots de registro**. Si el WAL del usuario se destruye, el hardware no puede restaurarse desde HKLM. | **VÁLIDO (REFUTACIÓN CONFIRMADA)** | [`WindowsPrivilegedOwnershipStore.cs#L140-L180`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/Privileged/WindowsPrivilegedOwnershipStore.cs#L140-L180) |
| **8B.1A.5 / 8B.1A.6** (P1-01) | `WindowsProtectionProvider` marca `micStandardIsVerified = true` ignorando si el hardware soporta mute físico. | `EvaluateMicrophoneStandardVerification` evalúa `AudioHardwareMuteSummary`. Si el mute es solo por software (`NotSupported` o `Unknown`), **no se otorga verificación de exclusión ni protección en modo exclusivo**, emitiendo advertencia explícita al usuario. | **SUPERSEDIDO** (Resuelto en código) | [`WindowsProtectionProvider.cs#L351-L355`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L351-L355)<br />[`WindowsProtectionProvider.cs#L481-L494`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L481-L494) |
| **8B.1A.5** (Problema B) | Archivos `.claim` en `%ProgramData%\PrivGuard\endpoints` resuelven la exclusión mutua cross-edition. | El diseño de claims en `ProgramData` fue declarado inviable por riesgo de *Claim Squatting*, DoS y redirección VFS. **No existe ninguna implementación de claims en código** (`src/**/*.cs` tiene cero referencias). El diseño está formalmente descartado. | **VÁLIDO (DISEÑO RECHAZADO)** | Búsqueda global en código (`0 coincidencias`). |
| **8B.1A.5** (P1-03) | Existe sincronización cross-edition mediante `Global\PrivGuard_Transaction_Gate` y `OwnerTuple`. | No existen en código. `Global\PrivLock_SingleInstance` y `Global\PrivLock_UninstallGate` utilizan el namespace global de Windows para exclusión de instancia única; no poseen DACL explícita y no constituyen un coordinador de recursos, de almacenamiento ni de diarios cross-edition. | **VÁLIDO (NO IMPLEMENTADO)** | [`WindowsSingleInstanceGuard.cs#L12-L15`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L12-L15) |
| **8B.1A.7.1** (Sección 6-7) | El paquete `PrivGuard-2.0.0.0-x64.msix` y su proveniencia generados en `$env:TEMP` son válidos para la prueba en VM. | Dicho paquete y su `BUILD-PROVENANCE.json` fueron generados desde el commit `9f856c0`. El historial contiene 5 commits en el rango `9f856c0..d8ca7acc` que alteraron código funcional y de packaging. **El artefacto está formalmente obsoleto** y debe reconstruirse desde el HEAD vigente cuando el laboratorio esté listo. | **SUPERSEDIDO (ARTEFACTO OBSOLETO)** | Historial Git: rango `9f856c0..d8ca7acc` (commits `0a270a1`, `391d79e`, `d8ca7ac`). |

---

## 4. Auditoría Estática Corregida del Código Vigente

### 4.1 Full Trust y Rutas Efectivas en AppData (Frontera D-05)
La evaluación de las rutas de almacenamiento se estructura a partir de los siguientes hechos verificables:

1. **Declaración del Manifiesto:**
   - [`packaging/Package.appxmanifest#L36-L38`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest#L36-L38): `Application Id="PrivGuardApp" Executable="PrivGvard.exe" EntryPoint="Windows.FullTrustApplication"`.
   - [`packaging/Package.appxmanifest#L52`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest#L52): Extensión de inicio con `EntryPoint="Windows.FullTrustApplication"`.
   - [`packaging/Package.appxmanifest#L60`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/packaging/Package.appxmanifest#L60): `<rescap:Capability Name="runFullTrust" />`.
   - No existe declaración de proceso AppContainer (`uap:AppContainer` ausente).
2. **Doctrina Oficial de Microsoft Learn:**
   - Según [*MSIX Containerization Overview*](https://learn.microsoft.com/en-us/windows/msix/msix-containerization-overview), los procesos Full Trust se ejecutan con nivel de integridad media (*medium integrity*), sin el aislamiento de AppContainer. La redirección de AppData hacia una carpeta privada por paquete (`Packages\<PFN>\LocalCache`) aplica a aplicaciones virtualizadas/AppContainer. En aplicaciones Full Trust, las operaciones sobre AppData siguen la ruta normal del sistema de archivos.
3. **Composición de DI en Código:**
   - En [`Program.cs#L632-L643`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L632-L643), los tres stores se instancian sin consumir `IEditionProfileResolver`:
     - `FileStateStore` -> [`StorageMigrationHelper.GetDefaultDataDirectory()`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Infrastructure.Common/Storage/FileStateStore.cs#L26) -> `%LOCALAPPDATA%\PrivGuard\state.json`.
     - `FilePrivacySessionStore` -> `.../Recovery/privacy-session-v1.json` y `privacy-session-v1.json.bak`.
     - `FileActivePrivacySessionMarker` -> [`GetWindowsActiveMarkerDirectory()`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Desktop/Program.cs#L740-L747) -> `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker`.
   - **D-05 no está integrada en DI**.
4. **Conclusión Estática Correcta:**
   - Para esta configuración específica, la expectativa técnica más fundamentada es que **la edición Store y la edición Standalone accedan a las mismas ubicaciones físicas en `%LOCALAPPDATA%`**. La compartición o colisión física es la hipótesis con mayor respaldo formal.
   - **Límite:** ProcMon deberá confirmar empíricamente en la VM que Windows no aplique ninguna redirección inesperada bajo la versión exacta de compilación del sistema operativo.

### 4.2 Alcance Real de la Cobertura de `installer/setup.iss`
Al auditar directamente el desinstalador Inno Setup ([`installer/setup.iss#L18-L21`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/installer/setup.iss#L18-L21) y [`lines 501-506`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/installer/setup.iss#L501-L506)):

- **Constantes de Ruta:**
  ```pascal
  #define RecoveryRelativePath "AppData\Local\PrivGvard\Recovery\"
  #define LegacyRecoveryRelativePath "AppData\Local\PrivLock\Recovery\"
  #define ModernRecoveryRelativePath "AppData\Local\PrivGuard\Recovery\"
  #define ActiveMarkerFile "active-session-v1.marker"
  #define JournalFile "privacy-session-v1.json"
  ```
- **Lógica de Inspección en `ProfileMayHaveActiveSession`:**
  1. `RecoveryRelativePath` (`PrivGvard\Recovery`): comprueba la existencia de `active-session-v1.marker` y del diario `privacy-session-v1.json` (con `.bak`).
  2. `LegacyRecoveryRelativePath` (`PrivLock\Recovery`): comprueba marcador y diario.
  3. `ModernRecoveryRelativePath` (`PrivGuard\Recovery`): comprueba **únicamente el diario y backup, NO el marcador activo**.
- **Resultado Factual:**
  - Dado que el código en `Program.cs` construye el marcador activo en `PrivGvard\Recovery\active-session-v1.marker`, **dicha ubicación sí está cubierta por la comprobación de Inno Setup**.
  - El diario moderno en `PrivGuard\Recovery` también está cubierto.
  - `setup.iss` enumera los perfiles interactivos desde `ProfileList` en HKLM y adopta una política *fail-closed* bloqueando la desinstalación si detecta un diario o marcador activo.
- **Límite Estricto:** Esta protección es exclusiva del instalador/desinstalador Standalone de Inno Setup. **No tiene intervención ni capacidad para bloquear la desinstalación del paquete MSIX por parte de Windows**.

### 4.3 Cuantificación de Commits (`9f856c0..fc345eb`)
El historial entre `9f856c0` y el HEAD remoto canónico `fc345eb` se estructura en dos segmentos perfectamente diferenciados:

1. **Rango `9f856c0..d8ca7acc` (5 commits funcionales / CI / merge):**
   - `7066b3b` — **Documental:** Registro de informes de Fase 8B.1A y proveniencia.
   - `0a270a1` — **Funcional / Packaging:** Integración de perfiles Store/Standalone, guardas D-01/D-02 y manifiesto depurado.
   - `391d79e` — **Funcional:** Inmutabilidad de perfil y rechazo fail-closed de mutaciones parciales `Both` en Store.
   - `ffc2273` — **Merge:** Fusión de Pull Request #11 (`feature/sprint-1-store-profile-guards-d01-d02`).
   - `d8ca7ac` — **CI / Packaging:** Incorporación del gate de validación Release-Store en `.github/workflows/ci.yml`.

2. **Rango `d8ca7acc..fc345eb` (8 commits exclusivamente documentales):**
   - `dded99c` — **Documental:** Corrección de evidencia de postflight Sprint 1.2.
   - `b9d4b8f` — **Documental:** Ajuste de afirmaciones de postflight Sprint 1.2.
   - `07e4188` — **Documental:** Reconciliación de postflight Sprint 1.2 con estado remoto.
   - `653be31` — **Documental:** Rebaseline 8B.1A.7.2 contra código vigente (baseline de auditoría estática).
   - `3f08f12` — **Documental:** Corrección factual de rebaseline 8B.1A.7.2.
   - `54fb446` — **Documental:** Corrección de análisis Full Trust AppData y rutas de desinstalación.
   - `aedd19c` — **Documental:** Alineación post-push de baseline y análisis de mutex FUS.
   - `fc345eb` — **Documental:** Corrección de análisis de DACL de mutex y handoff en FUS (HEAD documental canónico).

### 4.4 Mutex Global, DACL, Evento `Local\` y Fast User Switching (FUS)
La auditoría estática adversarial sobre los objetos de sincronización y control de instancia en Windows (`net10.0-windows10.0.19041.0`) establece las siguientes precisiones técnicas rigurosas:

1. **Implementación de .NET 10 y Win32 `CreateMutexExW`:**
   - En [`WindowsSingleInstanceGuard.cs#L40`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L40) y [`line 49`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L49), el código invoca:
     ```csharp
     _uninstallGate = new Mutex(initiallyOwned: false, _uninstallGateMutexName);
     _mutex = new Mutex(initiallyOwned: false, _mutexName);
     ```
   - Target framework: `net10.0-windows10.0.19041.0`.
   - Según el código fuente del runtime de .NET ([`Mutex.Windows.cs`](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/Mutex.Windows.cs) y [`NamedWaitHandleOptions.cs`](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/NamedWaitHandleOptions.cs)), el constructor de `Mutex(bool, string)` no invoca directamente `CreateMutexW`, sino la API nativa de Win32:
     ```c
     CreateMutexExW(IntPtr.Zero, lpName, 0, (uint)rights);
     ```
   - No se pasa un descriptor de seguridad personalizado (`lpMutexAttributes` es `IntPtr.Zero` / `NULL`).
   - Los derechos nativos solicitados por el runtime en la invocación son `MAXIMUM_ALLOWED | SYNCHRONIZE | MUTEX_MODIFY_STATE` (`0x02100001`), y **no estrictamente `MUTEX_ALL_ACCESS`** (`0x1F0001`).
   - La opción de opciones nombradas `NamedWaitHandleOptions` es `None`, por lo que **no se opta por `CurrentUserOnly` ni `CurrentSessionOnly`**.
2. **Namespace Global vs. Permisos DACL:**
   - Las constantes de producción son `Global\PrivLock_SingleInstance` y `Global\PrivLock_UninstallGate`.
   - El prefijo `Global\` sitúa los objetos nombrados en el namespace global del gestor de objetos de Windows (`\BaseNamedObjects`), haciéndolos visibles a través de todas las sesiones interactivas (Terminal Services / Fast User Switching).
   - **Visibilidad en el namespace no equivale a autorización en la DACL:** Que el identificador del objeto sea visible entre sesiones no otorga por sí mismo derechos para abrir el handle ni adquirir la sincronización.
   - Al no especificarse `lpMutexAttributes`, el kernel asigna al objeto creado un descriptor de seguridad predeterminado derivado del token del creador (`UserA`).
   - Cuando un segundo usuario (`UserB`) en otra sesión interactiva intenta instanciar `new Mutex(...)` con el mismo nombre, Windows evalúa el token de `UserB` contra la DACL existente:
     - Si la DACL del token de `UserA` otorga derechos de sincronización/apertura a `UserB` (por ejemplo, mediante la inclusión de `Authenticated Users` o `INTERACTIVE` en la DACL por defecto), la llamada nativa tiene éxito y el runtime obtiene un handle, pudiendo evaluar la contención mediante `WaitOne(0)` (que retorna `false` si `UserA` retiene la propiedad).
     - Si la DACL por defecto de `UserA` niega o no incluye derechos para `UserB`, la invocación de `CreateMutexExW` / apertura falla con Win32 `ERROR_ACCESS_DENIED` (5), mapeado por .NET como `UnauthorizedAccessException`.
   - Por tanto, **el comportamiento efectivo multiusuario no puede deducirse por deducción puramente estática sin conocer la DACL asignada por el token del usuario en la build de Windows del entorno**. Su comprobación experimental en VM es imperativa en TEST-09.2.
3. **Diferenciación Estricta de los Cuatro Resultados de Adquisición:**
   - `WaitOne(0) == true`: El hilo adquirió exitosamente la propiedad del mutex.
   - `WaitOne(0) == false`: El mutex existe, el handle fue abierto válidamente, pero está retenido por otro hilo/proceso (contención normal).
   - `AbandonedMutexException`: El proceso propietario finalizó abruptamente sin liberar el mutex; el hilo receptor adquiere la propiedad y debe manejar la posible inconsistencia de estado previa.
   - `UnauthorizedAccessException`: El proceso carece de derechos en la DACL para abrir el objeto existente; no equivale a contención normal ni mutex ocupado.
   - `WaitHandleCannotBeOpenedException`: Incompatibilidad de tipo o error del subsistema de objetos kernel.
   - En `WindowsSingleInstanceGuard.TryAcquireSingleInstance()`, cualquier excepción es capturada en `catch (Exception ex)` (líneas 61-68), registrándose en el log y retornando `false` (*fail-closed*). La UI superior recibe un fallo genérico de admisión sin distinguir necesariamente contención de denegación de permisos.
4. **Hipótesis de Amenaza: Precreación de Mutex (*Mutex Squatting*):**
   - La documentación oficial de Win32 para `CreateMutexW` / `CreateMutexExW` advierte explícitamente: *"If you are using a named mutex to limit your application to a single instance, a malicious user can create this mutex before you do and prevent your application from starting"*.
   - Dado que el identificador `Global\PrivLock_SingleInstance` es fijo y público, un proceso sin privilegios podría precrear dicho objeto en `\BaseNamedObjects` con una DACL restrictiva que deniegue acceso a los demás usuarios, induciendo un fallo cerrado (*Denial of Service*) en PrivGvard al impedir su arranque.
5. **Disparidad de Ámbitos entre Mutex Global y Evento Local de Desinstalación:**
   - Mientras los mutexes de guarda operan en `Global\`, el listener de desinstalación declara en [`WindowsUninstallRequestListener.cs#L10`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L10):
     ```csharp
     private const string RequestEventName = @"Local\PrivLock_UninstallRequested";
     ```
   - Según [*Kernel Object Namespaces*](https://learn.microsoft.com/en-us/windows/win32/termserv/kernel-object-namespaces), el prefijo `Local\` confina el objeto nombrado al namespace de la sesión Terminal Services del proceso creador (`\Sessions\<SessionId>\BaseNamedObjects`).
   - En escenarios de Fast User Switching (FUS), `UserA` reside en la Sesión 1 y `UserB` en la Sesión 2.
   - Si `UserB` inicia una desinstalación Standalone invocando `TryRequestRunningInstanceAndAcquire()`:
     1. Busca `Local\PrivLock_UninstallRequested` dentro de la Sesión 2; el evento de `UserA` (creado en Sesión 1) **es invisible para la Sesión 2**.
     2. `TrySignalExistingInstance` devuelve `false` (`requestSent = false`).
     3. El proceso de `UserB` comprueba `instanceMutex.WaitOne()`. Si `UserA` retiene `Global\PrivLock_SingleInstance` en el namespace global, la espera falla por contención.
     4. El bucle agota el timeout y retorna el error: *"No running PrivGvard instance accepted the uninstall shutdown request"*.
   - En la misma sesión interactiva, el evento funciona de forma adecuada; sin embargo, no ofrece coordinación entre sesiones distintas.
6. **Matriz de Cobertura de Tests Unitarios Existentes vs. Requerimientos de VM:**

| Test Unitario Existente | Ámbito de Ejecución | Primitivas y Nombres Evaluados | Qué Demuestra Empíricamente | Qué NO Demuestra (Requiere VM) |
|---|---|---|---|---|
| `WindowsSingleInstanceStartupTests` | Mismo proceso OS, misma sesión interactiva, 2 tareas `Task.Run` concurrentes | `Local\..._Test_<GUID>` (aisladas de producción) | Serialización de admisión entre dos hilos en el mismo proceso y sesión. | Procesos OS separados, cuentas de usuario con SIDs distintos, sesiones Terminal Services / FUS, DACL efectiva en `Global\`. |
| `WindowsUninstallRequestListenerTests` | Mismo proceso OS, misma sesión interactiva, 2 tareas `Task.Run` concurrentes | `Local\..._Test_<GUID>` (aisladas de producción) | Notificación y handoff de señalización de evento dentro de una misma sesión. | Visibilidad cruzada de eventos entre sesiones Terminal Services distintas bajo Fast User Switching. |
| `WindowsApplicationActivationListenerTests` | Mismo proceso OS, misma sesión interactiva, 2 tareas `Task.Run` concurrentes | `Local\..._Test_<GUID>` (aisladas de producción) | Activación y transferencia de señal dentro del espacio de la misma sesión interactiva. | Activación cruzada entre sesiones o perfiles de usuario distintos. |

7. **Límites Arquitectónicos Fundamentales:**
   - `Global\PrivLock_SingleInstance` es exclusivamente una **guarda de admisión de instancia/proceso único**.
   - **No es un coordinador transaccional de almacenamiento ni un gestor de recursos**.
   - **No otorga un lease de diario WAL** ni garantiza sincronización entre ediciones (Store y Standalone) si se ejecutan de forma secuencial.
   - **No integra D-03 ni D-05**.
   - Se ratifica que en `src/**/*.cs` **no existe ninguna implementación de `MachineSafetyLatch`, `EndpointClaim` ni leases en `ProgramData`**.

#### Enlaces de Referencia Oficiales de Microsoft y Runtime de .NET:
- [.NET Mutex constructor](https://learn.microsoft.com/en-us/dotnet/api/system.threading.mutex.-ctor?view=net-10.0#system-threading-mutex-ctor(system-boolean-system-string))
- [Win32 CreateMutexExW](https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-createmutexexw)
- [Win32 CreateMutexW](https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-createmutexw)
- [Win32 OpenMutexW](https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-openmutexw)
- [Windows Kernel Object Namespaces](https://learn.microsoft.com/en-us/windows/win32/termserv/kernel-object-namespaces)
- [Synchronization Object Security and Access Rights](https://learn.microsoft.com/en-us/windows/win32/sync/synchronization-object-security-and-access-rights)
- [dotnet/runtime Mutex.Windows.cs](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/Mutex.Windows.cs)
- [dotnet/runtime NamedWaitHandleOptions.cs](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/NamedWaitHandleOptions.cs)

---

## 5. Diagnóstico de Readiness del Entorno y Modelo de Capas

### 5.1 Parámetros Diagnósticos Observados (Solo Lectura)
| Parámetro | Valor Observado | Interpretación Factual Rigurosa |
|---|---|---|
| **OS / Build** | Windows 11 Enterprise Eval (Build 26300, 64-bit) | Sistema operativo de la máquina donde corre el agente. |
| **Manufacturer / Model** | `innotek GmbH` / `VirtualBox` | **Confirma que el entorno actual es un Guest de VirtualBox**. |
| **HypervisorPresent** | `True` | **El sistema invitado detecta un hipervisor expuesto**. No aporta información sobre el hardware ni configuración del host físico L0. |
| **CPU Virtual** | Intel Core i7-7700HQ @ 2.80GHz (4 vCPU) | Procesador virtual asignado a este guest. |
| **VirtualizationFirmwareEnabled** | `False` | **Virtualización anidada no expuesta** a este guest según la consulta. |
| **SLAT Extensions** | `False` | Extensiones de traducción de segundo nivel no expuestas al guest. |
| **Memoria RAM** | 3.98 GB utilizables (~1.15 GB libres) | Memoria asignada a este guest; insuficiente para hospedar otra VM. |
| **Disco C:** | 25.28 GB libres (de 78.90 GB) | Espacio ajustado para discos virtuales anidados. |
| **Herramientas de Virtualización** | `vmms`, `Get-VM`, VirtualBox, VMware, Sandbox ausentes | Ningún hipervisor utilizable dentro del guest. |

### 5.2 Modelo de Topologías y Estimaciones de Recursos

```mermaid
flowchart TD
    subgraph TopologiaA ["Opción A: Nested Virtualization (3 Capas) — Compleja y con Alto Overhead"]
        L0_A["Host Físico del Propietario<br/>(Estimación recomendada: 24–32 GB RAM física y VT-x en BIOS)"]
        L1_A["Guest Actual de VirtualBox (L1)<br/>(Requiere nested VT-x activo y 12–16 GB vRAM asignados)"]
        L2_A["VM de Prueba Windows 11 (L2)<br/>(4–8 GB vRAM asignados dentro de L1)"]
        L0_A -->|VirtualBox| L1_A
        L1_A -->|Hyper-V anidado| L2_A
    end

    subgraph TopologiaB ["Opción B: VM de Laboratorio Directa en Host Físico (2 Capas) — RECOMENDADA"]
        L0_B["Host Físico del Propietario<br/>(Estimación recomendada: ~16 GB RAM física total y SSD con 100 GB libres)"]
        L1_B["VM de Prueba Windows 11 (L1)<br/>(2 vCPU, 4–8 GB vRAM, vTPM 2.0, Secure Boot, Snapshots)"]
        L0_B -->|Hyper-V / VirtualBox / VMware| L1_B
    end
```

- **Requisitos del SO Invitado (Windows 11):** 4 GB vRAM, 2 vCPU, 64 GB almacenamiento, TPM 2.0 y Secure Boot.
- **Estimaciones de Planificación:**
  - *Topología B (Recomendada):* Provisión de VM directa en el host físico del propietario. Se estima una necesidad de ~16 GB de RAM física total en el anfitrión para sostener el SO base y una VM de 4 a 8 GB con fluidez.
  - *Topología A (Tres Capas):* Requiere que el host físico sostenga dos capas de virtualización simultáneas, estimándose entre 24 y 32 GB de RAM física.

---

## 6. Cadena de Custodia Criptográfica del Artefacto MSIX

Para garantizar la trazabilidad absoluta, se mantiene la separación estricta de hashes:

1. **Fase de Compilación (Build Stage — Fuera de la VM):**
   - Compilación unsigned con `scripts/build-msix.ps1 -Configuration Release-Store -ValidateStoreIdentity`.
   - Se registran en `BUILD-PROVENANCE.json`:
     - SHA del commit Git canónico.
     - Versión del .NET SDK y Windows SDK utilizados.
     - **SHA-256 del archivo `.msix` unsigned generado.**
     - **SHA-256 de los ensamblados en staging (`win-x64`).**
     - SHA-256 de `AppxManifest.xml` y `resources.pri`.
2. **Fase de Transferencia y Test-Signing (Exclusivamente Dentro del Guest VM):**
   - Transferencia a la VM (en red privada/aislada).
   - Verificación de correspondencia del SHA-256 unsigned recibido contra la proveniencia de build.
   - Firma con `SignTool.exe` en el snapshot `SNAP-02-TEST-CERT` usando certificado autofirmado confinado a la VM.
   - **Registro del nuevo SHA-256 del archivo `.msix` firmado resultante.**
   - Registro del Thumbprint del certificado de prueba y confirmación de que dicho certificado no existe en el host físico.

---

## 7. Actualización del Runbook Dinámico 8B.1A.7.2 (TEST-01 a TEST-11)

> [!IMPORTANT]
> **Estado de Ejecución Dinámica:** Todas las pruebas dinámicas (TEST-01 a TEST-11) permanecen formalmente en estado **`LAB HOLD / NOT RUN`**. No se han ejecutado en el entorno de desarrollo actual debido a la ausencia de virtualización anidada y snapshots en el guest de VirtualBox. Las especificaciones a continuación constituyen el protocolo experimental falsable a ejecutar cuando la VM aislada sea provista en el host físico (Topología B).

### Gate de Seguridad Previo a Pruebas Destructivas (TEST-06 / TEST-07)
- Red en modo aislado y dispositivos físicos (cámaras/micrófonos) desconectados/no compartidos.
- Comprobación previa de capacidad de reversión al snapshot `SNAP-02-TEST-CERT`.
- Registro previo de estado de registro, endpoints y archivos AppData.

### Secuencia de Pruebas Normativas
1. **TEST-01 (Equivalencia y Rutas Físicas Efectivas en AppData) — `LAB HOLD / NOT RUN`:**
   - **Objetivo:** Determinar empíricamente si existen divergencias entre las API del entorno y las rutas normalizadas del perfil, y registrar las ubicaciones físicas donde Store y Standalone operan.
   - **Procedimiento por Usuario (UserA y UserB):**
     1. Registrar la ruta devuelta por `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)`.
     2. Registrar el valor de la variable de entorno `%LOCALAPPDATA%` y `%USERPROFILE%\AppData\Local`.
     3. Registrar el SID del usuario y resolver la ruta `ProfileImagePath` en `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\<SID>`.
     4. Comparar las rutas normalizadas para verificar si ambas formas apuntan al mismo directorio físico o si existen reparse points, junctions o redirecciones de carpeta de usuario (Known Folder Redirection).
     5. Registrar la ruta construida por `GetWindowsActiveMarkerDirectory()` en `Program.cs`.
     6. Registrar las rutas exactas que inspecciona el desinstalador Inno Setup (`ProfileMayHaveActiveSession`).
     7. Ejecutar PrivGvard en ambas ediciones (Store y Standalone) y capturar con Process Monitor (ProcMon) las rutas físicas efectivas donde se crean y modifican:
        - Estado: `%LOCALAPPDATA%\PrivGuard\state.json`
        - Diario WAL: `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json`
        - Backup WAL: `%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json.bak`
        - Marcador activo: `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker`
     8. Inspeccionar `Packages\<PFN>\LocalCache` para documentar la presencia o ausencia de virtualización o carpetas privadas.
2. **TEST-02 (Trazas ProcMon de Operaciones de Archivo) — `LAB HOLD / NOT RUN`:**
   - Capturar con Process Monitor llamadas Win32 `CreateFileW`, `WriteFile`, `ReplaceFileW`/renombrado y `DeleteFileW`.
   - Comparar las rutas físicas observadas entre la ejecución de Store y Standalone.
3. **TEST-03 (Reemplazo Atómico `File.Replace`) — `LAB HOLD / NOT RUN`:**
   - Comprobar el comportamiento y fallbacks de `File.Replace` al persistir el WAL transaccional.
4. **TEST-04 (ConsentStore HKCU en Store Edition) — `LAB HOLD / NOT RUN`:**
   - Confirmar que Store Edition lee el ConsentStore pero devuelve error y no escribe en el registro (D-02).
5. **TEST-05 (Resiliencia ante MSIX App Repair) — `LAB HOLD / NOT RUN`:**
   - Ejecutar `Reparar` desde Configuración de Windows. Registrar por separado la supervivencia de `state.json`, WAL primario/backup, marcador activo y endpoints de audio.
6. **TEST-06 (Impacto de MSIX App Reset) — `LAB HOLD / NOT RUN`:**
   - Comprobación empírica: determinar qué datos elimina realmente la acción `Restablecer` de Windows para este paquete Full Trust; verificar si los archivos en `%LOCALAPPDATA%` fuera del contenedor se conservan y confirmar que ningún reset deje un endpoint silenciado sin camino de recuperación.
7. **TEST-07 (Impacto de MSIX Uninstall) — `LAB HOLD / NOT RUN`:**
   - Desinstalar el paquete MSIX mediante Windows. Verificar por separado la eliminación del paquete, la persistencia de datos en AppData, la capacidad de reinstalar y recuperar, y el estado final de los endpoints de Core Audio.
8. **TEST-08 (Mute de Core Audio y Exclusive Mode) — `LAB HOLD / NOT RUN`:**
   - Probar con aplicación en modo exclusivo (ej. Audacity WASAPI Exclusive) si la captura es bloqueada o si se produce bypass cuando `HardwareMuteSupport` es `NotSupported`.
9. **TEST-09 (Fast User Switching, Perfiles y Comportamiento del Mutex Global) — `LAB HOLD / NOT RUN`:**
   - **Objetivo:** Evaluar experimentalmente el aislamiento multiusuario, la resolución de rutas por perfil, la accesibilidad del mutex global entre distintos usuarios, el alcance del evento local de desinstalación y la seguridad de los endpoints y diarios en escenarios de cambio rápido de usuario (FUS).
   - **Cuatro Estados Físicos del Objeto Evaluados:**
     1. *Mutex Libre (Unowned):* Objeto creado en `\BaseNamedObjects`, pero ningún hilo o proceso retiene actualmente su propiedad.
     2. *Mutex con Propietario Activo:* El proceso de `UserA` en la Sesión 1 mantiene adquirido el mutex de forma activa.
     3. *Mutex Abandonado (`AbandonedMutexException`):* El proceso de `UserA` finalizó abruptamente (`taskkill /F` o crash) sin liberar el mutex.
     4. *Objeto Precreado / Squatting:* Objeto precreado maliciosamente en `\BaseNamedObjects` con una DACL restrictiva previa al arranque de la aplicación.
   - **TEST-09.1 — Identidad y Rutas por Perfil:**
     - En `UserA` y `UserB`, registrar y comparar por separado:
       - SID efectivo y token primario con grupos y privilegios.
       - Variables de entorno `USERPROFILE` y `LOCALAPPDATA`.
       - Valor de `Environment.SpecialFolder.LocalApplicationData`.
       - Session ID de Terminal Services (WTS).
       - Ruta del perfil en `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\<SID>\ProfileImagePath`.
       - Directorio del marcador devuelto por `GetWindowsActiveMarkerDirectory()`.
       - Rutas físicas reales de `state.json`, `privacy-session-v1.json` y `privacy-session-v1.json.bak`.
       - PID, edición y ruta absoluta del ejecutable.
     - Normalizar rutas sin asumir identidad física por igualdad textual; verificar mediante ProcMon y resolución de junctions/reparse points si `%LOCALAPPDATA%` resuelve en directorios independientes para cada perfil.
   - **TEST-09.2 — Mutex Global entre Sesiones y Permisos DACL:**
     - En la VM de laboratorio bajo snapshot:
       1. Crear dos cuentas locales estándar de prueba: `UserA` y `UserB`.
       2. Iniciar PrivGvard como `UserA`. Documentar SID, Session ID, PID y edición.
       3. Confirmar que `UserA` mantiene adquirido el mutex global (`Global\PrivLock_SingleInstance`).
       4. Cambiar a `UserB` mediante Fast User Switching (sin cerrar sesión de `UserA`).
       5. Intentar iniciar PrivGvard como `UserB`.
       6. Registrar de forma diferenciada:
          - Intento de llamada nativa `CreateMutexExW` / apertura de `Global\PrivLock_SingleInstance`.
          - Confirmación de si el objeto ya existía (`ERROR_ALREADY_EXISTS`).
          - Descriptor de seguridad (DACL) asignado al objeto en `\BaseNamedObjects` (inspeccionado con Sysinternals WinObj, Process Explorer o AccessChk), registrando ACEs, SIDs y derechos concedidos.
          - Derechos evaluados para el token de `UserB`.
          - Resultado de `WaitOne(0)`: registrar si retorna `false` (contención normal), arroja `UnauthorizedAccessException` (denegación de permisos en DACL), o `AbandonedMutexException`.
          - Resultado de admisión en el ciclo de vida de PrivGvard.
          - Mensaje mostrado en UI y entrada registrada en logs de `%LOCALAPPDATA%\PrivGvard\Logs\` para `UserB`.
       7. Evaluar el estado de *Mutex Libre (Unowned)* cerrando limpiamente PrivGvard en `UserA` y comprobando si `UserB` puede abrir y adquirir el handle sin restricciones.
       8. Restaurar el snapshot de la VM al finalizar.
   - **TEST-09.3 — Evento de Handoff `Local\` en FUS:**
     - 1. Iniciar el listener de desinstalación de `UserA` en su sesión interactiva (Sesión 1).
     - 2. Cambiar a `UserB` mediante FUS (Sesión 2).
     - 3. Intentar abrir/señalizar `Local\PrivLock_UninstallRequested` desde el proceso de `UserB`.
     - 4. Registrar si `UserB` encuentra el evento en su propia sesión (Sesión 2), si puede señalizarlo y si el listener de `UserA` en Sesión 1 recibe alguna notificación (verificación de aislamiento de namespace `\Sessions\<Id>\BaseNamedObjects`).
     - 5. Repetir el procedimiento dentro de la misma sesión (UserA señalizando a UserA en Sesión 1) para establecer la línea base de comparación donde el handoff opera nominalmente.
     - 6. Registrar el resultado de `TrySignalExistingInstance()` y la resolución del handoff en `TryRequestRunningInstanceAndAcquire()`.
     - 7. Distinguir formalmente entre:
       - *Evento no encontrado por namespace de sesión* (`WaitHandleCannotBeOpenedException` / `FileNotFoundException`).
       - *Acceso denegado por DACL* (`UnauthorizedAccessException`).
       - *Señal entregada exitosamente*.
       - *Timeout y terminación del proceso solicitante*.
     - 8. No ejecutar desinstalación real destructiva durante esta comprobación del protocolo.
   - **TEST-09.4 — Resistencia a Precreación (*Mutex Squatting*):**
     - Evaluar la hipótesis de amenaza documentada en la doctrina oficial de Win32 para `CreateMutexW` / `CreateMutexExW`:
       1. En la VM bajo snapshot, un proceso no privilegiado precrea `Global\PrivLock_SingleInstance` con una DACL restrictiva que niega acceso a otros usuarios.
       2. Intentar iniciar PrivGvard desde una cuenta estándar legítima.
       3. Observar el error exacto arrojado (`UnauthorizedAccessException`), la captura fail-closed en el bloque `catch (Exception ex)` de `WindowsSingleInstanceGuard` y el bloqueo subsiguiente del inicio de PrivGvard (*Denial of Service*).
       4. Documentar el hallazgo para posterior evaluación arquitectónica (sin introducir mitigaciones no autorizadas en esta fase).
       5. Restaurar snapshot.
   - **TEST-09.5 — Mutex Abandonado y Recuperación de Ownership:**
     - En la VM aislada bajo snapshot:
       1. Iniciar PrivGvard en `UserA` y forzar terminación abrupta mediante `taskkill /F /PID <pid>`.
       2. Cambiar a `UserB` e iniciar PrivGvard.
       3. Capturar la captura de `AbandonedMutexException` en el runtime de .NET, verificando que el hilo receptor asume la propiedad del mutex.
       4. Verificar si el proceso subsiguiente procesa la recuperación del WAL o activa mecanismos de quiescencia sin corrupción de archivos de estado.
       5. Comprobar que no se produzcan bloqueos huérfanos ni pérdidas de consistencia.
       6. Probar exclusivamente sobre dispositivos virtuales y snapshots; nunca sobre hardware físico.
   - **TEST-09.6 — Impacto Funcional, Fallback Fail-Closed y Observabilidad en Logs:**
     - Comprobar que ante cualquier excepción no manejada (`UnauthorizedAccessException`, excepciones de subsistema kernel), `WindowsSingleInstanceGuard.TryAcquireSingleInstance()` registra la entrada correspondiente con nivel de error o advertencia en los logs de Serilog (`%LOCALAPPDATA%\PrivGvard\Logs\`), retornando `false` y garantizando que la aplicación nunca continúe ejecutándose en un estado de concurrencia indeterminada.
10. **TEST-10 (Coexistencia con Edición Standalone y Jurisdicción de Desinstaladores) — `LAB HOLD / NOT RUN`:**
    - **10.1 Concurrencia Store/Standalone en Misma Sesión:** Comprobar que `Global\PrivLock_SingleInstance` impide la ejecución simultánea de ambas ediciones dentro de la misma sesión interactiva, capturando el motivo exacto de exclusión y la respuesta de la UI.
    - **10.2 Concurrencia entre Diferentes Sesiones (FUS):** Contrastar la exclusión mutua de ambas ediciones cuando se ejecutan desde sesiones Terminal Services distintas, evaluando la interacción entre la DACL del mutex global y las identidades de cada usuario.
    - **10.3 Ejecución Secuencial y Rutas Físicas AppData:** Ejecutar ambas ediciones de forma alternada (cerrando una antes de iniciar la otra) y observar mediante ProcMon si acceden a las mismas rutas físicas en AppData, documentando formalmente cualquier colisión, compartición o sobreescritura en `state.json` o en el diario WAL (`privacy-session-v1.json`).
    - **10.4 Handoff de Desinstalación:** Comprobar el comportamiento del uninstall gate y el alcance del evento `Local\` cuando se invoca el desinstalador Standalone con una instancia Store activa en segundo plano, sin ejecutar acciones destructivas fuera de snapshot.
    - **10.5 Jurisdicción de Inno Setup frente a MSIX:**
      - Verificar que la función `ProfileMayHaveActiveSession` en `setup.iss` detecta el marcador `active-session-v1.marker` en `PrivGvard\Recovery` y el diario en `PrivGuard\Recovery`, bloqueando la desinstalación (*fail-closed*).
      - Ratificar y demostrar en laboratorio que la protección de `setup.iss` es exclusiva de Inno Setup y **carece de autoridad sobre el ciclo de vida del paquete MSIX**, el cual puede ser desinstalado por Windows (vía Configuración de Windows o `Remove-AppxPackage`) sin consultar a Inno Setup.
11. **TEST-11 (Recuperación tras Caída Simulada) — `LAB HOLD / NOT RUN`:**
    - Forzar terminación del proceso (`taskkill /F`) con marcador activo presente; reiniciar la aplicación y verificar la recuperación automática del WAL sin intervención manual.

---

## 8. Preservación Estricta de Decisiones Arquitectónicas y Salvaguardas

- **Decisión D-01:** **Aprobada**. Manifiesto Store contiene exclusivamente `runFullTrust`. Prohibidas `allowElevation` y `unvirtualizedResources`.
- **Decisión D-02:** **Aprobada con condiciones**. Escritura en `ConsentStore` prohibida en Store Edition. Lectura permitida. Redirección informativa a `ms-settings:privacy-webcam`.
- **Decisión D-03:** **Aprobación parcial**. Aislamiento conceptual de almacenamiento acordado; integración efectiva de rutas en DI y coordinación en tiempo de ejecución pendientes de prueba en VM.
- **Decisión D-04:** **Pendiente** de formalización.
- **Decisión D-05:** **Pendiente** de formalización e integración en los stores reales.
- **Microsoft Store:** **`CONDITIONAL HOLD`** (publicación bloqueada).
- **Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`** (desarrollo prohibido).
- **Fase 8B.1A.7.2:** **Rebaseline completado — `LAB HOLD`** (bloqueada por entorno de laboratorio).

---

## 9. Próximos Pasos Recomendados

1. **Consolidación Local en Rama de Reconciliación:** Mantener la rama local `docs/8b1a7-2-final-evidence-reconciliation` con los cambios documentales y de auditoría estática reconciliados, en estricto cumplimiento de la directiva de no realizar push remoto sin autorización previa.
2. **Preparación para Pull Request:** Mantener la rama lista para apertura formal de Pull Request cuando el propietario determine oportuna su publicación hacia `origin/main`.
3. **Acción del Propietario en el Host Físico (L0):** Aprovisionar una máquina virtual Windows 11 independiente en el host físico según las especificaciones de la Topología B (sección 5.2).
4. **Generación de Artefactos desde HEAD Vigente:** Tras disponer de la VM, compilar el paquete `Release-Store` desde el HEAD canónico actual y registrar la doble proveniencia criptográfica (unsigned y signed).
5. **Ejecución del Runbook 8B.1A.7.2:** Llevar a cabo los tests normativos TEST-01 a TEST-11 en la VM aislada con capturas de ProcMon.

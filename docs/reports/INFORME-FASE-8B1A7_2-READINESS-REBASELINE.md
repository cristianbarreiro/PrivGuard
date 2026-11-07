# PrivGuard — Fase 8B.1A.7.2-R1: Rebaseline de Supuestos, Readiness del Laboratorio VM y Reconciliación Post-Push (Revisión Factual y Corrección Estática R1)

**Fecha de emisión original:** 2026-11-07<br />
**Fecha de revisión factual R1:** 2026-11-07<br />
**Autor:** Principal .NET / Windows Security Engineer & Transactional Recovery Auditor<br />
**Rama activa de trabajo:** `docs/8b1a7-2-post-push-r1`<br />
**Baseline de código auditado:** `653be31040da53bea72a540ddb2a111f16b35d50`<br />
**HEAD documental remoto (`origin/main`):** `11a7e497a212000809084d56cb3c596ccd3c6f43`<br />
**Parent directo en `main`:** `fc345ebd06239e8b86c0c502a4b289052e988a8c`<br />
**Parent funcional anterior (Baseline auditado):** `653be31040da53bea72a540ddb2a111f16b35d50`<br />
**Último workflow CI:** Run #96 (ID: `42426468347`, commit `11a7e49`, evento `push`, **7/7 jobs `success`**)<br />
**Workflows CI previos relevantes:**<br />
- Run #95 (ID: `42420318570`, commit `fc345eb`, evento `push`, **7/7 jobs `success`**)<br />
- Run #94 (ID: `42417187681`, commit `aedd19c`, evento `push`, **7/7 jobs `success`**)<br />
- Run #93 (ID: `42410227273`, commit `54fb446`, evento `push`, **7/7 jobs `success`**)<br />
- Run #92 (ID: `42396473407`, commit `653be31`, evento `push`, **7/7 jobs `success`**)<br />
- Runs #87 y #86 (IDs: `42346415046` y `42342850877`, commits `391d79e` y `0a270a1`, evento `pull_request`, `head_branch`: `feature/sprint-1-store-profile-guards-d01-d02`, destino: `main`, PR #11, **7/7 jobs `success`**)<br />
**Veredicto de Rebaseline R1:** **PASS — R1 DOCUMENTATION RECONCILED**<br />
**Readiness del Laboratorio VM:** **HOLD — Requiere aprovisionamiento/acción del propietario en Hipervisor L0**<br />
**Estado Microsoft Store:** **CONDITIONAL HOLD**<br />
**Estado Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**

---

## 1. Resumen Ejecutivo y Veredicto Formal

### 1.1 Resumen Ejecutivo y Addendum R1 de Reconciliación Post-Push
El presente documento constituye la **revisión factual, adversarial y correctiva R1** del rebaseline técnico de la Fase 8B.1A.7.2 (preparación para pruebas dinámicas de almacenamiento MSIX y recuperación transaccional en VM aislada).

Los informes y runbooks previos de virtualización (Fases 8B.1A.5, 8B.1A.6 y 8B.1A.7.1) fueron redactados tomando como base el commit histórico `9f856c0405e4e5b64e784309affad450c38d3a37`. Desde dicho punto, la historia del repositorio avanzó mediante 5 commits en el rango `9f856c0..d8ca7acc` (Sprint 1.0, 1.1 y 1.2) seguidos de **9 commits exclusivamente documentales en `d8ca7acc..11a7e49`**:
- Cuatro commits documentales previos: `dded99c`, `b9d4b8f`, `07e4188` y `653be31` (este último fijando el baseline del código auditado).
- Cinco commits documentales correctivos posteriores: `3f08f12`, `54fb446`, `aedd19c`, `fc345eb` y `11a7e49`.

**Reconciliación Factual Post-Push de `11a7e49` y Trazabilidad de Remoto:**
- **Estado de `main` en Remoto:** La consulta del repositorio remoto `cristianbarreiro/PrivGuard` confirma que la rama `main` apunta al commit `11a7e497a212000809084d56cb3c596ccd3c6f43` (`docs: finalize 8b1a7 evidence reconciliation`), cuyo parent directo es `fc345ebd06239e8b86c0c502a4b289052e988a8c`.
- **Corrección de la Declaración Anterior de Push:** En el informe anterior se consignó la frase *"ZERO PUSH EJECUTADO"* indicando que `11a7e49` era una confirmación estrictamente local. Sin embargo, la verificación del remoto constata que `11a7e49` fue integrado en `origin/main` y desencadenó el workflow Run #96 en GitHub Actions por evento `push`. La declaración anterior queda formalmente identificada como una descripción desfasada por los hechos posteriores; el estado real es que **`11a7e49` se encuentra publicado en `origin/main`**. No se formula ninguna inferencia sobre el cliente o mecanismo que produjo la publicación.
- **Ramas Remotas y Pull Requests:** La consulta de ramas remotas arroja exclusivamente `origin/dev`, `origin/feature/sprint-1-store-profile-guards-d01-d02` y `origin/main`. La rama dedicada `origin/docs/8b1a7-2-final-evidence-reconciliation` no existe en el remoto. No existen Pull Requests abiertas en el repositorio.
- **Evidencia de GitHub Actions en Run #96:**
  - **Run ID:** `42426468347` | **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42426468347](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42426468347)
  - **Commit:** `11a7e497a212000809084d56cb3c596ccd3c6f43` | **Rama:** `main` | **Evento:** `push`
  - **Conclusión:** **7/7 jobs finalizados en `success`** (Build & Test en windows-latest, ubuntu-latest, macos-latest; y Publish Single-File para osx-arm64, win-x64, linux-x64, win-arm64). En Windows se ejecutó con éxito el paso de verificación de empaquetado `Validate Release-Store MSIX Packaging (Windows)`.
  - **Artefactos conservados en CI:** 4 binarios single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicó ningún paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`** (el empaquetado MSIX se ejecuta de forma efímera en `RUNNER_TEMP` para comprobación del manifiesto y se descarta).
- **Gobernanza de `main`:** La API de GitHub confirma `main.protected = false`, `protection.enabled = false` y lista vacía de rulesets. La protección de rama permanece desactivada. Se documenta la recomendación de configurar una política estricta de protección (PR obligatoria, required status checks y prohibición de force-push) sin alterar configuraciones de GitHub en esta tarea.

La comparación Git confirma que los commits posteriores a `653be31` no modificaron código funcional ni de configuración (`0 líneas de código alteradas`). Por tanto, **`653be31` permanece como el baseline del código auditado estáticamente**, mientras que **`11a7e49` representa el HEAD documental remoto canónico**.

**Comportamiento de Eventos en CI (`push` vs. `pull_request`):**
- El trigger `pull_request` fue probado y superado con éxito en los Runs #86 y #87 (IDs `42342850877` y `42346415046`, commits `0a270a1` y `391d79e`) correspondientes a la rama fuente `feature/sprint-1-store-profile-guards-d01-d02` hacia `main` (PR #11).
- Todos los runs subsiguientes (#89 al #96) correspondieron a eventos de tipo `push` directo sobre `main`.
- Los runs históricos bajo PR no acreditan los cambios documentales posteriores, ni el éxito de los runs de `push` sustituye una futura ejecución bajo `pull_request`.

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
   - Conforme a la implementación en el runtime de .NET 10 (`Mutex.Windows.cs`), la llamada nativa se realiza mediante `CreateMutexExW(IntPtr.Zero, name, 0, (uint)rights)` con derechos `MAXIMUM_ALLOWED | SYNCHRONIZE | MUTEX_MODIFY_STATE` (`0x02100001`), y `NamedWaitHandleOptions.None` (sin `CurrentUserOnly` ni `CurrentSessionOnly`).
   - `Global\PrivLock_SingleInstance` y `Global\PrivLock_UninstallGate` utilizan el namespace global de Windows (`\BaseNamedObjects`).
   - La visibilidad global del nombre **no equivale a permisos irrestrictos de acceso**. Sin embargo, la ausencia de una ACL explícita **tampoco significa que otro usuario estándar siempre vaya a ser denegado**.
   - Si `lpMutexAttributes` es `NULL`, el objeto recibe un descriptor de seguridad predeterminado derivado del token del creador (`UserA`). Si la DACL del token de `UserA` no concede derechos a `UserB`, la llamada nativa falla con `ERROR_ACCESS_DENIED` (5), arrojando `UnauthorizedAccessException` en el constructor de .NET **antes de alcanzar `WaitOne`**.
   - Si el handle es abierto con éxito, `WaitOne(0)` distingue entre adquisición (`true`), contención normal (`false`), o adquisición de mutex abandonado (`AbandonedMutexException`).
   - El mutex actúa como una guarda de admisión de instancia única; **no es un coordinador transaccional de recursos, no provee aislamiento ni arbitraje de almacenamiento, no administra leases de diario y no integra D-03/D-05**. Su comportamiento multiusuario efectivo debe ser verificado en TEST-09.
4. **Disparidad de Ámbitos entre Mutex Global y Evento Local de Desinstalación (Análisis Condicional en FUS):**
   - Mientras los mutexes de admisión utilizan el namespace global (`Global\PrivLock_SingleInstance`, `Global\PrivLock_UninstallGate`), el listener de desinstalación declara `private const string RequestEventName = @"Local\PrivLock_UninstallRequested"`.
   - El prefijo `Local\` confina el evento al namespace de la sesión Terminal Services del creador (`\Sessions\<SessionId>\BaseNamedObjects`).
   - En Fast User Switching (FUS), UserA y UserB operan en sesiones Terminal Services distintas. El evento de UserA en la Sesión 1 **no es visible para el proceso de UserB en la Sesión 2** (`TryOpenExisting` retorna `false` con `error = null`).
   - En consecuencia, en un escenario FUS cruzado, `TryRequestRunningInstanceAndAcquire()` no señaliza al listener de UserA (`requestSent = false`), y el resultado depende del estado de retención del mutex global:
     - Si UserA retiene el mutex durante todo el plazo, se agota el timeout y devuelve: `"No running PrivGvard instance accepted the uninstall shutdown request."`
     - Si el mutex queda libre antes de terminar el plazo, UserB podría adquirirlo pero detectará `requestSent == false`, retornando el error de gate retenido sin aceptación.
     - Si la apertura del mutex falla por DACL, `UnauthorizedAccessException` escapa de `TryRequestRunningInstanceAndAcquire` hacia el manejador global de `Program.Main`.
   - En la misma sesión interactiva, el protocolo opera según su diseño original.
5. **Alcance Real de los Tests Unitarios Actuales:**
   - `WindowsSingleInstanceStartupTests.cs` evalúa dos tareas `Task.Run` concurrentes dentro del **mismo proceso OS** y utilizando nombres `Local\..._Test_<GUID>`. Demuestra exclusión entre tareas en un mismo proceso/sesión, pero **no demuestra procesos OS independientes, SIDs distintos, FUS ni DACL de nombres globales**.
   - `WindowsUninstallRequestListenerTests.cs` evalúa la señalización de eventos locales utilizando implementaciones fake (`RequestAwareSingleInstanceGuard`, `NeverAcquireSingleInstanceGuard`); **no evalúa procesos OS separados ni contención real del mutex de producción**.
   - `WindowsApplicationActivationListenerTests.cs` inyecta nombres `Local\` por GUID dentro del mismo proceso, mientras que el código de producción utiliza el nombre global `Global\PrivLock_ActivateExistingInstance`. **No valida la activación cross-session ni el evento global de producción**.
6. **Significado Exacto de `HypervisorPresent = True`:**
   - La consulta WMI/CIM se ejecutó desde dentro de un Guest de VirtualBox (`innotek GmbH / VirtualBox`). La propiedad `HypervisorPresent = True` indica únicamente que el Windows invitado detecta un hipervisor expuesto a él.
   - El estado real de la CPU física, BIOS/UEFI, memoria RAM total y capacidades de virtualización del host físico L0 son **desconocidos desde el interior del guest** y deben ser verificados por el propietario directamente en el host físico.
7. **Topologías de Virtualización y Estimaciones de Recursos:**
   - Se distinguen los requerimientos reales del sistema operativo invitado (Windows 11: 4 GB vRAM, 2 vCPU, 64 GB almacenamiento, TPM 2.0 y Secure Boot) de las **estimaciones y recomendaciones de planificación** para el host.
   - Se mantiene como alternativa recomendada y más simple la provisión de una VM directa en el host físico (Topología B, ~16 GB RAM física total estimada), frente al overhead y complejidad de tres capas (Topología A: L0 físico -> L1 VirtualBox -> L2 VM anidada).
8. **Cadena de Custodia Criptográfica del Artefacto MSIX:**
   - Se formaliza que el SHA-256 del paquete unsigned generado en build y el SHA-256 del paquete firmado tras test-signing dentro de la VM son distintos y deben registrarse de forma diferenciada en la proveniencia del laboratorio.

### 1.2 Veredicto Oficial

$$\mathbf{VEREDICTO:\ REBASELINE\ R1\ PASS\ —\ LAB\ HOLD\ (REQUIRES\ OWNER\ ACTION)}$$

- **Rebaseline de Supuestos y Conciliación R1:** **PASS**. Se reconcilia la presencia remota de `11a7e49` en `origin/main` y el workflow Run #96, se superan afirmaciones históricas desfasadas de push, se precisa la cobertura real de los tests unitarios frente al código de producción, se diferencia la apertura de mutex de su adquisición por `WaitOne`, y se detalla el handoff condicional en FUS.
- **Readiness del Laboratorio VM:** **HOLD**. El entorno local actual es un guest sin virtualización anidada expuesta y recursos insuficientes. Se requiere acción del propietario en el host físico (Topología B).
- **Salvaguardas Invariantes:** Microsoft Store en `CONDITIONAL HOLD`, Fase 8B.1C en `BLOCKED / NOT AUTHORIZED`.

---

## 2. Baseline de Control de Versiones e Integración Factual

### 2.1 Verificación de Git y Remoto
- **Repositorio:** `cristianbarreiro/PrivGuard`
- **Rama activa de trabajo:** `docs/8b1a7-2-post-push-r1`
- **HEAD Remoto Canónico (`origin/main`):** `11a7e497a212000809084d56cb3c596ccd3c6f43`
- **Parent directo en `main`:** `fc345ebd06239e8b86c0c502a4b289052e988a8c`
- **Parent funcional anterior (Baseline auditado):** `653be31040da53bea72a540ddb2a111f16b35d50`
- **Historial `d8ca7acc..11a7e49` (9 commits exclusivamente documentales):**
  1. `dded99c` — Documental: corrección de evidencia Sprint 1.2 postflight.
  2. `b9d4b8f` — Documental: ajuste de afirmaciones Sprint 1.2 postflight.
  3. `07e4188` — Documental: reconciliación Sprint 1.2 con estado remoto.
  4. `653be31` — Documental: rebaseline 8B.1A.7.2 contra código vigente (baseline de auditoría estática).
  5. `3f08f12` — Documental: corrección factual 8B.1A.7.2.
  6. `54fb446` — Documental: análisis Full Trust AppData y desinstalación.
  7. `aedd19c` — Documental: alineación post-push de baseline y análisis preliminar FUS.
  8. `fc345eb` — Documental: corrección de análisis de DACL de mutex y handoff en FUS.
  9. `11a7e49` — Documental: finalización de reconciliación de evidencia 8B.1A.7 (HEAD documental canónico en `origin/main`).
- **Forma Real de Integración:** Los commits sobre `main` fueron integrados directamente mediante push sin Pull Request. La consulta a la API de GitHub (`/branches/main`) confirma `protected: false`, `protection.enabled = false` y lista vacía de rulesets. No existen Pull Requests abiertas actualmente.
- **Estado del Árbol de Trabajo:** Limpio (`nothing to commit, working tree clean`).
- **Código Funcional Modificado:** **0 líneas**. Todos los commits posteriores a `d8ca7ac` son estrictamente documentales.

### 2.2 Evidencia de GitHub Actions (Runs #86, #87 y #89 al #96)
- **Workflow Run más reciente:** Run #96
  - **Run ID:** `42426468347`
  - **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42426468347](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42426468347)
  - **Commit:** `11a7e497a212000809084d56cb3c596ccd3c6f43`
  - **Rama:** `main` | **Evento:** `push`
  - **Estado / Conclusión:** `completed / success`
  - **Jobs:** **7/7 jobs finalizados en `success`** (Build & Test en windows-latest, ubuntu-latest, macos-latest; y Publish Single-File para osx-arm64, win-x64, linux-x64, win-arm64). En Windows se ejecutó con éxito el paso de empaquetado y verificación MSIX Store.
  - **Artefactos publicados:** 4 ejecutables single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicó ningún paquete MSIX descargable ni `BUILD-PROVENANCE.json`**.
- **Workflow Runs previos:**
  - **Run #95:** ID `42420318570`, commit `fc345eb`, evento `push`, conclusión `success` (7/7 jobs `success`, 4 ejecutables single-file).
  - **Run #94:** ID `42417187681`, commit `aedd19c`, evento `push`, conclusión `success` (7/7 jobs `success`, 4 ejecutables single-file).
  - **Run #93:** ID `42410227273`, commit `54fb446`, evento `push`, conclusión `success` (7/7 jobs `success`, 4 ejecutables single-file).
  - **Run #92:** ID `42396473407`, commit `653be31` (baseline de código auditado), evento `push`, conclusión `success` (7/7 jobs `success`).
  - **Runs #87 y #86 (Validación de evento `pull_request`):** IDs `42346415046` y `42342850877`, commits `391d79e` y `0a270a1`, `head_branch`: `feature/sprint-1-store-profile-guards-d01-d02`, destino: `main`, evento `pull_request` (PR #11), ambos con **7/7 jobs en `success`**.

> [!NOTE]
> **Clarificación de Cobertura de Eventos en CI:** El evento `pull_request` **sí fue verificado empíricamente y superado con éxito** en los Runs #86 y #87 durante la tramitación y merge de la PR #11 (`feature/sprint-1-store-profile-guards-d01-d02` hacia `main`). Lo que no ha corrido bajo `pull_request` son los 9 commits documentales posteriores (`d8ca7acc..11a7e49`), los cuales fueron integrados mediante eventos de tipo `push` directo sobre la rama `main` (Runs #89 al #96).

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

### 4.3 Cuantificación de Commits (`9f856c0..11a7e49`)
El historial entre `9f856c0` y el HEAD remoto canónico `11a7e49` se estructura en dos segmentos perfectamente diferenciados:

1. **Rango `9f856c0..d8ca7acc` (5 commits funcionales / CI / merge):**
   - `7066b3b` — **Documental:** Registro de informes de Fase 8B.1A y proveniencia.
   - `0a270a1` — **Funcional / Packaging:** Integración de perfiles Store/Standalone, guardas D-01/D-02 y manifiesto depurado.
   - `391d79e` — **Funcional:** Inmutabilidad de perfil y rechazo fail-closed de mutaciones parciales `Both` en Store.
   - `ffc2273` — **Merge:** Fusión de Pull Request #11 (`feature/sprint-1-store-profile-guards-d01-d02`).
   - `d8ca7ac` — **CI / Packaging:** Incorporación del gate de validación Release-Store en `.github/workflows/ci.yml`.

2. **Rango `d8ca7acc..11a7e49` (9 commits exclusivamente documentales):**
   - `dded99c` — **Documental:** Corrección de evidencia de postflight Sprint 1.2.
   - `b9d4b8f` — **Documental:** Ajuste de afirmaciones de postflight Sprint 1.2.
   - `07e4188` — **Documental:** Reconciliación de postflight Sprint 1.2 con estado remoto.
   - `653be31` — **Documental:** Rebaseline 8B.1A.7.2 contra código vigente (baseline de auditoría estática).
   - `3f08f12` — **Documental:** Corrección factual de rebaseline 8B.1A.7.2.
   - `54fb446` — **Documental:** Corrección de análisis Full Trust AppData y rutas de desinstalación.
   - `aedd19c` — **Documental:** Alineación post-push de baseline y análisis preliminar FUS.
   - `fc345eb` — **Documental:** Corrección de análisis de DACL de mutex y handoff en FUS.
   - `11a7e49` — **Documental:** Finalización de reconciliación de evidencia 8B.1A.7 (HEAD documental canónico en `origin/main`).

### 4.4 Mutex Global, DACL, Evento `Local\` y Fast User Switching (FUS)
La auditoría estática adversarial sobre los objetos de sincronización y control de instancia en Windows (`net10.0-windows10.0.19041.0`) establece las siguientes precisiones técnicas rigurosas:

1. **Implementación de .NET 10 y Win32 `CreateMutexExW`:**
   - En [`WindowsSingleInstanceGuard.cs#L40`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L40) y [`line 49`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L49), el código invoca:
     ```csharp
     _uninstallGate = new Mutex(initiallyOwned: false, _uninstallGateMutexName);
     _mutex = new Mutex(initiallyOwned: false, _mutexName);
     ```
   - Target framework: `net10.0-windows10.0.19041.0`.
   - Según el código fuente del runtime de .NET 10 ([`Mutex.Windows.cs` (tag v10.0.0)](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Threading/Mutex.Windows.cs) y [`NamedWaitHandleOptions.cs`](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Threading/NamedWaitHandleOptions.cs)), el constructor de `Mutex(bool, string)` no invoca directamente `CreateMutexW`, sino la API nativa de Win32:
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
     - Si la DACL por defecto de `UserA` niega o no incluye derechos para `UserB`, la invocación de `CreateMutexExW` / apertura falla con Win32 `ERROR_ACCESS_DENIED` (5), arrojando `UnauthorizedAccessException` en el constructor de .NET.
   - Por tanto, **el comportamiento efectivo multiusuario no puede deducirse por deducción puramente estática sin conocer la DACL asignada por el token del usuario en la build de Windows del entorno**. Su comprobación experimental en VM es imperativa en TEST-09.2.
3. **Separación Estricta de las Etapas de Adquisición de Mutex:**
   - **Etapa 1: Apertura / Creación del Handle Nativo:**
     - El runtime intenta abrir o crear el mutex nombrado invocando `CreateMutexExW`.
     - Si la llamada es denegada por la DACL del objeto existente, Win32 retorna `ERROR_ACCESS_DENIED` (5) y el constructor de .NET arroja `UnauthorizedAccessException`. **Este fallo ocurre antes de cualquier invocación a `WaitOne`**.
     - Si existe incompatibilidad de tipo con otro objeto kernel del mismo nombre, se produce `WaitHandleCannotBeOpenedException`.
   - **Etapa 2: Evaluación de Sincronización mediante `WaitOne(0)` (sobre Handle Válido):**
     - `WaitOne(0) == true`: El hilo adquirió exitosamente la propiedad del mutex (no había contención o el mutex estaba libre).
     - `WaitOne(0) == false`: El handle es válido y el mutex existe, pero está retenido por otro hilo/proceso (contención normal de instancia única).
     - `AbandonedMutexException`: El proceso propietario anterior finalizó abruptamente sin liberar el mutex; el hilo receptor adquiere la propiedad del objeto pero debe tratar el estado como potencialmente inconsistente.
4. **Flujo de Ejecución en Fast User Switching (FUS) y Caminos del Handoff:**
   - En [`WindowsUninstallRequestListener.cs#L10`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L10), el evento de solicitud declara:
     ```csharp
     private const string RequestEventName = @"Local\PrivLock_UninstallRequested";
     ```
   - El prefijo `Local\` confina el evento al namespace de la sesión Terminal Services del proceso creador (`\Sessions\<SessionId>\BaseNamedObjects`).
   - En Fast User Switching (FUS), `UserA` reside en la Sesión 1 y `UserB` en la Sesión 2.
   - Si `UserB` inicia una desinstalación Standalone invocando `TryRequestRunningInstanceAndAcquire()`:
     1. `TrySignalExistingInstance` ejecuta `EventWaitHandle.TryOpenExisting(@"Local\PrivLock_UninstallRequested", out _)`.
     2. Dado que el evento de `UserA` existe en Sesión 1 y es invisible para la Sesión 2, `TryOpenExisting` devuelve `false`, y `error` permanece en `null` (no se produce una excepción inmediata).
     3. A continuación, el método crea `using var instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName);`.
     4. Se evalúa el bucle de espera sobre `instanceMutex.WaitOne(waitInterval)`.
     5. **Caminos de Resolución Posibles (Análisis Condicional):**
        - *Camino A (Retención Continua):* Si `UserA` retiene `Global\PrivLock_SingleInstance` durante todo el plazo, `instanceMutex.WaitOne()` no adquiere y, al expirar el timeout, la función retorna `false` con el mensaje: *"No running PrivGvard instance accepted the uninstall shutdown request."*
        - *Camino B (Liberación Prematura):* Si `UserA` finaliza limpiamente antes de que expire el timeout de `UserB`, el mutex global queda libre; `UserB` adquiere el mutex pero detecta `requestSent == false`, retornando el error: *"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."*
        - *Camino C (Denegación por DACL):* Si la DACL de `Global\PrivLock_SingleInstance` niega derechos a `UserB`, la línea `new Mutex(...)` arroja `UnauthorizedAccessException` al intentar abrir el objeto.
5. **Propagación de Excepciones hacia `Program.Main`:**
   - En `WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire`:
     - La línea `using var instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName);` (línea 106) **no está envuelta por un bloque `try-catch` local**.
     - Si dicha llamada arroja `UnauthorizedAccessException`, la excepción escapa de `TryRequestRunningInstanceAndAcquire` y se propaga directamente al bloque `catch (Exception ex)` exterior en `Program.Main` (líneas 476–495).
     - El manejador de `Program.Main`:
       1. Genera un volcado JSON mediante `CrashReporter.GenerateCrashReport(ex, "Program.Main", ...)`.
       2. Registra el fallo fatal con Serilog.
       3. Al detectar `safeUninstallPlan != null`, presenta retroalimentación de error al usuario mediante `SafeUninstallLauncher.ShowErrorFeedback("PrivGvard could not safely prepare for uninstall. No files were removed; review the application log and try again.", safeUninstallPlan.Quiet)`.
       4. Finaliza el proceso con código de salida `1`.
   - En contraste, en `WindowsSingleInstanceGuard.TryAcquireSingleInstance()`:
     - La instanciación y adquisición del mutex sí están capturadas en `catch (Exception ex)` (líneas 61-68), registrándose en el log y retornando `false` (*fail-closed* sin escapar al manejador global).
6. **Hipótesis de Amenaza: Precreación de Mutex (*Mutex Squatting*):**
   - La documentación oficial de Win32 para `CreateMutexExW` advierte explícitamente: *"If you are using a named mutex to limit your application to a single instance, a malicious user can create this mutex before you do and prevent your application from starting"*.
   - Si un proceso no privilegiado precrea `Global\PrivLock_SingleInstance` en `\BaseNamedObjects` con una DACL restrictiva:
     - En el inicio normal de la aplicación, `WindowsSingleInstanceGuard` captura la denegación en su `catch` y retorna `false`, impidiendo el arranque (*Denial of Service* fail-closed).
     - En el flujo de safe uninstall, `TryRequestRunningInstanceAndAcquire` arroja la excepción hacia `Program.Main`, produciendo un crash report y feedback seguro sin alterar el estado del sistema.
7. **Precisión sobre los Tests Unitarios Actuales:**
   - **`WindowsUninstallRequestListenerTests`:**
     - Escenario 1 (`RunningInstanceReceivesSafeShutdownRequestAndReleasesSingleInstanceAdmission`): Utiliza nombres con GUIDs locales aislados y una implementación fake `RequestAwareSingleInstanceGuard` que retorna `true` al observar la señal. Demuestra la entrega de la señal al listener en el mismo proceso; **no evalúa una segunda instancia OS ni la liberación del mutex real de producción**.
     - Escenario 2 (`BusyUninstallGateWithoutRunningInstanceFailsExplicitly`): Utiliza `NeverAcquireSingleInstanceGuard` (fake). El mutex nombrado de test no está retenido por otro proceso; **no valida contención real del gate de desinstalación**.
     - Escenario 3 (`RunningInstanceThatDoesNotReleaseAdmissionTimesOutExplicitly`): Inicia un listener local y usa `NeverAcquireSingleInstanceGuard` (fake), simulando la expiración del timeout dentro del mismo proceso sin procesos externos.
   - **`WindowsSingleInstanceStartupTests`:**
     - Ejecuta dos tareas concurrentes `Task.Run` dentro del **mismo proceso OS** con identificadores `Local\..._Test_<GUID>`.
     - Demuestra la exclusión entre hilos/tareas compartiendo espacio de memoria; **no prueba procesos OS separados, cuentas con SIDs distintos, sesiones FUS ni la DACL de los nombres globales de producción**.
   - **`WindowsApplicationActivationListenerTests`:**
     - En código de producción, el evento de activación es `Global\PrivLock_ActivateExistingInstance`.
     - Los tests unitarios inyectan nombres `$@"Local\PrivGvard_ActivateExistingInstance_{Guid.NewGuid():N}"` mediante el constructor interno y ejecutan dos listeners dentro del mismo proceso.
     - Demuestran la señalización local entre handles en el escenario de prueba; **no acreditan el evento global de producción ni la activación entre sesiones o usuarios distintos**.
8. **Matriz de Cobertura de Tests Unitarios vs. Validación Requerida en VM:**

| Área Evaluada | Cobertura Automatizada Demostrada | Pendiente de Validación en VM de Laboratorio |
|---|---|---|
| **Startup Single-Instance** | Exclusión entre 2 tareas `Task.Run` en 1 proceso con mutexes `Local` de test | Concurrencia entre 2 procesos OS independientes, 2 SIDs distintos y sesiones FUS |
| **Listener de Desinstalación** | Señalización de evento local intra-proceso con guard fake; simulación de timeouts | Handoff real entre procesos OS distintos y entre sesiones Terminal Services en FUS |
| **Uninstall Gate** | Comprobación de fallback cuando el guard fake devuelve `false` | Retención y contención real del mutex de producción entre procesos independientes |
| **Listener de Activación** | 2 listeners dentro del mismo proceso con evento `Local` inyectado por GUID | Evento `Global\PrivLock_ActivateExistingInstance` y activación cross-session en el SO |
| **DACL / Mutex Global** | No evaluada por los tests unitarios (usan nombres herméticos `Local`) | Descriptor de seguridad, ACEs, SIDs y permisos efectivos en `\BaseNamedObjects` |
| **Abandono de Mutex** | Captura simulada en código sin terminación abrupta real de procesos | Muerte forzada de proceso (`taskkill /F`) y recuperación real de estado y diario WAL |
| **Identidad y Perfiles** | Detección unitaria con paquetes mock / identidad fija | Dos usuarios estándar reales con SIDs y rutas normalizadas en `%LOCALAPPDATA%` |
| **Ciclo de Vida MSIX** | Verificación estática de manifiesto en runner CI (`runFullTrust`) | Instalación real, reparación, reset, upgrade y desinstalación en Windows 11 |

9. **Límites Arquitectónicos Fundamentales:**
   - `Global\PrivLock_SingleInstance` es exclusivamente una **guarda de admisión de proceso único**.
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
- [dotnet/runtime Mutex.Windows.cs (v10.0.0)](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Threading/Mutex.Windows.cs)
- [dotnet/runtime NamedWaitHandleOptions.cs (v10.0.0)](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Threading/NamedWaitHandleOptions.cs)

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
       6. Registrar de forma rigurosamente separada las dos etapas de adquisición:
          - *Etapa 1 (Apertura/Creación de Handle Nativo):* Comprobar si `CreateMutexExW` tiene éxito o si la DACL asignada por el token de `UserA` deniega el acceso a `UserB`, arrojando `UnauthorizedAccessException` en el constructor de .NET **antes de alcanzar `WaitOne`**. Documentar el descriptor de seguridad del objeto en `\BaseNamedObjects` (inspeccionado con WinObj, Process Explorer o AccessChk), registrando SIDs, ACEs y permisos concedidos.
          - *Etapa 2 (Evaluación de Sincronización en `WaitOne(0)` si el handle fue abierto):*
            - `WaitOne(0) == false`: Contención normal (el objeto existe y está retenido activamente por `UserA`).
            - `WaitOne(0) == true`: Propiedad adquirida por `UserB`.
            - `AbandonedMutexException`: `UserA` finalizó abruptamente sin liberar el mutex; `UserB` adquiere propiedad.
          - Registrar el resultado de admisión en el ciclo de vida de PrivGvard, el mensaje mostrado en UI y las entradas registradas en logs de `%LOCALAPPDATA%\PrivGvard\Logs\` para `UserB`.
       7. Evaluar el estado de *Mutex Libre (Unowned)* cerrando limpiamente PrivGvard en `UserA` y comprobando si `UserB` puede abrir el handle y adquirir la propiedad sin restricciones.
       8. Restaurar el snapshot de la VM al finalizar.
   - **TEST-09.3 — Evento de Handoff `Local\` en FUS (Análisis Condicional):**
     - 1. Iniciar el listener de desinstalación de `UserA` en su sesión interactiva (Sesión 1).
     - 2. Cambiar a `UserB` mediante FUS (Sesión 2).
     - 3. Desde el proceso de `UserB`, invocar `TryRequestRunningInstanceAndAcquire()`:
        - Observar que `TrySignalExistingInstance` ejecuta `EventWaitHandle.TryOpenExisting(@"Local\PrivLock_UninstallRequested", out _)`.
        - Al pertenecer el evento a la Sesión 1 de `UserA`, `TryOpenExisting` retorna `false` en la Sesión 2 de `UserB`, con `error = null` (el evento local simplemente no existe en el namespace de la sesión actual).
        - Observar la creación de `instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName)` y la entrada al bucle de espera sobre `instanceMutex.WaitOne(waitInterval)`.
     - 4. Evaluar los caminos condicionales de handoff:
        - *Camino Condicional 1 (Retención Activa de UserA):* Si `UserA` retiene `Global\PrivLock_SingleInstance` durante todo el tiempo, `instanceMutex.WaitOne()` no adquiere; al agotarse el plazo, el método retorna `false` con: *"No running PrivGvard instance accepted the uninstall shutdown request."*
        - *Camino Condicional 2 (Liberación Prematura de UserA):* Si `UserA` finaliza limpiamente antes de que expire el timeout de `UserB`, `UserB` adquiere el mutex pero detecta `requestSent == false`, retornando: *"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."*
        - *Camino Condicional 3 (Denegación por DACL en Mutex):* Si la apertura de `Global\PrivLock_SingleInstance` falla por permisos DACL, la invocación de `new Mutex(...)` arroja `UnauthorizedAccessException`, la cual no está envuelta por try-catch local y escapa hacia el manejador global de `Program.Main`.
     - 5. Repetir el procedimiento dentro de la misma sesión interactiva (Sesión 1) para establecer la línea base donde el evento `Local\` sí es visible y el handoff opera nominalmente.
     - 6. No ejecutar desinstalación real destructiva fuera de snapshot durante esta comprobación.
   - **TEST-09.4 — Resistencia a Precreación (*Mutex Squatting*):**
     - Evaluar la hipótesis de amenaza documentada en la doctrina oficial de Win32 para `CreateMutexExW`:
       1. En la VM bajo snapshot, un proceso no privilegiado precrea `Global\PrivLock_SingleInstance` con una DACL restrictiva que niega acceso a otros usuarios.
       2. Probar el comportamiento en las dos rutas de ejecución diferenciadas del código:
          - *Ruta A (Inicio Normal de PrivGvard):* El constructor de `WindowsSingleInstanceGuard` intenta abrir el mutex; su bloque `catch (Exception ex)` captura la denegación de permisos, registra el error en Serilog y retorna `false`, impidiendo el inicio de PrivGvard (*Denial of Service* fail-closed).
          - *Ruta B (Flujo de Safe Uninstall):* `TryRequestRunningInstanceAndAcquire` intenta instanciar `new Mutex(...)` fuera de try-catch; la excepción `UnauthorizedAccessException` se propaga al bloque `catch (Exception ex)` de `Program.Main`, el cual genera un crash report con `CrashReporter.GenerateCrashReport`, registra el error fatal, muestra el diálogo de error al usuario (`SafeUninstallLauncher.ShowErrorFeedback`) y finaliza con código de salida 1.
       3. Registrar con precisión los puntos de llamada, excepciones arrojadas y mensajes de log en ambas rutas sin asumir una ruta fija previa a la observación.
       4. Restaurar snapshot.
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
- **Fase 8B.1A.7.2 / R1:** **Rebaseline completado — `LAB HOLD`** (bloqueada por entorno de laboratorio).

---

## 9. Próximos Pasos Recomendados

1. **Gobernanza del Repositorio en GitHub (Recomendación Documental):**
   - Se recomienda formalmente configurar la protección de la rama `main` en GitHub Settings (Branch Protection Rules o Repository Rulesets) estableciendo:
     - Requerimiento obligatorio de Pull Request antes de fusionar en `main`.
     - Required status checks asociados a los jobs esenciales del workflow de CI (`CI / build-test (windows-latest)` y comprobación de packaging MSIX Store).
     - Bloqueo estricto de `force-push` y de eliminación de la rama `main`.
   - *Nota de auditoría:* Esta recomendación se formula con fines de planificación y no fue ejecutada en esta fase para respetar la frontera documental y de no alteración de settings.
2. **Consolidación Local en Rama R1:** Mantener la rama local `docs/8b1a7-2-post-push-r1` con los cambios documentales y de auditoría estática reconciliados, en estricto cumplimiento de la directiva de no realizar push remoto ni abrir Pull Requests sin autorización previa.
3. **Acción del Propietario en el Host Físico (L0):** Aprovisionar una máquina virtual Windows 11 independiente en el host físico según las especificaciones de la Topología B (sección 5.2).
4. **Generación de Artefactos desde HEAD Vigente:** Tras disponer de la VM, compilar el paquete `Release-Store` desde el HEAD canónico actual y registrar la doble proveniencia criptográfica (unsigned en build stage y signed tras test-signing dentro de la VM).
5. **Ejecución del Runbook 8B.1A.7.2:** Llevar a cabo los tests normativos TEST-01 a TEST-11 en la VM aislada con capturas de ProcMon.

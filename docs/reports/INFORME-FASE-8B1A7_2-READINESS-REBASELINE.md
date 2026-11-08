# PrivGuard â€” Fase 8B.1A.7.2-R7: Rebaseline de Supuestos, Readiness del Laboratorio VM, Carrera de Orden de LiberaciÃ³n de Mutexes y Endurecimiento de Reintento FUS (ReconciliaciÃ³n Post-Push #102, VerificaciÃ³n CI R6, AuditorÃ­a de Deadline R5 y Gate L0)

**Fecha de emisiÃ³n original:** 2026-11-07<br />
**Fecha de revisiÃ³n factual R1:** 2026-11-07<br />
**Fecha de revisiÃ³n factual R2:** 2026-11-07<br />
**Fecha de revisiÃ³n factual R3:** 2026-11-07<br />
**Fecha de revisiÃ³n factual R4:** 2026-11-07<br />
**Fecha de revisiÃ³n factual R5:** 2026-11-07<br />
**Fecha de reconciliaciÃ³n post-push R6:** 2026-11-07<br />
**Fecha de reconciliaciÃ³n post-push y gate L0 R7:** 2026-11-08<br />
**Autor:** Principal .NET / Windows Security Engineer, Auditor de SincronizaciÃ³n Interproceso y Responsable de Calidad de Evidencia<br />
**Rama activa de trabajo:** `docs/8b1a7-2-r7-post-r6-reconciliation`<br />
**Baseline de cÃ³digo funcional auditado:** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (Commit R5)<br />
**HEAD remoto canÃ³nico (`origin/main`):** `e6a57ae58d7d0cd30265834fc51e2bab32928732` (Commit R6 Documental)<br />
**Parent directo en `main` (Commit R5):** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`<br />
**Ãšltimo workflow CI:** Run #102 (ID: `42444149346`, commit `e6a57ae58d7d0cd30265834fc51e2bab32928732`, evento `push`, **7/7 jobs `success`**)<br />
**Workflows CI previos relevantes (SHAs completos verificados vÃ­a GitHub Actions API):**<br />
- Run #101 (ID: `42438637184`, commit `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`, evento `push`, **7/7 jobs `success`**)<br />
- Run #100 (ID: `42435675882`, commit `86d6adcef9234b579e9313215673cf540e9fa52b`, evento `push`, **7/7 jobs `success`**)<br />
- Run #99 (ID: `42432585253`, commit `4d674ebd933f70edd2a1be789df2e3414e1b59f0`, evento `push`, **7/7 jobs `success`**)<br />
- Run #98 (ID: `42429999278`, commit `2f1281b0df0fa4906f14f690d95be42beadf15aa`, evento `push`, **7/7 jobs `success`**)<br />
- Run #97 (ID: `42428357398`, commit `405f948d9267a5e7c8b4b0b2f74eeedf799cbf9d`, evento `push`, **7/7 jobs `success`**)<br />
- Run #96 (ID: `42426468347`, commit `11a7e497a212000809084d56cb3c596ccd3c6f43`, evento `push`, **7/7 jobs `success`**)<br />
- Run #95 (ID: `42420318570`, commit `fc345ebd06239e8b86c0c502a4b289052e988a8c`, evento `push`, **7/7 jobs `success`**)<br />
- Run #94 (ID: `42417187681`, commit `aedd19c819ddeaf4e482576fb973f490980b7aa5`, evento `push`, **7/7 jobs `success`**)<br />
- Run #93 (ID: `42410227273`, commit `54fb44670c55b3d0e009e1236dc6a48e0b44378c`, evento `push`, **7/7 jobs `success`**)<br />
- Run #92 (ID: `42396473407`, commit `653be31040da53bea72a540ddb2a111f16b35d50`, evento `push`, **7/7 jobs `success`**)<br />
- Run #91 (ID: `42390732091`, commit `07e41885f4b0398c05c7193e7191773bb658bd6f`, evento `push`, **7/7 jobs `success`**)<br />
- Run #90 (ID: `42384884759`, commit `b9d4b8f1627c74b70b977f509cbc56acc7989ad0`, evento `push`, **7/7 jobs `success`**)<br />
- Run #89 (ID: `42379421580`, commit `d8ca7acc703397cabec8d5cbba92f39329fa940a`, evento `push`, **7/7 jobs `success`**)<br />
- Runs #87 y #86 (IDs: `42346415046` y `42342850877`, commits `391d79e9255cb07f87f6e13748225cd55fc6f514` y `0a270a1b7fb50bb894b0137cd17ef2dc65dcc246`, evento `pull_request`, `head_branch`: `feature/sprint-1-store-profile-guards-d01-d02`, destino: `main`, PR #11, **7/7 jobs `success`**)<br />
**Veredicto de Rebaseline R7:** **R7 PASS â€” R6 REMOTE STATE RECONCILED â€” CI R6 PASS â€” L0 NOT VERIFIED â€” LAB HOLD**<br />
**ClasificaciÃ³n de Host Preflight:** **GUEST DETECTED â€” L0 NOT VERIFIED**<br />
**Readiness del Laboratorio VM:** **HOLD â€” Requiere aprovisionamiento del propietario en Hipervisor L0 (TopologÃ­a B)**<br />
**Estado Microsoft Store:** **CONDITIONAL HOLD**<br />
**Estado Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**

---

## 1. Resumen Ejecutivo y Veredictos Formales

### 1.1 Resumen Ejecutivo y Addenda de ReconciliaciÃ³n Factual (R1, R2 y R3)
El presente documento constituye la **revisiÃ³n factual, adversarial, de sincronizaciÃ³n y correctiva R3** del rebaseline tÃ©cnico de la Fase 8B.1A.7.2 (preparaciÃ³n para pruebas dinÃ¡micas de almacenamiento MSIX y recuperaciÃ³n transaccional en VM aislada).

Los informes y runbooks previos de virtualizaciÃ³n (Fases 8B.1A.5, 8B.1A.6 y 8B.1A.7.1) fueron redactados tomando como base el commit histÃ³rico `9f856c0405e4e5b64e784309affad450c38d3a37`. Desde dicho punto, la historia del repositorio avanzÃ³ mediante 5 commits en el rango `9f856c0..d8ca7acc` (Sprint 1.0, 1.1 y 1.2) seguidos de **11 commits exclusivamente documentales en `d8ca7acc..2f1281b0`**:
- Cuatro commits documentales iniciales: `dded99c`, `b9d4b8f`, `07e4188` y `653be31` (este Ãºltimo fijando el baseline del cÃ³digo auditado).
- Siete commits documentales correctivos posteriores: `3f08f12`, `54fb446` (SHA real `54fb44670c55...`), `aedd19c` (SHA real `aedd19c819dd...`), `fc345eb`, `11a7e49`, `405f948d` y `2f1281b0df0fa4906f14f690d95be42beadf15aa`.

#### Addendum R3: ReconciliaciÃ³n Post-Push R2, Carrera por Orden de LiberaciÃ³n de Mutexes y DiseÃ±o TEST-09.7
1. **ReconciliaciÃ³n Factual Post-Push de `2f1281b` y Trazabilidad Remota:**
   - La consulta a la API de GitHub Actions confirma que el commit `2f1281b0df0fa4906f14f690d95be42beadf15aa` (`docs: reconcile R2 FUS state machine and host preflight`) se encuentra publicado en la rama `main` de `origin/main` y desencadenÃ³ el workflow **Run #98** (ID: `42429999278`), completando **7/7 jobs con conclusiÃ³n `success`**.
   - Se rectifica formalmente la declaraciÃ³n histÃ³rica `"ZERO PUSH EJECUTADO"` del informe R2: dicha frase reflejaba el Ã¡mbito local de los comandos en la sesiÃ³n del agente, pero el estado observable en el remoto confirma que **el commit R2 fue publicado en `main`**. No se formula ninguna inferencia sobre el actor, sincronizaciÃ³n o cliente que haya producido dicha publicaciÃ³n.
   - En el workflow Run #98 se publicaron 4 ejecutables single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicÃ³ ningÃºn paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`**.
2. **CorrecciÃ³n de SHAs Completos de CI en la Tabla HistÃ³rica:**
   - Se verificaron directamente contra la API de GitHub Actions los SHAs de 40 caracteres de los workflows anteriores, corrigiendo discrepancias de transcripciÃ³n presentes en informes previos:
     - Run #86: `0a270a1b7fb50bb894b0137cd17ef2dc65dcc246` (corrigiendo la errata de terminaciÃ³n `...22d645e`).
     - Run #87: `391d79e9255cb07f87f6e13748225cd55fc6f514` (corrigiendo la errata de terminaciÃ³n `...9294ae0`).
     - Run #93: `54fb44670c55b3d0e009e1236dc6a48e0b44378c` (corrigiendo la errata de terminaciÃ³n `...c21ec15`).
     - Run #94: `aedd19c819ddeaf4e482576fb973f490980b7aa5` (corrigiendo la errata de terminaciÃ³n `...06ebf19`).
3. **ModeraciÃ³n de la DeclaraciÃ³n sobre el SHA R1 Inexistente:**
   - Se matiza la conclusiÃ³n absoluta sobre el SHA `405f9485121b6a12dfd54d241772ea59419b4872`:
     - *DeclaraciÃ³n ajustada:* **El SHA completo reportado no se encontrÃ³ en el object database ni en el remoto consultado; la evidencia disponible indica una transcripciÃ³n incorrecta al expandir el prefijo abreviado `405f948`.**
4. **CorrecciÃ³n del Literal Exacto del Mensaje de Timeout:**
   - Se sustituye la parÃ¡frasis errÃ³nea de R2 por el texto literal exacto implementado en el cÃ³digo de producciÃ³n ([`WindowsUninstallRequestListener.cs#L149-L152`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L149-L152)):
     ```csharp
     error = requestSent
         ? "The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."
         : "No running PrivGvard instance accepted the uninstall shutdown request.";
     ```
5. **Hallazgo Central R3 â€” Carrera por Orden de LiberaciÃ³n de Mutexes (*Release-Order Race*):**
   - La revisiÃ³n de sincronizaciÃ³n detectÃ³ una intercalaciÃ³n crÃ­tica en el orden real de las operaciones de producciÃ³n:
     - **Orden de AdquisiciÃ³n en Guard:** `Global\PrivLock_UninstallGate` (1Âº) -> `Global\PrivLock_SingleInstance` (2Âº).
     - **Orden de LiberaciÃ³n en Guard (`Release()`):** En [`WindowsSingleInstanceGuard.cs#L84-L88`](../../src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L84-L88), el cÃ³digo ejecuta:
       ```csharp
       public void Release()
       {
           ReleaseMutexCore(ref _mutex, ref _hasAcquired, "single-instance");
           ReleaseUninstallGateCore();
       }
       ```
       Es decir: **`Global\PrivLock_SingleInstance` se libera PRIMERO, y `Global\PrivLock_UninstallGate` se libera DESPUÃ‰S**.
     - **Mutex Sondeado en Handoff:** En `TryRequestRunningInstanceAndAcquire()`, el proceso sondea exclusivamente `Global\PrivLock_SingleInstance`.
     - **Mecanismo de IntercalaciÃ³n en FUS (`requestSent == false`):**
       1. En FUS, el evento `Local\PrivLock_UninstallRequested` de UserA es inaccesible para UserB (`requestSent = false`).
       2. UserA inicia su cierre y llama a `Release()`.
       3. UserA libera `Global\PrivLock_SingleInstance`.
       4. Antes de que UserA alcance a ejecutar `ReleaseUninstallGateCore()`, el planificador del SO cede la CPU a UserB.
       5. UserB estaba esperando sobre `SingleInstance`; su `WaitOne` tiene Ã©xito porque UserA ya lo liberÃ³.
       6. UserB libera su handle de prueba e invoca `singleInstanceGuard.TryAcquireSingleInstance()`.
       7. La guarda de UserB intenta adquirir primero `Global\PrivLock_UninstallGate`.
       8. **`UninstallGate` todavÃ­a pertenece a UserA**, por lo que `TryAcquireSingleInstance()` falla y devuelve `false`.
       9. Al evaluar la lÃ­nea 140, dado que `requestSent == false`, UserB **aborta de inmediato** con el error: *"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."*
       10. UserA libera el uninstall gate un instante despuÃ©s, pero UserB ya finalizÃ³ el handoff con fallo.
   - **ClasificaciÃ³n Formal del Hallazgo:**
     $$\mathbf{RACE\ CONDITION\ PLAUSIBLE\ â€”\ STATICALLY\ GROUNDED\ â€”\ DYNAMIC\ REPRODUCTION\ REQUIRED}$$
   - **DelimitaciÃ³n de Impacto:** Se trata de un **fallo fail-closed de disponibilidad/fiabilidad del handoff** (se rechaza una desinstalaciÃ³n segura legÃ­tima). No constituye bypass de privacidad ni eliminaciÃ³n insegura de archivos.
6. **IncorporaciÃ³n de `TEST-09.7` en el Runbook DinÃ¡mico:**
   - Se formula el diseÃ±o formal de la prueba falsable `TEST-09.7` para reproducir controladamente esta intercalaciÃ³n mediante pausa en depuraciÃ³n o instrumentaciÃ³n de sincronizaciÃ³n, manteniÃ©ndola bajo **`LAB HOLD / NOT RUN`**.

#### Addendum R4: ReconciliaciÃ³n Post-Push Run #99, RemediaciÃ³n en CÃ³digo de la Carrera de LiberaciÃ³n FUS y Tests Automatizados
1. **ReconciliaciÃ³n Factual Post-Push de `4d674ebd` y Trazabilidad Remota:**
   - La consulta a la API de GitHub Actions confirma que el commit `4d674ebd933f70edd2a1be789df2e3414e1b59f0` (`docs: reconcile R3 release-order race and TEST-09.7 design`) se encuentra publicado en la rama `main` de `origin/main` y desencadenÃ³ el workflow **Run #99** (ID: `42432585253`), completando **7/7 jobs con conclusiÃ³n `success`**.
   - Se rectifica formalmente la declaraciÃ³n histÃ³rica `"ZERO PUSH EJECUTADO"` / `"LOCAL ONLY"` del informe R3: dicha declaraciÃ³n reflejaba con rigor el Ã¡mbito local de los comandos en la sesiÃ³n del agente al momento de la captura, pero el estado observable en el remoto confirma que **el commit R3 fue publicado en `main`**. No se formula ninguna inferencia sobre el actor, cliente o sincronizaciÃ³n externa que haya producido dicha publicaciÃ³n.
   - En el workflow Run #99 se publicaron 4 ejecutables single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicÃ³ ningÃºn paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`**.
2. **Matriz de DecisiÃ³n y AnÃ¡lisis de Opciones de RemediaciÃ³n (Opciones A, B y C):**
   - Se evaluaron tres opciones arquitectÃ³nicas para resolver el fallo prematuro por contenciÃ³n en el handoff de desinstalaciÃ³n frente a los 7 criterios de ingenierÃ­a:
     - **OpciÃ³n A (Reintento Acotado con Backoff en `TryRequestRunningInstanceAndAcquire`):** Ante un resultado de contenciÃ³n transitoria (`requestSent == false`), no aborta de inmediato; duerme un intervalo acotado (`effectivePollInterval`, default 100ms) y reintenta dentro de la ventana de timeout global (`Stopwatch` monotÃ³nico).
     - **OpciÃ³n B (Invertir Orden de LiberaciÃ³n en `WindowsSingleInstanceGuard.Release()`):** Liberar `_uninstallGate` antes que `_mutex`. Aunque reduce la ventana entre UserA y UserB, no resuelve la contenciÃ³n transitoria por un tercer proceso, altera el ciclo de vida de cierre en todas las salidas normales, y presenta riesgo de interacciÃ³n adversa con `ReleaseUninstallGateForHandoff()` en Inno Setup.
     - **OpciÃ³n C (Resultado Enriquecido EspecÃ­fico de Windows `WindowsSingleInstanceAcquisitionResult`):** Extiende el contrato interno en Windows para discernir si la contenciÃ³n se debiÃ³ a `UninstallGateBusy`, `SingleInstanceBusy` o `AccessOrCreationError`.
   - **DecisiÃ³n Adoptada: OpciÃ³n A + OpciÃ³n C (Reintento Acotado + Resultado EspecÃ­fico Windows).**
     - Mantiene `ISingleInstanceGuard` 100% inalterado (cero blast radius sobre abstracciones multiplataforma).
     - Conserva intacto el ciclo de vida de apagado e Inno Setup handoff.
     - Suprime la inundaciÃ³n de logs en Serilog (`suppressLogging: true` en reintentos).
     - Resuelve deterministamente tanto la *Release-Order Race* como la contenciÃ³n por tercer proceso.
     - Falla de inmediato (*fail-closed*) ante errores no transitorios (`AccessOrCreationError`).
3. **RemediaciÃ³n en CÃ³digo de ProducciÃ³n Implementada:**
   - En [`WindowsSingleInstanceGuard.cs`](../../src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs):
     - Declarado enum interno `WindowsSingleInstanceAcquisitionResult { Acquired, UninstallGateBusy, SingleInstanceBusy, AccessOrCreationError }`.
     - Declarada interfaz interna `IWindowsDetailedSingleInstanceGuard`.
     - Implementado mÃ©todo `internal WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false)`.
     - `TryAcquireSingleInstance()` consume `TryAcquireSingleInstanceDetail(suppressLogging: false) == WindowsSingleInstanceAcquisitionResult.Acquired`.
     - Limpieza rigurosa de mutex handles en casos de contenciÃ³n.
   - En [`WindowsUninstallRequestListener.cs`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs):
     - AÃ±adido parÃ¡metro opcional `TimeSpan? pollInterval = null` a `TryRequestRunningInstanceAndAcquire` (default 100ms).
     - En el bucle `while (stopwatch.Elapsed < timeout)`: si `outcome` es `UninstallGateBusy` o `SingleInstanceBusy`, duerme `Math.Min(effectivePollInterval, remaining)` y reintenta.
     - Si `outcome == AccessOrCreationError`, termina de inmediato devolviendo `false` con `"Failed to create or acquire single-instance mutex on Windows."` sin bucle innecesario.
     - DiagnÃ³sticos de salida precisos segÃºn `requestSent` y `probeAcquiredAtLeastOnce`.
4. **Nuevos Tests Automatizados de SincronizaciÃ³n:**
   - AÃ±adidos 5 nuevos tests deterministas `[Fact]` en [`WindowsUninstallRequestListenerTests.cs`](../../tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs):
     - `NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired` (simula Release-Order Race en FUS).
     - `NoEventPermanentContention_FailsClosedAtTimeoutWithoutBusySpin` (prueba fail-closed y ausencia de busy-spin).
     - `PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout` (prueba terminaciÃ³n inmediata ante error fatal).
     - `ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout` (simula contenciÃ³n transitoria por tercer proceso y recuperaciÃ³n).
     - `AbandonedMutex_OnProbeHandle_PreservesOwnershipAndSucceedsIfGuardAcquires` (prueba manejo de mutex abandonado).
   - AÃ±adido docstring explicativo en [`WindowsSingleInstanceStartupTests.cs`](../../tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs) clarificando que los tests concurrentes son unitarios intra-proceso y no sustituyen la validaciÃ³n dinÃ¡mica en VM.
   - La suite de pruebas de Windows cuenta ahora con **319 tests superados (0 fallos)**, y la soluciÃ³n completa con **601 tests superados (0 fallos)**.
5. **ReafirmaciÃ³n de `TEST-09.7` en el Runbook DinÃ¡mico:**
   - El caso de prueba dinÃ¡mico `TEST-09.7` en VM Windows 11 permanece bajo **`LAB HOLD / NOT RUN`**, reservado para la ejecuciÃ³n en el host fÃ­sico L0 del propietario.

#### Addendum R5: ReconciliaciÃ³n Post-Push Run #100, Endurecimiento del Contrato de Reintento (R5-01..R5-03), Tests con Mutexes Reales `Local\` y Criterios Post-Fix de TEST-09.7
1. **ReconciliaciÃ³n Factual Post-Push de `86d6adc` y Trazabilidad Remota:**
   - La consulta directa a la API de GitHub Actions y a las referencias remotas (`git ls-remote`) confirma que el commit funcional `86d6adcef9234b579e9313215673cf540e9fa52b` (`fix(windows): retry FUS handoff on transient uninstall-gate contention`, parent `4d674ebd933f70edd2a1be789df2e3414e1b59f0`) se encuentra publicado en la rama `main` de `origin/main` y desencadenÃ³ el workflow **Run #100** (ID: `42435675882`, evento `push`), completando **7/7 jobs con conclusiÃ³n `success`** (601 tests superados en CI: 25 Domain, 167 Application, 90 Infrastructure, 319 Windows Platform).
   - Se reconcilia formalmente la declaraciÃ³n histÃ³rica `"ZERO PUSH EJECUTADO"` / `"LOCAL ONLY"` del informe R4: dicha declaraciÃ³n describÃ­a con exactitud las acciones ejecutadas dentro de la sesiÃ³n del agente en el momento de su emisiÃ³n, pero el estado observable posterior en el remoto confirma que **el commit R4 fue publicado en `main`**. No se realiza ninguna atribuciÃ³n no verificada sobre el cliente o actor que efectuÃ³ dicha publicaciÃ³n.
   - En el workflow Run #100 se publicaron 4 ejecutables single-file (`PrivGvard-osx-arm64`, `PrivGvard-linux-x64`, `PrivGvard-win-arm64`, `PrivGvard-win-x64`). **CI no publicÃ³ ningÃºn paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`**.
   - Se corrigiÃ³ asimismo en `docs/ai/AUDIT-ROADMAP.md` la discrepancia histÃ³rica de conteos de R4 (`266 Windows / 559 total` -> `319 Windows / 601 total` verificados en R4 y Run #100).
2. **Endurecimiento del Contrato de Reintento y DiagnÃ³sticos en `WindowsUninstallRequestListener.cs` (R5-01, R5-02 y R5-03):**
   - **R5-02 (PreservaciÃ³n de la Signatura PÃºblica de 5 ParÃ¡metros):**
     - Se restaurÃ³ la signatura pÃºblica original de 5 parÃ¡metros:
       ```csharp
       public static bool TryRequestRunningInstanceAndAcquire(
           ISingleInstanceGuard singleInstanceGuard,
           TimeSpan timeout,
           out string error,
           string requestEventName = RequestEventName,
           string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
       ```
     - El parÃ¡metro `TimeSpan pollInterval` se trasladÃ³ a una sobrecarga `internal` consumida por la suite de pruebas (`InternalsVisibleTo("PrivLock.Platform.Windows.Tests")`), garantizando compatibilidad binaria estricta del API pÃºblico.
   - **R5-01 (ValidaciÃ³n Estricta de `timeout` y `pollInterval` y EliminaciÃ³n de Truncamiento Sub-Milisegundo):**
     - Se incorporÃ³ `ValidatePositiveFiniteInterval(TimeSpan value, string paramName)` antes de abrir cualquier handle de evento o mutex y antes de entrar al bucle de espera.
     - Rechaza inmediatamente con `ArgumentOutOfRangeException` valores `<= TimeSpan.Zero`, intervalos sub-milisegundo (`< 1 ms`, que `.NET` truncarÃ­a a `0 ms`), `Timeout.InfiniteTimeSpan` (`-1 ms`) y valores superiores a `TimeSpan.FromMilliseconds(int.MaxValue)`.
     - AdemÃ¡s, en el cÃ¡lculo de `waitInterval` y `sleepTime` dentro del bucle, se aplica un lÃ­mite inferior de `MinWaitHandleInterval` (`1 ms`) cuando `0 < remaining < 1 ms`, impidiendo que el truncamiento entero `(long)TimeSpan.TotalMilliseconds` de `.NET` genere llamadas `Thread.Sleep(0)` (busy-spin) en el Ãºltimo milisegundo del plazo de timeout.
   - **R5-03 (DiagnÃ³stico de Timeout Diferenciado segÃºn la Ãšltima Causa de ContenciÃ³n Observada):**
     - Se registra `lastContentionOutcome` (`UninstallGateBusy` vs `SingleInstanceBusy`) en cada intento de adquisiciÃ³n de la guarda tras el sondeo.
     - Al expirar el timeout cuando `requestSent == false`:
       - Si la Ãºltima causa observada fue `UninstallGateBusy`: `"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request (last observed contention: UninstallGateBusy; does not guarantee mutex state after timeout)."`
       - Si la Ãºltima causa observada fue `SingleInstanceBusy`: `"The Windows single-instance mutex remained busy after probe release, and no running PrivGvard instance accepted a shutdown request (last observed contention: SingleInstanceBusy; does not guarantee mutex state after timeout)."` (nunca afirma falsamente que `UninstallGate` estÃ¡ ocupado cuando fallÃ³ `SingleInstance`).
       - Si el mutex de sondeo nunca llegÃ³ a adquirirse (`lastContentionOutcome == null`): `"Timed out waiting for Windows single-instance admission after no running PrivGvard instance accepted the uninstall shutdown request."` (diagnÃ³stico neutral que no presupone si existÃ­a o no una instancia activa en otra sesiÃ³n).
     - Si `requestSent == true`, se preserva intacto el literal: `"The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."`
3. **AmpliaciÃ³n de Cobertura Automatizada con Mutexes con Nombre Reales `Local\`:**
   - En [`WindowsSingleInstanceStartupTests.cs`](../../tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs):
     - AÃ±adidos 3 tests `[Fact]` que ejercitan `WindowsSingleInstanceGuard.TryAcquireSingleInstanceDetail` sobre **mutexes con nombre reales de Windows en `Local\`** mantenidos por hilos dedicados del SO:
       1. `RealNamedMutexes_WhenUninstallGateOwnedByAnotherThread_ReturnsUninstallGateBusyAndLeavesMainMutexFree`
       2. `RealNamedMutexes_WhenMainMutexOwnedByAnotherThread_ReturnsSingleInstanceBusyAndReleasesGate` (verifica en un hilo separado que el gate fue liberado tras el fallo en `SingleInstance`)
       3. `RealNamedMutexes_WhenBothFree_AcquiresBothAndReleaseAllowsSubsequentAcquisition`
   - En [`WindowsUninstallRequestListenerTests.cs`](../../tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs):
     - AÃ±adido el test de integraciÃ³n intra-sesiÃ³n con mutexes reales `Local\` [`RealNamedMutexes_ReleaseOrderInterleaving_RetriesWhileUninstallGateHeldAndAcquiresBothWhenGateReleased`](../../tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs#L439-L545), que reproduce determinÃ­sticamente la ventana de intercalaciÃ³n de orden de liberaciÃ³n R3/R4 entre dos hilos del SO usando `ManualResetEventSlim` (sin hooks en producciÃ³n ni sleeps arbitrarios).
     - Reforzado `AbandonedMutex_OnProbeHandle_PreservesOwnershipReleasesProbeBeforeGuardAndLeavesNoExtraHoldCount` verificando desde un hilo separado que el handle de sondeo abandonado se libera antes de la adquisiciÃ³n de la guarda y no deja recuento recursivo residual tras `guard.Release()`.
     - AÃ±adidos tests de reflexiÃ³n para la signatura pÃºblica de 5 parÃ¡metros, validaciÃ³n de `pollInterval` y `timeout` (incluyendo `0`, `-1 ms` / `Timeout.InfiniteTimeSpan`, negativos, `< 1 ms` y `TimeSpan.MaxValue`), y diagnÃ³sticos diferenciados de `UninstallGateBusy`, `SingleInstanceBusy` y secuencia mixta.
     - La suite de Windows alcanza **334 casos de prueba superados (0 fallos)** y la soluciÃ³n completa **616 casos de prueba superados (0 fallos)**.
4. **ActualizaciÃ³n del Runbook `TEST-09.7` (SeparaciÃ³n Pre-Fix vs. Post-Fix):**
   - La especificaciÃ³n de `TEST-09.7` en la SecciÃ³n 7 distingue ahora explÃ­citamente entre:
     - **SecciÃ³n 6.1 (Comportamiento HistÃ³rico Pre-Fix R3 â€” Baseline de RegresiÃ³n):** Fallo prematuro en el primer intento cuando `requestSent == false` y UserA retiene `UninstallGate`.
     - **SecciÃ³n 6.2 (Comportamiento Esperado Post-Fix R4/R5 en CÃ³digo Actual â€” Criterio de AceptaciÃ³n):** UserB observa `UninstallGateBusy`, reintenta durante la ventana de timeout y, al reanudarse UserA y liberar `UninstallGate`, adquiere ambos mutexes y retorna `true`.
     - **SecciÃ³n 6.3 (Controles Negativos):** RetenciÃ³n hasta agotar timeout (`UninstallGateBusy`), contenciÃ³n en `SingleInstance` (`SingleInstanceBusy`), error de DACL (`AccessOrCreationError`) y timeout sin adquirir nunca el sondeo.
   - `TEST-09.7` permanece estrictamente en **`LAB HOLD / NOT RUN`**.

#### Addendum R6: ReconciliaciÃ³n Post-Push Run #101, VerificaciÃ³n CI por SHA, AuditorÃ­a de Diffs R5 y Gate Pre-Laboratorio
1. **ReconciliaciÃ³n Factual Post-Push de `85d3f71d` y Trazabilidad Remota:**
   - La consulta directa a la API de GitHub Actions y a las referencias remotas (`git ls-remote`) confirma que el commit funcional + documental R5 `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (`fix(windows): harden FUS handoff retry contract and add real mutex tests`, parent `86d6adcef9234b579e9313215673cf540e9fa52b`) se encuentra publicado en la rama `main` de `origin/main` y desencadenÃ³ el workflow **Run #101** (ID: `42438637184`, evento `push`), completando **7/7 jobs con conclusiÃ³n `success`** (616 tests superados en la soluciÃ³n: 25 Domain, 167 Application, 90 Infrastructure, 334 Windows Platform).
   - Se reconcilia formalmente la declaraciÃ³n histÃ³rica `"ZERO PUSH EJECUTADO"` / `"LOCAL ONLY"` del informe R5: describÃ­a rigurosamente el Ã¡mbito local de los comandos en la sesiÃ³n del agente al emitir el informe, pero la realidad observable posterior en el remoto demuestra que el commit R5 fue publicado en `main`. No se formula ninguna inferencia sobre el actor, cliente o sincronizaciÃ³n externa que haya producido dicha publicaciÃ³n.
   - En el workflow Run #101 se publicaron 4 ejecutables single-file (`PrivGvard-win-arm64`, `PrivGvard-win-x64`, `PrivGvard-osx-arm64`, `PrivGvard-linux-x64`). **CI no publicÃ³ ningÃºn paquete MSIX descargable ni el archivo `BUILD-PROVENANCE.json`**.
2. **AuditorÃ­a EstÃ¡tica de Diffs R5 y ValidaciÃ³n de Cero Regresiones:**
   - El diff `86d6adcef9234b579e9313215673cf540e9fa52b..85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` afectÃ³ a 6 archivos (`WindowsUninstallRequestListener.cs`, `WindowsSingleInstanceStartupTests.cs`, `WindowsUninstallRequestListenerTests.cs`, `AUDIT-ROADMAP.md`, `PROJECT_CONTEXT.md` e `INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`). `WindowsSingleInstanceGuard.cs` no fue modificado en R5.
   - Se ratifican los 15 puntos estÃ¡ticos de la remediaciÃ³n: signatura pÃºblica original de 5 parÃ¡metros preservada, sobrecarga `internal` de 6 parÃ¡metros para pruebas, validaciÃ³n pre-handle de `timeout` y `pollInterval` contra valores `<= 0`, sub-milisegundo, `-1 ms` y `> int.MaxValue`, acotamiento de 1ms en esperas residuales contra busy-spin, diagnÃ³sticos diferenciados de `UninstallGateBusy`, `SingleInstanceBusy` y timeout neutral sin presuponer instancias activas, y ausencia total de hooks de depuraciÃ³n en producciÃ³n.
3. **Reproducibilidad Local Exacta y Forense de `task-9809`:**
   - La suite completa de 616 pruebas reproduce 100% PASS localmente en `.NET SDK 10.0.403` bajo ejecuciÃ³n serializada por proyecto (`dotnet test PrivGvard.sln -c Release --no-build -m:1`), coincidiendo con la topologÃ­a de CI.
   - Se reconcilia el fallo observado en el intento previo `task-9809` de la sesiÃ³n R5 en `SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin`: la ausencia inicial de lÃ­mite inferior `MinWaitHandleInterval = 1 ms` en las esperas residuales provocaba que un remanente `< 1 ms` se truncase a `0 ms` (`Thread.Sleep(0)`), inflando el conteo de reintentos a 30 (fuera del rango 1â€“20); el commit R5 solventÃ³ esto acotando a 1ms, estabilizando los intentos entre 8 y 9.
4. **Gate Pre-Laboratorio:**
   - Todo el cÃ³digo de producciÃ³n y pruebas se encuentra reconciliado, verificado en CI (Run #101) y reproducido localmente. El entorno de desarrollo actual permanece como `GUEST DETECTED â€” L0 NOT VERIFIED`, por lo que el conjunto de pruebas dinÃ¡micas (TEST-01 a TEST-11, incluido TEST-09.7) permanece formalmente bajo **`LAB HOLD / NOT RUN`**, a la espera de que el propietario del repositorio aprovisione una VM aislada directa en el hipervisor fÃ­sico L0 (TopologÃ­a B).

#### Addendum R7: ReconciliaciÃ³n Post-Push R6, VerificaciÃ³n CI Run #102, AuditorÃ­a de Deadline R5 y Gate L0
1. **ReconciliaciÃ³n Factual Post-Push del Commit R6 (`e6a57ae`):**
   - La consulta directa a las referencias remotas (`git ls-remote --heads origin`) y a la API de GitHub (`GET /repos/cristianbarreiro/PrivGuard/branches/main`, consultado 2026-11-08T00:29:40Z) confirma que el commit R6 `e6a57ae58d7d0cd30265834fc51e2bab32928732` (`docs: reconcile R5 post-push evidence and CI state`) se encuentra publicado en la rama `main` de `origin` y constituye su HEAD canÃ³nico actual.
   - El diff `85d3f71..e6a57ae` es estrictamente documental (4 archivos de documentaciÃ³n modificados, cero archivos de cÃ³digo de producciÃ³n o pruebas).
   - Se rectifica la declaraciÃ³n de cierre de la sesiÃ³n R6 (`ZERO PUSH / local only`): describÃ­a rigurosamente las acciones locales ejecutadas en dicha sesiÃ³n, pero quedÃ³ superada por la publicaciÃ³n remota posterior. No se atribuye quiÃ©n realizÃ³ el push ni mediante quÃ© cliente/herramienta de sincronizaciÃ³n.
2. **VerificaciÃ³n de GitHub Actions para R6 (Run #102 â€” `CI R6 PASS`):**
   - La consulta por `head_sha=e6a57ae58d7d0cd30265834fc51e2bab32928732` arrojÃ³ el workflow **Run #102** (ID: `42444149346`, evento `push`, rama `main`, conclusiÃ³n `success`, 7/7 jobs exitosos).
   - Todos los pasos en Windows (Restore, Build, Domain, Application, Infrastructure, Windows Platform Tests [334 tests] y Validate Release-Store MSIX Packaging) concluyeron en `success`.
   - Se publicaron 4 ejecutables single-file (`PrivGvard-win-x64`, `PrivGvard-win-arm64`, `PrivGvard-osx-arm64`, `PrivGvard-linux-x64`). Cero paquetes `.msix` o archivos `BUILD-PROVENANCE.json` descargables.
3. **AuditorÃ­a del Borde Temporal de `WindowsUninstallRequestListener` (R7-04 â€” `DOCUMENTED ACCEPTANCE`):**
   - Se evaluÃ³ estÃ¡ticamente el matiz por el cual `WaitOne(waitInterval)` en la Ãºltima iteraciÃ³n puede retornar tras cruzar la ventana nominal `timeout` debido a la granularidad del temporizador de Windows (~15.6 ms) y al lÃ­mite inferior `MinWaitHandleInterval = 1 ms`.
   - Se adoptÃ³ formalmente la **OpciÃ³n A (Deadline Aproximado Aceptado)**: el contrato de timeout en handoff es monotÃ³nico (`Stopwatch`) y acotado por sondeo (*polling bounded*); no constituye una garantÃ­a de tiempo real duro sub-milisegundo. Descartar una adquisiciÃ³n legÃ­tima post-deadline por microsegundos provocarÃ­a un falso negativo destructivo para la desinstalaciÃ³n. Cero cambios en cÃ³digo de producciÃ³n.
4. **PreparaciÃ³n del Gate de Aprovisionamiento en Host FÃ­sico L0 (TopologÃ­a B):**
   - Se elaborÃ³ el script PowerShell read-only [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1) y el runbook [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md).
   - Se ratifica el estado **`L0 NOT VERIFIED â€” LAB HOLD`** hasta que el propietario ejecute el diagnÃ³stico en el hardware fÃ­sico anfitriÃ³n.

#### Addendum R8: ReconciliaciÃ³n Post-Push R7, VerificaciÃ³n CI Run #103, ReparaciÃ³n de Evidencia y Endurecimiento L0
1. **ReconciliaciÃ³n Factual Post-Push del Commit R7 (`f581a1c`):**
   - Al consultar las referencias remotas (`git ls-remote --heads origin` y `git fetch origin main`, consultado 2026-11-08T02:03:30Z), se confirmÃ³ que el commit R7 `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` (`docs: reconcile R6 remote state and prepare L0 lab gate`) se encuentra publicado como HEAD canÃ³nico en `origin/main`.
   - La declaraciÃ³n histÃ³rica `ZERO PUSH` de la sesiÃ³n local R7 queda superada por la observaciÃ³n remota, sin atribuir la sincronizaciÃ³n a un actor o cliente concreto.
2. **VerificaciÃ³n de GitHub Actions para R7 (Run #103 â€” `CI R7 PASS`):**
   - La consulta directa por `head_sha=f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` confirmÃ³ el workflow **Run #103** (ID: `42448487384`, evento `push`, rama `main`, conclusiÃ³n `success`, **7/7 jobs exitosos**).
   - Publicados los 4 ejecutables single-file habituales. Cero paquetes `.msix` o archivos `BUILD-PROVENANCE.json` descargables.
3. **ReparaciÃ³n de Integridad de Evidencia en el Informe R7:**
   - Se corrigieron los snippets de cÃ³digo ajenos (`ValueStopwatch`, `MaxAcquisitionRetries`) sustituyÃ©ndolos por el algoritmo real de producciÃ³n de [`WindowsUninstallRequestListener.cs`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197).
   - Se corrigieron las firmas pÃºblica e internal y se armonizÃ³ el inventario real de 16 tests unitarios en `WindowsUninstallRequestListenerTests` y 4 tests en `WindowsSingleInstanceStartupTests`.
   - Se corrigieron los job IDs de Run #102 (`Build & Test (macos-latest)` = `128619439904`, `Publish Single-File (linux-x64)` = `128619848706`), la versiÃ³n del SDK (`.NET 10.0.403`) y el proyecto de test de infraestructura (`tests/PrivLock.Infrastructure.Tests`).
4. **Endurecimiento del Preflight de Host FÃ­sico L0:**
   - Se rediseÃ±Ã³ [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1) eliminando la etiqueta prematura `L0_READY_FOR_TOPOLOGY_B` e introduciendo estados rigurosos (`L0_PREFLIGHT_PASS_CANDIDATE`, `L0_CAPABLE_CONSTRAINED`, `L0_PREFLIGHT_BLOCKED`, `PREFLIGHT_INCOMPLETE`, `GUEST_DETECTED`).
   - Se incorporÃ³ el parÃ¡metro `-VmStoragePath` para validar el volumen y espacio libre destino de la VM, y se aÃ±adieron pruebas unitarias automatizadas (`scripts/lab/Test-L0HostPreflightLogic.ps1`) con 10/10 casos pasando.
5. **ArmonizaciÃ³n de Requisitos de VM Windows 11 en TopologÃ­a B:**
   - Se armonizÃ³ [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) con el mÃ­nimo oficial de Microsoft de **>= 64 GiB de almacenamiento** para VMs Windows 11 y se clarificÃ³ la taxonomÃ­a de hipervisores (Hyper-V como Tipo 1 nativo vs VirtualBox/VMware como Tipo 2 alojados).

### 1.2 Veredicto Oficial

$$\mathbf{VEREDICTO:\ REBASELINE\ R8\ PASS\ â€”\ R7\ REMOTE/CI\ RECONCILED\ â€”\ REPORT\ INTEGRITY\ REPAIRED\ â€”\ L0\ NOT\ VERIFIED\ â€”\ LAB\ HOLD}$$

- **Rebaseline, ReconciliaciÃ³n Run #103, ReparaciÃ³n de Evidencia y Endurecimiento L0:** **PASS**. Se reconcilia el commit R7 `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` en `origin/main` y el Run #103 (`42448487384`, 7/7 jobs green), se reparan las firmas y fragmentos en la documentaciÃ³n de R7, se ratifica el dictamen `SOFT DEADLINE ACCEPTED â€” SEMANTICS DOCUMENTED` sobre el cÃ³digo real, y se endurece el tooling y runbook de preflight de host fÃ­sico L0 con validaciÃ³n por tests.
- **Readiness del Laboratorio VM:** **HOLD**. El host de desarrollo actual permanece clasificado como `GUEST DETECTED â€” L0 NOT VERIFIED`. Se requiere aprovisionamiento de una VM aislada directa en el host fÃ­sico L0 del propietario (TopologÃ­a B).
- **Salvaguardas Invariantes:** Microsoft Store en `CONDITIONAL HOLD`, Fase 8B.1C en `BLOCKED / NOT AUTHORIZED`.

---

## 2. Baseline de Control de Versiones e IntegraciÃ³n Factual

### 2.1 VerificaciÃ³n de Git y Remoto
- **Repositorio:** `cristianbarreiro/PrivGuard`
- **Rama activa de trabajo:** `docs/8b1a7-2-r7-post-r6-reconciliation`
- **HEAD Remoto CanÃ³nico (`origin/main`):** `e6a57ae58d7d0cd30265834fc51e2bab32928732` (R6 documental)
- **Parent directo en `main`:** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (R5 funcional)
- **Baseline de cÃ³digo funcional auditado:** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`
- **Historial `d8ca7acc..e6a57ae` (13 commits documentales + 2 commits funcionales R4 y R5 en `main`):**
  1. `dded99c` â€” Documental: correcciÃ³n de evidencia Sprint 1.2 postflight.
  2. `b9d4b8f` â€” Documental: ajuste de afirmaciones Sprint 1.2 postflight.
  3. `07e4188` â€” Documental: reconciliaciÃ³n Sprint 1.2 con estado remoto.
  4. `653be31` â€” Documental: rebaseline 8B.1A.7.2 contra cÃ³digo vigente (baseline de auditorÃ­a estÃ¡tica).
  5. `3f08f12` â€” Documental: correcciÃ³n factual 8B.1A.7.2.
  6. `54fb44670c55b3d0e009e1236dc6a48e0b44378c` â€” Documental: anÃ¡lisis Full Trust AppData y desinstalaciÃ³n (SHA verificado).
  7. `aedd19c819ddeaf4e482576fb973f490980b7aa5` â€” Documental: alineaciÃ³n post-push de baseline y anÃ¡lisis preliminar FUS (SHA verificado).
  8. `fc345ebd06239e8b86c0c502a4b289052e988a8c` â€” Documental: correcciÃ³n de anÃ¡lisis de DACL de mutex y handoff en FUS.
  9. `11a7e497a212000809084d56cb3c596ccd3c6f43` â€” Documental: finalizaciÃ³n de reconciliaciÃ³n de evidencia 8B.1A.7.
  10. `405f948d9267a5e7c8b4b0b2f74eeedf799cbf9d` â€” Documental: reconciliaciÃ³n post-push R1 y precisiÃ³n de cobertura de tests.
  11. `2f1281b0df0fa4906f14f690d95be42beadf15aa` â€” Documental: reconciliaciÃ³n R2 de mÃ¡quina de estados FUS y preflight de host.
  12. `4d674ebd933f70edd2a1be789df2e3414e1b59f0` â€” Documental: reconciliaciÃ³n R3 de carrera de liberaciÃ³n y diseÃ±o TEST-09.7.
  13. `86d6adcef9234b579e9313215673cf540e9fa52b` â€” Funcional + Documental (R4): remediaciÃ³n de reintento FUS ante contenciÃ³n transitoria en uninstall-gate y tests unitarios.
  14. `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` â€” Funcional + Documental (R5): hardening de contrato de reintento, preservaciÃ³n de signatura pÃºblica de 5 parÃ¡metros, diagnÃ³sticos de timeout diferenciados y tests con mutexes reales `Local\`.
  15. `e6a57ae58d7d0cd30265834fc51e2bab32928732` â€” Documental (R6): reconciliaciÃ³n post-push R5, verificaciÃ³n Run #101 en CI y sincronizaciÃ³n de documentaciÃ³n canÃ³nica (HEAD canÃ³nico en `origin/main`).
- **Ramas Remotas en `origin`:** `main` (`e6a57ae...`), `dev` (`85d028e...`), `feature/sprint-1-store-profile-guards-d01-d02` (`391d79e...`). No existen ramas remotas para R4, R5 ni R6.
- **Forma Real de IntegraciÃ³n en Remoto:** IntegraciÃ³n directa mediante push sin Pull Request. La API de GitHub (`/branches/main`) confirma `protected: false`, `protection.enabled = false` y lista vacÃ­a de rulesets. Existen `0` Pull Requests abiertas actualmente.
- **Estado del Ãrbol de Trabajo:** Modificado localmente en la rama `docs/8b1a7-2-r7-post-r6-reconciliation` (sincronizaciÃ³n documental de R7, sin push ni PR).
- **CÃ³digo Funcional y Pruebas Vigentes (`85d3f71d` / `e6a57ae`):**
  - `src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs` (signatura pÃºblica de 5 parÃ¡metros + sobrecarga `internal` de 6 parÃ¡metros, validaciÃ³n de intervalos pre-handle, deadline aproximado monotÃ³nico acotado por sondeo, y diagnÃ³sticos diferenciados de contenciÃ³n).
  - `tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs` (3 tests con mutexes con nombre reales `Local\` entre hilos dedicados del SO).
  - `tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs` (16 mÃ©todos de prueba / 20 casos ejecutables).

### 2.2 Evidencia de GitHub Actions (Runs #86, #87 y #89 al #102)

| Run ID | Commit SHA (40 caracteres verificados vÃ­a API) | Rama / Ref | Evento | ConclusiÃ³n | Jobs Exitosos | Artefactos Publicados |
|---|---|---|---|---|---|---|
| **42444149346** (#102) | `e6a57ae58d7d0cd30265834fc51e2bab32928732` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42438637184** (#101) | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42435675882** (#100) | `86d6adcef9234b579e9313215673cf540e9fa52b` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42432585253** (#99) | `4d674ebd933f70edd2a1be789df2e3414e1b59f0` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42429999278** (#98) | `2f1281b0df0fa4906f14f690d95be42beadf15aa` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42428357398** (#97) | `405f948d9267a5e7c8b4b0b2f74eeedf799cbf9d` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42426468347** (#96) | `11a7e497a212000809084d56cb3c596ccd3c6f43` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Sin MSIX ni provenance) |
| **42420318570** (#95) | `fc345ebd06239e8b86c0c502a4b289052e988a8c` | `main` | `push` | `success` | 7/7 | 4 Single-File executables |
| **42417187681** (#94) | `aedd19c819ddeaf4e482576fb973f490980b7aa5` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (SHA rectificado) |
| **42410227273** (#93) | `54fb44670c55b3d0e009e1236dc6a48e0b44378c` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (SHA rectificado) |
| **42396473407** (#92) | `653be31040da53bea72a540ddb2a111f16b35d50` | `main` | `push` | `success` | 7/7 | 4 Single-File executables (Baseline auditado) |
| **42390732091** (#91) | `07e41885f4b0398c05c7193e7191773bb658bd6f` | `main` | `push` | `success` | 7/7 | 4 Single-File executables |
| **42384884759** (#90) | `b9d4b8f1627c74b70b977f509cbc56acc7989ad0` | `main` | `push` | `success` | 7/7 | 4 Single-File executables |
| **42379421580** (#89) | `d8ca7acc703397cabec8d5cbba92f39329fa940a` | `main` | `push` | `success` | 7/7 | 4 Single-File executables |
| **42346415046** (#87) | `391d79e9255cb07f87f6e13748225cd55fc6f514` | `feature/sprint-1-store-profile-guards-d01-d02` | `pull_request` (PR #11) | `success` | 7/7 | 4 Single-File executables (SHA rectificado) |
| **42342850877** (#86) | `0a270a1b7fb50bb894b0137cd17ef2dc65dcc246` | `feature/sprint-1-store-profile-guards-d01-d02` | `pull_request` (PR #11) | `success` | 7/7 | 4 Single-File executables (SHA rectificado) |

> [!NOTE]
> **Trazabilidad de Triggers:** El evento `pull_request` fue probado y superado con Ã©xito en los Runs #86 y #87 durante la tramitaciÃ³n de la PR #11. Los 14 commits posteriores (`d8ca7acc..85d3f71d`, formados por 12 commits documentales y 2 commits funcionales R4 y R5) fueron integrados mediante eventos de tipo `push` directo sobre `main` (Runs #89 al #101).

---

## 3. Matriz de ReconciliaciÃ³n Corregida de Supuestos HistÃ³ricos

| Documento HistÃ³rico | AfirmaciÃ³n HistÃ³rica | Estado Real en CÃ³digo Actual (`653be31`) | Dictamen de Vigencia | Evidencia / UbicaciÃ³n en CÃ³digo |
|---|---|---|---|---|
| **8B.1A.5 / 8B.1A.6** (P0-01) | El manifiesto MSIX declara `allowElevation` y `unvirtualizedResources`, violando D-01. | El manifiesto `packaging/Package.appxmanifest` declara **Ãºnicamente** `<rescap:Capability Name="runFullTrust" />`. Las capacidades `allowElevation` y `unvirtualizedResources` fueron eliminadas y su ausencia estÃ¡ protegida por 16 tests unitarios y el CI gate. | **SUPERSEDIDO** (Resuelto por D-01) | [`packaging/Package.appxmanifest#L59-L61`](../../packaging/Package.appxmanifest#L59-L61)<br />[`StoreMsixManifestVerifier.cs#L95-L105`](../../src/PrivLock.Platform.Windows/Packaging/StoreMsixManifestVerifier.cs#L95-L105) |
| **8B.1A.5 / 8B.1A.6** (P0-02) | `IPackageIdentityDetector` e `IEditionProfileResolver` no estÃ¡n registrados en el contenedor de DI en `Program.cs`. | `Program.Main` resuelve el snapshot inmutable y `ConfigureServices` **registra como singletons en DI** tanto `IPackageIdentityDetector` como `IEditionProfileResolver`. TambiÃ©n se configura en `WindowsPrivilegedSession`. | **SUPERSEDIDO** (Resuelto por Sprint 1.0) | [`Program.cs#L25`](../../src/PrivLock.Desktop/Program.cs#L25)<br />[`Program.cs#L628-L630`](../../src/PrivLock.Desktop/Program.cs#L628-L630)<br />[`Program.cs#L682`](../../src/PrivLock.Desktop/Program.cs#L682) |
| **8B.1A.5 / 8B.1A.6** (P0-03) | `WindowsUserPrivacyManager` escribe directamente en HKCU ConsentStore para Store Edition y no existe uso de `ms-settings:`. | `WindowsUserPrivacyManager` divide sus contratos en Reader y Writer. `SetCameraUserPrivacy` y `SetMicrophoneUserPrivacy` **rechazan explÃ­citamente cualquier intento de escritura si el perfil no es `Standalone`** (D-02). En Store Edition, la mutaciÃ³n de cÃ¡mara devuelve error dirigiendo a `ms-settings:privacy-webcam`. | **SUPERSEDIDO** (Resuelto por Sprint 1.0 / D-02) | [`WindowsUserPrivacyManager.cs#L62-L71`](../../src/PrivLock.Platform.Windows/Policies/WindowsUserPrivacyManager.cs#L62-L71)<br />[`WindowsProtectionProvider.cs#L66-L74`](../../src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L66-L74) |
| **8B.1A.5 / 8B.1A.6** (P0-04) | El almacenamiento de Store estÃ¡ aislado por perfil y protegido frente a desinstalaciÃ³n de Windows. | **PrecisiÃ³n Full Trust:** El paquete declara `Windows.FullTrustApplication` y `runFullTrust`. SegÃºn Microsoft Learn, las escrituras de Full Trust en AppData no se redirigen por paquete (pass-through). En cÃ³digo, los stores no consumen el resolver (D-05 no integrada en DI). Por ende, **la expectativa tÃ©cnica documentada es que ambas ediciones compartan las mismas rutas fÃ­sicas en `%LOCALAPPDATA%`**. La confirmaciÃ³n empÃ­rica corresponde a ProcMon en VM. | **PARCIALMENTE VÃLIDO / EXPECTATIVA PASS-THROUGH (REQUIERE VM)** | [`Program.cs#L632-L643`](../../src/PrivLock.Desktop/Program.cs#L632-L643)<br />[`FileStateStore.cs#L24-L30`](../../src/PrivLock.Infrastructure.Common/Storage/FileStateStore.cs#L24-L30)<br />[`FilePrivacySessionStore.cs#L59-L62`](../../src/PrivLock.Infrastructure.Common/Storage/FilePrivacySessionStore.cs#L59-L62) |
| **8B.1A.5 / 8B.1A.6** (P0-05) | `HKLM\SOFTWARE\PrivLock\PrivilegedOwnership\v2` actÃºa como un Machine Recovery Escrow completo. | Se confirma la refutaciÃ³n: HKLM almacena Ãºnicamente un fingerprint criptogrÃ¡fico SHA-256, un SID y una etapa. **No almacena datos de reversiÃ³n de hardware ni snapshots de registro**. Si el WAL del usuario se destruye, el hardware no puede restaurarse desde HKLM. | **VÃLIDO (REFUTACIÃ“N CONFIRMADA)** | [`WindowsPrivilegedOwnershipStore.cs#L140-L180`](../../src/PrivLock.Platform.Windows/Privileged/WindowsPrivilegedOwnershipStore.cs#L140-L180) |
| **8B.1A.5 / 8B.1A.6** (P1-01) | `WindowsProtectionProvider` marca `micStandardIsVerified = true` ignorando si el hardware soporta mute fÃ­sico. | `EvaluateMicrophoneStandardVerification` evalÃºa `AudioHardwareMuteSummary`. Si el mute es solo por software (`NotSupported` o `Unknown`), **no se otorga verificaciÃ³n de exclusiÃ³n ni protecciÃ³n en modo exclusivo**, emitiendo advertencia explÃ­cita al usuario. | **SUPERSEDIDO** (Resuelto en cÃ³digo) | [`WindowsProtectionProvider.cs#L351-L355`](../../src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L351-L355)<br />[`WindowsProtectionProvider.cs#L481-L494`](../../src/PrivLock.Platform.Windows/WindowsProtectionProvider.cs#L481-L494) |
| **8B.1A.5** (Problema B) | Archivos `.claim` en `%ProgramData%\PrivGuard\endpoints` resuelven la exclusiÃ³n mutua cross-edition. | El diseÃ±o de claims en `ProgramData` fue declarado inviable por riesgo de *Claim Squatting*, DoS y redirecciÃ³n VFS. **No existe ninguna implementaciÃ³n de claims en cÃ³digo** (`src/**/*.cs` tiene cero referencias). El diseÃ±o estÃ¡ formalmente descartado. | **VÃLIDO (DISEÃ‘O RECHAZADO)** | BÃºsqueda global en cÃ³digo (`0 coincidencias`). |
| **8B.1A.5** (P1-03) | Existe sincronizaciÃ³n cross-edition mediante `Global\PrivGuard_Transaction_Gate` y `OwnerTuple`. | No existen en cÃ³digo. `Global\PrivLock_SingleInstance` y `Global\PrivLock_UninstallGate` utilizan el namespace global de Windows para exclusiÃ³n de instancia Ãºnica; no poseen DACL explÃ­cita y no constituyen un coordinador de recursos, de almacenamiento ni de diarios cross-edition. | **VÃLIDO (NO IMPLEMENTADO)** | [`WindowsSingleInstanceGuard.cs#L12-L15`](../../src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L12-L15) |
| **8B.1A.7.1** (SecciÃ³n 6-7) | El paquete `PrivGuard-2.0.0.0-x64.msix` y su proveniencia generados en `$env:TEMP` son vÃ¡lidos para la prueba en VM. | Dicho paquete y su `BUILD-PROVENANCE.json` fueron generados desde el commit `9f856c0`. El historial contiene 5 commits en el rango `9f856c0..d8ca7acc` que alteraron cÃ³digo funcional y de packaging. **El artefacto estÃ¡ formalmente obsoleto** y debe reconstruirse desde el HEAD vigente cuando el laboratorio estÃ© listo. | **SUPERSEDIDO (ARTEFACTO OBSOLETO)** | Historial Git: rango `9f856c0..d8ca7acc` (commits `0a270a1`, `391d79e`, `d8ca7ac`). |

---

## 4. AuditorÃ­a EstÃ¡tica Corregida del CÃ³digo Vigente

### 4.1 Full Trust y Rutas Efectivas en AppData (Frontera D-05)
1. **DeclaraciÃ³n del Manifiesto:**
   - [`packaging/Package.appxmanifest#L36-L38`](../../packaging/Package.appxmanifest#L36-L38): `EntryPoint="Windows.FullTrustApplication"`.
   - [`packaging/Package.appxmanifest#L52`](../../packaging/Package.appxmanifest#L52): ExtensiÃ³n de inicio Full Trust.
   - [`packaging/Package.appxmanifest#L60`](../../packaging/Package.appxmanifest#L60): `<rescap:Capability Name="runFullTrust" />`.
   - Proceso AppContainer ausente.
2. **Doctrina Oficial de Microsoft Learn:**
   - SegÃºn [*MSIX Containerization Overview*](https://learn.microsoft.com/en-us/windows/msix/msix-containerization-overview), los procesos Full Trust operan con nivel de integridad media (*medium integrity*). La redirecciÃ³n de AppData hacia `Packages\<PFN>\LocalCache` aplica a procesos AppContainer/virtualizados. En aplicaciones Full Trust, las operaciones sobre AppData siguen la ruta normal del sistema de archivos (*pass-through*).
3. **ComposiciÃ³n de DI en CÃ³digo:**
   - En [`Program.cs#L632-L643`](../../src/PrivLock.Desktop/Program.cs#L632-L643), los tres stores se instancian sin consumir `IEditionProfileResolver`:
     - `FileStateStore` -> [`StorageMigrationHelper.GetDefaultDataDirectory()`](../../src/PrivLock.Infrastructure.Common/Storage/FileStateStore.cs#L26) -> `%LOCALAPPDATA%\PrivGuard\state.json`.
     - `FilePrivacySessionStore` -> `.../Recovery/privacy-session-v1.json` y `privacy-session-v1.json.bak`.
     - `FileActivePrivacySessionMarker` -> [`GetWindowsActiveMarkerDirectory()`](../../src/PrivLock.Desktop/Program.cs#L740-L747) -> `%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker`.
   - **D-05 no estÃ¡ integrada en DI**.
4. **ConclusiÃ³n EstÃ¡tica Correcta:**
   - Para esta configuraciÃ³n especÃ­fica, la expectativa tÃ©cnica mÃ¡s fundamentada es que **la ediciÃ³n Store y la ediciÃ³n Standalone accedan a las mismas ubicaciones fÃ­sicas en `%LOCALAPPDATA%`**. La comparticiÃ³n o colisiÃ³n fÃ­sica es la hipÃ³tesis con mayor respaldo formal. ProcMon en VM confirmarÃ¡ empÃ­ricamente este comportamiento.

### 4.2 Alcance Real de la Cobertura de `installer/setup.iss`
Al auditar directamente el desinstalador Inno Setup ([`installer/setup.iss#L18-L21`](../../installer/setup.iss#L18-L21) y [`lines 501-506`](../../installer/setup.iss#L501-L506)):
- `RecoveryRelativePath` (`PrivGvard\Recovery`): comprueba la existencia de `active-session-v1.marker` y del diario `privacy-session-v1.json` (con `.bak`). **Cubre el marcador activo del cÃ³digo vigente**.
- `ModernRecoveryRelativePath` (`PrivGuard\Recovery`): comprueba diario y backup, **no el marcador activo**.
- `setup.iss` enumera los perfiles interactivos desde `ProfileList` en HKLM y bloquea la desinstalaciÃ³n (*fail-closed*) si detecta un diario o marcador activo.
- **LÃ­mite Estricto:** Esta protecciÃ³n es exclusiva del instalador Standalone Inno Setup. **No tiene intervenciÃ³n ni capacidad para bloquear la desinstalaciÃ³n del paquete MSIX por parte de Windows**.

### 4.3 CuantificaciÃ³n de Commits (`9f856c0..e6a57ae`)
El historial se divide en:
1. **Rango `9f856c0..d8ca7acc` (5 commits funcionales / CI / merge):** `7066b3b` (Docs 8B.1A), `0a270a1` (Sprint 1.0 D-01/D-02), `391d79e` (Sprint 1.1 Inmutabilidad), `ffc2273` (Merge PR #11), `d8ca7ac` (Sprint 1.2 CI Gate).
2. **Rango `d8ca7acc..4d674ebd` (12 commits exclusivamente documentales):** `dded99c`, `b9d4b8f`, `07e4188`, `653be31` (baseline de cÃ³digo auditado), `3f08f12`, `54fb446` (`54fb44670c55...`), `aedd19c` (`aedd19c819dd...`), `fc345eb`, `11a7e49`, `405f948d`, `2f1281b0` y `4d674ebd933f70edd2a1be789df2e3414e1b59f0`.
3. **Commit funcional + documental R4 en `origin/main`:** `86d6adcef9234b579e9313215673cf540e9fa52b` (remediaciÃ³n de reintento FUS y tests unitarios deterministas; parent directo de R5).
4. **Commit funcional + documental R5 en `origin/main`:** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (endurecimiento del contrato de reintento FUS, preservaciÃ³n de signatura pÃºblica, diagnÃ³sticos de timeout diferenciados y tests con mutexes reales `Local\`; parent directo de R6).
5. **Commit documental R6 en `origin/main`:** `e6a57ae58d7d0cd30265834fc51e2bab32928732` (reconciliaciÃ³n post-push R5, verificaciÃ³n CI Run #101 y sincronizaciÃ³n canÃ³nica; HEAD remoto canÃ³nico verificado en Run #102).

### 4.4 Mutex Global, DACL, Dos Mutexes en Guard, Release-Order Race y MÃ¡quina de Estados FUS
La auditorÃ­a estÃ¡tica adversarial sobre los objetos de sincronizaciÃ³n y control de instancia en Windows (`net10.0-windows10.0.19041.0`) establece las siguientes precisiones tÃ©cnicas rigurosas:

#### 1. ImplementaciÃ³n de .NET 10 y Win32 `CreateMutexExW`:
- En [`WindowsSingleInstanceGuard.cs#L40`](../../src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L40) y [`line 49`](../../src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L49), el cÃ³digo invoca:
  ```csharp
  _uninstallGate = new Mutex(initiallyOwned: false, _uninstallGateMutexName);
  _mutex = new Mutex(initiallyOwned: false, _mutexName);
  ```
- SegÃºn el runtime de .NET 10 (`Mutex.Windows.cs`), el constructor invoca `CreateMutexExW(IntPtr.Zero, lpName, 0, (uint)rights)` con derechos `MAXIMUM_ALLOWED | SYNCHRONIZE | MUTEX_MODIFY_STATE` (`0x02100001`) y `NamedWaitHandleOptions.None` (sin `CurrentUserOnly` ni `CurrentSessionOnly`).
- Los objetos residen en el namespace global `\BaseNamedObjects` con la DACL predeterminada del token del usuario creador.

#### 2. La Guarda Utiliza DOS Mutexes con Orden Inverso entre AdquisiciÃ³n y LiberaciÃ³n:
- **Orden de AdquisiciÃ³n en `TryAcquireSingleInstance()`:**
  1. `_uninstallGate` (`Global\PrivLock_UninstallGate`) se adquiere primero.
  2. `_mutex` (`Global\PrivLock_SingleInstance`) se adquiere segundo.
- **Orden de LiberaciÃ³n en `Release()`:**
  ```csharp
  public void Release()
  {
      ReleaseMutexCore(ref _mutex, ref _hasAcquired, "single-instance");
      ReleaseUninstallGateCore();
  }
  ```
  1. `_mutex` (`Global\PrivLock_SingleInstance`) se libera **PRIMERO**.
  2. `_uninstallGate` (`Global\PrivLock_UninstallGate`) se libera **SEGUNDO**.
- **Limpieza en Fallo:** Si falla la adquisiciÃ³n de `_mutex`, se libera `_uninstallGate` de inmediato, se llama a `Dispose()` en ambos y se establecen ambos campos a `null`.

#### 3. DiagnÃ³stico del Llamador (`bool`) vs. Mensajes Internos de Serilog:
- `TryAcquireSingleInstance()` devuelve Ãºnicamente un `bool`. El llamador no puede discernir directamente a partir de dicho booleano si fallÃ³ `UninstallGate`, si fallÃ³ `SingleInstance`, o si se capturÃ³ una excepciÃ³n interna.
- Sin embargo, la implementaciÃ³n de `WindowsSingleInstanceGuard` **sÃ­ genera entradas diferenciadas en Serilog**:
  - Si no adquiere `_uninstallGate`: `Log.Information("PrivGvard cannot start while the uninstall gate is owned.");`
  - Si no adquiere `_mutex`: `Log.Information("Another instance of PrivGvard is already running on Windows.");`
  - Si ocurre excepciÃ³n: `Log.Error(ex, "Failed to create or acquire single-instance mutex on Windows");`
- Por ende, los logs permiten distinguir la causa exacta de contenciÃ³n, aunque el contrato `bool` hacia el llamador sea opaco.

#### 4. Hallazgo Central R3: La Carrera por Orden de LiberaciÃ³n (*Release-Order Race*):
- En `WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire()`:
  - El mutex de sondeo es `instanceMutexName = WindowsSingleInstanceGuard.MutexName` (`Global\PrivLock_SingleInstance`).
  - En Fast User Switching (FUS), el evento `Local\PrivLock_UninstallRequested` de UserA en SesiÃ³n 1 no es visible para UserB en SesiÃ³n 2 (`requestSent = false`).
  - Cuando UserA cierra limpiamente, ejecuta `Release()`:
    1. UserA libera `_mutex` (`SingleInstance`).
    2. UserA aÃºn no ha ejecutado `ReleaseUninstallGateCore()`.
  - **Ventana de IntercalaciÃ³n:**
    1. UserB, que esperaba sobre `instanceMutex` (`SingleInstance`), adquiere el mutex de sondeo.
    2. UserB libera su mutex de sondeo en la lÃ­nea 136 (`instanceMutex.ReleaseMutex()`).
    3. UserB invoca `singleInstanceGuard.TryAcquireSingleInstance()` en la lÃ­nea 137.
    4. La guarda de UserB intenta adquirir primero `Global\PrivLock_UninstallGate`.
    5. **`UninstallGate` todavÃ­a pertenece a UserA**, por lo que `TryAcquireSingleInstance()` retorna `false`.
    6. UserB alcanza la lÃ­nea 140:
       ```csharp
       if (!requestSent)
       {
           error = "The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request.";
           return false;
       }
       ```
    7. Dado que `requestSent == false`, UserB **aborta de inmediato** y devuelve `false` con dicho mensaje de error.
    8. UserA libera `UninstallGate` instantes despuÃ©s, pero UserB ya abortÃ³ el proceso de desinstalaciÃ³n segura.
- **Diferencia con la Carrera de Tercer Proceso:**
  - *Carrera de Tercer Proceso (R2):* Un tercer proceso independiente gana la contenciÃ³n de cualquiera de los mutexes durante el intervalo entre probe release y guard retry.
  - *Release-Order Race (R3):* Ocurre estrictamente entre UserA y UserB debido a que `SingleInstance` se libera antes que `UninstallGate` y el sondeo solo vigila `SingleInstance`.
- **Impacto y Certeza:**
  - Impacto: **Fallo fail-closed de disponibilidad/fiabilidad del handoff**.
  - Certeza: `RACE CONDITION PLAUSIBLE â€” STATICALLY GROUNDED â€” DYNAMIC REPRODUCTION REQUIRED`.

#### 5. Modelado Exhaustivo de Caminos de EjecuciÃ³n en `TryRequestRunningInstanceAndAcquire()`:

- **Camino A (Handoff Exitoso en Misma SesiÃ³n):** SeÃ±al local entregada (`requestSent = true`), UserA libera ambos mutexes, sondeo adquiere, reintento adquiere ambos -> retorna `true` (**Ã‰XITO**).
- **Camino B (Timeout con RetenciÃ³n Activa en Misma SesiÃ³n o FUS):** UserA retiene el mutex durante todo el plazo (`instanceAcquired = false`). Al expirar el timeout:
  - Si `requestSent == true`: retorna `false` con el literal exacto:
    `"The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."`
  - Si `requestSent == false`: retorna `false` con:
    `"No running PrivGvard instance accepted the uninstall shutdown request."`
- **Camino C (LiberaciÃ³n Completa en FUS con Reintento Exitoso):** En FUS (`requestSent == false`), UserA libera completamente AMBOS mutexes antes de que UserB salga de la espera. El sondeo adquiere, libera probe handle, reintento adquiere ambos mutexes -> retorna `true` (**Ã‰XITO**).
- **Camino D1 (Release-Order Race en FUS â€” HALLAZGO CENTRAL R3):** UserB despierta al liberarse `SingleInstance`, pero UserA todavÃ­a retiene `UninstallGate`. El reintento falla al adquirir `UninstallGate`. Con `requestSent == false`, retorna de inmediato `false` con *"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."*
- **Camino D2 (ContenciÃ³n por Tercer Proceso en FUS o Misma SesiÃ³n):** Ambos mutexes fueron liberados, pero un tercer proceso adquiere alguno de ellos entre la lÃ­nea 136 y la lÃ­nea 137. El reintento falla; si `requestSent == false`, retorna de inmediato el error de gate owned.
- **Camino E (DenegaciÃ³n por DACL / Error en Probe Mutex):** Win32 deniega apertura de `Global\PrivLock_SingleInstance`. `new Mutex(...)` en lÃ­nea 106 arroja `UnauthorizedAccessException` sin try-catch local, propagÃ¡ndose a `Program.Main` (lÃ­neas 476â€“495) -> crash report, log fatal, retroalimentaciÃ³n segura y salida con cÃ³digo 1.
- **Camino F (Mutex Abandonado por CaÃ­da de UserA):** `instanceMutex.WaitOne` arroja `AbandonedMutexException` (capturada en lÃ­neas 122â€“126). Se libera probe handle y se reintenta guard. Si guard adquiere -> retorna `true` (**Ã‰XITO**); si no adquiere -> Camino D.

#### 6. RemediaciÃ³n Implementada en R4, Hardening de Contrato en R5 y AnÃ¡lisis Comparativo Antes/DespuÃ©s:

##### A. Contratos Internos y Arquitectura de RemediaciÃ³n (R4 + R5)
Para resolver la *Release-Order Race* y la contenciÃ³n transitoria sin alterar la interfaz pÃºblica multiplataforma `ISingleInstanceGuard` ni perturbar el ciclo de vida de desinstalaciÃ³n de Inno Setup, se implementÃ³ y endureciÃ³ en `PrivLock.Platform.Windows`:
1. **Enum Interno de DiagnÃ³stico (`WindowsSingleInstanceAcquisitionResult`):**
   ```csharp
   internal enum WindowsSingleInstanceAcquisitionResult
   {
       Acquired,
       UninstallGateBusy,
       SingleInstanceBusy,
       AccessOrCreationError
   }
   ```
2. **Interfaz Interna de Guarda Detallada (`IWindowsDetailedSingleInstanceGuard`):**
   ```csharp
   internal interface IWindowsDetailedSingleInstanceGuard
   {
       WindowsSingleInstanceAcquisitionResult TryAcquireSingleInstanceDetail(bool suppressLogging = false);
   }
   ```
3. **Comportamiento en `WindowsSingleInstanceGuard`:**
   - Implementa `IWindowsDetailedSingleInstanceGuard`.
   - `TryAcquireSingleInstanceDetail` evalÃºa secuencialmente `_uninstallGate` y `_mutex`. Si no puede adquirir `_uninstallGate`, retorna `UninstallGateBusy`. Si no puede adquirir `_mutex`, libera `_uninstallGate` y retorna `SingleInstanceBusy`. Si ocurre una excepciÃ³n de creaciÃ³n/seguridad, retorna `AccessOrCreationError`.
   - Soporta el flag `suppressLogging: true` para evitar inundar los registros de Serilog durante los reintentos en bucle.
   - El mÃ©todo estÃ¡ndar `TryAcquireSingleInstance()` consume `TryAcquireSingleInstanceDetail(suppressLogging: false) == WindowsSingleInstanceAcquisitionResult.Acquired`, manteniendo 100% de compatibilidad binaria y semÃ¡ntica previa.
4. **PreservaciÃ³n de Signatura PÃºblica y ValidaciÃ³n de Intervalos en `WindowsUninstallRequestListener` (R5-01 y R5-02):**
   - **R5-02:** Se preserva la signatura pÃºblica original de 5 parÃ¡metros de `TryRequestRunningInstanceAndAcquire(...)`, que delega en una sobrecarga `internal` de 6 parÃ¡metros con `TimeSpan pollInterval` para pruebas.
   - **R5-01:** `ValidatePositiveFiniteInterval` valida `timeout` y `pollInterval` antes de abrir cualquier handle de sincronizaciÃ³n. Rechaza con `ArgumentOutOfRangeException` valores `<= TimeSpan.Zero`, intervalos sub-milisegundo (`< 1 ms`), `Timeout.InfiniteTimeSpan` (`-1 ms`) y valores superiores a `TimeSpan.FromMilliseconds(int.MaxValue)`.
   - Asimismo, dentro del bucle `while (stopwatch.Elapsed < timeout)`, tanto `waitInterval` como `sleepTime` se acotan inferiormente a `MinWaitHandleInterval` (`1 ms`), impidiendo que un remanente `0 < remaining < 1 ms` se trunque a `0 ms` en `WaitOne`/`Thread.Sleep` y provoque un bucle de espera activa (*busy-spin*).
5. **DiagnÃ³sticos de Timeout Diferenciados por Ãšltima Causa de ContenciÃ³n Observada (R5-03):**
   - El bucle registra `lastContentionOutcome` (`UninstallGateBusy` o `SingleInstanceBusy`) en cada intento de la guarda posterior al sondeo.
   - Si el sondeo adquiere `instanceMutex`, pero el reintento de la guarda devuelve `UninstallGateBusy` o `SingleInstanceBusy`, duerme `sleepTime` y reintenta dentro de la ventana global `timeout`.
   - Si la guarda devuelve `AccessOrCreationError`, **termina inmediatamente (*fail-closed*)** sin esperar al timeout, devolviendo `error = "Failed to create or acquire single-instance mutex on Windows."`.
   - Si el plazo expira (`stopwatch.Elapsed >= timeout`), `FormatTimeoutError(requestSent, lastContentionOutcome)` devuelve los literales exactos implementados en [`WindowsUninstallRequestListener.cs#L214-L227`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L214-L227):
     - Con `requestSent == true`: `"The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."`
     - Con `requestSent == false` y `lastContentionOutcome == UninstallGateBusy`: `"The Windows uninstall gate remained busy before the uninstall handoff expired, and no local shutdown request could be delivered (last observed contention: UninstallGateBusy; mutex state is not guaranteed after timeout)."`
     - Con `requestSent == false` y `lastContentionOutcome == SingleInstanceBusy`: `"The Windows single-instance mutex remained busy before the uninstall handoff expired, and no local shutdown request could be delivered (last observed contention: SingleInstanceBusy; mutex state is not guaranteed after timeout)."`
     - Con `requestSent == false` y `lastContentionOutcome == null` (`_`, el mutex de sondeo nunca llegÃ³ a adquirirse): `"Single-instance admission was not obtained before the uninstall handoff expired, and the local shutdown request signal could not be delivered."`
   - Adicionalmente, el bucle `while (stopwatch.Elapsed < timeout)` respeta un plazo suave acotado (*soft deadline*): en cada iteraciÃ³n de espera sobre el mutex de sondeo, calcula `waitInterval = Math.Min(effectivePollInterval, remainingTime)` (donde `remainingTime = timeout - stopwatch.Elapsed`), garantizando que la espera sobre el WaitHandle no exceda el presupuesto remanente del timeout.

##### B. Tabla Comparativa Antes vs. DespuÃ©s para los Caminos Aâ€“G (R3 -> R4 -> R5)

| Camino | Escenario | Comportamiento Previo (R3) | Comportamiento Remediado y Endurecido (R4 + R5) | Veredicto de Mejora |
|---|---|---|---|---|
| **Camino A** | Misma sesiÃ³n, seÃ±al entregada (`requestSent = true`), UserA libera ambos mutexes. | Ã‰xito en 1er intento. Retorna `true`. | Ã‰xito en 1er intento. Retorna `true`. | **Preservado (Sin regresiÃ³n)** |
| **Camino B** | RetenciÃ³n continua del mutex de sondeo durante todo el plazo (misma sesiÃ³n o FUS). | Timeout al expirar. Si `requestSent == false`, devolvÃ­a mensaje que inferÃ­a ausencia de instancia aceptante. | Timeout al expirar. Con `requestSent == true` preserva el mensaje exacto; con `requestSent == false` y `lastContentionOutcome == null` emite diagnÃ³stico neutral sin presuponer si existÃ­a un proceso activo. | **PRECISIÃ“N DIAGNÃ“STICA (R5-03)** |
| **Camino C** | FUS (`requestSent = false`), UserA libera ambos mutexes antes de que UserB sondee. | Ã‰xito en 1er intento. Retorna `true`. | Ã‰xito en 1er intento. Retorna `true`. | **Preservado (Sin regresiÃ³n)** |
| **Camino D1** | **Release-Order Race en FUS:** UserA libera `SingleInstance` antes que `UninstallGate`; UserB sondea antes de que UserA complete `ReleaseUninstallGateCore()`. | **FALLO PREMATURO:** `TryAcquireSingleInstance()` falla; al evaluar `requestSent == false`, **aborta de inmediato** con error de gate owned. | **Ã‰XITO TRAS REINTENTO:** Al recibir `UninstallGateBusy`, duerme `pollInterval` (~100ms); en la siguiente iteraciÃ³n UserA ya liberÃ³ `UninstallGate`, adquiere ambos y retorna `true`. | **RESOLUCIÃ“N COMPLETA (Verificado con mutexes reales `Local\` en R5)** |
| **Camino D2** | **ContenciÃ³n por 3er Proceso (`SingleInstanceBusy`):** Un proceso concurrente adquiere `SingleInstance` entre la liberaciÃ³n del sondeo y el reintento de la guarda. | **FALLO PREMATURO Y DIAGNÃ“STICO FALSO:** Abortaba de inmediato afirmando que `UninstallGate` estaba ocupado. | **RESILIENCIA Y DIAGNÃ“STICO VERAZ:** Reintenta durante la ventana de timeout; si persiste al expirar, reporta `SingleInstanceBusy` como Ãºltima causa observada sin atribuirlo falsamente a `UninstallGate`. | **MEJORA DE DISPONIBILIDAD Y EXACTITUD (R5-03)** |
| **Camino E** | **Error de DACL o CreaciÃ³n:** ExcepciÃ³n de Win32 / Permisos al abrir o crear mutexes en la guarda. | ExcepciÃ³n no estructurada o salida con cÃ³digo 1. | **FAIL-CLOSED DETERMINISTA:** Retorna de inmediato `false` con mensaje de error especÃ­fico sin bucles innecesarios. | **ROBUSTEZ Y FAIL-CLOSED** |
| **Camino F** | **Mutex Abandonado:** CaÃ­da anÃ³mala de UserA dejando mutex de sondeo en estado abandonado. | Sondeo captura `AbandonedMutexException`, libera y prueba guard. | **Preservado y Verificado:** Sondeo captura `AbandonedMutexException`, libera el handle de sondeo antes de invocar la guarda y deja recuento recursivo cero tras `guard.Release()`. | **Preservado y Endurecido en Tests (R5)** |
| **Camino G** | **Gate Ocupado sin Instancia (`UninstallGateBusy`):** Instalador previo retiene gate durante todo el plazo. | Aborta en 1er intento si no hay evento. | Reintenta de forma acotada sin busy-spin sub-milisegundo; al expirar timeout retorna diagnÃ³stico explÃ­cito de `UninstallGateBusy`. | **CONTROL DE CONCURRENCIA (R5-01 / R5-03)** |

##### C. AuditorÃ­a del Borde Temporal de Timeout (R7-04) â€” Dictamen: DOCUMENTED ACCEPTANCE

En la Fase R7 se auditÃ³ adversarialmente la frontera de tiempo de [`WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L131-L193):
1. **Mecanismo de TemporizaciÃ³n en Bucle:**
   - La condiciÃ³n de control del bucle es `while (stopwatch.Elapsed < timeout)`, gobernada por un `Stopwatch.StartNew()` monotÃ³nico.
   - En cada iteraciÃ³n, se calcula `remaining = timeout - stopwatch.Elapsed`. Si `remaining <= TimeSpan.Zero`, se rompe el bucle de inmediato sin intentar nuevas esperas.
   - La ventana de espera sobre el mutex de sondeo se acota mediante `waitInterval = pollInterval <= remaining ? pollInterval : remaining;`, aplicando el lÃ­mite inferior de seguridad `if (waitInterval < MinWaitHandleInterval) waitInterval = MinWaitHandleInterval;` (1 ms).
2. **Matiz de Borde Evaluado:**
   - Si en la Ãºltima iteraciÃ³n `remaining` es inferior a 1 ms (ej. 0.2 ms), `waitInterval` se eleva a 1 ms para evitar `WaitOne(0)` (espera activa/busy spin). Si el mutex de sondeo se adquiere en ese Ãºltimo milisegundo, la llamada posterior `TryAcquireSingleInstanceDetail` puede ejecutarse y retornar `true` habiendo rebasado levemente la ventana nominal `timeout`.
3. **Dictamen Formal: `DOCUMENTED ACCEPTANCE` (OpciÃ³n A):**
   - El timeout de handoff en sincronizaciÃ³n interproceso en modo usuario de Windows opera por definiciÃ³n como un **deadline aproximado acotado por sondeo (*monotonic polling-bounded timeout*)**, sujeto a la granularidad del temporizador del sistema operativo (~15.6 ms en Windows estÃ¡ndar) y al quantum mÃ­nimo de 1 ms.
   - Un llamador de handoff de desinstalaciÃ³n (Inno Setup o script de cierre) utiliza plazos del orden de segundos (ej. 5 s) con el propÃ³sito de garantizar que ninguna instancia quede activa antes de remover archivos; no requiere garantÃ­as de tiempo real duro (*hard real-time*) sub-milisegundo.
   - Abortar con falso negativo una desinstalaciÃ³n cuando la instancia previa liberÃ³ legÃ­timamente la guarda dentro de la ventana de espera introducirÃ­a un fallo de disponibilidad injustificado.
   - Por tanto, se documenta expresamente esta semÃ¡ntica en el contrato arquitectÃ³nico y se mantiene el cÃ³digo de producciÃ³n 100% estable sin modificaciones funcionales ni dependencias temporales artificiales.

---

## 5. DiagnÃ³stico de Readiness del Entorno y Modelo de Capas

### 5.1 ParÃ¡metros DiagnÃ³sticos Observados (Solo Lectura)

| ParÃ¡metro DiagnÃ³stico | Valor Observado | InterpretaciÃ³n Factual Rigurosa |
|---|---|---|
| **OS / Build** | Windows NT 10.0.26300.0 (64-bit) | Windows 11 Enterprise Evaluation (Build 26300). |
| **Manufacturer / Model** | `innotek GmbH` / `VirtualBox` | **Confirma que el entorno actual es un Guest de VirtualBox**. |
| **HypervisorPresent** | `True` | El SO invitado detecta un hipervisor expuesto; no acredita hardware del host fÃ­sico L0. |
| **CPU Virtual** | Intel Core i7-7700HQ @ 2.80GHz (4 vCPUs) | Procesador virtual asignado a este guest. |
| **VirtualizationFirmwareEnabled** | `False` | **VirtualizaciÃ³n anidada no habilitada / no expuesta** a este guest. |
| **Extensiones SLAT / Monitor** | `False`, `False` | Extensiones de traducciÃ³n de segundo nivel no expuestas al guest. |
| **Memoria RAM** | 3.98 GB utilizables (~0.39 GB libres) | Memoria asignada a este guest; insuficiente para hospedar otra VM. |
| **Disco C:** | 24.9 GB libres (de 78.9 GB totales) | Espacio ajustado para albergar imÃ¡genes de disco virtual anidadas. |
| **TPM / SecureBoot** | Acceso denegado (Standard user token) | No modificable desde el contexto de usuario estÃ¡ndar. |
| **Hipervisores Locales** | `vmms`, Hyper-V, VirtualBox, VMware ausentes | NingÃºn hipervisor operativo dentro del guest. |
| **ClasificaciÃ³n Oficial** | **`GUEST DETECTED â€” L0 NOT VERIFIED`** | **El entorno no puede actuar como anfitriÃ³n de VM de pruebas.** |

### 5.2 Modelo de TopologÃ­as y Estimaciones de Recursos

```mermaid
flowchart TD
    subgraph TopologiaA ["OpciÃ³n A: Nested Virtualization (3 Capas) â€” Compleja y con Alto Overhead"]
        L0_A["Host FÃ­sico del Propietario<br/>(EstimaciÃ³n recomendada: 24â€“32 GB RAM fÃ­sica y VT-x en BIOS)"]
        L1_A["Guest Actual de VirtualBox (L1)<br/>(Requiere nested VT-x activo y 12â€“16 GB vRAM asignados)"]
        L2_A["VM de Prueba Windows 11 (L2)<br/>(4â€“8 GB vRAM asignados dentro de L1)"]
        L0_A -->|VirtualBox| L1_A
        L1_A -->|Hyper-V anidado| L2_A
    end

    subgraph TopologiaB ["OpciÃ³n B: VM de Laboratorio Directa en Host FÃ­sico (2 Capas) â€” RECOMENDADA"]
        L0_B["Host FÃ­sico del Propietario<br/>(EstimaciÃ³n recomendada: ~16 GB RAM fÃ­sica total y SSD con 100 GB libres)"]
        L1_B["VM de Prueba Windows 11 (L1)<br/>(2 vCPU, 4â€“8 GB vRAM, vTPM 2.0, Secure Boot, Snapshots)"]
        L0_B -->|Hyper-V / VirtualBox / VMware| L1_B
    end
```

- **TopologÃ­a Recomendada:** **TopologÃ­a B**. ProvisiÃ³n de VM directa en el host fÃ­sico del propietario (~16 GB de RAM fÃ­sica total estimada en el anfitriÃ³n), evitando el overhead crÃ­tico y la inestabilidad de la virtualizaciÃ³n anidada en tres capas.

---

## 6. Cadena de Custodia CriptogrÃ¡fica del Artefacto MSIX

Se mantiene la separaciÃ³n estricta de hashes entre las etapas del ciclo de vida:

1. **Fase de CompilaciÃ³n (Build Stage â€” Fuera de la VM):**
   - CompilaciÃ³n unsigned con `scripts/build-msix.ps1 -Configuration Release-Store -ValidateStoreIdentity`.
   - Registro en `BUILD-PROVENANCE.json`: commit Git, SDK versions, SHA-256 del `.msix` unsigned, SHA-256 de binarios `win-x64`, y hashes de `AppxManifest.xml` y `resources.pri`.
2. **Fase de Transferencia y Test-Signing (Exclusivamente Dentro del Guest VM):**
   - Transferencia a la VM (en red aislada).
   - VerificaciÃ³n de hash unsigned contra proveniencia de build.
   - Firma con `SignTool.exe` en snapshot `SNAP-02-TEST-CERT` usando certificado autofirmado confinado a la VM.
   - **Registro del nuevo SHA-256 del archivo `.msix` firmado resultante** y Thumbprint del certificado de prueba.

---

## 7. ActualizaciÃ³n del Runbook DinÃ¡mico 8B.1A.7.2 (TEST-01 a TEST-11)

> [!IMPORTANT]
> **Estado de EjecuciÃ³n DinÃ¡mica:** Todas las pruebas dinÃ¡micas (TEST-01 a TEST-11, incluido TEST-09.7) permanecen formalmente en estado **`LAB HOLD / NOT RUN`**. No se han ejecutado en el entorno de desarrollo actual debido a la clasificaciÃ³n `GUEST DETECTED â€” L0 NOT VERIFIED`.

### Gate de Seguridad Previo a Pruebas Destructivas (TEST-06 / TEST-07)
- Red en modo aislado y dispositivos fÃ­sicos (cÃ¡maras/micrÃ³fonos) desconectados/no compartidos.
- ComprobaciÃ³n previa de capacidad de reversiÃ³n al snapshot `SNAP-02-TEST-CERT`.
- Registro previo de estado de registro, endpoints y archivos AppData.

### Secuencia de Pruebas Normativas
1. **TEST-01 (Equivalencia y Rutas FÃ­sicas Efectivas en AppData) â€” `LAB HOLD / NOT RUN`:**
   - Registrar y comparar rutas devueltas por `Environment.GetFolderPath`, `%LOCALAPPDATA%`, `%USERPROFILE%\AppData\Local` y `ProfileImagePath` en `ProfileList\<SID>`.
   - Ejecutar PrivGvard en ambas ediciones (Store y Standalone) y capturar con Process Monitor (ProcMon) las rutas fÃ­sicas efectivas donde se crean y modifican `state.json`, `privacy-session-v1.json` y `active-session-v1.marker`.
   - Inspeccionar `Packages\<PFN>\LocalCache` para documentar presencia o ausencia de virtualizaciÃ³n o carpetas privadas.
2. **TEST-02 (Trazas ProcMon de Operaciones de Archivo) â€” `LAB HOLD / NOT RUN`:**
   - Capturar con Process Monitor llamadas Win32 `CreateFileW`, `WriteFile`, `ReplaceFileW`/renombrado y `DeleteFileW`. Comparar rutas entre Store y Standalone.
3. **TEST-03 (Reemplazo AtÃ³mico `File.Replace`) â€” `LAB HOLD / NOT RUN`:**
   - Comprobar comportamiento y fallbacks de `File.Replace` al persistir el WAL transaccional.
4. **TEST-04 (ConsentStore HKCU en Store Edition) â€” `LAB HOLD / NOT RUN`:**
   - Confirmar que Store Edition lee ConsentStore pero devuelve error y no escribe en el registro (D-02).
5. **TEST-05 (Resiliencia ante MSIX App Repair) â€” `LAB HOLD / NOT RUN`:**
   - Ejecutar `Reparar` desde ConfiguraciÃ³n de Windows. Registrar supervivencia de `state.json`, WAL y endpoints.
6. **TEST-06 (Impacto de MSIX App Reset) â€” `LAB HOLD / NOT RUN`:**
   - ComprobaciÃ³n empÃ­rica: determinar quÃ© datos elimina la acciÃ³n `Restablecer` de Windows para este paquete Full Trust; verificar si los archivos en `%LOCALAPPDATA%` se conservan y confirmar que ningÃºn reset deje un endpoint silenciado sin camino de recuperaciÃ³n.
7. **TEST-07 (Impacto de MSIX Uninstall) â€” `LAB HOLD / NOT RUN`:**
   - Desinstalar el paquete MSIX mediante Windows. Verificar eliminaciÃ³n del paquete, persistencia de datos en AppData, capacidad de reinstalar y recuperar, y estado final de los endpoints.
8. **TEST-08 (Mute de Core Audio y Exclusive Mode) â€” `LAB HOLD / NOT RUN`:**
   - Probar con aplicaciÃ³n en modo exclusivo si la captura es bloqueada o si se produce bypass cuando `HardwareMuteSupport` es `NotSupported`.
9. **TEST-09 (Fast User Switching, Perfiles y Comportamiento del Mutex Global) â€” `LAB HOLD / NOT RUN`:**
   - **Cuatro Estados FÃ­sicos Evaluados:** Mutex Libre (Unowned), Mutex con Propietario Activo, Mutex Abandonado (`AbandonedMutexException`) y Objeto Precreado (Squatting).
   - **TEST-09.1 â€” Identidad y Rutas por Perfil:** SIDs, variables de entorno, Session IDs y rutas fÃ­sicas en `UserA` y `UserB`.
   - **TEST-09.2 â€” Mutex Global entre Sesiones y Permisos DACL:**
     - Iniciar PrivGvard en `UserA` (SesiÃ³n 1). Cambiar a `UserB` vÃ­a FUS (SesiÃ³n 2). Intentar iniciar en `UserB`.
     - *Etapa 1 (Apertura de Handle):* Comprobar si `CreateMutexExW` tiene Ã©xito o si la DACL deniega el acceso arrojando `UnauthorizedAccessException` en el constructor antes de `WaitOne`. Inspeccionar DACL en `\BaseNamedObjects` (WinObj/Process Explorer).
     - *Etapa 2 (SincronizaciÃ³n en `WaitOne(0)` si handle abierto):* Comprobar contenciÃ³n normal (`false`), adquisiciÃ³n (`true`) o abandono.
   - **TEST-09.3 â€” Evento de Handoff `Local\` en FUS (Caminos A a F):**
     - Evaluar experimentalmente en VM los caminos de handoff modelados en la SecciÃ³n 4.4.
   - **TEST-09.4 â€” Resistencia a PrecreaciÃ³n (*Mutex Squatting*):**
     - Proceso no privilegiado precrea `Global\PrivLock_SingleInstance` con DACL restrictiva.
     - *Ruta A (Inicio Normal):* Bloque `catch` en `WindowsSingleInstanceGuard` captura la denegaciÃ³n, registra error en Serilog y retorna `false` (*DoE fail-closed*).
     - *Ruta B (Flujo Safe Uninstall):* `new Mutex(...)` fuera de try-catch en `TryRequestRunningInstanceAndAcquire` arroja `UnauthorizedAccessException`, propagÃ¡ndose al `catch` de `Program.Main`, generando crash report, fatal log, diÃ¡logo de error y exit code 1.
   - **TEST-09.5 â€” Mutex Abandonado y RecuperaciÃ³n de Ownership:** Forzar `taskkill /F` en `UserA`; iniciar `UserB` y verificar adquisiciÃ³n y recuperaciÃ³n sin bloqueos huÃ©rfanos.
   - **TEST-09.6 â€” Impacto Funcional, Fallback Fail-Closed y Observabilidad:** Registrar fallbacks fail-closed en logs de Serilog (`%LOCALAPPDATA%\PrivGvard\Logs\`).
   - **TEST-09.7 â€” FUS Handoff Release-Order Race (SingleInstance Released Before UninstallGate) â€” `LAB HOLD / NOT RUN`:**
     - **Objetivo:** Verificar en una VM aislada multi-sesiÃ³n con Fast User Switching (FUS) el comportamiento del handoff de desinstalaciÃ³n segura cuando el proceso de `UserB` despierta sobre `Global\PrivLock_SingleInstance` inmediatamente despuÃ©s de que `UserA` lo libera pero antes de que `UserA` libere `Global\PrivLock_UninstallGate`, distinguiendo el baseline histÃ³rico de regresiÃ³n pre-fix (R3) del criterio de aceptaciÃ³n post-remediaciÃ³n (R4/R5).
     - **Precondiciones:**
       1. VM Windows 11 aislada en host fÃ­sico L0 con snapshots limpios (`SNAP-02-TEST-CERT`).
       2. Dos cuentas estÃ¡ndar de prueba: `UserA` y `UserB`.
       3. Dos sesiones interactivas independientes activas simultÃ¡neamente mediante Fast User Switching.
       4. Una cuenta administrativa separada para instrumentaciÃ³n y depuraciÃ³n.
       5. InstrumentaciÃ³n de procesos, handles (`\BaseNamedObjects`) y logs de Serilog disponible.
       6. Dispositivos de cÃ¡mara y micrÃ³fono virtuales o descartables (sin hardware fÃ­sico).
     - **Escenario Controlado (MÃ©todo de Pausa):**
       - Se pausarÃ¡ el proceso de `UserA` inmediatamente entre:
         ```csharp
         ReleaseMutexCore(ref _mutex, ref _hasAcquired, "single-instance");
         // PAUSA CONTROLADA AQUÃ
         ReleaseUninstallGateCore();
         ```
       - Se implementa en laboratorio mediante una sesiÃ³n de depuraciÃ³n (breakpoint en Visual Studio / WinDbg) o un harness de laboratorio aislado, sin introducir hooks en cÃ³digo de producciÃ³n.
       - `UserB` ejecuta Ãºnicamente el mÃ©todo de handoff (`TryRequestRunningInstanceAndAcquire`), sin invocar el desinstalador Inno ni borrar archivos.
     - **Pasos de la Prueba Futura:**
       1. Iniciar PrivGvard en `UserA` (SesiÃ³n 1) y confirmar que posee ambos mutexes globales (`Global\PrivLock_UninstallGate` y `Global\PrivLock_SingleInstance`).
       2. Cambiar a `UserB` (SesiÃ³n 2) mediante FUS sin cerrar `UserA`.
       3. Confirmar que `UserB` no puede abrir el evento `Local\PrivLock_UninstallRequested` de `UserA` (`requestSent == false`).
       4. Dejar a `UserB` esperando sobre `Global\PrivLock_SingleInstance` en `TryRequestRunningInstanceAndAcquire`.
       5. En `UserA`, iniciar el cierre y pausar la ejecuciÃ³n inmediatamente despuÃ©s de liberar `SingleInstance` y antes de liberar `UninstallGate`.
       6. Permitir que `UserB` despierte del sondeo, libere el handle de sondeo e invoque `TryAcquireSingleInstanceDetail` mientras `UserA` permanece pausado reteniendo `UninstallGate`.
       7. Capturar: resultado de `WaitOne` del sondeo, liberaciÃ³n del handle de sondeo, retorno `UninstallGateBusy` en el primer intento de la guarda, ausencia de spam de logs en Serilog (`suppressLogging: true`), y PIDs/SIDs/Session IDs de ambos procesos.
       8. SegÃºn la secciÃ³n bajo prueba (6.1 o 6.2), observar el desenlace antes y despuÃ©s de reanudar `UserA`.
       9. Reanudar `UserA` y confirmar la liberaciÃ³n de `UninstallGate`.
       10. Restaurar snapshot limpio de la VM.
     - **SecciÃ³n 6.1 â€” Comportamiento HistÃ³rico Pre-Fix (R3 â€” Baseline de RegresiÃ³n):**
       - En un binario compilado en el commit `4d674ebd` (anterior a R4):
         1. `UserA` libera `SingleInstance` y queda pausado reteniendo `UninstallGate`.
         2. `UserB` adquiere el mutex de sondeo, lo libera y llama a `TryAcquireSingleInstance()`.
         3. La guarda de `UserB` falla en `_uninstallGate.WaitOne(0)`.
         4. Al ser `requestSent == false`, `UserB` **aborta de inmediato en el primer intento** devolviendo `false` con `"The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."` antes de que `UserA` reanude.
     - **SecciÃ³n 6.2 â€” Comportamiento Esperado Post-Fix en CÃ³digo Actual (R4/R5 â€” Criterio de AceptaciÃ³n):**
       - En un binario compilado desde el cÃ³digo actual (R4/R5):
         1. `UserA` libera `SingleInstance` y queda pausado reteniendo `UninstallGate`.
         2. `UserB` adquiere el mutex de sondeo, lo libera y llama a `TryAcquireSingleInstanceDetail(suppressLogging: true)`.
         3. La guarda de `UserB` devuelve `UninstallGateBusy`.
         4. **`UserB` NO falla inmediatamente**; registra `lastContentionOutcome = UninstallGateBusy`, duerme `pollInterval` (~100ms) y reintenta dentro de la ventana monotÃ³nica de `timeout`.
         5. Cuando `UserA` se reanuda y libera `UninstallGate` dentro del `timeout`, el siguiente reintento de `UserB` adquiere **ambos** mutexes (`UninstallGate` + `SingleInstance`) y `TryRequestRunningInstanceAndAcquire()` **retorna `true`** con `error == string.Empty`.
     - **SecciÃ³n 6.3 â€” Controles Negativos Post-Fix (R5):**
       - *Control 09.7-A (LiberaciÃ³n Completa Previa):* `UserA` libera ambos mutexes antes de que `UserB` salga de la espera. Resultado esperado: primer intento adquiere ambos y retorna `true`.
       - *Control 09.7-B (RetenciÃ³n de `UninstallGate` Durante Todo el Timeout):* `UserA` permanece pausado reteniendo `UninstallGate` hasta que expira `timeout`. Resultado esperado: `UserB` reintenta de forma acotada y retorna `false` con diagnÃ³stico que identifica `last observed contention: UninstallGateBusy`.
       - *Control 09.7-C (ContenciÃ³n en `SingleInstance` por Tercer Proceso hasta Timeout):* `UninstallGate` estÃ¡ libre pero un tercer proceso retiene `SingleInstance` hasta agotar `timeout`. Resultado esperado: `UserB` libera `UninstallGate` en cada intento fallido y al expirar retorna `false` con diagnÃ³stico que identifica `last observed contention: SingleInstanceBusy` (sin afirmar falsamente que `UninstallGate` estÃ¡ ocupado).
       - *Control 09.7-D (DenegaciÃ³n de DACL / Error de CreaciÃ³n):* Una DACL restrictiva impide crear/adquirir los mutexes en la guarda (`AccessOrCreationError`). Resultado esperado: terminaciÃ³n fail-closed inmediata sin agotar `timeout`.
       - *Control 09.7-E (RetenciÃ³n Continua de `SingleInstance` Sin Llegar a Adquirir el Sondeo):* `UserA` retiene `SingleInstance` durante todo el plazo sin liberarlo nunca. Resultado esperado: con `requestSent == false`, retorna `false` con el diagnÃ³stico neutral `"Timed out waiting for Windows single-instance admission after no running PrivGvard instance accepted the uninstall shutdown request."`
     - **Criterios de Dictamen en VM (CÃ³digo Actual R5):**
       - **PASS (AceptaciÃ³n de la remediaciÃ³n R4/R5):** Se verifica empÃ­ricamente la SecciÃ³n 6.2 (`UserB` sobrevive a la pausa intermedia reintentando ante `UninstallGateBusy` y retorna `true` al liberar `UserA` el `UninstallGate` dentro del plazo) junto con los controles negativos 6.3.
       - **FAIL (RegresiÃ³n):** `UserB` aborta en el primer intento mientras `timeout` sigue vigente, o devuelve un diagnÃ³stico que atribuye errÃ³neamente `SingleInstanceBusy` a `UninstallGate`.
       - **INCONCLUSIVE:** La ejecuciÃ³n en laboratorio no logra aislar la ventana temporal entre ambas liberaciones.
10. **TEST-10 (Coexistencia con EdiciÃ³n Standalone y JurisdicciÃ³n de Desinstaladores) â€” `LAB HOLD / NOT RUN`:**
    - Concurrencia Store/Standalone en misma sesiÃ³n y entre sesiones; ejecuciÃ³n secuencial e inspecciÃ³n de rutas en AppData; handoff de desinstalaciÃ³n; y ratificaciÃ³n de que Inno Setup carece de autoridad sobre el paquete MSIX.
11. **TEST-11 (RecuperaciÃ³n tras CaÃ­da Simulada) â€” `LAB HOLD / NOT RUN`:**
    - Forzar terminaciÃ³n del proceso (`taskkill /F`) con marcador activo presente; reiniciar y verificar recuperaciÃ³n automÃ¡tica del WAL.

### 7.2 Cobertura de Pruebas Unitarias e Intra-SesiÃ³n Automatizadas (Fase 8B.1A.7.2-R6)

> [!NOTE]
> **DelimitaciÃ³n Estricta de Evidencia Automatizada:** Los tests prefijados con `SimulatedGuard_` o `FakeGuard_` ejercitan determinÃ­sticamente la mÃ¡quina de estados mediante dobles de prueba. Los tests prefijados con `RealNamedMutexes_` utilizan **mutexes con nombre reales de Windows en `Local\`** a travÃ©s de hilos dedicados del sistema operativo dentro de la sesiÃ³n de logon actual. **Ninguna prueba automatizada intra-sesiÃ³n simula Fast User Switching (FUS) entre sesiones distintas de Windows, tokens/SIDs de mÃºltiples usuarios ni fronteras DACL en `Global\`, los cuales permanecen sujetos a `TEST-09` / `TEST-09.7` (`LAB HOLD / NOT RUN`).**

#### A. Tests en `WindowsUninstallRequestListenerTests.cs` (16 mÃ©todos / 20 casos ejecutables)

| Test Automatizado (`[Fact]` / `[Theory]`) | Tipo de Guarda / Primitiva | PropÃ³sito / Invariante Verificado | Resultado |
|---|---|---|---|
| `PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature` | ReflexiÃ³n de Metadatos | **R5-02:** Verifica que existe un Ãºnico mÃ©todo pÃºblico `TryRequestRunningInstanceAndAcquire` con exactamente los 5 parÃ¡metros originales y valores por defecto intactos. | **SUPERADO** |
| `PollIntervalValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex` (3 casos) | ValidaciÃ³n Pre-Handle | **R5-01:** Verifica que `pollInterval` con `0 ms`, `-1 ms` (`Timeout.InfiniteTimeSpan`) o `-250 ms` lanza `ArgumentOutOfRangeException("pollInterval")` inmediatamente sin crear el mutex de sondeo ni invocar la guarda. | **SUPERADO** |
| `PollIntervalValidation_SubMillisecondOrExceedingWaitHandleLimit_ThrowsArgumentOutOfRangeImmediately` (2 casos) | ValidaciÃ³n Pre-Handle | **R5-01:** Verifica que intervalos sub-milisegundo (`TimeSpan.FromTicks(100)`, truncables a `0 ms`) o `TimeSpan.MaxValue` lanzan `ArgumentOutOfRangeException("pollInterval")` de inmediato. | **SUPERADO** |
| `TimeoutValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex` (3 casos) | ValidaciÃ³n Pre-Handle | **R5-01:** Verifica que `timeout` con `0 ms`, `-1 ms` (`Timeout.InfiniteTimeSpan`) o `-500 ms` lanza `ArgumentOutOfRangeException("timeout")` sin abrir handles. | **SUPERADO** |
| `RunningInstanceReceivesSafeShutdownRequestAndReleasesSingleInstanceAdmission` | `WindowsSingleInstanceGuard` real (`Local\`) | Handoff normal en misma sesiÃ³n (`requestSent == true`): instancia activa libera ambos mutexes y el solicitante adquiere ambos. | **SUPERADO** |
| `FakeGuardNeverAcquires_WithoutRunningInstance_TimesOutWithSingleInstanceBusyDiagnostic` | `NeverAcquireSingleInstanceGuard` + Mutex de sondeo real (`Local\`) | **R5-03:** Guarda opaca que nunca adquiere (`requestSent == false`): el timeout atribuye la contenciÃ³n a `SingleInstanceBusy` y **nunca** afirma falsamente que `UninstallGate` estÃ¡ ocupado. | **SUPERADO** |
| `RunningInstanceThatDoesNotReleaseAdmissionTimesOutExplicitly` | `NeverAcquireSingleInstanceGuard` + Listener real (`Local\`) | Con `requestSent == true` y mutex retenido, preserva intacto el mensaje `"The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."` | **SUPERADO** |
| `SimulatedGuard_NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired` | `ContentionThenSuccessGuard` (fake) | SimulaciÃ³n determinista de mÃ¡quina de estados para contenciÃ³n transitoria en `UninstallGateBusy` (2 fallos) seguida de Ã©xito (`Acquired`). | **SUPERADO** |
| `SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin` | `ContentionCountingGuard` (fake) | Verifica fail-closed con diagnÃ³stico `UninstallGateBusy` y ausencia de *busy-spin* (intentos acotados a 8-9 gracias al lÃ­mite inferior `MinWaitHandleInterval = 1 ms`). | **SUPERADO** |
| `SimulatedGuard_NoEventPermanentSingleInstanceContention_FailsClosedWithSingleInstanceDiagnostic` | `ContentionCountingGuard` (fake) | **R5-03:** ContenciÃ³n permanente `SingleInstanceBusy`: falla al expirar timeout reportando `SingleInstanceBusy` sin mencionar `UninstallGate`. | **SUPERADO** |
| `SimulatedGuard_MixedContentionCauses_TimeoutDiagnosticReflectsLastObservedContention` | `SequenceContentionGuard` (fake) | **R5-03:** Secuencias mixtas (`UninstallGateBusy` -> `SingleInstanceBusy` y viceversa): el diagnÃ³stico final refleja con veracidad la Ãºltima causa observada. | **SUPERADO** |
| `RealNamedMutexes_WhenProbeMutexNeverAcquiredAndNoEvent_TimesOutWithNeutralDiagnostic` | Hilo reteniendo Mutex real (`Local\`) | **R5-03:** Cuando `requestSent == false` y el mutex de sondeo nunca se libera durante el plazo, devuelve diagnÃ³stico neutral de timeout sin inferir si habÃ­a o no un proceso aceptante. | **SUPERADO** |
| `SimulatedGuard_PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout` | `ContentionCountingGuard` (fake) | Verifica terminaciÃ³n *fail-closed* inmediata en el 1er intento (sin bucle ni espera hasta timeout) ante `AccessOrCreationError`. | **SUPERADO** |
| `SimulatedGuard_ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout` | `ContentionThenSuccessGuard` (fake) | SimulaciÃ³n determinista de contenciÃ³n transitoria por 3er proceso (`SingleInstanceBusy`) y recuperaciÃ³n posterior en intento 2. | **SUPERADO** |
| `AbandonedMutex_OnProbeHandle_ReleasesProbeOwnershipBeforeGuardAndLeavesNoResidualLockAfterRelease` | Hilo del SO abandonando Mutex (`Local\`) + guarda real | Verifica desde un hilo separado que tras capturar `AbandonedMutexException`, el handle de sondeo se libera antes de invocar la guarda y deja recuento recursivo cero tras `guard.Release()`. | **SUPERADO** |
| `RealNamedMutexes_ReleaseOrderInterleaving_RetriesWhileUninstallGateHeldAndAcquiresBothWhenGateReleased` | **Dos hilos del SO con mutexes reales `Local\`** | **ReproducciÃ³n intra-sesiÃ³n real de la ventana R3/R4:** Hilo A libera `SingleInstance` pero retiene `UninstallGate`; Hilo B despierta del sondeo, observa `UninstallGateBusy` sobre el mutex real, reintenta, y cuando Hilo A libera `UninstallGate`, Hilo B adquiere ambos mutexes reales y los libera limpiamente. | **SUPERADO** |

#### B. Nuevos Tests con Mutexes Reales `Local\` en `WindowsSingleInstanceStartupTests.cs` (3 nuevos `[Fact]`)

| Test Automatizado (`[Fact]`) | Primitivas | PropÃ³sito / Invariante Verificado | Resultado |
|---|---|---|---|
| `RealNamedMutexes_WhenUninstallGateOwnedByAnotherThread_ReturnsUninstallGateBusyAndLeavesMainMutexFree` | Mutexes reales `Local\` en hilos separados del SO | Cuando otro hilo posee `UninstallGate`, `TryAcquireSingleInstanceDetail` devuelve `UninstallGateBusy` y deja el mutex principal libre. | **SUPERADO** |
| `RealNamedMutexes_WhenMainMutexOwnedByAnotherThread_ReturnsSingleInstanceBusyAndReleasesGate` | Mutexes reales `Local\` en hilos separados del SO | Cuando `UninstallGate` estÃ¡ libre pero otro hilo posee `SingleInstance`, `TryAcquireSingleInstanceDetail` devuelve `SingleInstanceBusy` y **libera inmediatamente `UninstallGate`** (verificado desde un hilo separado). | **SUPERADO** |
| `RealNamedMutexes_WhenBothFree_AcquiresBothAndReleaseAllowsSubsequentAcquisition` | Mutexes reales `Local\` en hilos separados del SO | Cuando ambos mutexes estÃ¡n libres, adquiere ambos (`Acquired`), bloquea a otros hilos, y tras `Release()` ambos quedan libres para otro hilo. | **SUPERADO** |

- **Resumen de EjecuciÃ³n Local de Pruebas (`dotnet test PrivGvard.sln -c Release`):**
  - `PrivLock.Domain.Tests`: **25 superados**, 0 con error, 0 omitidos (91 ms).
  - `PrivLock.Infrastructure.Tests`: **90 superados**, 0 con error, 0 omitidos (1 s).
  - `PrivLock.Application.Tests`: **167 superados**, 0 con error, 0 omitidos (1 s).
  - `PrivLock.Platform.Windows.Tests`: **334 superados**, 0 con error, 0 omitidos (14 s).
  - **Total de la SoluciÃ³n:** **616 superados**, **0 con error**, **0 omitidos**.

---

## 8. PreservaciÃ³n Estricta de Decisiones ArquitectÃ³nicas y Salvaguardas

- **DecisiÃ³n D-01:** **Aprobada**. Manifiesto Store contiene exclusivamente `runFullTrust`. Prohibidas `allowElevation` y `unvirtualizedResources`.
- **DecisiÃ³n D-02:** **Aprobada con condiciones**. Escritura en `ConsentStore` prohibida en Store Edition. Lectura permitida. RedirecciÃ³n informativa a `ms-settings:privacy-webcam`.
- **DecisiÃ³n D-03:** **AprobaciÃ³n parcial**. Aislamiento conceptual de almacenamiento acordado; integraciÃ³n efectiva de rutas en DI y coordinaciÃ³n en tiempo de ejecuciÃ³n pendientes de prueba en VM.
- **DecisiÃ³n D-04:** **Pendiente** de formalizaciÃ³n.
- **DecisiÃ³n D-05:** **Pendiente** de formalizaciÃ³n e integraciÃ³n en los stores reales.
- **Microsoft Store:** **`CONDITIONAL HOLD`** (publicaciÃ³n bloqueada).
- **Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`** (desarrollo prohibido).
- **Fase 8B.1A.7.2 / R7:** **ReconciliaciÃ³n y Gate L0 completados â€” `PASS â€” R6 REMOTE STATE RECONCILED â€” CI R6 PASS â€” L0 NOT VERIFIED â€” LAB HOLD`** (pruebas fÃ­sicas dinÃ¡micas multi-sesiÃ³n retenidas a la espera del aprovisionamiento por el propietario en el host fÃ­sico L0 segÃºn TopologÃ­a B).

---

## 9. PrÃ³ximos Pasos Recomendados

1. **Preflight en Host FÃ­sico (L0):** El propietario del repositorio debe ejecutar [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1) en el equipo fÃ­sico anfitriÃ³n para confirmar fabricante, CPU, memoria fÃ­sica libre y habilitaciÃ³n de virtualizaciÃ³n en firmware (VT-x/AMD-V).
2. **Aprovisionamiento de la VM Aislada (TopologÃ­a B):** Configurar la mÃ¡quina virtual Windows 11 segÃºn las directrices de [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) (2 vCPUs, 4â€“8 GB vRAM, vTPM 2.0, Secure Boot, red interna aislada y snapshots limpios).
3. **GeneraciÃ³n de Artefactos con Doble Proveniencia:** Tras verificar L0, compilar el paquete Release-Store desde el HEAD canÃ³nico aprobado (`e6a57ae58d7d0cd30265834fc51e2bab32928732`) y registrar la proveniencia criptogrÃ¡fica antes de iniciar el runbook dinÃ¡mico (`TEST-09.7`, `TEST-09`, `TEST-10` y `TEST-01..TEST-11`).
4. **Gobernanza del Repositorio en GitHub (RecomendaciÃ³n Documental):** Configurar la protecciÃ³n de la rama `main` en GitHub Settings (requerimiento de PR y status checks asociados a `windows-latest` y MSIX packaging) sin realizar cambios en esta sesiÃ³n.
5. **ConsolidaciÃ³n Documental Local R7:** Mantener la rama local `docs/8b1a7-2-r7-post-r6-reconciliation` bajo polÃ­tica estricta de ZERO PUSH y ZERO PR en esta sesiÃ³n.

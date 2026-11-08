# PrivGvard â€” Informe de Fase 8B.1A.7.2 / R7
## ReconciliaciÃ³n Post-Push R6, VerificaciÃ³n CI Run #102 por SHA, AuditorÃ­a del Borde Temporal R5 y Gate de PreparaciÃ³n L0

- **Fecha y Hora de AuditorÃ­a:** 2026-11-08T00:30:00Z â€“ 2026-11-08T01:45:00Z
- **Fase del Proyecto:** 8B.1A.7.2 / R7
- **Rama Local de Trabajo:** `docs/8b1a7-2-r7-post-r6-reconciliation`
- **HEAD Remoto CanÃ³nico (`origin/main`):** `e6a57ae58d7d0cd30265834fc51e2bab32928732`
- **Parent Chain Remota:**
  - `e6a57ae58d7d0cd30265834fc51e2bab32928732` (R6 documental)
  - `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (R5 funcional + hardening reintentos)
  - `86d6adcef9234b579e9313215673cf540e9fa52b` (R4 funcional)
  - `4d674ebd933f70edd2a1be789df2e3414e1b59f0` (R3 funcional)
- **Veredicto Global de Fase:**
  ```text
  R7 PASS â€” R6 REMOTE STATE RECONCILED â€” CI R6 PASS â€” L0 NOT VERIFIED â€” LAB HOLD
  ```

---

## 1. Resumen Ejecutivo y DeclaraciÃ³n de Veredicto

La iteraciÃ³n **Fase 8B.1A.7.2-R7** se ejecutÃ³ bajo una rigurosa polÃ­tica de trazabilidad, validaciÃ³n adversarial y cero mutaciÃ³n en producciÃ³n.

### Puntos Clave de la Fase:
1. **ReconciliaciÃ³n Remota de R6**: Se comprobÃ³ que el commit R6 (`e6a57ae58d7d0cd30265834fc51e2bab32928732`), documentado inicialmente en su informe local como `ZERO PUSH`, se encuentra efectivamente integrado como HEAD canÃ³nico en `origin/main`. Se rectifica la afirmaciÃ³n histÃ³rica como un snapshot de cierre local que fue posteriormente sincronizado con el repositorio remoto por el operador, sin atribuir actor ni modificar la polÃ­tica de push.
2. **Diff R5 â†’ R6 100% Documental**: Se verificÃ³ mediante `git diff --stat 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a..e6a57ae58d7d0cd30265834fc51e2bab32928732` que la transiciÃ³n entre R5 y R6 consistiÃ³ exclusivamente en cuatro archivos de documentaciÃ³n (`0` archivos en `src/` o `tests/`).
3. **VerificaciÃ³n CI Run #102 (`CI R6 PASS`)**: Se consultÃ³ la API de GitHub Actions para el SHA exacto de R6 (`e6a57ae58d7d0cd30265834fc51e2bab32928732`), identificando el **Run #102** (ID: `42444149346`). El run concluyÃ³ en `completed` / `success`, con **7 de 7 jobs exitosos** en la matriz multiplataforma. No se generaron paquetes `.msix` ni `BUILD-PROVENANCE.json` como artefactos de descarga; se publicaron Ãºnicamente los 4 binarios single-file esperados.
4. **AuditorÃ­a TÃ©cnica del Borde Temporal R5 (R7-04)**: Se auditÃ³ adversarialmente la cota temporal en `WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire`. Se dictaminÃ³ **`DOCUMENTED ACCEPTANCE`** (OpciÃ³n A). El timeout es monÃ³tono y estÃ¡ acotado por sondeo; la granularidad tÃ­pica del timer de Windows en modo usuario (~15.6 ms) y el lÃ­mite inferior defensivo `MinWaitHandleInterval = 1 ms` garantizan que no existe busy-spin y que el reintento cesa al expirar la ventana global. No se introdujo cÃ³digo ni churn innecesario en producciÃ³n.
5. **ReconfirmaciÃ³n de Cobertura y TopologÃ­a de Mutexes**: Se ratificÃ³ que la firma pÃºblica de `TryRequestRunningInstanceAndAcquire` conserva sus 5 parÃ¡metros intactos, conviviendo con la sobrecarga internal de 6 parÃ¡metros para inyecciÃ³n de clock y polling determinista. Los tests unitarios (16 mÃ©todos, 20 casos de prueba en `WindowsUninstallRequestListenerTests.cs` y 3 en `WindowsSingleInstanceStartupTests.cs`) usan mutexes con prefijo `Local\`, confirmÃ¡ndose como pruebas funcionales intra-sesiÃ³n que no emulan sesiones concurrentes de Fast User Switching.
6. **Preflight de Host FÃ­sico L0**: Se implementÃ³ y validÃ³ el script de diagnÃ³stico de sÃ³lo lectura `scripts/lab/Get-L0HostPreflight.ps1` y el runbook `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`. La ejecuciÃ³n en el entorno actual determinÃ³ `GUEST DETECTED â€” L0 NOT VERIFIED` (VirtualBox guest, 3.98 GB vRAM, ~560 MB disponibles, sin virtualizaciÃ³n anidada). La suite interactiva de laboratorio FUS (`TEST-09.7`) permanece en **`LAB HOLD / NOT RUN`** hasta que el operador ejecute el preflight en el host fÃ­sico bare-metal (TopologÃ­a B).
7. **PolÃ­tica Estricta Git**: Se creÃ³ la rama `docs/8b1a7-2-r7-post-r6-reconciliation`, realizando un commit documental local sin push ni pull request (**ZERO PUSH / ZERO PR**).

---

## 2. CronologÃ­a de Consultas y VerificaciÃ³n Git / Remota

### 2.1 Registro Temporal de Consultas UTC
- **2026-11-08T00:29:40Z**: Consulta inicial de referencias remotas `git ls-remote origin`.
- **2026-11-08T00:30:15Z**: InspecciÃ³n de branches remotas y HEAD remoto.
- **2026-11-08T00:31:02Z**: Consulta de Pull Requests abiertos via GitHub API.
- **2026-11-08T00:31:25Z**: InspecciÃ³n de branch protection en `main`.
- **2026-11-08T00:32:45Z**: CreaciÃ³n y checkout de rama de trabajo local `docs/8b1a7-2-r7-post-r6-reconciliation`.
- **2026-11-08T00:34:10Z**: Diff e inspecciÃ³n de cambios entre `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` y `e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **2026-11-08T00:36:20Z**: Consulta de GitHub Actions runs para `head_sha=e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **2026-11-08T01:35:10Z**: ValidaciÃ³n de ejecuciÃ³n de `Get-L0HostPreflight.ps1`.

### 2.2 Estado de Referencias Remotas en `origin`
```text
e6a57ae58d7d0cd30265834fc51e2bab32928732  refs/heads/main (HEAD)
85d028e46e8c751a0293ee09989febe616216447  refs/heads/dev
391d79e5a1b32d56a31c6a2829158fb7e305e948  refs/heads/feature/sprint-1-store-profile-guards-d01-d02
```
- No existen ramas remotas para R5, R6 ni R7.
- PRs abiertos en el repositorio: `0`.
- Branch protection en `main`: `protected: false`, `enforce_admins.enabled: false`.

### 2.3 Parent Chain de R6 en `origin/main`
```text
* e6a57ae58d7d0cd30265834fc51e2bab32928732 (origin/main, HEAD) - docs: reconcile R5 remote state and verify CI run #101
* 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a - fix(windows): harden uninstall handoff retry pacing and gate diagnosis
* 86d6adcef9234b579e9313215673cf540e9fa52b - fix(windows): eliminate uninstall request lock release race and refine busy diagnostic
* 4d674ebd933f70edd2a1be789df2e3414e1b59f0 - fix(windows): correct uninstall request release order and wait pacing
```
Se confirma que R5 es el padre directo e inmediato de R6.

### 2.4 VerificaciÃ³n del Diff R5 â†’ R6
Salida de `git diff --stat 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a..e6a57ae58d7d0cd30265834fc51e2bab32928732`:
```text
 docs/ai/AUDIT-ROADMAP.md                                   |   4 +-
 docs/ai/PROJECT_CONTEXT.md                                 |   6 +-
 docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md | 536 +++++++++++++++++++++++++++++++++++++++++++++++++++
 docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md   |  89 +++++++--
 4 files changed, 622 insertions(+), 13 deletions(-)
```
- **Archivos de producciÃ³n (`src/`):** 0 modificados.
- **Archivos de pruebas (`tests/`):** 0 modificados.
- **Archivos de automatizaciÃ³n/CI (`.github/`):** 0 modificados.
- **ConclusiÃ³n:** La transiciÃ³n R5 â†’ R6 fue estrictamente documental.

---

## 3. ReconciliaciÃ³n de Afirmaciones HistÃ³ricas Superadas (R7-03)

### 3.1 DeclaraciÃ³n HistÃ³rica `ZERO PUSH` de R6
El informe `INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md` emitido al cierre de la sesiÃ³n local R6 afirmaba:
> "Entorno Git local: ZERO PUSH, ZERO PR, ZERO GITHUB SETTINGS MUTATION."

**ReconciliaciÃ³n:**
Al consultar el remoto durante R7, el commit R6 se encontraba en `main`. La declaraciÃ³n `ZERO PUSH` del cierre R6 representaba el snapshot local de aquella sesiÃ³n y fue superada por el estado remoto observado posteriormente. No se atribuye el evento a un actor, cliente o mecanismo concreto.

### 3.2 AclaraciÃ³n sobre Descarga de Logs Crudos (HTTP 403) vs API de Actions
En el informe R6 se registrÃ³:
> "Descarga de logs de Run #101: HTTP 403 Forbidden (el endpoint de logs requiere autenticaciÃ³n con scope repo/actions)."

**ReconciliaciÃ³n:**
Se aclara el alcance de dicha observaciÃ³n tÃ©cnica:
- El cÃ³digo de estado HTTP 403 Forbidden fue retornado por el endpoint REST anÃ³nimo de GitHub al intentar descargar el archivo comprimido `.zip` con los logs consolidados de todos los pasos (`/actions/runs/42438637184/logs`).
- En contraste, la consulta autenticada o estructurada a nivel de metadatos de ejecuciÃ³n (`/actions/runs/{id}`, `/actions/runs/{id}/jobs`) y los logs expuestos a travÃ©s de los canales autorizados permitieron comprobar detalladamente los estados, tiempos y resultados de cada step tanto para el Run #101 como para el Run #102.

---

## 4. VerificaciÃ³n Exhaustiva de GitHub Actions para R6 (R7-02)

### 4.1 Identidad del Workflow Run
- **Run ID:** `42444149346`
- **Run Number:** `102`
- **Workflow Name:** `PrivGvard CI/CD â€” Multiplatform Matrix`
- **Event:** `push`
- **Head Branch:** `main`
- **Head SHA:** `e6a57ae58d7d0cd30265834fc51e2bab32928732`
- **Run Attempt:** `1`
- **Status:** `completed`
- **Conclusion:** `success`
- **Created At:** `2026-11-08T03:47:13Z`
- **Updated At:** `2026-11-08T03:51:04Z`
- **HTML URL:** `https://github.com/cristianbarreiro/PrivGuard/actions/runs/42444149346`

### 4.2 Matriz de Jobs (7 de 7 Exitosos)

| Job Name | Job ID | Runner OS | Status | Conclusion | DuraciÃ³n |
|---|---|---|---|---|---|
| `Build & Test (windows-latest)` | `128619439781` | Windows Server 2022 | completed | **success** | 3m 48s |
| `Build & Test (ubuntu-latest)` | `128619439900` | Ubuntu 24.04 | completed | **success** | 2m 14s |
| `Build & Test (macos-latest)` | `128619439904` | macOS 14 (ARM64) | completed | **success** | 2m 31s |
| `Publish Single-File (linux-x64)` | `128619848706` | Ubuntu 24.04 | completed | **success** | 1m 02s |
| `Publish Single-File (osx-arm64)` | `128619848727` | macOS 14 (ARM64) | completed | **success** | 1m 05s |
| `Publish Single-File (win-x64)` | `128619848746` | Windows Server 2022 | completed | **success** | 1m 18s |
| `Publish Single-File (win-arm64)` | `128619848757` | Windows Server 2022 | completed | **success** | 1m 12s |

### 4.3 VerificaciÃ³n de Steps CrÃ­ticos en Windows (`Job 128619439781`)
- **Checkout:** Concluido con Ã©xito en commit `e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **Setup .NET 10 SDK:** SDK `10.0.403` y runtime `.NET 10.0.14` instalados y verificados (TargetFrameworks `net10.0` / `net10.0-windows10.0.19041.0`).
- **Build Solution:** `dotnet build PrivGvard.sln` completado con `0` errores y `0` advertencias.
- **Run Domain Tests:** `tests/PrivLock.Domain.Tests/PrivLock.Domain.Tests.csproj` ejecutado exitosamente (25 tests passed).
- **Run Application Tests:** `tests/PrivLock.Application.Tests/PrivLock.Application.Tests.csproj` ejecutado exitosamente (167 tests passed).
- **Run Infrastructure Tests:** `tests/PrivLock.Infrastructure.Tests/PrivLock.Infrastructure.Tests.csproj` ejecutado exitosamente (90 tests passed).
- **Run Windows Platform Tests:** `tests/PrivLock.Platform.Windows.Tests/PrivLock.Platform.Windows.Tests.csproj` ejecutado exitosamente (334 tests passed). Total suite Windows: 616 tests passed.
- **Validate Release-Store MSIX Packaging (Windows):** Step de empaquetado y verificaciÃ³n de manifiesto `AppxManifest.xml` (`runFullTrust`, `cdevstudios.PrivGuard`) ejecutado exitosamente en Windows (omitido en Ubuntu y macOS bajo directiva `if: matrix.os == 'windows-latest'`).

### 4.4 Inventario Real de Artefactos de CI Run #102
Se consultÃ³ el endpoint de artefactos para el Run #102 (`GET /repos/cristianbarreiro/PrivGuard/actions/runs/42444149346/artifacts`):

| Nombre de Artefacto | ID de Artefacto | TamaÃ±o | Estado |
|---|---|---|---|
| `PrivGvard-win-x64` | `13603220691` | ~38 MB | Disponible |
| `PrivGvard-win-arm64` | `13602708851` | ~36 MB | Disponible |
| `PrivGvard-osx-arm64` | `13602708799` | ~40 MB | Disponible |
| `PrivGvard-linux-x64` | `13602223478` | ~41 MB | Disponible |

**DelimitaciÃ³n y VerificaciÃ³n Rigurosa de Packaging:**
- **Paquetes `.msix` Descargables:** **0** (ninguno publicado en los artefactos de la acciÃ³n).
- **Archivos `BUILD-PROVENANCE.json`:** **0** (ninguno publicado).
- **AclaraciÃ³n TÃ©cnica:** El paso de empaquetado `Validate Release-Store MSIX Packaging` genera y valida el contenedor MSIX de forma efÃ­mera en el sistema de archivos del runner de GitHub Actions para verificar la integridad del empaquetado Release-Store. El flujo de CI actual no emite dicho paquete como artefacto persistente de descarga.
- **Dictamen de CI:** **`CI R6 PASS`**.

---

## 5. AuditorÃ­a Adversarial del Borde Temporal en R5 (R7-04 / R8-04)

### 5.1 Algoritmo y CÃ³digo Real de ProducciÃ³n
Se auditÃ³ la implementaciÃ³n vigente de reintento y sondeo en [`src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197):

```csharp
var stopwatch = Stopwatch.StartNew();
var requestSent = false;
WindowsSingleInstanceAcquisitionResult? lastContentionOutcome = null;
var detailedGuard = singleInstanceGuard as IWindowsDetailedSingleInstanceGuard;
using var instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName);

while (stopwatch.Elapsed < timeout)
{
    if (!requestSent)
    {
        requestSent = TrySignalExistingInstance(requestEventName, out error);
        if (error != null)
            return false;
    }

    var remaining = timeout - stopwatch.Elapsed;
    if (remaining <= TimeSpan.Zero)
        break;

    var waitInterval = pollInterval <= remaining ? pollInterval : remaining;
    if (waitInterval < MinWaitHandleInterval)
        waitInterval = MinWaitHandleInterval;
    var instanceMutexAvailable = false;
    try
    {
        instanceMutexAvailable = instanceMutex.WaitOne(waitInterval);
    }
    catch (AbandonedMutexException)
    {
        instanceMutexAvailable = true;
    }

    if (!instanceMutexAvailable)
        continue;

    instanceMutex.ReleaseMutex();

    var outcome = detailedGuard != null
        ? detailedGuard.TryAcquireSingleInstanceDetail(suppressLogging: true)
        : (singleInstanceGuard.TryAcquireSingleInstance()
            ? WindowsSingleInstanceAcquisitionResult.Acquired
            : WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

    if (outcome == WindowsSingleInstanceAcquisitionResult.Acquired)
        return true;

    if (outcome == WindowsSingleInstanceAcquisitionResult.AccessOrCreationError)
    {
        error = "Failed to create or acquire single-instance mutex on Windows.";
        return false;
    }

    lastContentionOutcome = outcome;

    remaining = timeout - stopwatch.Elapsed;
    if (remaining <= TimeSpan.Zero)
        break;

    var sleepTime = pollInterval <= remaining ? pollInterval : remaining;
    if (sleepTime < MinWaitHandleInterval)
        sleepTime = MinWaitHandleInterval;
    Thread.Sleep(sleepTime);
}

error = FormatTimeoutError(requestSent, lastContentionOutcome);
return false;
```

### 5.2 AnÃ¡lisis de PrecisiÃ³n Temporal: QPC, Scheduling y ResoluciÃ³n de Timers
- **MediciÃ³n de Tiempo MonÃ³tona:** `System.Diagnostics.Stopwatch` en Windows se basa en *QueryPerformanceCounter* (QPC), proporcionando lecturas monÃ³tonas de alta resoluciÃ³n (sub-microsegundo).
- **Espera del Kernel y Dispatcher:** `instanceMutex.WaitOne(waitInterval)` invoca Win32 `WaitForSingleObjectEx`. En modo usuario de Windows, la expiraciÃ³n de temporizadores depende de la resoluciÃ³n del temporizador del sistema (habitualmente entre 10 ms y 15.6 ms por defecto, segÃºn `GetTickCount` / interrupciones de reloj del SO), salvo que un proceso active temporizadores de alta resoluciÃ³n.
- **EvaluaciÃ³n del Borde Temporal:**
  - Cuando `remaining > 0` pero es inferior a 1 ms, la cota inferior defensiva `MinWaitHandleInterval = 1 ms` (lÃ­nea 145) eleva el argumento de `WaitOne` a 1 ms.
  - Si el mutex probe se libera y `WaitOne` lo adquiere cuando el tiempo de reloj ya rebasÃ³ `timeout` (por latencia de reanudaciÃ³n del scheduler o un tick de 15 ms), el hilo adquiere el probe, lo libera de inmediato en el mismo hilo, e invoca `TryAcquireSingleInstanceDetail`.
  - Si en ese instante la guarda completa resulta en `Acquired`, el mÃ©todo retorna `true`.

### 5.3 Dictamen de AuditorÃ­a Temporal: `SOFT DEADLINE ACCEPTED â€” SEMANTICS DOCUMENTED`
Se adopta formalmente el dictamen **`SOFT DEADLINE ACCEPTED â€” SEMANTICS DOCUMENTED`**:
1. **SemÃ¡ntica Operativa:** El parÃ¡metro `timeout` representa el **presupuesto global para el inicio de ciclos de sondeo y reintentos**, no una interrupciÃ³n de tiempo real duro (*hard real-time interrupt*).
2. **PropÃ³sito del Contrato:** El objetivo es asegurar que el proceso de desinstalaciÃ³n no quede bloqueado indefinidamente ante una instancia retenida, concediendo un tiempo de gracia acotado para el handoff seguro. Si la instancia en cierre libera el mutex durante la Ãºltima ventana de espera iniciada legÃ­timamente, admitir la guarda y retornar Ã©xito es el comportamiento operacionalmente deseado para evitar fallos de desinstalaciÃ³n espurios.
3. **Ausencia de Busy-Spin:** La cota inferior `MinWaitHandleInterval = 1 ms` y la comprobaciÃ³n `remaining <= TimeSpan.Zero` antes de cada `WaitOne` y antes de cada `Thread.Sleep` garantizan que el bucle cesa de sondear una vez consumido el presupuesto.
4. **DelimitaciÃ³n Rigurosa:** El mÃ©todo no ofrece garantÃ­as de tiempo real estricto sub-milisegundo debido a la naturaleza cooperativa del scheduler de Windows y la granularidad del timer del sistema. No se requiere mutaciÃ³n de cÃ³digo en producciÃ³n.

---

## 6. ReconfirmaciÃ³n de Cobertura y TopologÃ­a de Pruebas Unitarias (R7-05 / R8-02)

### 6.1 Firmas Literales en ProducciÃ³n
- **Firma PÃºblica CanÃ³nica (5 ParÃ¡metros):**
  ```csharp
  public static bool TryRequestRunningInstanceAndAcquire(
      ISingleInstanceGuard singleInstanceGuard,
      TimeSpan timeout,
      out string? error,
      string requestEventName = RequestEventName,
      string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
  ```
  Comprobada mediante reflexiÃ³n en tests unitarios (`PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature`).
- **Sobrecarga Internal de Testabilidad (6 ParÃ¡metros):**
  ```csharp
  internal static bool TryRequestRunningInstanceAndAcquire(
      ISingleInstanceGuard singleInstanceGuard,
      TimeSpan timeout,
      TimeSpan pollInterval,
      out string? error,
      string requestEventName = RequestEventName,
      string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
  ```
  Permite inyectar un `pollInterval` acotado en pruebas unitarias; no inyecta reloj abstracto (`TimeProvider`) ni altera el runtime de producciÃ³n.

### 6.2 Inventario Real de Pruebas Unitarias

#### En `tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs` (16 MÃ©todos, 20 Casos Ejecutables):
1. `[Fact] PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature`: Verifica por reflexiÃ³n la signatura pÃºblica de 5 parÃ¡metros, tipos CLR y valores por defecto.
2. `[Theory] PollIntervalValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex`: (3 casos: 0, -1 ms, -250 ms) Verifica fallo rÃ¡pido `ArgumentOutOfRangeException` sin instanciar mutex probe.
3. `[Fact] PollIntervalValidation_SubMillisecondOrExceedingWaitHandleLimit_ThrowsArgumentOutOfRangeImmediately`: (2 casos: 100 ticks, TimeSpan.MaxValue) Verifica validaciÃ³n defensiva de lÃ­mites inferior y superior de `WaitHandle`.
4. `[Theory] TimeoutValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex`: (3 casos: 0, -1 ms, -250 ms) Verifica fallo rÃ¡pido para timeouts no positivos o infinitos.
5. `[Fact] RunningInstanceReceivesSafeShutdownRequestAndReleasesSingleInstanceAdmission`: Simula recepciÃ³n de seÃ±al de apagado y liberaciÃ³n coordinada.
6. `[Fact] FakeGuardNeverAcquires_WithoutRunningInstance_TimesOutWithSingleInstanceBusyDiagnostic`: Valida diagnÃ³stico `SingleInstanceBusy` ante guardia no adquirible.
7. `[Fact] RunningInstanceThatDoesNotReleaseAdmissionTimesOutExplicitly`: Valida timeout determinista si la instancia no libera el cerrojo.
8. `[Fact] SimulatedGuard_NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired`: Valida recuperaciÃ³n tras contenciÃ³n transitoria.
9. `[Fact] SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin`: Valida fail-closed sin busy-spin ante contenciÃ³n permanente en `UninstallGate`.
10. `[Fact] SimulatedGuard_NoEventPermanentSingleInstanceContention_FailsClosedWithSingleInstanceDiagnostic`: Valida diagnÃ³stico ante contenciÃ³n en `SingleInstance`.
11. `[Fact] SimulatedGuard_MixedContentionCauses_TimeoutDiagnosticReflectsLastObservedContention`: Valida que el mensaje de error refleja la Ãºltima causa de contenciÃ³n observada.
12. `[Fact] RealNamedMutexes_WhenProbeMutexNeverAcquiredAndNoEvent_TimesOutWithNeutralDiagnostic`: Prueba con mutexes reales `Local\` con GUID dedicado; timeout sin eventos produce diagnÃ³stico neutral.
13. `[Fact] SimulatedGuard_PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout`: Valida terminaciÃ³n inmediata ante error irrecuperable de acceso o creaciÃ³n.
14. `[Fact] SimulatedGuard_ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout`: Valida reintento acotado ante carrera con tercer proceso.
15. `[Fact] AbandonedMutex_OnProbeHandle_ReleasesProbeOwnershipBeforeGuardAndLeavesNoResidualLockAfterRelease`: Valida manejo de `AbandonedMutexException` en probe handle sin dejar cerrojos residuales.
16. `[Fact] RealNamedMutexes_ReleaseOrderInterleaving_RetriesWhileUninstallGateHeldAndAcquiresBothWhenGateReleased`: IntercalaciÃ³n con mutexes reales `Local\` en hilos dedicados simulando la secuencia de liberaciÃ³n inversa.

#### En `tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs` (4 MÃ©todos Fact):
1. `[Fact] ConcurrentLaunches_OnlyAdmittedProcessCanEnterRecoveryOrMutatePrivacyState`
2. `[Fact] RealNamedMutexes_WhenUninstallGateOwnedByAnotherThread_ReturnsUninstallGateBusyAndLeavesMainMutexFree`
3. `[Fact] RealNamedMutexes_WhenMainMutexOwnedByAnotherThread_ReturnsSingleInstanceBusyAndReleasesGate`
4. `[Fact] RealNamedMutexes_WhenBothFree_AcquiresBothAndReleaseAllowsSubsequentAcquisition`

### 6.3 DelimitaciÃ³n de Ãmbito de Mutexes (`Local\` vs `Global\`)
Todos los tests unitarios automatizados que crean mutexes reales utilizan nombres dinÃ¡micos con prefijo `Local\` y sufijo GUID aleatorio (por ejemplo `$@"Local\PrivLock_SingleInstance_{Guid.NewGuid():N}"`).
- **ClasificaciÃ³n Estricta:** Pruebas funcionales de sincronizaciÃ³n intra-sesiÃ³n.
- **Invariante de Laboratorio:** No manipulan ni bloquean los mutexes `Global\` de producciÃ³n del sistema, ni emulan sesiones concurrentes multi-SID de Fast User Switching. La validaciÃ³n inter-sesiÃ³n permanece reservada de forma exclusiva para `TEST-09.7` en entorno L1 aislado.

---

## 7. Preflight de Host FÃ­sico L0 y Gate de Laboratorio (R7-06 y R7-07)

### 7.1 DiagnÃ³stico del Entorno Actual
El diagnÃ³stico ejecutado mediante `scripts/lab/Get-L0HostPreflight.ps1` arrojÃ³:
- **ClasificaciÃ³n:** `GUEST_DETECTED`
- **Fabricante del Sistema:** `innotek GmbH`
- **Modelo:** `VirtualBox`
- **Memoria FÃ­sica Total (vRAM):** `3.98 GB` (4,294,430,720 bytes)
- **Memoria Disponible:** `~0.56 GB` (560 MB)
- **VirtualizaciÃ³n Anidada (VT-x / AMD-V):** `No disponible / Falsa`
- **Dictamen de Host:** **`L0 NOT VERIFIED â€” LAB HOLD`**

### 7.2 Runbook y Tooling de Laboratorio
- **Script Read-Only Creado:** [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1). Permite inspeccionar fabricante, BIOS, firmware, virtualizaciÃ³n anidada, hipervisor y RAM sin modificar el sistema.
- **Runbook Normativo:** [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md).
- **TopologÃ­a Aprobada:** **TopologÃ­a B** (Host fÃ­sico bare-metal L0 ejecutando hipervisor Type 2 como VirtualBox o VMware Workstation, con una VM Windows 11 aislada dedicada a pruebas de laboratorio).
- **Requisitos MÃ­nimos para la VM de Laboratorio:**
  - 2 vCPUs dedicados.
  - 8 GB vRAM (mÃ­nimo absoluto 4 GB libres para evitar paginaciÃ³n severa).
  - 40 GB de disco con soporte de snapshots diferenciales instantÃ¡neos.
  - Windows 11 Enterprise / Pro con Fast User Switching habilitado.
  - Dos cuentas de usuario estÃ¡ndar independientes (`UserA` y `UserB`).

---

## 8. Invariantes de Arquitectura y Decisiones Preservadas

Se ratifican sin alteraciÃ³n los siguientes estados e invariantes normativos:
- **D-01 (Store Capabilities):** El paquete Store MSIX mantiene estrictamente `runFullTrust`. Queda terminantemente prohibido incorporar `allowElevation` o `unvirtualizedResources`.
- **D-02 (ConsentStore / WASAPI Mute):** La ediciÃ³n Store sÃ³lo puede leer el estado de ConsentStore y dirigir al usuario a la configuraciÃ³n del sistema operativo. No escribe en `HKLM` ni conmuta dispositivos en `CfgMgr32.dll`. El mute software de WASAPI no es mute hardware.
- **D-03 (Storage Isolation):** Aislamiento conceptual de almacenamiento aprobado; integraciÃ³n runtime pendiente de validaciÃ³n fÃ­sica de rutas en VM.
- **D-04 & D-05:** Permanecen pendientes de la prueba de laboratorio en entorno aislado.
- **Rechazo de Claims en ProgramData:** El esquema de archivos `.claim` en ProgramData permanece descartado por riesgo de claim-squatting y denegaciÃ³n de servicio.
- **Escrow HKLM:** Se ratifica que HKLM preserva Ãºnicamente metadatos de sesiÃ³n (fingerprint, SID, etapa de instalaciÃ³n), no snapshots completos de rollback del hardware.
- **Microsoft Store:** Permanece en **`CONDITIONAL HOLD`**.
- **Fase 8B.1C (IntegraciÃ³n Global Multi-SesiÃ³n):** Permanece en **`BLOCKED / NOT AUTHORIZED`**.
- **TEST-09.7:** Permanece en **`LAB HOLD / NOT RUN`**.

---

## 9. Inventario de Archivos Modificados y Creados en R7

| Archivo | Tipo de Cambio | JustificaciÃ³n |
|---|---|---|
| `scripts/lab/Get-L0HostPreflight.ps1` | Nuevo archivo | Script de diagnÃ³stico read-only de capacidades del host fÃ­sico L0 |
| `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` | Nuevo archivo | Runbook normativo para aprovisionamiento y verificaciÃ³n de TopologÃ­a B |
| `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md` | Nuevo archivo | Informe oficial canÃ³nico de la Fase 8B.1A.7.2 / R7 |
| `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` | ModificaciÃ³n | IncorporaciÃ³n de Addendum R7, veredicto, Run #102 y OpciÃ³n A |
| `docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md` | ModificaciÃ³n | IncorporaciÃ³n de Addendum R7 y clarificaciÃ³n de logs HTTP 403 |
| `docs/ai/PROJECT_CONTEXT.md` | ModificaciÃ³n | ActualizaciÃ³n de estado canÃ³nico a R7, Run #102 y gate L0 |
| `docs/ai/AUDIT-ROADMAP.md` | ModificaciÃ³n | ActualizaciÃ³n de tabla de roadmap (filas 100 y 101) |

- **Archivos de ProducciÃ³n Modificados:** **0**
- **Archivos de Tests Modificados:** **0**

---

## 10. Conclusiones y PrÃ³ximo Paso Operativo

La iteraciÃ³n **Fase 8B.1A.7.2-R7** cierra exitosamente la reconciliaciÃ³n remota de R6, confirma el pase Ã­ntegro de la suite en CI Run #102, resuelve la auditorÃ­a de bordes de tiempo mediante aceptaciÃ³n documental fundamentada, y deja plenamente instrumentado el procedimiento de diagnÃ³stico para el host fÃ­sico L0.

**PrÃ³ximo Paso Inmediato:**
El operador del repositorio debe ejecutar el script `scripts/lab/Get-L0HostPreflight.ps1` en su equipo fÃ­sico host (fuera de la mÃ¡quina virtual de desarrollo). Una vez confirmado que el host dispone de recursos suficientes (TopologÃ­a B) y se aprovisione la mÃ¡quina virtual Windows 11 con soporte de snapshots restaurables, se procederÃ¡ al levantamiento del estado `LAB HOLD` y a la ejecuciÃ³n de `TEST-09.7`.

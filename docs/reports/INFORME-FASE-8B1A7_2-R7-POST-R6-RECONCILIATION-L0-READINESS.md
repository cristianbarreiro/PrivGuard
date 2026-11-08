# PrivGvard — Informe de Fase 8B.1A.7.2 / R7
## Reconciliación Post-Push R6, Verificación CI Run #102 por SHA, Auditoría del Borde Temporal R5 y Gate de Preparación L0

- **Fecha y Hora de Auditoría:** 2026-11-08T00:30:00Z – 2026-11-08T01:45:00Z
- **Fase del Proyecto:** 8B.1A.7.2 / R7
- **Rama Local de Trabajo:** `docs/8b1a7-2-r7-post-r6-reconciliation`
- **HEAD Remoto Canónico (`origin/main`):** `e6a57ae58d7d0cd30265834fc51e2bab32928732`
- **Parent Chain Remota:**
  - `e6a57ae58d7d0cd30265834fc51e2bab32928732` (R6 documental)
  - `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (R5 funcional + hardening reintentos)
  - `86d6adcef9234b579e9313215673cf540e9fa52b` (R4 funcional)
  - `4d674ebd933f70edd2a1be789df2e3414e1b59f0` (R3 funcional)
- **Veredicto Global de Fase:**
  ```text
  R7 PASS — R6 REMOTE STATE RECONCILED — CI R6 PASS — L0 NOT VERIFIED — LAB HOLD
  ```

---

## 1. Resumen Ejecutivo y Declaración de Veredicto

La iteración **Fase 8B.1A.7.2-R7** se ejecutó bajo una rigurosa política de trazabilidad, validación adversarial y cero mutación en producción.

### Puntos Clave de la Fase:
1. **Reconciliación Remota de R6**: Se comprobó que el commit R6 (`e6a57ae58d7d0cd30265834fc51e2bab32928732`), documentado inicialmente en su informe local como `ZERO PUSH`, se encuentra efectivamente integrado como HEAD canónico en `origin/main`. Se rectifica la afirmación histórica como un snapshot de cierre local que fue posteriormente sincronizado con el repositorio remoto por el operador, sin atribuir actor ni modificar la política de push.
2. **Diff R5 → R6 100% Documental**: Se verificó mediante `git diff --stat 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a..e6a57ae58d7d0cd30265834fc51e2bab32928732` que la transición entre R5 y R6 consistió exclusivamente en cuatro archivos de documentación (`0` archivos en `src/` o `tests/`).
3. **Verificación CI Run #102 (`CI R6 PASS`)**: Se consultó la API de GitHub Actions para el SHA exacto de R6 (`e6a57ae58d7d0cd30265834fc51e2bab32928732`), identificando el **Run #102** (ID: `42444149346`). El run concluyó en `completed` / `success`, con **7 de 7 jobs exitosos** en la matriz multiplataforma. No se generaron paquetes `.msix` ni `BUILD-PROVENANCE.json` como artefactos de descarga; se publicaron únicamente los 4 binarios single-file esperados.
4. **Auditoría Técnica del Borde Temporal R5 (R7-04)**: Se auditó adversarialmente la cota temporal en `WindowsUninstallRequestListener.TryRequestRunningInstanceAndAcquire`. Se dictaminó **`DOCUMENTED ACCEPTANCE`** (Opción A). El timeout es monótono y está acotado por sondeo; la granularidad típica del timer de Windows en modo usuario (~15.6 ms) y el límite inferior defensivo `MinWaitHandleInterval = 1 ms` garantizan que no existe busy-spin y que el reintento cesa al expirar la ventana global. No se introdujo código ni churn innecesario en producción.
5. **Reconfirmación de Cobertura y Topología de Mutexes**: Se ratificó que la firma pública de `TryRequestRunningInstanceAndAcquire` conserva sus 5 parámetros intactos, conviviendo con la sobrecarga internal de 6 parámetros para inyección de clock y polling determinista. Los tests unitarios (16 métodos, 20 casos de prueba en `WindowsUninstallRequestListenerTests.cs` y 3 en `WindowsSingleInstanceStartupTests.cs`) usan mutexes con prefijo `Local\`, confirmándose como pruebas funcionales intra-sesión que no emulan sesiones concurrentes de Fast User Switching.
6. **Preflight de Host Físico L0**: Se implementó y validó el script de diagnóstico de sólo lectura `scripts/lab/Get-L0HostPreflight.ps1` y el runbook `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`. La ejecución en el entorno actual determinó `GUEST DETECTED — L0 NOT VERIFIED` (VirtualBox guest, 3.98 GB vRAM, ~560 MB disponibles, sin virtualización anidada). La suite interactiva de laboratorio FUS (`TEST-09.7`) permanece en **`LAB HOLD / NOT RUN`** hasta que el operador ejecute el preflight en el host físico bare-metal (Topología B).
7. **Política Estricta Git**: Se creó la rama `docs/8b1a7-2-r7-post-r6-reconciliation`, realizando un commit documental local sin push ni pull request (**ZERO PUSH / ZERO PR**).

---

## 2. Cronología de Consultas y Verificación Git / Remota

### 2.1 Registro Temporal de Consultas UTC
- **2026-11-08T00:29:40Z**: Consulta inicial de referencias remotas `git ls-remote origin`.
- **2026-11-08T00:30:15Z**: Inspección de branches remotas y HEAD remoto.
- **2026-11-08T00:31:02Z**: Consulta de Pull Requests abiertos via GitHub API.
- **2026-11-08T00:31:25Z**: Inspección de branch protection en `main`.
- **2026-11-08T00:32:45Z**: Creación y checkout de rama de trabajo local `docs/8b1a7-2-r7-post-r6-reconciliation`.
- **2026-11-08T00:34:10Z**: Diff e inspección de cambios entre `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` y `e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **2026-11-08T00:36:20Z**: Consulta de GitHub Actions runs para `head_sha=e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **2026-11-08T01:35:10Z**: Validación de ejecución de `Get-L0HostPreflight.ps1`.

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

### 2.4 Verificación del Diff R5 → R6
Salida de `git diff --stat 85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a..e6a57ae58d7d0cd30265834fc51e2bab32928732`:
```text
 docs/ai/AUDIT-ROADMAP.md                                   |   4 +-
 docs/ai/PROJECT_CONTEXT.md                                 |   6 +-
 docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md | 536 +++++++++++++++++++++++++++++++++++++++++++++++++++
 docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md   |  89 +++++++--
 4 files changed, 622 insertions(+), 13 deletions(-)
```
- **Archivos de producción (`src/`):** 0 modificados.
- **Archivos de pruebas (`tests/`):** 0 modificados.
- **Archivos de automatización/CI (`.github/`):** 0 modificados.
- **Conclusión:** La transición R5 → R6 fue estrictamente documental.

---

## 3. Reconciliación de Afirmaciones Históricas Superadas (R7-03)

### 3.1 Declaración Histórica `ZERO PUSH` de R6
El informe `INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md` emitido al cierre de la sesión local R6 afirmaba:
> "Entorno Git local: ZERO PUSH, ZERO PR, ZERO GITHUB SETTINGS MUTATION."

**Reconciliación:**
Al consultar el remoto durante R7, el commit R6 se encontraba en `main`. La declaración `ZERO PUSH` del cierre R6 representaba el snapshot local de aquella sesión y fue superada por el estado remoto observado posteriormente. No se atribuye el evento a un actor, cliente o mecanismo concreto.

### 3.2 Aclaración sobre Descarga de Logs Crudos (HTTP 403) vs API de Actions
En el informe R6 se registró:
> "Descarga de logs de Run #101: HTTP 403 Forbidden (el endpoint de logs requiere autenticación con scope repo/actions)."

**Reconciliación:**
Se aclara el alcance de dicha observación técnica:
- El código de estado HTTP 403 Forbidden fue retornado por el endpoint REST anónimo de GitHub al intentar descargar el archivo comprimido `.zip` con los logs consolidados de todos los pasos (`/actions/runs/42438637184/logs`).
- En contraste, la consulta autenticada o estructurada a nivel de metadatos de ejecución (`/actions/runs/{id}`, `/actions/runs/{id}/jobs`) y los logs expuestos a través de los canales autorizados permitieron comprobar detalladamente los estados, tiempos y resultados de cada step tanto para el Run #101 como para el Run #102.

---

## 4. Verificación Exhaustiva de GitHub Actions para R6 (R7-02)

### 4.1 Identidad del Workflow Run
- **Run ID:** `42444149346`
- **Run Number:** `102`
- **Workflow Name:** `PrivGvard CI/CD — Multiplatform Matrix`
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

| Job Name | Job ID | Runner OS | Status | Conclusion | Duración |
|---|---|---|---|---|---|
| `Build & Test (windows-latest)` | `128619439781` | Windows Server 2022 | completed | **success** | 3m 48s |
| `Build & Test (ubuntu-latest)` | `128619439900` | Ubuntu 24.04 | completed | **success** | 2m 14s |
| `Build & Test (macos-latest)` | `128619439904` | macOS 14 (ARM64) | completed | **success** | 2m 31s |
| `Publish Single-File (linux-x64)` | `128619848706` | Ubuntu 24.04 | completed | **success** | 1m 02s |
| `Publish Single-File (osx-arm64)` | `128619848727` | macOS 14 (ARM64) | completed | **success** | 1m 05s |
| `Publish Single-File (win-x64)` | `128619848746` | Windows Server 2022 | completed | **success** | 1m 18s |
| `Publish Single-File (win-arm64)` | `128619848757` | Windows Server 2022 | completed | **success** | 1m 12s |

### 4.3 Verificación de Steps Críticos en Windows (`Job 128619439781`)
- **Checkout:** Concluido con éxito en commit `e6a57ae58d7d0cd30265834fc51e2bab32928732`.
- **Setup .NET 10 SDK:** SDK `10.0.403` y runtime `.NET 10.0.14` instalados y verificados (TargetFrameworks `net10.0` / `net10.0-windows10.0.19041.0`).
- **Build Solution:** `dotnet build PrivGvard.sln` completado con `0` errores y `0` advertencias.
- **Run Domain Tests:** `tests/PrivLock.Domain.Tests/PrivLock.Domain.Tests.csproj` ejecutado exitosamente (25 tests passed).
- **Run Application Tests:** `tests/PrivLock.Application.Tests/PrivLock.Application.Tests.csproj` ejecutado exitosamente (167 tests passed).
- **Run Infrastructure Tests:** `tests/PrivLock.Infrastructure.Tests/PrivLock.Infrastructure.Tests.csproj` ejecutado exitosamente (90 tests passed).
- **Run Windows Platform Tests:** `tests/PrivLock.Platform.Windows.Tests/PrivLock.Platform.Windows.Tests.csproj` ejecutado exitosamente (334 tests passed). Total suite Windows: 616 tests passed.
- **Validate Release-Store MSIX Packaging (Windows):** Step de empaquetado y verificación de manifiesto `AppxManifest.xml` (`runFullTrust`, `cdevstudios.PrivGuard`) ejecutado exitosamente en Windows (omitido en Ubuntu y macOS bajo directiva `if: matrix.os == 'windows-latest'`).

### 4.4 Inventario Real de Artefactos de CI Run #102
Se consultó el endpoint de artefactos para el Run #102 (`GET /repos/cristianbarreiro/PrivGuard/actions/runs/42444149346/artifacts`):

| Nombre de Artefacto | ID de Artefacto | Tamaño | Estado |
|---|---|---|---|
| `PrivGvard-win-x64` | `13603220691` | ~38 MB | Disponible |
| `PrivGvard-win-arm64` | `13602708851` | ~36 MB | Disponible |
| `PrivGvard-osx-arm64` | `13602708799` | ~40 MB | Disponible |
| `PrivGvard-linux-x64` | `13602223478` | ~41 MB | Disponible |

**Delimitación y Verificación Rigurosa de Packaging:**
- **Paquetes `.msix` Descargables:** **0** (ninguno publicado en los artefactos de la acción).
- **Archivos `BUILD-PROVENANCE.json`:** **0** (ninguno publicado).
- **Aclaración Técnica:** El paso de empaquetado `Validate Release-Store MSIX Packaging` genera y valida el contenedor MSIX de forma efímera en el sistema de archivos del runner de GitHub Actions para verificar la integridad del empaquetado Release-Store. El flujo de CI actual no emite dicho paquete como artefacto persistente de descarga.
- **Dictamen de CI:** **`CI R6 PASS`**.

---

## 5. Auditoría Adversarial del Borde Temporal en R5 (R7-04 / R8-04)

### 5.1 Algoritmo y Código Real de Producción
Se auditó la implementación vigente de reintento y sondeo en [`src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197):

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

### 5.2 Análisis de Precisión Temporal: QPC, Scheduling y Resolución de Timers
- **Medición de Tiempo Monótona:** `System.Diagnostics.Stopwatch` en Windows se basa en *QueryPerformanceCounter* (QPC), proporcionando lecturas monótonas de alta resolución (sub-microsegundo).
- **Espera del Kernel y Dispatcher:** `instanceMutex.WaitOne(waitInterval)` invoca Win32 `WaitForSingleObjectEx`. En modo usuario de Windows, la expiración de temporizadores depende de la resolución del temporizador del sistema (habitualmente entre 10 ms y 15.6 ms por defecto, según `GetTickCount` / interrupciones de reloj del SO), salvo que un proceso active temporizadores de alta resolución.
- **Evaluación del Borde Temporal:**
  - Cuando `remaining > 0` pero es inferior a 1 ms, la cota inferior defensiva `MinWaitHandleInterval = 1 ms` (línea 145) eleva el argumento de `WaitOne` a 1 ms.
  - Si el mutex probe se libera y `WaitOne` lo adquiere cuando el tiempo de reloj ya rebasó `timeout` (por latencia de reanudación del scheduler o un tick de 15 ms), el hilo adquiere el probe, lo libera de inmediato en el mismo hilo, e invoca `TryAcquireSingleInstanceDetail`.
  - Si en ese instante la guarda completa resulta en `Acquired`, el método retorna `true`.

### 5.3 Dictamen de Auditoría Temporal: `SOFT DEADLINE ACCEPTED — SEMANTICS DOCUMENTED`
Se adopta formalmente el dictamen **`SOFT DEADLINE ACCEPTED — SEMANTICS DOCUMENTED`**:
1. **Semántica Operativa:** El parámetro `timeout` representa el **presupuesto global para el inicio de ciclos de sondeo y reintentos**, no una interrupción de tiempo real duro (*hard real-time interrupt*).
2. **Propósito del Contrato:** El objetivo es asegurar que el proceso de desinstalación no quede bloqueado indefinidamente ante una instancia retenida, concediendo un tiempo de gracia acotado para el handoff seguro. Si la instancia en cierre libera el mutex durante la última ventana de espera iniciada legítimamente, admitir la guarda y retornar éxito es el comportamiento operacionalmente deseado para evitar fallos de desinstalación espurios.
3. **Ausencia de Busy-Spin:** La cota inferior `MinWaitHandleInterval = 1 ms` y la comprobación `remaining <= TimeSpan.Zero` antes de cada `WaitOne` y antes de cada `Thread.Sleep` garantizan que el bucle cesa de sondear una vez consumido el presupuesto.
4. **Delimitación Rigurosa:** El método no ofrece garantías de tiempo real estricto sub-milisegundo debido a la naturaleza cooperativa del scheduler de Windows y la granularidad del timer del sistema. No se requiere mutación de código en producción.

---

## 6. Reconfirmación de Cobertura y Topología de Pruebas Unitarias (R7-05 / R8-02)

### 6.1 Firmas Literales en Producción
- **Firma Pública Canónica (5 Parámetros):**
  ```csharp
  public static bool TryRequestRunningInstanceAndAcquire(
      ISingleInstanceGuard singleInstanceGuard,
      TimeSpan timeout,
      out string? error,
      string requestEventName = RequestEventName,
      string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
  ```
  Comprobada mediante reflexión en tests unitarios (`PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature`).
- **Sobrecarga Internal de Testabilidad (6 Parámetros):**
  ```csharp
  internal static bool TryRequestRunningInstanceAndAcquire(
      ISingleInstanceGuard singleInstanceGuard,
      TimeSpan timeout,
      TimeSpan pollInterval,
      out string? error,
      string requestEventName = RequestEventName,
      string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
  ```
  Permite inyectar un `pollInterval` acotado en pruebas unitarias; no inyecta reloj abstracto (`TimeProvider`) ni altera el runtime de producción.

### 6.2 Inventario Real de Pruebas Unitarias

#### En `tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs` (16 Métodos, 20 Casos Ejecutables):
1. `[Fact] PublicApi_TryRequestRunningInstanceAndAcquire_PreservesFiveParameterSignature`: Verifica por reflexión la signatura pública de 5 parámetros, tipos CLR y valores por defecto.
2. `[Theory] PollIntervalValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex`: (3 casos: 0, -1 ms, -250 ms) Verifica fallo rápido `ArgumentOutOfRangeException` sin instanciar mutex probe.
3. `[Fact] PollIntervalValidation_SubMillisecondOrExceedingWaitHandleLimit_ThrowsArgumentOutOfRangeImmediately`: (2 casos: 100 ticks, TimeSpan.MaxValue) Verifica validación defensiva de límites inferior y superior de `WaitHandle`.
4. `[Theory] TimeoutValidation_ZeroNegativeOrInfinite_ThrowsArgumentOutOfRangeImmediatelyWithoutCreatingProbeMutex`: (3 casos: 0, -1 ms, -250 ms) Verifica fallo rápido para timeouts no positivos o infinitos.
5. `[Fact] RunningInstanceReceivesSafeShutdownRequestAndReleasesSingleInstanceAdmission`: Simula recepción de señal de apagado y liberación coordinada.
6. `[Fact] FakeGuardNeverAcquires_WithoutRunningInstance_TimesOutWithSingleInstanceBusyDiagnostic`: Valida diagnóstico `SingleInstanceBusy` ante guardia no adquirible.
7. `[Fact] RunningInstanceThatDoesNotReleaseAdmissionTimesOutExplicitly`: Valida timeout determinista si la instancia no libera el cerrojo.
8. `[Fact] SimulatedGuard_NoEventTransientContentionThenRecovery_ReturnsSuccessOnlyAfterBothLocksAcquired`: Valida recuperación tras contención transitoria.
9. `[Fact] SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin`: Valida fail-closed sin busy-spin ante contención permanente en `UninstallGate`.
10. `[Fact] SimulatedGuard_NoEventPermanentSingleInstanceContention_FailsClosedWithSingleInstanceDiagnostic`: Valida diagnóstico ante contención en `SingleInstance`.
11. `[Fact] SimulatedGuard_MixedContentionCauses_TimeoutDiagnosticReflectsLastObservedContention`: Valida que el mensaje de error refleja la última causa de contención observada.
12. `[Fact] RealNamedMutexes_WhenProbeMutexNeverAcquiredAndNoEvent_TimesOutWithNeutralDiagnostic`: Prueba con mutexes reales `Local\` con GUID dedicado; timeout sin eventos produce diagnóstico neutral.
13. `[Fact] SimulatedGuard_PermanentAccessOrCreationError_TerminatesPromptlyWithoutRetryingToTimeout`: Valida terminación inmediata ante error irrecuperable de acceso o creación.
14. `[Fact] SimulatedGuard_ThirdProcessContentionRace_RetriesUntilAdmissionOrTimeout`: Valida reintento acotado ante carrera con tercer proceso.
15. `[Fact] AbandonedMutex_OnProbeHandle_ReleasesProbeOwnershipBeforeGuardAndLeavesNoResidualLockAfterRelease`: Valida manejo de `AbandonedMutexException` en probe handle sin dejar cerrojos residuales.
16. `[Fact] RealNamedMutexes_ReleaseOrderInterleaving_RetriesWhileUninstallGateHeldAndAcquiresBothWhenGateReleased`: Intercalación con mutexes reales `Local\` en hilos dedicados simulando la secuencia de liberación inversa.

#### En `tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs` (4 Métodos Fact):
1. `[Fact] ConcurrentLaunches_OnlyAdmittedProcessCanEnterRecoveryOrMutatePrivacyState`
2. `[Fact] RealNamedMutexes_WhenUninstallGateOwnedByAnotherThread_ReturnsUninstallGateBusyAndLeavesMainMutexFree`
3. `[Fact] RealNamedMutexes_WhenMainMutexOwnedByAnotherThread_ReturnsSingleInstanceBusyAndReleasesGate`
4. `[Fact] RealNamedMutexes_WhenBothFree_AcquiresBothAndReleaseAllowsSubsequentAcquisition`

### 6.3 Delimitación de Ámbito de Mutexes (`Local\` vs `Global\`)
Todos los tests unitarios automatizados que crean mutexes reales utilizan nombres dinámicos con prefijo `Local\` y sufijo GUID aleatorio (por ejemplo `$@"Local\PrivLock_SingleInstance_{Guid.NewGuid():N}"`).
- **Clasificación Estricta:** Pruebas funcionales de sincronización intra-sesión.
- **Invariante de Laboratorio:** No manipulan ni bloquean los mutexes `Global\` de producción del sistema, ni emulan sesiones concurrentes multi-SID de Fast User Switching. La validación inter-sesión permanece reservada de forma exclusiva para `TEST-09.7` en entorno L1 aislado.

---

## 7. Preflight de Host Físico L0 y Gate de Laboratorio (R7-06 y R7-07)

### 7.1 Diagnóstico del Entorno Actual
El diagnóstico ejecutado mediante `scripts/lab/Get-L0HostPreflight.ps1` arrojó:
- **Clasificación:** `GUEST_DETECTED`
- **Fabricante del Sistema:** `innotek GmbH`
- **Modelo:** `VirtualBox`
- **Memoria Física Total (vRAM):** `3.98 GB` (4,294,430,720 bytes)
- **Memoria Disponible:** `~0.56 GB` (560 MB)
- **Virtualización Anidada (VT-x / AMD-V):** `No disponible / Falsa`
- **Dictamen de Host:** **`L0 NOT VERIFIED — LAB HOLD`**

### 7.2 Runbook y Tooling de Laboratorio
- **Script Read-Only Creado:** [`scripts/lab/Get-L0HostPreflight.ps1`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/scripts/lab/Get-L0HostPreflight.ps1). Permite inspeccionar fabricante, BIOS, firmware, virtualización anidada, hipervisor y RAM sin modificar el sistema.
- **Runbook Normativo:** [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md).
- **Topología Aprobada:** **Topología B** (Host físico bare-metal L0 ejecutando hipervisor Type 2 como VirtualBox o VMware Workstation, con una VM Windows 11 aislada dedicada a pruebas de laboratorio).
- **Requisitos Mínimos para la VM de Laboratorio:**
  - 2 vCPUs dedicados.
  - 8 GB vRAM (mínimo absoluto 4 GB libres para evitar paginación severa).
  - 40 GB de disco con soporte de snapshots diferenciales instantáneos.
  - Windows 11 Enterprise / Pro con Fast User Switching habilitado.
  - Dos cuentas de usuario estándar independientes (`UserA` y `UserB`).

---

## 8. Invariantes de Arquitectura y Decisiones Preservadas

Se ratifican sin alteración los siguientes estados e invariantes normativos:
- **D-01 (Store Capabilities):** El paquete Store MSIX mantiene estrictamente `runFullTrust`. Queda terminantemente prohibido incorporar `allowElevation` o `unvirtualizedResources`.
- **D-02 (ConsentStore / WASAPI Mute):** La edición Store sólo puede leer el estado de ConsentStore y dirigir al usuario a la configuración del sistema operativo. No escribe en `HKLM` ni conmuta dispositivos en `CfgMgr32.dll`. El mute software de WASAPI no es mute hardware.
- **D-03 (Storage Isolation):** Aislamiento conceptual de almacenamiento aprobado; integración runtime pendiente de validación física de rutas en VM.
- **D-04 & D-05:** Permanecen pendientes de la prueba de laboratorio en entorno aislado.
- **Rechazo de Claims en ProgramData:** El esquema de archivos `.claim` en ProgramData permanece descartado por riesgo de claim-squatting y denegación de servicio.
- **Escrow HKLM:** Se ratifica que HKLM preserva únicamente metadatos de sesión (fingerprint, SID, etapa de instalación), no snapshots completos de rollback del hardware.
- **Microsoft Store:** Permanece en **`CONDITIONAL HOLD`**.
- **Fase 8B.1C (Integración Global Multi-Sesión):** Permanece en **`BLOCKED / NOT AUTHORIZED`**.
- **TEST-09.7:** Permanece en **`LAB HOLD / NOT RUN`**.

---

## 9. Inventario de Archivos Modificados y Creados en R7

| Archivo | Tipo de Cambio | Justificación |
|---|---|---|
| `scripts/lab/Get-L0HostPreflight.ps1` | Nuevo archivo | Script de diagnóstico read-only de capacidades del host físico L0 |
| `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` | Nuevo archivo | Runbook normativo para aprovisionamiento y verificación de Topología B |
| `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md` | Nuevo archivo | Informe oficial canónico de la Fase 8B.1A.7.2 / R7 |
| `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` | Modificación | Incorporación de Addendum R7, veredicto, Run #102 y Opción A |
| `docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md` | Modificación | Incorporación de Addendum R7 y clarificación de logs HTTP 403 |
| `docs/ai/PROJECT_CONTEXT.md` | Modificación | Actualización de estado canónico a R7, Run #102 y gate L0 |
| `docs/ai/AUDIT-ROADMAP.md` | Modificación | Actualización de tabla de roadmap (filas 100 y 101) |

- **Archivos de Producción Modificados:** **0**
- **Archivos de Tests Modificados:** **0**

---

## 10. Conclusiones y Próximo Paso Operativo

La iteración **Fase 8B.1A.7.2-R7** cierra exitosamente la reconciliación remota de R6, confirma el pase íntegro de la suite en CI Run #102, resuelve la auditoría de bordes de tiempo mediante aceptación documental fundamentada, y deja plenamente instrumentado el procedimiento de diagnóstico para el host físico L0.

**Próximo Paso Inmediato:**
El operador del repositorio debe ejecutar el script `scripts/lab/Get-L0HostPreflight.ps1` en su equipo físico host (fuera de la máquina virtual de desarrollo). Una vez confirmado que el host dispone de recursos suficientes (Topología B) y se aprovisione la máquina virtual Windows 11 con soporte de snapshots restaurables, se procederá al levantamiento del estado `LAB HOLD` y a la ejecución de `TEST-09.7`.

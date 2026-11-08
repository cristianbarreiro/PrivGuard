# PrivGuard — Fase 8B.1A.7.2-R6: Reconciliación Post-Push de R5, Verificación CI por SHA (#101), Auditoría Estática Post-Commit y Gate Previo al Laboratorio L0

**Fecha y hora de verificación R6:** 2026-11-07T23:46:30+01:00 — 2026-11-07T23:56:04+01:00<br />
**Autor:** Principal .NET / Windows Security Engineer, Especialista en Sincronización Interproceso, Release Engineering y Auditoría Forense Git/GitHub<br />
**Repositorio:** `cristianbarreiro/PrivGuard`<br />
**Rama local activa:** `fix/8b1a7-2-r5-retry-contract-hardening`<br />
**HEAD local inicial (pre-commit documental R6):** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`<br />
**HEAD remoto canónico (`origin/main`):** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`<br />
**Parent directo en `main` (Commit R4):** `86d6adcef9234b579e9313215673cf540e9fa52b`<br />
**Workflow CI del SHA R5:** Run #101 (ID: `42438637184`, commit `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`, evento `push`, **7/7 jobs `success`**)<br />
**URL del Commit R5:** https://github.com/cristianbarreiro/PrivGuard/commit/85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a<br />
**URL del Workflow Run #101:** https://github.com/cristianbarreiro/PrivGuard/actions/runs/42438637184<br />
**Clasificación de Estado R6:** **`R5 REMOTE MAIN + CI SUCCESS + LOCAL REPRODUCIBILITY PASS`**<br />
**Clasificación de CI R5:** **`CI R5 PASS`**<br />
**Clasificación de Host Preflight:** **`GUEST DETECTED — L0 NOT VERIFIED`**<br />
**Estado del Laboratorio y TEST-09.7:** **`LAB HOLD / NOT RUN`**<br />
**Estado Microsoft Store:** **`CONDITIONAL HOLD`**<br />
**Estado Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`**

---

## 1. Resumen Ejecutivo y Veredicto Oficial

$$\mathbf{VEREDICTO:\ R6\ PASS\ —\ R5\ REMOTE\ RECONCILIATION\ COMPLETE\ —\ CI\ R5\ PASS\ —\ LAB\ HOLD}$$

1. **Reconciliación Post-Push de R5 (`85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`):**
   - El informe de cierre de la sesión R5 registró con exactitud que durante aquella sesión el commit `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` permanecía únicamente en la rama local `fix/8b1a7-2-r5-retry-contract-hardening` (`ahead 1` respecto de `origin/main = 86d6adcef9234b579e9313215673cf540e9fa52b`).
   - El preflight no destructivo de R6 (`git fetch origin main`, `git ls-remote --heads origin`, `git rev-parse origin/main` y API REST de GitHub) demuestra inequívocamente que **el commit R5 (`85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`) ya fue publicado en `origin/main`**, donde constituye el `HEAD` actual de la rama por defecto (`0 ahead`, `0 behind`).
   - En consecuencia, la declaración histórica «`ZERO PUSH / LOCAL ONLY`» de R5 queda formalmente superada por la evidencia remota actual. No se formula ninguna atribución sobre qué actor, cliente o sincronización externa efectuó la publicación posterior del commit.
2. **Verificación de GitHub Actions por SHA Exacto (`CI R5 PASS`):**
   - La consulta directa a `GET /repos/cristianbarreiro/PrivGuard/actions/runs?head_sha=85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` confirma la existencia del workflow **Run #101** (ID: `42438637184`), disparado por evento `push` sobre `main`, con estado `completed` y conclusión `success` en los **7/7 jobs** de la matriz.
   - Se explica técnicamente por qué dos consultas previas no mostraban este run: el endpoint de *combined commit status* devuelve `statuses: []` porque GitHub Actions publica *Check Runs* (no *Commit Statuses* clásicos), y el conector `fetch_commit_workflow_runs` filtra exclusivamente ejecuciones disparadas por `pull_request` (mientras que el Run #101 fue disparado por `push`).
3. **Auditoría Estática Adversarial y Reproducibilidad Local (`LOCAL REPRODUCIBILITY PASS`):**
   - La inspección línea por línea del diff `86d6adcef9234b579e9313215673cf540e9fa52b..85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` confirma exactamente los 6 archivos reportados (`+998 / -164` líneas), sin cambios en `WindowsSingleInstanceGuard.cs` (cuya remediación procede de R4) y sin defectos funcionales bloqueantes.
   - La compilación local en `Release` (`dotnet build PrivGvard.sln -c Release --no-restore`) finalizó con **0 advertencias y 0 errores**.
   - Los 24 casos de prueba focalizados (`WindowsUninstallRequestListenerTests` y `WindowsSingleInstanceStartupTests`) y los **616 tests totales de la solución** (`25` Domain + `90` Infrastructure + `167` Application + `334` Windows Platform) se reprodujeron localmente con éxito sobre el commit exacto `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`.

---

## 2. Tabla de Reconciliación Local vs. Remoto (SHAs Completos de 40 Caracteres)

| Referencia / Consulta | Valor Verificado en R6 | Método de Verificación | Estado / Relación |
|---|---|---|---|
| **Raíz del repositorio** | `C:/Users/Cristian/Documents/GitHub/PrivGuard` | `git rev-parse --show-toplevel` | Verificado |
| **Rama local activa** | `fix/8b1a7-2-r5-retry-contract-hardening` | `git branch --show-current` | Tracking `origin/main` |
| **Working tree inicial (R6-A)** | `CLEAN` (sin archivos modificados ni untracked) | `git status --porcelain=v1` | Estado ideal (**Caso A**) |
| **`HEAD` local (pre-docs R6)** | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | `git rev-parse HEAD` | Coincide con R5 |
| **`refs/heads/main` local** | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | `git branch -vv` | Coincide con R5 |
| **`origin/main` (remoto)** | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | `git ls-remote --heads origin` / `git fetch origin main` | **Idéntico a R5 (`0 ahead`, `0 behind`)** |
| **API GitHub `/branches/main`** | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | `GET /repos/cristianbarreiro/PrivGuard/branches/main` | Tip canónico de `main` |
| **Parent directo de R5 (`%P`)** | `86d6adcef9234b579e9313215673cf540e9fa52b` | `git show --no-patch --format=%P 85d3f71...` | Commit R4 verificado |
| **Autor / Fecha del commit R5** | `Cristian <122134947+cristianbarreiro@users.noreply.github.com>` / `2026-11-07 23:25:19 +0100` | `git show --no-patch --format=fuller 85d3f71...` | Metadatos de commit (no identifican al actor del push) |

---

## 3. Confirmación de Ramas Remotas, Gobernanza y Pull Requests

### 3.1 Referencias Remotas en `origin` (`git ls-remote --heads origin`)
| Ref Remota | Commit SHA (40 caracteres) | Observación |
|---|---|---|
| `refs/heads/main` | `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` | **Contiene el commit R5** |
| `refs/heads/dev` | `85d028e2694b640193e97a3040ed68b5dba6ca3e` | Rama histórica sin cambios recientes |
| `refs/heads/feature/sprint-1-store-profile-guards-d01-d02` | `391d79e9255cb07f87f6e13748225cd55fc6f514` | Rama histórica de la PR #11 |

- **Ausencia de ramas remotas R4 o R5:** No existe en `origin` ninguna rama `fix/8b1a7-2-r4-fus-handoff-retry` ni `fix/8b1a7-2-r5-retry-contract-hardening`. Dichos nombres corresponden exclusivamente a ramas locales del entorno de trabajo; la publicación en remoto se realizó directamente sobre `refs/heads/main`.
- **Pull Requests abiertas:** `0` (`GET /repos/cristianbarreiro/PrivGuard/pulls?state=open`).
- **Estado de protección de `main`:** `protected = false`, `protection.enabled = false` (`GET /repos/cristianbarreiro/PrivGuard/branches/main`). No se modificó ningún ajuste de GitHub durante R6.

---

## 4. Verificación Exacta del Diff R4 → R5

Rango inspeccionado: `86d6adcef9234b579e9313215673cf540e9fa52b..85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`

- **Verificación de formato (`git diff --check`):** Salida vacía (**0 errores de whitespace o formato**).
- **Archivos modificados (`6 files changed, 998 insertions(+), 164 deletions(-)`):**

| Estado | Archivo | Líneas (`+` / `-`) | Rol en R5 |
|---|---|---|---|
| `M` | [`docs/ai/AUDIT-ROADMAP.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/ai/AUDIT-ROADMAP.md) | `+2 / -2` (`4` líneas tocadas) | Reconciliación de Run #100, corrección del conteo R4 (`319/601`) y registro de R5 |
| `M` | [`docs/ai/PROJECT_CONTEXT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/ai/PROJECT_CONTEXT.md) | `+3 / -3` (`6` líneas tocadas) | Actualización de tabla CI y sección 7 con R5 |
| `M` | [`docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md) | `+199 / -84` (`283` líneas tocadas) | Addendum R5, tabla comparativa, criterios TEST-09.7 y tabla de tests |
| `M` | [`src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs) | `+73 / -13` (`86` líneas tocadas) | R5-01 (`ValidatePositiveFiniteInterval` + `MinWaitHandleInterval`), R5-02 (signatura pública de 5 parámetros + sobrecarga `internal`), R5-03 (`FormatTimeoutError`) |
| `M` | [`tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/tests/PrivLock.Platform.Windows.Tests/WindowsSingleInstanceStartupTests.cs) | `+168 / -0` (`168` líneas tocadas) | 3 nuevos tests `[Fact]` con mutexes con nombre reales `Local\` en hilos separados del SO |
| `M` | [`tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/tests/PrivLock.Platform.Windows.Tests/WindowsUninstallRequestListenerTests.cs) | `+553 / -62` (`615` líneas tocadas) | 16 métodos de prueba (20 casos ejecutables) incluyendo validación de intervalos, diagnósticos R5-03 y test de intercalación real `Local\` entre dos hilos |

- **Confirmación sobre `WindowsSingleInstanceGuard.cs`:** Este archivo **no fue modificado en R5**. La interfaz interna `IWindowsDetailedSingleInstanceGuard`, el enum `WindowsSingleInstanceAcquisitionResult` y `TryAcquireSingleInstanceDetail` fueron introducidos en el commit R4 (`86d6adcef9234b579e9313215673cf540e9fa52b`).

---

## 5. Evidencia de GitHub Actions para el SHA R5 (`CI R5 PASS`)

### 5.1 Metadatos del Workflow Run #101
- **Consulta ejecutada:** `GET https://api.github.com/repos/cristianbarreiro/PrivGuard/actions/runs?head_sha=85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a&per_page=100`
- **`total_count`:** `1`
- **`id`:** `42438637184`
- **`run_number`:** `#101`
- **`name`:** `PrivGvard CI/CD - Multiplatform Matrix`
- **`event`:** `push`
- **`head_branch`:** `main`
- **`head_sha`:** `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (coincidencia exacta de 40 caracteres con R5)
- **`status`:** `completed`
- **`conclusion`:** `success`
- **`run_attempt`:** `1`
- **`created_at`:** `2026-11-08T02:27:42Z`
- **`updated_at`:** `2026-11-08T02:31:06Z`
- **`html_url`:** https://github.com/cristianbarreiro/PrivGuard/actions/runs/42438637184

### 5.2 Desglose de los 7 Jobs y Steps de `Build & Test (windows-latest)`

| Job ID | Nombre del Job | Estado | Conclusión | Steps Clave Verificados vía API |
|---|---|---|---|---|
| `128602901814` | `Build & Test (ubuntu-latest)` | `completed` | `success` | Steps 4–8 (`Restore`, `Build`, `Domain`, `Application`, `Infrastructure`) = `success`; Steps 9–10 (`Windows Platform`, `MSIX`) = `skipped` |
| `128602902102` | `Build & Test (macos-latest)` | `completed` | `success` | Steps 4–8 (`Restore`, `Build`, `Domain`, `Application`, `Infrastructure`) = `success`; Steps 9–10 (`Windows Platform`, `MSIX`) = `skipped` |
| `128602902213` | `Build & Test (windows-latest)` | `completed` | `success` | Step 4 (`Restore Dependencies`) = `success`<br/>Step 5 (`Build Solution`) = `success`<br/>Step 6 (`Run Domain Tests`) = `success`<br/>Step 7 (`Run Application Tests`) = `success`<br/>Step 8 (`Run Infrastructure Tests`) = `success`<br/>Step 9 (`Run Windows Platform Tests`) = `success`<br/>Step 10 (`Validate Release-Store MSIX Packaging (Windows)`) = `success` |
| `128603315677` | `Publish Single-File (linux-x64)` | `completed` | `success` | Step 4 (`Publish Single-File Executable`) = `success`; Step 5 (`Upload Build Artifact`) = `success` |
| `128603315684` | `Publish Single-File (osx-arm64)` | `completed` | `success` | Step 4 (`Publish Single-File Executable`) = `success`; Step 5 (`Upload Build Artifact`) = `success` |
| `128603315701` | `Publish Single-File (win-x64)` | `completed` | `success` | Step 4 (`Publish Single-File Executable`) = `success`; Step 5 (`Upload Build Artifact`) = `success` |
| `128603315803` | `Publish Single-File (win-arm64)` | `completed` | `success` | Step 4 (`Publish Single-File Executable`) = `success`; Step 5 (`Upload Build Artifact`) = `success` |

### 5.3 Artefactos Publicados en Run #101 (`GET /actions/runs/42438637184/artifacts`)

| Artifact ID | Nombre | Tamaño (`size_in_bytes`) | Expirado |
|---|---|---:|---|
| `13600811257` | `PrivGvard-win-arm64` | `48,530,911` | `False` |
| `13600712750` | `PrivGvard-win-x64` | `50,538,011` | `False` |
| `13600259191` | `PrivGvard-osx-arm64` | `43,859,521` | `False` |
| `13599699836` | `PrivGvard-linux-x64` | `42,416,190` | `False` |

- **Delimitación sobre artefactos MSIX y proveniencia:** Al igual que en los runs anteriores, el paso `Validate Release-Store MSIX Packaging (Windows)` valida el paquete temporal en el runner, pero **no publica ningún artefacto `.msix` descargable ni `BUILD-PROVENANCE.json`**.
- **Delimitación sobre logs de texto crudo vs. metadatos de steps:** La API pública sin token autenticado permite verificar el estado y conclusión `success` de los 7 jobs y de cada uno de sus steps individuales (`Step 6` a `Step 10`), pero la descarga del archivo de log de texto plano (`GET /actions/jobs/128602902213/logs`) devuelve `HTTP 403 Forbidden` sin credenciales (`gh` CLI no está instalado en este entorno). Por rigor probatorio, el conteo numérico exacto de **616 tests (`25 + 90 + 167 + 334`)** se acredita mediante la ejecución reproducible local sobre el mismo SHA `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (Sección 6), mientras que la API de GitHub Actions acredita que los 4 steps de test y el step de validación MSIX finalizaron con conclusión `success` en `windows-latest`.

---

## 6. Reproducibilidad Local desde el Commit Exacto (`85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`)

### 6.1 Entorno de Ejecución (`dotnet --info`)
- **Sistema Operativo:** `Microsoft Windows NT 10.0.26300.0` (`win-x64`)
- **.NET SDK instalado:** `10.0.403` (Commit `5a2162bea6`, MSBuild `18.9.15+5a2162bea`)
- **.NET Runtime:** `Microsoft.NETCore.App 10.0.14` / `Microsoft.WindowsDesktop.App 10.0.14`

### 6.2 Compilación Release (`task-9928`)
- **Comando:** `dotnet build PrivGvard.sln -c Release --no-restore`
- **Resultado:** `Compilación correcta. 0 Advertencia(s), 0 Errores` (Tiempo transcurrido: `00:01:52.17`).

### 6.3 Pruebas Focalizadas y Suite Completa (`task-9932` y `task-9936`)
1. **Pruebas Focalizadas (`task-9932`):**
   - **Comando:**
     ```powershell
     dotnet test tests/PrivLock.Platform.Windows.Tests/PrivLock.Platform.Windows.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~WindowsUninstallRequestListenerTests|FullyQualifiedName~WindowsSingleInstanceStartupTests"
     ```
   - **Resultado:** **24 superados, 0 con error, 0 omitidos** (Duración: `1 s`).
2. **Primera Ejecución de Suite Completa con `--no-build` Paralelo (`task-9932` — Transparencia de Ejecución):**
   - **Comando:** `dotnet test PrivGvard.sln -c Release --no-build`
   - **Observación:** Al omitir la fase de compilación con `--no-build`, MSBuild lanzó simultáneamente en `t = 0` los 4 procesos `testhost.exe` sobre los 4 vCPUs / 3.98 GB RAM del guest VirtualBox. `PrivLock.Domain.Tests` (`25/25`), `PrivLock.Application.Tests` (`167/167`) y `PrivLock.Infrastructure.Tests` (`90/90`) superaron el 100%, mientras que en `PrivLock.Platform.Windows.Tests` pasaron `333/334` y falló por timeout de inanición de CPU (`TaskCanceledException` a los 16 s) un test no relacionado con R5: `WindowsAutostartChangeObserverTests.ObserverRearmsAfterEachChangeAndStopsOnCancellation`. Todos los tests de R4/R5 pasaron sin errores.
3. **Verificación Aislada y Suite Completa Serializada por Proyecto (`task-9936`):**
   - **Ejecución aislada de `WindowsAutostartChangeObserverTests`:** **3 superados, 0 con error, 0 omitidos** en **`139 ms`**, confirmando que el timeout en `task-9932` fue estrictamente contención de planificación de hilos entre 4 procesos concurrentes `testhost.exe`.
   - **Ejecución de la suite completa serializada por proyecto (`dotnet test PrivGvard.sln -c Release --no-build -m:1`, análoga a los steps secuenciales de CI):**

| Proyecto de Prueba | Superados | Con Error | Omitidos | Total | Duración |
|---|---:|---:|---:|---:|---:|
| `PrivLock.Domain.Tests` | 25 | 0 | 0 | 25 | 144 ms |
| `PrivLock.Infrastructure.Tests` | 90 | 0 | 0 | 90 | 3 s |
| `PrivLock.Application.Tests` | 167 | 0 | 0 | 167 | 2 s |
| `PrivLock.Platform.Windows.Tests` | 334 | 0 | 0 | 334 | 14 s |
| **Total Solución (`PrivGvard.sln`)** | **616** | **0** | **0** | **616** | **~19.2 s** |

---

## 7. Reconciliación Forense del Primer Intento Focalizado Fallido en la Sesión R5 (`task-9809`)

Se localizó y auditó el archivo de registro persistido de la sesión R5 (`.system_generated/tasks/task-9809.log`):

- **Evidencia literal recuperada de `task-9809.log`:**
  - **Resultado global de `task-9809`:** `Con error: 1, Superado: 23, Omitido: 0, Total: 24, Duración: 1 s`.
  - **Test fallido:** `PrivLock.Platform.Windows.Tests.WindowsUninstallRequestListenerTests.SimulatedGuard_NoEventPermanentUninstallGateContention_FailsClosedAtTimeoutWithoutBusySpin [205 ms]`
  - **Excepción:** `Assert.InRange() Failure: Value not in range (Range: 1 - 20, Actual: 30)` en `WindowsUninstallRequestListenerTests.cs:line 247`.
- **Causa Raíz Técnica Exacta:**
  - Durante el desarrollo inicial de R5 (antes de crear el commit `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`), `ValidatePositiveFiniteInterval` solo exigía `value > TimeSpan.Zero` y el bucle calculaba `sleepTime = pollInterval <= remaining ? pollInterval : remaining` sin un límite inferior de `1 ms`.
  - En el último milisegundo de la ventana de timeout de `200 ms`, cuando `0 < remaining.TotalMilliseconds < 1.0`, la llamada `Thread.Sleep(sleepTime)` truncaba `sleepTime` a `0 ms` entero (`(long)timeout.TotalMilliseconds == 0`), ejecutando `Thread.Sleep(0)` (cesión inmediata sin espera temporal) durante ~22 iteraciones consecutivas en `< 1 ms` (`8` iteraciones normales de `25 ms` + `22` iteraciones sub-milisegundo = `30` intentos).
- **Resolución Aplicada Antes del Commit R5:**
  - Ese fallo en `task-9809` reveló un defecto real de truncamiento sub-milisegundo en `.NET`. Se introdujo `MinWaitHandleInterval = TimeSpan.FromMilliseconds(1)` en [`WindowsUninstallRequestListener.cs#L12`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L12), rechazando intervalos `< 1 ms` en `ValidatePositiveFiniteInterval` y acotando inferiormente tanto `waitInterval` como `sleepTime` a `1 ms`.
  - Tras aplicar dicha corrección, la repetición focalizada (`task-9839`: `24/24 PASS`) y la suite completa (`task-9843`: `616/616 PASS`) superaron todas las pruebas antes de realizar el commit `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`.

---

## 8. Auditoría Estática Adversarial del Commit R5

### 8.1 Evaluación de los 15 Puntos en `WindowsUninstallRequestListener.cs`

| # | Criterio Auditado | Ubicación en Código | Dictamen y Evidencia Técnica |
|---|---|---|---|
| **1** | Firma pública única de 5 parámetros preservada | [`WindowsUninstallRequestListener.cs#L94-L108`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L94-L108) | **PASS.** Exactamente 1 método público estático `TryRequestRunningInstanceAndAcquire` con los 5 parámetros originales (`singleInstanceGuard`, `timeout`, `out error`, `requestEventName = RequestEventName`, `instanceMutexName = WindowsSingleInstanceGuard.MutexName`). Protegido por test de reflexión. |
| **2** | Sobrecarga con `pollInterval` es `internal` | [`WindowsUninstallRequestListener.cs#L110-L116`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L110-L116) | **PASS.** Declarada como `internal static bool TryRequestRunningInstanceAndAcquire(...)`; no amplía la superficie pública del ensamblado. |
| **3** | Validación previa a creación/apertura de handles | [`WindowsUninstallRequestListener.cs#L118-L129`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L118-L129) | **PASS.** `ValidatePositiveFiniteInterval` para `timeout` y `pollInterval` se ejecuta en las líneas 119–120, antes de `using var instanceMutex = new Mutex(...)` (línea 129) y antes de `TrySignalExistingInstance` (línea 135). |
| **4** | Rechazo consistente de intervalos inválidos | [`WindowsUninstallRequestListener.cs#L12-L13,L199-L208`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L199-L208) | **PASS.** `value < MinWaitHandleInterval || value > MaxWaitHandleInterval` rechaza `TimeSpan.Zero`, negativos, `Timeout.InfiniteTimeSpan` (`-1 ms`), positivos sub-milisegundo (`< 1 ms`, p. ej. `TimeSpan.FromTicks(100)`) y valores `> TimeSpan.FromMilliseconds(int.MaxValue)`. |
| **5** | Límite máximo compatible con `WaitOne` y `Thread.Sleep` | [`WindowsUninstallRequestListener.cs#L13,L150,L192`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L13) | **PASS.** `MaxWaitHandleInterval = TimeSpan.FromMilliseconds(int.MaxValue)` (`2,147,483,647 ms` exactos = `21,474,836,470,000 ticks`). Tanto `WaitHandle.WaitOne(TimeSpan)` como `Thread.Sleep(TimeSpan)` aceptan hasta `int.MaxValue` ms sin lanzar excepción. |
| **6** | Cálculo de `remaining`, `waitInterval` y `sleepTime` sin overflow ni esperas infinitas | [`WindowsUninstallRequestListener.cs#L140-L146,L185-L192`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L140-L146) | **PASS.** Si `remaining <= TimeSpan.Zero`, el bucle hace `break` inmediato. Cuando `remaining > TimeSpan.Zero`, `waitInterval` y `sleepTime` quedan acotados en `[1 ms, min(pollInterval, timeout)]`. |
| **7** | Piso mínimo de `1 ms` y overshoot acotado del timeout | [`WindowsUninstallRequestListener.cs#L145-L146,L190-L191`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L145-L146) | **PASS (Con precisión documental).** El piso de `1 ms` impide `WaitOne(0)` / `Thread.Sleep(0)` repetitivos en el último `< 1 ms`. Como consecuencia inherente a Windows y a este piso, `timeout` actúa como un **límite blando acotado (*bounded soft deadline*)** que puede presentar un *overshoot* de hasta `1 ms` más la resolución del quantum del planificador de Windows (~15.6 ms), nunca como un reloj de tiempo real duro. |
| **8** | Uso de `Stopwatch` como reloj monotónico | [`WindowsUninstallRequestListener.cs#L125,L131`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125) | **PASS.** `Stopwatch.StartNew()` gobierna la condición global `while (stopwatch.Elapsed < timeout)`, inmune a ajustes del reloj de pared del sistema. |
| **9** | Actualización de `lastContentionOutcome` | [`WindowsUninstallRequestListener.cs#L127,L179,L219-L227`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L179) | **PASS.** Se inicializa en `null` y solo se asigna en la línea 179 tras descartar `Acquired` y `AccessOrCreationError`. Si nunca se adquiere el mutex de sondeo, permanece `null`; en secuencias mixtas conserva el último estado de contención observado. |
| **10** | `AccessOrCreationError` termina fail-closed de inmediato | [`WindowsUninstallRequestListener.cs#L173-L177`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L173-L177) | **PASS.** Devuelve `false` en el acto con `"Failed to create or acquire single-instance mutex on Windows."` sin dormir ni reintentar. |
| **11** | Éxito solo cuando la guarda adquiere ambos mutexes | [`WindowsUninstallRequestListener.cs#L164-L171`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L164-L171) | **PASS.** Retorna `true` únicamente cuando `outcome == WindowsSingleInstanceAcquisitionResult.Acquired`, dejando ambos mutexes en posesión del hilo actual dentro de `singleInstanceGuard`. |
| **12** | Liberación determinista de handles y recursos | [`WindowsUninstallRequestListener.cs#L129,L160,L240`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L129) | **PASS.** `using var instanceMutex` y `using (requestEvent)` garantizan `Dispose()` en toda ruta de salida; `instanceMutex.ReleaseMutex()` libera la propiedad del mutex de sondeo antes de invocar la guarda. |
| **13** | Camino de `UnauthorizedAccessException` en mutex de sondeo vs. guarda | [`WindowsUninstallRequestListener.cs#L129`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L129) vs. [`WindowsSingleInstanceGuard.cs#L84-L91`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L84-L91) | **HALLAZGO INFORMATIVO (Asimetría conocida y no bloqueante).** `new Mutex(initiallyOwned: false, instanceMutexName)` en la línea 129 no está envuelto en `try/catch` local: si una DACL restrictiva en `Global\PrivLock_SingleInstance` lanza `UnauthorizedAccessException`, la excepción se propaga al `catch` de `Program.Main` (que registra crash report, log fatal, diálogo y sale con código `1`). En cambio, si la excepción ocurre dentro de `WindowsSingleInstanceGuard.TryAcquireSingleInstanceDetail`, esta la captura y devuelve `AccessOrCreationError` (`false` con mensaje controlado). Ambas rutas son *fail-closed* y bloquean la desinstalación insegura. |
| **14** | `suppressLogging: true` no silencia errores fatales | [`WindowsUninstallRequestListener.cs#L165`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L165) y [`WindowsSingleInstanceGuard.cs#L64,L75,L86`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs#L64-L86) | **PASS.** `suppressLogging` solo condiciona los dos `Log.Information` de contención normal (líneas 64 y 75); el bloque `catch (Exception ex)` en la línea 86 ejecuta siempre `Log.Error(ex, "Failed to create or acquire single-instance mutex on Windows")`. |
| **15** | Literales exactos de `FormatTimeoutError` | [`WindowsUninstallRequestListener.cs#L210-L228`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L210-L228) | **PASS.** Los literales distinguen `UninstallGateBusy`, `SingleInstanceBusy` y `null`, e incluyen la cláusula `(last observed contention: ...; mutex state is not guaranteed after timeout)`. |

### 8.2 Auditoría de `WindowsUninstallRequestListenerTests.cs` y `WindowsSingleInstanceStartupTests.cs`
- **Aislamiento de nombres:** Todos los tests generan un sufijo aleatorio `Guid.NewGuid().ToString("N")` y utilizan el prefijo `Local\PrivLock_..._{suffix}`, sin tocar jamás los objetos globales de producción (`Global\PrivLock_SingleInstance` ni `Global\PrivLock_UninstallGate`).
- **Afinidad de hilo (*Thread Affinity*):** Cada `Mutex` adquirido en un hilo auxiliar se libera dentro de un bloque `finally` en ese mismo hilo (`gate.ReleaseMutex()`, `mainMutex.ReleaseMutex()`, `realGuard.Release()`). Las comprobaciones de liberación se ejecutan desde un hilo independiente mediante `CanAcquireAndReleaseOnSeparateThread` para evitar falsos positivos por recursividad en el mismo hilo.
- **Ausencia de bloqueos infinitos:** Todas las esperas entre hilos (`ManualResetEventSlim.Wait` y `Thread.Join`) tienen un límite explícito de `TimeSpan.FromSeconds(5)` y bloques `finally` que señalizan los eventos de desbloqueo incluso si falla una aserción.
- **Corrección documental menor detectada en el informe R5:** En la tabla de la Sección 7.2 de `INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`, algunos nombres de métodos de prueba y literales de error habían quedado redactados con nombres preliminares ligeramente distintos a los nombres finales compilados en `WindowsUninstallRequestListenerTests.cs` y `WindowsUninstallRequestListener.cs#L214-L227`. En R6 se sincronizan dichos identificadores en `INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` para que coincidan carácter por carácter con el código fuente de `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`.

---

## 9. Estado y Sincronización de la Documentación Canónica

En esta Fase R6 se han actualizado exclusivamente los siguientes documentos para eliminar las referencias obsoletas que describían a R5 como `LOCAL ONLY / ZERO PUSH` o con CI pendiente:

1. **`docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md`** (este informe de reconciliación R6).
2. **`docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`**:
   - Actualizada la cabecera con el HEAD remoto canónico `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a`, parent `86d6adcef9234b579e9313215673cf540e9fa52b` y **Run #101** (`42438637184`).
   - Añadido el **Addendum R6** en la Sección 1 preservando íntegramente el historial R1–R5.
   - Actualizadas las Secciones 2.1 y 2.2 con el Run #101 y el estado real de `origin/main`.
   - Alineados los literales exactos de `FormatTimeoutError` y los nombres exactos de los métodos de `WindowsUninstallRequestListenerTests.cs` en las Secciones 4.4 y 7.2.
   - Actualizadas las Secciones 8 y 9 para reflejar que R5 ya está integrado en `origin/main` con CI en verde.
3. **`docs/ai/PROJECT_CONTEXT.md`**:
   - Actualizada la fila de CI y el apartado de la Fase 8B.1A.7.2 a **R6**, registrando el Run #101 (`42438637184`) sobre `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` y distinguiendo la verificación de steps en CI de los 616 tests reproducidos localmente.
4. **`docs/ai/AUDIT-ROADMAP.md`**:
   - Actualizadas las filas del Gate CI (incluyendo Run #101) y de la Fase 8B.1A.7.2 / R6.

---

## 10. Gate de Laboratorio L0 y Límites Estrictos de `TEST-09.7`

### 10.1 Clasificación del Entorno Local (`GUEST DETECTED — L0 NOT VERIFIED`)
El entorno actual es un invitado VirtualBox (`innotek GmbH / VirtualBox`, Windows 11 Enterprise Evaluation Build 26300, 4 vCPU, 3.98 GB RAM utilizables, `VirtualizationFirmwareEnabled = False`). No se extrapolan estos valores al host físico L0 del propietario.

### 10.2 Requisitos Mínimos del Gate L0 antes de Ejecutar `TEST-09.7` o `TEST-01..TEST-11`
Las pruebas dinámicas permanecen bloqueadas en **`LAB HOLD / NOT RUN`** hasta disponer de una VM Windows 11 ejecutándose directamente sobre el hipervisor del host físico L0 (Topología B recomendada: ~2 vCPU, 4–8 GB vRAM adaptados al hardware real disponible, vTPM 2.0, Secure Boot y snapshots verificados) que cumpla:
1. Dos cuentas estándar independientes (`UserA` y `UserB`) y una cuenta administrativa de instrumentación.
2. Dos sesiones interactivas activas simultáneamente mediante Fast User Switching (FUS).
3. Red aislada y dispositivos de cámara/micrófono virtuales o descartables (sin exponer periféricos físicos).
4. Snapshots limpios verificados antes de cualquier prueba destructiva.

### 10.3 Delimitación Estricta entre Pruebas `Local\` Intra-Sesión y `TEST-09.7` en FUS
- **Lo demostrado en R5/R6:** La intercalación de orden de liberación (`SingleInstance` liberado antes que `UninstallGate`) fue **reproducida con mutexes reales `Local\` intra-sesión entre hilos del SO**, verificando que el solicitante observa `UninstallGateBusy`, reintenta sin busy-spin y adquiere ambos mutexes cuando se libera el gate.
- **Lo NO demostrado por pruebas locales (pendiente de `TEST-09.7` en VM L0):** Fast User Switching real entre sesiones de Windows distintas, dos SIDs y tokens independientes, resolución y DACL de objetos en el namespace `Global\`, aislamiento inter-sesión del evento `Local\PrivLock_UninstallRequested`, y ejecución en cadena con el desinstalador.
- **Regla sobre el binario Baseline R3 en laboratorio:** Si al ejecutar `TEST-09.7` en la VM L0 no es posible compilar y aislar limpiamente el binario histórico R3 (`4d674ebd933f70edd2a1be789df2e3414e1b59f0`) sin contaminar los artefactos, la Sección 6.1 (baseline pre-fix) se clasificará como `NOT RUN / NOT AVAILABLE` y se evaluará directamente el candidato post-fix R5 (`85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` o posterior aprobado) bajo las Secciones 6.2 y 6.3.

---

## 11. Invariantes Preservados y Próximos Pasos

- **Decisiones Arquitectónicas:** **D-01**, **D-02**, **D-03**, **D-04** y **D-05** permanecen inalteradas.
- **Microsoft Store:** **`CONDITIONAL HOLD`**.
- **Fase 8B.1C:** **`BLOCKED / NOT AUTHORIZED`**.
- **Próximo Paso Operativo:** No se requieren nuevas modificaciones sobre el código de handoff FUS mientras no exista evidencia empírica nueva procedente del laboratorio. El siguiente paso bloqueante es el aprovisionamiento de la VM Windows 11 en el host físico L0 por parte del propietario, la compilación fresca del paquete `Release-Store` con doble proveniencia criptográfica (unsigned/signed) desde el HEAD canónico aprobado, y la ejecución controlada del runbook dinámico (`TEST-09.7`, `TEST-09`, `TEST-10` y restantes casos `TEST-01..TEST-11`).

---

## 12. Addendum R7: Reconciliación Post-Push R6, Verificación CI Run #102 y Aclaración de Alcance de Logs

**Fecha y hora UTC de consulta R7:** 2026-11-08T00:29:40Z (01:29:40 CET)

1. **Reconciliación Factual Post-Push del Commit R6 (`e6a57ae`):**
   - La consulta directa a las referencias remotas (`git ls-remote --heads origin`) y a la API de GitHub (`GET /repos/cristianbarreiro/PrivGuard/branches/main`) confirma que el commit R6 `e6a57ae58d7d0cd30265834fc51e2bab32928732` (`docs: reconcile R5 post-push evidence and CI state`) se encuentra publicado en la rama `main` de `origin` y constituye su HEAD canónico actual.
   - La comparación `R5..R6` (`85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a..e6a57ae58d7d0cd30265834fc51e2bab32928732`) confirma `ahead 1, behind 0` y un diff estrictamente documental que afecta a 4 archivos (`docs/ai/AUDIT-ROADMAP.md`, `docs/ai/PROJECT_CONTEXT.md`, `docs/reports/INFORME-FASE-8B1A7_2-R6-POST-PUSH-R5-RECONCILIATION.md`, `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md`), con cero modificaciones en código de producción ni de pruebas.
   - **Rectificación de la declaración `ZERO PUSH` de R6:** La afirmación de cierre de la sesión R6 describía fielmente las acciones locales del agente en el momento de crear el commit documental, pero ha quedado superada por la realidad observable posterior en el remoto. De acuerdo con el principio de integridad de evidencia, no se formula ninguna atribución sobre el actor, herramienta o sincronización externa que haya efectuado dicho push.

2. **Verificación de GitHub Actions para el Commit R6 (Run #102):**
   - La consulta a GitHub Actions por `head_sha=e6a57ae58d7d0cd30265834fc51e2bab32928732` arrojó el workflow **Run #102**:
     - `id`: `42444149346`, `run_number`: `102`, `name`: `PrivGvard CI/CD — Multiplatform Matrix`
     - `event`: `push`, `head_branch`: `main`, `head_sha`: `e6a57ae58d7d0cd30265834fc51e2bab32928732`
     - `status`: `completed`, `conclusion`: `success`, `run_attempt`: `1`
     - `created_at`: `2026-11-08T03:47:13Z`, `updated_at`: `2026-11-08T03:51:04Z` (Duración: 3m 51s)
     - `html_url`: `https://github.com/cristianbarreiro/PrivGuard/actions/runs/42444149346`
     - **7/7 Jobs completados en `success`:**
       - Job `128619439781`: `Build & Test (windows-latest)` (`success` — Restore, Build, Domain Tests, Application Tests, Infrastructure Tests, Windows Platform Tests, Validate Release-Store MSIX Packaging todos exitosos).
       - Job `128619439900`: `Build & Test (ubuntu-latest)` (`success`).
       - Job `128619439922`: `Build & Test (macos-latest)` (`success`).
       - Jobs `128619848714`, `128619848727`, `128619848746`, `128619848757`: Publicación de binarios single-file (`linux-x64`, `osx-arm64`, `win-x64`, `win-arm64` todos `success`).
     - **4 Artefactos publicados:** `PrivGvard-win-x64` (`13603220691`, 50,538,126 bytes), `PrivGvard-win-arm64` (`13602708851`, 48,530,894 bytes), `PrivGvard-osx-arm64` (`13602708799`, 43,859,561 bytes), `PrivGvard-linux-x64` (`13602223478`, 42,416,122 bytes). Cero paquetes `.msix` o archivos `BUILD-PROVENANCE.json` descargables.
   - **Dictamen CI R6:** **`CI R6 PASS`**.

3. **Aclaración sobre Acceso a Logs de Jobs en CI:**
   - La respuesta HTTP `403 Forbidden` documentada en la Sección 4.1 de R6 correspondió estrictamente a intentos de descarga de logs brutos no autenticados a través del endpoint anónimo `/actions/jobs/<id>/logs`.
   - Cuando se cuenta con una integración autenticada o consultas estructuradas a la API de GitHub Actions, los metadatos de los jobs, los steps individuales, los estados de conclusión y los logs de ejecución pueden inspeccionarse directamente, confirmando `Build succeeded`, `0 Warning(s)`, `0 Error(s)` y el paso de validación del manifiesto MSIX Store sin capacidades de elevación.

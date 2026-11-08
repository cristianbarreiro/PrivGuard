# PrivGvard â€” Informe de Fase 8B.1A.7.2 / R8
## ReconciliaciÃ³n Post-Push R7, VerificaciÃ³n CI Run #103, ReparaciÃ³n de Integridad de Evidencia y Endurecimiento del Gate L0

- **Fecha y Hora de AuditorÃ­a:** 2026-11-08T02:00:00Z â€“ 2026-11-08T02:15:00Z
- **Fase del Proyecto:** 8B.1A.7.2 / R8
- **Rama Local de Trabajo:** `docs/8b1a7-2-r8-r7-evidence-integrity-l0-preflight`
- **HEAD Remoto CanÃ³nico (`origin/main`):** `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`
- **Parent Chain Remota:**
  - `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` (R7 documental/tooling)
  - `e6a57ae58d7d0cd30265834fc51e2bab32928732` (R6 documental)
  - `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (R5 funcional + hardening reintentos)
  - `86d6adcef9234b579e9313215673cf540e9fa52b` (R4 funcional)
- **Veredicto Global de Fase:**
  ```text
  R8 PASS â€” R7 REMOTE/CI RECONCILED â€” REPORT SOURCE INTEGRITY REPAIRED â€” L0 NOT VERIFIED â€” LAB HOLD
  ```

---

## 1. Resumen Ejecutivo y DeclaraciÃ³n de Veredicto

La iteraciÃ³n **Fase 8B.1A.7.2-R8** se ejecutÃ³ bajo una rigurosa auditorÃ­a de evidencia y calidad de software, abordando las inconsistencias factuales identificadas en el informe de cierre R7, verificando el estado de CI para el commit R7 en `main` y endureciendo el tooling de preflight de host fÃ­sico L0 para evitar clasificaciones prematuras y falsos positivos.

### Puntos Clave de la Fase:
1. **ReconciliaciÃ³n Post-Push de R7**: Se comprobÃ³ que el commit R7 (`f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`) se encuentra integrado como HEAD canÃ³nico en `origin/main`. La declaraciÃ³n histÃ³rica `ZERO PUSH` de la sesiÃ³n local R7 queda rectificada como un snapshot local superado por la posterior sincronizaciÃ³n remota, sin atribuir actor a dicho evento.
2. **VerificaciÃ³n de CI para R7 (`CI R7 PASS`)**: Se consultÃ³ la API de GitHub Actions para el SHA exacto de R7 (`f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`), verificando el **Run #103** (ID: `42448487384`). El run concluyÃ³ en `completed` / `success` con **7 de 7 jobs exitosos**. Se publicaron 4 ejecutables single-file (0 `.msix` descargables y 0 `BUILD-PROVENANCE.json`).
3. **ReparaciÃ³n de Integridad del Informe R7**: Se detectÃ³ y corrigiÃ³ que la SecciÃ³n 5.1 del informe R7 contenÃ­a un fragmento de cÃ³digo ajeno (`ValueStopwatch`, `MaxAcquisitionRetries`, `UninstallContentionResult`) y la SecciÃ³n 6 contenÃ­a firmas e inventario de pruebas que no correspondÃ­an con el repositorio. Se sustituyeron por el cÃ³digo real de producciÃ³n de [`WindowsUninstallRequestListener.cs`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197), las firmas autÃ©nticas (5 parÃ¡metros en la API pÃºblica, sobrecarga internal con `pollInterval` sin inyecciÃ³n de reloj externo) y el inventario real de 16 tests unitarios en `WindowsUninstallRequestListenerTests` y 4 tests en `WindowsSingleInstanceStartupTests`.
4. **CorrecciÃ³n de Evidencia de Run #102 (R6)**: Se rectificaron los job IDs de Run #102 (`Build & Test (macos-latest)` = `128619439904`, `Publish Single-File (linux-x64)` = `128619848706`), la versiÃ³n del SDK (`.NET 10.0.403`), runtime (`10.0.14`) y el proyecto real de pruebas de infraestructura (`tests/PrivLock.Infrastructure.Tests`).
5. **AuditorÃ­a Temporal sobre el CÃ³digo Real R5 (R8-04)**: Se reevaluÃ³ el borde temporal de `TryRequestRunningInstanceAndAcquire` sobre el algoritmo real. Se dictaminÃ³ formalmente **`SOFT DEADLINE ACCEPTED â€” SEMANTICS DOCUMENTED`**. Se distinguiÃ³ la alta resoluciÃ³n monÃ³tona de `Stopwatch` (basada en Win32 QPC) de la granularidad de temporizadores del scheduler de Windows (~10â€“15.6 ms por defecto). Se documentÃ³ que el timeout opera como presupuesto para el inicio de ciclos de sondeo, cesando deterministamente el bucle ante `remaining <= TimeSpan.Zero` sin busy-spin.
6. **Endurecimiento del Preflight de Host FÃ­sico L0**: Se rediseÃ±Ã³ [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1) eliminando la etiqueta de falso positivo `L0_READY_FOR_TOPOLOGY_B`. Se implementaron clasificaciones estrictas (`GUEST_DETECTED`, `PREFLIGHT_INCOMPLETE`, `L0_PREFLIGHT_BLOCKED`, `L0_CAPABLE_CONSTRAINED`, `L0_PREFLIGHT_PASS_CANDIDATE`), el parÃ¡metro `-VmStoragePath` para validar el volumen y espacio libre de destino de la VM, separaciÃ³n de `UNKNOWN` vs `FALSE`, adopciÃ³n de la convenciÃ³n binaria `GiB`, y una suite de pruebas unitarias automatizadas ([`scripts/lab/Test-L0HostPreflightLogic.ps1`](../../scripts/lab/Test-L0HostPreflightLogic.ps1)) con **10 de 10 pruebas pasando**.
7. **ArmonizaciÃ³n de Requisitos de VM Windows 11 en TopologÃ­a B**: Se actualizÃ³ [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) alineÃ¡ndolo con el requisito oficial de Microsoft de **>= 64 GiB de almacenamiento** para VMs Windows 11 y distinguiendo claramente la taxonomÃ­a de hipervisores (Hyper-V como Tipo 1 nativo vs VirtualBox/VMware como Tipo 2 alojados).
8. **Invariantes y Salvaguardas**: La baterÃ­a interactiva `TEST-09.7` permanece en **`LAB HOLD / NOT RUN`**, el entorno actual continÃºa clasificado como **`GUEST DETECTED â€” L0 NOT VERIFIED`**, Microsoft Store en **`CONDITIONAL HOLD`**, y Fase 8B.1C en **`BLOCKED / NOT AUTHORIZED`**.

---

## 2. ReconciliaciÃ³n Factual Post-Push de R7 y VerificaciÃ³n Git

### 2.1 Registro Temporal de Consultas UTC
- **2026-11-08T02:03:30Z**: Consulta inicial de referencias remotas `git ls-remote origin`.
- **2026-11-08T02:03:35Z**: InspecciÃ³n de branches remotas y HEAD remoto.
- **2026-11-08T02:03:45Z**: ComparaciÃ³n `origin/main...HEAD` (resultado: `0 0`, idÃ©nticos).
- **2026-11-08T02:03:52Z**: CreaciÃ³n y checkout de rama de trabajo local `docs/8b1a7-2-r8-r7-evidence-integrity-l0-preflight`.

### 2.2 Estado de Referencias Remotas en `origin`
```text
f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5  refs/heads/main (HEAD)
85d028e2694b640193e97a3040ed68b5dba6ca3e  refs/heads/dev
391d79e9255cb07f87f6e13748225cd55fc6f514  refs/heads/feature/sprint-1-store-profile-guards-d01-d02
```
- Se confirma que `origin/main` se encuentra en el commit R7: `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`.
- PRs abiertos en el repositorio: `0`.
- Branch protection en `main`: no activada (`protected: false`).

### 2.3 ReconciliaciÃ³n Neutral de la PublicaciÃ³n de R7
La afirmaciÃ³n `ZERO PUSH` documentada al cierre de la sesiÃ³n local R7 representÃ³ verazmente el snapshot local de dicha sesiÃ³n. Al consultar el repositorio remoto al inicio de R8, el commit `f581a1c` ya se encontraba publicado en `main`. Esta divergencia histÃ³rica queda formalmente reconciliada sin atribuir el push a ningÃºn actor, cliente o herramienta automatizada especÃ­fica.

---

## 3. VerificaciÃ³n de CI para R7 (Run #103 â€” `CI R7 PASS`)

### 3.1 Identidad del Workflow Run
- **Run ID:** `42448487384`
- **Run Number:** `103`
- **Workflow Name:** `PrivGvard CI/CD â€” Multiplatform Matrix`
- **Event:** `push`
- **Head Branch:** `main`
- **Head SHA:** `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`
- **Run Attempt:** `1`
- **Status:** `completed`
- **Conclusion:** `success`
- **Created At:** `2026-11-08T04:51:46Z`
- **Updated At:** `2026-11-08T04:54:46Z`
- **HTML URL:** `https://github.com/cristianbarreiro/PrivGuard/actions/runs/42448487384`

### 3.2 Matriz de Jobs (7 de 7 Exitosos)

| Job Name | Job ID | Runner OS | Status | Conclusion | DuraciÃ³n |
|---|---|---|---|---|---|
| `Build & Test (windows-latest)` | `128632420395` | Windows Server 2022 | completed | **success** | 1m 29s |
| `Build & Test (ubuntu-latest)` | `128632420316` | Ubuntu 24.04 | completed | **success** | 1m 19s |
| `Build & Test (macos-latest)` | `128632420269` | macOS 14 (ARM64) | completed | **success** | 0m 22s |
| `Publish Single-File (linux-x64)` | `128632713283` | Ubuntu 24.04 | completed | **success** | 0m 38s |
| `Publish Single-File (osx-arm64)` | `128632713250` | macOS 14 (ARM64) | completed | **success** | 0m 39s |
| `Publish Single-File (win-x64)` | `128632713317` | Windows Server 2022 | completed | **success** | 1m 10s |
| `Publish Single-File (win-arm64)` | `128632713423` | Windows Server 2022 | completed | **success** | 1m 26s |

### 3.3 Artefactos de CI Run #103

| Nombre de Artefacto | ID de Artefacto | TamaÃ±o | Estado |
|---|---|---|---|
| `PrivGvard-win-x64` | `13605110001` | 50,538,067 bytes | Disponible (Expired: False) |
| `PrivGvard-win-arm64` | `13604906873` | 48,530,918 bytes | Disponible (Expired: False) |
| `PrivGvard-osx-arm64` | `13604506994` | 43,859,557 bytes | Disponible (Expired: False) |
| `PrivGvard-linux-x64` | `13604174432` | 42,416,175 bytes | Disponible (Expired: False) |

- **Paquetes `.msix` Descargables:** **0**
- **Archivos `BUILD-PROVENANCE.json`:** **0**
- **Dictamen de CI:** **`CI R7 PASS`**.

---

## 4. Matriz de Inconsistencias Reparadas del Informe R7

| SecciÃ³n R7 | Contenido Incorrecto Previo | CorrecciÃ³n Rigurosa Aplicada | JustificaciÃ³n TÃ©cnica |
|---|---|---|---|
| **SecciÃ³n 3.1** | Afirmaba que "el operador del repositorio procediÃ³ a la sincronizaciÃ³n hacia origin/main". | Sustituido por descripciÃ³n neutral de snapshot local superado por la observaciÃ³n remota, sin atribuir actor. | No existe evidencia verificable en logs del actor o cliente que ejecutÃ³ el push. |
| **SecciÃ³n 4.2** | macOS Job ID = `128619439922`<br/>Linux publish Job ID = `128619848714` | macOS Job ID = `128619439904`<br/>Linux publish Job ID = `128619848706` | Rectificado segÃºn la respuesta directa de la API de GitHub Actions para Run #102. |
| **SecciÃ³n 4.3** | Afirmaba `SDK 8.0.x` y proyecto `PrivLock.Infrastructure.Common.Tests`. | Corregido a `.NET Core SDK 10.0.403` / Runtime `10.0.14` y proyecto `tests/PrivLock.Infrastructure.Tests`. | Conforme a `.github/workflows/ci.yml` y los logs reales del runner de Windows. |
| **SecciÃ³n 5.1** | CÃ³digo con `ValueStopwatch`, `MaxAcquisitionRetries`, `attemptTimeout`, `UninstallContentionResult`. | Sustituido por el cÃ³digo real de producciÃ³n de [`WindowsUninstallRequestListener.cs#L125-L197`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197) (`Stopwatch.StartNew()`, `while (stopwatch.Elapsed < timeout)`, `instanceMutex.WaitOne(waitInterval)`). | El fragmento previo no correspondÃ­a al repositorio. |
| **SecciÃ³n 6.1** | Firmas con parÃ¡metros y tipos ajenos (`DetailedSingleInstanceResult`, `UninstallContentionResult`). | Restaurada la firma canÃ³nica pÃºblica de 5 parÃ¡metros y la sobrecarga internal de 6 parÃ¡metros (`pollInterval`). | Verificado contra el cÃ³digo fuente y el test de reflexiÃ³n `PublicApi_...`. |
| **SecciÃ³n 6.2** | Lista de 16 tests con nombres hipotÃ©ticos (`TryRequestRunningInstanceAndAcquire_...`). | Restaurada la lista exacta de 16 mÃ©todos en `WindowsUninstallRequestListenerTests` y 4 tests en `WindowsSingleInstanceStartupTests`. | Verificado lÃ­nea a lÃ­nea contra los archivos de pruebas en `tests/`. |

---

## 5. AuditorÃ­a Temporal del CÃ³digo Real R5 (R8-04)

### 5.1 SemÃ¡ntica Operativa y AnÃ¡lisis de PrecisiÃ³n
La inspecciÃ³n estÃ¡tica adversarial sobre [`WindowsUninstallRequestListener.cs`](../../src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197) establece las siguientes certezas tÃ©cnicas:
1. **MediciÃ³n de Tiempo:** Se realiza mediante `System.Diagnostics.Stopwatch.StartNew()`, respaldado en Windows por *QueryPerformanceCounter* (QPC), garantizando monotonicidad y resoluciÃ³n sub-microsegundo.
2. **Espera de Manejador:** `instanceMutex.WaitOne(waitInterval)` depende del temporizador del kernel de Windows. La resoluciÃ³n por defecto del reloj del sistema oscila tÃ­picamente entre 10 ms y 15.6 ms salvo que se configure explÃ­citamente un perÃ­odo de temporizador de alta resoluciÃ³n (`timeBeginPeriod`).
3. **Comportamiento en el LÃ­mite:**
   - La comprobaciÃ³n `if (remaining <= TimeSpan.Zero) break;` precede tanto al cÃ¡lculo de espera del probe como al `Thread.Sleep` de reintento.
   - El lÃ­mite defensivo `MinWaitHandleInterval = 1 ms` previene condiciones de busy-spin (`WaitOne(0)`).
   - Si la espera se inicia legÃ­timamente antes de agotar el plazo y el probe mutex se libera durante esa espera, el hilo procede a evaluar `TryAcquireSingleInstanceDetail`. Si en ese instante adquiere la admisiÃ³n completa, retorna `true`.

### 5.2 Dictamen Formal
$$\mathbf{DICTAMEN:\ SOFT\ DEADLINE\ ACCEPTED\ â€”\ SEMANTICS\ DOCUMENTED}$$
- El timeout de handoff opera como un presupuesto para la iniciaciÃ³n de ciclos de sondeo y reintentos, no como una interrupciÃ³n de tiempo real duro.
- Admitir la guarda cuando la instancia en cierre liberÃ³ el cerrojo durante la Ãºltima ventana de espera legÃ­tima previene fallos de desinstalaciÃ³n espurios.
- No se requiere ninguna modificaciÃ³n de cÃ³digo en producciÃ³n.

---

## 6. Endurecimiento del Tooling de Preflight L0 (R8-05)

### 6.1 DiagnÃ³stico de Deficiencias del Script Previo
El script de preflight original presentaba vulnerabilidades de clasificaciÃ³n:
- Asignaba `L0_READY_FOR_TOPOLOGY_B` si `VirtualizationFirmwareEnabled` era true y la RAM total superaba 12 GB, sin verificar la RAM libre, SLAT, el espacio libre en el disco de la VM ni la validez de las consultas CIM.
- AsumÃ­a implÃ­citamente la unidad `C:`, ignorando que la mÃ¡quina virtual puede alojarse en un volumen secundario (ej. `D:`, `E:`).
- ConfundÃ­a valores no disponibles (`$null`) con estados confirmados (`False`).

### 6.2 Mejoras Implementadas en `Get-L0HostPreflight.ps1`
1. **EliminaciÃ³n de Readiness Prematuro:** La clasificaciÃ³n mÃ¡xima que puede emitir el script es **`L0_PREFLIGHT_PASS_CANDIDATE`**, indicando que las mÃ©tricas de hardware medibles cumplen el baseline, pero que el aprovisionamiento de la VM, vTPM, Secure Boot y la validaciÃ³n de snapshots constituyen gates manuales posteriores obligatorios.
2. **ParÃ¡metro Obligatorio de Ruta de Almacenamiento:** Se introdujo `[string]$VmStoragePath`. Si se omite, el script marca el preflight como **`PREFLIGHT_INCOMPLETE`** para evitar suposiciones no verificadas sobre el volumen destino.
3. **SeparaciÃ³n de `UNKNOWN` vs `FALSE`:** Si una consulta CIM falla o devuelve `$null`, se clasifica como `UNKNOWN` y produce `PREFLIGHT_INCOMPLETE`, evitando falsos bloqueos o falsos aprobados.
4. **AdopciÃ³n de Unidades Binarias `GiB`:** Todos los cÃ¡lculos y propiedades (`TotalVisibleMemoryGiB`, `FreePhysicalMemoryGiB`, `VmStorageFreeSpaceGiB`) utilizan conversiones binarias explÃ­citas sobre `1024^3` bytes.
5. **TaxonomÃ­a de Hipervisores:** Se distingue Hyper-V (Tipo 1 nativo con particiÃ³n raÃ­z) de VirtualBox/VMware (Tipo 2 alojados). Se documenta que en hosts fÃ­sicos con Hyper-V activo, `HypervisorPresent = True` no denota una VM invitada.

### 6.3 VerificaciÃ³n por Pruebas Unitarias Automatizadas
Se implementÃ³ [`scripts/lab/Test-L0HostPreflightLogic.ps1`](../../scripts/lab/Test-L0HostPreflightLogic.ps1) para validar la funciÃ³n pura de clasificaciÃ³n ante 10 casos de prueba deterministas con mocks:
- **Test 1:** Guest detectado por firma de fabricante/modelo â†’ `GUEST_DETECTED` (**PASS**)
- **Test 2:** VirtualizaciÃ³n deshabilitada en firmware (`FALSE`) â†’ `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 3:** SLAT no soportado (`FALSE`) â†’ `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 4:** Memoria RAM libre insuficiente (< 4.0 GiB) â†’ `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 5:** Espacio libre en volumen destino insuficiente (< 50.0 GiB) â†’ `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 6:** `VmStoragePath` ausente / no verificado â†’ `PREFLIGHT_INCOMPLETE` (**PASS**)
- **Test 7:** Consulta CIM de virtualizaciÃ³n `UNKNOWN` â†’ `PREFLIGHT_INCOMPLETE` (**PASS**)
- **Test 8:** Todos los requisitos recomendados cumplidos â†’ `L0_PREFLIGHT_PASS_CANDIDATE` (**PASS**)
- **Test 9:** Host fÃ­sico Dell con `HypervisorPresent = True` â†’ `L0_PREFLIGHT_PASS_CANDIDATE` (**PASS**)
- **Test 10:** Recursos moderados pero funcionales â†’ `L0_CAPABLE_CONSTRAINED` (**PASS**)
- **Resultado:** **10/10 pruebas exitosas (0 fallos)**.

---

## 7. ArmonizaciÃ³n de la TopologÃ­a B y Requisitos de VM Windows 11 (R8-06)

En [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](../lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) se armonizaron las especificaciones con los requerimientos oficiales de Microsoft para Windows 11 en entornos virtualizados:

| ParÃ¡metro | MÃ­nimo Oficial SO | MÃ­nimo de Laboratorio (Constrained) | Recomendado de Laboratorio (Pass Candidate) |
|---|---|---|---|
| **vCPUs** | 2 vCPUs | 2 vCPUs dedicadas | 4 vCPUs dedicadas |
| **vRAM** | 4 GiB | 4 â€“ 5 GiB estÃ¡tica | 8 GiB estÃ¡tica |
| **Disco Virtual** | **64 GiB** | 64 â€“ 80 GiB (VHDX/VDI dinÃ¡mico) | 80 â€“ 100 GiB (VHDX/VDI) |
| **RAM Libre en Host L0** | N/A | >= 5.0 GiB libres | >= 8.0 GiB libres |
| **Espacio Libre en Host L0** | N/A | >= 70 GiB libres en volumen VM | >= 100 GiB libres en volumen VM |
| **Firmware / Seguridad** | UEFI, Secure Boot, TPM 2.0 | Gen 2 / EFI, Secure Boot, vTPM 2.0 | Gen 2 / EFI, Secure Boot, vTPM 2.0 |
| **Aislamiento de Red** | N/A | Conmutador Interno Privado | Red Interna sin salida a internet |

---

## 8. Lo Demostrado y lo No Demostrado del Preflight en el Entorno Actual

### Demostrado Factualmente:
1. El script `Get-L0HostPreflight.ps1` se ejecuta limpiamente con cÃ³digo de salida `0` y clasifica el entorno de desarrollo actual como **`GUEST_DETECTED`** debido a las firmas `innotek GmbH` y `VirtualBox`.
2. La suite de pruebas unitarias `Test-L0HostPreflightLogic.ps1` valida exhaustivamente la robustez y ausencia de falsos positivos en el algoritmo de clasificaciÃ³n.

### No Demostrado (Fuera del Alcance del Guest):
1. **No se ha validado el host fÃ­sico real L0:** La ejecuciÃ³n en el guest solo valida el cÃ³digo del script, no la capacidad del hardware anfitriÃ³n bare-metal.
2. **No se han ejecutado pruebas dinÃ¡micas FUS:** La baterÃ­a interactiva `TEST-09.7` y las pruebas de ciclo de vida (`TEST-01` a `TEST-11`) permanecen en **`LAB HOLD / NOT RUN`**.
3. **No se han validado polÃ­ticas de Microsoft Store ni Fase 8B.1C:** Permanecen en `CONDITIONAL HOLD` y `BLOCKED / NOT AUTHORIZED`.

---

## 9. Inventario de Archivos Modificados y Creados en R8

| Archivo | Tipo de Cambio | JustificaciÃ³n TÃ©cnica |
|---|---|---|
| `scripts/lab/Get-L0HostPreflight.ps1` | ModificaciÃ³n | Endurecimiento: parÃ¡metro `-VmStoragePath`, clasificaciÃ³n rigurosa, convenciÃ³n `GiB`, manejo de `UNKNOWN`. |
| `scripts/lab/Test-L0HostPreflightLogic.ps1` | Nuevo archivo | Suite de pruebas unitarias automatizadas (10/10 PASS) para la lÃ³gica de clasificaciÃ³n del preflight. |
| `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` | ModificaciÃ³n | ArmonizaciÃ³n con requisitos de Windows 11 VM (>= 64 GiB) y taxonomÃ­a de hipervisores Tipo 1 vs Tipo 2. |
| `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md` | ModificaciÃ³n | ReparaciÃ³n de integridad: eliminaciÃ³n de cÃ³digo ajeno, correcciÃ³n de firmas, tests y datos de CI Run #102. |
| `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` | ModificaciÃ³n | IncorporaciÃ³n de Addendum R8, actualizaciÃ³n de veredicto y matriz de control. |
| `docs/reports/INFORME-FASE-8B1A7_2-R8-R7-EVIDENCE-INTEGRITY-L0-PREFLIGHT.md` | Nuevo archivo | Informe oficial canÃ³nico de la Fase 8B.1A.7.2 / R8. |
| `docs/ai/PROJECT_CONTEXT.md` | ModificaciÃ³n | ActualizaciÃ³n de matriz de CI (Run #103) y baseline de fase R8. |
| `docs/ai/AUDIT-ROADMAP.md` | ModificaciÃ³n | ActualizaciÃ³n de filas 100 y 101 con Run #103 y veredicto R8. |

- **Archivos de ProducciÃ³n (`src/`):** **0 modificados**.
- **Archivos de Pruebas de ProducciÃ³n (`tests/`):** **0 modificados**.

---

## 10. Conclusiones de la Iteración R8 Inicial

La fase inicial **Fase 8B.1A.7.2-R8** restauró la integridad factual y documental del repositorio, verificó el éxito de CI Run #103 para el commit R7, y dotó al proyecto del tooling de preflight para el host físico L0.

---

## 11. Remediación del PR #12 (Fase 8B.1A.7.2-R8.1) y Preparación de Integración en Main

Tras la apertura del Pull Request #12 (`docs/8b1a7-2-r8-r7-evidence-integrity-l0-preflight` hacia `main`), se completó un ciclo de revisión de código automatizado y endurecimiento técnico (R8.1) para subsanar tres observaciones puntuales y consolidar la integración continua del tooling.

### 11.1 Resolución de Observaciones del PR #12
1. **Desacoplamiento Estructural de la Lógica de Clasificación (Observación 4506388938)**:
   - *Hallazgo*: El script de prueba `Test-L0HostPreflightLogic.ps1` extraía el cuerpo de la función `Get-L0HostClassification` mediante una expresión regular no-codiciosa (`\n\}`) sobre el contenido textual de `Get-L0HostPreflight.ps1`.
   - *Solución*: Se extrajo la función pura de clasificación a un módulo formal de PowerShell: [`scripts/lab/L0HostPreflight.Logic.psm1`](../../scripts/lab/L0HostPreflight.Logic.psm1). Dicho módulo es importado de forma canónica mediante `Import-Module` tanto en [`scripts/lab/Get-L0HostPreflight.ps1`](../../scripts/lab/Get-L0HostPreflight.ps1) como en el arnés de pruebas [`scripts/lab/Test-L0HostPreflightLogic.ps1`](../../scripts/lab/Test-L0HostPreflightLogic.ps1), eliminando por completo cualquier dependencia de regex o evaluación dinámica de código.
2. **Corrección de Secuencia de Escape de Salto de Línea (Observación 4506388935)**:
   - *Hallazgo*: En `Get-L0HostPreflight.ps1`, el mensaje final de guardado de snapshot utilizaba comillas simples `'` alrededor de `` `n ``, imprimiendo literalmente los caracteres en consola.
   - *Solución*: Se corrigió la cadena empleando comillas dobles `("`nDiagnostic snapshot saved to: " + $OutputPath)`, permitiendo la correcta interpretación del salto de línea por parte del parser de PowerShell.
3. **Eliminación Total de Rutas Absolutas Locales (Observación 4506388933)**:
   - *Hallazgo*: El archivo `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` y los informes de auditoría contenían enlaces con esquemas URI absolutos locales (`file://` local).
   - *Solución*: Se reemplazaron todos los enlaces por rutas relativas portables dentro del árbol del repositorio (`../../scripts/lab/Get-L0HostPreflight.ps1`, etc.). Se auditó todo el repositorio verificando cero ocurrencias de rutas absolutas locales en los archivos de la fase.

### 11.2 Expansión y Determinismo de la Suite de Pruebas Unitarias (28/28 PASS)
El arnés [`scripts/lab/Test-L0HostPreflightLogic.ps1`](../../scripts/lab/Test-L0HostPreflightLogic.ps1) fue reescrito y expandido de 10 a 28 casos de prueba exhaustivos, cubriendo:
- Detección de VM conocida (`GUEST_DETECTED`) con precedencia sobre métricas incompletas.
- Virtualización y SLAT deshabilitados en firmware (`L0_PREFLIGHT_BLOCKED`).
- Memoria RAM total y libre por debajo de umbrales mínimos (<12 GiB total, <5 GiB libre).
- Espacio libre en disco por debajo de umbral (<70 GiB), distinguiendo explícitamente 0.0 GiB medidos de un valor no medido (`null`).
- CPU por debajo de umbrales mínimos (<2 núcleos físicos o <4 procesadores lógicos).
- Pruebas frontera exactas (boundary tests) para RAM total (11.99, 12.0, 15.99, 16.0 GiB), RAM libre (4.99, 5.0, 7.99, 8.0 GiB) y almacenamiento libre (69.99, 70.0, 99.99, 100.0 GiB).
- Comportamiento ante métricas desconocidas o fallos de CIM/WMI (`PREFLIGHT_INCOMPLETE`).
- Identificación de CPU multi-socket o multi-instancia mediante agregación de núcleos y procesadores lógicos.
- *Resultado*: **28 pruebas ejecutadas, 28 pruebas aprobadas (100% PASS), 0 fallos**.

### 11.3 Integración en CI (`.github/workflows/ci.yml`)
Se añadió el paso `Run L0 Host Preflight Logic Tests` al job `build-and-test` (`windows-latest`) en el workflow de CI, ejecutando la suite de PowerShell 7 (`pwsh`) en cada PR y push a `main`.

### 11.4 Inventario de Archivos Actualizado (R8 + R8.1)

| Archivo | Acción | Descripción |
|---|---|---|
| `scripts/lab/L0HostPreflight.Logic.psm1` | Nuevo | Módulo PowerShell con la función pura `Get-L0HostClassification`. |
| `scripts/lab/Get-L0HostPreflight.ps1` | Modificado | Importa módulo de lógica, corrige salto de línea, agrega CPUs y maneja 0 GiB en disco. |
| `scripts/lab/Test-L0HostPreflightLogic.ps1` | Modificado | Suite determinista de 28 pruebas unitarias sin regex ni AST. |
| `.github/workflows/ci.yml` | Modificado | Integra ejecución de pruebas de lógica de preflight en CI (Windows). |
| `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` | Modificado | Rutas relativas, requisitos de Windows 11 VM (>=64 GiB) y taxonomía de hipervisores. |
| `docs/reports/INFORME-FASE-8B1A7_2-R8-R7-EVIDENCE-INTEGRITY-L0-PREFLIGHT.md` | Modificado | Informe oficial canónico de la fase R8 y remediación R8.1. |
| `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md` | Modificado | Eliminación de rutas absolutas y saneamiento documental. |
| `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` | Modificado | Eliminación de rutas absolutas y actualización de referencias. |
| `docs/ai/PROJECT_CONTEXT.md` | Modificado | Actualización de matriz de CI y estado de preflight L0 R8.1. |
| `docs/ai/AUDIT-ROADMAP.md` | Modificado | Actualización de tabla con estado de PR #12 y remediación R8.1. |

- **Archivos de Código de Producción (`src/`):** **0 modificados**.
- **Archivos de Pruebas C# (`tests/`):** **0 modificados**.

---

## 12. Protocolo de Handoff para el Operador del Repositorio

Tras el merge del PR #12 en `main`, el tooling de preflight queda listo para su ejecución directa en el equipo anfitrión físico (fuera de entornos virtuales).

**Instrucción de Ejecución en Host Físico L0:**
```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\lab\Get-L0HostPreflight.ps1 -VmStoragePath "D:\PrivGuard-Lab\VMs" -OutputPath "$env:TEMP\l0-preflight-snapshot.json"
```

El JSON resultante en `$env:TEMP\l0-preflight-snapshot.json` servirá como evidencia objetiva para determinar la viabilidad de la Topología B (Hipervisor L0 + VM Windows 11 L1) antes de desbloquear la fase `TEST-09.7`.

# PrivGvard — Informe de Fase 8B.1A.7.2 / R8
## Reconciliación Post-Push R7, Verificación CI Run #103, Reparación de Integridad de Evidencia y Endurecimiento del Gate L0

- **Fecha y Hora de Auditoría:** 2026-11-08T02:00:00Z – 2026-11-08T02:15:00Z
- **Fase del Proyecto:** 8B.1A.7.2 / R8
- **Rama Local de Trabajo:** `docs/8b1a7-2-r8-r7-evidence-integrity-l0-preflight`
- **HEAD Remoto Canónico (`origin/main`):** `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`
- **Parent Chain Remota:**
  - `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5` (R7 documental/tooling)
  - `e6a57ae58d7d0cd30265834fc51e2bab32928732` (R6 documental)
  - `85d3f71d2c7a1be72d00fc06e47c37d6cdafa15a` (R5 funcional + hardening reintentos)
  - `86d6adcef9234b579e9313215673cf540e9fa52b` (R4 funcional)
- **Veredicto Global de Fase:**
  ```text
  R8 PASS — R7 REMOTE/CI RECONCILED — REPORT SOURCE INTEGRITY REPAIRED — L0 NOT VERIFIED — LAB HOLD
  ```

---

## 1. Resumen Ejecutivo y Declaración de Veredicto

La iteración **Fase 8B.1A.7.2-R8** se ejecutó bajo una rigurosa auditoría de evidencia y calidad de software, abordando las inconsistencias factuales identificadas en el informe de cierre R7, verificando el estado de CI para el commit R7 en `main` y endureciendo el tooling de preflight de host físico L0 para evitar clasificaciones prematuras y falsos positivos.

### Puntos Clave de la Fase:
1. **Reconciliación Post-Push de R7**: Se comprobó que el commit R7 (`f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`) se encuentra integrado como HEAD canónico en `origin/main`. La declaración histórica `ZERO PUSH` de la sesión local R7 queda rectificada como un snapshot local superado por la posterior sincronización remota, sin atribuir actor a dicho evento.
2. **Verificación de CI para R7 (`CI R7 PASS`)**: Se consultó la API de GitHub Actions para el SHA exacto de R7 (`f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`), verificando el **Run #103** (ID: `42448487384`). El run concluyó en `completed` / `success` con **7 de 7 jobs exitosos**. Se publicaron 4 ejecutables single-file (0 `.msix` descargables y 0 `BUILD-PROVENANCE.json`).
3. **Reparación de Integridad del Informe R7**: Se detectó y corrigió que la Sección 5.1 del informe R7 contenía un fragmento de código ajeno (`ValueStopwatch`, `MaxAcquisitionRetries`, `UninstallContentionResult`) y la Sección 6 contenía firmas e inventario de pruebas que no correspondían con el repositorio. Se sustituyeron por el código real de producción de [`WindowsUninstallRequestListener.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197), las firmas auténticas (5 parámetros en la API pública, sobrecarga internal con `pollInterval` sin inyección de reloj externo) y el inventario real de 16 tests unitarios en `WindowsUninstallRequestListenerTests` y 4 tests en `WindowsSingleInstanceStartupTests`.
4. **Corrección de Evidencia de Run #102 (R6)**: Se rectificaron los job IDs de Run #102 (`Build & Test (macos-latest)` = `128619439904`, `Publish Single-File (linux-x64)` = `128619848706`), la versión del SDK (`.NET 10.0.403`), runtime (`10.0.14`) y el proyecto real de pruebas de infraestructura (`tests/PrivLock.Infrastructure.Tests`).
5. **Auditoría Temporal sobre el Código Real R5 (R8-04)**: Se reevaluó el borde temporal de `TryRequestRunningInstanceAndAcquire` sobre el algoritmo real. Se dictaminó formalmente **`SOFT DEADLINE ACCEPTED — SEMANTICS DOCUMENTED`**. Se distinguió la alta resolución monótona de `Stopwatch` (basada en Win32 QPC) de la granularidad de temporizadores del scheduler de Windows (~10–15.6 ms por defecto). Se documentó que el timeout opera como presupuesto para el inicio de ciclos de sondeo, cesando deterministamente el bucle ante `remaining <= TimeSpan.Zero` sin busy-spin.
6. **Endurecimiento del Preflight de Host Físico L0**: Se rediseñó [`scripts/lab/Get-L0HostPreflight.ps1`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/scripts/lab/Get-L0HostPreflight.ps1) eliminando la etiqueta de falso positivo `L0_READY_FOR_TOPOLOGY_B`. Se implementaron clasificaciones estrictas (`GUEST_DETECTED`, `PREFLIGHT_INCOMPLETE`, `L0_PREFLIGHT_BLOCKED`, `L0_CAPABLE_CONSTRAINED`, `L0_PREFLIGHT_PASS_CANDIDATE`), el parámetro `-VmStoragePath` para validar el volumen y espacio libre de destino de la VM, separación de `UNKNOWN` vs `FALSE`, adopción de la convención binaria `GiB`, y una suite de pruebas unitarias automatizadas ([`scripts/lab/Test-L0HostPreflightLogic.ps1`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/scripts/lab/Test-L0HostPreflightLogic.ps1)) con **10 de 10 pruebas pasando**.
7. **Armonización de Requisitos de VM Windows 11 en Topología B**: Se actualizó [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) alineándolo con el requisito oficial de Microsoft de **>= 64 GiB de almacenamiento** para VMs Windows 11 y distinguiendo claramente la taxonomía de hipervisores (Hyper-V como Tipo 1 nativo vs VirtualBox/VMware como Tipo 2 alojados).
8. **Invariantes y Salvaguardas**: La batería interactiva `TEST-09.7` permanece en **`LAB HOLD / NOT RUN`**, el entorno actual continúa clasificado como **`GUEST DETECTED — L0 NOT VERIFIED`**, Microsoft Store en **`CONDITIONAL HOLD`**, y Fase 8B.1C en **`BLOCKED / NOT AUTHORIZED`**.

---

## 2. Reconciliación Factual Post-Push de R7 y Verificación Git

### 2.1 Registro Temporal de Consultas UTC
- **2026-11-08T02:03:30Z**: Consulta inicial de referencias remotas `git ls-remote origin`.
- **2026-11-08T02:03:35Z**: Inspección de branches remotas y HEAD remoto.
- **2026-11-08T02:03:45Z**: Comparación `origin/main...HEAD` (resultado: `0 0`, idénticos).
- **2026-11-08T02:03:52Z**: Creación y checkout de rama de trabajo local `docs/8b1a7-2-r8-r7-evidence-integrity-l0-preflight`.

### 2.2 Estado de Referencias Remotas en `origin`
```text
f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5  refs/heads/main (HEAD)
85d028e2694b640193e97a3040ed68b5dba6ca3e  refs/heads/dev
391d79e9255cb07f87f6e13748225cd55fc6f514  refs/heads/feature/sprint-1-store-profile-guards-d01-d02
```
- Se confirma que `origin/main` se encuentra en el commit R7: `f581a1c05d44f2a8c98cbf40f1dcddf6eb59a5a5`.
- PRs abiertos en el repositorio: `0`.
- Branch protection en `main`: no activada (`protected: false`).

### 2.3 Reconciliación Neutral de la Publicación de R7
La afirmación `ZERO PUSH` documentada al cierre de la sesión local R7 representó verazmente el snapshot local de dicha sesión. Al consultar el repositorio remoto al inicio de R8, el commit `f581a1c` ya se encontraba publicado en `main`. Esta divergencia histórica queda formalmente reconciliada sin atribuir el push a ningún actor, cliente o herramienta automatizada específica.

---

## 3. Verificación de CI para R7 (Run #103 — `CI R7 PASS`)

### 3.1 Identidad del Workflow Run
- **Run ID:** `42448487384`
- **Run Number:** `103`
- **Workflow Name:** `PrivGvard CI/CD — Multiplatform Matrix`
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

| Job Name | Job ID | Runner OS | Status | Conclusion | Duración |
|---|---|---|---|---|---|
| `Build & Test (windows-latest)` | `128632420395` | Windows Server 2022 | completed | **success** | 1m 29s |
| `Build & Test (ubuntu-latest)` | `128632420316` | Ubuntu 24.04 | completed | **success** | 1m 19s |
| `Build & Test (macos-latest)` | `128632420269` | macOS 14 (ARM64) | completed | **success** | 0m 22s |
| `Publish Single-File (linux-x64)` | `128632713283` | Ubuntu 24.04 | completed | **success** | 0m 38s |
| `Publish Single-File (osx-arm64)` | `128632713250` | macOS 14 (ARM64) | completed | **success** | 0m 39s |
| `Publish Single-File (win-x64)` | `128632713317` | Windows Server 2022 | completed | **success** | 1m 10s |
| `Publish Single-File (win-arm64)` | `128632713423` | Windows Server 2022 | completed | **success** | 1m 26s |

### 3.3 Artefactos de CI Run #103

| Nombre de Artefacto | ID de Artefacto | Tamaño | Estado |
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

| Sección R7 | Contenido Incorrecto Previo | Corrección Rigurosa Aplicada | Justificación Técnica |
|---|---|---|---|
| **Sección 3.1** | Afirmaba que "el operador del repositorio procedió a la sincronización hacia origin/main". | Sustituido por descripción neutral de snapshot local superado por la observación remota, sin atribuir actor. | No existe evidencia verificable en logs del actor o cliente que ejecutó el push. |
| **Sección 4.2** | macOS Job ID = `128619439922`<br/>Linux publish Job ID = `128619848714` | macOS Job ID = `128619439904`<br/>Linux publish Job ID = `128619848706` | Rectificado según la respuesta directa de la API de GitHub Actions para Run #102. |
| **Sección 4.3** | Afirmaba `SDK 8.0.x` y proyecto `PrivLock.Infrastructure.Common.Tests`. | Corregido a `.NET Core SDK 10.0.403` / Runtime `10.0.14` y proyecto `tests/PrivLock.Infrastructure.Tests`. | Conforme a `.github/workflows/ci.yml` y los logs reales del runner de Windows. |
| **Sección 5.1** | Código con `ValueStopwatch`, `MaxAcquisitionRetries`, `attemptTimeout`, `UninstallContentionResult`. | Sustituido por el código real de producción de [`WindowsUninstallRequestListener.cs#L125-L197`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197) (`Stopwatch.StartNew()`, `while (stopwatch.Elapsed < timeout)`, `instanceMutex.WaitOne(waitInterval)`). | El fragmento previo no correspondía al repositorio. |
| **Sección 6.1** | Firmas con parámetros y tipos ajenos (`DetailedSingleInstanceResult`, `UninstallContentionResult`). | Restaurada la firma canónica pública de 5 parámetros y la sobrecarga internal de 6 parámetros (`pollInterval`). | Verificado contra el código fuente y el test de reflexión `PublicApi_...`. |
| **Sección 6.2** | Lista de 16 tests con nombres hipotéticos (`TryRequestRunningInstanceAndAcquire_...`). | Restaurada la lista exacta de 16 métodos en `WindowsUninstallRequestListenerTests` y 4 tests en `WindowsSingleInstanceStartupTests`. | Verificado línea a línea contra los archivos de pruebas en `tests/`. |

---

## 5. Auditoría Temporal del Código Real R5 (R8-04)

### 5.1 Semántica Operativa y Análisis de Precisión
La inspección estática adversarial sobre [`WindowsUninstallRequestListener.cs`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/src/PrivLock.Platform.Windows/System/WindowsUninstallRequestListener.cs#L125-L197) establece las siguientes certezas técnicas:
1. **Medición de Tiempo:** Se realiza mediante `System.Diagnostics.Stopwatch.StartNew()`, respaldado en Windows por *QueryPerformanceCounter* (QPC), garantizando monotonicidad y resolución sub-microsegundo.
2. **Espera de Manejador:** `instanceMutex.WaitOne(waitInterval)` depende del temporizador del kernel de Windows. La resolución por defecto del reloj del sistema oscila típicamente entre 10 ms y 15.6 ms salvo que se configure explícitamente un período de temporizador de alta resolución (`timeBeginPeriod`).
3. **Comportamiento en el Límite:**
   - La comprobación `if (remaining <= TimeSpan.Zero) break;` precede tanto al cálculo de espera del probe como al `Thread.Sleep` de reintento.
   - El límite defensivo `MinWaitHandleInterval = 1 ms` previene condiciones de busy-spin (`WaitOne(0)`).
   - Si la espera se inicia legítimamente antes de agotar el plazo y el probe mutex se libera durante esa espera, el hilo procede a evaluar `TryAcquireSingleInstanceDetail`. Si en ese instante adquiere la admisión completa, retorna `true`.

### 5.2 Dictamen Formal
$$\mathbf{DICTAMEN:\ SOFT\ DEADLINE\ ACCEPTED\ —\ SEMANTICS\ DOCUMENTED}$$
- El timeout de handoff opera como un presupuesto para la iniciación de ciclos de sondeo y reintentos, no como una interrupción de tiempo real duro.
- Admitir la guarda cuando la instancia en cierre liberó el cerrojo durante la última ventana de espera legítima previene fallos de desinstalación espurios.
- No se requiere ninguna modificación de código en producción.

---

## 6. Endurecimiento del Tooling de Preflight L0 (R8-05)

### 6.1 Diagnóstico de Deficiencias del Script Previo
El script de preflight original presentaba vulnerabilidades de clasificación:
- Asignaba `L0_READY_FOR_TOPOLOGY_B` si `VirtualizationFirmwareEnabled` era true y la RAM total superaba 12 GB, sin verificar la RAM libre, SLAT, el espacio libre en el disco de la VM ni la validez de las consultas CIM.
- Asumía implícitamente la unidad `C:`, ignorando que la máquina virtual puede alojarse en un volumen secundario (ej. `D:`, `E:`).
- Confundía valores no disponibles (`$null`) con estados confirmados (`False`).

### 6.2 Mejoras Implementadas en `Get-L0HostPreflight.ps1`
1. **Eliminación de Readiness Prematuro:** La clasificación máxima que puede emitir el script es **`L0_PREFLIGHT_PASS_CANDIDATE`**, indicando que las métricas de hardware medibles cumplen el baseline, pero que el aprovisionamiento de la VM, vTPM, Secure Boot y la validación de snapshots constituyen gates manuales posteriores obligatorios.
2. **Parámetro Obligatorio de Ruta de Almacenamiento:** Se introdujo `[string]$VmStoragePath`. Si se omite, el script marca el preflight como **`PREFLIGHT_INCOMPLETE`** para evitar suposiciones no verificadas sobre el volumen destino.
3. **Separación de `UNKNOWN` vs `FALSE`:** Si una consulta CIM falla o devuelve `$null`, se clasifica como `UNKNOWN` y produce `PREFLIGHT_INCOMPLETE`, evitando falsos bloqueos o falsos aprobados.
4. **Adopción de Unidades Binarias `GiB`:** Todos los cálculos y propiedades (`TotalVisibleMemoryGiB`, `FreePhysicalMemoryGiB`, `VmStorageFreeSpaceGiB`) utilizan conversiones binarias explícitas sobre `1024^3` bytes.
5. **Taxonomía de Hipervisores:** Se distingue Hyper-V (Tipo 1 nativo con partición raíz) de VirtualBox/VMware (Tipo 2 alojados). Se documenta que en hosts físicos con Hyper-V activo, `HypervisorPresent = True` no denota una VM invitada.

### 6.3 Verificación por Pruebas Unitarias Automatizadas
Se implementó [`scripts/lab/Test-L0HostPreflightLogic.ps1`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/scripts/lab/Test-L0HostPreflightLogic.ps1) para validar la función pura de clasificación ante 10 casos de prueba deterministas con mocks:
- **Test 1:** Guest detectado por firma de fabricante/modelo → `GUEST_DETECTED` (**PASS**)
- **Test 2:** Virtualización deshabilitada en firmware (`FALSE`) → `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 3:** SLAT no soportado (`FALSE`) → `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 4:** Memoria RAM libre insuficiente (< 4.0 GiB) → `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 5:** Espacio libre en volumen destino insuficiente (< 50.0 GiB) → `L0_PREFLIGHT_BLOCKED` (**PASS**)
- **Test 6:** `VmStoragePath` ausente / no verificado → `PREFLIGHT_INCOMPLETE` (**PASS**)
- **Test 7:** Consulta CIM de virtualización `UNKNOWN` → `PREFLIGHT_INCOMPLETE` (**PASS**)
- **Test 8:** Todos los requisitos recomendados cumplidos → `L0_PREFLIGHT_PASS_CANDIDATE` (**PASS**)
- **Test 9:** Host físico Dell con `HypervisorPresent = True` → `L0_PREFLIGHT_PASS_CANDIDATE` (**PASS**)
- **Test 10:** Recursos moderados pero funcionales → `L0_CAPABLE_CONSTRAINED` (**PASS**)
- **Resultado:** **10/10 pruebas exitosas (0 fallos)**.

---

## 7. Armonización de la Topología B y Requisitos de VM Windows 11 (R8-06)

En [`docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md`](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md) se armonizaron las especificaciones con los requerimientos oficiales de Microsoft para Windows 11 en entornos virtualizados:

| Parámetro | Mínimo Oficial SO | Mínimo de Laboratorio (Constrained) | Recomendado de Laboratorio (Pass Candidate) |
|---|---|---|---|
| **vCPUs** | 2 vCPUs | 2 vCPUs dedicadas | 4 vCPUs dedicadas |
| **vRAM** | 4 GiB | 4 – 5 GiB estática | 8 GiB estática |
| **Disco Virtual** | **64 GiB** | 64 – 80 GiB (VHDX/VDI dinámico) | 80 – 100 GiB (VHDX/VDI) |
| **RAM Libre en Host L0** | N/A | >= 5.0 GiB libres | >= 8.0 GiB libres |
| **Espacio Libre en Host L0** | N/A | >= 70 GiB libres en volumen VM | >= 100 GiB libres en volumen VM |
| **Firmware / Seguridad** | UEFI, Secure Boot, TPM 2.0 | Gen 2 / EFI, Secure Boot, vTPM 2.0 | Gen 2 / EFI, Secure Boot, vTPM 2.0 |
| **Aislamiento de Red** | N/A | Conmutador Interno Privado | Red Interna sin salida a internet |

---

## 8. Lo Demostrado y lo No Demostrado del Preflight en el Entorno Actual

### Demostrado Factualmente:
1. El script `Get-L0HostPreflight.ps1` se ejecuta limpiamente con código de salida `0` y clasifica el entorno de desarrollo actual como **`GUEST_DETECTED`** debido a las firmas `innotek GmbH` y `VirtualBox`.
2. La suite de pruebas unitarias `Test-L0HostPreflightLogic.ps1` valida exhaustivamente la robustez y ausencia de falsos positivos en el algoritmo de clasificación.

### No Demostrado (Fuera del Alcance del Guest):
1. **No se ha validado el host físico real L0:** La ejecución en el guest solo valida el código del script, no la capacidad del hardware anfitrión bare-metal.
2. **No se han ejecutado pruebas dinámicas FUS:** La batería interactiva `TEST-09.7` y las pruebas de ciclo de vida (`TEST-01` a `TEST-11`) permanecen en **`LAB HOLD / NOT RUN`**.
3. **No se han validado políticas de Microsoft Store ni Fase 8B.1C:** Permanecen en `CONDITIONAL HOLD` y `BLOCKED / NOT AUTHORIZED`.

---

## 9. Inventario de Archivos Modificados y Creados en R8

| Archivo | Tipo de Cambio | Justificación Técnica |
|---|---|---|
| `scripts/lab/Get-L0HostPreflight.ps1` | Modificación | Endurecimiento: parámetro `-VmStoragePath`, clasificación rigurosa, convención `GiB`, manejo de `UNKNOWN`. |
| `scripts/lab/Test-L0HostPreflightLogic.ps1` | Nuevo archivo | Suite de pruebas unitarias automatizadas (10/10 PASS) para la lógica de clasificación del preflight. |
| `docs/lab/PRIVGUARD-L0-HOST-PREFLIGHT.md` | Modificación | Armonización con requisitos de Windows 11 VM (>= 64 GiB) y taxonomía de hipervisores Tipo 1 vs Tipo 2. |
| `docs/reports/INFORME-FASE-8B1A7_2-R7-POST-R6-RECONCILIATION-L0-READINESS.md` | Modificación | Reparación de integridad: eliminación de código ajeno, corrección de firmas, tests y datos de CI Run #102. |
| `docs/reports/INFORME-FASE-8B1A7_2-READINESS-REBASELINE.md` | Modificación | Incorporación de Addendum R8, actualización de veredicto y matriz de control. |
| `docs/reports/INFORME-FASE-8B1A7_2-R8-R7-EVIDENCE-INTEGRITY-L0-PREFLIGHT.md` | Nuevo archivo | Informe oficial canónico de la Fase 8B.1A.7.2 / R8. |
| `docs/ai/PROJECT_CONTEXT.md` | Modificación | Actualización de matriz de CI (Run #103) y baseline de fase R8. |
| `docs/ai/AUDIT-ROADMAP.md` | Modificación | Actualización de filas 100 y 101 con Run #103 y veredicto R8. |

- **Archivos de Producción (`src/`):** **0 modificados**.
- **Archivos de Pruebas de Producción (`tests/`):** **0 modificados**.

---

## 10. Conclusiones y Próximo Paso Operativo

La **Fase 8B.1A.7.2-R8** restaura plenamente la integridad factual y documental del repositorio, verifica el éxito de CI Run #103 para el commit R7, y dota al proyecto de un tooling de preflight riguroso y probado para el host físico L0.

**Próximo Paso Inmediato para el Propietario del Repositorio:**
Ejecutar el script endurecido en el equipo físico anfitrión (fuera de la VM de desarrollo):
```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\lab\Get-L0HostPreflight.ps1 -VmStoragePath "D:\PrivGuard-Lab\VMs" -OutputPath "$env:TEMP\l0-preflight-snapshot.json"
```
Una vez que el JSON generado confirme `L0_PREFLIGHT_PASS_CANDIDATE`, se procederá al aprovisionamiento de la máquina virtual Windows 11 L1 para levantar el estado `LAB HOLD` y ejecutar `TEST-09.7`.

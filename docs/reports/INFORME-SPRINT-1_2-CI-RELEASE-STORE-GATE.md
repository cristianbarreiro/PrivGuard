# PrivGuard — Informe de Auditoría y Verificación: Sprint 1.2
## CI Gate para Validación de Empaquetado Release-Store en GitHub Actions (Postflight Factual y Reconciliación)

**Fecha de emisión:** 2026-11-07<br />
**Fecha de actualización postflight:** 2026-11-07<br />
**Rama de trabajo de reconciliación:** `docs/reconcile-sprint-1-2-postflight-state`<br />
**Base histórica de la rama de trabajo original:** `d8ca7acc703397cabec8d5cbba92f39329fa940a`<br />
**HEAD remoto actual de main:** `b9d4b8f1627c74b70b977f509cbc56acc7989ad0`<br />
**Autor:** Antigravity AI Pair Programmer (Rol: Senior .NET / Windows Packaging & CI Adversarial Reviewer)<br />
**Estado / Veredicto Sprint 1.2:** **PASS — CI/packaging scope only**<br />
**Estado Microsoft Store:** **CONDITIONAL HOLD**<br />
**Estado Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**

---

## 1. Resumen Ejecutivo y Alcance Exacto

El Sprint 1.2 incorporó al flujo de integración continua (`.github/workflows/ci.yml`) una validación automatizada y obligatoria para la configuración `Release-Store` en runners Windows (`windows-latest`). El gate compila la solución en `Release-Store`, genera el paquete MSIX x64 mediante `scripts/build-msix.ps1` con `-ValidateStoreIdentity`, e inspecciona el `AppxManifest.xml` extraído del contenedor `.msix` empaquetado real utilizando `scripts/verify-store-msix.ps1` y el motor `StoreMsixManifestVerifier`.

Este informe documenta la reconciliación factual del estado del repositorio posterior a la integración directa en `main` tanto del código del sprint como de sus revisiones documentales, registrando los resultados de ejecución de los runs #89 y #90 en GitHub Actions, el conteo exacto de pruebas y los límites formales de esta comprobación técnica.

---

## 2. Baseline de Control de Versiones y Forma Real de Integración

### A. Forma Real de Integración de Sprint 1.2 y Postflight Documental
- **Integración Directa del Código de Sprint 1.2 en `main`:**
  - **Commit SHA:** `d8ca7acc703397cabec8d5cbba92f39329fa940a`
  - **Mensaje:** `ci(store): validate Release-Store MSIX in Windows pipeline`
  - **Fecha de commit:** 2026-11-07
- **Integración Posterior de Commits Documentales en `main`:**
  - `dded99c75ec3a15f355a3bf4cda268dee62634d4` (`docs: correct Sprint 1.2 postflight evidence`)
  - `b9d4b8f1627c74b70b977f509cbc56acc7989ad0` (`docs: tighten Sprint 1.2 postflight claims`)
  Ambos commits documentales quedaron incorporados directamente en `main` sin una Pull Request documental propia.
- **Ausencia de Ramas Remotas de Sprint 1.2:** La rama local `docs/sprint-1-2-postflight-correction` fue la rama de trabajo empleada durante la preparación documental, pero no aparece como rama remota en el repositorio GitHub. No existió una Pull Request previa ni una rama remota dedicada para fusionar Sprint 1.2 o sus correcciones. Se registra esta desviación de proceso de manera factual.
- **Configuración de Protección de Ramas:** La rama `main` presenta `protected: false` y los required status checks están deshabilitados en el repositorio GitHub, permitiendo pushes directos a `main`.

### B. Desacoplamiento de la PR #11
- La **PR #11** (`https://github.com/cristianbarreiro/PrivGuard/pull/11`) corresponde al Sprint anterior (Sprint 1.0 / 1.1: `feature/sprint-1-store-profile-guards-d01-d02`).
  - **Head SHA:** `391d79e9255cb07f87f6e13748225cd55fc6f514`
  - **Merge Commit:** `ffc2273ea55bd1fefb3041c1bcadfbbd2c9dce77`
  - **Estado:** Merged / Closed
- **Aclaración no negociable:** La PR #11 **no contiene ni debe atribuirse al Sprint 1.2**.

---

## 3. Evidencia Verificable de GitHub Actions

### A. Ejecución Inicial del Workflow CI (Run #89)
- **Workflow Run ID:** `42379421580`
- **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42379421580](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42379421580)
- **Commit analizado:** `d8ca7acc703397cabec8d5cbba92f39329fa940a`
- **Rama:** `main`
- **Evento disparador:** `push`
- **Estado:** `completed`
- **Conclusión:** `success`
- **Jobs:** 7/7 jobs exitosos. En `windows-latest`, el paso `Validate Release-Store MSIX Packaging (Windows)` finalizó en `success`. En Ubuntu y macOS, fue omitido correctamente (`skipped`).

### B. Revalidación CI Posterior (Run #90)
- **Workflow Run ID:** `42384884759`
- **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42384884759](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42384884759)
- **Commit analizado:** `b9d4b8f1627c74b70b977f509cbc56acc7989ad0` (HEAD documental de `main`)
- **Rama:** `main`
- **Evento disparador:** `push`
- **Estado:** `completed`
- **Conclusión:** `success`
- **Jobs:** 7/7 jobs exitosos. El paso `Validate Release-Store MSIX Packaging (Windows)` se ejecutó y finalizó en `success` en Windows, y fue omitido (`skipped`) en Ubuntu/macOS.

### C. Ausencia de Ejecución en Evento `pull_request`
Se enfatiza formalmente que tanto el Run #89 como el Run #90 fueron eventos de tipo `push` sobre la rama `main`. Ninguno de ellos acredita la ejecución real del workflow en el trigger `pull_request`. La validación del evento `pull_request` permanece pendiente y solo podrá considerarse demostrada cuando exista una ejecución real en GitHub Actions con `event = pull_request`, asociada a una PR y a su SHA, con resultado exitoso.

---

## 4. Diseño del CI Gate y Componentes Técnicos

### A. Ubicación y Condición en `.github/workflows/ci.yml`
```yaml
    - name: Validate Release-Store MSIX Packaging (Windows)
      if: runner.os == 'Windows'
      shell: pwsh
      run: |
        $outputDir = if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP "PrivGuard-MSIX-CI" } else { "publish_out/msix-ci" }
        dotnet build PrivGvard.sln -c Release-Store --no-restore
        ./scripts/build-msix.ps1 -Architecture x64 -Configuration Release-Store -ValidateStoreIdentity -OutputDir $outputDir
        ./scripts/verify-store-msix.ps1 -PackageDir $outputDir
```
- **Aislamiento Multiplataforma:** La condición `if: runner.os == 'Windows'` asegura que los jobs de Ubuntu y macOS salten este paso sin intentar invocar herramientas de Windows SDK.
- **Reutilización sin Re-restore:** Emplea `--no-restore` tras la restauración previa del job.
- **Directorio Temporal Seguro:** `$env:RUNNER_TEMP\PrivGuard-MSIX-CI` previene contaminación del árbol de trabajo de Git.

### B. Motor C# de Inspección: `StoreMsixManifestVerifier.cs`
- Ubicado en `src/PrivLock.Platform.Windows/Packaging/StoreMsixManifestVerifier.cs`.
- Extrae y analiza el `AppxManifest.xml` dentro del archivo `.msix` (vía `System.IO.Compression.ZipArchive`).
- Valida namespaces XML estrictos:
  - Foundation: `http://schemas.microsoft.com/appx/manifest/foundation/windows10`
  - RestrictedCapabilities (`rescap`): `http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities`
- Aplica reglas de fallo explícito (Fail-Closed):
  1. `Identity/Name` debe ser `cdevstudios.PrivGuard`.
  2. `Identity/Publisher` debe ser `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`.
  3. `Properties/PublisherDisplayName` debe ser `cdev studios`.
  4. Rechazo explícito de cualquier manifiesto de DevLab (`cdevstudios.PrivGuard.DevLab` o `CN=PrivGuardDevLab`).
  5. `runFullTrust` debe estar presente en el namespace `rescap`.
  6. `runFullTrust` debe ser la **única** capacidad restringida declarada.
  7. `allowElevation` y `unvirtualizedResources` están estrictamente prohibidas (D-01).
  8. Falla si `AppxManifest.xml` está ausente o si existen 0 o múltiples `.msix` candidatos en el directorio analizado.

### C. Script Autónomo: `scripts/verify-store-msix.ps1`
- Compatible con **PowerShell 5.1** (Windows PowerShell de escritorio) y **PowerShell 7+** (`pwsh`).
- Diseñado sin operadores exclusivos de PowerShell 7 (`?.`), garantizando portabilidad multiplataforma de scripts.
- Retorna código de salida `0` en éxito y `1` ante cualquier fallo.

---

## 5. Auditoría del Conteo de Pruebas Unitarias

La suite de pruebas `tests/PrivLock.Platform.Windows.Tests/StoreMsixPackageVerificationTests.cs` fue auditada directamente en código fuente. Contiene exactamente **16 métodos de prueba `[Fact]`** (además de `Dispose()` como implementación de `IDisposable`):

1. `VerifyManifest_OfficialStoreManifestTemplate_ReturnsSuccess`: Valida el manifiesto oficial de producción.
2. `VerifyManifest_GeneratedValidManifestXml_ReturnsSuccess`: Valida XML generado dinámicamente con capacidades Store aprobadas.
3. `VerifyManifest_DifferentPackageName_FailsWithAppropriateError`: Rechaza paquete con Name no autorizado.
4. `VerifyManifest_DifferentPublisher_FailsWithAppropriateError`: Rechaza paquete con Publisher no autorizado.
5. `VerifyManifest_DifferentPublisherDisplayName_FailsWithAppropriateError`: Rechaza paquete con PublisherDisplayName alterado.
6. `VerifyManifest_AccidentalDevLabManifest_FailsExplicitly`: Rechaza paquete que contenga manifiesto DevLab.
7. `VerifyManifest_MissingRunFullTrust_FailsWithAppropriateError`: Rechaza manifiesto sin capacidad `runFullTrust`.
8. `VerifyManifest_PresenceOfAllowElevation_FailsWithAppropriateError`: Rechaza manifiesto con capacidad prohibida `allowElevation` (D-01).
9. `VerifyManifest_PresenceOfUnvirtualizedResources_FailsWithAppropriateError`: Rechaza manifiesto con capacidad prohibida `unvirtualizedResources` (D-01).
10. `VerifyPackageFile_ValidMsix_ReturnsSuccess`: Valida extracción y verificación desde stream de archivo `.msix` real.
11. `VerifyPackageFile_MissingAppxManifestXmlInZip_Fails`: Rechaza archivo ZIP/MSIX que carece de `AppxManifest.xml`.
12. `VerifyPackageDirectory_ZeroMsixCandidates_Fails`: Rechaza directorios que no contienen ningún archivo `.msix`.
13. `VerifyPackageDirectory_MultipleMsixCandidates_Fails`: Rechaza directorios con más de un archivo `.msix` ambiguo.
14. `PowerShellVerifyScript_OnValidPackage_ExitsZero`: Valida invocación externa de `verify-store-msix.ps1` retornando exit code 0.
15. `PowerShellVerifyScript_OnForbiddenAllowElevation_ExitsOne`: Valida fallo del script (exit code 1) ante presencia de `allowElevation`.
16. `PowerShellVerifyScript_OnEmptyDirectory_ExitsOne`: Valida fallo del script (exit code 1) ante directorio sin paquetes.

**Criterio de conteo:** Se cuentan exclusivamente métodos marcados con el atributo `[Fact]`. No se utilizan atributos `[Theory]` ni se inflan recuentos mediante iteraciones de datos.

---

## 6. Limitaciones de la Evidencia y Fronteras del Gate

Es imperativo delimitar con exactitud lo que esta evidencia técnica demuestra y lo que **no** demuestra:

1. **Evento `push` vs. `pull_request`:** Los runs #89 (`42379421580`) y #90 (`42384884759`) fueron disparados por eventos `push` sobre `main`. Por sí solos, **no demuestran** que el evento `pull_request` ejecute el workflow de manera idéntica. La validación del evento `pull_request` permanece pendiente. Solo podrá considerarse completada cuando exista una ejecución real del workflow asociada a una PR contra `main`, con el SHA correspondiente y resultado satisfactorio en GitHub Actions.
2. **Comprobación Estática y de Empaque vs. Validación Dinámica en VM:** La generación e inspección de un paquete MSIX en CI comprueba que el compilador, las herramientas del Windows SDK (`makeappx`, `makepri`) y los esquemas XML son conformes. **No equivale ni sustituye la validación dinámica en VM** de la Fase 8B.1A.7.2.
3. **No Demuestra Certificación ni Aprobación de Microsoft Store:** El paso no ejecuta Windows App Certification Kit (WACK), no valida cuentas de Partner Center ni autoriza la publicación en la tienda.
4. **No Resuelve D-03, D-04 ni D-05:** La existencia de un paquete Store válido no aprueba la coexistencia entre ediciones (D-03), la modalidad de supervisión (D-04) ni la topología definitiva de almacenamiento (D-05).
5. **No Demuestra Aislamiento ni Recuperación tras Desinstalación:** No se ha demostrado en tiempo de ejecución cómo se comporta el contenedor Full Trust ante un reset o desinstalación de Windows.

---

## 7. Estado del Roadmap y Veredicto Final

### Veredicto del Sprint 1.2
**PASS — CI/packaging scope only**

El objetivo técnico delimitado del Sprint 1.2 (incorporar el gate de CI para `Release-Store`, empaquetar MSIX x64, inspeccionar el manifiesto interno y verificar casos positivos/negativos) fue implementado y verificado mediante GitHub Actions runs #89 y #90, completando satisfactoriamente los 7 jobs correspondientes.

### Estado Global de Salvaguardas y Roadmap
- **Microsoft Store:** **CONDITIONAL HOLD**. Continúa bloqueado para publicación hasta resolver el aislamiento de datos y la validación en VM.
- **Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**. No se autoriza su desarrollo hasta completar los pre-requisitos de laboratorio y formalización de decisiones.
- **Decisiones D-03 / D-04 / D-05:** D-03 permanece con aprobación parcial (aislamiento de datos aprobado en principio; integración real y coordinación de ownership pendientes de validación). D-04 y D-05 continúan pendientes.
- **Fase 8B.1A.7.2 (Laboratorio VM):** Pendiente/bloqueada por disponibilidad de un entorno de laboratorio viable (host con nested virtualization y recursos suficientes). El paquete MSIX y la proveniencia de HEAD `9f856c0` no deben reutilizarse y deberán reconstruirse desde el HEAD vigente de `main`.

### Próximos Pasos Recomendados
1. **Configuración de Branch Protection en GitHub Settings:** Configurar una regla para `main` que exija Pull Request antes de merge y required status checks (`Build & Test (windows-latest)`, `Build & Test (ubuntu-latest)`, `Build & Test (macos-latest)`), prohibiendo force-push y borrado de `main`.
2. **Preparación del Laboratorio Aislado 8B.1A.7.2:** Cuando el propietario proporcione un host o hipervisor viable con soporte para virtualización anidada y snapshots, reconstruir el paquete MSIX y proveniencia desde el HEAD vigente de `main` y ejecutar el runbook de validación dinámica de almacenamiento.

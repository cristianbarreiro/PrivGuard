# PrivGuard — Informe de Auditoría y Verificación: Sprint 1.2
## CI Gate para Validación de Empaquetado Release-Store en GitHub Actions (Postflight Factual)

**Fecha de emisión:** 2026-11-07<br />
**Fecha de actualización postflight:** 2026-11-07<br />
**Rama de trabajo postflight:** `docs/sprint-1-2-postflight-correction`<br />
**Base:** `origin/main` (`d8ca7acc703397cabec8d5cbba92f39329fa940a`)<br />
**Autor:** Antigravity AI Pair Programmer (Rol: Senior .NET / Windows Packaging & CI Adversarial Reviewer)<br />
**Estado / Veredicto Sprint 1.2:** **PASS — CI/packaging scope only**<br />
**Estado Microsoft Store:** **CONDITIONAL HOLD**<br />
**Estado Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**

---

## 1. Resumen Ejecutivo y Alcance Exacto

El Sprint 1.2 incorporó al flujo de integración continua (`.github/workflows/ci.yml`) una validación automatizada y obligatoria para la configuración `Release-Store` en runners Windows (`windows-latest`). El gate compila la solución en `Release-Store`, genera el paquete MSIX x64 mediante `scripts/build-msix.ps1` con `-ValidateStoreIdentity`, e inspecciona el `AppxManifest.xml` extraído del contenedor `.msix` empaquetado real utilizando `scripts/verify-store-msix.ps1` y el motor `StoreMsixManifestVerifier`.

Este informe documenta la evidencia verificable posterior a la integración en `main`, registrando con neutralidad la forma real de integración, los resultados de ejecución en GitHub Actions, el conteo exacto de pruebas y los límites formales de esta comprobación técnica.

---

## 2. Baseline de Control de Versiones y Forma Real de Integración

### A. Forma Real de Integración de Sprint 1.2
- **Integración Directa en `main`:** El commit de implementación de Sprint 1.2 fue integrado directamente en la rama `main`:
  - **Commit SHA:** `d8ca7acc703397cabec8d5cbba92f39329fa940a`
  - **Mensaje:** `ci(store): validate Release-Store MSIX in Windows pipeline`
  - **Fecha de commit:** 2026-11-07
- **Ausencia de PR Específica para Sprint 1.2:** La auditoría de la API de GitHub confirma que no existió una Pull Request previa ni una rama remota dedicada para fusionar Sprint 1.2. Se registra esta desviación de proceso de manera factual.

### B. Desacoplamiento de la PR #11
- La **PR #11** (`https://github.com/cristianbarreiro/PrivGuard/pull/11`) corresponde al Sprint anterior (Sprint 1.0 / 1.1: `feature/sprint-1-store-profile-guards-d01-d02`).
  - **Head SHA:** `391d79e9255cb07f87f6e13748225cd55fc6f514`
  - **Merge Commit:** `ffc2273ea55bd1fefb3041c1bcadfbbd2c9dce77`
  - **Estado:** Merged / Closed
- **Aclaración no negociable:** La PR #11 **no contiene ni debe atribuirse al Sprint 1.2**.

---

## 3. Evidencia Verificable de GitHub Actions

### A. Ejecución del Workflow CI (Run #89)
- **Workflow Run ID:** `42379421580`
- **URL:** [https://github.com/cristianbarreiro/PrivGuard/actions/runs/42379421580](https://github.com/cristianbarreiro/PrivGuard/actions/runs/42379421580)
- **Commit analizado:** `d8ca7acc703397cabec8d5cbba92f39329fa940a`
- **Rama:** `main`
- **Evento disparador:** `push`
- **Estado:** `completed`
- **Conclusión:** `success`

### B. Matriz de Jobs Ejecutados (7/7 Exitosos)

| Job ID | Nombre del Job | Runner / Target | Estado | Conclusión | Comportamiento del Gate Release-Store |
|---|---|---|---|---|---|
| `128423706121` | Build & Test (ubuntu-latest) | Linux Ubuntu | completed | success | Paso `Validate Release-Store...` omitido (`skipped`) |
| `128423706325` | Build & Test (macos-latest) | macOS | completed | success | Paso `Validate Release-Store...` omitido (`skipped`) |
| `128423706329` | Build & Test (windows-latest) | Windows | completed | success | Paso `Validate Release-Store...` ejecutado con éxito (`success`) |
| `128424247825` | Publish Single-File (linux-x64) | Linux x64 | completed | success | No aplica (publish job independiente) |
| `128424247826` | Publish Single-File (osx-arm64) | macOS ARM64 | completed | success | No aplica (publish job independiente) |
| `128424247857` | Publish Single-File (win-arm64) | Windows ARM64 | completed | success | No aplica (publish job independiente) |
| `128424247948` | Publish Single-File (win-x64) | Windows x64 | completed | success | No aplica (publish job independiente) |

### C. Detalle del Paso Windows `Validate Release-Store MSIX Packaging (Windows)`
En el job `Build & Test (windows-latest)` (ID `128423706329`), el paso número 10 ejecutó secuencialmente:
1. `dotnet build PrivGvard.sln -c Release-Store --no-restore` -> Exitoso (código 0).
2. `./scripts/build-msix.ps1 -Architecture x64 -Configuration Release-Store -ValidateStoreIdentity -OutputDir $outputDir` -> Exitoso (código 0). Generó `PrivGuard-2.0.0.0-x64.msix`.
3. `./scripts/verify-store-msix.ps1 -PackageDir $outputDir` -> Exitoso (código 0). Verificó `AppxManifest.xml` interno.

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

1. **Evento `push` vs. `pull_request`:** El run #89 (`42379421580`) fue disparado por un evento `push` sobre `main`. Por sí solo, **no demuestra** que el evento `pull_request` ejecute el workflow de manera idéntica. La validación del evento `pull_request` permanece pendiente. Solo podrá considerarse completada cuando exista una ejecución real del workflow asociada a una PR contra `main`, con el SHA correspondiente y resultado satisfactorio en GitHub Actions.
2. **Comprobación Estática y de Empaque vs. Validación Dinámica en VM:** La generación e inspección de un paquete MSIX en CI comprueba que el compilador, las herramientas del Windows SDK (`makeappx`, `makepri`) y los esquemas XML son conformes. **No equivale ni sustituye la validación dinámica en VM** de la Fase 8B.1A.7.2.
3. **No Demuestra Certificación ni Aprobación de Microsoft Store:** El paso no ejecuta Windows App Certification Kit (WACK), no valida cuentas de Partner Center ni autoriza la publicación en la tienda.
4. **No Resuelve D-03, D-04 ni D-05:** La existencia de un paquete Store válido no aprueba la coexistencia entre ediciones (D-03), la modalidad de supervisión (D-04) ni la topología definitiva de almacenamiento (D-05).
5. **No Demuestra Aislamiento ni Recuperación tras Desinstalación:** No se ha demostrado en tiempo de ejecución cómo se comporta el contenedor Full Trust ante un reset o desinstalación de Windows.

---

## 7. Estado del Roadmap y Veredicto Final

### Veredicto del Sprint 1.2
**PASS — CI/packaging scope only**

El objetivo técnico delimitado del Sprint 1.2 (incorporar el gate de CI para `Release-Store`, empaquetar MSIX x64, inspeccionar el manifiesto interno y verificar casos positivos/negativos) fue implementado y verificado mediante GitHub Actions run #89, cuyo pipeline completó satisfactoriamente los 7 jobs correspondientes.

### Estado Global de Salvaguardas y Roadmap
- **Microsoft Store:** **CONDITIONAL HOLD**. Continúa bloqueado para publicación hasta resolver el aislamiento de datos y la validación en VM.
- **Fase 8B.1C:** **BLOCKED / NOT AUTHORIZED**. No se autoriza su desarrollo hasta completar los pre-requisitos de laboratorio y formalización de decisiones.
- **Próximo Paso Recomendado:** Publicar la rama documental y abrir la Pull Request hacia `main` para que GitHub Actions ejecute los checks en el evento `pull_request`, y posteriormente proceder con la preparación del entorno aislado para la validación dinámica de la Fase 8B.1A.7.2.

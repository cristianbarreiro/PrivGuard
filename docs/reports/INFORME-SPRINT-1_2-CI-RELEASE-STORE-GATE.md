# PrivGuard — Informe de Auditoría y Verificación: Sprint 1.2
## CI Gate para Validación de Empaquetado Release-Store en GitHub Actions

**Fecha:** 2026-11-07  
**Rama:** `ci/release-store-validation`  
**Base:** `origin/main` (`ffc2273ea55bd1fefb3041c1bcadfbbd2c9dce77`)  
**Autor:** Antigravity AI Pair Programmer (Rol: Senior .NET / Windows Packaging & CI Adversarial Reviewer)  
**Estado / Veredicto:** **PARTIAL — IMPLEMENTED, CI PENDING**

---

## 1. Resumen Ejecutivo

El presente Sprint 1.2 implementa y valida localmente el **CI Gate automatizado para la configuración `Release-Store`** en PrivGuard, garantizando que el flujo de integración continua (`.github/workflows/ci.yml`) verifique de forma no negociable el empaquetado MSIX x64, la identidad oficial de Microsoft Partner Center y la política de capacidades restringidas (D-01) extrayendo e inspeccionando el `AppxManifest.xml` dentro del contenedor empaquetado real.

Se preservaron estrictamente los límites arquitectónicos del sprint:
- **No se modificaron ni relajan las guardas de D-01/D-02** implementadas en el Sprint 1.1.
- **No se modificó la rama `main` directamente** ni se reabrió el PR #11 (cerrado e integrado como `ffc2273`).
- **No se implementaron cambios sobre D-03, D-04 ni D-05** (ownership coordination, SupervisionOnly, topología definitiva de almacenamiento).
- **No se ejecutaron mutaciones nativas en el host** (`ConsentStore`, `HKLM`, `PnP`, `WASAPI`).
- **Se mantiene Microsoft Store en `CONDITIONAL HOLD`** y la Fase 8B.1C en `BLOCKED / NOT AUTHORIZED` hasta completar los gates de laboratorio físico en VM.

---

## 2. Preflight y Baseline de Control de Versiones

1. **Estado de PR #11 y Commit Sprint 1.1:**
   - Commit Sprint 1.1: `391d79e9255cb07f87f6e13748225cd55fc6f514` ("fix(store): harden privileged profile immutability and reject BlockTarget.Both partial mutations in Store").
   - Integrado en `origin/main` mediante merge commit `ffc2273ea55bd1fefb3041c1bcadfbbd2c9dce77` ("Merge pull request #11 from cristianbarreiro/feature/sprint-1-store-profile-guards-d01-d02").
   - Se confirmó mediante `git log origin/main` que `391d79e` es ancestro directo de `origin/main`.
2. **Creación de Rama Aislada:**
   - Se creó y checkout de la rama `ci/release-store-validation` directamente desde `origin/main` (`ffc2273`).
   - El árbol de trabajo local no tocó `main` directamente.

---

## 3. Arquitectura y Diseño del CI Gate

### A. Ubicación en `.github/workflows/ci.yml`
El paso de validación fue incorporado al final del job `build-and-test` con condición de runner:
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
- **Aislamiento Multiplataforma:** La condición `if: runner.os == 'Windows'` asegura que los runners de Ubuntu y macOS continúen ejecutando sus pruebas multiplataforma sin invocar MakeAppx ni herramientas de Windows SDK.
- **Reutilización de Artefactos Restaurados:** Se ejecuta después de la compilación y pruebas de `PrivLock.Platform.Windows.Tests`, empleando `--no-restore` para evitar colisiones concurrentes de NuGet.
- **Directorio Fuera del Repositorio:** Utiliza `$env:RUNNER_TEMP\PrivGuard-MSIX-CI` en CI y fallback local a `publish_out/msix-ci`.

### B. Motor de Inspección del Manifiesto Interno
Se desarrollaron dos capas complementarias de verificación estricta:

1. **C# Helper de Producción / Prueba (`StoreMsixManifestVerifier.cs`):**
   - Extrae `AppxManifest.xml` directamente desde streams o archivos `.msix` (ZIP container).
   - Usa `System.Xml.Linq` (`XDocument`) con validación de namespaces XML formales:
     - Foundation: `http://schemas.microsoft.com/appx/manifest/foundation/windows10`
     - RestrictedCapabilities (`rescap`): `http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities`
   - Valida identidad exacta:
     - `Identity/Name`: `cdevstudios.PrivGuard`
     - `Identity/Publisher`: `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
     - `Properties/PublisherDisplayName`: `cdev studios`
   - Rechaza explícitamente manifiestos de DevLab (`cdevstudios.PrivGuard.DevLab`, `CN=PrivGuardDevLab`).
   - Valida capacidades D-01:
     - Requiere la presencia de `runFullTrust` en el namespace `rescap`.
     - Exige que `runFullTrust` sea la **única** capacidad restringida declarada.
     - Prohíbe explícitamente `rescap:allowElevation` y `rescap:unvirtualizedResources` en cualquier nodo del documento.
   - Valida cardinalidad: falla ante 0 o múltiples paquetes `.msix` cuando se apunta a un directorio.

2. **Script PowerShell Autónomo (`scripts/verify-store-msix.ps1`):**
   - Invocable por CI y por desarrolladores.
   - Compatible con **PowerShell 5.1** (Windows PowerShell de escritorio) y **PowerShell 7+** (`pwsh`).
   - No utiliza operadores sintácticos exclusivos de PS7 (como `?.`).
   - Retorna código de salida `0` si y solo si la verificación es exitosa; retorna código `1` y emite error detallado ante cualquier incumplimiento.

3. **Endurecimiento de `scripts/build-msix.ps1`:**
   - La función interna `Test-MsixPackageArchive` fue migrada de comparaciones de texto plano/regex a parsing XML formal con `System.Xml.Linq` respetando namespaces de `rescap`.

---

## 4. Archivos Creados y Modificados

| Ruta del Archivo | Acción | Propósito |
|---|---|---|
| `.github/workflows/ci.yml` | Modificado | Incorpora el paso de validación `Validate Release-Store MSIX Packaging (Windows)`. |
| `scripts/build-msix.ps1` | Modificado | Migra la verificación interna del MSIX a `System.Xml.Linq` con namespaces XML formales. |
| `scripts/verify-store-msix.ps1` | Creado | Script PowerShell standalone para validar `.msix` o carpetas de salida en CI y local. |
| `src/PrivLock.Platform.Windows/Packaging/StoreMsixManifestVerifier.cs` | Creado | Motor de inspección C# de manifiestos MSIX con tipado estricto y sin advertencias de compilación. |
| `tests/PrivLock.Platform.Windows.Tests/StoreMsixPackageVerificationTests.cs` | Creado | 16 pruebas unitarias e integración que cubren todos los casos positivos y negativos requeridos. |
| `docs/ai/AUDIT-ROADMAP.md` | Modificado | Registra Sprint 1.1 como integrado y Sprint 1.2 como IMPLEMENTED (CI PENDING). |
| `docs/ai/PROJECT_CONTEXT.md` | Modificado | Sincroniza la sección de pruebas sin recuentos numéricos volátiles. |
| `docs/reports/INFORME-SPRINT-1_2-CI-RELEASE-STORE-GATE.md` | Creado | Informe técnico formal de auditoría y evidencia del sprint. |

---

## 5. Matriz de Cobertura de Pruebas Unitarias

La suite `StoreMsixPackageVerificationTests.cs` evalúa de forma exhaustiva los casos positivos y negativos en memoria (sin requerir MakeAppx ni certificados instalados):

1. `VerifyManifestXml_ValidOfficialManifest_Succeeds`: Manifiesto oficial Store con `runFullTrust` -> **Éxito**.
2. `VerifyManifestXml_IncorrectPackageName_Fails`: Manifiesto con Name no autorizado -> **Falla**.
3. `VerifyManifestXml_IncorrectPublisher_Fails`: Manifiesto con Publisher no autorizado -> **Falla**.
4. `VerifyManifestXml_IncorrectPublisherDisplayName_Fails`: Manifiesto con DisplayName incorrecto -> **Falla**.
5. `VerifyManifestXml_DevLabManifest_Fails`: Manifiesto de DevLab accidental -> **Falla**.
6. `VerifyManifestXml_MissingRunFullTrust_Fails`: Manifiesto sin capacidad `runFullTrust` -> **Falla**.
7. `VerifyManifestXml_ForbiddenAllowElevation_Fails`: Manifiesto con `rescap:allowElevation` (violación D-01) -> **Falla**.
8. `VerifyManifestXml_ForbiddenUnvirtualizedResources_Fails`: Manifiesto con `rescap:unvirtualizedResources` (violación D-01) -> **Falla**.
9. `VerifyManifestXml_MultipleRestrictedCapabilities_Fails`: Declaración de capacidades restringidas adicionales -> **Falla**.
10. `VerifyPackageFile_MissingAppxManifestInArchive_Fails`: Archivo MSIX corrupto sin manifiesto -> **Falla**.
11. `VerifyPackageDirectory_NoMsixPackages_Fails`: Directorio vacío sin paquetes -> **Falla**.
12. `VerifyPackageDirectory_MultipleMsixPackages_Fails`: Directorio con más de un paquete -> **Falla**.
13. `PowerShellVerifyScript_OnValidPackage_ExitsZero`: Invocación del script `.ps1` en paquete válido -> Exit Code `0`.
14. `PowerShellVerifyScript_OnForbiddenAllowElevation_ExitsOne`: Invocación del script `.ps1` ante capacidad prohibida -> Exit Code `1`.
15. `PowerShellVerifyScript_OnEmptyDirectory_ExitsOne`: Invocación del script `.ps1` ante directorio vacío -> Exit Code `1`.

---

## 6. Resultados de Comandos de Validación Local

### 1. `dotnet build PrivGvard.sln --no-restore`
- **Código de salida:** `0`
- **Advertencias:** `0`
- **Errores:** `0`

### 2. `dotnet test PrivGvard.sln --no-restore`
- **Código de salida:** `0`
- **Resultados por Suite:**
  - `PrivLock.Domain.Tests`: 25 superadas, 0 con error.
  - `PrivLock.Infrastructure.Tests`: 90 superadas, 0 con error.
  - `PrivLock.Application.Tests`: 167 superadas, 0 con error.
  - `PrivLock.Platform.Windows.Tests`: 314 superadas, 0 con error (incluye las 16 nuevas pruebas de verificación de manifiesto Store y ejecución del script PowerShell).
  - **Total de pruebas:** 596 superadas, 0 fallos.

### 3. `dotnet format whitespace PrivGvard.sln --no-restore --verify-no-changes`
- **Código de salida:** `0` (formato de espacios en blanco 100% verificado sin desviaciones).

### 4. Build de Configuración `Release-Store`
- **Comando:** `dotnet build PrivGvard.sln -c Release-Store --no-restore`
- **Código de salida:** `0`
- **Advertencias:** `0`
- **Errores:** `0`

### 5. Generación Local de MSIX x64 con `-ValidateStoreIdentity`
- **Comando:** `powershell -ExecutionPolicy Bypass -File .\scripts\build-msix.ps1 -Architecture x64 -Configuration Release-Store -ValidateStoreIdentity -OutputDir publish_out\msix-ci-test`
- **Código de salida:** `0`
- **Salida Relevante:**
  - `Store identity validation: PASS`
  - `Package creation succeeded.`
  - `Capabilities (D-01): runFullTrust (sole capability verified)`
  - `Verification: PASS (Identity, Version, Architecture, D-01 Capabilities, Executable, PRI & 7 Visual Assets)`

### 6. Inspección de Manifiesto con `scripts/verify-store-msix.ps1`
- **Comando:** `powershell -ExecutionPolicy Bypass -File .\scripts\verify-store-msix.ps1 -PackageDir publish_out\msix-ci-test`
- **Código de salida:** `0`
- **Salida:**
  ```text
  ==========================================================
   Verifying Store MSIX Package: PrivGuard-2.0.0.0-x64.msix
  ==========================================================
     Identity/Name:                   cdevstudios.PrivGuard
     Identity/Publisher:              CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD
     Properties/PublisherDisplayName: cdev studios
     Identity/Version:                2.0.0.0
     Identity/ProcessorArchitecture:  x64
     Capabilities (D-01):             runFullTrust (sole restricted capability verified)
     Forbidden Capabilities:          0 (allowElevation: absent, unvirtualizedResources: absent)

  ==========================================================
   Store MSIX Verification: PASS
  ==========================================================
  ```

---

## 7. Riesgos Residuales y Próximos Pasos

1. **Dependencia de Ejecución en Actions:** El paso de empaquetado y validación de MSIX en GitHub Actions requiere que la imagen `windows-latest` proporcione `makeappx.exe` y el SDK de Windows 10/11 en la ruta estándar de kits. Si bien esto está garantizado en los runners oficiales de GitHub Actions de Windows, la operatividad del gate solo podrá declararse `PASS` una vez que un workflow remoto complete la ejecución con éxito.
2. **Estado de Microsoft Store:** Microsoft Store permanece en **CONDITIONAL HOLD**. Este gate automatiza la validación técnica del paquete, pero **no autoriza la publicación en Partner Center**.
3. **Fase 8B.1C:** Permanece **BLOCKED / NOT AUTHORIZED** hasta formalizar las decisiones de coexistencia (D-03) y validar el comportamiento de virtualización de almacenamiento de MSIX Full Trust en la VM de laboratorio.

---

## 8. Veredicto Final

**PARTIAL — IMPLEMENTED, CI PENDING**

Todos los componentes del gate de CI han sido implementados, testeados unitariamente con casos positivos y negativos, y verificados en empaquetado real local. El estado final quedará elevado a `PASS` una vez que el PR hacia `main` sea evaluado y el pipeline de GitHub Actions se ejecute en verde.

# Informe de Fase 8B.1C — Store Release Candidate: Implementación y Empaquetado

**Fecha:** 9 de noviembre de 2026  
**Rama activa:** `main` (commit base: `47410df712c094effc2aa747b5dec1615d717a28`)  
**Autor:** Antigravity (Advanced Agentic Coding)  
**Veredicto canónico:** `STORE RELEASE CANDIDATE — EXTERNAL LAB VALIDATION PENDING`

---

## 1. Resumen Ejecutivo y Veredicto Canónico

En la Fase **8B.1C**, se ha implementado de forma integral y hermética la arquitectura dual de **PrivGuard Store Edition**, completando las decisiones de diseño D-01, D-02, D-03, D-04 y D-05, generando el paquete candidato a publicación para Microsoft Store y verificando su conformidad técnica mediante pruebas automatizadas y herramientas oficiales del Windows SDK.

### Dictamen Técnico
```text
========================================================================================
 VEREDICTO: STORE RELEASE CANDIDATE — EXTERNAL LAB VALIDATION PENDING
========================================================================================
 [PASS] D-01: Manifiesto MSIX con identidad oficial y rescap:runFullTrust como única capability restringida.
 [PASS] D-02: Experiencia Store con atajo a ms-settings:privacy-webcam, silencio WASAPI y aviso de desinstalación.
 [PASS] D-03: Aislamiento estricto de directorios (%LOCALAPPDATA%\PrivGuard\Store) sin migración legacy.
 [PASS] D-04: Coordinador Cross-Edition (WindowsCrossEditionCoordinator) con protección de PID reciclado y modo supervisión.
 [PASS] D-05: Inyección completa de IEditionProfileResolver en Program.cs y raíz de composición DI.
 [PASS] Tests: 632/632 pruebas unitarias e integración en VERDE (0 errores, 0 fallos, 0 omitidos).
 [PASS] Packaging: build-msix.ps1 y verify-store-msix.ps1 en VERDE (Identity PASS, D-01 PASS).
 [HOLD] Validación Dinámica VM: Retenida por entorno de ejecución (Guest VM sin virtualización anidada).
========================================================================================
```

### Artefacto Release Candidate Generado
- **Nombre de archivo:** `PrivGuard-2.0.0.0-x64.msix`
- **Ruta relativa:** `release/store/PrivGuard-2.0.0.0-x64.msix`
- **Tamaño:** `58.573.769 bytes` (~55,86 MB)
- **Hash SHA-256:** `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`
- **Configuración de compilación:** `Release-Store` (`STORE_EDITION`)
- **Arquitectura de destino:** `win-x64`
- **SDK / Runtime:** .NET SDK `10.0.403` / Target Framework `net10.0-windows10.0.19041.0`
- **Ficha de proveniencia:** `release/store/BUILD-PROVENANCE.json`

---

## 2. Decisiones de Diseño e Implementación Técnica

### D-01: Endurecimiento del Manifiesto y Capacidades Restringidas
- **Identidad de Microsoft Store:**
  - `Identity/Name`: `cdevstudios.PrivGuard`
  - `Identity/Publisher`: `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
  - `Properties/PublisherDisplayName`: `cdev studios`
  - `Identity/Version`: `2.0.0.0`
- **Capacidades permitidas:** Exactamente una capacidad restringida declarada: `rescap:runFullTrust`.
- **Capacidades prohibidas:** `allowElevation` y `unvirtualizedResources` se encuentran estrictamente ausentes.
- **Validación automática:** Tanto `StoreMsixManifestVerifier.cs` como `scripts/verify-store-msix.ps1` validan el paquete MSIX real extrayendo y parseando `AppxManifest.xml` directamente del contenedor ZIP.

### D-02: Experiencia de Usuario y Delimitación Funcional en Store Edition
- **Cámara:** En Store Edition, la mutación directa de `ConsentStore` o desactivación de nodos PnP está bloqueada. La interfaz presenta un botón directo "Configuración" (`OpenCameraPrivacySettingsCommand`) que invoca `ms-settings:privacy-webcam`.
- **Micrófono:** Se soporta silenciamiento de software mediante WASAPI/CoreAudio (`CompareAndMuteEndpoint`). La interfaz refleja la capacidad de hardware del dispositivo (`Supported`, `NotSupported` con aviso de que aplicaciones con modo exclusivo podrían omitir el silencio de software, o `Unknown`).
- **Aviso persistente de desinstalación:** Dado que el desinstalador de MSIX de Windows no ejecuta código de la aplicación ni revierte el estado de silenciamiento al desinstalar, la UI muestra un banner informativo persistente cuando el micrófono está silenciado, advirtiendo al usuario de restaurar el micrófono antes de desinstalar.

### D-03: Aislamiento Completo de Almacenamiento
- **Store Edition:** Resuelve determinísticamente todas sus rutas bajo `%LOCALAPPDATA%\PrivGuard\Store`:
  - Datos y estado deseado: `%LOCALAPPDATA%\PrivGuard\Store\state.json`
  - Recuperación y WAL: `%LOCALAPPDATA%\PrivGuard\Store\Recovery\`
  - Logs rotativos: `%LOCALAPPDATA%\PrivGuard\Store\Logs\`
  - Informes de fallos: `%LOCALAPPDATA%\PrivGuard\Store\CrashReports\`
  - Mutex de instancia única: `Global\PrivGuard_Store_SingleInstance`
- **Standalone Edition:** Conserva de forma estricta sus rutas bajo `%LOCALAPPDATA%\PrivGuard\Standalone` y mutex `Global\PrivGuard_Standalone_SingleInstance`.
- **Incompatibilidad Legacy:** `DefaultLegacyArtifactDetector` implementa un chequeo explícito de `IEditionProfileResolver`. Si `Profile != Standalone`, retorna inmediatamente `false`, garantizando que la edición Store nunca lea ni migre artefactos legacy de PrivLock.

### D-04: Coordinador de Coexistencia entre Ediciones (Cross-Edition Coordinator)
Se implementó el contrato `ICrossEditionCoordinator` y su implementación nativa `WindowsCrossEditionCoordinator`:
- **Atomicidad y Ámbito:** Usa un mutex con ámbito de sesión de usuario (`Local\PrivGuard_CrossEdition_Lock`), eliminando colisiones en entornos multi-usuario.
- **Descriptors Aislados:** Cada edición almacena su descriptor exclusivamente en su propio directorio de `LocalAppData` (`%LOCALAPPDATA%\PrivGuard\<Edition>\cross-edition-latch.json`).
- **Regla de No Injerencia:** Cada edición escribe y elimina **únicamente** su propio descriptor. Ninguna edición puede borrar ni sobrescribir el descriptor de la edición opuesta.
- **Protección de PID Reciclado:** Al verificar si un proceso está vivo, no basta con `Process.GetProcessById(pid)`. Se valida comparando `process.StartTime.ToUniversalTime()` contra `ProcessStartTimeUtc` registrado. Si el PID fue reciclado por otro proceso del sistema, se detecta la discrepancia y se clasifica como `AbandonedByOtherEdition`.
- **Modo Solo Supervisión (`SupervisionOnly`):** Si la edición opuesta tiene una sesión activa o abandonada, la edición entrante entra en modo supervisión. Se bloquean los toggles de mutación (`IsCameraToggleEnabled = false`, `IsMicToggleEnabled = false`, `IsBothToggleEnabled = false`) y se presenta un banner explicativo en la UI informando al usuario.
- **Fail-Closed:** Descriptores dañados o ilegibles reportan `CorruptedOrIncomplete` y bloquean la mutación del sistema.

### D-05: Inyección en Raíz de Composición DI (`Program.cs`)
- En `Program.Main`, si `profileResolver.Profile == EditionProfile.UntrustedOrIndeterminate`, la aplicación registra el error en `Trace` y termina con código de salida 1 de inmediato, sin tocar disco ni abrir journals.
- Se configuran los directorios de logging (`LoggingConfiguration.Initialize(profileResolver.LogsDirectory)`) y crash reports (`CrashReporter.ConfigureDefaultDirectory(profileResolver.CrashReportsDirectory)`).
- En `ConfigureServices`, `IEditionProfileResolver` se inyecta en `FileStateStore`, `FilePrivacySessionStore`, `FileActivePrivacySessionMarker`, `DefaultLegacyArtifactDetector`, `WindowsCrossEditionCoordinator` y `WindowsSingleInstanceGuard`.

---

## 3. Evidencia de Verificación y Resultados de Pruebas

### 3.1 Compilación de la Solución
- **Debug:** `dotnet build PrivGvard.sln` -> 0 advertencias, 0 errores.
- **Release-Store:** `dotnet build PrivGvard.sln -c Release-Store` -> 0 advertencias, 0 errores.

### 3.2 Cobertura de Pruebas Automatizadas
Ejecución completa sobre `PrivGvard.sln` (`dotnet test PrivGvard.sln --no-build`):

| Proyecto de Pruebas | Pruebas Superadas | Falladas | Omitidas | Duración |
| :--- | :---: | :---: | :---: | :---: |
| `PrivLock.Domain.Tests` | **25** | 0 | 0 | 0.35 s |
| `PrivLock.Infrastructure.Tests` | **95** | 0 | 0 | 4.0 s |
| `PrivLock.Application.Tests` | **167** | 0 | 0 | 4.0 s |
| `PrivLock.Platform.Windows.Tests` | **345** | 0 | 0 | 16.0 s |
| **TOTAL GENERAL** | **632** | **0** | **0** | **24.35 s** |

Nuevas pruebas añadidas en esta fase:
- `StoreEditionStorageIsolationTests.cs` (5 tests): Comprueba persistencia de estado en Store, almacenamiento de WAL en Recovery de Store, inhibición de migración legacy en Store, y direccionamiento de CrashReporter.
- `WindowsCrossEditionCoordinatorTests.cs` (7 tests): Inspección inicial libre, adquisición atómica, rechazo ante edición opuesta activa, detección de PID reciclado, fail-closed ante JSON corrupto, eliminación exclusiva del descriptor propio, y resolución del mutex de Store.
- `StoreEditionViewModelSupervisionTests.cs` (3 tests): Comportamiento de `MainViewModel` en Store Edition (redirección a ajustes de cámara), activación del banner de modo supervisión con bloqueo de toggles, y visualización/ocultación del aviso de desinstalación de micrófono.

### 3.3 Empaquetado y Verificación MSIX
Ejecución de `scripts/build-msix.ps1`:
```powershell
.\scripts\build-msix.ps1 -Architecture x64 -Configuration Release-Store -ValidateStoreIdentity -OutputDir release\store
```
Resultado: **EXIT CODE 0**  
Generado `release\store\PrivGuard-2.0.0.0-x64.msix` y recursos PRI con 7 variantes de logotipos.

Ejecución de `scripts/verify-store-msix.ps1`:
```powershell
.\scripts\verify-store-msix.ps1 -PackageDir release\store
```
Resultado: **PASS**
- Identidad: `cdevstudios.PrivGuard` / `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
- Display Name: `cdev studios`
- Capacidades D-01: `runFullTrust` (única capability)
- Capacidades prohibidas: 0 (`allowElevation` ausente, `unvirtualizedResources` ausente)

### 3.4 Evaluación de Windows App Certification Kit (WACK)
- La herramienta `appcert.exe` se encuentra en `C:\Program Files (x86)\Windows Kits\10\App Certification Kit\appcert.exe`.
- Al ejecutarse en el host actual, devolvió `Solicitud no compatible` debido a las restricciones del entorno guest/virtualizado no interactivo.
- Esta limitación está prevista y formalmente categorizada: no constituye un defecto del paquete sino un requisito de validación en laboratorio físico externo antes de la publicación final en producción.

---

## 4. Estado de Archivos y Plan de Acción Inmediato

### Archivos Modificados / Creados
1. **Contratos e Infraestructura:**
   - `src/PrivLock.Platform.Abstractions/ICrossEditionCoordinator.cs` (Nuevo)
   - `src/PrivLock.Infrastructure.Common/Storage/NoOpCrossEditionCoordinator.cs` (Nuevo)
   - `src/PrivLock.Infrastructure.Common/Storage/FileStateStore.cs` (Sobrecarga con `IEditionProfileResolver`)
   - `src/PrivLock.Infrastructure.Common/Storage/FilePrivacySessionStore.cs` (Sobrecarga con `IEditionProfileResolver`)
   - `src/PrivLock.Infrastructure.Common/Storage/DefaultLegacyArtifactDetector.cs` (Guarda de Store Edition)
   - `src/PrivLock.Infrastructure.Common/Logging/CrashReporter.cs` (`ConfigureDefaultDirectory`)
   - `src/PrivLock.Infrastructure.Common/Localization/LocalizationCatalog.cs` (Cadenas ES/EN de supervisión y desinstalación)
2. **Plataforma Windows:**
   - `src/PrivLock.Platform.Windows/System/WindowsCrossEditionCoordinator.cs` (Nuevo)
   - `src/PrivLock.Platform.Windows/System/WindowsSingleInstanceGuard.cs` (Constructor con `IEditionProfileResolver`)
3. **Aplicación y Host:**
   - `src/PrivLock.Application/Services/PrivacySessionService.cs` (Integración con `ICrossEditionCoordinator`)
   - `src/PrivLock.Application/Services/ProtectionService.cs` (Guardas de modo supervisión y sobrecargas DI)
   - `src/PrivLock.Desktop/Program.cs` (Fallo cerrado en untrusted, inyección de storage profile y coordinador)
4. **Presentación (UI):**
   - `src/PrivLock.UI/ViewModels/MainViewModel.cs` (Propiedades observables `IsSupervisionOnly`, avisos de Store y guardas de toggles)
   - `src/PrivLock.UI/Views/MainWindow.axaml` (Banner de supervisión, banner de aviso Store y botón a ajustes de cámara)
5. **Pruebas:**
   - `tests/PrivLock.Infrastructure.Tests/StoreEditionStorageIsolationTests.cs` (Nuevo)
   - `tests/PrivLock.Platform.Windows.Tests/WindowsCrossEditionCoordinatorTests.cs` (Nuevo)
   - `tests/PrivLock.Platform.Windows.Tests/StoreEditionViewModelSupervisionTests.cs` (Nuevo)
6. **Artefactos y Proveniencia:**
   - `release/store/PrivGuard-2.0.0.0-x64.msix`
   - `release/store/BUILD-PROVENANCE.json`

### Próximos Pasos para Publicación
1. Realizar commit local de los cambios en `main` conforme a la instrucción expresa del usuario.
2. Ejecutar WACK en una máquina Windows física o entorno interactivo con soporte de gráficos.
3. Subir el paquete `PrivGuard-2.0.0.0-x64.msix` a Microsoft Partner Center para la certificación oficial de Store.

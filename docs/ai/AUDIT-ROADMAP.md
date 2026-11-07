# PrivGvard — Roadmap de Evolución y Estado de Auditoría

**Estado:** Activo / PrivGuard 2.0<br />
**Fecha de actualización:** 2026-11-06<br />
**Repositorio:** `PrivGuard`<br />
**Rama activa:** `main`<br />
**Identidad del producto:** PrivGuard (ejecutable interno: `PrivGvard.exe` / `PrivGvard`, paquete Store: `cdevstudios.PrivGuard`)<br />
**Postura de soporte:** Windows es la plataforma principal soportada para protección y recuperación de privacidad. Linux y macOS permanecen como scaffolds de descubrimiento/interfaz (`CapabilityLevel.None`) hasta que cuenten con adaptadores de persistencia y rollback reversibles.

---

## 1. Resumen de Estado del Sistema

- `AGENTS.md`, `README.md` y `docs/recovery-validation.md`.
- La solución `PrivGvard.sln` (renombrada desde `CamMicBlocker.sln`), el host `src/PrivLock.Desktop` y la composición de dependencias.
- Servicios de aplicación, modelos de estado, almacenamiento, journal/recovery y cierre coordinado.
- Proveedores Windows, Linux y macOS, con especial atención a cualquier mutación del sistema.
- Tests, workflow de CI y la separación del árbol heredado `src/CamMicBlocker`.
- Compilación y suite local.

---

```text
dotnet build PrivGvard.sln --no-restore       -> correcto, 0 advertencias, 0 errores
dotnet test PrivGvard.sln --no-restore        -> correcto en todas las suites activas
```

### P0 — Salvaguardas de Seguridad Multiplataforma (Linux / macOS)

*Estado: BLOQUEO DE CAPACIDAD ACTIVO (`CapabilityLevel.None`)*

- **Linux (Controlador experimental de hardware)**:
  - `src/PrivLock.Platform.Linux/Devices/LinuxDeviceController.cs` contiene código experimental que utiliza `chmod 000` y `chmod 660` sin capturar modo, permisos POSIX, ACL, dueño ni reglas udev originales.
  - La manipulación de audio mediante `wpctl` y `pactl` no captura endpoints específicos, volumen original ni dependencias de sesión.
  - *Acción requerida*: Mantener `CapabilityLevel.None` y `UnsupportedPrivacySessionPlatformAdapter` en producción. No habilitar mutaciones en Linux hasta diseñar un contrato de snapshot reversible y verificación de estado efectivo.
- **macOS (Controlador experimental de hardware y límites TCC)**:
  - `src/PrivLock.Platform.MacOS/Devices/MacOSDeviceController.cs` restaura el volumen de entrada a un valor fijo (`75`) en vez de restituir el valor original capturado.
  - Los métodos de protección segura retornan éxito sin aplicar una barrera nativa efectiva.
  - TCC (Transparency, Consent, and Control) es una frontera de seguridad del SO; PrivGvard no puede revocar silenciosamente permisos de otras aplicaciones.
  - *Acción requerida*: Mantener `CapabilityLevel.None` y `UnsupportedPrivacySessionPlatformAdapter`. Tratar TCC como guía de usuario/inspección y no como mutación silenciosa.

---

### P1 — Endurecimiento y Validación de Entorno Físico (Windows) [RESUELTO EN FASE 6]

- **Matriz de aceptación en hardware / VM aislada**:
  - Validado en [docs/reports/PHASE-6-WINDOWS-HARDENING-REPORT.md](../reports/PHASE-6-WINDOWS-HARDENING-REPORT.md) y `tests/PrivLock.Platform.Windows.Tests/WindowsHardeningPhase6Tests.cs`:
    - Desconexión y reconexión física de cámaras/micrófonos (*hotplug*) durante sesión activa de bloqueo.
    - Cierre forzado del proceso (*kill / crash simulated*) y verificación de quiescencia y rollback (CRASH-B a D).
    - Cancelación de diálogo UAC por parte del usuario y contención de fallos sin mutaciones huérfanas (UAC-B y D).
    - Compatibilidad formal en Windows 11 (x64 y ARM64) y Windows 10 x64.
- **Propagación de contratos de resultado**:
  - UI refleja fielmente estados intermedios y degradados (`Unsupported`, `Denied`, `Failed`, `Unknown`, `OutcomeUncertain`, `Conflict`, `StandardActive`, `SecureActive`) basándose exclusivamente en observación nativa efectiva (`EffectiveStatus`).

---

### P2 — Arquitectura Dual y Microsoft Store (Fase 8A / 8A.1) [EN CONDITIONAL HOLD]

- **Auditoría Técnica y de Cumplimiento Regulatorio**:
  - Informe exhaustivo completado en `INFORME-FASE-8A1-VALIDACION-FINAL.md`.
  - **Decisión D-01 (Aprobada):** El perfil MSIX Store elimina `rescap:allowElevation` y `rescap:unvirtualizedResources`, conservando únicamente `rescap:runFullTrust`. La funcionalidad avanzada que requiere elevación queda reservada para Standalone. Eliminadas conclusiones no demostradas respecto a rechazos de cuentas individuales (la exclusión de elevación es una decisión de diseño proactiva).
  - **Decisión D-02 (Aprobada con condiciones):** Store Edition utilizará WASAPI para silenciar endpoints y ofrecerá acceso a la configuración nativa de privacidad (`ms-settings:`). No escribirá directamente en `ConsentStore`, `HKLM` ni administrará dispositivos PnP. La interfaz no afirmará bloqueo absoluto ni cobertura universal del micrófono, reflejando honestamente los matices de modo compartido/exclusivo y soporte de hardware (`QueryHardwareSupport`).
  - **Decisión D-03 (Aprobación parcial):** Aprobado el aislamiento independiente de configuración, diarios, backups, marcadores y registros de cada edición. Pendiente la validación del almacenamiento real de MSIX Full Trust en VM y el diseño de un coordinador de propiedad de recursos entre ambas ediciones.
  - **Estado de Envío a Store:** **CONDITIONAL HOLD**. Bloqueado hasta completar las pruebas de laboratorio de almacenamiento y la implementación de la Fase 8B.

---

### P3 — Mejoras Post-2.0 y Roadmap Futuro

- **Adaptador nativo de audio en Linux**:
  - Diseñar un proveedor PipeWire / WirePlumber nativo que capture nodos de audio estables, volumen y estado de mute con rollback exacto.
- **Adaptador nativo de audio en macOS**:
  - Implementar captura y restauración de volumen/mute por AudioDeviceID exacto mediante CoreAudio HAL.
- **Migración de identificadores internos**:
  - Cuando se apruebe una migración interna de código, planificar el renombrado coordinado de espacios de nombres `PrivLock.*` y la solución `PrivGvard.sln` hacia `PrivGuard`, garantizando la migración de llaves de registro de propiedad y mutex.

---

## 3. Elementos Resueltos y Consolidados (Histórico de Auditoría)

| Elemento / Problema | Resolución | Evidencia |
|---|---|---|
| **Topología de CI y fallo en Ubuntu** | **RESUELTO** | CI en `.github/workflows/ci.yml` divide la ejecución: pruebas de Dominio, Aplicación e Infraestructura corren en todas las plataformas; pruebas de Windows se ejecutan únicamente si `runner.os == 'Windows'`. |
| **Localización del menú de System Tray** | **RESUELTO** | `App.axaml.cs` actualiza dinámicamente las cabeceras de `NativeMenuItem` ("Abrir PrivGvard" / "Open PrivGvard", "Salir" / "Exit") al alternar idioma en `LocalizationCatalog`. |
| **Hover y pulsado del botón Cerrar (X)** | **RESUELTO** | Estilo en `App.axaml` (`Button.titlebar-close`) asegura fondo rojo y glifo 'X' blanco en estados hover y pressed. |
| **Identidad de Ejecutable y Metadatos** | **RESUELTO** | `PrivLock.Desktop.csproj` define `<AssemblyName>PrivGvard</AssemblyName>`, produciendo `PrivGvard.exe` con metadatos de producto `PrivGvard 2.0.0`. |
| **Generación de Instalador y Portable** | **RESUELTO** | `build-installer.ps1` e `installer/setup.iss` compilan `PrivGvard-Setup-2.0.0.exe` y `PrivGvard-Portable-2.0.0.zip` en `installer_out/`. |
| **Migración Segura de Datos y Logs** | **RESUELTO** | `StorageMigrationHelper` migra automáticamente configuraciones y registros de `%LOCALAPPDATA%\PrivLock` hacia `%LOCALAPPDATA%\PrivGvard`. |
| **Cuarentena del Código Heredado (WPF)** | **RESUELTO** | Código 1.x aislado en `legacy/CamMicBlocker` con guardas de compilación para evitar compilación accidental. |
| **Eliminación de Recuentos Fijos de Tests** | **RESUELTO** | Recuentos volátiles eliminados de la documentación permanente para evitar obsolescencia rápida. |
| **Validación de Ciclo de Vida y Reinicio (Fase 4)** | **RESUELTO** | `tests/PrivLock.Application.Tests/RestartLifecycleValidationTests.cs` valida Test A (Cámara), Test B (Micrófono), Test C (Combinado), Test D (Desactivación de usuario), idempotencia multi-ciclo, fallos parciales de reconciliación segura, recuperación asimétrica y preservación estricta del invariante de separación de estados. |
| **Inicio Automático en Windows (Fase 5)** | **RESUELTO** | `tests/PrivLock.Platform.Windows.Tests/WindowsAutostartLifecycleTests.cs` valida exhaustivamente los 10 escenarios normativos de autostart (Test A al Test J): autorización, lectura efectiva, deshabilitación de registro, respeto de directivas de usuario y directivas de grupo de Windows, idempotencia y cero duplicados, arranque manual desacoplado, degradación ante plataformas no soportadas, guardia de instancia única bajo autostart, precedencia de recuperación sobre reconciliación y persistencia de desactivación explícita. Aceptación física en VM Windows 11 PASS. |
| **Endurecimiento de Windows y Preparación de Release (Fase 6)** | **RESUELTO** | `tests/PrivLock.Platform.Windows.Tests/WindowsHardeningPhase6Tests.cs` valida hotplug de hardware (HOTPLUG-A a D), barrera de inmovilidad/quiescencia en caídas (CRASH-B a D), mitigación de fallos en diario corrupto sin borrado destructivo, manejo de cancelación de UAC sin corrupción de DesiredState (UAC-B y D), y paquete MSIX Release (`publish_out\msix\PrivGvard-2.0.0.0-x64.msix`) validado con 0 errores. |
| **Preparación para Microsoft Store (Fase 7 - Etapas A y B)** | **RESUELTO** | Paquete MSIX generado con identidad `cdevstudios.PrivGuard` y validación estricta de Partner Center. Documentación de preflight completada. |
| **Auditoría Técnica y Validación Dual (Fase 8A / 8A.1)** | **CONDITIONAL HOLD (D-01/D-02/D-03)** | Auditoría de virtualización (`Helium\User.dat`), ciclo de vida MSIX (purga de `%LOCALAPPDATA%\Packages`), WASAPI Mute y políticas de Store. Eliminadas conclusiones no demostradas. Aprobada arquitectura dual: Store Edition (MSIX, `runFullTrust` únicamente, WASAPI Mute y atajos nativos de SO) y Standalone Edition (Inno Setup / Portable, Secure Protection completa con PnP y HKLM). Envío a Store en CONDITIONAL HOLD pendiente de validación física de almacenamiento en VM y diseño de coordinador de recursos. |
| **Contrato de Identidad y Fail-Closed (Fases 8B.1B.1 – 8B.1B.3)** | **RESUELTO** | Enums `PackageIdentityKind` y `EditionProfile` reordenados para garantizar defaults fail-closed (`DetectionFailed = 0`, `UntrustedOrIndeterminate = 0`). Snapshot inmutable en `DefaultEditionProfileResolver`. Implementación `WindowsPackageIdentityDetector` con P/Invoke `GetCurrentPackageFamilyName`. 554 tests unitarios pasando. |
| **Auditoría Correctiva de Coherencia MSIX y Storage (Fase 8B.1A.6.2)** | **RESUELTO** | Corrección de contradicciones documentales, análisis riguroso de `ReplaceFileW` (requisito de mismo volumen) y VFS de AppData post-1903. Establecida matriz de evidencia de 8 subsistemas. Documentado en `docs/reports/INFORME-FASE-8B1A6_2-CIERRE-CONTRADICCIONES-MATRIZ-EVIDENCIA-MSIX.md`. |
| **Preflight Laboratorio VM y Build MSIX Reproducible (Fase 8B.1A.7.1)** | **BUILD READY — VM STILL UNAVAILABLE** | Diagnóstico del host actual (Guest VM VirtualBox sin virtualización anidada y 3.98 GB RAM). Compilación 100% aislada de `PrivGuard-2.0.0.0-x64.msix` (SHA-256 `AE7E...071A`) y `BUILD-PROVENANCE.json` desde HEAD `9f856c0`. Runbook preparado para validación dinámica en Fase 8B.1A.7.2. |
| **Guardas Store D-01/D-02 e Inmutabilidad de Perfil (Sprint 1.0 / 1.1)** | **RESUELTO** | Integrado en `main` vía PR #11 (`ffc2273`, commit `391d79e`). Inmutabilidad de perfil y rechazo fail-closed de mutaciones parciales `BlockTarget.Both` en Store. |
| **Gate CI para Packaging Release-Store (Sprint 1.2)** | **IMPLEMENTED — CI PENDING** | Paso `Validate Release-Store MSIX Packaging (Windows)` incorporado en `.github/workflows/ci.yml`. Compila `Release-Store`, empaqueta MSIX x64 con `-ValidateStoreIdentity` y valida el manifiesto interno (`AppxManifest.xml`) con `StoreMsixManifestVerifier` y `verify-store-msix.ps1`. Pruebas unitarias positivas y negativas validadas localmente. Operatividad formal en CI sujeta a ejecución remota en GitHub Actions. |

---

## 4. Definition of Done para Soporte de una Plataforma

Para que una plataforma sea considerada **Soportada** (más allá de compilación o UI), debe cumplir:

1. Detección nativa estable del recurso físico o directiva del SO.
2. Captura exacta (*snapshot*) del estado previo (permisos, volumen, existencia de clave, etc.).
3. Mutación acotada exclusivamente al recurso especificado.
4. Verificación nativa posterior al bloqueo y posterior a la restauración (`EffectiveStatus`).
5. Registro persistente en diario Write-Ahead Log (WAL) con atestación de propiedad.
6. Reversibilidad garantizada ante fallo parcial, cancelación, reinicio o caída del proceso.
7. Tratamiento adecuado de dispositivos desconectados (*hotplug*).
8. Ejecución bajo mínimo privilegio sin retención innecesaria de permisos.
9. Pruebas unitarias de rollback y aceptación manual en hardware real.

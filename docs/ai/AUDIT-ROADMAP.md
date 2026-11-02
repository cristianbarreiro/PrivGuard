# PrivGvard — Roadmap de Evolución y Estado de Auditoría

**Estado:** Activo / PrivGvard 2.0<br />
**Fecha de actualización:** 2026-09-12<br />
**Repositorio:** `Cam&MicroBlocker`<br />
**Rama activa:** `dev`<br />
**Identidad del producto:** PrivGvard (ejecutable: `PrivGvard.exe` / `PrivGvard`)<br />
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

### P2 — Mejoras Post-2.0 y Roadmap Futuro

- **Adaptador nativo de audio en Linux**:
  - Diseñar un proveedor PipeWire / WirePlumber nativo que capture nodos de audio estables, volumen y estado de mute con rollback exacto.
- **Adaptador nativo de audio en macOS**:
  - Implementar captura y restauración de volumen/mute por AudioDeviceID exacto mediante CoreAudio HAL.
- **Migración de identificadores internos**:
  - Cuando se apruebe una migración interna de código, planificar la renonmbrada coordinada de espacios de nombres `PrivLock.*` y la solución `CamMicBlocker.sln` hacia `PrivGvard`, garantizando la migración de llaves de registro de propiedad y mutex.

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
| **Endurecimiento de Windows y Preparación de Release (Fase 6)** | **RESUELTO** | `tests/PrivLock.Platform.Windows.Tests/WindowsHardeningPhase6Tests.cs` valida hotplug de hardware (HOTPLUG-A a D), barrera de inmovilidad/quiescencia en caídas (CRASH-B a D), mitigación de fallos en diario corrupto sin borrado destructivo, manejo de cancelación de UAC sin corrupción de DesiredState (UAC-B y D), y paquete MSIX Release (`publish_out\msix\PrivGvard-2.0.0.0-x64.msix`) validado con 0 errores y 418 pruebas exitosas. |

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

# Informe de Auditoría y Puerta de Congelación Final — Fase 8B.1F
**PrivGuard Store Edition — Pre-Submission Compliance, Listing Reconciliation, Privacy Audit & Release Freeze Gate**

**Fecha:** 9 de Noviembre de 2026  
**Identidad del Paquete:** `cdevstudios.PrivGuard`  
**Publisher:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` (`cdev studios`)  
**Versión:** `2.0.0.0` | **Arquitectura:** `x64` | **Configuración:** `Release-Store`  
**Dictamen Canónico:** **`READY FOR PARTNER CENTER DRAFT — REPOSITORY HANDOFF FROZEN`**  

---

## 1. Resumen Ejecutivo y Alcance de la Fase

La **Fase 8B.1F** constituye la compuerta final y definitiva de cumplimiento previo al envío de **PrivGuard Store Edition** a través de **Microsoft Partner Center**. Su propósito ha sido auditar exhaustivamente la privacidad del código fuente frente al comportamiento real, reconciliar las discrepancias documentales en la lista de capturas y formatos de subida, desvincular predicciones automáticas de clasificación de edad (IARC), aislar las decisiones comerciales exclusivas del propietario y congelar el expediente de entrega sin alterar el paquete binario candidato ni realizar commits o publicaciones remotas.

### Invariantes de Seguridad y Congelación Aplicadas:
1. **Cero Commits Locales en la Fase:** Se ha respetado de manera estricta la regla 0.1 (*"No crear commits locales. El propietario conserva el control de commits y push. Deja los cambios revisables y sin commit"*).
2. **Cero Pushes / Cero PRs / Cero Ramas:** No se ha ejecutado ninguna operación de red ni modificación en el repositorio remoto `origin/main`.
3. **Condición de Parada (Stop Condition):** No se generan fases posteriores (no hay Fase 8B.1G), no se generan prompts encadenados ni auditorías infinitas. La próxima acción es externa y corresponde al propietario en Microsoft Partner Center.
4. **No Fabricación de Contenedores Arbitrarios:** Se respeta la política de empaquetado seguro: no se generan archivos ZIP o `.msixupload` ficticios sin pipeline de símbolos; se documenta `.msix` como formato soportado y verificado.
5. **Auditoría de Privacidad Veraz:** Se erradica la afirmación genérica de "cero procesamiento de datos" para transparentar con exactitud el inventario de datos operativos locales (`state.json`, journal WAL, latch de coordinación, logs rotativos y crash dumps) frente a la certeza de cero transmisión por red.

---

## 2. Línea Base de Git y Estado del Árbol de Trabajo

- **Rama Activa:** `main`
- **Sincronización Remota:** Sincronizado en commit canónico `131198197a85446ad968c60d2799e419b12f7241` (`origin/main`).
- **Estado de Modificaciones:** Modificaciones de documentación y activos de entrega en el árbol de trabajo, deliberadamente **sin commit** para inspección y aprobación final del propietario:
  - Modificados: `docs/store/OWNER-DECISIONS.md`, `docs/store/PARTNER-CENTER-CHECKLIST.md`, `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`, `docs/store/PRIVACY-POLICY-DRAFT.md`, `docs/store/STORE-POLICY-COMPLIANCE.md`, `release/store/submission-handoff/ARTIFACT-MANIFEST.json`, `release/store/submission-handoff/PARTNER-CENTER-STEP-BY-STEP.md`, `release/store/submission-handoff/README-SUBMISSION-HANDOFF.md`, `release/store/submission-handoff/SHA256SUMS.txt`.
  - Nuevos: `docs/store/PRIVACY-POLICY-FINAL.md`, `docs/store/privacy-policy/index.html`, `docs/store/STORE-LISTING-FINAL.md`, `release/store/StoreListing-300x300.png`, `docs/reports/INFORME-8B1F-FINAL-SUBMISSION-GATE.md`.
- **Integridad del Código Fuente:** Cero alteraciones en archivos `.cs`, `.csproj` o `.sln`. La suite de 632 pruebas unitarias e integración se mantiene al 100% verde bajo el commit de compilación `ecd15afd5200696cde838d778125ae4fe4dbf73b` (CI Run `42705190058`).

---

## 3. Revalidación del Paquete Binario Candidato

El paquete binario generado en la Fase 8B.1C y verificado en la Fase 8B.1D/8B.1E ha sido revalidado criptográfica y estructuralmente:

| Parámetro | Valor Verificado en Disco | Estado |
|---|---|---|
| **Ruta del Paquete** | `release/store/PrivGuard-2.0.0.0-x64.msix` | Confirmado en disco |
| **Tamaño Exacto** | `58,573,769 bytes` | Coincidencia exacta con `BUILD-PROVENANCE.json` |
| **Hash SHA-256** | `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7` | Coincidencia exacta con `BUILD-PROVENANCE.json` |
| **Identity Name** | `cdevstudios.PrivGuard` | Extraído de `AppxManifest.xml` |
| **Publisher Subject** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` | Extraído de `AppxManifest.xml` |
| **Publisher Display Name** | `cdev studios` | Extraído de `AppxManifest.xml` |
| **Package Version** | `2.0.0.0` | Extraído de `AppxManifest.xml` |
| **Arquitectura** | `x64` | Extraído de `AppxManifest.xml` |
| **Capacidades Declaradas** | `rescap:runFullTrust` | Única restricted capability |
| **Capacidades Prohibidas** | `allowElevation`: Ausente, `unvirtualizedResources`: Ausente | 0 violaciones |
| **Script de Verificación** | `scripts/verify-store-msix.ps1 -PackageDir release\store` | **PASS (Exit code 0)** |

---

## 4. Dictamen Técnico sobre Formatos de Carga (`.msix` vs `.msixupload`)

### Normativa Oficial de Microsoft Partner Center:
1. Microsoft documenta que Partner Center admite archivos `.msix`, `.msixupload`, `.msixbundle`, `.appx`, `.appxupload` y `.appxbundle`.
2. Para Windows 10 y versiones posteriores, Microsoft **recomienda** `.msixupload` cuando el entorno de compilación (ej. Visual Studio con Desktop Bridge Packaging Project) empaqueta automáticamente el paquete binario junto con un archivo de símbolos públicos (`.appxsym`).
3. El archivo `.msix` individual es un **formato estándar oficialmente soportado y fallback directo** en Partner Center para paquetes Win32 empaquetados.

### Evaluación del Pipeline de PrivGuard:
- El pipeline de empaquetado del repositorio (`scripts/build-msix.ps1`) utiliza directamente la herramienta del SDK de Windows `MakeAppx.exe` para empaquetar la carpeta de publicación (`staging-x64`).
- Aunque existen símbolos PDB locales generados por `dotnet publish` en `staging-x64`, no existe un proceso oficial del SDK que construya un contenedor de símbolos `.appxsym` sin recurrir a compresión ZIP manual.
- Conforme a la regla expresa del prompt: *"Si el flujo correcto no puede generarse de forma segura, no fabricar ni renombrar un ZIP arbitrario. Mantener `.msix` como formato admitido y registrar `MSIXUPLOAD NOT GENERATED — DIRECT MSIX SUPPORTED FALLBACK` con la razón concreta"*.
- **Dictamen:** **`MSIXUPLOAD NOT GENERATED — DIRECT MSIX SUPPORTED FALLBACK`**. El archivo que el propietario debe cargar en Partner Center es `release/store/PrivGuard-2.0.0.0-x64.msix`.

---

## 5. Auditoría Exhaustiva de Privacidad y Red

### 5.1 Inexistencia de Transmisión por Red
- **P/Invoke / DLLs Nativas:** Cero importaciones de bibliotecas nativas de red (`wininet.dll`, `winhttp.dll`, `ws2_32.dll`, `urlmon.dll` ausentes en toda la solución).
- **APIs Gestionadas:** Cero instanciación o consumo de `HttpClient`, `WebRequest`, `TcpClient`, `Socket`, `WebSocket` o `Dns` en el código de aplicación o plataforma.
- **SDKs de Telemetría o Analítica:** Cero dependencias de Microsoft Application Insights, Sentry, Google Analytics, Firebase, AppCenter o equivalentes.
- **Manifiesto:** Ausencia total de capabilities de red (`internetClient`, `internetClientServer` ausentes).

### 5.2 Inventario Completo de Datos Operativos Locales
El almacenamiento de datos en PrivGuard Store Edition se ubica exclusivamente en `%LOCALAPPDATA%\PrivGuard\Store\`:

| Artefacto / Archivo | Ruta Relativa | Campos Exactos Almacenados | Finalidad Técnica | Retención | ¿Sale del Dispositivo? |
|---|---|---|---|---|---|
| **Estado y Preferencias** (`state.json`) | `%LOCALAPPDATA%\PrivGuard\Store\state.json` | `cameraStandard`, `cameraSecure`, `microphoneStandard`, `microphoneSecure`, `language` ("es"/"en"), `autostart` (bool), `advancedProtectionEnabled` (bool). | Recordar las preferencias de configuración y el estado de privacidad deseado por el usuario entre sesiones. | Indefinida hasta modificación por el usuario o borrado manual. | **No.** Estrictamente local. |
| **Diario de Sesión (WAL)** (`privacy-session-v1.json`) | `%LOCALAPPDATA%\PrivGuard\Store\Recovery\privacy-session-v1.json` | `schemaVersion`, `sessionId`, `createdAtUtc`, `updatedAtUtc`, `completedAtUtc`, `ownerProcessId`, `isActive`, `wasRestored`, `status`, `lastOperationId`, `resources` (`resourceId`, `operationId`, `endpointId`, `originalMutedState`, `protectedMutedState`, `journalState`, `restoreAttempts`, `lastError`). | Registro de recuperación por escritura anticipada (WAL) para garantizar la restauración y reversión del silenciado WASAPI al salir o recuperarse de un cierre inesperado. | Se conserva mientras existan recursos en protección; se actualiza en tiempo de ejecución. | **No.** Estrictamente local. |
| **Descriptor de Coordinación** (`cross-edition-latch.json`) | `%LOCALAPPDATA%\PrivGuard\Store\cross-edition-latch.json` | `schemaVersion`, `ownerEdition` (Store=1), `userSid`, `processId`, `processStartTimeUtc`, `sessionId`, `acquiredAtUtc`, `lastHeartbeatUtc`, `operationStatus`. | Coordinación de exclusión mutua por usuario para evitar conflictos de sesión entre la edición Store y Standalone en un mismo PC. | Se crea al inicio de la sesión activa y se elimina al salir limpiamente. Reconciliado en arranque. | **No.** Estrictamente local. |
| **Registros Locales (Logs)** (`PrivGvard-*.log`) | `%LOCALAPPDATA%\PrivGuard\Store\Logs\` | Timestamp, nivel de severidad, PID, ThreadId, SourceContext, mensaje y excepciones. Cabecera inicial con versión de SO, arquitectura, versión de .NET y ruta de logs. | Diagnóstico técnico local de detección de dispositivos y estados de silenciado. | Archivos rotativos diarios con retención máxima de **7 días** (`retainedFileCountLimit: 7`) y tope de 10 MB por archivo. | **No.** Estrictamente local. |
| **Reportes de Fallo** (`crash-*.json`) | `%LOCALAPPDATA%\PrivGuard\Store\CrashReports\` | Timestamp UTC, sourceContext, exceptionType, message, stackTrace, innerException, environmentInfo (OS, arquitectura, runtime, PID, proceso, flags 64-bit), appState. | Volcado estructurado ante excepciones no controladas para depuración local. | Se conservan en disco local hasta eliminación manual por el usuario. | **No.** Estrictamente local. |

### 5.3 Advertencia de Desinstalación y Retención
Debido a que PrivGuard Store Edition utiliza la carpeta de datos personalizada `%LOCALAPPDATA%\PrivGuard\Store\` y no el contenedor gestionado por el sistema (`%LOCALAPPDATA%\Packages\<PFN>\LocalState`), el desinstalador de MSIX de Windows **puede no eliminar automáticamente** esta carpeta al desinstalar la aplicación. Se han incorporado instrucciones explícitas en la política de privacidad y en la documentación para que el usuario conozca los pasos de desinstalación limpia y borrado manual.

---

## 6. Reconciliación de Activos Gráficos y Ficha Comercial

### 6.1 Unificación de Capturas de Pantalla (4 Imágenes Verificadas)
Se corrigió la omisión histórica en la checklist de Partner Center, unificando las 4 capturas reales auditadas en todos los documentos:
1. `release/store/main-window.png` (593 x 625 px, SHA-256: `ebcc070cc59e4a7b0b04367bef722858db04969317f50af556264dd16c34a37c`) — Ventana principal en tema Fluent Dark.
2. `release/store/mainwindow_dark.png` (563 x 788 px, SHA-256: `324c190a5904927afbc323eff6f738e525293c7ed7b6de2f875b6cc0947bc7ae`) — Vista completa con indicadores de estado.
3. `release/store/protection_overview.png` (563 x 356 px, SHA-256: `edc20a997c061b57364f3f36f8e7a4f5f3cd9b3081e88ded1c5f31f4fb18e9d3`) — Detalle de estado de protección y accesos directos.
4. `release/store/tray_menu.png` (465 x 190 px, SHA-256: `5b56e1fbbcbf83024dbf8a1098a2a7b3e11a1849dbe227460324f7c74ea62076`) — Menú contextual de la bandeja del sistema.

### 6.2 Icono de Ficha de Aplicación (1:1 300x300 px)
Microsoft recomienda un icono cuadrado de 300x300 px para los resultados de búsqueda y colecciones de la Tienda. Se localizó el logotipo maestro original de alta resolución (`assets/logo/privgvard_logo.png`, 1024x1024 px, 32bpp ARGB) y se generó mediante remuestreo bicúbico de alta calidad:
- **Archivo:** `release/store/StoreListing-300x300.png`
- **Dimensiones:** 300 x 300 píxeles | **Formato:** PNG 32bpp ARGB | **Tamaño:** 136,229 bytes
- **Hash SHA-256:** `75b7a44ab32dbc6f0923bff4c2e31605c3334d2e75e1046ab3acab361927fd92`
- **Ubicación en el Expediente:** Incorporado en `ARTIFACT-MANIFEST.json` y `SHA256SUMS.txt`.

### 6.3 Declaración de Licencia de Software
- **Licencia Oficial:** GNU General Public License v3.0 (GPL-3.0), verificada con el archivo `LICENSE` del repositorio.
- **Enlace Canónico:** `https://github.com/cristianbarreiro/PrivGuard/blob/main/LICENSE`
- **Modelo Comercial:** Gratuito ($0.00 USD). El carácter Open Source de la licencia se distingue con claridad del precio en la Tienda.

---

## 7. Entregables Generados y Actualizados en la Fase 8B.1F

| Archivo Generado / Actualizado | Finalidad / Contenido |
|---|---|
| `docs/store/PRIVACY-POLICY-FINAL.md` | Política de privacidad definitiva bilingüe (EN/ES) basada en el inventario real de datos, con límites de silenciado y retención de desinstalación. |
| `docs/store/privacy-policy/index.html` | Página web HTML estática, responsive, bilingüe y accesible (0 recursos externos, 0 scripts, 0 fuentes remotas), lista para alojar. |
| `docs/store/PRIVACY-POLICY-DRAFT.md` | Actualizado con nota de redirección al documento final auditado. |
| `docs/store/STORE-LISTING-FINAL.md` | Ficha comercial definitiva lista para copiar y pegar en `en-us` y `es-es`, palabras clave, capturas y notas para certificación. |
| `docs/store/OWNER-DECISIONS.md` | Matriz limpia y consolidada de decisiones exclusivas del propietario, sin valores asumidos como aprobados. |
| `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md` | Checklist técnica reconciliada (distinción `.msix`/`.msixupload`, 4 capturas, icono 300x300, 0 enlaces absolutos). |
| `release/store/submission-handoff/PARTNER-CENTER-STEP-BY-STEP.md` | Runbook paso a paso alineado con la documentación oficial actual de Microsoft Partner Center y sin predicciones IARC. |
| `release/store/submission-handoff/README-SUBMISSION-HANDOFF.md` | Documento resumen del expediente de entrega con justificación del formato `.msix`. |
| `release/store/submission-handoff/ARTIFACT-MANIFEST.json` | Manifiesto estructurado JSON actualizado con los 4 screenshots, icono 300x300 y sumas recalculadas. |
| `release/store/submission-handoff/SHA256SUMS.txt` | Sumas de comprobación SHA-256 regeneradas y verificadas para todos los artefactos de entrega. |
| `release/store/StoreListing-300x300.png` | Icono oficial de alta resolución para la ficha de la Tienda generado desde el logotipo maestro de 1024x1024 px. |
| `docs/reports/INFORME-8B1F-FINAL-SUBMISSION-GATE.md` | Este informe de auditoría y puerta de congelación. |

---

## 8. Verificaciones de Integridad y Validación Técnica

1. **Validación del Manifiesto JSON:** Ejecutado `ConvertFrom-Json` sobre `ARTIFACT-MANIFEST.json`: estructura válida y libre de errores sintácticos.
2. **Validación de Existencia de Rutas:** Comprobado que todas las rutas relativas especificadas en `ARTIFACT-MANIFEST.json` existen físicamente en disco.
3. **Validación de Sumas Criptográficas:** Verificado que los 12 archivos enumerados en `SHA256SUMS.txt` coinciden al 100% con los hashes calculados en disco.
4. **Ausencia de Enlaces Locales Absolutos:** Búsqueda recursiva de `file:///` en `docs/store/` y `release/store/`: **0 coincidencias**.
5. **Limpieza de Marcadores Temporales:** Búsqueda sensible a mayúsculas de `[OWNER INPUT REQUIRED]`, `TODO` y `TBD` en documentos públicos: **0 coincidencias**.
6. **Verificación Estructural MSIX:** Ejecutado `scripts/verify-store-msix.ps1`: **PASS (Exit code 0)**.

---

## 9. Checklist Resumida para el Propietario en Partner Center (10 Acciones)

Para completar la presentación en Microsoft Partner Center (`https://partner.microsoft.com/dashboard`), el propietario debe seguir estas 10 acciones:

1. **Verificar Identidad:** Confirmar que la app reservada muestre el **Product ID `9NQ56N45XH5N`**, nombre `PrivGuard`, editor `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` (`cdev studios`).
2. **Pricing & Availability:** Seleccionar precio base (**Free** recomendado), mercados de distribución deseados y visibilidad (**Público**).
3. **Configurar Retención Manual:** En fecha de publicación, marcar obligatoriamente **Manual Hold** (`Don't publish this submission until I select Publish now`) para revisar el informe técnico de Microsoft antes de la salida a tienda.
4. **Propiedades:** Asignar categoría principal (`Security > Personal security` o la utilidad más cercana disponible) y secundaria (`Utilities & tools`).
5. **Alojar Política de Privacidad:** Publicar el contenido de `docs/store/privacy-policy/index.html` en un servidor o dominio HTTPS público e ingresar la URL en el portal.
6. **Cuestionario IARC (Edad):** Completar cada pregunta del cuestionario oficial en el portal basándose en el funcionamiento real de la app (0 violencia, 0 compras, 0 transmisión de red, etc.).
7. **Cargar Paquete Binario:** En la sección **Packages**, subir `release/store/PrivGuard-2.0.0.0-x64.msix` (esperar estado *Validated* y excluir dispositivos no soportados como Xbox).
8. **Justificar `runFullTrust`:** Copiar y pegar el bloque de justificación técnica provisto en la Sección 6 de `docs/store/STORE-LISTING-FINAL.md`.
9. **Completar Fichas Comerciales (EN/ES):** Copiar título, subtítulo, descripción, características y palabras clave desde `docs/store/STORE-LISTING-FINAL.md`. Subir las 4 capturas (`main-window.png`, `mainwindow_dark.png`, `protection_overview.png`, `tray_menu.png`) y el icono 300x300 (`StoreListing-300x300.png`).
10. **Notas de Certificación y Envío:** Pegar las instrucciones para los revisores de Microsoft desde la Sección 7 de `docs/store/STORE-LISTING-FINAL.md`, revisar marcas verdes y pulsar **Submit to the Store**.

---

## 10. Dictamen Final y Conclusión

Con base en la auditoría rigurosa y las evidencias verificadas en disco:

### Dictamen Canónico:
```text
READY FOR PARTNER CENTER DRAFT — REPOSITORY HANDOFF FROZEN
```

- **Condición de Parada Activa:** No se requieren ni se permiten fases adicionales en el repositorio.
- **Próximo Paso Canónico:** Acción externa por parte del propietario del producto en el portal de Microsoft Partner Center utilizando los entregables congelados de la Fase 8B.1F.

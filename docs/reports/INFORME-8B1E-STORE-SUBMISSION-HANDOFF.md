# INFORME TÉCNICO OFICIAL — FASE 8B.1E
## Store Submission Handoff & Partner Center Draft Readiness

**Fecha de Emisión:** 9 de noviembre de 2026  
**Proyecto:** PrivGuard (Solución: `PrivGvard.sln`, Binario: `PrivGvard.exe` / MSIX: `cdevstudios.PrivGuard`)  
**Fase de Ingeniería:** Fase 8B.1E — Store Submission Handoff & Partner Center Draft Readiness  
**Árbol Git Canónico:** Rama `main` en `cristianbarreiro/PrivGuard`  
**Commit de HEAD Verificado:** `926f018ddf5021431bec96897d81ee5c79712acd` (sincronizado con `origin/main`, **0 pushes ejecutados por Antigravity**, **0 Pull Requests**)  
**Estado del Árbol de Trabajo:** Limpio previo a la emisión de los entregables de handoff de esta fase  
**Dictamen Canónico Final:**
# `READY FOR PARTNER CENTER DRAFT — OWNER INPUTS PENDING`

---

## 1. Identificación Criptográfica y Procedencia del Paquete Candidato

El paquete candidato analizado y preparado para entrega ha sido revalidado en disco:

| Parámetro del Artefacto | Valor Observado y Verificado | Trazabilidad con Fases Anteriores |
|---|---|---|
| **Ruta del Artefacto** | `release/store/PrivGuard-2.0.0.0-x64.msix` | Determinista y bit a bit idéntica |
| **Tamaño Exacto en Bytes** | `58.573.769 bytes` | Coincidencia exacta con Fases 8B.1C y 8B.1D |
| **Hash Criptográfico SHA-256** | `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7` | Coincidencia exacta con Fases 8B.1C y 8B.1D |
| **Package Identity Name** | `cdevstudios.PrivGuard` | Verificado desde `AppxManifest.xml` interno |
| **Publisher** | `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD` | Sujeto oficial del Partner Center |
| **Publisher Display Name** | `cdev studios` | Verificado desde `AppxManifest.xml` interno |
| **Package Family Name (PFN)** | `cdevstudios.PrivGuard_tv7b4yqg10sw2` | Calculado por Windows AppModel |
| **Versión del Paquete** | `2.0.0.0` | 2.0.0.0 |
| **Arquitectura de Procesador** | `x64` | Windows x64 (AMD64) |
| **Target OS / Min Version** | `10.0.19041.0` (MinVersion) / `10.0.26100.0` (Target) | Windows 10 2004+ y Windows 11 |
| **Restricted Capabilities (D-01)**| `rescap:runFullTrust` (Única capability) | Cumplimiento estricto D-01 |
| **Forbidden Capabilities (D-01)** | 0 (`allowElevation`: ausente, `unvirtualizedResources`: ausente) | Cumplimiento estricto D-01 |
| **Resultado `verify-store-msix.ps1`**| **PASS (Exit Code 0)** | Revalidado en Fase 8B.1E |
| **Proveniencia Oficial** | `release/store/BUILD-PROVENANCE.json` | Hash y metadatos íntegros |

---

## 2. Formato de Carga: Direct `.msix` vs `.msixupload`

De acuerdo con la documentación oficial de Microsoft Learn (*"Upload app packages"*, sección MSIX submissions):
1. **Formatos Soportados:** Microsoft Partner Center admite directamente archivos `.msix`, `.msixupload`, `.msixbundle`, `.appx`, `.appxupload` y `.appxbundle`.
2. **Naturaleza de `.msixupload`:** Un archivo `.msixupload` es un contenedor ZIP que agrupa un paquete `.msix` junto con un archivo de símbolos `.appxsym` (PDBs) para el indexado de símbolos en el servidor de Microsoft.
3. **Criterio Técnico Adoptado:** 
   - Para aplicaciones de escritorio Win32 / Avalonia empaquetadas mediante herramientas CLI estándar (`dotnet publish` + `makeappx.exe`), la **carga directa del archivo `.msix` individual** está oficialmente soportada, es plenamente compatible y recomendada para este flujo.
   - En estricto cumplimiento de las restricciones de la Fase 8B.1E, **no se ha fabricado ningún `.msixupload` ficticio renombrando archivos ni empaquetando símbolos vacíos o desalineados**.
   - El artefacto que el propietario debe cargar en la sección **Packages** es:  
     `release/store/PrivGuard-2.0.0.0-x64.msix`

---

## 3. Expediente de Handoff Generado

Se ha consolidado el directorio formal de entrega en `release/store/submission-handoff/`:

- **[`README-SUBMISSION-HANDOFF.md`](../../release/store/submission-handoff/README-SUBMISSION-HANDOFF.md):** Guía ejecutiva del paquete a subir, hash esperado y secuencia de envío.
- **[`ARTIFACT-MANIFEST.json`](../../release/store/submission-handoff/ARTIFACT-MANIFEST.json):** Manifiesto JSON estructurado con metadatos del paquete, arquitecturas, capacidades, hashes y validaciones.
- **[`SHA256SUMS.txt`](../../release/store/submission-handoff/SHA256SUMS.txt):** Sumas criptográficas SHA-256 del binario MSIX y de todos los activos gráficos de la ficha.
- **[`PARTNER-CENTER-STEP-BY-STEP.md`](../../release/store/submission-handoff/PARTNER-CENTER-STEP-BY-STEP.md):** Procedimiento operativo paso a paso en el portal de Partner Center desde la carga hasta el envío.

---

## 4. Auditoría de Activos Visuales y Ficha Comercial (Listing Review)

Se completó la auditoría detallada en [`docs/store/STORE-LISTING-REVIEW.md`](../store/STORE-LISTING-REVIEW.md):

1. **Activos Gráficos:**
   - 4 capturas reales de pantalla en Fluent Dark (`main-window.png`, `mainwindow_dark.png`, `protection_overview.png`, `tray_menu.png`).
   - Todos los logotipos requeridos (`StoreLogo.png`, `Square150x150Logo.png`, `Square44x44Logo.png`, `Wide310x150Logo.png`, `SplashScreen.png`) verificados en dimensiones y formato PNG.
   - Matriz monolítica de assets PRI generada e integrada en el paquete.
2. **Honestidad de Declaraciones Comerciales:**
   - Erradicación de afirmaciones absolutas o de hardware: el micrófono se describe como silenciado de software vía Windows Core Audio WASAPI (`IAudioEndpointVolume`).
   - Advertencia explícita sobre flujos en modo exclusivo y soporte de controladores.
   - La cámara se describe con precisión como redirección a la configuración nativa de privacidad de Windows (`ms-settings:privacy-webcam`).
   - Se ratifica el funcionamiento como usuario estándar (`asInvoker`) con cero elevación y cero telemetría remota.

---

## 5. Auditoría de Privacidad y Red (Zero-Telemetry Verification)

Se inspeccionó exhaustivamente el código fuente y las referencias de proyectos en `src/`:
- **Bibliotecas de Red / Telemetría:** 0 coincidencias de `HttpClient`, `WebRequest`, `TcpClient`, `Socket`, Sentry o Application Insights.
- **Capacidades de Manifiesto:** No se declara `internetClient` ni `privateNetworkClientServer`.
- **Persistencia Local:** Todos los datos (configuración, diarios WAL, logs rotativos y crash reports) se escriben exclusivamente en espacio de usuario local bajo `%LOCALAPPDATA%\PrivGuard\Store\`.
- **Borrador de Política de Privacidad:** Generado en versión bilingüe en [`docs/store/PRIVACY-POLICY-DRAFT.md`](../store/PRIVACY-POLICY-DRAFT.md), listo para ser publicado por el propietario en su canal público.

---

## 6. Estado de la Validación de Hardware Físico (RT-05)

- **Estado Actual:** `PENDING OWNER — REAL WINDOWS HARDWARE TEST`.
- **Causa:** El entorno actual de desarrollo y CI es una máquina virtual de evaluación (VirtualBox) sin micrófono físico conectado (`Status: Error`, `Problem: CM_PROB_DISABLED`).
- **Instrumentación:** Se ha diseñado un protocolo de prueba de una sola página en [`docs/store/REAL-HARDWARE-RT05-TEST.md`](../store/REAL-HARDWARE-RT05-TEST.md).
- **Procedimiento para el Propietario:** Probar en una máquina Windows 10/11 física con micrófono real el ciclo silenciar -> verificar con Grabadora de voz -> desilenciar -> verificar recuperación de sonido (duración aproximada: 2 minutos).

---

## 7. Matriz de Decisiones y Acciones Pendientes del Propietario

En [`docs/store/OWNER-DECISIONS.md`](../store/OWNER-DECISIONS.md) se consolidan los puntos que dependen exclusivamente del propietario:

1. **Store Product ID:** Confirmar en el dashboard que la app reservada coincida con `9NQ56N45XH5N`.
2. **Precio y Mercados:** Confirmar precio (Recomendado: Gratuito / Free) y selección de países.
3. **Opción de Publicación:** Seleccionar **Manual Publish (Hold)** para retener el lanzamiento tras la certificación oficial y revisar el dictamen de Microsoft antes de publicar.
4. **Cuestionario IARC:** Responder el cuestionario de clasificación de edad en el portal (Utilidad: 0 violencia, 0 compras integradas).
5. **URL Pública de Política de Privacidad:** Publicar el contenido de `docs/store/PRIVACY-POLICY-DRAFT.md` en una URL accesible e ingresarla en Partner Center.
6. **Canal de Soporte:** Ingresar URL de GitHub Issues o correo oficial.
7. **Notas para Certificación:** Pegar el texto en inglés preparado en `docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md`.

---

## 8. Dictamen Canónico Final

Conforme a las reglas de evaluación de la Sección 14 del Prompt Maestro:

# **`READY FOR PARTNER CENTER DRAFT — OWNER INPUTS PENDING`**

**Fundamentación Técnica:**
- El paquete candidato `release/store/PrivGuard-2.0.0.0-x64.msix` está unívocamente identificado por hash SHA-256 (`707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`) y revalidado con `verify-store-msix.ps1` (PASS).
- El formato de carga directa `.msix` está respaldado oficialmente por Microsoft para Desktop Bridge.
- Los activos gráficos, capturas reales y textos de ficha comercial están 100% auditados y conformes con las políticas de honestidad técnica.
- El expediente de entrega en `release/store/submission-handoff/` está completo y cerrado.
- El siguiente paso natural no requiere desarrollo ni auditorías adicionales, sino la creación manual del borrador en Microsoft Partner Center por parte del propietario de la cuenta.

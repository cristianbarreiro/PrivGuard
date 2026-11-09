# PrivGuard 2.0 — Protocolo de Validación en Hardware Físico: RT-05 (Fase 8B.1E)

**Estado de la Prueba:** `PENDING OWNER — REAL WINDOWS HARDWARE TEST`  
**Objetivo:** Validar el silenciado y la restauración efectiva del micrófono mediante la API Core Audio WASAPI sobre un equipo físico real con Windows 10 o Windows 11.

> **Regla de Oro de Seguridad:**  
> Jamás desinstales PrivGuard mientras el micrófono permanezca silenciado. Siempre pulsa el botón para desilenciar y verifica que el sonido se haya restablecido antes de cerrar o desinstalar la aplicación.

---

## 1. Precondiciones en el Equipo Físico

1. **Equipo Físico con Windows 10 (2004+) o Windows 11 (x64)**.
2. **Micrófono Físico Conectado y Activo:**
   - Abrir `Configuración > Sistema > Sonido`.
   - En la sección **Entrada**, verificar que el micrófono figure como predeterminado y que la barra de volumen responda al hablar.
3. **Prueba de Captura Previa:**
   - Abrir la aplicación nativa **Grabadora de voz** (o Grabadora de sonidos de Windows).
   - Iniciar una grabación de 3 segundos, detenerla y reproducirla para confirmar que el hardware graba con normalidad.

---

## 2. Procedimiento de Ejecución Paso a Paso

| Paso | Acción a Realizar | Resultado Esperado | Resultado Observado |
|---|---|---|---|
| **1** | Abrir PrivGuard (Edición Store o ejecutable en entorno físico). | La app inicia sin avisos de elevación administrativa (UAC). | `[ ]` |
| **2** | Observar la fila **Micrófono** en la ventana principal. | Muestra el nombre descriptivo del endpoint y clasifica la capacidad (`Supported`, `NotSupported` o `Unknown`). | `[ ]` |
| **3** | Activar el silenciado pulsando el botón/toggle de micrófono. | El botón cambia de estado; aparece el banner naranja advirtiendo desilenciar antes de desinstalar. | `[ ]` |
| **4** | Abrir la **Grabadora de voz** e intentar grabar hablando al micrófono. | La barra de volumen de Windows no detecta sonido; la grabación resulta completamente en silencio. | `[ ]` |
| **5** | En PrivGuard, pulsar nuevamente el botón para **desilenciar** el micrófono. | El botón vuelve al estado normal; el banner naranja de advertencia desaparece. | `[ ]` |
| **6** | Volver a probar la grabación en la **Grabadora de voz**. | El micrófono vuelve a capturar audio con normalidad y volumen audible. | `[ ]` |
| **7** | Cerrar la ventana con la 'X' y comprobar el icono en la bandeja (System Tray). | La ventana se oculta a la bandeja del sistema. | `[ ]` |
| **8** | Clic derecho en el icono de la bandeja y seleccionar **Salir**. | El proceso finaliza completamente sin dejar endpoints silenciados ni bloqueados. | `[ ]` |

---

## 3. Registro de Evidencia de Hardware (A Completar por el Propietario)

- **Fabricante y Modelo del PC:** `________________________________________`
- **Fabricante y Modelo del Micrófono:** `____________________________________` (ej. Realtek High Definition Audio, USB Mic, etc.)
- **Versión de Windows y Build:** `__________________________________________` (ej. Windows 11 Pro 23H2 / Build 22631.xxxx)
- **Versión del Controlador de Audio:** `_____________________________________`
- **Capacidad Detectada en la UI:** `Supported` / `NotSupported` / `Unknown`
- **¿El silenciado funcionó correctamente en la grabadora?** `SÍ` / `NO`
- **¿La restauración devolvió el audio al 100%?** `SÍ` / `NO`
- **Observaciones o Advertencias:** `________________________________________`
- **Firma / Aprobación del Operador:** `____________________ Fecha: _________`

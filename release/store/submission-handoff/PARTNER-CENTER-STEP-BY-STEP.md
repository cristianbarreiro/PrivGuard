# PrivGuard 2.0 — Partner Center Step-by-Step Submission Runbook (Fase 8B.1F)

Este documento describe la secuencia exacta paso a paso que el **propietario de la cuenta** debe seguir en el portal de **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard/apps-and-games/overview`) para cargar el paquete y enviar la presentación a certificación.

---

## Pre-requisitos Inmediatos
1. Archivo binario verificado: `release/store/PrivGuard-2.0.0.0-x64.msix` (SHA-256: `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`).
2. Activos de capturas en `release/store/`:
   - `main-window.png` (593 x 625)
   - `mainwindow_dark.png` (563 x 788)
   - `protection_overview.png` (563 x 356)
   - `tray_menu.png` (465 x 190)
3. Icono de ficha de aplicación (1:1): `release/store/StoreListing-300x300.png` (300 x 300).
4. URL pública HTTPS con el contenido de `docs/store/privacy-policy/index.html` (o `docs/store/PRIVACY-POLICY-FINAL.md`).
5. Textos comerciales preparados en `docs/store/STORE-LISTING-FINAL.md` (y `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`).
6. Notas para certificadores y justificación `runFullTrust` en `docs/store/STORE-LISTING-FINAL.md`.

---

## Flujo Paso a Paso en Partner Center

### Paso 1: Acceso y Selección de la Aplicación
1. Iniciar sesión con la cuenta de desarrollador en [Microsoft Partner Center](https://partner.microsoft.com/dashboard/apps-and-games/overview).
2. En la lista de aplicaciones, localizar la aplicación reservada **PrivGuard**.
3. Confirmar que el **Store Product ID** coincida con `9NQ56N45XH5N` y que el nombre reservado sea `PrivGuard`.
4. Hacer clic en **Start your submission** (o continuar el borrador si ya existe).

---

### Paso 2: Pricing and availability (Precios y Disponibilidad)
1. **Markets:** Seleccionar **All markets** (o personalizar según los países deseados por el propietario).
2. **Pricing:** Seleccionar **Free** (o el nivel de precio decidido por el propietario en `OWNER-DECISIONS.md`).
3. **Discoverability:** Seleccionar **Make this product available in the Store** (o *Hidden* si se desea prueba cerrada).
4. **Publish date (MUY RECOMENDADO):** Seleccionar **Don't publish this submission until I select Publish now (Manual Hold)**.
   > *Motivo Crítico:* Microsoft no permite cancelar el envío una vez iniciado el release público. Mantenerlo en *Hold* permite al propietario revisar el informe oficial de certificación emitido por Microsoft antes de que la app sea visible al público general.
5. Hacer clic en **Save**.

---

### Paso 3: Properties (Propiedades)
1. **Category:** 
   - Primaria: `Security` > `Personal security` (o la categoría de utilidad más cercana en el portal).
   - Secundaria: `Utilities & tools`.
2. **Support contact info:** Introducir la URL de soporte (`https://github.com/cristianbarreiro/PrivGuard/issues`) o correo del desarrollador.
3. **Privacy policy URL:** Introducir la URL pública HTTPS donde se alojó la política de privacidad (`docs/store/privacy-policy/index.html`).
4. **Website:** `https://github.com/cristianbarreiro/PrivGuard`.
5. Hacer clic en **Save**.

---

### Paso 4: Age ratings (Clasificación por Edad — IARC)
1. Abrir el cuestionario de clasificación de la International Age Rating Coalition (IARC):
   - Proporcionar correo electrónico de contacto del desarrollador.
   - Seleccionar categoría de la app (ej. *Utility, Productivity, Communication, or Other*).
   - Responder honestamente a cada una de las preguntas formuladas por el portal basándose en el funcionamiento real de la app:
     - Violencia, sangre, lenguaje soez, referencias a sustancias o contenido sexual: responder **No**.
     - Compras integradas o monedas virtuales: responder **No**.
     - Recopilación o compartición de ubicación física con terceros: responder **No**.
     - Intercambio de mensajes o interacción social entre usuarios: responder **No**.
     - Funciones de red: PrivGuard no transmite datos por la red; responde en consecuencia a los términos de la pregunta.
2. Generar y guardar la clasificación oficial calculada por el cuestionario de IARC.
3. Hacer clic en **Save**.

---

### Paso 5: Packages (Carga del Paquete Binario)
1. Arrastrar y soltar el archivo `release/store/PrivGuard-2.0.0.0-x64.msix` en la zona de carga de paquetes.
   > *Nota de Formato de Carga:* Microsoft Partner Center recomienda `.msixupload` cuando existe un contenedor de símbolos compilado automáticamente, pero admite la subida directa de archivos `.msix` como fallback estándar y plenamente válido (`MSIXUPLOAD NOT GENERATED — DIRECT MSIX SUPPORTED FALLBACK`).
2. Esperar a que el portal procese el archivo y extraiga las propiedades:
   - Identidad detectada: `cdevstudios.PrivGuard`
   - Versión: `2.0.0.0`
   - Arquitectura: `x64`
   - Familia de SO: `Windows 10 Desktop (10.0.19041.0+)`
   - Estado: `Validated`
3. Excluir familias de dispositivos no compatibles (ej. Xbox, HoloLens) si el portal las presenta marcadas.
4. **Declaración de Capacidad Restringida (`runFullTrust`):**
   - Cuando el portal detecte la capacidad restringida `runFullTrust`, aparecerá una casilla de justificación obligatoria.
   - Copiar y pegar el texto de justificación técnica disponible en la Sección 6 de `docs/store/STORE-LISTING-FINAL.md`.
5. Hacer clic en **Save**.

---

### Paso 6: Store listings (Ficha Comercial en la Tienda)
1. **English (United States) — Idioma Principal (`en-us`):**
   - **Product title:** `PrivGuard`
   - **Short description / Subtitle:** `Manage and verify native Windows camera and microphone privacy controls.`
   - **Description:** Pegar la descripción completa en inglés de `docs/store/STORE-LISTING-FINAL.md`.
   - **Key Features:** Pegar los puntos clave en inglés.
   - **Keywords:** Pegar las 7 palabras clave en inglés.
   - **Screenshots:** Subir las 4 capturas:
     - `release/store/main-window.png`
     - `release/store/mainwindow_dark.png`
     - `release/store/protection_overview.png`
     - `release/store/tray_menu.png`
   - **App tile icon (1:1 300x300):** Subir `release/store/StoreListing-300x300.png`.
   - **License Terms:** Declarar `GNU General Public License v3.0 (GPL-3.0)` y enlace `https://github.com/cristianbarreiro/PrivGuard/blob/main/LICENSE`.
2. **Spanish (Spain) — Idioma Adicional (`es-es`):**
   - Añadir el idioma `Spanish (Spain)`.
   - Pegar título, subtítulo, descripción, características y palabras clave en español desde `docs/store/STORE-LISTING-FINAL.md`.
   - Subir las capturas correspondientes.
3. Hacer clic en **Save**.

---

### Paso 7: Submission options / Notes for certification (Notas de Certificación)
1. En la sección **Notes for certification**, pegar el contenido completo en inglés de la Sección 7 de `docs/store/STORE-LISTING-FINAL.md`.
   *(Este texto explica al revisor técnico de Microsoft que la app corre como `asInvoker` sin UAC, cómo probar el silenciado WASAPI, que la cámara abre los ajustes nativos de Windows y que el System Tray icon permite el cierre definitivo).*
2. Confirmar que la opción de publicación manual (**Don't publish until I select Publish now**) permanezca seleccionada.
3. Hacer clic en **Save**.

---

### Paso 8: Submit for certification (Envío Final)
1. Revisar que todas las secciones del formulario muestren marca de verificación verde.
2. Hacer clic en **Submit to the Store**.
3. El paquete entrará en la cola de procesamiento y certificación de Microsoft (habitualmente entre 24 y 72 horas).
4. El propietario recibirá un correo electrónico con el reporte oficial de certificación.
5. Tras superar la certificación con éxito, al haberse seleccionado Manual Hold, el paquete quedará listo para que el propietario pulse **Publish now** cuando lo considere oportuno.

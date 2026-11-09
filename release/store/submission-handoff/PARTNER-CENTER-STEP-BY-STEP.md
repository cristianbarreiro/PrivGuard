# PrivGuard 2.0 — Partner Center Step-by-Step Submission Runbook (Fase 8B.1E)

Este documento describe la secuencia exacta paso a paso que el **propietario de la cuenta** debe seguir en el portal de **Microsoft Partner Center** (`https://partner.microsoft.com/dashboard/apps-and-games/overview`) para cargar el paquete y enviar la presentación a certificación.

---

## Pre-requisitos Inmediatos
1. Archivo binario verificado: `release/store/PrivGuard-2.0.0.0-x64.msix` (SHA-256: `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`).
2. Activos de capturas en `release/store/` (`main-window.png`, `mainwindow_dark.png`, `protection_overview.png`, `tray_menu.png`).
3. URL pública HTTPS con el texto de `docs/store/PRIVACY-POLICY-DRAFT.md`.
4. Textos comerciales preparados en `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`.
5. Notas para certificadores en `docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md`.

---

## Flujo Paso a Paso en Partner Center

### Paso 1: Acceso y Selección de la Aplicación
1. Iniciar sesión con la cuenta de desarrollador en [Microsoft Partner Center](https://partner.microsoft.com/dashboard/apps-and-games/overview).
2. En la lista de aplicaciones, localizar la aplicación reservada **PrivGuard**.
3. Confirmar que el **Store Product ID** coincida con `9NQ56N45XH5N` y que el nombre reservado sea `PrivGuard`.
4. Hacer clic en **Start your submission** (o continuar el borrador si ya existe).

---

### Paso 2: Pricing and availability (Precios y Disponibilidad)
1. **Markets:** Seleccionar **All markets** (o personalizar según los países deseados).
2. **Pricing:** Seleccionar **Free** (o el nivel de precio decidido en `OWNER-DECISIONS.md`).
3. **Discoverability:** Seleccionar **Make this product available in the Store** (o *Hidden* si se desea prueba cerrada).
4. **Publish date (MUY RECOMENDADO):** Seleccionar **Publish this submission manually (Hold)**.
   > *Motivo:* Microsoft no permite cancelar el envío una vez iniciado el release público. Mantenerlo en *Hold* permite al propietario revisar el informe oficial de certificación emitido por Microsoft antes de que la app sea visible al público general.
5. Hacer clic en **Save**.

---

### Paso 3: Properties (Propiedades)
1. **Category:** 
   - Primaria: `Security` > `Personal security`.
   - Secundaria: `Utilities & tools`.
2. **Support contact info:** Introducir la URL de soporte (`https://github.com/cristianbarreiro/PrivGuard/issues`) o correo del desarrollador.
3. **Privacy policy URL:** Introducir la URL pública HTTPS donde se alojó la política de privacidad.
4. **Website:** `https://github.com/cristianbarreiro/PrivGuard`.
5. Hacer clic en **Save**.

---

### Paso 4: Age ratings (Clasificación por Edad — IARC)
1. Completar el cuestionario de clasificación de la IARC:
   - Correo electrónico de contacto del desarrollador.
   - Categoría de la app: **Utility, Productivity, Communication, or Other**.
   - Violencia, sangre, lenguaje soez, contenido sexual: Seleccionar **No** en todos.
   - Compras dentro de la app o monedas virtuales: Seleccionar **No**.
   - Uso de red, interacción con otros usuarios o compartición de ubicación física: Seleccionar **No**.
2. Guardar y generar la clasificación (habitualmente PEGI 3, ESRB Everyone).
3. Hacer clic en **Save**.

---

### Paso 5: Packages (Carga del Paquete Binario)
1. Arrastrar y soltar el archivo `release/store/PrivGuard-2.0.0.0-x64.msix` en la zona de carga de paquetes.
2. Esperar a que el portal procese el archivo y extraiga las propiedades:
   - Identidad detectada: `cdevstudios.PrivGuard`
   - Versión: `2.0.0.0`
   - Arquitectura: `x64`
   - Familia de SO: `Windows 10 Desktop (10.0.19041.0+)`
3. **Declaración de Capacidad Restringida (`runFullTrust`):**
   - Cuando el portal detecte la capacidad restringida `runFullTrust`, aparecerá una casilla de justificación obligatoria.
   - Copiar y pegar el texto de justificación técnica disponible en la Sección 3 de `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`.
4. Hacer clic en **Save**.

---

### Paso 6: Store listings (Ficha Comercial en la Tienda)
1. **English (United States) — Idioma Principal:**
   - **Product title:** `PrivGuard`
   - **Short description / Subtitle:** `Manage and verify native Windows camera and microphone privacy controls.`
   - **Description:** Pegar la descripción completa de `docs/store/STORE-LISTING-REVIEW.md`.
   - **Key Features:** Pegar los 7 puntos clave.
   - **Keywords:** Pegar las 7 palabras clave.
   - **Screenshots:** Subir `release/store/main-window.png`, `release/store/mainwindow_dark.png`, `release/store/protection_overview.png`, y `release/store/tray_menu.png`.
   - **App icon / Store logo:** Subir `release/store/StoreLogo.png` y `release/store/Square150x150Logo.png`.
2. **Spanish (Spain) — Idioma Adicional:**
   - Añadir el idioma `Spanish (Spain)`.
   - Pegar título, subtítulo, descripción, características y palabras clave en español.
   - Subir las capturas correspondientes.
3. Hacer clic en **Save**.

---

### Paso 7: Submission options / Notes for certification (Notas de Certificación)
1. En la sección **Notes for certification**, pegar el contenido completo en inglés de `docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md`.
   *(Este texto explica al revisor técnico de Microsoft que la app no requiere UAC, que la cámara abre los ajustes nativos de Windows, cómo probar el silenciado WASAPI y que el tray icon permite cerrar la app desde el menú Salir).*
2. Confirmar que la opción de publicación manual siga seleccionada.
3. Hacer clic en **Save**.

---

### Paso 8: Submit for certification (Envío Final)
1. Revisar que todas las secciones del checklist figuren con marca de verificación verde.
2. Hacer clic en **Submit to the Store**.
3. El paquete entrará en la cola de procesamiento automatizado y revisión de Microsoft (habitualmente entre 24 y 72 horas).
4. El propietario recibirá un correo electrónico con el reporte oficial de certificación.

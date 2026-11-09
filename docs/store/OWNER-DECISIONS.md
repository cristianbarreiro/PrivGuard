# PrivGuard 2.0 — Matriz de Decisiones del Propietario (Fase 8B.1E)

Este documento identifica y aísla de forma explícita las decisiones comerciales, de cuenta y de configuración de Microsoft Partner Center que **corresponden exclusivamente al propietario de la aplicación**. 

> **Advertencia Crítica sobre Publicación en Microsoft Store:**  
> La documentación oficial de Microsoft establece que, una vez iniciado el proceso de publicación automática tras la certificación, **no es posible cancelar la presentación ni alterar la fecha de lanzamiento dentro de esa misma fase**.  
> **Recomendación Técnica Obligatoria:** Configurar la opción de publicación como **Manual Publishing (Hold)** para que el paquete quede retenido tras superar la certificación oficial, permitiendo al propietario revisar el dictamen y autorizar manualmente la salida a la tienda.

---

## 1. Matriz de Decisiones para Microsoft Partner Center

| Campo / Sección en Partner Center | Estado Actual | Dato que Falta / A Decidir | Recomendación Técnica / Contexto | Confirmación del Propietario |
|---|---|---|---|---|
| **Reserva de Nombre y Product ID** | PENDIENTE | Coincidencia de `9NQ56N45XH5N` con la reserva activa | Verificar en el dashboard que la app reservada coincida con `cdevstudios.PrivGuard` y PFN `cdevstudios.PrivGuard_tv7b4yqg10sw2`. | `[PENDING OWNER]` |
| **Precio Base y Oferta** | PENDIENTE | Nivel de precio ($0.00 Free vs de pago) | Se sugiere **Gratuito (Free)** para maximizar la adopción de la utilidad y alinearse con la naturaleza open source del proyecto. | `[PENDING OWNER]` |
| **Mercados de Distribución** | PENDIENTE | Lista de países habilitados | Seleccionar **Todos los mercados mundiales** (o excluir regiones donde no se ofrezca soporte). | `[PENDING OWNER]` |
| **Visibilidad / Discoverability** | PENDIENTE | Distribución pública o mediante enlace directo | Para lanzamiento general en tienda: **Público (Make this product available in the Store)**. Para pruebas privadas previas: **Oculto (Hidden)**. | `[PENDING OWNER]` |
| **Opciones de Publicación** | PENDIENTE | Inmediata vs Retención Manual | **Manual Publish (Recomendada):** Permite inspeccionar el informe emitido por Microsoft antes de liberar el producto a los usuarios finales. | `[PENDING OWNER]` |
| **Categoría del Producto** | DRAFT READY | Confirmación en portal | Principal: `Security` > `Personal security`. Secundaria: `Utilities & tools`. | `[PENDING OWNER]` |
| **Cuestionario IARC (Clasificación de Edad)** | PENDIENTE | Completar respuestas en el portal | Responder con las funciones reales de la app: 0 violencia, 0 lenguaje ofensivo, 0 compras integradas, 0 uso de red. Suele otorgar clasificación 3+ / Todos. | `[PENDING OWNER]` |
| **URL de Política de Privacidad** | PENDIENTE | URL pública HTTPS | Publicar el contenido de `docs/store/PRIVACY-POLICY-DRAFT.md` en una URL accesible (ej. GitHub Pages o blob público del repo) e ingresarla en el portal. | `[PENDING OWNER]` |
| **Canal de Soporte / Contacto** | PENDIENTE | URL de soporte o correo electrónico | Ingresar enlace al repositorio (`https://github.com/cristianbarreiro/PrivGuard/issues`) o correo oficial del publicador. | `[PENDING OWNER]` |
| **Capturas de Pantalla (Screenshots)** | LISTO | Carga en la ficha comercial | Utilizar las 4 capturas reales auditadas en `release/store/` (`main-window.png`, `mainwindow_dark.png`, `protection_overview.png`, `tray_menu.png`). | `[PENDING OWNER]` |
| **Restricted Capability (`runFullTrust`)** | DRAFT READY | Pegar justificación técnica | Texto oficial redactado y verificado en `docs/store/PARTNER-CENTER-SUBMISSION-CHECKLIST.md`. No requiere cambios. | **DRAFT READY** |
| **Notas para Certificación** | DRAFT READY | Pegar instrucciones de prueba | Texto oficial en inglés con pasos de prueba guiados en `docs/store/PARTNER-CENTER-CERTIFICATION-NOTES.md`. | **DRAFT READY** |

---

## 2. Hoja de Ruta de Entrada de Datos

Para completar el formulario de envío en Partner Center en una sola sesión, el propietario debe disponer de:

1. [ ] Confirmación visual del Store Product ID en `partner.microsoft.com`.
2. [ ] Decisión tomada sobre precio (Free) y mercados.
3. [ ] URL pública accesible para la Política de Privacidad (subir `docs/store/PRIVACY-POLICY-DRAFT.md`).
4. [ ] Archivo binario candidato: `release/store/PrivGuard-2.0.0.0-x64.msix` (SHA-256 verificado: `707857E283D304F0965BD1BCFECC1636017B732E9C6497B883FF7AA28BC45BA7`).
5. [ ] Las 4 imágenes de captura de `release/store/`.
6. [ ] Opción "Manual publish" seleccionada en la pestaña **Submission options**.

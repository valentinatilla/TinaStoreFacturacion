# KNOWN ISSUES — Tina Store

Registro de problemas conocidos, limitaciones y deuda técnica del proyecto.
Los issues marcados ✅ ya están resueltos. Los marcados 🔴/🟡/🟢 están pendientes.

---

## Fases I–N — Issues resueltos y deuda técnica registrada

### ✅ BUG-19 — Error silencioso al subir imagen en edición masiva → resuelto (Fase I)
### ✅ BUG-20 — Campo Unidad acepta números → resuelto (Fase J)
### ✅ BUG-21 — Productos duplicados por nombre/SKU → resuelto parcialmente (Fase J)
### ✅ BUG-22 — Proveedor en blanco en listado/edición masiva → resuelto (Fase K)
### ✅ BUG-23 — Sin indicador de carga en acciones lentas → resuelto (Fase L)
### ✅ BUG-24 — Bajo stock y Agotado al mismo tiempo → resuelto (Fase M)
### ✅ BUG-25 — Plantillas Excel inconsistentes → resuelto (Fase N)

### 🟡 DEUDA-01 — Índice único en Products(Name) y Products(Sku) pendiente
- **Estado**: Deuda técnica. La validación de unicidad está implementada en `ProductService` (nivel de servicio), pero **no existe** un índice único en la base de datos.
- **Motivo**: La BD de desarrollo contiene cientos de productos duplicados por nombre, resultado de una importación masiva accidental. Crear el índice ahora rompería el startup de la aplicación.
- **Impacto**: Si dos usuarios crean el mismo producto simultáneamente (condición de carrera), el servicio no garantiza unicidad absoluta.
- **Acción requerida**: Limpiar los duplicados de la BD (revisar cuáles son legítimos y cuáles son residuos de importación), luego ejecutar una migración que agregue `HasIndex` con `IsUnique()` en `AppDbContext`.
- **Prioridad**: Media (no bloquea operación normal).

---

## Sprint Corrección 2026-06-21 — Issues resueltos en Fase A

### ✅ BUG-A-B02 — Ojito de contraseña no existía → resuelto en Fase A (2026-06-21)
- Login, Usuarios (crear y resetear). Ver BUGFIXES.md para detalle.

### ✅ BUG-A-B01/B03 — Botones de modal Usuarios con btn-sm → resuelto en Fase A (2026-06-21)

### ✅ BUG-A-B04 — Labels seleccionables con doble clic → resuelto en Fase A (2026-06-21)

### ✅ BUG-A-B09 — Campo Contacto en Proveedores → resuelto en Fase A (2026-06-21)
- Requiere `dotnet ef database update` en producción para aplicar la migración.

### ✅ BUG-A-B10/B11 — NIT/Teléfono sin validación en Proveedores → resuelto parcialmente en Fase A
- Validación en frontend lista. Pendiente: actualizar `SupplierValidators.cs` en Fase D.

### ✅ BUG-B05 — Tarjetas del Dashboard no clickeables → resuelto en Fase B (2026-06-22)

### ✅ BUG-B06 — Documento de cliente puede repetirse → resuelto en Fase C (2026-06-22)

### ✅ BUG-B07/B08 — Faltan filtros de email y estado en Clientes → resuelto en Fase C (2026-06-22)

### ✅ BUG-B10/B11 Fase D — Validaciones NIT/teléfono al backend → resuelto en Fase D (2026-06-22)

### ✅ BUG-B12 — Errores de backend no se muestran → resuelto en Fase E (2026-06-22)

### ✅ BUG-B14/B15 — Sin validación por campo en Productos → resuelto en Fase F (2026-06-22)

### ✅ BUG-B17 — Botón X de imagen ovalado → resuelto en Fase F (2026-06-22)

### ✅ BUG-B19 — Imágenes de productos: Content-Type incorrecto → resuelto en Fase F (2026-06-22)

### ✅ BUG-B20 — Enter no guarda en Categorías → resuelto en Fase G (2026-06-22)

### ✅ BUG-B21 — Categorías duplicadas → resuelto en Fase G (2026-06-22)

## 🟡 Pendientes post-Fases B–G

### ✅ BUG-B16 — Drag & drop de imagen en Productos → resuelto en Fase H (2026-06-22)
- `InputFile` superpuesto como overlay invisible captura el drop de forma nativa; estado visual `.ts-drag-active` con `@ondragenter`/`@ondragleave`.

---

## ✅ RESUELTOS EN v2.8.0 (2025-07-14 — Fases A–F)

### ✅ ISSUE-08 — Logo roto en sidebar y configuración → resuelto en v2.8.0
- **Causa**: `ApiBaseUrl` (interna) usada para construir URL de imagen accesible por el navegador.
- **Solución**: `PublicApiUrl`/`PublicBaseUrl` separado; todos los componentes que generan URLs de imágenes ahora usan la URL pública.

### ✅ ISSUE-09 — Imágenes de productos rotas en detalle de venta → resuelto en v2.8.0
- **Causa**: `IConfiguration["ApiBaseUrl"]` inyectado directamente en las páginas Razor, mismo problema que ISSUE-08.

- **Solución**: Eliminada inyección directa; uso de `Api.PublicBaseUrl`.

### ✅ ISSUE-10 — Desbordamiento de tablas y filtros en pantallas pequeñas → resuelto en v2.8.0
- **Causa**: Las filas de filtros no tenían `flex-wrap`; las tablas no ocultaban columnas secundarias en móvil.
- **Solución**: `flex-wrap` y clase `ts-table-hide-mobile` aplicados en todos los módulos.

### ✅ ISSUE-11 — Categorías sin orden alfabético → resuelto en v2.8.0
- **Solución**: Barra de orden A→Z / Z→A con soporte de tildes/ñ mediante `CultureInfo("es-CO")`.

---

## 🟡 PENDIENTES

### ✅ ISSUE-12 — Íconos PWA generados → resuelto en Fase H (2026-06-22)
- `icon-192.png` (192×192) e `icon-512.png` (512×512) creados en `wwwroot/icons/` con la identidad visual de Tina Store: fondo morado, círculo rosa, letra T blanca. Zona maskable al 80%.

---

## ✅ RESUELTOS EN v2.7.0 (2026-06-20 — Fases C + D + E)

### ✅ ISSUE-05 — Diseño del login no alineado con la identidad visual → resuelto en v2.7.0
- **Solución**: Login rediseñado con fondo glassmorphism kawaii, tarjeta con backdrop-filter, logo degradado rosa-morado, animación de entrada. Versión de la app visible (`v2.7.0`) con badge de ambiente (DEV/STAGING) solo en entornos no productivos.

---

## ✅ RESUELTOS EN v1.3.0 (2026-06-19 — Fase A)

### BUG-A1 — Correo sin validación visual → resuelto en v1.3.0
### BUG-A2 — Teléfono sin formato colombiano → resuelto en v1.3.0
### BUG-A3 — Errores de modal visibles fuera del modal → resuelto en v1.3.0
### BUG-A4 — Modales no responsivos ni scrollables → resuelto en v1.3.0

---

## ✅ RESUELTOS EN v1.1.0 (2025-06-18 — Fase A1)

### BUG-06 — Botones de acción invisibles → resuelto en v1.1.0
### BUG-07 — Estado de factura sin estilo → resuelto en v1.1.0
### BUG-08 — Abono siempre $0 en listado → resuelto en v1.1.0
### BUG-09 — Congelamiento al subir imagen → resuelto en v1.1.0
### BUG-10 — Dashboard no conecta API → resuelto en v1.1.0

---

## ✅ RESUELTOS EN v2.5.0 (2026-06-19 — Fase A2)

### BUG-A2-01 — Badge de stock incorrecto en productos inactivos → resuelto en v2.5.0
### BUG-A2-02 — Categoría vacía al abrir modal de edición → resuelto en v2.5.0
### BUG-A2-03 — Conteo de productos por categoría siempre 0 → resuelto en v2.5.0

---

## ✅ RESUELTOS EN versiones anteriores

### BUG-01 — 401 Unauthorized en todas las peticiones autenticadas
- **Versión corregida**: 0.2.0
- **Ver detalles**: `docs/BUGFIXES.md → BUG-01`

### BUG-02 — Campo Cliente vacío en Facturas
- **Versión corregida**: 0.2.0
- **Ver detalles**: `docs/BUGFIXES.md → BUG-02`

### BUG-03 — Categoría y Proveedor se borran al editar productos
- **Versión corregida**: 0.2.0
- **Ver detalles**: `docs/BUGFIXES.md → BUG-03`

---

## 🟡 PENDIENTES (media prioridad)

### ✅ ISSUE-01 — Sesión no persiste al recargar el servidor Blazor → resuelto en v1.3.0
- **Versión corregida**: v1.3.0
- **Solución**: Token JWT persistido en cookie HTTP-only vía `SessionStateService.PersistAsync()` + endpoints `/session/set`, `/session/get`, `/session/clear` en `Program.cs` del Web. `SessionRestorer.razor` restaura la sesión al iniciar el circuito Blazor.

### ✅ ISSUE-02 — Código interno (InternalCode) y SKU duplicaban responsabilidad
- **Versión corregida**: 0.3.0
- **Ver detalles**: `docs/CHANGELOG.md → v0.3.0`

### ✅ ISSUE-03 — Sin soporte de imágenes en productos → resuelto en v2.6.0
- **Versión corregida**: v2.6.0
- **Solución**: Campo `ImagePath` en entidad `Product`. Subida de imagen en modal de Productos. Miniaturas en tabla de Productos, Nueva Venta y Venta Libre. Migración `AddProductImagePath` aplicada.

### ✅ ISSUE-04 — Login solo con usuario/contraseña (sin Google) → resuelto en v1.3.0
- **Versión corregida**: v1.3.0
- **Solución**: Google OAuth integrado. `AuthController.GoogleLogin` valida `id_token` con `Google.Apis.Auth`. Lista `Google:AllowedEmails` controla qué cuentas tienen acceso. Botón "Continuar con Google" visible en Login cuando Google está configurado.

---

## 🟢 BAJA PRIORIDAD / DEUDA TÉCNICA

### WARN-01 — Warnings de EF Core: query filters en relaciones requeridas
- **Módulo**: Infraestructura / Base de datos
- **Descripción**: EF Core lanza advertencias al iniciar sobre entidades con `global query filter` que son el extremo requerido de relaciones (`AccountReceivable`, `InventoryMovement`, `InvoiceDetail`, `Payment`, `Reminder`). No es un error, pero puede causar comportamientos inesperados si una entidad relacionada es eliminada lógicamente.
- **Solución propuesta**: Hacer opcionales esas propiedades de navegación, o agregar query filters espejo en las entidades dependientes.
- **Impacto actual**: Bajo. Los filtros de borrado lógico funcionan correctamente para los casos de uso actuales.

### ✅ ISSUE-05 — Diseño del login no alineado con la identidad visual → resuelto en v2.7.0
- **Versión corregida**: v2.7.0
- **Solución**: Login rediseñado con fondo glassmorphism kawaii, tarjeta con backdrop-filter, logo degradado rosa-morado, animación de entrada. Versión de la app visible (`v2.7.0`) con badge de ambiente (DEV/STAGING) solo en entornos no productivos.

### ISSUE-06 — Sistema de roles innecesariamente complejo para un solo usuario
- **Módulo**: Usuarios / Autenticación
- **Descripción**: Existe un módulo completo de usuarios con roles Admin/Seller/Viewer que no se usa en la práctica actual.
- **Planificado para**: Simplificar en Fase 4, sin eliminar el código para poder escalar.

---

## ✅ RESUELTOS EN v1.2.0 (2026-06-18 — Fases A2 + B1 + B3)

### BUG-09b — Estado de inventario siempre "Activo" → resuelto en v1.2.0
### BUG-10 — Conteo de productos por categoría siempre 0 → resuelto en v1.2.0
### BUG-11 — Fórmula de porcentaje de ganancia incorrecta → resuelto en v1.2.0
### BUG-12 — Bloque @media sin cerrar en CSS → resuelto en v1.2.0
### BUG-13 — Columna Estado en Proveedores (oculta, no eliminada) → resuelto en v1.2.0

---

## 🟡 PENDIENTES (media prioridad — Fases B2+)

### ~~ISSUE-A2-04 — Importación masiva de productos con vista previa~~ ✅ Resuelto en v1.3.0
### ~~ISSUE-B3-01 — Historial de recordatorios WhatsApp no registrado~~ ✅ Resuelto en v1.3.0
### ~~ISSUE-B1-02 — Logo en sidebar no se actualiza sin recargar la app~~ ✅ Resuelto en v1.3.0

---

## ✅ RESUELTOS EN v1.3.0 (Fase B — Pendientes)

### ISSUE-A2-04 — Importación masiva con vista previa editable → resuelto en v1.3.0
- **Solución**: Página `Productos/Importar.razor` con flujo 3 pasos: subir Excel → tabla editable por fila con validación → confirmar e importar. Endpoint `POST /api/documents/productos/previsualizar` (sin guardar) y `POST /api/documents/productos/importar-confirmado` (persistir). `PreviewImportAsync` e `ImportFromPreviewAsync` en `ExcelService`.

### ISSUE-B3-01 — Historial de recordatorios WhatsApp → resuelto en v1.3.0
- **Solución**: `IReminderRepository` + `ReminderRepository` con `AddHistoryAsync`. `IReminderService` + `ReminderService` calculan y persisten `Reminder` y `ReminderHistory`. `RemindersController` expone `POST /api/reminders/whatsapp`. `ConfirmarWhatsApp()` en `Clientes/Index.razor` llama al API tras abrir el enlace de WhatsApp.

### ISSUE-B1-02 — Logo en sidebar sin refresco en tiempo real → resuelto en v1.3.0
- **Solución**: `LogoStateService` (singleton Blazor) con propiedad `LogoUrl` y evento `OnChange`. `MainLayout.razor` suscribe `OnChange` e implementa `IDisposable`. `StoreSettings.razor` llama `LogoState.SetLogo()` tras subir logo exitosamente.

---

## 📝 Cómo reportar un nuevo issue

Al encontrar un bug o limitación, agregar una entrada con:

```markdown
### ISSUE-XX — Título del problema
- **Módulo**: Nombre del módulo
- **Descripción**: Descripción clara del problema.
- **Pasos para reproducir**: (si aplica)
- **Causa identificada**: (si se conoce)
- **Solución propuesta**: (si se conoce)
- **Planificado para**: Fase X / Sin fecha
- **Riesgo**: Alto / Medio / Bajo
```

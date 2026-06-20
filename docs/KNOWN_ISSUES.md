# KNOWN ISSUES — Tina Store

Registro de problemas conocidos, limitaciones y deuda técnica del proyecto.
Los issues marcados ✅ ya están resueltos. Los marcados 🔴/🟡/🟢 están pendientes.

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

### ISSUE-01 — Sesión no persiste al recargar el servidor Blazor
- **Módulo**: Autenticación
- **Descripción**: `SessionStateService` guarda el token solo en memoria del circuito Blazor Server. Si el servidor se reinicia o el circuito se desconecta, el usuario debe volver a iniciar sesión.
- **Causa**: No hay persistencia en cookie ni en `localStorage`.
- **Solución propuesta**: Persistir token en cookie segura (HttpOnly) o en `localStorage` via JS Interop.
- **Planificado para**: Fase 4

### ✅ ISSUE-02 — Código interno (InternalCode) y SKU duplicaban responsabilidad
- **Versión corregida**: 0.3.0
- **Ver detalles**: `docs/CHANGELOG.md → v0.3.0`

### ISSUE-03 — Sin soporte de imágenes en productos
- **Módulo**: Productos
- **Descripción**: No existe campo de imagen en la entidad `Product`. Los listados muestran texto sin representación visual de los productos.
- **Planificado para**: Fase 3

### ISSUE-04 — Login solo con usuario/contraseña (sin Google)
- **Módulo**: Autenticación
- **Descripción**: El sistema actual requiere gestionar contraseñas manualmente. No tiene login con Google.
- **Planificado para**: Fase 4

---

## 🟢 BAJA PRIORIDAD / DEUDA TÉCNICA

### WARN-01 — Warnings de EF Core: query filters en relaciones requeridas
- **Módulo**: Infraestructura / Base de datos
- **Descripción**: EF Core lanza advertencias al iniciar sobre entidades con `global query filter` que son el extremo requerido de relaciones (`AccountReceivable`, `InventoryMovement`, `InvoiceDetail`, `Payment`, `Reminder`). No es un error, pero puede causar comportamientos inesperados si una entidad relacionada es eliminada lógicamente.
- **Solución propuesta**: Hacer opcionales esas propiedades de navegación, o agregar query filters espejo en las entidades dependientes.
- **Impacto actual**: Bajo. Los filtros de borrado lógico funcionan correctamente para los casos de uso actuales.

### ISSUE-05 — Diseño del login no alineado con la identidad visual de Tina Store
- **Módulo**: Login
- **Descripción**: La pantalla de login es funcional pero tiene un estilo diferente al kawaii/rosado del resto de la aplicación.
- **Planificado para**: Fase 5

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

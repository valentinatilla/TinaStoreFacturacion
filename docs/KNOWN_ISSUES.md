# KNOWN ISSUES — Tina Store

Registro de problemas conocidos, limitaciones y deuda técnica del proyecto.
Los issues marcados ✅ ya están resueltos. Los marcados 🔴/🟡/🟢 están pendientes.

---

## ✅ RESUELTOS

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

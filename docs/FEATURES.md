# FEATURES — Tina Store

Registro de funcionalidades implementadas y planificadas del proyecto.

---

## Funcionalidades implementadas (v0.1.0 — v0.2.0)

### ✅ Autenticación
- Login con correo y contraseña.
- Token JWT generado por la API.
- Cierre de sesión.
- Redirección automática al login si no está autenticado.

### ✅ Dashboard
- Resumen de ventas del día, semana y mes.
- Total por cobrar (cuentas por cobrar).
- Gastos del día y del mes.
- Productos con stock bajo.
- Últimas facturas del día.
- Top deudores.

### ✅ Productos
- Listado con búsqueda por nombre y SKU.
- Crear producto con nombre, SKU, descripción, precio de venta/costo, stock, stock mínimo, unidad, categoría y proveedor.
- Editar producto (corregido en v0.2.0).
- Eliminar producto (baja lógica).
- Indicador visual de stock bajo.
- Exportar inventario a Excel.

### ✅ Categorías
- Listado de categorías.
- Crear y eliminar categorías.

### ✅ Proveedores
- Listado de proveedores.
- Crear, editar y eliminar proveedores.
- Campos: nombre, NIT/RUT, contacto, teléfono, email, dirección, notas.

### ✅ Clientes
- Listado de clientes activos e inactivos.
- Crear, editar y eliminar clientes.
- Campos: nombre completo, tipo y número de documento, teléfono, email, dirección, notas.
- Saldo pendiente (fiado/crédito).

### ✅ Facturas
- Listado de facturas con estado (pendiente, pagada, parcial, anulada).
- Crear nueva factura con cliente, productos, descuento e impuesto.
- Pago inicial al crear factura.
- Anular factura.
- Descargar factura en PDF.
- Registro de pagos/abonos sobre facturas pendientes.

### ✅ Egresos
- Listado de egresos.
- Crear y editar egresos con categoría, monto, fecha y método de pago.
- Anular egresos.
- Categorías de egreso: arriendo, servicios, compras a proveedor, transporte, nómina, otros.

### ✅ Cuentas por cobrar
- Listado de clientes con saldo pendiente.
- Resumen de deuda total.

### ✅ Reportes
- Reporte de ventas por rango de fechas.
- Top productos más vendidos.
- Reporte de gastos por categoría.
- Reporte de cuentas por cobrar.

### ✅ Configuración de tienda
- Nombre de la tienda, dirección, teléfono, email, NIT.
- Moneda, porcentaje de impuesto.
- Mensaje de pie de factura.
- Control de stock negativo.

### ✅ Usuarios
- Listado de usuarios del sistema.
- Crear, editar y resetear contraseña de usuarios.
- Roles: Admin, Seller, Viewer.

---

## Funcionalidades planificadas

### 📋 Fase 2 — Simplificación de identificadores de producto
- [x] Eliminar campo "Código interno" (`InternalCode`).
- [x] Mantener únicamente SKU como identificador principal.
- [x] Migración de datos existentes (InternalCode copiado a SKU donde SKU estaba vacío).
- [ ] Validación de SKU único (índice único en BD — planificado para siguiente iteración).

### 📋 Fase 3 — Imágenes de productos ✅ (v0.3.0)
- [x] Campo `ImagePath` en entidad `Product` y columna en BD (migración `AddProductImagePath`).
- [x] `ImagePath` propagado en `ProductDto` y `ProductSummaryDto`.
- [x] Endpoints API: `POST /api/products/{id}/imagen` y `DELETE /api/products/{id}/imagen`.
- [x] Validación: máximo 2 MB, solo JPG / PNG / WEBP.
- [x] Archivos servidos como static files desde `wwwroot/uploads/productos/`.
- [x] Miniatura en listado de productos (40×40 px, con placeholder si no hay imagen).
- [x] Sección de imagen en el modal de creación/edición con preview en base64.
- [x] Reemplazar imagen automáticamente al subir una nueva (borra el archivo anterior).
- [x] Botón para quitar imagen en el modal.
- [x] Estilos CSS consistentes con el tema Kawaii rosado.

### 📋 Fase 4 — Autenticación con Google ✅ (v0.4.0)
- [x] Botón "Continuar con Google" en la pantalla de login con logo SVG oficial.
- [x] Flujo OAuth 2.0 / OIDC con `Microsoft.AspNetCore.Authentication.Google` en la Web.
- [x] Endpoint `POST /api/auth/google` en la API: valida `id_token` con `Google.Apis.Auth`.
- [x] Lista de correos autorizados configurable en `appsettings.json` (`Google:AllowedEmails`).
- [x] Búsqueda o creación automática del usuario local al autenticar con Google.
- [x] `GoogleCallback.razor` (`/auth/google-complete`) cierra el ciclo OAuth y establece la sesión JWT de TinaStore.
- [x] Manejo de errores en login: mensajes claros si Google falla o el correo no está autorizado.
- [x] Login con contraseña conservado (no se eliminó).
- [x] Separador visual "o" entre formulario y botón Google.
- [x] Estilos `.btn-google` y `.login-divider` integrados al tema Kawaii.
- [x] Placeholders de configuración en `appsettings.Development.json` de Web y API.
- [ ] Sesión persistente entre recargas (planificado para Fase siguiente — requiere almacenamiento de token en cookie o localStorage).

### 📋 Fase 5 — Rediseño del Login + Sesión Persistente ✅ (v0.5.0)
- [x] Fondo con capas de degradado kawaii usando pseudoelementos `::before` y `::after`.
- [x] Tarjeta glassmorphism: `backdrop-filter: blur(24px)`, fondo semitransparente, borde sutil, sombra multicapa.
- [x] Logo de la tienda con icono en cuadro redondeado degradado rosa-violeta con sombra de color.
- [x] Animación de entrada `loginFadeIn` (fade + slide + scale).
- [x] Botón principal con degradado rosa-violeta, elevación en hover y estado deshabilitado correcto.
- [x] Botón "Continuar con Google" con borde suave, sombra en hover y elevación.
- [x] Separador visual "o" con líneas semitransparentes mejoradas.
- [x] Responsive: tarjeta ocupa 90% del ancho en móvil, padding adaptado.
- [x] Cookie HTTP-only `tinastore_session` para persistir JWT entre recargas (duración 1 día).
- [x] Endpoints `/session/set`, `/session/clear` y `/session/get` en la Web (Minimal API).
- [x] `SessionRestorer.razor` restaura la sesión automáticamente al arrancar el circuito Blazor.
- [x] Logout borra la cookie de sesión via `fetch('/session/clear')`.
- [x] `SessionStateService` añade `RestoreSession`, `PersistAsync` y `ClearPersistenceAsync`.

### 📋 Fase 6 — Documentación y pruebas
- [x] Estructura de documentación `docs/`.
- [ ] Pruebas unitarias completas.
- [ ] Pruebas de integración.
- [ ] Guía de despliegue en producción.

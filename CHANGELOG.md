# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue el estándar [Keep a Changelog](https://keepachangelog.com/es/1.1.0/).
El versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

---

## [1.4.0] — 2026-06-23

### Añadido
- **Exportación Excel de clientes** (`GET /api/customers/exportar`): hoja con nombre, documento, teléfono, email, dirección, saldo pendiente y fecha de última compra. Botón con spinner en la pantalla de clientes.
- **Exportación Excel de ventas** (`GET /api/invoices/exportar?desde=&hasta=`): dos hojas — "Resumen ventas" (número, fecha, cliente, total, pagado, saldo, estado) y "Detalle de productos" (factura, SKU, producto, cantidad, precio unitario, descuento, subtotal). Botón con spinner respetando los filtros de fecha activos.
- **Categoría del sistema "Sin categoría"** (Id=99): se crea automáticamente vía seed y migración `AddSinCategoriaCategory`. Los productos sin clasificar se reasignan a esta categoría al eliminar la categoría que los contenía.
- **Migración EF Core** `AddSinCategoriaCategory` para el seed de la categoría del sistema.
- **44 nuevos tests unitarios**: `CategoryServiceTests` (13), `StoreSettingsServiceTests` (5), `ValidatorsTests` (26).

### Cambiado
- **Límites de caracteres** endurecidos en validadores y formularios:
  | Campo | Antes | Ahora |
  |---|---|---|
  | Cliente — nombre | 200 | 100 |
  | Cliente — teléfono | regex flexible 7-20 | exactamente 10 dígitos |
  | Producto — nombre | 200 | 100 |
  | Producto — SKU | 100 | 50 |
  | Producto — descripción | sin límite | 300 |
  | Producto — unidad de medida | sin límite | 30 |
  | Categoría — nombre | 100 | 60 |
  | Categoría — descripción | 500 | 200 |
  | Proveedor — nombre | 200 | 100 |
- Todos los formularios Blazor actualizados con atributos `maxlength` acordes a los nuevos límites.
- **Precio de venta**: la validación frontend rechaza valores `<= 0`. Se añade techo de `$9.999.999,99` en validadores backend y atributos `max` en los inputs.
- **Nombre de la tienda fijo**: `StoreSettingsService.UpdateAsync` ignora el valor enviado por la UI y siempre persiste `"Tina Store"`. El campo en la pantalla de configuración es ahora de solo lectura con leyenda informativa.
- **Categorías — listado de gestión**: la categoría del sistema "Sin categoría" (Id=99) se oculta del listado de `Categorias/Index.razor` pero sigue disponible en los selectores de productos.

### Protegido
- `CategoryService.CreateAsync`: lanza `InvalidOperationException` si el nombre es "Sin categoría".
- `CategoryService.UpdateAsync`: lanza `InvalidOperationException` si `id == 99`.
- `CategoryService.DeleteAsync`: lanza `InvalidOperationException` si `id == 99`. Al eliminar cualquier otra categoría, los productos que la referencien se reasignan automáticamente a Id=99.

---

## [1.3.0] — 2026-06-22

### Añadido
- Índices únicos filtrados en `Products(Name)` y `Products(Sku)` (solo filas activas).
- Migración `AddUniqueIndexProductNameSku`.
- Script de limpieza de duplicados en `tinastore-dev.db` (soft-delete).

### Corregido
- `LeerMensajeErrorAsync` en `TinaStoreApiClient`: ahora lee la clave `mensaje` (además de `message`) para mostrar errores del backend en español.
- `ProductsController`, `CustomersController`, `InvoicesController`: respuestas de error de FluentValidation y `DomainException` unificadas al formato `{ mensaje: "..." }`.

---

## [1.2.0] — 2026-06-20

### Añadido
- Módulo de egresos (gastos) con categorías de egreso.
- Recordatorios de cobro con historial de envío.
- Dashboard con métricas de ventas, clientes con deuda y stock bajo.
- Exportación e importación masiva de productos vía Excel (ClosedXML).
- Vista previa de importación con confirmación antes de persistir.
- Edición masiva de productos (precio, costo, stock) en dos pasos.
- PDF de factura con logo y datos de la tienda (QuestPDF).

### Cambiado
- Arquitectura reorganizada en capas: Domain, Application, Infrastructure, Api, Web.
- Autenticación migrada a JWT con roles (Admin/Operator).

---

## [1.1.0] — 2026-06-10

### Añadido
- Módulo de cuentas por cobrar con abonos parciales.
- Registro de pagos con múltiples métodos (Efectivo, Nequi, Daviplata, Transferencia, Tarjeta, Crédito).
- Ventas libres (sin producto de inventario).
- Anulación de facturas con razón y reversión de stock/cuentas por cobrar.

---

## [1.0.0] — 2026-06-01

### Añadido
- Gestión de clientes (CRUD + búsqueda + estado comercial).
- Gestión de productos con inventario (stock mínimo, stock bajo, imágenes).
- Gestión de categorías y proveedores.
- Facturación con detalle de productos, descuentos e impuestos.
- Configuración de tienda (nombre, logo, IVA, pie de factura, mensaje WhatsApp).
- Autenticación de usuarios.
- Borrado lógico en clientes, productos, categorías, facturas y egresos.
- Base de datos SQLite con EF Core y migraciones automáticas al arrancar.

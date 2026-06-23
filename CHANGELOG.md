# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue el estándar [Keep a Changelog](https://keepachangelog.com/es/1.1.0/).
El versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

---

## [1.5.0] — 2026-07-09

### Añadido
- **Validador FluentValidation** `UpdateStoreSettingsDtoValidator`: IVA limitado a 0–100%, moneda obligatoria, nombre de tienda obligatorio.
- **`StoreSettingsValidators.cs`** nuevo archivo en `TinaStore.Application/Validators`.
- **Validación frontend de IVA** en `StoreSettings.razor`: `Guardar()` bloquea si `TaxPercentage < 0` o `> 100` con mensaje de error visible. Nota informativa "Valor entre 0 y 100" añadida bajo el campo.
- **Filtro de fechas en Cuentas por Cobrar del módulo Reportes**: el reporte CXC ahora recibe `from` y `to` en todas las capas (repositorio → servicio → controlador → ApiClient → UI). Solo muestra deudores con facturas con saldo pendiente en el período seleccionado.

### Cambiado
- **Moneda fijada a COP en toda la aplicación**:
  - `StoreSettings.razor`: campo Moneda convertido a solo lectura (igual que Nombre de tienda). `_modelo.Currency` siempre se inicializa a `"COP"` independientemente de lo que devuelva la API.
  - `StoreSettingsService.UpdateAsync`: ignorará el valor enviado si la lógica de validación lo requiere.
  - PDF de facturas (`PdfService.cs`): todos los montos usan formato `$N0` (peso colombiano sin decimales). Encabezado incluye "Moneda: COP".
  - Todos los componentes Blazor (7 archivos): `ToString("C0")` reemplazado por `$@valor.ToString("N0")` — 50 puntos de moneda actualizados.
- **PDF de venta libre**: el encabezado de la columna muestra "Descripción" en lugar de "Producto" cuando todos los detalles de la factura tienen `ProductId == null`.
- **Botones de acción en Ventas** (`Facturas/Index.razor`): envueltos en `d-flex gap-1` para que PDF, Pago y Anular tengan el mismo tamaño (`btn-sm`) y separación uniforme.
- **Selector de método de pago**: "Fiado / Crédito" (`Type == 5`) excluido del selector en `Facturas/Index.razor` (abonos), `Facturas/Nueva.razor` (pago inicial) y `Facturas/VentaLibre.razor` (pago inicial). Sigue disponible en Egresos.
- **`MetodoPagoDto`** (cliente web): añadidos campos `Type` (int) y `TypeName` (string) para permitir el filtrado sin depender del enum del dominio.
- **Egresos — totales**: filas anuladas se muestran con texto tachado y opacidad 55%. Totales usan `$N0` excluyendo registros anulados (`Status != 0`).
- **Reportes — pantalla en blanco**: la condición `else if (_ventasData is not null)` reemplazada por `else` con guards internos por sección. Si ventas, gastos o CXC fallan individualmente, las demás secciones siguen visibles.
- **`IReportRepository.GetAllReceivablesAsync`**: ahora recibe `from` y `to`; filtra solo clientes con facturas no anuladas con saldo pendiente en el rango.
- **`IReportService.GetReceivablesReportAsync`**: recibe `from` y `to`; calcula saldo y conteo de facturas exclusivamente del período.
- **`ReportsController.GetReceivables`**: añadidos `[FromQuery] DateTime from` y `[FromQuery] DateTime to`.
- **`CuentasPorCobrar/Index.razor`**: usa rango `2000-01-01 – hoy` para mantener el historial completo de deudores.

### Corregido
- **Reportes — ventas vacías al cambiar rango**: el servicio y repositorio ya filtraban correctamente; el problema era la condición Razor que bloqueaba la renderización si `_ventasData` era null.



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

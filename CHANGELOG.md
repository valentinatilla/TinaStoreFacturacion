# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue el estándar [Keep a Changelog](https://keepachangelog.com/es/1.1.0/).
El versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

---

## [1.7.0] — 2026-07-02

### Añadido
- **Ventas – Imágenes de productos:** Las imágenes de productos en la pantalla de nueva venta ahora se sirven correctamente a través de la ruta proxy `/proxy/img/productos/...`, resolviendo el renderizado roto en producción.
- **Soporte .avif:** El servidor acepta, valida (magic bytes) y almacena imágenes en formato `.avif`. Los inputs de archivo en Productos e Importar también aceptan este formato.
- **Productos – Filtro "Recientes (7 días)":** Nuevo botón de acceso rápido `🕐 Recientes` junto al botón "Limpiar" en la barra de filtros. El selector de Estado también incluye la opción y queda sincronizado al pulsar el botón.
- **Productos – SKU editable en edición:** El campo SKU ya no está bloqueado en modo edición. El botón de sugerencia ✨ está disponible tanto al crear como al editar.
- **Importación Excel – Egresos automáticos:** Al confirmar una importación masiva de productos, se crean automáticamente registros de egreso en la categoría "Compras a proveedor" para cada producto importado con precio de costo y stock mayor a cero.
- **Importación Excel – Imagen por fila:** La tabla de previsualización incluye una columna de imagen. Cada fila permite seleccionar una imagen individual (JPG, PNG, WEBP, AVIF ≤ 2 MB) con vista previa inline. Las imágenes se suben automáticamente tras la importación exitosa.
- **Importación Excel – Reporte de duplicados:** Los productos cuyo nombre ya existe en la base de datos se descartan y se informan claramente en el resultado de importación.
- **Edición masiva – Categoría y Proveedor:** La tabla de edición masiva incluye columnas para cambiar la categoría y el proveedor de cada producto seleccionado, con opción de limpiar el proveedor.
- **Categorías – Editar categoría existente:** Botón ✏️ por fila que abre el mismo modal de creación en modo edición, permitiendo modificar nombre y descripción.

### Corregido
- **Egresos – Bloqueo de anulación:** Si el producto asociado al egreso ya tiene ventas registradas, la anulación se bloquea con un mensaje claro en la UI en lugar de fallar silenciosamente o revertir stock de forma incorrecta.

---

## [1.6.0] — 2026-06-25

### Añadido
- **Productos – Botón "Siguiente SKU":** Al crear un nuevo producto, un botón junto al campo SKU sugiere automáticamente el próximo código disponible en la serie `SKU-XXXX`.
- **Productos – "Sin categoría" en filtros:** El desplegable de categoría del listado incluye la opción "Sin categoría" (valor `-1`) para filtrar productos sin categoría asignada.
- **Egresos – Hora en tablas:** La columna Fecha de egresos muestra ahora también la hora (`dd/MM/yyyy HH:mm`) en ambas tablas (Compras y Otros).
- **Importar – Botón "Corregir":** En el paso 3 (resultado de importación), si hubo errores, aparece un botón "Corregir errores" que regresa al paso 2 manteniendo la previsualización para editar las filas con problemas.
- **CuentasPorCobrar – Modal de previsualización WhatsApp:** Antes de abrir WhatsApp se muestra un modal con el mensaje editable; tiene un texto fijo por defecto con nombre y saldo del deudor.
- **Egresos – Categoría "Insumos":** Nueva categoría de egreso disponible tanto en instalaciones nuevas (seed) como en bases de datos existentes (migración `AddInsumosExpenseCategory`).
- **Configuración – NIT editable cuando está vacío:** Si el NIT llega vacío en producción, el campo pasa a ser editable para que se pueda ingresar una sola vez; una vez guardado queda bloqueado.

### Corregido
- **#1 Ganancia cuando costo = 0:** Productos con `PurchasePrice = 0` muestran el precio de venta como "ganancia neta" en lugar de "—".
- **#5 Ventas por día en reportes:** `ReportService` calcula rangos de fecha en UTC-5 (Colombia) y agrupa por día local para que el gráfico de ventas muestre datos correctamente.
- **#6 WhatsApp mensaje fijo:** Reemplazado envío directo por un modal de previsualización con mensaje configurable y texto por defecto.
- **#7 Descuento decimal en ventas:** El input de descuento usa `step="0.01"` para evitar que un valor de `1` se trate como entero y genere precio en cero.
- **#8 Hora en egresos:** Ambas tablas de egresos muestran `dd/MM/yyyy HH:mm` en lugar de solo la fecha.
- **#9 / #10 Teléfono y documento en editar cliente:** Los campos Teléfono y Documento del modal de edición bloquean letras mediante `inputmode + pattern + oninput` (métodos `SoloNumerosTelefono` / `SoloNumerosDocumento`).
- **#11 Botón Limpiar tamaño en clientes:** El botón usa `height:fit-content` para no crecer al tamaño del `col-auto` del flex container.
- **#13 Error email proveedor muestra 404:** Se añade validación de formato de email en el frontend antes de llamar la API, mostrando mensaje claro en el modal.
- **#14 Categoría "Sin categoría" en filtro de productos:** Opción añadida al `<select>` de categoría con valor `-1`; el filtro busca productos con `CategoryId == null`.
- **#15 Logo no renderiza:** La URL del logo incluye `?v={UnixTimeSeconds}` al cargar y al subir, invalidando la caché del navegador.
- **#16 NIT vacío en producción:** NIT editable cuando está vacío; una vez guardado se vuelve `readonly` para evitar modificaciones accidentales.
- **#17 Insumos en tipo de egreso:** Categoría "Insumos" añadida al seed inicial y migración EF Core para bases de datos existentes.

---

## [1.5.0]

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

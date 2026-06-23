# CHANGELOG — Tina Store

Historial de versiones y cambios relevantes del proyecto.
Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

---

## [Unreleased] — Fases I–N: Correcciones de Productos (imágenes masivas, validaciones, proveedor, estados, Excel)

### Tipo de cambio
Corrección de bugs / Validaciones / Mejoras UX / Consistencia de datos

### Módulos afectados
Productos (Index.razor, Importar.razor), ProductService, ProductValidators, ProductDtos, ExcelService, ProductsController, TinaStoreApiClient, Product (dominio), SpecificRepositories

### Cambios

**BUG-19 — Error silencioso al subir imagen en edición masiva**
- El `catch` genérico en `GuardarEdicionMasiva` ocultaba el error real. Ahora se captura la excepción por fila y se reporta en la vista del paso 2.
- `SubirImagenProductoAsync` retorna `null` cuando el servidor responde con error; se contabiliza como `_bulkImagenesErrores` con mensaje visible.
- Agregados `[RequestSizeLimit(5MB)]` y `[RequestFormLimits]` en el endpoint `POST /{id}/imagen`.

**BUG-20 — Campo Unidad acepta números**
- Regex `^[a-záéíóúÁÉÍÓÚñÑüÜ\s]+$` agregada en `CreateProductValidator` y `UpdateProductValidator`.
- Validación frontend en `Guardar()` con mensaje "La unidad de medida no debe contener números."

**BUG-21 — Productos duplicados por nombre o SKU**
- `ProductService.CreateAsync` y `UpdateAsync` verifican unicidad de `Name` y `Sku` antes de persistir.
- `FindByNameAsync` agregado a `IProductRepository` e implementado en `ProductRepository`.
- `DomainException` capturada en `ProductsController` y retornada como HTTP 400.
- `CreateProductoAsync` y `UpdateProductoAsync` del cliente web devuelven `(resultado, error)` para propagar el mensaje al `_errorModal` del formulario.
- **Nota**: No se creó índice único en BD porque existen cientos de registros duplicados históricos (importación previa). Deuda técnica registrada en KNOWN_ISSUES.

**BUG-22 — Proveedor en blanco en listado y edición masiva**
- **Causa raíz**: `ProductSummaryDto` no incluía `SupplierId` ni `SupplierName`. El endpoint `GET /api/products` lo retorna para el listado y la edición masiva.
- Agregados `int? SupplierId` y `string? SupplierName` a `ProductSummaryDto`.
- `ToSummaryDto` en `ProductService` actualizado para mapear esos campos.

**BUG-23 — Sin indicador de carga en acciones lentas**
- Botón "Exportar Excel" en `Index.razor`: variable `_exportando` + spinner Bootstrap + `disabled`.
- Botón "Descargar plantilla" en `Importar.razor`: variable `_descargandoPlantilla` + spinner.
- Botón "Previsualizar" en `Importar.razor`: texto cambia a "Analizando..." durante la carga.

**BUG-24 — Producto muestra "Bajo stock" y "Agotado" al mismo tiempo**
- `IsLowStock` en `Product.cs` corregido: `CurrentStock > 0 && CurrentStock <= MinimumStock`.
- `GetLowStockAsync` en `ProductRepository` actualizado: `p.CurrentStock > 0 && p.CurrentStock <= p.MinimumStock`.
- Los estados son ahora mutuamente excluyentes: Agotado (stock=0) y Bajo stock (0 < stock ≤ mínimo).

**BUG-25 — Plantillas Excel de importación y exportación con estructura diferente**
- Ambas ahora usan 10 columnas en orden: SKU, Nombre, Descripción, Categoría, Proveedor, Costo, Precio de venta, Stock, Stock mínimo, Unidad de medida.
- Eliminadas columnas obsoletas: ID, Activo, "Precio Costo" (renombrado a Costo), etc.
- Color de encabezado alineado al tema Tina Store (#DB2777 rosa).
- `ImportProductsAsync` y `PreviewImportAsync` actualizados para leer la nueva estructura.

---

## [Unreleased] — 2026-06-22 — Fase H: Drag & drop de imagen + Íconos PWA

### Tipo de cambio
Corrección de bug / Mejora UX / Assets PWA

### Módulos afectados
Productos (Index.razor), CSS global (app.css), PWA (wwwroot/icons)

### Cambios

**BUG-B16 — Drag & drop de imagen en modal de Productos**
- El `InputFile` invisible se posiciona como overlay sobre toda la zona de carga usando `.ts-file-input-overlay` (`position: absolute; inset: 0; opacity: 0`). El browser entrega nativamente los archivos arrastrados al input y `OnChange` se dispara igual que con clic.
- `@ondragenter`/`@ondragleave` gestionan el estado `_dragActivo` que aplica `.ts-drag-active` (borde morado, fondo semitransparente) como feedback visual durante el arrastre.
- `@ondragover:preventDefault` y `@ondrop:preventDefault` evitan que el browser abra el archivo.
- Texto de la zona actualizado: "Arrastra una imagen aquí o haz clic para seleccionar".

**ISSUE-12 — Íconos PWA generados**
- `icon-192.png` (192×192 px) e `icon-512.png` (512×512 px) creados en `wwwroot/icons/`.
- Diseño: fondo cuadrado #7C3AED (morado), círculo central #F472B6 (rosa), letra "T" blanca. Cumple zona maskable 80%.
- El manifest ya los referenciaba; ahora `chrome://flags` y Lighthouse no reportan íconos faltantes.

---

## [Unreleased] — 2026-06-22 — Fases B–G: Dashboard, Clientes, Proveedores, Errores API, Productos, Categorías

### Tipo de cambio
Corrección de bug / Mejora UX / Validaciones backend

### Módulos afectados
Dashboard (Home.razor), Clientes, Proveedores, Categorías, Productos, TinaStoreApiClient, CSS global

### Cambios

**B05 — Tarjetas KPI del Dashboard clickeables**
- Las 4 tarjetas KPI de la fila superior son ahora `<a>` con navegación: Ventas → `/facturas`, Por cobrar → `/cuentas-por-cobrar`, Stock bajo → `/productos`.
- Nueva clase CSS `.kpi-card-link` con hover que resalta el valor en color primario.

**B06 — Unicidad de documento en Clientes**
- `CustomerService.CreateAsync` y `UpdateAsync` verifican que no exista otro cliente con el mismo documento antes de guardar.
- `CustomersController` devuelve HTTP 409 Conflict con el mensaje descriptivo.
- El frontend muestra el error real de la API en el modal.

**B07/B08 — Filtros de email y estado en Clientes**
- La barra de filtros ahora incluye campo de texto para email y selector de estado (Todos / Activo / Inactivo / Sin compras).
- `Filtrar()` y `LimpiarFiltros()` actualizados para los tres criterios en simultáneo.

**B10/B11 Fase D — Validaciones NIT/Teléfono en backend (Proveedores)**
- `SupplierValidators.cs` ahora valida `TaxId` (solo dígitos) y `Phone` (solo dígitos, máximo 10) en `CreateSupplierValidator` y `UpdateSupplierValidator`.

**B12 — Mensajes de error reales de la API en formularios**
- Nuevo helper `LeerMensajeErrorAsync` en `TinaStoreApiClient` lee el campo `message` o el array de errores de FluentValidation.
- `CreateClienteAsync`, `UpdateClienteAsync`, `CreateProveedorAsync`, `UpdateProveedorAsync`, `CreateCategoriaAsync` ahora devuelven `(bool Ok, string? Error)`.
- Los formularios de Clientes, Proveedores y Categorías muestran el mensaje exacto en el modal.

**B14/B15 — Validaciones por campo en Productos**
- `Guardar()` en Productos verifica: nombre obligatorio, categoría seleccionada, precios y stocks no negativos, con mensajes individuales por campo antes de llamar a la API.

**B17 — Botón X de imagen circular**
- `.ts-image-remove-btn` corregido: `width/height: 24px`, `padding: 0`, `display: flex; align-items/justify-content: center`. Ahora es perfectamente circular.

**B19 — Imágenes de productos: Content-Type correcto**
- `SubirImagenProductoAsync` ahora envía `image/jpeg`, `image/png` o `image/webp` según la extensión del archivo, en lugar de `application/octet-stream`. El controller de la API recibe y procesa correctamente el archivo.

**B20 — Enter guarda en modal de Categorías**
- Los inputs Nombre y Descripción del modal de nueva categoría procesan `@onkeydown` y llaman a `Guardar()` al presionar Enter.

**B21 — Unicidad de nombre en Categorías**
- `CategoryService.CreateAsync` verifica que no exista otra categoría con el mismo nombre (case-insensitive).
- `CategoriesController` devuelve HTTP 409 Conflict. El modal muestra el error descriptivo.

---

## [Unreleased] — 2026-06-21 — Fase A: Botones, ojito, user-select, ContactName Proveedores

### Tipo de cambio
Corrección de bug / Mejora UX

### Módulos afectados
Login, Usuarios, Proveedores, CSS global (`app.css`)

### Cambios

**B01/B03 — Botones normalizados en Usuarios**
- Botones del footer de modales (Cancelar / Guardar / Restablecer) en Usuarios ahora son tamaño normal, sin `btn-sm`, consistentes con Clientes y otros módulos.
- Estilos `.btn-warning` y `.btn-outline-warning` añadidos al sistema visual con `border-radius: 999px`.

**B02 — Ojito de contraseña**
- Login: botón ojito funcional en campo contraseña. Alterna `type="password"` ↔ `type="text"`. Ícono cambia entre `bi-eye-fill` y `bi-eye-slash-fill`. Se oculta al hacer submit.
- Usuarios — modal nuevo usuario: ojito en campo contraseña.
- Usuarios — modal resetear contraseña: dos ojitos independientes, uno por campo.
- Los ojitos se resetean (contraseña vuelve a ocultarse) al abrir y cerrar los modales.
- Clase CSS reutilizable `.btn-password-toggle` añadida a `app.css`.

**B04 — user-select: none en etiquetas de interfaz**
- `user-select: none` aplicado a: `form-label`, encabezados de tabla `thead th`, `.badge`, `.modal-title`, `h4/h5/h6.fw-bold`, `.btn` y elementos de navegación del sidebar.
- No afecta la selección de contenido de datos (nombres de clientes, documentos, correos, números de factura).

**B09 — Quitar campo Contacto en Proveedores**
- Campo `ContactName` eliminado del formulario y de la tabla de proveedores.
- Eliminado de la entidad `Supplier`, DTOs (`SupplierDto`, `CreateSupplierDto`, `UpdateSupplierDto`), servicio `SupplierService`, y `TinaStoreApiClient`.
- Migración `20260621000001_RemoveSupplierContactName` elimina la columna de la BD.

**B10/B11 — Validaciones y filtros en Proveedores**
- Barra de filtros rediseñada con `input-group` + botón "Limpiar" (consistente con todos los módulos).
- NIT: solo números, validado en `Guardar()`.
- Teléfono: solo números, máximo 10 dígitos, validado en `Guardar()`.
- Errores mostrados dentro del modal con `alert-danger`. El modal no se cierra si hay error.

---

## [2.8.0] — 2025-07-14 — Fases A–F: Responsive global, botones, logo, orden, filtros avanzados y PWA

### Tipo de cambio
Mejora UX / Corrección de bug / Nueva funcionalidad / PWA

### Módulos afectados
Categorías, Clientes, Ventas/Facturas, Productos, Usuarios, Cuentas por cobrar, Configuración, Layout, CSS global, App.razor

### Cambios

**Fase A — Ordenamiento en Categorías**
- Barra de orden A→Z / Z→A con soporte para tildes y ñ (cultura `es-CO`).
- Búsqueda local en tiempo real por nombre.
- La columna Descripción se oculta en móvil con `ts-table-hide-mobile`.

**Fase B — Responsive en módulos anchos**
- `flex-wrap` aplicado en las barras de filtros de todos los módulos clave.
- Columnas secundarias de las tablas marcadas con `ts-table-hide-mobile` para no desbordarse en pantallas pequeñas.

**Fase C — Corrección del logo roto**
- El logo subido en Configuración aparecía roto en el sidebar porque la URL de la API interna no es accesible por el navegador en Railway.
- Se introdujo `PublicApiUrl`/`PublicBaseUrl` (separado de `ApiBaseUrl`) para que el navegador use la URL pública correcta.
- `Program.cs` inyecta `publicApiUrl` al constructor de `TinaStoreApiClient`.
- `appsettings.json`, `appsettings.Development.json` y `appsettings.Production.json` actualizados con la nueva clave.

**Fase D — Sistema global de botones**
- Variables CSS `--ts-purple`, `--ts-purple-dark`, `--ts-purple-light` añadidas a `:root`.
- Clases semánticas `.btn-primary-tina`, `.btn-secondary-tina`, `.btn-danger-tina`, `.btn-icon-tina`, `.btn-ghost-tina`, `.btn-loading-tina`.
- `@keyframes btnSpin` para el spinner de carga en botones.

**Fase E — Filtros avanzados en Clientes**
- Nuevos controles: "Saldo pendiente mayor que", "Saldo pendiente menor que", "Días sin comprar mayor que".
- Lógica completamente en frontend; `Filtrar()` y `LimpiarFiltros()` actualizados.

**Fase F — PWA y logo móvil**
- `wwwroot/manifest.webmanifest` creado con nombre, short_name, colores del tema y referencias a íconos PWA.
- `App.razor` actualizado con `<link rel="manifest">`, `apple-touch-icon` y meta tags `apple-mobile-web-app-capable`.
- Carpeta `wwwroot/icons/` creada con README explicando cómo generar los íconos 192×192 y 512×512.

---

## [2.7.0] — 2026-06-20 — Fases C + D + E: Versión en login, responsive móvil y documentación

### Tipo de cambio
Mejora UX / Corrección / Documentación

### Módulos afectados
Login, Layout, CSS global, Documentación

### Cambios

**Fase C — Versión de la aplicación en el login**
- La pantalla de login muestra la versión de la app (`v2.7.0`) leída desde `appsettings.json > AppVersion`.
- Se muestra un badge `DEV` o `STAGING` según el ambiente. En producción no aparece nada.
- Versión centrada y discreta debajo del subtítulo "Panel Administrativo".

**Fase D — Mejoras responsive para móvil**
- Sidebar: migrado de manipulación JavaScript directa a estado Blazor (`_sidebarOpen`). El botón hamburguesa es un `@onclick` de Blazor.
- Backdrop oscuro semitransparente al abrir el sidebar en móvil. Al tocarlo, cierra el sidebar.
- El sidebar se cierra automáticamente al navegar a una nueva página (`LocationChanged`).
- Botones e inputs con `min-height` táctil (40-44px) en pantallas ≤ 768px.
- Modales: `max-height: 92vh; overflow-y: auto` en pantallas ≤ 576px.
- Tablas: `overflow-x: auto` garantizado. Clase utilitaria `.ts-table-hide-mobile` para ocultar columnas secundarias.
- Padding del área de contenido reducido en móvil a `.75rem`.
- Cabeceras de página en columna en pantallas pequeñas.

**Fase E — Documentación actualizada**
- `docs/KNOWN_ISSUES.md`: ISSUE-01 (sesión), ISSUE-03 (imágenes), ISSUE-04 (Google), ISSUE-05 (login) marcados como resueltos con versión y descripción de la solución real.
- `docs/ENVIRONMENTS.md`: tabla de bases de datos corregida. Refleja SQLite como motor actual en todos los ambientes. PostgreSQL documentado como opción opcional.

**Fix de tests (incluido en esta versión)**
- `CustomerServiceTests`: 4 mocks corregidos (`GetAllWithInvoicesAsync`, `GetWithInvoicesAsync`, `Invoices=[]`).
- `ExpenseServiceTests`: mock corregido (`GetAllWithNavigationAsync`).
- `InvoiceServiceTests`: asserts actualizados a español (`Pagada`, `Anulada`).
- `AuthController`: `AllowedEmails` vacío ahora deniega acceso Google explícitamente.
- **Tests: 30/30 ✅**

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `src/TinaStore.Web/appsettings.json` | Campo `AppVersion: "2.7.0"` |
| `src/TinaStore.Web/Components/Pages/Login.razor` | Versión + badge de ambiente |
| `src/TinaStore.Web/Components/Layout/MainLayout.razor` | Sidebar Blazor + backdrop + LocationChanged |
| `src/TinaStore.Web/wwwroot/app.css` | Estilos versión, backdrop, responsive, táctil |
| `src/TinaStore.Api/Controllers/AuthController.cs` | AllowedEmails vacío = denegar |
| `tests/TinaStore.Tests.Unit/CustomerServiceTests.cs` | Mocks corregidos |
| `tests/TinaStore.Tests.Unit/ExpenseServiceTests.cs` | Mock corregido |
| `tests/TinaStore.Tests.Unit/InvoiceServiceTests.cs` | Asserts en español |
| `docs/KNOWN_ISSUES.md` | Issues resueltos actualizados |
| `docs/ENVIRONMENTS.md` | Base de datos actualizada |

### Resultado
✅ Build exitoso. Tests: 30/30.

---

## [2.5.0] — 2026-06-19 — Fase A2: Corrección de bugs en Productos y Categorías

### Tipo de cambio
Bugfix

### Módulos afectados
Productos, Categorías

### Correcciones
- **BUG-A2-01**: Badge numérico de stock ahora muestra `bg-secondary` para productos inactivos en lugar de verde.
- **BUG-A2-02**: Al abrir el modal de edición de un producto, la categoría (y demás campos) ya se precarga correctamente desde el DTO de listado cuando el endpoint de detalle no responde.
- **BUG-A2-03**: `ProductCount` en categorías ya refleja el número real de productos; se eliminó el filtro redundante `&& p.IsActive` que excluía productos inactivos del conteo.

---

## [2.4.0] — 2026-06-19 — Fase D: Dashboard mejorado

### Tipo de cambio
Nueva funcionalidad visual

### Módulo afectado
Dashboard

### Descripción
Se añadieron dos nuevos widgets en el Dashboard:

**Producto estrella del mes**
- El producto con más unidades vendidas en el mes en curso.
- Muestra nombre, SKU, unidades vendidas e ingresos generados.
- Si no hay ventas en el mes, muestra un mensaje informativo.

**Mini gráfico de tendencia (7 días)**
- Barras proporcionales de las ventas de los últimos 7 días.
- La barra del día actual se resalta en morado sólido.
- El total del periodo aparece como badge junto al título.
- Implementado con divs CSS, sin JS ni librerías externas.
- Cada barra tiene tooltip con fecha y total al hacer hover.

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `Domain/Interfaces/IRepositories.cs` | Firmas `GetTopProductThisMonthAsync` y `GetSalesLast7DaysAsync` en `IDashboardRepository` |
| `Infrastructure/Repositories/SpecificRepositories.cs` | Implementación EF Core de los dos nuevos métodos |
| `Application/DTOs/DashboardDtos.cs` | `ProductoEstrellaDto`, `VentaDiariaDto` y nuevos campos en `DashboardDto` |
| `Application/Services/DashboardService.cs` | Pobla `ProductoEstrella` y `VentasUltimos7Dias` |
| `Web/Services/TinaStoreApiClient.cs` | `ProductoEstrellaDto`, `VentaDiariaDto` y `DashboardDto` actualizado |
| `Web/Components/Pages/Home.razor` | Fila 2 de KPIs con tarjeta estrella y mini gráfico |

### Resultado
✅ Build exitoso. Sin regresiones.

---

## [2.3.0] — 2026-06-19 — Fase E2: Detalle desplegable en Cuentas por Cobrar

### Tipo de cambio
Mejora UX

### Módulo afectado
Cuentas por Cobrar

### Descripción
Cada fila de cliente en Cuentas por Cobrar es ahora expandible. Al hacer clic
se despliega un panel con la lista de ventas pendientes de cobro del cliente,
cargadas bajo demanda desde `/api/invoices/cliente/{id}`.

**Comportamiento:**
- Chevron `›` / `⌄` indica si la fila está cerrada o abierta.
- La carga es lazy: la primera vez que se expande un cliente se llama a la API;
las siguientes expansiones usan caché local (sin llamada adicional).
- El panel muestra: N° venta, fecha, total, pagado, saldo pendiente y estado.
- Solo se muestran facturas con `Balance > 0` y estado distinto de Anulada.
- El pie del panel resume el total pendiente del cliente.
- El botón de WhatsApp sigue funcionando con `stopPropagation` para no activar el toggle.

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `Web/Services/TinaStoreApiClient.cs` | Método `GetVentasPorClienteAsync(int customerId)` |
| `Web/Components/Pages/CuentasPorCobrar/Index.razor` | Chevron, `ToggleDetalle`, `_expandidos`, `_detallesCache`, panel desplegable |

### Resultado
✅ Build exitoso. Sin regresiones.

---

## [2.2.0] — 2026-06-19 — Fase B: Venta libre

### Tipo de cambio
Nueva funcionalidad

### Módulo afectado
Ventas

### Descripción
Se añadió la modalidad de **venta libre**: permite registrar una venta con cliente
obligatorio pero con líneas de detalle de **descripción libre** (sin productos del
inventario). Útil para servicios, productos importados puntuales o artículos que
no se gestionan en el inventario habitual.

**Diferencias con una venta normal:**
| Aspecto | Venta normal | Venta libre |
|---------|-------------|-------------|
| Cliente | Obligatorio | Obligatorio |
| Líneas | Vinculadas a producto del inventario | Descripción libre (texto) |
| Descuento de stock | Sí | No |
| Movimiento de inventario | Sí (Exit) | No |
| CxC / pagos | Sí | Sí |
| Consecutivo / PDF | Sí | Sí |

**Reglas de negocio:**
- Cada línea libre debe tener descripción, cantidad ≥ 1 y precio ≥ 0.
- El pago inicial, descuento e impuesto funcionan igual que en venta normal.
- Las líneas libres no generan `InventoryMovement`.
- Al anular una venta libre, no se revierte stock (solo se cambia estado y CxC).

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `Domain/Entities/InvoiceDetail.cs` | `ProductId int → int?`, `Product → Product?` |
| `Application/DTOs/InvoiceDtos.cs` | `CreateInvoiceDetailDto` con `ProductId int?` y `FreeDescription`; `InvoiceDetailDto` con `ProductId int?` |
| `Application/DTOs/ReportDtos.cs` | `TopProductoDto.ProductId int → int?` |
| `Application/Validators/InvoiceValidators.cs` | Validación condicional: `ProductId > 0` solo si tiene valor; `FreeDescription` obligatoria si `ProductId` es null |
| `Application/Services/InvoiceService.cs` | Guard de stock/movimiento solo para líneas con `ProductId`; `CancelAsync` omite reversión de stock en líneas libres |
| `Web/Services/TinaStoreApiClient.cs` | `CreateDetalleFacturaDto` con `ProductId int?` y `FreeDescription` |
| `Web/Components/Pages/Facturas/VentaLibre.razor` | Nueva página `/ventas/libre` |
| `Web/Components/Layout/NavMenu.razor` | Enlace "Venta libre" en menú comercial |
| `Infrastructure/Migrations/AllowFreeLineInInvoice` | `ProductId` nullable en `InvoiceDetails` |

### Resultado
✅ Build exitoso. Migración aplicada. Sin regresiones en ventas normales ni anulaciones.

---

## [2.1.0] — 2026-06-19 — Fase C2: Edición masiva de productos

### Tipo de cambio
Nueva funcionalidad (bulk edit)

### Módulo afectado
Productos

### Descripción
Se implementó un flujo de edición masiva que permite actualizar costo de compra,
precio de venta y stock de varios productos simultáneamente en un único lote.

**Flujo UX de dos pasos:**
1. **Paso 1 — Editar:** tabla con buscador en tiempo real; celdas editables para
   costo, precio y stock; resaltado automático de filas modificadas; validación
   inmediata (valores negativos marcados como inválidos).
2. **Paso 2 — Confirmar:** resumen con valores anteriores y nuevos en modo diff;
   advertencia de que los cambios de stock generan movimientos de inventario.

**Reglas de negocio:**
- Los cambios de stock registran automáticamente un `InventoryMovement` de tipo
  `Adjustment` con los valores antes/después.
- Si el costo, precio o stock no cambia respecto al original, ese campo se envía
  como `null` y el backend no lo modifica.
- Validaciones: costo ≥ 0, precio ≥ 0, stock ≥ 0.
- Respuesta por fila con `Ok/Error` para mostrar problemas parciales sin abortar
  el lote completo.

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `Application/DTOs/ProductDtos.cs` | Nuevos records `BulkUpdateItemDto`, `BulkUpdateItemResultDto`, `BulkUpdateResultDto` |
| `Application/Interfaces/IServices.cs` | Firma `Task<BulkUpdateResultDto> BulkUpdateAsync(...)` en `IProductService` |
| `Application/Services/ProductService.cs` | Implementación de `BulkUpdateAsync` con validaciones y `InventoryMovement` |
| `Api/Controllers/ProductsController.cs` | Endpoint `PUT /api/products/bulk` |
| `Web/Services/TinaStoreApiClient.cs` | DTOs de bulk y método `BulkUpdateProductosAsync` |
| `Web/Components/Pages/Productos/Index.razor` | Botón "Edición masiva", modal 2 pasos, clase `BulkFila`, métodos de estado |
| `Web/wwwroot/app.css` | Clases `btn-outline-purple`, `bulk-row-modified`, `bulk-table`, `badge-estado-pendiente` |

### Resultado
✅ Build exitoso. Sin regresiones en módulos anteriores.

---

## [2.0.0] — 2026-06-19 — Fase C1: Tarjetas resumen en módulo Productos

### Tipo de cambio
Nueva funcionalidad visual

### Módulo afectado
Productos

### Descripción
Se agregaron dos tarjetas KPI en la parte superior del módulo Productos, visibles
inmediatamente al cargar la página. Los valores se calculan directamente desde la lista
de productos ya cargada en memoria, sin llamadas adicionales a la API.

| Tarjeta | Valor | Definición |
|---------|-------|-----------|
| **Referencias disponibles** | Número entero | Productos activos con `CurrentStock > 0` |
| **Costo total del inventario** | Moneda formateada | `∑ PurchasePrice × CurrentStock` para productos activos |

Las tarjetas usan las clases `kpi-card` y `kpi-icon` del tema rosado/kawaii existente.
Se actualizan automáticamente cada vez que la lista de productos se recarga (al crear,
editar o eliminar un producto).

### Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `src/TinaStore.Web/Components/Pages/Productos/Index.razor` | Bloque de dos tarjetas KPI antes de los filtros; propiedades calculadas `_refDisponibles` y `_costoInventario` en `@code` |

### Decisiones tomadas

- **Las tarjetas muestran totales generales** (no filtrados): refleja el estado real del
  inventario independientemente del filtro activo.
- **Productos con costo 0** aportan 0 al total de costo, sin advertencia — el diseño
  es simple y claro.
- **Sin nueva llamada a la API**: los datos provienen de `_productos` ya cargado en
  `OnInitializedAsync`.

### Impacto en datos existentes
Ninguno. Solo lectura de datos ya disponibles en memoria.

### Migración requerida
No

### Pruebas realizadas
- ✅ Compilación correcta (0 errores)
- ✅ Tarjeta "Referencias disponibles" muestra conteo correcto de activos con stock > 0
- ✅ Tarjeta "Costo total" aplica fórmula `PurchasePrice × CurrentStock`
- ✅ Tarjetas visibles en escritorio y móvil (Bootstrap `col-sm-6`)
- ✅ Al crear/editar/eliminar un producto, los valores se recalculan automáticamente
- ✅ Estilo coherente con el tema kawaii rosado del dashboard

### Resultado
✅ Exitoso

---

## [2.0.0] — 2026-06-19 — Fase E1: Detalle desplegable en módulo Ventas

### Tipo de cambio
Nueva funcionalidad

### Módulo afectado
Ventas

### Descripción
Al hacer clic en cualquier fila del listado de Ventas, se despliega un panel que muestra
los productos vendidos, totales desglosados y los pagos registrados de esa venta. La
carga es bajo demanda (lazy loading): solo se consulta la API la primera vez que se
expande una fila; las siguientes expansiones usan caché local del componente.

### Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `src/TinaStore.Web/Services/TinaStoreApiClient.cs` | Nuevos DTOs `DetalleLineaVentaDto`, `PagoRegistradoDto`, `VentaDetalleDto`; método `GetVentaDetalleAsync(int id)` |
| `src/TinaStore.Web/Components/Pages/Facturas/Index.razor` | Columna chevron, fila expandible, variables y método `ToggleDetalle`, limpieza de caché en `Cargar()` |
| `src/TinaStore.Web/wwwroot/app.css` | Clases `.detalle-venta-panel`, `.pago-chip`, `tr.detalle-expandido`, animación `slideDown` |

### Impacto en datos existentes
Ninguno. Solo lectura del endpoint `GET /api/invoices/{id}` ya existente.

### Migración requerida
No

### Pruebas realizadas
- ✅ Compilación correcta (0 errores)
- ✅ Clic en fila expande panel; segundo clic lo colapsa
- ✅ Botones de acción no activan el toggle (`stopPropagation`)
- ✅ Panel muestra productos, totales y pagos
- ✅ Caché local evita llamadas repetidas a la API
- ✅ Al recargar la lista el caché se limpia

### Resultado
✅ Exitoso

---

## [2.0.0] — 2026-06-19 — Fase A: Cambio de lenguaje Facturas → Ventas

### Tipo de cambio
Ajuste visual / cambio de lenguaje (sin impacto en base de datos ni en lógica interna)

### Módulo afectado
Ventas (anteriormente llamado Facturas en la UI)

### Descripción
Se cambió el lenguaje visible de la aplicación para que el módulo deje de llamarse
"Facturas" y pase a llamarse "Ventas". Los nombres internos técnicos (`Invoice`,
`InvoiceService`, `InvoicesController`, tabla `Invoices` en SQLite) se mantienen sin
cambios para preservar estabilidad y evitar migraciones de base de datos innecesarias.

### Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `src/TinaStore.Web/Components/Layout/NavMenu.razor` | Enlace `/facturas` → `/ventas`; texto "Facturas" → "Ventas" |
| `src/TinaStore.Web/Components/Pages/Facturas/Index.razor` | `@page "/ventas"`, título, botón "Nueva venta", textos modales |
| `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor` | `@page "/ventas/nueva"`, título, botón "Registrar Venta", mensajes y navegación |
| `src/TinaStore.Web/Components/Pages/Home.razor` | "Últimas Facturas" → "Últimas Ventas" en dashboard |
| `src/TinaStore.Web/Components/Pages/CuentasPorCobrar/Index.razor` | "Facturas pend." → "Ventas pend.", "Última factura" → "Última venta" |

### Impacto en datos existentes
Ninguno. Solo se modificaron textos visibles en la UI.

### Migración requerida
No

### Pruebas realizadas
- ✅ Compilación correcta (0 errores)
- ✅ Menú muestra "Ventas" con enlace `/ventas`
- ✅ Rutas `/ventas` y `/ventas/nueva` funcionales
- ✅ Dashboard muestra "Últimas Ventas"
- ✅ Cuentas por cobrar muestra "Ventas pend." y "Última venta"

### Pendientes conocidos
- El PDF generado sigue diciendo "Factura de Venta" (decisión intencional: es documento legal).
- Fases B, C, D, E pendientes de implementación.

---

## [1.6.0]

### Añadido

#### [D1] Filtro de categorías en la pantalla "Nueva Factura"
- **Módulo**: Facturas
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor`
- **Detalle**: Selector de categoría junto al campo de búsqueda. Permite filtrar la lista de productos por categoría sin necesidad de escribir. Funciona combinado con búsqueda por nombre/SKU.

#### [D2] Indicador "Stock bajo" en el buscador de productos
- **Módulo**: Facturas
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor`
- **Detalle**: Badge amarillo "Stock bajo" visible en cada fila del dropdown de productos cuando `IsLowStock == true`. Botón rápido `+` para abrir el modal de stock cuando el producto tiene `CurrentStock == 0`.

#### [D3] Modal de ajuste rápido de stock desde la factura
- **Módulo**: Facturas / Productos
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor`, `src/TinaStore.Web/Services/TinaStoreApiClient.cs`
- **Detalle**: Modal pequeño que permite ingresar unidades al stock de un producto sin salir de la pantalla de factura. Muestra el nombre del producto y el stock actual antes de confirmar. Actualiza la lista local en memoria al cerrar.

#### [D4] Endpoint `POST /api/products/{id}/ajuste-stock`
- **Módulo**: Productos (API)
- **Archivos**: `src/TinaStore.Api/Controllers/ProductsController.cs`
- **Detalle**: Nuevo endpoint dedicado para entradas rápidas de stock. Recibe `{ Cantidad, Notas }`, registra un `InventoryMovement` de tipo `Entry` con trazabilidad completa y devuelve el `ProductDto` actualizado.

#### [D5] Servicio `AjustarStockAsync` en la capa Application
- **Módulo**: Productos (Application)
- **Archivos**: `src/TinaStore.Application/Services/ProductService.cs`, `src/TinaStore.Application/Interfaces/IServices.cs`, `src/TinaStore.Application/DTOs/ProductDtos.cs`
- **Detalle**: `IProductService` extendido con `AjustarStockAsync(int id, AjusteStockDto dto)`. La implementación incrementa `CurrentStock`, persiste un movimiento de inventario con stock antes/después y devuelve el producto actualizado.

---

## [1.5.0] — 2026-06-19 — Fase C: PDF y facturas

### Corregido / Añadido

#### [C8] Indicador de carga al descargar PDF
- **Módulo**: Facturas
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`
- **Detalle**: `HashSet<int> _pdfDescargando` por ID de factura. El botón muestra spinner mientras descarga, queda deshabilitado para evitar dobles clics, y vuelve a la normalidad al terminar. Si el PDF se genera correctamente muestra confirmación; si falla muestra error claro.

#### [C9] Estado de factura en español en el PDF
- **Módulo**: PDF / Facturas
- **Archivos**: `src/TinaStore.Infrastructure/Services/PdfService.cs`
- **Detalle**: Switch expression que mapea `InvoiceStatus` a etiquetas en español: PAGADA, ANULADA, PARCIAL, PENDIENTE. Reemplaza `invoice.Status.ToString().ToUpper()` que producía PAID/CANCELLED/etc.

#### [C10] Etiqueta "N° Factura" visible en el PDF
- **Módulo**: PDF / Facturas
- **Archivos**: `src/TinaStore.Infrastructure/Services/PdfService.cs`
- **Detalle**: Encabezado del PDF con texto "N° Factura" en gris pequeño sobre el número, para distinguirlo claramente del nombre de la tienda y la fecha.

#### [C11] Confirmar anulación de factura con Enter
- **Módulo**: Facturas
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`
- **Detalle**: Handler `HandleAnulacionKeyDown` con `@onkeydown` en el textarea. Si `Key == "Enter"` y no es `ShiftKey`, ejecuta `ConfirmarAnulacion()`. Shift+Enter permite salto de línea. Textarea usa `@bind:event="oninput"` para que el valor esté actualizado al presionar Enter.

#### [C12] Validación de descuento y valores negativos en nueva factura
- **Módulo**: Facturas / Nueva factura
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor`
- **Detalle**: `RecalcularTotales()` recorta automáticamente el descuento si es negativo o supera el subtotal, mostrando advertencia inline con Bootstrap `is-invalid`. `CrearFactura()` valida 8 reglas antes de llamar a la API: cliente seleccionado, al menos un producto, cantidad > 0, precio >= 0, descuento dentro del rango, impuesto >= 0, pago inicial >= 0 y pago inicial <= total.

---

## [1.4.0] — 2026-06-19 — Fase B: Filtros y ordenamientos

### Añadido

#### [B6] Ordenamiento interactivo en módulo Facturas
- **Módulo**: Facturas
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`
- **Detalle**: Cabeceras de tabla clickeables con iconos de estado (↓ mayor a menor, ↑ menor a mayor, ↕ sin orden) para las columnas: N° Factura, Total, Pagado, Saldo y Fecha. Al hacer clic en una columna ya activa se invierte la dirección. Al hacer clic en una columna diferente se activa descendente por defecto. "Limpiar filtros" también resetea el orden a Fecha descendente.

#### [B7] Selector de orden ampliado en módulo Productos
- **Módulo**: Productos
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Detalle**: El selector "Orden por stock" fue reemplazado por "Ordenar por" con 10 opciones agrupadas: Stock (↑↓), Precio venta (↑↓), Costo (↑↓), % Ganancia (↑↓) y Nombre (A→Z / Z→A). El campo interno `_ordenStock` fue renombrado a `_orden` y el switch en `Filtrar()` fue extendido con todos los nuevos casos.

---

## [1.3.0] — 2026-06-19 — Fase A: Validaciones, errores en modal y responsividad

### Corregido / Añadido

#### [A1] Validación de correo en frontend
- **Módulo**: Clientes
- **Causa raíz**: El campo Email no tenía validación visual en Blazor; los errores de la API se mostraban fuera del modal.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Solución**: Regex `^[^@\s]+@[^@\s]+\.[^@\s]+$` validado antes de llamar a la API. El modal no se cierra si el correo es inválido.

#### [A2] Validación de teléfono en frontend y backend
- **Módulo**: Clientes
- **Causa raíz**: Backend solo validaba longitud máxima; no había regex de formato colombiano.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`, `src/TinaStore.Application/Validators/CustomerValidators.cs`
- **Solución**: Regex `^\+?[\d\s\-]{7,20}$` en frontend y en CreateCustomerValidator + UpdateCustomerValidator. Mensaje en español consistente.

#### [A3] Errores dentro del modal de cliente
- **Módulo**: Clientes
- **Causa raíz**: `_mensaje` se renderizaba a nivel de página, detrás del backdrop del modal.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Solución**: Nuevo `_erroresModal` (List<string>) + campos `_errNombre`, `_errEmail`, `_errTelefono`. Resumen de errores en `alert-danger` dentro del `modal-body`. Clases Bootstrap `is-invalid` + `invalid-feedback` por campo.

#### [A4] Modales responsivos con scroll interno
- **Módulo**: Clientes, Facturas, Productos
- **Causa raíz**: Modales sin `modal-dialog-scrollable`; en pantallas pequeñas el contenido se cortaba.
- **Archivos**: `Clientes/Index.razor`, `Facturas/Index.razor`, `Productos/Index.razor`
- **Solución**: `modal-dialog-scrollable` añadido a todos los modales afectados. Columnas responsive `col-12 col-sm-6 col-md-*`.

#### [B5-parcial] Filtros de estado y correo en módulo Clientes
- **Módulo**: Clientes
- **Causa raíz**: Solo existía filtro de texto libre y fechas. Sin filtro por estado ni correo.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Solución**: Nuevo `<select>` de estado (Todos/Activo/Inactivo/Sin correo) y campo de búsqueda por correo. `Filtrar()` ampliado con ambas condiciones.

---

## [1.2.0] — 2026-06-18 — Fases A2 + B1 + B3: Datos, visualización y nuevas funcionalidades

### Corregido

#### [B05] Estado de inventario siempre mostraba "Activo"
- **Módulo**: Productos
- **Causa**: Solo se usaba `IsActive` para el badge. No se diferenciaban los estados de inventario.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Solución**: Lógica de estado diferenciado: **Inactivo** (estado admin), **Agotado** (stock=0), **Bajo stock** (stock ≤ mínimo), **Activo** (stock > mínimo).

#### [B06] Categoría vacía en Productos (conteo siempre 0)
- **Módulo**: Categorías
- **Causa**: `CategoryRepository.GetByIdAsync` no incluía `Products`; conteo no filtraba `IsActive`.
- **Archivos**: `src/TinaStore.Infrastructure/Repositories/SpecificRepositories.cs`, `src/TinaStore.Application/Services/CategoryService.cs`
- **Solución**: `GetByIdAsync` sobrescrito con `Include(c => c.Products)`. Conteo ahora filtra `IsActive && !IsDeleted`.

#### [B07] Fórmula de porcentaje de ganancia incorrecta
- **Módulo**: Productos
- **Causa**: Usaba `SalePrice` como divisor en vez de `PurchasePrice`.
- **Archivos**: `src/TinaStore.Domain/Entities/Product.cs`
- **Solución**: `(SalePrice - PurchasePrice) / PurchasePrice × 100`. Guard para `PurchasePrice == 0`.

#### [B08b] Botones en modales y tarjetas podían quedar sin fondo visible
- **Módulo**: Global (UI)
- **Archivos**: `src/TinaStore.Web/wwwroot/app.css`
- **Solución**: Reglas `!important` para `.btn.bg-purple` en `.card`, `.modal-footer` y `.modal-content`. Bloque `@media` sin cerrar corregido.

#### [B15] Columna Estado en Proveedores
- **Módulo**: Proveedores
- **Causa**: No se requiere gestión de estado en proveedores en esta versión.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Proveedores/Index.razor`
- **Solución**: Columna Estado ocultada en UI. Columna `IsActive` conservada en BD para uso futuro.

### Agregado

#### [F10] Logo dinámico en sidebar
- **Módulo**: Layout / Configuración
- **Archivos**: `src/TinaStore.Web/Components/Layout/MainLayout.razor`, `src/TinaStore.Web/wwwroot/app.css`
- **Descripción**: El sidebar carga el logo desde `/api/settings` al iniciar. Muestra emoji 🛍️ si no hay logo configurado.

#### [F11] Fecha de última compra y estado comercial de clientes
- **Módulo**: Clientes
- **Archivos**: `src/TinaStore.Domain/Interfaces/IRepositories.cs`, `src/TinaStore.Infrastructure/Repositories/SpecificRepositories.cs`, `src/TinaStore.Application/DTOs/CustomerDtos.cs`, `src/TinaStore.Application/Services/CustomerService.cs`, `src/TinaStore.Web/Services/TinaStoreApiClient.cs`, `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Descripción**: `LastPurchaseDate` calculado desde facturas no anuladas. `CommercialStatus` automático: "Activo" si compró en últimos 6 meses, "Inactivo" si hace más, "Sin compras" si nunca. Columna visible en tabla de clientes.

#### [F14] Porcentaje de ganancia en Productos
- **Módulo**: Productos
- **Archivos**: `src/TinaStore.Web/Services/TinaStoreApiClient.cs`, `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Descripción**: `ProfitMargin` agregado a `ProductoDto`. Visible en listado y en modal de creación/edición con cálculo en tiempo real.

#### [F16] Modal editable de recordatorio WhatsApp
- **Módulo**: Clientes
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Descripción**: Modal con mensaje prellenado editable antes de abrir WhatsApp. Normalización de número colombiano (+57). Plantilla configurable desde Configuración.

---

## [1.1.0] — 2026-06-18 — Fase A1: Corrección de bugs críticos

### Corregido

#### [B08] Botones de acción principales invisibles
- **Causa**: Las clases `.bg-purple` y `.text-purple` no existían en Bootstrap 5 ni en `app.css`. Los botones usaban `btn bg-purple text-white` y quedaban sin fondo visible.
- **Archivos**: `src/TinaStore.Web/wwwroot/app.css`
- **Solución**: Agregadas clases `.bg-purple`, `.text-purple`, `.badge.bg-purple` con color #7C3AED, hover, focus, active y disabled.

#### [B04a] Badge de estado de factura sin estilo visual
- **Causa**: `StatusName` devolvía el nombre del enum en inglés (`Pending`, `Paid`). Las clases CSS usaban español (`badge-estado-pendiente`, `badge-estado-pagada`).
- **Archivos**: `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`
- **Solución**: Agregado método `StatusEnEspanol()` en ambos servicios. Retorna: Pendiente, Pagada, Parcial, Anulada.

#### [B04b] Campo Abono siempre muestra $0 en listado de Facturas
- **Causa**: `InvoiceSummaryDto` no incluía `AmountPaid`. El cliente Web lo deserializaba como 0.
- **Archivos**: `src/TinaStore.Application/DTOs/InvoiceDtos.cs`, `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`
- **Solución**: Campo `AmountPaid` agregado con valor por defecto 0. `ToSummaryDto` actualizado en ambos servicios.

#### [B01] Congelamiento de la aplicación al cargar imagen de producto
- **Causa**: `ReadAsync(buffer)` en Blazor Server no garantiza lectura completa del stream en una sola llamada.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Solución**: Reemplazado por `MemoryStream` + `await stream.CopyToAsync(ms)`. Validación de extensión y límite 2 MB.

#### [B02] Dashboard no conecta con la API — error silencioso
- **Causa**: `UseStaticFiles()` duplicado y `UseHttpsRedirection()` en posición incorrecta.
- **Archivos**: `src/TinaStore.Api/Program.cs`, `src/TinaStore.Web/Services/TinaStoreApiClient.cs`, `src/TinaStore.Web/Components/Pages/Home.razor`
- **Solución**: Pipeline HTTP corregido. Nuevo método diagnóstico en `TinaStoreApiClient`. Dashboard con mensaje descriptivo y botón "Reintentar".

---

## [0.3.0] — 2026-06-18

### Fase 2 — Eliminación de InternalCode, mantener solo SKU

#### MEJORA-01 — Eliminación completa de `InternalCode`
- **Archivos**: múltiples (ver `TECHNICAL_DECISIONS.md`)
- **Migración**: `20260618140145_RemoveInternalCode.cs` — copia `InternalCode → Sku` y elimina columna.

---

## [0.1.0] — 2026-06-17

### Primera versión funcional
- Módulos: Dashboard, Productos, Categorías, Clientes, Proveedores, Facturas, Egresos, Cuentas por Cobrar, Reportes, Configuración, Usuarios.
- Autenticación JWT con usuario y contraseña.
- Base de datos SQLite con migraciones EF Core.
- Arquitectura Clean Architecture: Domain / Application / Infrastructure / Api / Web.


### Corregido

#### [B08] Botones de acción principales invisibles
- **Causa**: Las clases `.bg-purple` y `.text-purple` no existían en Bootstrap 5 ni en `app.css`. Los botones usaban `btn bg-purple text-white` y quedaban sin fondo visible.
- **Archivos**: `src/TinaStore.Web/wwwroot/app.css`
- **Solución**: Agregadas clases `.bg-purple`, `.text-purple`, `.badge.bg-purple` con color #7C3AED, hover, focus, active y disabled.

#### [B04a] Badge de estado de factura sin estilo visual
- **Causa**: `StatusName` devolvía el nombre del enum en inglés (`Pending`, `Paid`). Las clases CSS usaban español (`badge-estado-pendiente`, `badge-estado-pagada`).
- **Archivos**: `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`
- **Solución**: Agregado método `StatusEnEspanol()` en ambos servicios. Retorna: Pendiente, Pagada, Parcial, Anulada.

#### [B04b] Campo Abono siempre muestra $0 en listado de Facturas
- **Causa**: `InvoiceSummaryDto` no incluía `AmountPaid`. El cliente Web lo deserializaba como 0.
- **Archivos**: `src/TinaStore.Application/DTOs/InvoiceDtos.cs`, `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`
- **Solución**: Campo `AmountPaid` agregado con valor por defecto 0. `ToSummaryDto` actualizado en ambos servicios.

#### [B01] Congelamiento de la aplicación al cargar imagen de producto
- **Causa**: `ReadAsync(buffer)` en Blazor Server no garantiza lectura completa del stream en una sola llamada. Para archivos grandes, la lectura era parcial y el preview quedaba corrupto o la operación se bloqueaba.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Solución**: Reemplazado por `MemoryStream` + `await stream.CopyToAsync(ms)`. El stream de upload se abre por separado del stream de preview. Agregada validación de extensión (.jpg/.jpeg/.png/.webp) y límite de 2 MB con mensajes de error visibles.

#### [B02] Dashboard no conecta con la API — error silencioso
- **Causa 1**: `Program.cs` de la API tenía `UseStaticFiles()` duplicado y `UseHttpsRedirection()` antes de `UseAuthentication()`, que en algunos entornos rechazaba peticiones HTTP válidas.
- **Causa 2**: `GetSafeAsync` silenciaba todas las excepciones. El Dashboard no podía mostrar la causa real del error.
- **Archivos**: `src/TinaStore.Api/Program.cs`, `src/TinaStore.Web/Services/TinaStoreApiClient.cs`, `src/TinaStore.Web/Components/Pages/Home.razor`
- **Solución**: Pipeline HTTP corregido (eliminado duplicado y `UseHttpsRedirection`). Nuevo método `GetDashboardConDiagnosticoAsync()` que retorna `(Data, Error)`. Dashboard muestra mensaje descriptivo con URL de API y botón "Reintentar".

---

## [0.3.0] — 2026-06-18

### Fase 2 — Eliminar InternalCode, mantener solo SKU

#### MEJORA-01 — Eliminación completa de `InternalCode`
- **Módulos afectados**: Productos, Reportes, Excel (exportación, plantilla, importación), API, Web
- **Archivos modificados**:
  - `src/TinaStore.Domain/Entities/Product.cs`
  - `src/TinaStore.Application/DTOs/ProductDtos.cs`
  - `src/TinaStore.Application/Validators/ProductValidators.cs`
  - `src/TinaStore.Application/Services/ProductService.cs`
  - `src/TinaStore.Application/Services/ReportService.cs`
  - `src/TinaStore.Infrastructure/Repositories/SpecificRepositories.cs`
  - `src/TinaStore.Infrastructure/Services/ExcelService.cs`
  - `src/TinaStore.Infrastructure/Migrations/20260618140145_RemoveInternalCode.cs` (nueva)
  - `src/TinaStore.Web/Services/TinaStoreApiClient.cs`
  - `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
  - `tests/TinaStore.Tests.Unit/InvoiceServiceTests.cs`
- **Migración de datos**: La migración EF Core `RemoveInternalCode` primero copia `InternalCode → Sku` donde `Sku` estaba vacío, y luego elimina la columna. El producto existente con `InternalCode='MEDIA123'` queda con `Sku='MEDIA123'`.
- **Pruebas**: 30/30 pruebas unitarias pasadas.

---

## [0.2.0] — 2026-06-18

### Correcciones críticas (Fase 1)

#### BUG-01 — Todas las peticiones autenticadas fallaban con 401 Unauthorized
- **Módulo**: Global (todos los módulos que consumen la API)
- **Archivos modificados**: `src/TinaStore.Web/appsettings.Development.json`
- **Cambio**: `ApiBaseUrl` corregido de `http://localhost:5172` a `https://localhost:7073`
- **Resultado**: El dashboard, clientes, productos, facturas y demás módulos cargan correctamente con sesión iniciada.

#### BUG-03 — Al editar un producto, Categoría y Proveedor aparecían vacíos
- **Módulo**: Productos
- **Archivos modificados**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
- **Cambio**: `AbrirModal` convertido de `void` a `async Task`. Ahora al editar se consulta `GET /api/products/{id}` para obtener el DTO completo con `CategoryId` y `SupplierId` reales.
- **Resultado**: El formulario de edición muestra correctamente todos los datos del producto, incluyendo categoría y proveedor seleccionados.

---

## [0.1.0] — 2026-06-17

### Primera versión funcional
- Módulos: Dashboard, Productos, Categorías, Clientes, Proveedores, Facturas, Egresos, Cuentas por Cobrar, Reportes, Configuración, Usuarios.
- Autenticación JWT con usuario y contraseña.
- Base de datos SQLite con migraciones EF Core.
- Exportación de productos a Excel (QuestPDF / ClosedXML).
- Generación de PDF de facturas (QuestPDF).
- Arquitectura Clean Architecture: Domain / Application / Infrastructure / Api / Web.

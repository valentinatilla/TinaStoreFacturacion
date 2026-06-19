# CHANGELOG — Tina Store

Historial de versiones y cambios relevantes del proyecto.
Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

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

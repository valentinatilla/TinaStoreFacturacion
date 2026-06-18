# CHANGELOG — Tina Store

Historial de versiones y cambios relevantes del proyecto.
Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

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

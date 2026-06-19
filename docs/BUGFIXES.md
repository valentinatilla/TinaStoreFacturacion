# BUGFIXES — Tina Store

Registro detallado de todos los bugs corregidos en el proyecto.

---

## BUG-06 — Botones de acción principales invisibles *(v1.1.0 — 2025-06-18)*

- **Módulo**: Global / UI
- **Causa**: `.bg-purple` y `.text-purple` no existían en Bootstrap 5 ni en `app.css`. Los botones `btn bg-purple text-white` quedaban transparentes.
- **Solución**: Agregadas clases en `app.css` con color #7C3AED, hover, focus, active y disabled.
- **Archivos**: `src/TinaStore.Web/wwwroot/app.css`

---

## BUG-07 — Estado de factura sin estilo visual *(v1.1.0 — 2025-06-18)*

- **Módulo**: Facturas, Dashboard
- **Causa**: `InvoiceStatus.ToString()` retorna inglés (`Pending`). CSS usa español (`badge-estado-pendiente`). Mismatch → sin estilo.
- **Solución**: Método `StatusEnEspanol()` en `InvoiceService` y `DashboardService`.
- **Archivos**: `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`

---

## BUG-08 — Campo Abono siempre $0 en listado de Facturas *(v1.1.0 — 2025-06-18)*

- **Módulo**: Facturas
- **Causa**: `InvoiceSummaryDto` no incluía `AmountPaid`. Deserialización del cliente Web dejaba el campo en 0.
- **Solución**: `AmountPaid` agregado a `InvoiceSummaryDto` (con default 0). `ToSummaryDto` actualizado.
- **Archivos**: `src/TinaStore.Application/DTOs/InvoiceDtos.cs`, `src/TinaStore.Application/Services/InvoiceService.cs`, `src/TinaStore.Application/Services/DashboardService.cs`

---

## BUG-09 — Congelamiento al cargar imagen de producto *(v1.1.0 — 2025-06-18)*

- **Módulo**: Productos
- **Causa**: `Stream.ReadAsync(buffer)` en Blazor Server no garantiza lectura completa. Lectura parcial → preview corrupto y bloqueo.
- **Solución**: `MemoryStream + CopyToAsync`. Streams separados para preview y upload. Validación de extensión y tamaño.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-10 — Dashboard no conecta con la API *(v1.1.0 — 2025-06-18)*

- **Módulo**: Dashboard, API
- **Causa 1**: `UseStaticFiles()` duplicado y `UseHttpsRedirection()` en pipeline de la API bloqueaban peticiones HTTP válidas.
- **Causa 2**: `GetSafeAsync` silenciaba errores → Dashboard no mostraba la causa real.
- **Solución**: Pipeline corregido en `Program.cs`. Nuevo `GetDashboardConDiagnosticoAsync()`. Dashboard con mensaje descriptivo y botón Reintentar.
- **Archivos**: `src/TinaStore.Api/Program.cs`, `src/TinaStore.Web/Services/TinaStoreApiClient.cs`, `src/TinaStore.Web/Components/Pages/Home.razor`

---

## BUG-01 — Peticiones autenticadas fallan con 401 Unauthorized

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-18 |
| **Versión** | 0.2.0 |
| **Módulo** | Global |
| **Prioridad** | 🔴 CRÍTICA |
| **Estado** | ✅ Corregido |

### Problema
Después de iniciar sesión correctamente, todas las peticiones a la API que requerían autenticación retornaban `401 Unauthorized`. Los módulos cargaban vacíos (clientes, productos, facturas, etc.).

### Causa identificada
`ApiBaseUrl` en `appsettings.Development.json` de `TinaStore.Web` apuntaba a `http://localhost:5172`.
La API tenía habilitada la redirección HTTPS, por lo que respondía con un `307 Temporary Redirect` hacia `https://localhost:7073`.
El `HttpClient` de .NET sigue automáticamente los redirects **pero elimina el header `Authorization: Bearer ...`** por seguridad (comportamiento estándar RFC). Resultado: la petición llegaba a la API sin token, y la API la rechazaba con 401.

### Archivos modificados
- `src/TinaStore.Web/appsettings.Development.json`

### Solución aplicada
```json
// Antes
"ApiBaseUrl": "http://localhost:5172"

// Después
"ApiBaseUrl": "https://localhost:7073"
```

### Riesgo del cambio
Bajo. Solo afecta la URL base en desarrollo. La API ya escuchaba en HTTPS.

### Cómo se probó
- Compilación exitosa.
- Al iniciar sesión, el dashboard carga con datos reales.
- La lista de clientes ya aparece al crear facturas.
- La lista de productos ya carga en el módulo de productos.

### Resultado
✅ Corrección validada. Todos los módulos vuelven a funcionar con sesión autenticada.

### Tareas pendientes
- Para producción: configurar `ApiBaseUrl` mediante variable de entorno segura (no en código fuente).

---

## BUG-03 — Al editar un producto, Categoría y Proveedor aparecen vacíos

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-18 |
| **Versión** | 0.2.0 |
| **Módulo** | Productos |
| **Prioridad** | 🔴 CRÍTICA |
| **Estado** | ✅ Corregido |

### Problema
Al hacer clic en el ícono de lápiz (editar) en la lista de productos, el formulario abría con los campos de Categoría y Proveedor en "Seleccione...", aunque el producto ya tenía esos datos guardados.

### Causa identificada
El listado de productos llama a `GET /api/products` que retorna `ProductSummaryDto[]`. Este DTO de resumen **no incluye `CategoryId` ni `SupplierId`** (solo nombre de categoría como texto).

Al abrir el modal de edición, el código usaba directamente el objeto del listado (`ProductoDto` del cliente Web) para poblar el formulario. Como `CategoryId` llegaba como `0` y `SupplierId` como `null`, los `<select>` no encontraban ninguna opción coincidente y mostraban vacíos.

### Archivos modificados
- `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

### Solución aplicada
`AbrirModal` convertido de `void` a `async Task`. Al recibir un producto para editar, ahora consulta el endpoint individual:
```
GET /api/products/{id}  →  ProductDto (completo, con CategoryId y SupplierId)
```
Este DTO sí contiene todos los campos necesarios para poblar correctamente el formulario.

### Riesgo del cambio
Bajo. Agrega una llamada HTTP adicional solo al abrir el modal de edición. La creación de nuevos productos no se ve afectada.

### Cómo se probó
- Compilación exitosa.
- Abrir modal de edición de un producto → categoría y proveedor se muestran correctamente seleccionados.
- Editar solo el precio → categoría y proveedor no se modifican al guardar.
- Editar solo la categoría → el proveedor no se pierde.

### Resultado
✅ Corrección validada. El formulario de edición carga y conserva todos los datos del producto.

### Tareas pendientes
- Ninguna relacionada con este bug.

---

## BUG-02 — Campo Cliente aparece vacío en Facturas

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-18 |
| **Versión** | 0.2.0 |
| **Módulo** | Facturas |
| **Prioridad** | 🔴 CRÍTICA |
| **Estado** | ✅ Corregido (resuelto como consecuencia de BUG-01) |

### Problema
Al crear una nueva factura, el selector de clientes aparecía vacío (sin opciones).

### Causa identificada
Consecuencia directa de BUG-01. La petición `GET /api/customers` también fallaba con 401 por el redirect HTTP→HTTPS, devolviendo lista vacía.

### Solución aplicada
Resuelta automáticamente al corregir BUG-01 (cambio de `ApiBaseUrl` a HTTPS).

### Resultado
✅ Los clientes aparecen correctamente en el selector al crear una factura.

---

## BUG-09 — Estado de inventario siempre "Activo" en Productos *(v1.2.0 — 2026-06-18)*

- **Módulo**: Productos
- **Causa**: Solo se usaba `p.IsActive` para el badge. No existía diferenciación entre estado administrativo y estado de inventario.
- **Solución**: Lógica condicional: **Inactivo** (`!IsActive`), **Agotado** (`stock=0`), **Bajo stock** (`stock ≤ mínimo`), **Activo** (`stock > mínimo`). Badges con clases CSS existentes.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-10 — Conteo de productos por categoría siempre 0 *(v1.2.0 — 2026-06-18)*

- **Módulo**: Categorías
- **Causa**: `CategoryRepository.GetByIdAsync` usaba `FindAsync` sin `Include(Products)`. Además el conteo no filtraba `IsActive`.
- **Solución**: `GetByIdAsync` sobrescrito en `CategoryRepository` con `Include(c => c.Products)`. Conteo actualizado a `Count(p => !p.IsDeleted && p.IsActive)`.
- **Archivos**: `src/TinaStore.Infrastructure/Repositories/SpecificRepositories.cs`, `src/TinaStore.Application/Services/CategoryService.cs`

---

## BUG-11 — Fórmula de porcentaje de ganancia incorrecta *(v1.2.0 — 2026-06-18)*

- **Módulo**: Productos
- **Causa**: La fórmula dividía por `SalePrice` en vez de `PurchasePrice`. Ejemplo: costo $60, venta $100 → resultado incorrecto era 40% en vez de 66,7%.
- **Solución**: `(SalePrice - PurchasePrice) / PurchasePrice × 100`. Guard para `PurchasePrice == 0` devuelve 0.
- **Archivos**: `src/TinaStore.Domain/Entities/Product.cs`

---

## BUG-12 — Bloque @media sin cerrar en app.css *(v1.2.0 — 2026-06-18)*

- **Módulo**: Global / CSS
- **Causa**: El último bloque `@media (max-width: 576px)` al final del archivo no tenía llave de cierre `}`.
- **Solución**: Cierre agregado. Reglas adicionales de refuerzo para botones en tarjetas y modales.
- **Archivos**: `src/TinaStore.Web/wwwroot/app.css`

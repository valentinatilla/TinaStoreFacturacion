# BUGFIXES — Tina Store

Registro detallado de todos los bugs corregidos en el proyecto.

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

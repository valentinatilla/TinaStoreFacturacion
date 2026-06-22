# BUGFIXES — Tina Store

Registro detallado de todos los bugs corregidos en el proyecto.

---

## 2026-06-22 — Fase H: Drag & drop de imagen, íconos PWA

### BUG-B16 — Sin drag & drop de imagen en Productos
- **Módulo**: Productos
- **Problema**: La zona de carga de imagen del modal de producto solo respondía al clic. Arrastrar y soltar un archivo desde el explorador no hacía nada.
- **Causa raíz**: El `<InputFile>` estaba oculto con `class="d-none"` y no era alcanzable por el evento `drop` del browser; no había `@ondragover:preventDefault` para habilitar el drop.
- **Solución**: El `InputFile` pasa a `class="ts-file-input-overlay"` — se posiciona con `position: absolute; inset: 0; opacity: 0` cubriendo toda la zona de drop. El browser entrega nativamente los archivos arrastrados al `<input type="file">` superpuesto y `OnChange` se dispara como en el clic normal. Se añadieron `@ondragenter`/`@ondragleave` para gestionar el estado visual `_dragActivo` que aplica la clase `.ts-drag-active` (borde morado, fondo ligeramente coloreado). `@ondragover:preventDefault` y `@ondrop:preventDefault` evitan la apertura del archivo en el browser.
- **Archivos**: `Productos/Index.razor`, `app.css`
- **Resultado**: ✅ Corregido

### ISSUE-12 — Íconos PWA no generados
- **Módulo**: PWA / manifest
- **Problema**: `manifest.webmanifest` referenciaba `/icons/icon-192.png` e `/icons/icon-512.png` que no existían como archivos físicos. Al instalar la app en móvil se mostraba el ícono genérico del browser.
- **Causa raíz**: Los archivos simplemente no habían sido creados. La carpeta `wwwroot/icons/` solo contenía el `README.md` de instrucciones.
- **Solución**: Generados con System.Drawing: fondo cuadrado #7C3AED (morado marca), círculo rosa #F472B6 en el centro y letra "T" en blanco como inicial de Tina Store. El diseño ocupa el 80% central seguro de la zona maskable según la especificación PWA.
  - `icon-192.png` — 192×192 px, 3.7 KB
  - `icon-512.png` — 512×512 px, 11.6 KB
- **Archivos**: `src/TinaStore.Web/wwwroot/icons/icon-192.png`, `src/TinaStore.Web/wwwroot/icons/icon-512.png`
- **Resultado**: ✅ Corregido

---

## 2026-06-22 — Fases B–G: Dashboard, Clientes, Proveedores, Errores API, Productos, Categorías

### BUG-B05 — Tarjetas KPI del Dashboard no eran clickeables
- **Módulo**: Dashboard (Home.razor)
- **Problema**: Las 4 tarjetas de métricas (Ventas hoy, Ventas del mes, Por cobrar, Stock bajo) eran divs estáticos sin navegación.
- **Causa raíz**: Renderizadas como `<div class="card kpi-card">` sin interactividad.
- **Solución**: Reemplazadas por `<a href="...">` apuntando a `/facturas`, `/cuentas-por-cobrar` y `/productos`. Clase CSS `.kpi-card-link` con hover que resalta el valor en color primario.
- **Archivos**: `Home.razor`, `app.css`
- **Resultado**: ✅ Corregido

### BUG-B06 — Número de documento de cliente podía repetirse
- **Módulo**: Clientes
- **Problema**: Crear dos clientes con el mismo número de documento era posible sin advertencia.
- **Causa raíz**: `CustomerService` no verificaba unicidad antes de persistir.
- **Solución**: `CreateAsync` y `UpdateAsync` llaman a `GetByDocumentAsync`; si ya existe, lanzan `InvalidOperationException`. `CustomersController` captura la excepción y devuelve HTTP 409 Conflict con mensaje descriptivo. El modal de Clientes muestra el error real de la API.
- **Archivos**: `CustomerService.cs`, `CustomersController.cs`, `Clientes/Index.razor`
- **Resultado**: ✅ Corregido

### BUG-B07/B08 — Faltan filtros de email y estado en Clientes
- **Módulo**: Clientes
- **Problema**: La barra de filtros solo permitía buscar por nombre/documento/teléfono. Sin filtro por email ni por estado comercial.
- **Causa raíz**: No implementados.
- **Solución**: Se añadieron campo de texto para email y selector de estado (Todos / Activo / Inactivo / Sin compras) en la barra de filtros. `Filtrar()` y `LimpiarFiltros()` actualizados para los tres criterios en simultáneo.
- **Archivos**: `Clientes/Index.razor`
- **Resultado**: ✅ Corregido

### BUG-B10/B11-D — Validaciones NIT/Teléfono no estaban en el backend
- **Módulo**: Proveedores (backend)
- **Problema**: La validación de NIT solo-numérico y Teléfono máximo 10 dígitos existía solo en el frontend (Fase A). Un cliente HTTP directo a la API podía enviar valores inválidos.
- **Causa raíz**: `SupplierValidators.cs` no tenía las reglas de formato para `TaxId` ni `Phone`.
- **Solución**: Añadidas reglas `Matches(@"^\d+$")` y `MaximumLength(10)` a `CreateSupplierValidator` y `UpdateSupplierValidator`.
- **Archivos**: `SupplierValidators.cs`
- **Resultado**: ✅ Corregido

### BUG-B12 — Errores reales de la API no se propagaban al formulario
- **Módulo**: Global (Clientes, Proveedores, Categorías)
- **Problema**: Al fallar una operación de guardado, el usuario veía "Error al guardar" genérico en lugar del mensaje real de la API (ej: "Ya existe un cliente con ese documento").
- **Causa raíz**: Los métodos `CreateClienteAsync`, `UpdateClienteAsync`, etc. en `TinaStoreApiClient` retornaban `bool` sin leer el body de la respuesta de error.
- **Solución**: Nuevo helper `LeerMensajeErrorAsync` que extrae el campo `message` del JSON de error (incluyendo arrays de FluentValidation). Los métodos de Clientes, Proveedores y Categorías ahora devuelven `(bool Ok, string? Error)`. Los modales muestran el mensaje exacto.
- **Archivos**: `TinaStoreApiClient.cs`, `Clientes/Index.razor`, `Proveedores/Index.razor`, `Categorias/Index.razor`
- **Resultado**: ✅ Corregido

### BUG-B14/B15 — Sin validaciones por campo ni rechazo de negativos en Productos
- **Módulo**: Productos
- **Problema**: `Guardar()` solo verificaba que el nombre no fuera vacío y que hubiera categoría. No rechazaba precios ni stocks negativos; no mostraba mensajes por campo.
- **Causa raíz**: Validación mínima en el `if` inicial de `Guardar()`.
- **Solución**: Bloque de validaciones explícitas con mensaje individual para: nombre obligatorio, categoría seleccionada, precio de venta ≥ 0, precio de costo ≥ 0, stock ≥ 0, stock mínimo ≥ 0.
- **Archivos**: `Productos/Index.razor`
- **Resultado**: ✅ Corregido

### BUG-B17 — Botón X de imagen era ovalado
- **Módulo**: Productos
- **Problema**: El botón para quitar la imagen seleccionada era un óvalo en lugar de un círculo perfecto.
- **Causa raíz**: `.ts-image-remove-btn` usaba `padding: .1rem .35rem` que generaba ancho variable.
- **Solución**: Dimensiones fijas `width/height: 24px`, `padding: 0`, `display: flex; align-items/justify-content: center`. Ahora es siempre circular.
- **Archivos**: `app.css`
- **Resultado**: ✅ Corregido

### BUG-B19 — Imágenes de productos no se guardaban correctamente
- **Módulo**: Productos
- **Problema**: Al seleccionar y guardar una imagen de producto, el servidor podía rechazarla o no procesarla.
- **Causa raíz**: `SubirImagenProductoAsync` enviaba `Content-Type: application/octet-stream` en el `StreamContent` del multipart. ASP.NET Core lee la extensión pero el tipo incorrecto causaba problemas de validación.
- **Solución**: El `Content-Type` ahora se infiere de la extensión del archivo: `image/jpeg`, `image/png` o `image/webp`.
- **Archivos**: `TinaStoreApiClient.cs`
- **Resultado**: ✅ Corregido

### BUG-B20 — Enter no guardaba en el formulario de Categorías
- **Módulo**: Categorías
- **Problema**: Al escribir el nombre de una categoría y presionar Enter, el modal no guardaba — el usuario tenía que usar el ratón para clicar "Guardar".
- **Causa raíz**: Los inputs del modal no tenían handler de teclado.
- **Solución**: `@onkeydown="HandleKeyDown"` en ambos inputs (Nombre y Descripción). `HandleKeyDown` llama a `Guardar()` cuando la tecla es `"Enter"`.
- **Archivos**: `Categorias/Index.razor`
- **Resultado**: ✅ Corregido

### BUG-B21 — Se podían crear categorías con nombre duplicado
- **Módulo**: Categorías
- **Problema**: Crear dos categorías con el mismo nombre era posible.
- **Causa raíz**: `CategoryService.CreateAsync` no verificaba unicidad.
- **Solución**: `CreateAsync` obtiene todas las categorías y verifica `OrdinalIgnoreCase` antes de persistir; lanza `InvalidOperationException` si hay duplicado. `CategoriesController` devuelve HTTP 409 Conflict. El modal muestra el error descriptivo.
- **Archivos**: `CategoryService.cs`, `CategoriesController.cs`, `Categorias/Index.razor`, `TinaStoreApiClient.cs`
- **Resultado**: ✅ Corregido

---

## 2026-06-21 — Fase A: Botones, ojito, user-select, ContactName

### BUG-A-B02 — Ojito de contraseña no existía / no era reutilizable
- **Módulo**: Login, Usuarios
- **Problema**: No existía toggle ver/ocultar contraseña. El input usaba `type="password"` fijo.
- **Causa raíz**: No implementado.
- **Solución**: Variable booleana por campo, binding dinámico de `type`, clase `.btn-password-toggle` reutilizable. Ojito se resetea al abrir/cerrar modales y al hacer submit.
- **Archivos**: `Login.razor`, `Usuarios/Index.razor`, `app.css`
- **Resultado**: ✅ Corregido

### BUG-A-B01/B03 — Botones de modal en Usuarios con `btn-sm` inconsistente
- **Módulo**: Usuarios
- **Problema**: Botones Cancelar/Guardar/Restablecer del modal usaban `btn-sm`, diferente a Clientes.
- **Causa raíz**: `btn-sm` hardcodeado en footers de modal.
- **Solución**: Removido `btn-sm`. Añadidos `.btn-warning` y `.btn-outline-warning` al sistema CSS.
- **Archivos**: `Usuarios/Index.razor`, `app.css`
- **Resultado**: ✅ Corregido

### BUG-A-B04 — Labels se seleccionaban accidentalmente con doble clic
- **Módulo**: Global
- **Problema**: `form-label`, `thead th`, `.badge`, títulos, botones seleccionables con doble clic.
- **Causa raíz**: No se usaba `user-select: none` en elementos decorativos.
- **Solución**: `user-select: none` en clases de interfaz. No afecta celdas de datos.
- **Archivos**: `app.css`
- **Resultado**: ✅ Corregido

### BUG-A-B09 — Campo Contacto visible en Proveedores
- **Módulo**: Proveedores
- **Problema**: Formulario mostraba campo `ContactName` innecesario.
- **Causa raíz**: Campo presente en entidad, DTOs, servicio, UI y BD.
- **Solución**: Eliminado en todos los niveles + migración `20260621000001_RemoveSupplierContactName`.
- **Archivos**: `Supplier.cs`, `SupplierDtos.cs`, `SupplierService.cs`, `TinaStoreApiClient.cs`, `Proveedores/Index.razor`, migración, snapshot.
- **Resultado**: ✅ Corregido

### BUG-A-B10/B11 — NIT y Teléfono sin validación; errores fuera del modal
- **Módulo**: Proveedores
- **Problema**: NIT y Teléfono aceptaban letras. Errores salían fuera del modal. Modal se cerraba con error.
- **Causa raíz**: Sin validación de formato en frontend. Sin variable de error interna al modal.
- **Solución**: Regex `^\d+$` para NIT, `^\d+$` + maxlength 10 para Teléfono en `Guardar()`. `_errorModal` dentro del modal. Modal no se cierra si hay error.
- **Archivos**: `Proveedores/Index.razor`
- **Resultado**: ✅ Corregido (validación backend pendiente en Fase D)

---

## BUG-C1-01 — Logo roto en sidebar y configuración *(v2.8.0 — 2025-07-14)*

- **Módulo**: Configuración, Layout (MainLayout)
- **Problema**: Al subir un logo nuevo, el sidebar mostraba una imagen rota (icono de "?") y el mismo problema ocurría en la pantalla de Configuración.
- **Causa raíz**: La URL del logo se construía usando `ApiBaseUrl` (URL interna del servidor, por ejemplo `http://localhost:5000`), que **no es accesible desde el navegador** en Railway (donde el frontend y el backend corren en contenedores separados).
- **Solución**: Se introdujo `PublicApiUrl` en `appsettings.json` (URL pública accesible por el browser) y se separó de `ApiBaseUrl` (uso interno backend→backend). `TinaStoreApiClient` expone `PublicBaseUrl` y todos los componentes que construyen URLs de imágenes lo usan.
- **Archivos afectados**:
  - `src/TinaStore.Web/Services/TinaStoreApiClient.cs`
  - `src/TinaStore.Web/Program.cs`
  - `src/TinaStore.Web/Components/Pages/Configuracion/StoreSettings.razor`
  - `src/TinaStore.Web/Components/Layout/MainLayout.razor`
  - `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`
  - `src/TinaStore.Web/Components/Pages/Productos/Index.razor`
  - `src/TinaStore.Web/appsettings.json`
  - `src/TinaStore.Web/appsettings.Development.json`
  - `src/TinaStore.Web/appsettings.Production.json`

---

## BUG-C1-02 — Imágenes de productos rotas en detalle de venta *(v2.8.0 — 2025-07-14)*

- **Módulo**: Ventas/Facturas
- **Problema**: Las miniaturas de producto en el detalle expandible de una factura mostraban imagen rota en entornos distintos a desarrollo local.
- **Causa raíz**: Se usaba `Config["ApiBaseUrl"]` (inyección directa de `IConfiguration`) para construir la URL; en Railway la URL interna no resuelve desde el navegador.
- **Solución**: Se eliminó la inyección de `IConfiguration` en `Facturas/Index.razor` y `Productos/Index.razor`; ahora usan `Api.PublicBaseUrl`.
- **Archivos afectados**:
  - `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`
  - `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-A2-01 — Badge de stock incorrecto en productos inactivos *(v2.5.0 — 2026-06-19)*

- **Módulo**: Productos
- **Problema**: El badge numérico de stock mostraba fondo verde (`bg-success`) incluso en productos inactivos, sin reflejar su estado real.
- **Causa raíz**: El ternario evaluaba primero `CurrentStock == 0` y `IsLowStock` con `IsActive`, pero no tenía rama explícita para `!IsActive`; el caso por defecto era siempre `bg-success`.
- **Solución**: Priorizar `!p.IsActive → bg-secondary` antes de evaluar las condiciones de stock.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-A2-02 — Categoría vacía al abrir el modal de edición *(v2.5.0 — 2026-06-19)*

- **Módulo**: Productos
- **Problema**: Al abrir el modal de edición de un producto el selector de categoría quedaba en "Seleccione..." (vacío).
- **Causa raíz**: El fallback cuando `GetProductoAsync` devuelve null solo copiaba `Name` y `Sku`, dejando `CategoryId = 0` y los demás campos vacíos.
- **Solución**: Rellenar el fallback con todos los campos disponibles del DTO de listado (`CategoryId`, `SupplierId`, precios, stock, imagen).
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-A2-03 — ProductCount siempre 0 en categorías *(v2.5.0 — 2026-06-19)*

- **Módulo**: Categorías
- **Problema**: La columna "Productos" en la pantalla de Categorías siempre mostraba 0, impidiendo además eliminar categorías que sí tenían productos.
- **Causa raíz**: `CategoryService.ToDto` filtraba `!p.IsDeleted && p.IsActive`; EF Core ya aplica `HasQueryFilter(e => !e.IsDeleted)` en el `Include`, y el `&& p.IsActive` adicional excluía todos los productos inactivos.
- **Solución**: Simplificar a `c.Products?.Count ?? 0`; el query filter de EF garantiza que no aparecen registros eliminados.
- **Archivos**: `src/TinaStore.Application/Services/CategoryService.cs`

---

## BUG-C8 — Sin indicador de carga al descargar PDF *(v1.5.0 — 2026-06-19)*

- **Módulo**: Facturas
- **Problema**: Al hacer clic en "Descargar PDF" no había feedback visual; el usuario podía hacer clic múltiples veces.
- **Causa raíz**: `DescargarPdf()` sin estado de carga por ID.
- **Solución**: `HashSet<int> _pdfDescargando` — spinner mientras descarga, botón deshabilitado, confirmación/error al terminar.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`

---

## BUG-C9 — Estado de factura en inglés en el PDF *(v1.5.0 — 2026-06-19)*

- **Módulo**: PDF / Facturas
- **Problema**: El PDF mostraba PAID, CANCELLED, PARTIAL, PENDING.
- **Causa raíz**: `invoice.Status.ToString().ToUpper()` usa el nombre del enum en inglés.
- **Solución**: Switch expression `(color, etiqueta)` con valores PAGADA / ANULADA / PARCIAL / PENDIENTE.
- **Archivos**: `src/TinaStore.Infrastructure/Services/PdfService.cs`

---

## BUG-C10 — N° Factura sin etiqueta visible en PDF *(v1.5.0 — 2026-06-19)*

- **Módulo**: PDF / Facturas
- **Problema**: El número de factura aparecía sin contexto en el encabezado del PDF.
- **Solución**: Añadida etiqueta "N° Factura" en gris pequeño encima del número.
- **Archivos**: `src/TinaStore.Infrastructure/Services/PdfService.cs`

---

## BUG-C11 — No se podía confirmar anulación con Enter *(v1.5.0 — 2026-06-19)*

- **Módulo**: Facturas
- **Problema**: Presionar Enter en el textarea de motivo no hacía nada.
- **Causa raíz**: Sin `@onkeydown` en el textarea.
- **Solución**: Handler `HandleAnulacionKeyDown` — Enter (sin Shift) llama `ConfirmarAnulacion()`. Shift+Enter permite salto de línea.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`

---

## BUG-C12 — Sin validación de descuento y valores negativos en nueva factura *(v1.5.0 — 2026-06-19)*

- **Módulo**: Facturas / Nueva factura
- **Problema**: No había bloqueo para descuentos mayores al subtotal, valores negativos ni pago inicial mayor al total.
- **Causa raíz**: `RecalcularTotales()` usaba `Math.Max(0,...)` pero no avisaba al usuario. `CrearFactura()` enviaba cualquier valor.
- **Solución**: Clamp + advertencia inline en `RecalcularTotales()`. 8 validaciones en `CrearFactura()` antes de llamar a la API.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Nueva.razor`

---

## FEAT-B6 — Ordenamiento por columnas en Facturas *(v1.4.0 — 2026-06-19)*

- **Módulo**: Facturas
- **Problema**: No había forma de ordenar la lista de facturas por importe, saldo o fecha desde la UI.
- **Solución**: Cabeceras de tabla clickeables con iconos Bootstrap (↓↑↕). Métodos `OrdenarPor()`, `AplicarOrden()` y `SortIconClass()` en el bloque `@code`. El estado de sort se resetea con "Limpiar filtros".
- **Archivos**: `src/TinaStore.Web/Components/Pages/Facturas/Index.razor`

---

## FEAT-B7 — Selector de orden ampliado en Productos *(v1.4.0 — 2026-06-19)*

- **Módulo**: Productos
- **Problema**: Solo existía ordenamiento por stock (mayor/menor). No había orden por precio, costo, ganancia ni nombre.
- **Solución**: Selector "Ordenar por" con 10 opciones en 5 grupos. Campo `_orden` (antes `_ordenStock`) con switch extendido de 10 casos en `Filtrar()`.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Productos/Index.razor`

---

## BUG-A1 — Correo sin validación visual frontend *(v1.3.0 — 2026-06-19)*

- **Módulo**: Clientes
- **Problema**: El campo Email aceptaba cualquier texto; los errores del backend llegaban como mensaje genérico fuera del modal.
- **Causa raíz**: `<input>` sin validación en Blazor; `Guardar()` solo validaba que el nombre no fuera vacío.
- **Solución**: Regex `^[^@\s]+@[^@\s]+\.[^@\s]+$` ejecutado en `Guardar()` antes de llamar a la API. Si falla, se muestra `invalid-feedback` junto al campo.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Pruebas**: Correo inválido → error inline. Correo vacío → permite guardar. Correo válido → guarda correctamente.

---

## BUG-A2 — Teléfono sin validación de formato *(v1.3.0 — 2026-06-19)*

- **Módulo**: Clientes
- **Problema**: El backend solo validaba longitud máxima (20 chars). No había validación de formato colombiano ni en frontend.
- **Causa raíz**: `CustomerValidators.cs` sin `.Matches()`. Frontend sin regex.
- **Solución**: Regex `^\+?[\d\s\-]{7,20}$` en frontend (`EsTelefonoValido()`) y en `CreateCustomerValidator` + `UpdateCustomerValidator`.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`, `src/TinaStore.Application/Validators/CustomerValidators.cs`
- **Pruebas**: "abc123" → error. "3001234567" → válido. "+573001234567" → válido. Vacío → permite guardar.

---

## BUG-A3 — Errores del modal de cliente aparecían fuera del modal *(v1.3.0 — 2026-06-19)*

- **Módulo**: Clientes
- **Problema**: `_mensaje` se renderizaba a nivel de página. Al tener el backdrop del modal activo, el error quedaba tapado o era invisible para el usuario.
- **Causa raíz**: Un solo `_mensaje` compartido entre página y modal. Sin errores por campo.
- **Solución**: Nuevos `_erroresModal` (List<string>), `_errNombre`, `_errEmail`, `_errTelefono`. El `modal-body` muestra `alert-danger` como resumen + `invalid-feedback` por campo. El modal no se cierra al fallar validación.
- **Archivos**: `src/TinaStore.Web/Components/Pages/Clientes/Index.razor`
- **Pruebas**: Guardar con nombre vacío → error dentro del modal. Guardar con email inválido → error en campo email. Modal permanece abierto al fallar.

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

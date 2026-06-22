# TESTING — Tina Store

Guía de pruebas manuales y automatizadas del proyecto.

---

## Cómo ejecutar las pruebas automatizadas

```bash
# Desde la raíz del repositorio
dotnet test

# Solo pruebas unitarias
dotnet test tests/TinaStore.Tests.Unit

# Solo pruebas de integración
dotnet test tests/TinaStore.Tests.Integration
```

---

## Pruebas manuales — Sprint Corrección 2026-06-22 (Fase H)

### H1 — Drag & drop de imagen en Productos (BUG-B16)
- [ ] Abrir modal de nuevo/editar producto → la zona de imagen muestra "Arrastra una imagen aquí o haz clic para seleccionar"
- [ ] Arrastrar un archivo `.jpg` desde el explorador a la zona → la zona se resalta en morado mientras el archivo está sobre ella
- [ ] Soltar el archivo → se muestra el preview de la imagen correctamente
- [ ] Arrastrar un archivo `.png` → mismo comportamiento, preview correcto
- [ ] Arrastrar un archivo `.webp` → preview correcto
- [ ] Arrastrar un archivo de más de 2 MB → mensaje de error "La imagen no puede superar 2 MB", la zona vuelve a su estado vacío
- [ ] Arrastrar un archivo `.pdf` o `.txt` → mensaje de error "Solo se permiten imágenes JPG, PNG o WEBP"
- [ ] Clic en la zona (sin arrastrar) → se abre el explorador de archivos → seleccionar imagen → preview correcto
- [ ] Con imagen ya seleccionada: clic en X → imagen se elimina, vuelve la zona de drop
- [ ] Guardar el producto con imagen arrastrada → imagen se sube y se muestra en la tabla de productos
- [ ] En móvil (touch): el clic sobre la zona abre el selector de archivo del SO

### H2 — Íconos PWA generados (ISSUE-12)
- [ ] Abrir Chrome DevTools → Application → Manifest → los íconos 192×192 y 512×512 aparecen sin error
- [ ] En Android (Chrome): "Agregar a pantalla de inicio" → el ícono instalado muestra la T de Tina Store sobre fondo morado
- [ ] En iOS (Safari): "Añadir a inicio" → ícono correcto o fallback del sistema (iOS no usa maskable)
- [ ] Lighthouse PWA audit → puntaje de íconos sin advertencias de tamaño faltante
- [ ] `GET /icons/icon-192.png` responde 200 con `Content-Type: image/png`
- [ ] `GET /icons/icon-512.png` responde 200 con `Content-Type: image/png`

---

## Pruebas manuales — Sprint Corrección 2026-06-22 (Fases B–G)

### B — Tarjetas KPI clickeables (BUG-B05)
- [ ] Clic en tarjeta "Ventas hoy" → navega a `/facturas`
- [ ] Clic en tarjeta "Ventas del mes" → navega a `/facturas`
- [ ] Clic en tarjeta "Por cobrar" → navega a `/cuentas-por-cobrar`
- [ ] Clic en tarjeta "Stock bajo" → navega a `/productos`
- [ ] Hover sobre cualquier tarjeta → cursor pointer y valor cambia de color
- [ ] En móvil: las tarjetas siguen siendo clickeables y no hay regresión visual

### C — Documento único de cliente (BUG-B06)
- [ ] Crear cliente con documento ya existente → modal no cierra, muestra mensaje "Ya existe un cliente con ese número de documento"
- [ ] Editar cliente cambiando documento al de otro cliente → mismo rechazo con mensaje
- [ ] Crear cliente con documento nuevo → se guarda correctamente
- [ ] Editar cliente manteniendo su propio documento → se guarda sin error

### C2 — Filtros de email y estado en Clientes (BUG-B07/B08)
- [ ] Filtrar por email parcial → lista filtra correctamente
- [ ] Filtrar por estado "Activo" → solo clientes activos
- [ ] Filtrar por estado "Inactivo" → solo clientes inactivos
- [ ] Filtrar por estado "Sin compras" → solo clientes sin historial de ventas
- [ ] Combinar texto + email + estado → los tres filtros en AND
- [ ] Clic en "Limpiar" → todos los filtros (incluidos email y estado) se resetean

### D — Validaciones backend de proveedores (BUG-B10/B11-D)
- [ ] Desde Postman/curl: `POST /api/suppliers` con `taxId: "ABC123"` → 400 con mensaje de validación
- [ ] Desde Postman/curl: `POST /api/suppliers` con `phone: "12345678901"` (11 dígitos) → 400 con error
- [ ] Desde la UI: campos solo aceptan números (validación frontend sigue activa)
- [ ] Crear proveedor con NIT y teléfono válidos → se guarda correctamente

### E — Mensajes de error reales de la API (BUG-B12)
- [ ] Crear cliente con documento duplicado → modal muestra mensaje exacto de la API, no texto genérico
- [ ] Crear categoría duplicada → mensaje exacto visible en modal
- [ ] Error de validación FluentValidation (campo vacío) → mensaje(s) de validación visibles
- [ ] Operación exitosa → modal cierra normalmente, sin mensaje de error residual

### F — Validaciones por campo en Productos (BUG-B14/B15)
- [ ] Guardar producto sin nombre → mensaje "El nombre del producto es obligatorio"
- [ ] Guardar producto sin categoría seleccionada → mensaje "Debes seleccionar una categoría"
- [ ] Guardar producto con precio de venta negativo → mensaje de rechazo
- [ ] Guardar producto con precio de costo negativo → mensaje de rechazo
- [ ] Guardar producto con stock negativo → mensaje de rechazo
- [ ] Guardar producto con stock mínimo negativo → mensaje de rechazo
- [ ] Guardar producto con todos los campos válidos → se guarda correctamente

### F2 — Botón X circular de imagen (BUG-B17)
- [ ] Seleccionar imagen en modal de producto → aparece preview con botón X
- [ ] El botón X es un círculo perfecto (no óvalo)
- [ ] Clic en X → imagen se deselecciona y vuelve la zona de carga
- [ ] En móvil: el botón sigue siendo circular y táctilmente cómodo

### F3 — Content-Type correcto en imágenes (BUG-B19)
- [ ] Subir imagen `.jpg` → se guarda y se muestra correctamente
- [ ] Subir imagen `.png` → se guarda y se muestra correctamente
- [ ] Subir imagen `.webp` → se guarda y se muestra correctamente
- [ ] Imagen del producto visible en detalle de factura (no rota)

### G — Enter guarda en Categorías (BUG-B20)
- [ ] Modal abierto con foco en "Nombre" → presionar Enter → se guarda
- [ ] Modal abierto con foco en "Descripción" → presionar Enter → se guarda
- [ ] Campos vacíos + Enter → no guarda, muestra validación

### G2 — Unicidad de nombre de categoría (BUG-B21)
- [ ] Crear categoría con nombre exactamente igual a una existente → modal no cierra, mensaje de error
- [ ] Crear categoría con nombre en MAYÚSCULAS igual a una existente → rechazado (case-insensitive)
- [ ] Crear categoría con nombre nuevo → se guarda correctamente
- [ ] Lista de categorías se actualiza inmediatamente después de crear

---

## Pruebas manuales — Sprint Corrección 2026-06-21 (Fase A)

### A1 — Ojito de contraseña (Login y Usuarios)
- [ ] Login: clic en ojito → contraseña visible, ícono cambia
- [ ] Login: segundo clic → contraseña oculta, ícono vuelve
- [ ] Login: alternar 5+ veces → funciona todas las veces
- [ ] Login: hacer submit con contraseña visible → no queda expuesta al navegar
- [ ] Usuarios / Nuevo usuario: ojito funciona igual
- [ ] Usuarios / Nuevo usuario: cerrar modal → contraseña vuelve a ocultarse
- [ ] Usuarios / Resetear contraseña: dos ojitos independientes funcionan por separado
- [ ] Probar en móvil (Chrome / Safari)

### A2 — Botones normalizados en Usuarios
- [ ] Modal "Nuevo usuario" → footer: Cancelar y Guardar del mismo tamaño que en Clientes
- [ ] Modal "Resetear contraseña" → footer: Cancelar y Restablecer normalizados
- [ ] Verificar en móvil: botones con tamaño táctil adecuado

### A3 — user-select en etiquetas de interfaz
- [ ] Doble clic en label "Nombre completo *" → NO se selecciona
- [ ] Doble clic en encabezado de columna tabla → NO se selecciona
- [ ] Doble clic en badge "Activo" → NO se selecciona
- [ ] Doble clic en celda con nombre de cliente → SÍ se puede seleccionar (datos reales)
- [ ] Doble clic en número de documento → SÍ se puede seleccionar

### A4 — Campo Contacto eliminado en Proveedores
- [ ] Tabla de proveedores: columna "Contacto" no aparece
- [ ] Modal "Nuevo proveedor": campo Contacto no aparece
- [ ] Modal "Editar proveedor": campo Contacto no aparece
- [ ] Crear proveedor → guarda correctamente
- [ ] Editar proveedor → actualiza correctamente

### A5 — Validaciones NIT y Teléfono en Proveedores
- [ ] NIT con letras (ej: "ABC123") → error en modal: "El NIT debe contener solo números."
- [ ] NIT vacío → guarda (campo opcional)
- [ ] NIT con solo dígitos → guarda correctamente
- [ ] Teléfono con guiones (ej: "300-123-4567") → error: "El teléfono debe contener solo números."
- [ ] Teléfono con 11 dígitos → error: "El teléfono debe tener máximo 10 dígitos."
- [ ] Teléfono vacío → guarda (campo opcional)
- [ ] Teléfono con 10 dígitos → guarda correctamente
- [ ] Modal NO se cierra cuando hay error de validación

### A6 — Filtros en Proveedores
- [ ] Escribir en barra de búsqueda → lista filtra en tiempo real
- [ ] Buscar por NIT parcial → resultados correctos
- [ ] Clic en "Limpiar" → lista completa restaurada
- [ ] Sin resultados → mensaje "Sin resultados"

### A7 — Migración RemoveSupplierContactName
- [ ] `dotnet ef database update` en dev → sin errores
- [ ] Columna ContactName ya no existe en tabla Suppliers

---

## Pruebas manuales — v2.8.0 (Fases A–F)

### A — Categorías: ordenamiento

| # | Prueba | Resultado esperado |
|---|---|---|
| A1 | Clic en "A→Z" | Categorías ordenadas de menor a mayor considerando tildes (ej: ñ después de n) |
| A2 | Clic en "Z→A" | Categorías ordenadas de mayor a menor |
| A3 | Escribir en el buscador | La lista se filtra en tiempo real sin recargar |
| A4 | En pantalla móvil | La columna Descripción desaparece; solo se ve Nombre y Acciones |

### B — Responsive en módulos

| # | Módulo | Prueba | Resultado esperado |
|---|---|---|---|
| B1 | Clientes | Abrir en móvil (<576px) | Filtros hacen wrap; columnas secundarias ocultas |
| B2 | Ventas | Abrir en móvil | Columnas Pagado/Saldo/Fecha se ocultan |
| B3 | Productos | Abrir en móvil | Columnas SKU/Categoría/Costo ocultas |
| B4 | Usuarios | Abrir en móvil | Columnas Email/Estado/Último acceso ocultas |
| B5 | Cuentas por cobrar | Abrir en móvil | Columnas secundarias ocultas; Cliente/Saldo/WhatsApp visibles |

### C — Logo e imágenes

| # | Prueba | Resultado esperado |
|---|---|---|
| C1 | Subir logo en Configuración | Logo aparece inmediatamente en el sidebar sin recargar |
| C2 | Recargar página tras subir logo | Logo persiste y se ve correctamente |
| C3 | Abrir detalle de venta con imagen de producto | La imagen del producto se muestra (no imagen rota) |
| C4 | Entorno Railway (producción) | Logo e imágenes accesibles usando la `PublicApiUrl` correcta |

### E — Filtros avanzados en Clientes

| # | Prueba | Resultado esperado |
|---|---|---|
| E1 | Ingresar saldo mínimo | Solo aparecen clientes con saldo pendiente ≥ valor ingresado |
| E2 | Ingresar saldo máximo | Solo aparecen clientes con saldo pendiente ≤ valor ingresado |
| E3 | Ingresar días sin comprar | Clientes cuya última compra fue hace ≥ días ingresados; clientes sin compra siempre aparecen |
| E4 | Combinar saldo mínimo + días | Se aplican ambos filtros en AND |
| E5 | Clic en "Limpiar" | Todos los filtros (incluidos saldo y días) se resetean |

### F — PWA

| # | Prueba | Resultado esperado |
|---|---|---|
| F1 | Chrome móvil → "Agregar a pantalla de inicio" | Opción disponible; nombre corto "Tina Store"; color de tema rosado |
| F2 | App instalada en Android | Ícono en pantalla de inicio (requiere archivos `icon-192.png` / `icon-512.png`) |
| F3 | Barra de estado en iOS Safari | Color de barra coincide con `theme-color` (#F472B6) |

---

## Pruebas manuales — Fase C (v1.5.0)

### 🧾 Facturas — PDF, anulación y descuento

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| C1 | Clic en descargar PDF | Botón muestra spinner, se deshabilita mientras descarga | ✅ v1.5.0 |
| C2 | Descarga completa | Botón vuelve al ícono PDF normal, mensaje "PDF generado" | ✅ v1.5.0 |
| C3 | Error al generar PDF | Botón se reactiva, mensaje de error claro | ✅ v1.5.0 |
| C4 | PDF descargado: estado | Muestra PAGADA / PENDIENTE / PARCIAL / ANULADA (en español) | ✅ v1.5.0 |
| C5 | PDF descargado: encabezado | Aparece "N° Factura" en gris sobre el número de factura | ✅ v1.5.0 |
| C6 | Escribir motivo + Enter | Ejecuta la anulación si el motivo es válido | ✅ v1.5.0 |
| C7 | Enter con motivo vacío | No anula, muestra error "El motivo de anulación es obligatorio" | ✅ v1.5.0 |
| C8 | Shift+Enter en textarea | Inserta salto de línea, no ejecuta anulación | ✅ v1.5.0 |
| C9 | Descuento negativo en nueva factura | Se ajusta a 0 automáticamente con advertencia | ✅ v1.5.0 |
| C10 | Descuento > subtotal | Se ajusta al valor del subtotal con advertencia inline | ✅ v1.5.0 |
| C11 | Descuento válido < subtotal | Total se calcula correctamente sin advertencia | ✅ v1.5.0 |
| C12 | Pago inicial > total | Muestra error "Pago inicial no puede ser mayor al total" | ✅ v1.5.0 |
| C13 | Pago inicial negativo | Muestra error "Pago inicial no puede ser negativo" | ✅ v1.5.0 |
| C14 | Cantidad de producto = 0 | Muestra error antes de enviar | ✅ v1.5.0 |
| C15 | Emitir factura con descuento y pago correcto | Factura guardada, estado correcto en listado | ✅ v1.5.0 |

---

## Pruebas manuales — Fase B (v1.4.0)

### 🗂️ Facturas — Ordenamiento por columnas

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| B1 | Clic en cabecera "Fecha" | Ordena descendente (↓ activo), más recientes primero | ✅ v1.4.0 |
| B2 | Clic de nuevo en "Fecha" | Invierte a ascendente (↑), más antiguas primero | ✅ v1.4.0 |
| B3 | Clic en "Total" | Ordena descendente, facturas de mayor total primero | ✅ v1.4.0 |
| B4 | Clic de nuevo en "Total" | Invierte a ascendente (↑ activo) | ✅ v1.4.0 |
| B5 | Clic en "Saldo" descendente | Facturas con mayor saldo pendiente primero | ✅ v1.4.0 |
| B6 | Clic en "Pagado" descendente | Facturas con mayor pago registrado primero | ✅ v1.4.0 |
| B7 | Clic en "N° Factura" | Ordena por número de factura descendente | ✅ v1.4.0 |
| B8 | Clic en columna diferente | Iconos anteriores vuelven a ↕ neutro, nuevo campo activo | ✅ v1.4.0 |
| B9 | Limpiar filtros | Orden vuelve a Fecha descendente, ícono ↕ en demás columnas | ✅ v1.4.0 |
| B10 | Filtrar + ordenar | Filtros y sort funcionan combinados sin afectar datos | ✅ v1.4.0 |

### 📦 Productos — Selector de orden ampliado

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| B11 | Seleccionar "Stock mayor a menor" | Productos con más stock aparecen primero | ✅ v1.4.0 |
| B12 | Seleccionar "Stock menor a mayor" | Productos con menos stock (agotados) primero | ✅ v1.4.0 |
| B13 | Seleccionar "Precio mayor a menor" | Productos más caros primero | ✅ v1.4.0 |
| B14 | Seleccionar "Precio menor a mayor" | Productos más económicos primero | ✅ v1.4.0 |
| B15 | Seleccionar "Costo mayor a menor" | Productos con mayor costo primero | ✅ v1.4.0 |
| B16 | Seleccionar "Ganancia mayor a menor" | Mayor % de margen primero | ✅ v1.4.0 |
| B17 | Seleccionar "A → Z" | Productos en orden alfabético | ✅ v1.4.0 |
| B18 | Seleccionar "Z → A" | Productos en orden alfabético inverso | ✅ v1.4.0 |
| B19 | Ordenar + filtrar por categoría | Orden conserva el filtro activo | ✅ v1.4.0 |
| B20 | Limpiar filtros | Selector vuelve a "Sin orden" | ✅ v1.4.0 |

---

## Pruebas manuales — Fase A (v1.3.0)

### 👥 Clientes — Validaciones y modal

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| A1 | Crear cliente con correo válido (`cliente@correo.com`) | Guarda correctamente | ✅ v1.3.0 |
| A2 | Crear cliente con correo inválido (`cliente@`) | Error inline en campo email, modal no se cierra | ✅ v1.3.0 |
| A3 | Crear cliente con correo inválido (`cliente.com`) | Error inline en campo email | ✅ v1.3.0 |
| A4 | Crear cliente con correo vacío (campo opcional) | Permite guardar sin error | ✅ v1.3.0 |
| A5 | Crear cliente con teléfono válido (`3001234567`) | Guarda correctamente | ✅ v1.3.0 |
| A6 | Crear cliente con teléfono internacional (`+573001234567`) | Guarda correctamente | ✅ v1.3.0 |
| A7 | Crear cliente con teléfono inválido (`abc123!!`) | Error inline en campo teléfono | ✅ v1.3.0 |
| A8 | Crear cliente con teléfono vacío (opcional) | Permite guardar sin error | ✅ v1.3.0 |
| A9 | Guardar con nombre vacío | Error inline en campo nombre, modal no se cierra | ✅ v1.3.0 |
| A10 | Error de API (p.ej. duplicado) | Mensaje de error aparece DENTRO del modal | ✅ v1.3.0 |
| A11 | Ver modal en pantalla pequeña (móvil) | Modal con scroll, no se corta, botones visibles | ✅ v1.3.0 |
| A12 | Filtrar clientes por estado "Activo" | Solo muestra clientes activos | ✅ v1.3.0 |
| A13 | Filtrar clientes por estado "Inactivo" | Solo muestra clientes inactivos | ✅ v1.3.0 |
| A14 | Filtrar clientes por "Sin correo" | Solo clientes activos sin email | ✅ v1.3.0 |
| A15 | Buscar cliente por correo parcial | Filtra correctamente | ✅ v1.3.0 |
| A16 | Limpiar filtros | Restaura lista completa | ✅ v1.3.0 |
| A17 | Modal de eliminar cliente en móvil | Modal con scroll, botones visibles | ✅ v1.3.0 |

---

## Pruebas manuales por módulo

### 🔐 Autenticación

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Iniciar sesión con credenciales válidas | Redirige al dashboard con datos cargados | ✅ v0.2.0 |
| 2 | Iniciar sesión con contraseña incorrecta | Mensaje de error claro | ✅ v0.2.0 |
| 3 | Acceder a `/productos` sin iniciar sesión | Redirige al login | ✅ v0.2.0 |
| 4 | Cerrar sesión | Redirige al login, limpia la sesión | ✅ v0.2.0 |
| 5 | Recargar la aplicación con sesión activa | Mantiene la sesión | 🔴 Pendiente (ISSUE-01) |
| 6 | Login con Google | Autenticación exitosa con cuenta autorizada | 📋 Fase 4 |

---

### 📦 Productos

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|

| 2 | Crear producto sin nombre | Mensaje de validación | ✅ v0.2.0 |
| 3 | Crear producto sin categoría | Mensaje de validación | ✅ v0.2.0 |
| 4 | Abrir modal de edición | Todos los campos del producto cargados, incluyendo categoría y proveedor | ✅ v0.2.0 |
| 5 | Editar solo el precio | Precio actualizado, categoría y proveedor sin cambios | ✅ v0.2.0 |
| 6 | Editar solo la categoría | Categoría actualizada, proveedor sin cambios | ✅ v0.2.0 |
| 7 | Editar solo el proveedor | Proveedor actualizado, categoría sin cambios | ✅ v0.2.0 |
| 8 | Buscar producto por nombre | Filtra correctamente | ✅ v0.2.0 |
| 9 | Buscar producto por SKU | Filtra correctamente | ✅ v0.2.0 |
| 10 | Eliminar producto | Producto desaparece del listado | ✅ v0.2.0 |
| 11 | Exportar a Excel | Descarga archivo `.xlsx` válido | ✅ v0.2.0 |
| 12 | Crear producto con SKU duplicado | Mensaje de error de SKU duplicado | 📋 Fase 2 |
| 13 | Cargar imagen de producto | Vista previa y guardado correcto | 📋 Fase 3 |

---

### 👥 Clientes

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Ver listado de clientes | Lista cargada con clientes reales | ✅ v0.2.0 |
| 2 | Crear cliente con nombre y teléfono | Cliente aparece en listado | ✅ v0.2.0 |
| 3 | Editar cliente | Datos actualizados correctamente | ✅ v0.2.0 |
| 4 | Desactivar cliente | Cliente marcado como inactivo | ✅ v0.2.0 |

---

### 🧾 Facturas

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Abrir formulario de nueva factura | Selector de clientes muestra opciones | ✅ v0.2.0 |
| 2 | Buscar producto en factura | Aparecen resultados con nombre y SKU | ✅ v0.2.0 |
| 3 | Agregar producto a factura | Aparece en la tabla de detalle | ✅ v0.2.0 |
| 4 | Modificar cantidad en detalle | Subtotal y total se actualizan | ✅ v0.2.0 |
| 5 | Crear factura con cliente y productos | Factura creada, redirige al listado | ✅ v0.2.0 |
| 6 | Intentar crear factura sin cliente | Botón deshabilitado | ✅ v0.2.0 |
| 7 | Descargar PDF de factura | Archivo PDF descargado | ✅ v0.2.0 |
| 8 | Anular factura | Estado cambia a "Anulada" | ✅ v0.2.0 |

---

### 💸 Egresos

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Ver listado de egresos | Lista cargada | ✅ v0.2.0 |
| 2 | Crear egreso | Aparece en el listado | ✅ v0.2.0 |
| 3 | Anular egreso | Estado cambia a "Anulado" | ✅ v0.2.0 |

---

### 📊 Reportes

| # | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| 1 | Ver reporte de ventas | Datos cargados con gráfico y tabla | ✅ v0.2.0 |
| 2 | Ver reporte de gastos | Datos cargados por categoría | ✅ v0.2.0 |
| 3 | Ver cuentas por cobrar | Lista de deudores con saldo | ✅ v0.2.0 |

---

## Pruebas de regresión obligatorias antes de cada versión

Antes de publicar una nueva versión, ejecutar mínimo estas pruebas:

1. ✅ Iniciar sesión correctamente
2. ✅ Dashboard carga con datos
3. ✅ Crear un producto nuevo
4. ✅ Editar un producto existente (verificar categoría y proveedor)
5. ✅ Crear un cliente
6. ✅ Crear una factura con cliente y al menos un producto
7. ✅ Ver listado de facturas
8. ✅ Descargar PDF de una factura
9. ✅ Ver reportes de ventas
10. ✅ Exportar productos a Excel
11. ✅ Cerrar sesión

---

## Pruebas manuales — Fases A2 + B1 + B3 (v1.2.0)

### Productos — Estado de inventario

| Caso | Acción | Resultado esperado |
|---|---|---|
| Producto activo, stock > mínimo | Ver listado | Badge verde "Activo" |
| Producto activo, stock ≤ mínimo | Ver listado | Badge amarillo "Bajo stock" |
| Producto activo, stock = 0 | Ver listado | Badge rojo "Agotado" |
| Producto inactivo (cualquier stock) | Ver listado | Badge gris "Inactivo" |

### Productos — Porcentaje de ganancia

| Caso | Costo | Precio venta | Resultado esperado |
|---|---|---|---|
| Ganancia normal | $60.000 | $100.000 | 66,7 % |
| Sin ganancia | $100.000 | $100.000 | 0,0 % |
| Costo cero | $0 | $50.000 | "No calculable" |
| Pérdida | $100.000 | $80.000 | -20,0 % |

### Categorías — Conteo de productos

| Caso | Resultado esperado |
|---|---|
| Categoría con 3 productos activos | Muestra 3 |
| Categoría con 1 producto inactivo | Muestra 0 |
| Categoría vacía | Muestra 0, botón Eliminar habilitado |
| Intentar eliminar categoría con productos | Error visible, no elimina |

### Clientes — Fecha última compra y estado comercial

| Caso | Resultado esperado |
|---|---|
| Cliente sin facturas | "—" en columna, badge "Sin compras" |
| Cliente con factura reciente (< 6 meses) | Fecha visible, badge "Activo" |
| Cliente con última factura hace > 6 meses | Fecha visible, badge "Inactivo" (amarillo) |
| Cliente inactivo administrativamente | Badge "Inactivo" gris, sin importar compras |

### WhatsApp — Modal editable

| Caso | Resultado esperado |
|---|---|
| Cliente con teléfono | Botón WhatsApp visible en fila |
| Clic en botón WhatsApp | Modal abre con mensaje prellenado |
| Editar mensaje en modal | Cambios se reflejan en el enlace |
| Confirmar | Abre WhatsApp Web/app en nueva pestaña |
| Cliente sin teléfono | Botón WhatsApp no aparece |

### Logo en sidebar

| Caso | Resultado esperado |
|---|---|
| Sin logo configurado | Muestra emoji 🛍️ |
| Con logo configurado | Muestra la imagen del logo |
| API no disponible al iniciar | Muestra emoji 🛍️ sin error visible |

---

## Ambientes de prueba

| Ambiente | URL Web | URL API | Base de datos |
|---|---|---|---|
| Desarrollo | `https://localhost:7173` | `https://localhost:7073` | `tinastore-dev.db` |
| Producción | Por definir | Por definir | `tinastore.db` |

> ⚠️ **Nunca usar datos reales en pruebas de desarrollo.** Usar datos ficticios para clientes, productos y facturas.

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
| 1 | Crear producto con todos los campos | Producto aparece en el listado | ✅ v0.2.0 |
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

## Ambientes de prueba

| Ambiente | URL Web | URL API | Base de datos |
|---|---|---|---|
| Desarrollo | `https://localhost:7173` | `https://localhost:7073` | `tinastore-dev.db` |
| Producción | Por definir | Por definir | `tinastore.db` |

> ⚠️ **Nunca usar datos reales en pruebas de desarrollo.** Usar datos ficticios para clientes, productos y facturas.

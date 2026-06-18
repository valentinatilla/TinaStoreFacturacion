# 🎨 Sistema de Diseño — Tina Store

Guía visual completa del tema **Kawaii Rosado 🌸** de Tina Store.
Este documento es la referencia para mantener un diseño consistente en toda la aplicación.

---

## 🎯 Identidad visual

**Nombre:** Tina Store
**Estilo:** Kawaii, rosado, tierno, femenino, moderno, limpio y profesional
**Tipografía:** Nunito (Google Fonts)
**Framework CSS:** Bootstrap 5 + variables CSS personalizadas

---

## 🎨 Paleta de colores

### Colores principales

| Variable CSS | Valor HEX | Vista previa | Uso |
|---|---|---|---|
| `--ts-primary` | `#F472B6` | 🌸 Rosa principal | Botones, links activos, acentos |
| `--ts-primary-dark` | `#DB2777` | 🌺 Rosa oscuro | Hover, énfasis, headings |
| `--ts-primary-light` | `#FCE7F3` | 🩷 Rosa pastel | Fondos suaves, badges, tablas |
| `--ts-secondary` | `#C084FC` | 💜 Lila suave | Elementos secundarios |
| `--ts-accent` | `#F9A8D4` | 🌷 Rosa claro | Bordes, separadores |

### Colores de fondo

| Variable CSS | Valor HEX | Uso |
|---|---|---|
| `--ts-bg` | `#FFF1F8` | Fondo general de la aplicación |
| `--ts-surface` | `#FFFFFF` | Tarjetas, formularios, modales |
| `--ts-sidebar-bg` | `#4A1942` | Sidebar (vino rosado profundo) |
| `--ts-sidebar-brand` | `#6D1F6A` | Bloque del logo/brand en sidebar |

### Colores de texto

| Variable CSS | Valor HEX | Uso |
|---|---|---|
| `--ts-text-main` | `#3D1040` | Texto principal |
| `--ts-text-soft` | `#9D6F8A` | Texto secundario, labels |
| `--ts-text-light` | `#F9E8F5` | Texto en sidebar |

### Colores de estados

| Estado | Fondo | Texto | Uso |
|---|---|---|---|
| ✅ Pagado / Activo | `#D1FAE5` | `#065F46` | Facturas pagadas, clientes activos |
| ⏳ Pendiente | `#FDE68A` | `#92400E` | Facturas por cobrar |
| 🔵 Parcial | `#BAE6FD` | `#075985` | Pago incompleto |
| ❌ Anulado / Deuda | `#FCA5A5` | `#991B1B` | Facturas anuladas, deudas |
| ⚠️ Bajo stock | `#FEF3C7` | `#B45309` | Producto con poco inventario |
| 🔴 Sin stock | `#FEE2E2` | `#991B1B` | Producto agotado |
| 💜 Activo (badge) | `#EDE9FE` | `#5B21B6` | Cliente/proveedor activo |
| ⚪ Inactivo | `#E5E7EB` | `#6B7280` | Registros deshabilitados |

---

## ✍️ Tipografía

### Fuente principal: Nunito

```html
<!-- Ya incluida en App.razor -->
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Nunito:wght@400;500;600;700;800;900&display=swap" />
```

| Uso | Peso | Tamaño |
|---|---|---|
| Títulos de página (`h1`) | 800 (ExtraBold) | 1.75rem |
| Subtítulos (`h2`, `h3`) | 700 (Bold) | 1.25–1.5rem |
| Texto normal | 400 (Regular) | 0.95rem |
| Labels de formularios | 700 (Bold) | 0.875rem |
| Texto de tablas | 500 (Medium) | 0.875rem |
| Badges y etiquetas | 700 (Bold) | 0.78rem |

---

## 🔘 Botones

Todos los botones usan `border-radius: 999px` (completamente redondeados).

```html
<!-- Botón principal (rosa) -->
<button class="btn btn-primary">Guardar</button>

<!-- Botón secundario (contorno rosa) -->
<button class="btn btn-outline-primary">Cancelar</button>

<!-- Botón de eliminar (rojo salmón) -->
<button class="btn btn-danger">Eliminar</button>

<!-- Botón lila (acción secundaria) -->
<button class="btn btn-secondary">Exportar</button>

<!-- Botón verde (confirmación positiva) -->
<button class="btn btn-success">Confirmar pago</button>
```

---

## 🃏 Tarjetas (Cards)

```html
<!-- Tarjeta estándar -->
<div class="ts-card">
	<h5>Título</h5>
	<p>Contenido de la tarjeta</p>
</div>

<!-- Tarjeta KPI del dashboard -->
<div class="card kpi-card p-4">
	<div class="d-flex align-items-center gap-3">
		<div class="kpi-icon">
			<i class="bi bi-cash-stack"></i>
		</div>
		<div>
			<div class="text-muted small">Total ventas</div>
			<div class="fs-4 fw-bold text-pink-dark">$1,250,000</div>
		</div>
	</div>
</div>
```

---

## 📊 Tablas

```html
<table class="table ts-table">
	<thead>
		<tr>
			<th>Cliente</th>
			<th>Total</th>
			<th>Estado</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>María González</td>
			<td>$85,000</td>
			<td><span class="badge-estado-pagada">✅ Pagada</span></td>
		</tr>
		<tr>
			<td>Juan Pérez</td>
			<td>$120,000</td>
			<td><span class="badge-estado-pendiente">⏳ Pendiente</span></td>
		</tr>
	</tbody>
</table>
```

---

## 🏷️ Badges de estado

```html
<!-- Usar según el estado del registro -->
<span class="badge-estado-pagada">Pagada</span>
<span class="badge-estado-pendiente">Pendiente</span>
<span class="badge-estado-parcial">Pago parcial</span>
<span class="badge-estado-anulada">Anulada</span>
<span class="badge-estado-activo">Activo</span>
<span class="badge-estado-inactivo">Inactivo</span>
<span class="badge-bajo-stock">⚠️ Bajo stock</span>
<span class="badge-sin-stock">🔴 Sin stock</span>
```

---

## 📝 Formularios

```html
<div class="mb-3">
	<label class="form-label">Nombre del cliente</label>
	<input type="text" class="form-control" placeholder="Ej: María González" />
</div>

<div class="mb-3">
	<label class="form-label">Estado</label>
	<select class="form-select">
		<option>Activo</option>
		<option>Inactivo</option>
	</select>
</div>
```

---

## 🚨 Alertas

```html
<!-- Éxito: operación completada -->
<div class="alert alert-success">✅ Factura guardada correctamente.</div>

<!-- Advertencia: atención requerida -->
<div class="alert alert-warning">⚠️ Este producto tiene bajo stock.</div>

<!-- Error: algo salió mal -->
<div class="alert alert-danger">❌ Error al guardar. Verifica los datos.</div>

<!-- Información: dato importante -->
<div class="alert alert-info">ℹ️ Los cambios se aplicarán en el próximo cierre.</div>
```

---

## 📱 Responsive (móvil)

El diseño se adapta automáticamente:

- **Desktop (>992px):** Sidebar visible fijo a la izquierda
- **Tablet/Móvil (<992px):** Sidebar oculto, se abre con botón hamburger
- **Móvil (<576px):** Tarjetas apiladas, tablas con scroll horizontal

---

## 🎀 Principios del diseño kawaii de Tina Store

1. **Suave pero profesional** — No infantil, sí amigable
2. **Rosa sin exceso** — El rosa es el acento, el blanco es el fondo
3. **Bordes redondeados** — En botones, tarjetas y badges
4. **Sombras ligeras** — Profundidad sin peso visual
5. **Espacio cómodo** — Padding generoso, nada apretado
6. **Colores con significado** — Cada color indica un estado claro
7. **Emojis decorativos** — Solo en el sidebar y títulos de sección, no en datos

---

## 🔧 Cómo agregar nuevas páginas siguiendo el diseño

```razor
@page "/nueva-pagina"

<PageTitle>Título — Tina Store</PageTitle>

<div class="d-flex justify-content-between align-items-center mb-4">
	<h1 class="fw-bold" style="color: var(--ts-primary-dark);">
		<i class="bi bi-icon-aqui me-2" style="color: var(--ts-primary);"></i>
		Título de la Página
	</h1>
	<button class="btn btn-primary">
		<i class="bi bi-plus-lg me-1"></i> Nueva acción
	</button>
</div>

<div class="ts-card">
	<!-- Contenido de la página -->
</div>
```

# 🛍️ Tina Store — Sistema de Gestión y Facturación

> Sistema de administración completo para **Tina Store**: facturación, inventario, clientes, proveedores, cuentas por cobrar, egresos, reportes y más.

---

## 📋 ¿Qué es Tina Store?

Tina Store es una aplicación web desarrollada en **.NET 10** con **Blazor** que permite administrar de forma completa una tienda minorista. Nació para resolver la necesidad de tener todo el control del negocio en un solo lugar: desde registrar una venta hasta generar el PDF de la factura, controlar el stock y saber cuánto te deben los clientes.

---

## 🌸 Características del sistema

| Módulo | Descripción |
|---|---|
| 🧾 **Facturación** | Crear, editar y anular facturas. Generación de PDF. |
| 📦 **Inventario** | Productos, categorías, control de stock, alertas de bajo stock. |
| 👥 **Clientes** | Registro, historial de compras, estado de deudas. |
| 🚚 **Proveedores** | Gestión de proveedores y relación con productos. |
| 💰 **Cuentas por cobrar** | Control de pagos pendientes y vencidos. |
| 💸 **Egresos** | Registro de gastos y salidas de dinero. |
| 📊 **Reportes** | Ventas, ingresos, egresos, deudas. |
| 📥 **Importación Excel** | Carga masiva de productos y clientes. |
| 🔐 **Autenticación** | Login con JWT, usuarios y roles. |
| ⚙️ **Configuración** | Datos de la tienda, métodos de pago. |

---

## 🛠️ Tecnologías usadas

| Capa | Tecnología |
|---|---|
| Frontend | Blazor Web App (.NET 10, Server-Side) |
| Backend / API | ASP.NET Core Web API (.NET 10) |
| Base de datos (dev) | SQLite + Entity Framework Core |
| Base de datos (prod) | PostgreSQL (recomendado) |
| Autenticación | JWT (JSON Web Tokens) |
| PDF | QuestPDF |
| Excel | ClosedXML / EPPlus |
| Logs | Serilog |
| Estilos | Bootstrap 5 + CSS personalizado (tema kawaii rosado 🌸) |
| Tipografía | Nunito (Google Fonts) |
| Iconos | Bootstrap Icons |
| Tests | xUnit + NUnit |

---

## 📁 Estructura del proyecto

```
TinaStoreFacturacion/
├── src/
│   ├── TinaStore.Api/           → API REST con todos los endpoints
│   ├── TinaStore.Web/           → Frontend Blazor (panel administrativo)
│   ├── TinaStore.Application/   → Lógica de negocio y casos de uso
│   ├── TinaStore.Domain/        → Entidades y reglas del dominio
│   └── TinaStore.Infrastructure/→ Base de datos, repositorios, servicios externos
├── tests/
│   ├── TinaStore.Tests.Unit/    → Tests unitarios
│   └── TinaStore.Tests.Integration/ → Tests de integración
├── docs/                        → Documentación del proyecto
├── .gitignore                   → Archivos excluidos del repositorio
└── README.md                    → Este archivo
```

---

## 🚀 Instalación y ejecución local

Ver la guía completa en [`docs/INSTALLATION.md`](docs/INSTALLATION.md)

**Resumen rápido:**

```powershell
# 1. Clonar el repositorio
git clone https://github.com/tuusuario/TinaStoreFacturacion.git
cd TinaStoreFacturacion

# 2. Configurar secretos de desarrollo
cd src/TinaStore.Api
dotnet user-secrets set "Jwt:Key" "mi-clave-de-desarrollo-local-32chars"

# 3. Aplicar migraciones
dotnet ef database update --project src/TinaStore.Infrastructure --startup-project src/TinaStore.Api

# 4. Ejecutar la API
dotnet run --project src/TinaStore.Api

# 5. En otra terminal, ejecutar el frontend
dotnet run --project src/TinaStore.Web
```

---

## 🌐 Ambientes

| Ambiente | Descripción | Base de datos |
|---|---|---|
| `Development` | Local en tu computadora | SQLite (`tinastore-dev.db`) |
| `Staging` | Servidor de pruebas | PostgreSQL de prueba |
| `Production` | Servidor real de la tienda | PostgreSQL de producción |

Ver detalles en [`docs/ENVIRONMENTS.md`](docs/ENVIRONMENTS.md)

---

## 📦 Publicar en producción

Ver la guía completa en [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md)

```powershell
# Publicar la API en modo Release
dotnet publish src/TinaStore.Api -c Release -o ./publish/api

# Publicar el Frontend
dotnet publish src/TinaStore.Web -c Release -o ./publish/web
```

---

## 🌿 Ramas y versionamiento

| Rama | Propósito |
|---|---|
| `master` | Versión estable de producción |
| `develop` | Desarrollo activo |
| `feature/*` | Nuevas funcionalidades |
| `bugfix/*` | Corrección de errores |
| `release/*` | Preparación de versión |
| `hotfix/*` | Correcciones urgentes en producción |

Ver la estrategia completa en [`docs/VERSIONING.md`](docs/VERSIONING.md)

---

## 🎨 Diseño visual

Tina Store usa un tema visual **kawaii rosado** 🌸:
- Paleta: rosa principal `#F472B6`, vino sidebar `#4A1942`, crema fondo `#FFF1F8`
- Tipografía: **Nunito** (Google Fonts)
- Estilo: botones redondeados, tarjetas suaves, badges de colores por estado

Ver la guía completa en [`docs/DESIGN_SYSTEM.md`](docs/DESIGN_SYSTEM.md)

---

## ✅ Checklist antes de publicar

Ver [`docs/RELEASE_CHECKLIST.md`](docs/RELEASE_CHECKLIST.md)

---

## 🔒 Seguridad

- ❌ **Nunca** subas archivos `.db` al repositorio
- ❌ **Nunca** pongas contraseñas en el código
- ✅ Usa `dotnet user-secrets` en desarrollo
- ✅ Usa variables de entorno en producción
- ✅ El `.gitignore` ya excluye archivos sensibles

---

## 📌 Versiones

| Versión | Estado | Descripción |
|---|---|---|
| `v0.1.0` | ✅ En progreso | Base técnica: estructura, login, layout kawaii |
| `v0.2.0` | ⏳ Pendiente | Clientes e inventario completo |
| `v0.3.0` | ⏳ Pendiente | Facturación + PDF |
| `v0.4.0` | ⏳ Pendiente | Cuentas por cobrar |
| `v1.0.0` | 🎯 Meta | Primera versión estable para la tienda |

---

## 📄 Licencia

Proyecto privado — Tina Store © 2025. Todos los derechos reservados.

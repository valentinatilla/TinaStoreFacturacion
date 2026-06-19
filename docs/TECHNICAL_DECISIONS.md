# TECHNICAL DECISIONS — Tina Store

Registro de decisiones técnicas importantes tomadas durante el desarrollo y mantenimiento del proyecto.
Esto sirve para entender *por qué* se hizo algo de cierta manera, y cómo revertirlo o cambiarlo en el futuro.

---

## TD-Fase-A1 — Decisiones técnicas de la Fase A1 (v1.1.0 — 2025-06-18)

### TD-A1-01 — StatusName en español a nivel de servicio, no de DTO

**Decisión**: Traducir `InvoiceStatus` a español dentro del método de mapeo del servicio (`StatusEnEspanol()`), no como una propiedad del enum ni como una extensión.

**Alternativas consideradas**:
1. Agregar atributos `[Description]` al enum → requiere reflexión en runtime.
2. Método de extensión en el dominio → acopla el dominio al idioma.
3. ✅ Método estático privado en el servicio de aplicación → limpio, sin dependencias nuevas.

**Impacto**: Consistente en todos los endpoints que usan `InvoiceSummaryDto` e `InvoiceDto`.

---

### TD-A1-02 — Lectura de imágenes con MemoryStream en Blazor Server

**Decisión**: Usar `MemoryStream + CopyToAsync` en lugar de `ReadAsync(buffer)` para leer archivos en Blazor Server.

**Razón técnica**: `Stream.ReadAsync` puede completar con menos bytes de los solicitados en una sola llamada (comportamiento estándar de .NET). `CopyToAsync` garantiza copia completa independientemente del tamaño. En Blazor Server el stream tiene un tiempo de vida limitado al evento, por lo que también se deben abrir streams separados para preview y upload.

---

### TD-A1-03 — Pipeline HTTP de la API sin UseHttpsRedirection en desarrollo

**Decisión**: Eliminar `UseHttpsRedirection()` del pipeline de la API para desarrollo local.

**Razón**: El cliente Web Blazor en desarrollo puede ejecutarse en HTTP. La redirección forzada a HTTPS causaba que peticiones válidas de desarrollo fueran rechazadas o redirigidas en un bucle. En producción, HTTPS debe gestionarse a nivel de proxy inverso (Nginx, Azure App Service, etc.), no en la capa de aplicación.

---

## TD-01 — Arquitectura: Clean Architecture con API separada

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-17 |
| **Versión** | 0.1.0 |
| **Autor** | Equipo Tina Store |

### Decisión
Separar la aplicación en dos proyectos independientes:
- `TinaStore.Api`: ASP.NET Core Web API REST
- `TinaStore.Web`: Blazor Server que consume la API

Capas internas: Domain → Application → Infrastructure → Api/Web.

### Razón
- Permite escalar la API de forma independiente.
- Facilita agregar clientes adicionales en el futuro (móvil, otro web).
- Separación clara de responsabilidades.

### Consecuencias
- La autenticación debe manejarse en la API (JWT) y el Web guarda el token en sesión.
- Las URLs de la API deben ser correctas en cada ambiente (ver BUG-01).
- Cualquier cambio en la API requiere también actualizar los DTOs del cliente Web.

---

## TD-02 — Base de datos: SQLite con EF Core

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-17 |
| **Versión** | 0.1.0 |

### Decisión
Usar SQLite como base de datos principal en desarrollo y primera versión de producción.

### Razón
- Sin servidor de base de datos adicional para instalar.
- Suficiente para una aplicación de un solo usuario.
- Fácil respaldo (copiar el archivo `.db`).

### Consecuencias
- Para escalar a múltiples usuarios o mayor carga, migrar a PostgreSQL o SQL Server.
- El archivo `tinastore.db` o `tinastore-dev.db` es la base de datos. **Debe respaldarse regularmente**.
- No soportar múltiples escrituras concurrentes de forma eficiente.

### Cómo migrar a otro motor
1. Cambiar `UseSqlite()` por `UseNpgsql()` o `UseSqlServer()` en `Infrastructure/DependencyInjection.cs`.
2. Actualizar el connection string en `appsettings.json` y variables de entorno.
3. Ejecutar `dotnet ef migrations add MigrationName` y `dotnet ef database update`.

---

## TD-03 — Autenticación: JWT propio (no Identity completo)

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-17 |
| **Versión** | 0.1.0 |

### Decisión
Usar JWT (JSON Web Token) generado por la propia API, con hash de contraseñas via `IPasswordHasher<User>` de ASP.NET Core Identity (sin usar el sistema completo de Identity).

### Razón
- Mayor control sobre el flujo de autenticación.
- Evitar la complejidad de Identity completo para una app de usuario único.
- Compatible con la adición futura de Google OAuth.

### Consecuencias
- Los tokens expiran (configurado en `appsettings.json → Jwt:ExpiresInMinutes`).
- En desarrollo: 1440 min (24 horas). En producción: 480 min (8 horas).
- La sesión actual es **en memoria** del circuito Blazor. Se pierde al reiniciar el servidor.
- **Pendiente**: persistencia de sesión en cookie segura (Fase 4).

---

## TD-04 — URL base de API: HTTPS obligatorio en desarrollo

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-18 |
| **Versión** | 0.2.0 |
| **Relacionado** | BUG-01 |

### Decisión
`ApiBaseUrl` en `appsettings.Development.json` del Web debe apuntar **siempre a HTTPS** (`https://localhost:7073`), no a HTTP.

### Razón
La API tiene habilitada la redirección HTTPS (`app.UseHttpsRedirection()`). Llamar a HTTP causa un redirect 307 que elimina el header `Authorization`, provocando 401 en todas las peticiones autenticadas.

### Regla
- Desarrollo: `ApiBaseUrl = "https://localhost:7073"`
- Producción: configurar via variable de entorno `ApiBaseUrl`
- **Nunca** usar HTTP como URL base cuando la API tenga HTTPS activado.

---

## TD-05 — Simplificación de roles (decisión pendiente de implementar)

| Campo | Detalle |
|---|---|
| **Fecha** | 2026-06-18 |
| **Versión** | Pendiente Fase 4 |
| **Estado** | 📋 Documentado, no implementado aún |

### Contexto
La aplicación tiene un enum `UserRole` con valores `Admin`, `Seller`, `Viewer`. Para el caso de uso actual (un solo usuario), este sistema es innecesariamente complejo.

### Decisión propuesta
- Mantener el enum `UserRole` en el código (sin eliminarlo) para poder escalar en el futuro.
- La autorización real pasará a verificar si el email autenticado está en una lista de emails autorizados (configurable en `appsettings.json`).
- Ocultar o simplificar el módulo de Usuarios en la interfaz.

### Cómo reactivar roles en el futuro
1. Agregar atributos `[Authorize(Roles = "Admin")]` en los controladores correspondientes.
2. Incluir el claim de rol en el token JWT (ya se hace en `TokenService.cs`).
3. Agregar lógica de autorización por rol en los componentes Blazor con `AuthorizeView`.

---

## TD-Fase-A2-B — Decisiones técnicas de las Fases A2 y B (v1.2.0 — 2026-06-18)

### TD-A2-01 — Fórmula de ProfitMargin sobre PurchasePrice (costo)

**Decisión**: `ProfitMargin = (SalePrice - PurchasePrice) / PurchasePrice × 100`

**Razón**: La fórmula anterior usaba `SalePrice` como divisor (margen bruto sobre venta). La fórmula correcta para *porcentaje de ganancia sobre el costo* usa `PurchasePrice`. Esta es la métrica más intuitiva para un negocio de retail.

**Costo = 0**: Devuelve 0, no lanza excepción. Se muestra "No calculable" en la UI.

**Tipo de costo**: Costo fijo de adquisición (no promedio ponderado). Esta decisión es suficiente para la v1.x. En versiones futuras se puede implementar costo promedio ponderado (PEPS/UEPS).

---

### TD-A2-02 — Estado de inventario separado del estado administrativo

**Decisión**: Mantener dos conceptos independientes en `Product`:
- `IsActive` (bool): estado **administrativo**. Lo establece el usuario manualmente.
- Estado de inventario (calculado en UI): **Agotado**, **Bajo stock**, **Activo** — derivado de `CurrentStock` y `MinimumStock`.

**Razón**: Un producto puede estar activo pero agotado temporalmente. Fusionarlos causaría problemas al reponer stock. El estado administrativo no se modifica automáticamente por stock.

---

### TD-A2-03 — Estado comercial de cliente totalmente automático

**Decisión**: `CommercialStatus` se calcula en `CustomerService.ToDto()` sin intervención manual.

**Regla**: Activo si `LastPurchaseDate >= DateTime.UtcNow.AddMonths(-6)`. Inactivo si no. Sin compras si `Invoices` está vacío.

**Alternativas descartadas**:
1. Campo manual en BD → riesgo de desincronización.
2. Cron job nocturno → complejidad innecesaria en v1.x.
3. ✅ Calculado al consultar → siempre consistente, costo mínimo.

**Estado administrativo `IsActive`**: Se mantiene separado. El usuario puede desactivar un cliente manualmente sin importar su historial de compras.

---

### TD-A2-04 — Columna IsActive de Proveedores conservada en BD

**Decisión**: Ocultar `IsActive` de proveedores en la UI sin eliminarlo de la base de datos.

**Razón**: No se requiere gestión de estado de proveedores en v1.x. Eliminar la columna requeriría una migración de BD con riesgo de pérdida de datos. La columna permanece con valor `true` por defecto para todos los proveedores.

**Reactivación futura**: Agregar la columna al formulario de edición y al filtro de listado en `Proveedores/Index.razor`.

---

### TD-A2-05 — Logo en sidebar cargado desde API al iniciar sesión

**Decisión**: `MainLayout.razor` llama a `GET /api/settings` en `OnInitializedAsync` para obtener `LogoPath`.

**Trade-off**: Agrega una petición HTTP adicional al cargar el layout. Es aceptable porque el endpoint es rápido y se realiza solo una vez por sesión.

**Sin caché local**: En v1.x no se cachea el logo en sesión. Si la API no responde, se muestra el emoji predeterminado sin error visible al usuario.

---

### TD-A2-06 — WhatsApp mediante enlace "click to chat" (sin API de pago)

**Decisión**: Usar `https://wa.me/{telefono}?text={mensaje}` para abrir WhatsApp con mensaje prellenado.

**Ventajas**: Gratuito, sin registro, sin API key. El usuario revisa y confirma el envío desde WhatsApp.

**Limitaciones documentadas**:
- No se puede confirmar si el mensaje fue enviado.
- Requiere que el usuario tenga WhatsApp instalado o use WhatsApp Web.
- No apto para envíos masivos.

**Integración futura con API oficial**: Requeriría registro en Meta Business, número de teléfono verificado y posiblemente costo por mensaje. La arquitectura actual permite agregar un servicio `IWhatsAppService` que use la API oficial sin modificar la lógica de negocio.

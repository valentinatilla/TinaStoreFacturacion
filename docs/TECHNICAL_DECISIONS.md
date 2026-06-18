# TECHNICAL DECISIONS — Tina Store

Registro de decisiones técnicas importantes tomadas durante el desarrollo y mantenimiento del proyecto.
Esto sirve para entender *por qué* se hizo algo de cierta manera, y cómo revertirlo o cambiarlo en el futuro.

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

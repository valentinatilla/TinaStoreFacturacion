# 🌐 Guía de Ambientes — Tina Store

Esta guía explica cómo manejar correctamente los tres ambientes del sistema: Desarrollo, Staging (Pruebas) y Producción.

---

## 📖 ¿Qué es un ambiente?

Un **ambiente** (también llamado **entorno**) es una configuración del sistema para un propósito específico. Cada ambiente tiene su propia base de datos, sus propias configuraciones y sus propios datos.

Tina Store tiene tres ambientes:

| Ambiente | Nombre en .NET | ¿Para qué sirve? | ¿Datos reales? |
|---|---|---|---|
| Desarrollo | `Development` | Tu computadora local | ❌ Solo datos de prueba |
| Pruebas | `Staging` | Servidor de prueba externo | ❌ Solo datos de prueba |
| Producción | `Production` | Servidor real de la tienda | ✅ Datos reales de Tina Store |

> ⚠️ **Regla de oro:** Nunca uses datos reales de clientes en el ambiente de desarrollo o staging.

---

## 🏠 Ambiente de Desarrollo (Development)

Este ambiente es para trabajar en tu computadora personal.

### Características:
- Base de datos SQLite local (`tinastore-dev.db`)
- Logs detallados (debug) en la consola
- JWT con clave de desarrollo simple
- Correos simulados (no llegan a nadie)
- Hot-reload activado (los cambios se ven al instante)

### Cómo activarlo:
.NET lo activa automáticamente cuando corres con `dotnet run` desde Visual Studio.
El archivo que usa es `appsettings.Development.json`.

### Datos de prueba (seed data):
Cuando ejecutes las migraciones por primera vez, el sistema puede cargar datos de ejemplo:
- Usuarios de prueba (admin@tinastore.local / password de prueba)
- Productos ficticios
- Clientes ficticios
- Facturas de ejemplo

---

## 🧪 Ambiente de Staging (Pruebas en servidor)

Este ambiente es una copia del sistema real pero con datos de prueba, en un servidor externo.

### Características:
- Base de datos PostgreSQL de prueba (separada de la real)
- Correos controlados (van a una bandeja de prueba, no a clientes reales)
- Configuración idéntica a producción
- Se usa para verificar que todo funciona antes de publicar

### Cómo activarlo:
Configurar la variable de entorno en el servidor:
```
ASPNETCORE_ENVIRONMENT = Staging
```

### Variables de entorno necesarias en staging:
```
ASPNETCORE_ENVIRONMENT = Staging
ConnectionStrings__DefaultConnection = (cadena de BD de prueba)
Jwt__Key = (clave JWT de staging — diferente a producción)
Email__SmtpHost = (servidor de correo de prueba)
Email__SmtpUser = (correo de prueba)
Email__SmtpPassword = (contraseña del correo de prueba)
```

---

## 🏭 Ambiente de Producción (Production)

Este es el sistema real que usa Tina Store con datos reales de clientes, ventas y productos.

### Características:
- Base de datos PostgreSQL real con copias de seguridad
- Correos reales que llegan a los clientes
- HTTPS obligatorio
- Logs mínimos (solo errores y advertencias importantes)
- Variables de entorno con secretos reales

### Cómo activarlo:
```
ASPNETCORE_ENVIRONMENT = Production
```

### Variables de entorno obligatorias en producción:
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = Host=servidor;Database=tinastore_prod;Username=usuario;Password=contraseña
Jwt__Key = (clave JWT aleatoria de 48+ caracteres)
Jwt__Issuer = TinaStore
Jwt__Audience = TinaStoreClients
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUser = tinastore@gmail.com
Email__SmtpPassword = (contraseña de aplicación)
Email__FromAddress = tinastore@gmail.com
Email__FromName = Tina Store
```

**Generar clave JWT segura:**
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

---

## 📁 Archivos de configuración por ambiente

| Archivo | ¿Se sube al repo? | ¿Contiene secretos? | Propósito |
|---|---|---|---|
| `appsettings.json` | ✅ Sí | ❌ No | Valores base compartidos |
| `appsettings.Development.json` | ✅ Sí | ⚠️ Solo claves de dev | Configuración local |
| `appsettings.Staging.json` | ❌ No (en .gitignore) | ⚠️ Referencia | Configuración de pruebas |
| `appsettings.Production.json` | ❌ No (en .gitignore) | ⚠️ Referencia | Configuración de producción |

---

## 🗄️ Estrategia de bases de datos

| Ambiente | Motor actual | Nombre | Ubicación |
|---|---|---|---|
| Desarrollo | SQLite | `tinastore-dev.db` | En el proyecto local (en .gitignore) |
| Staging | SQLite o PostgreSQL | `tinastore-staging.db` / `tinastore_staging` | Servidor de pruebas |
| Producción | SQLite (actual) / PostgreSQL (recomendado) | `tinastore.db` / `tinastore_prod` | Servidor de producción |

> **Estado actual**: la aplicación usa SQLite en todos los ambientes. SQLite es válido para un solo usuario o carga baja. Para múltiples usuarios concurrentes o alta disponibilidad, migrar a PostgreSQL cambiando solo la cadena de conexión (EF Core no requiere cambios en el código).

### Migrar de SQLite a PostgreSQL (opcional, para producción con alta carga):

1. Instalar el paquete de PostgreSQL para EF Core:
   ```powershell
   dotnet add src/TinaStore.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. En `DependencyInjection.cs` de Infrastructure, cambiar `UseSqlite` por `UseNpgsql`:
   ```csharp
   options.UseNpgsql(connectionString)
   ```

3. Cambiar la cadena de conexión en el servidor:
   ```
   Host=servidor;Port=5432;Database=tinastore_prod;Username=usuario;Password=contraseña
   ```

4. Generar nueva migración inicial para PostgreSQL y aplicarla:
   ```powershell
   dotnet ef database update --project src/TinaStore.Infrastructure --startup-project src/TinaStore.Api
   ```

---

## 💾 Estrategia de Backups

### En desarrollo:
No es crítico. El archivo `tinastore-dev.db` tiene solo datos de prueba.

### En producción (CRÍTICO):
- **Frecuencia:** Backup automático diario
- **Retención:** Guardar los últimos 30 backups
- **Lugar:** Carpeta separada del servidor o servicio de almacenamiento en la nube

**Backup manual de PostgreSQL:**
```powershell
# Crear backup
pg_dump -h localhost -U postgres tinastore_prod > backup_$(Get-Date -Format "yyyyMMdd_HHmmss").sql

# Restaurar backup
psql -h localhost -U postgres tinastore_prod < backup_20251201_120000.sql
```

### Si Railway incluye PostgreSQL:
Railway hace backups automáticos en el plan Hobby ($5/mes). Puedes restaurar desde el dashboard.

---

## 🔒 Reglas de seguridad entre ambientes

```
❌ Nunca copiar datos reales de producción a desarrollo
❌ Nunca usar la misma clave JWT en todos los ambientes
❌ Nunca compartir la contraseña de la BD de producción con el equipo de desarrollo
✅ Usar datos ficticios en desarrollo y staging
✅ Tener claves JWT diferentes por ambiente
✅ Hacer backup antes de cada migración en producción
✅ Probar en staging antes de publicar en producción
```

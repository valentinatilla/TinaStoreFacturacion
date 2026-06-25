# 🚀 Guía de Despliegue — Tina Store

Esta guía explica cómo publicar Tina Store en un servidor externo (hosting) para que funcione en internet.

---

## 📋 Requisitos antes de desplegar

Antes de publicar, verifica:

- [ ] El proyecto compila sin errores: `dotnet build -c Release`
- [ ] Todos los tests pasan: `dotnet test`
- [ ] La clave JWT de producción tiene al menos 32 caracteres aleatorios
- [ ] Las variables de entorno están configuradas en el servidor
- [ ] La base de datos de producción está creada y accesible
- [ ] El `.gitignore` excluye todos los archivos sensibles
- [ ] Revisaste el `docs/RELEASE_CHECKLIST.md`

---

## 🌐 Ambientes disponibles

| Variable de entorno | Valor | Efecto |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Usa `appsettings.Development.json` |
| `ASPNETCORE_ENVIRONMENT` | `Staging` | Usa `appsettings.Staging.json` |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Usa `appsettings.Production.json` |

---

## 🔑 Variables de entorno necesarias en producción

Configura estas variables directamente en tu servidor o plataforma de hosting:

```
ASPNETCORE_ENVIRONMENT = Production

# Base de datos (reemplaza con tu cadena de conexión real)
ConnectionStrings__DefaultConnection = Host=...;Database=tinastore;Username=...;Password=...

# JWT (genera una clave segura con el comando de abajo)
Jwt__Key = (clave aleatoria de 48+ caracteres)
Jwt__Issuer = TinaStore
Jwt__Audience = TinaStoreClients
Jwt__ExpiresInMinutes = 480

# Correo (para envío de facturas y notificaciones)
Email__SmtpHost = smtp.gmail.com
Email__SmtpPort = 587
Email__SmtpUser = tinastore@gmail.com
Email__SmtpPassword = (contraseña de aplicación de Gmail)
Email__FromAddress = tinastore@gmail.com
Email__FromName = Tina Store
```

**Generar una clave JWT segura en PowerShell:**
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

---

## 📦 Publicar en modo Release

Los siguientes comandos compilan la aplicación en modo optimizado para producción.

```powershell
# Publicar la API
dotnet publish src/TinaStore.Api -c Release -o ./publish/api

# Publicar el Frontend Blazor
dotnet publish src/TinaStore.Web -c Release -o ./publish/web
```

---

## 🗄️ Migraciones en producción

Antes de iniciar la aplicación en el servidor, aplica las migraciones de base de datos:

```powershell
# Con la cadena de conexión de producción configurada como variable de entorno
dotnet ef database update \
  --project src/TinaStore.Infrastructure \
  --startup-project src/TinaStore.Api
```

> **Importante:** En producción, siempre haz un **backup de la base de datos** antes de aplicar migraciones.

---

## 🏠 Opción A: Despliegue en Railway ($5/mes) — RECOMENDADO PARA PRODUCCIÓN

Railway es la opción más simple para producción. Soporta .NET via Docker.

### Pasos:

1. Crear cuenta en https://railway.app
2. Conectar tu cuenta de GitHub
3. Crear nuevo proyecto desde tu repositorio
4. Railway detectará el `Dockerfile` automáticamente
5. Agregar una base de datos PostgreSQL en Railway
6. Configurar las variables de entorno en el dashboard de Railway
7. Railway hace el deploy automáticamente con cada push a `master`

Ver guía detallada en [`docs/RAILWAY_DEPLOY.md`](./RAILWAY_DEPLOY.md).

---

## 🐳 Despliegue con Docker

El proyecto incluye un `Dockerfile` en la raíz. Para ejecutar localmente con Docker:

```powershell
# Construir la imagen de la API
docker build -f Dockerfile.api -t tinastore-api .

# Construir la imagen del Web
docker build -f Dockerfile.web -t tinastore-web .

# Ejecutar con docker-compose (desarrollo local)
docker-compose up
```

---

## ✅ Verificar que el despliegue funcionó

Después de desplegar, verifica:

```powershell
# Verificar que la API responde
curl https://tu-dominio.com/api/health

# Verificar que el login funciona
# Abre el navegador en https://tu-dominio.com y prueba el login
```

---

## 🔄 Actualizar una versión existente

```powershell
# 1. Asegúrate de estar en la rama correcta
git checkout master

# 2. Hacer backup de la base de datos (¡SIEMPRE antes de actualizar!)
# Ver sección de backups en docs/ENVIRONMENTS.md

# 3. Publicar la nueva versión
dotnet publish src/TinaStore.Api -c Release -o ./publish/api
dotnet publish src/TinaStore.Web -c Release -o ./publish/web

# 4. Aplicar migraciones si las hay
dotnet ef database update --project src/TinaStore.Infrastructure --startup-project src/TinaStore.Api

# 5. Reiniciar el servidor / servicio
```

---

## 🆘 Rollback: volver a la versión anterior si algo sale mal

```powershell
# Ver los tags de versión disponibles
git tag -l

# Volver a una versión anterior (ejemplo: v0.1.0)
git checkout v0.1.0

# Republicar esa versión
dotnet publish src/TinaStore.Api -c Release -o ./publish/api

# Restaurar el backup de la base de datos si es necesario
# (ver instrucciones en docs/ENVIRONMENTS.md)
```

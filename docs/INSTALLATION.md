# 🛠️ Guía de Instalación — Tina Store

Esta guía te explica paso a paso cómo instalar y ejecutar Tina Store en tu computadora local para desarrollo.

---

## 📋 Requisitos previos

Antes de empezar, necesitas tener instalado:

| Herramienta | Versión mínima | ¿Cómo verificar? |
|---|---|---|
| .NET SDK | 10.0 | `dotnet --version` |
| Git | Cualquier versión reciente | `git --version` |
| Visual Studio o VS Code | Recomendado: VS 2026 | — |
| SQLite | Se instala automáticamente con EF Core | — |

Para descargar .NET 10: https://dot.net/

---

## 📥 Paso 1: Clonar el repositorio

```powershell
# Clonar el repositorio desde GitHub
git clone https://github.com/tuusuario/TinaStoreFacturacion.git

# Entrar a la carpeta del proyecto
cd TinaStoreFacturacion
```

---

## 🔐 Paso 2: Configurar secretos de desarrollo

Los secretos (como la clave JWT) no están en el código por seguridad.
Debes configurarlos con `dotnet user-secrets`, que los guarda en tu computadora sin subir al repositorio.

```powershell
# Ir al proyecto de la API
cd src/TinaStore.Api

# Inicializar user-secrets (si no está inicializado)
dotnet user-secrets init

# Configurar la clave JWT para desarrollo local
# Puedes usar cualquier texto de más de 32 caracteres
dotnet user-secrets set "Jwt:Key" "TinaStore-Dev-Key-Solo-Para-Desarrollo-Local-2025!"

# Volver a la raíz del proyecto
cd ../..
```

> **¿Qué es `user-secrets`?**
> Es una forma de guardar contraseñas y claves en tu computadora personal sin escribirlas en el código.
> Los valores se guardan en `%APPDATA%\Microsoft\UserSecrets\` en Windows.
> **Nunca se suben a GitHub**.

---

## 🗄️ Paso 3: Aplicar las migraciones de base de datos

Las migraciones son instrucciones que crean la estructura de la base de datos automáticamente.

```powershell
# Aplicar las migraciones (crea el archivo tinastore-dev.db)
dotnet ef database update \
  --project src/TinaStore.Infrastructure \
  --startup-project src/TinaStore.Api

# Si el comando ef no está instalado, instálalo así:
dotnet tool install --global dotnet-ef
```

---

## ▶️ Paso 4: Ejecutar la aplicación

La aplicación tiene dos partes: la **API** (backend) y el **Web** (frontend Blazor).
Necesitas ejecutar ambas al mismo tiempo en terminales separadas.

**Terminal 1 — API:**
```powershell
dotnet run --project src/TinaStore.Api
# La API queda disponible en: http://localhost:5172
```

**Terminal 2 — Frontend Blazor:**
```powershell
dotnet run --project src/TinaStore.Web
# El panel queda disponible en: http://localhost:5173 o https://localhost:7173
```

Abre el navegador en la dirección del frontend para ver el panel de Tina Store.

---

## 🧪 Paso 5: Ejecutar los tests

```powershell
# Ejecutar todos los tests del proyecto
dotnet test

# Ejecutar solo los tests unitarios
dotnet test tests/TinaStore.Tests.Unit

# Ejecutar solo los tests de integración
dotnet test tests/TinaStore.Tests.Integration
```

---

## 🔄 Paso 6: Trabajar con la base de datos

```powershell
# Ver las migraciones existentes
dotnet ef migrations list \
  --project src/TinaStore.Infrastructure \
  --startup-project src/TinaStore.Api

# Crear una nueva migración cuando cambias el modelo
dotnet ef migrations add NombreDeLaMigracion \
  --project src/TinaStore.Infrastructure \
  --startup-project src/TinaStore.Api

# Revertir la última migración
dotnet ef migrations remove \
  --project src/TinaStore.Infrastructure \
  --startup-project src/TinaStore.Api
```

---

## ⚠️ Problemas comunes

| Problema | Solución |
|---|---|
| `dotnet ef` no reconocido | Instalar: `dotnet tool install --global dotnet-ef` |
| Error de puerto en uso | Cambiar el puerto en `Properties/launchSettings.json` |
| Error de JWT inválido | Verificar que configuraste el secret con `dotnet user-secrets` |
| La base de datos no existe | Ejecutar `dotnet ef database update` del Paso 3 |
| CORS bloqueado | Verificar que el puerto del Web coincide en `appsettings.Development.json` del API |

---

## 📂 Archivos importantes de configuración

| Archivo | Descripción |
|---|---|
| `src/TinaStore.Api/appsettings.json` | Configuración base de la API |
| `src/TinaStore.Api/appsettings.Development.json` | Configuración de desarrollo local |
| `src/TinaStore.Web/appsettings.json` | Configuración base del frontend |
| `src/TinaStore.Web/appsettings.Development.json` | URL de la API en desarrollo |

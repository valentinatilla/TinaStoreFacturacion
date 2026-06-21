# 🚂 Guía de despliegue en Railway — TinaStore

## Arquitectura en Railway

```
┌─────────────────────────────────────────────────────────┐
│                    Railway Project                      │
│                                                         │
│  ┌──────────────┐   HTTP interno   ┌─────────────────┐  │
│  │  tinastore-  │ ──────────────►  │  tinastore-api  │  │
│  │     web      │                  │  (ASP.NET API)  │  │
│  │ (Blazor Srv) │                  │                 │  │
│  └──────────────┘                  └────────┬────────┘  │
│                                             │           │
│                                    ┌────────▼────────┐  │
│                                    │  Volume /app/   │  │
│                                    │    data/        │  │
│                                    │  (SQLite DB)    │  │
│                                    └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

- **tinastore-web**: Blazor Server, accesible por el usuario final.
- **tinastore-api**: API REST, solo llamada internamente por el Web.
- **Volumen Railway**: Persistencia de `tinastore.db` entre reinicios.

---

## Requisitos previos

1. Cuenta en [railway.app](https://railway.app)
2. Repositorio en GitHub (ya tienes: `github.com/valentinatilla/TinaStoreFacturacion`)
3. Railway CLI (opcional, para deploy manual): `npm install -g @railway/cli`

---

## Paso 1 — Crear el proyecto Railway

1. Ve a [railway.app/new](https://railway.app/new)
2. Selecciona **"Deploy from GitHub repo"**
3. Conecta tu cuenta GitHub y selecciona `TinaStoreFacturacion`
4. Railway detectará el repo. **No configures nada todavía** — primero ajusta los servicios.

---

## Paso 2 — Configurar el servicio de la API

1. En el proyecto Railway, haz clic en **"+ New Service"** → **"GitHub Repo"**
2. Selecciona el mismo repositorio
3. En la pestaña **Settings** del servicio, define:
   - **Service Name**: `tinastore-api`
   - **Dockerfile Path**: `Dockerfile.api`
   - **Root Directory**: `/` (raíz del repo)
4. Ve a la pestaña **Variables** y añade:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/tinastore.db` |
| `Jwt__Key` | _(ver cómo generar abajo)_ |
| `Jwt__Issuer` | `TinaStore` |
| `Jwt__Audience` | `TinaStoreClients` |
| `App__AdminEmail` | `tu-correo@gmail.com` |
| `App__AdminPassword` | _(contraseña segura)_ |
| `Cors__AllowedOrigins` | _(URL del Web — la obtienes en el Paso 3)_ |
| `Google__ClientId` | _(opcional — tu ClientId de Google Cloud)_ |
| `Google__ClientSecret` | _(opcional — tu ClientSecret de Google)_ |
| `Google__AllowedEmails` | _(opcional — tu correo Google)_ |

5. Ve a la pestaña **Volumes** y añade un volumen:
   - **Mount Path**: `/app/data`
   - Esto persiste la base de datos SQLite entre reinicios y redeploys.

6. Railway generará automáticamente una URL pública como `https://tinastore-api-<hash>.up.railway.app`. Cópiala.

---

## Paso 3 — Configurar el servicio Web

1. Añade otro servicio → mismo repositorio
2. En **Settings**:
   - **Service Name**: `tinastore-web`
   - **Dockerfile Path**: `Dockerfile.web`
   - **Root Directory**: `/`
3. Ve a **Variables** y añade:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ApiBaseUrl` | _(URL pública de la API del Paso 2)_ |
| `Google__ClientId` | _(mismo ClientId de Google Cloud)_ |
| `Google__ClientSecret` | _(mismo ClientSecret de Google)_ |

4. Railway generará la URL del Web, por ejemplo `https://tinastore-web-<hash>.up.railway.app`.

---

## Paso 4 — Actualizar CORS en la API

Una vez que tengas la URL del Web, vuelve a la API → **Variables** y actualiza:

```
Cors__AllowedOrigins = https://tinastore-web-<hash>.up.railway.app
```

Luego haz **Redeploy** en el servicio API.

---

## Paso 5 — Configurar Google OAuth (si lo usas)

1. Ve a [Google Cloud Console](https://console.cloud.google.com)
2. En tu proyecto → **APIs & Services** → **Credentials**
3. En el OAuth 2.0 Client, añade a **Authorized redirect URIs**:
   ```
   https://tinastore-web-<hash>.up.railway.app/auth/google-callback
   ```
4. Guarda y espera unos minutos a que se propague.

---

## Cómo generar la clave JWT segura

En PowerShell local:
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

Copia el resultado (64 caracteres en base64) y úsalo como valor de `Jwt__Key`.

---

## Proceso de deploy continuo

Cada vez que hagas `git push` a `master` (o al branch que configures en Railway), Railway reconstruye y redespliega automáticamente los servicios.

Para desplegar manualmente desde `develop`:
```bash
# Con Railway CLI
railway up --service tinastore-api
railway up --service tinastore-web
```

---

## Verificar que todo funciona

1. Accede a la URL del Web (`https://tinastore-web-*.up.railway.app`)
2. Verifica que el login funciona con tu usuario administrador
3. Verifica el login con Google (si lo configuraste)
4. Accede a `https://tinastore-api-*.up.railway.app/` — debe responder:
   ```json
   {"message": "TinaStore API funcionando ✓", "version": "1.0"}
   ```

---

## Notas importantes

- **SQLite en producción**: Para un uso personal (un solo usuario) es perfectamente válido. Si en el futuro necesitas más capacidad, consulta `docs/ENVIRONMENTS.md` para migrar a PostgreSQL.
- **Logs**: Railway captura todo `stdout`/`stderr`. Revísalos en la pestaña **Logs** de cada servicio.
- **Costos**: El plan gratuito de Railway incluye 500 horas/mes de ejecución. Para un uso personal continuo, considera el plan Hobby (~$5/mes) que incluye uso ilimitado.
- **Primer arranque**: La API creará automáticamente la base de datos y el usuario admin al iniciar por primera vez.

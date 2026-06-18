# ✅ Checklist de Publicación — Tina Store

Usa esta lista **SIEMPRE** antes de publicar una versión o hacer merge a `master`.
Cada punto debe estar verificado antes de continuar.

---

## 🔐 Seguridad (CRÍTICO — nunca saltar estos puntos)

- [ ] La clave JWT de producción tiene al menos 32 caracteres aleatorios
- [ ] La clave JWT **NO** está escrita en ningún archivo del repositorio
- [ ] Las contraseñas de base de datos NO están en el código
- [ ] Las contraseñas de correo NO están en el código
- [ ] Los archivos `appsettings.Production.json` y `appsettings.Staging.json` NO están en GitHub
- [ ] El archivo `.db` (base de datos SQLite) NO está en GitHub
- [ ] Los archivos de logs NO están en GitHub
- [ ] El `.gitignore` está actualizado y funcionando
- [ ] Verificar con `git status` que no hay archivos sensibles pendientes de commit
- [ ] HTTPS está activado en el servidor de producción

---

## 🧪 Calidad del código

- [ ] El proyecto compila sin errores: `dotnet build -c Release`
- [ ] Todos los tests unitarios pasan: `dotnet test tests/TinaStore.Tests.Unit`
- [ ] Todos los tests de integración pasan: `dotnet test tests/TinaStore.Tests.Integration`
- [ ] No hay warnings de seguridad en los paquetes NuGet
- [ ] No hay código comentado innecesario ni TODOs críticos
- [ ] Los logs en producción están en nivel `Warning` o superior (no `Debug`)

---

## 🗄️ Base de datos

- [ ] **Se hizo un BACKUP de la base de datos antes de proceder**
- [ ] Las migraciones pendientes están creadas y probadas
- [ ] Las migraciones se probaron en staging antes de aplicar en producción
- [ ] Los datos de prueba (seed) no están mezclados con datos reales
- [ ] La cadena de conexión de producción apunta a la base de datos correcta

---

## 🌿 Git y versionamiento

- [ ] Estás en la rama correcta (`release/vX.X.X` o `master`)
- [ ] Los commits tienen mensajes descriptivos y siguen la convención
- [ ] No hay cambios sin commitear: `git status` está limpio
- [ ] La rama `develop` está actualizada con todos los cambios
- [ ] Se hizo merge de `develop` a `release/vX.X.X` (si aplica)
- [ ] Se probó la versión en `staging` antes de publicar en `production`

---

## 🚀 Despliegue

- [ ] Las variables de entorno están configuradas en el servidor de producción
- [ ] El servidor tiene suficiente espacio en disco
- [ ] El comando de publicación se ejecutó en modo Release: `dotnet publish -c Release`
- [ ] Las migraciones se aplicaron: `dotnet ef database update`
- [ ] El servidor se reinició correctamente después del despliegue

---

## ✅ Verificación post-despliegue

- [ ] La aplicación abre correctamente en el navegador
- [ ] El login funciona con un usuario real
- [ ] Las páginas principales cargan sin errores (Dashboard, Clientes, Productos, Facturas)
- [ ] Se puede crear una factura de prueba
- [ ] Los logs del servidor no muestran errores críticos
- [ ] HTTPS está activo (el candado verde aparece en el navegador)
- [ ] La aplicación responde en menos de 3 segundos

---

## 📦 Después del despliegue exitoso

- [ ] Crear el tag de versión en Git:
  ```powershell
  git tag -a vX.X.X -m "vX.X.X: descripción de la versión"
  git push origin vX.X.X
  ```
- [ ] Documentar los cambios de esta versión en GitHub Releases
- [ ] Actualizar la tabla de versiones en `README.md` y `docs/VERSIONING.md`
- [ ] Notificar a los usuarios del sistema si aplica
- [ ] Guardar el backup en un lugar seguro etiquetado con la versión

---

## 🆘 ¿Qué hacer si algo falla después de publicar?

1. **No entres en pánico** — Tienes un backup
2. Revisar los logs del servidor para identificar el error
3. Si el error es en la aplicación: hacer rollback al commit anterior
   ```powershell
   git checkout v(versión-anterior)
   dotnet publish -c Release -o ./publish/api
   ```
4. Si el error es en la base de datos: restaurar el backup
5. Documentar qué falló y cómo se resolvió
6. Crear un `bugfix/` o `hotfix/` para la corrección

---

## 📋 Checklist específico antes de merge a `master`

- [ ] La rama `develop` tiene todos los cambios de la versión
- [ ] Se creó la rama `release/vX.X.X` desde `develop`
- [ ] Se probó la versión completa en staging
- [ ] El QA (revisión de calidad) pasó correctamente
- [ ] Se revisó el checklist completo de esta guía
- [ ] Se aprobó el merge por el responsable del proyecto

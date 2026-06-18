# =============================================================
# Tina Store — Crear nueva rama de feature desde develop
# Uso: .\scripts\nueva-feature.ps1 nombre-de-la-feature
# Ejemplo: .\scripts\nueva-feature.ps1 modulo-inventario
# =============================================================

param(
	[Parameter(Mandatory = $true)]
	[string]$Nombre
)

$branch = "feature/$Nombre"

Write-Host ""
Write-Host "🛍️  Tina Store — Nueva Feature" -ForegroundColor Magenta
Write-Host "================================" -ForegroundColor Magenta

# Asegurar que estamos en develop actualizado
Write-Host "➡️  Cambiando a develop..." -ForegroundColor Cyan
git checkout develop
git pull origin develop

# Crear y publicar la rama
Write-Host "✨ Creando rama: $branch" -ForegroundColor Yellow
git checkout -b $branch
git push -u origin $branch

Write-Host ""
Write-Host "✅ Rama '$branch' creada y publicada en GitHub." -ForegroundColor Green
Write-Host "   Cuando termines: haz commit y abre un Pull Request hacia 'develop'." -ForegroundColor Gray
Write-Host ""

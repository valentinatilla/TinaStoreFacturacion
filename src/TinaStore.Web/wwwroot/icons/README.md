# Íconos PWA — Tina Store

Esta carpeta contiene los íconos de la aplicación para que funcione como PWA
(Progressive Web App) instalable en Android e iOS.

## Archivos

| Archivo         | Tamaño    | Uso                                        |
|-----------------|-----------|--------------------------------------------|
| `icon-192.png`  | 192×192px | Ícono Android / Chrome "Agregar a inicio"  |
| `icon-512.png`  | 512×512px | Ícono splash screen Android                |

## Diseño

Generados con System.Drawing (.NET):
- **Fondo**: cuadrado #7C3AED (morado marca Tina Store)
- **Círculo central**: #F472B6 (rosa acento)
- **Letra**: "T" blanca — inicial de Tina Store
- Diseño centrado en el 80% seguro de la zona maskable (spec PWA)

## Estado

✅ Íconos generados y listos — 2026-06-22

## Regenerar

Si se cambia la identidad visual, ejecutar desde PowerShell en la raíz del repo:

```powershell
Add-Type -AssemblyName System.Drawing
# ... ver script en docs/BUGFIXES.md sección ISSUE-12
```

## Referencia en el manifest

El archivo `wwwroot/manifest.webmanifest` referencia:
- `/icons/icon-192.png`
- `/icons/icon-512.png`

# Íconos PWA — Tina Store

Esta carpeta debe contener los íconos de la aplicación para que funcione como PWA
(Progressive Web App) instalable en Android e iOS.

## Archivos necesarios

| Archivo         | Tamaño    | Uso                                        |
|-----------------|-----------|--------------------------------------------|
| `icon-192.png`  | 192×192px | Ícono Android / Chrome "Agregar a inicio"  |
| `icon-512.png`  | 512×512px | Ícono splash screen Android                |

## Cómo generarlos

1. Prepara una imagen cuadrada del logo de Tina Store (mínimo 512×512 px, fondo rosado
   o transparente).
2. Usa https://realfavicongenerator.net o https://maskable.app para generar las versiones.
3. Coloca los archivos generados en esta carpeta con los nombres exactos indicados.

## Estado actual

⚠️ Los íconos aún no han sido generados. El manifest.webmanifest referencia estas rutas
pero los archivos no existen todavía. La app funciona normalmente; solo el ícono PWA
mostrará un fallback hasta que se agreguen los archivos.

## Referencia en el manifest

El archivo `wwwroot/manifest.webmanifest` referencia:
- `/icons/icon-192.png`
- `/icons/icon-512.png`

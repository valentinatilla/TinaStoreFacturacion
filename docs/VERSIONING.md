# 🌿 Estrategia de Versionamiento — Tina Store

Esta guía explica cómo manejar Git, las ramas, los commits y las versiones del sistema de forma ordenada.

---

## 📖 Conceptos básicos (explicados para Tina Store)

### ¿Qué es un commit?
Un commit es como **tomar una foto** del estado actual de tu código en un momento específico. Cada foto tiene un mensaje que dice qué cambiaste.

Ejemplo: `feat: agregar módulo de facturación con generación de PDF`

### ¿Qué es una rama (branch)?
Una rama es una **copia paralela** de tu proyecto donde puedes trabajar sin afectar la versión estable. Cuando terminas, unes los cambios.

Ejemplo: Creas `feature/invoice-pdf`, trabajas ahí, y cuando funciona bien, lo unes a `develop`.

### ¿Qué es un merge?
Un merge es **unir dos ramas**. Combina los cambios de una rama en otra.

Ejemplo: Cuando terminas `feature/customer-management`, haces merge hacia `develop`.

### ¿Qué es un tag?
Un tag es una **etiqueta permanente** que se pone en un commit específico para marcar una versión.

Ejemplo: `v1.0.0` marcado en el commit del primer lanzamiento estable.

### ¿Qué es una release?
Una release es una **versión publicada y documentada**. Incluye qué cambió, qué se arregló y qué es nuevo. Queda como registro histórico.

---

## 🌿 Estructura de ramas

```
master          → Versión estable en producción (lo que usa la tienda real)
develop         → Versión de desarrollo (junta todas las funcionalidades terminadas)
feature/xxx     → Nueva funcionalidad en progreso
bugfix/xxx      → Corrección de un error específico
release/vX.X.X  → Preparación final antes de publicar
hotfix/xxx      → Corrección urgente directamente en producción
```

---

## 🔄 Flujo de trabajo

### Para una nueva funcionalidad:
```
1. Partir desde develop
   git checkout develop
   git pull origin develop

2. Crear la rama de la funcionalidad
   git checkout -b feature/customer-management

3. Trabajar y hacer commits
   git add .
   git commit -m "feat: agregar formulario de registro de clientes"

4. Cuando termina, volver a develop y hacer merge
   git checkout develop
   git merge feature/customer-management

5. Subir develop a GitHub
   git push origin develop
```

### Para preparar una versión:
```
1. Crear rama de release desde develop
   git checkout -b release/v0.2.0

2. Hacer pruebas finales y correcciones menores

3. Hacer merge a master y a develop
   git checkout master
   git merge release/v0.2.0
   git tag -a v0.2.0 -m "Versión 0.2.0: Clientes e inventario"
   git push origin master --tags

   git checkout develop
   git merge release/v0.2.0
   git push origin develop
```

### Para un error urgente en producción:
```
1. Partir desde master
   git checkout master
   git checkout -b hotfix/fix-invoice-total

2. Arreglar el error y hacer commit
   git commit -m "hotfix: corregir cálculo de total en factura con descuento"

3. Hacer merge a master y a develop
   git checkout master
   git merge hotfix/fix-invoice-total
   git tag -a v0.1.1 -m "Hotfix: error en total de factura"
   git push origin master --tags

   git checkout develop
   git merge hotfix/fix-invoice-total
```

---

## ✍️ Convención de commits

Formato: `tipo: descripción corta en español`

| Tipo | Cuándo usarlo | Ejemplo |
|---|---|---|
| `feat` | Nueva funcionalidad | `feat: agregar módulo de clientes` |
| `fix` | Corrección de error | `fix: corregir cálculo de total en factura` |
| `docs` | Documentación | `docs: actualizar guía de instalación` |
| `style` | Cambios visuales/CSS | `style: aplicar tema kawaii rosado al sidebar` |
| `refactor` | Reorganizar código | `refactor: separar lógica de facturación en servicios` |
| `test` | Agregar o corregir tests | `test: agregar tests de validación de cliente` |
| `chore` | Configuración, dependencias | `chore: actualizar paquetes NuGet` |
| `hotfix` | Corrección urgente | `hotfix: arreglar login de usuarios bloqueados` |

---

## 🏷️ Convención de nombres de ramas

| Tipo | Formato | Ejemplo |
|---|---|---|
| Funcionalidad | `feature/nombre-funcionalidad` | `feature/inventory-module` |
| Corrección | `bugfix/nombre-error` | `bugfix/product-import-validation` |
| Versión | `release/vX.X.X` | `release/v1.0.0` |
| Urgente | `hotfix/nombre-error` | `hotfix/login-blocked-users` |

---

## 📦 Hoja de ruta de versiones

| Versión | Qué incluye | Estado |
|---|---|---|
| `v0.1.0` | Base técnica: estructura, login, dashboard, layout kawaii | ✅ Completada |
| `v0.2.0` | Clientes e inventario (productos, categorías) | ✅ Completada |
| `v0.3.0` | Facturación básica + generación de PDF + registro de pagos | ✅ Completada |
| `v0.4.0` | Cuentas por cobrar + métodos de pago | ✅ Completada |
| `v0.5.0` | Egresos + proveedores + reportes básicos | ✅ Completada |
| `v0.6.0` | Importación/exportación Excel | ✅ Completada |
| `v0.7.0` | Configuración de tienda + administración de usuarios | ✅ Completada |
| `v0.8.0` | Cobertura de tests unitarios (CustomerService, ExpenseService, InvoiceService) | ✅ Completada |
| `v1.0.0` | Primera versión completa y estable para la tienda | 🔄 En progreso |

---

## 🏷️ Cómo crear un tag de versión

```powershell
# Crear el tag con mensaje
git tag -a v0.1.0 -m "v0.1.0: Base técnica — estructura, login, layout kawaii rosado"

# Subir el tag a GitHub
git push origin v0.1.0

# Ver todos los tags
git tag -l
```

---

## 📋 Ejemplos reales de nombres de ramas para Tina Store

```
feature/inventory-module          → Módulo completo de inventario
feature/customer-management       → CRUD de clientes
feature/invoice-pdf               → Generación de PDF de facturas
feature/excel-import              → Importación masiva por Excel
feature/accounts-receivable       → Cuentas por cobrar
feature/expense-tracking          → Módulo de egresos
feature/reports-dashboard         → Reportes y dashboard
bugfix/product-import-validation  → Error al importar productos
bugfix/invoice-total-calculation  → Error en cálculo de totales
bugfix/login-token-expiry         → Error con expiración de token JWT
release/v0.2.0                    → Preparación de versión 0.2.0
hotfix/login-blocked-users        → Error urgente de login en producción
```

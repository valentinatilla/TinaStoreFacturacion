using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Infrastructure.Data;

namespace TinaStore.Infrastructure.Services;

public sealed class ExcelService : IExcelService
{
    private readonly AppDbContext _db;

    public ExcelService(AppDbContext db) => _db = db;

    // ── Exportar productos ────────────────────────────────────────────────────
    public async Task<byte[]> ExportProductsAsync()
    {
        var productos = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Productos");

        // Encabezados
        var headers = new[]
        {
            "ID", "Nombre", "Descripción", "SKU",
            "Precio Costo", "Precio Venta", "Stock Actual", "Stock Mínimo",
            "Categoría", "Proveedor", "Activo"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Datos
        for (var i = 0; i < productos.Count; i++)
        {
            var p = productos[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = p.Id;
            ws.Cell(row, 2).Value = p.Name;
            ws.Cell(row, 3).Value = p.Description ?? string.Empty;
            ws.Cell(row, 4).Value = p.Sku ?? string.Empty;
            ws.Cell(row, 5).Value = (double)p.PurchasePrice;
            ws.Cell(row, 6).Value = (double)p.SalePrice;
            ws.Cell(row, 7).Value = p.CurrentStock;
            ws.Cell(row, 8).Value = p.MinimumStock;
            ws.Cell(row, 9).Value = p.Category?.Name ?? string.Empty;
            ws.Cell(row, 10).Value = p.Supplier?.Name ?? string.Empty;
            ws.Cell(row, 11).Value = p.IsActive ? "Sí" : "No";

            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Plantilla para importación ────────────────────────────────────────────
    public async Task<byte[]> GetProductTemplateAsync()
    {
        // Cargar categorías y proveedores activos desde BD
        var categorias  = await _db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync();
        var proveedores = await _db.Suppliers .Where(s => !s.IsDeleted && s.IsActive).OrderBy(s => s.Name).ToListAsync();

        using var wb = new XLWorkbook();

        // ── Hoja oculta con las listas de validación ──────────────────────────
        var wsListas = wb.Worksheets.Add("_Listas");
        wsListas.Visibility = XLWorksheetVisibility.Hidden;

        for (var i = 0; i < categorias.Count; i++)
            wsListas.Cell(i + 1, 1).Value = categorias[i].Name;

        for (var i = 0; i < proveedores.Count; i++)
            wsListas.Cell(i + 1, 2).Value = proveedores[i].Name;

        // Rangos con nombre para las validaciones
        if (categorias.Count > 0)
            wb.NamedRanges.Add("ListaCategorias",
                wsListas.Range(1, 1, categorias.Count, 1));

        if (proveedores.Count > 0)
            wb.NamedRanges.Add("ListaProveedores",
                wsListas.Range(1, 2, proveedores.Count, 2));

        // ── Hoja principal ────────────────────────────────────────────────────
        var ws = wb.Worksheets.Add("Productos");

        var headers = new[]
        {
            "Nombre*", "Descripción", "SKU",
            "Precio Costo*", "Precio Venta*", "Stock Inicial*", "Stock Mínimo",
            "Categoría*", "Proveedor"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Validación desplegable en filas 2-500 para Categoría (col 8) y Proveedor (col 9)
        const int maxDataRows = 500;
        if (categorias.Count > 0)
        {
            var validCat = ws.Range(2, 8, maxDataRows, 8);
            validCat.SetDataValidation().List(wsListas.Range(1, 1, categorias.Count, 1), true);
        }
        if (proveedores.Count > 0)
        {
            var validProv = ws.Range(2, 9, maxDataRows, 9);
            validProv.SetDataValidation().List(wsListas.Range(1, 2, proveedores.Count, 2), true);
        }

        // Fila de ejemplo con primera categoría/proveedor si existen
        ws.Cell(2, 1).Value = "Producto Ejemplo";
        ws.Cell(2, 2).Value = "Descripción opcional";
        ws.Cell(2, 3).Value = "SKU-001";
        ws.Cell(2, 4).Value = 5000;
        ws.Cell(2, 5).Value = 8000;
        ws.Cell(2, 6).Value = 10;
        ws.Cell(2, 7).Value = 2;
        ws.Cell(2, 8).Value = categorias.Count  > 0 ? categorias[0].Name  : string.Empty;
        ws.Cell(2, 9).Value = proveedores.Count > 0 ? proveedores[0].Name : string.Empty;
        ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");

        // Nota en fila 1 columna 11
        ws.Cell(1, 11).Value = "⚠ Selecciona Categoría y Proveedor desde el desplegable (clic en la celda).";
        ws.Cell(1, 11).Style.Font.Italic = true;
        ws.Cell(1, 11).Style.Font.FontColor = XLColor.DarkRed;

        ws.Columns(1, 9).AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Importar productos desde Excel ────────────────────────────────────────
    public async Task<ExcelImportResultDto> ImportProductsAsync(Stream excelStream)
    {
        using var wb = new XLWorkbook(excelStream);
        var ws = wb.Worksheets.FirstOrDefault(w => w.Name == "Productos")
                 ?? wb.Worksheets.First(w => w.Visibility == XLWorksheetVisibility.Visible);
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        var importados = 0;
        var errores = new List<string>();

        for (var row = 2; row <= lastRow; row++)
        {
            try
            {
                var nombre = SafeStr(ws, row, 1);
                if (string.IsNullOrEmpty(nombre)) continue;

                var costPrice  = SafeDecimal(ws, row, 4);
                var salePrice  = SafeDecimal(ws, row, 5);
                var stock      = SafeInt(ws, row, 6);
                var minStock   = SafeInt(ws, row, 7);
                var categoryRaw = SafeStr(ws, row, 8);
                var supplierRaw = SafeStr(ws, row, 9);

                if (costPrice < 0 || salePrice <= 0)
                {
                    errores.Add($"Fila {row}: precio inválido.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(categoryRaw))
                {
                    errores.Add($"Fila {row}: categoría es requerida.");
                    continue;
                }

                // Buscar categoría por nombre; fallback a ID numérico
                var categoria = await _db.Categories
                    .FirstOrDefaultAsync(c => !c.IsDeleted && c.Name.ToLower() == categoryRaw.ToLower());
                if (categoria is null && int.TryParse(categoryRaw, out var cid))
                    categoria = await _db.Categories
                        .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == cid);
                if (categoria is null)
                {
                    errores.Add($"Fila {row}: categoría '{categoryRaw}' no encontrada.");
                    continue;
                }
                var categoryId = categoria.Id;

                // Proveedor opcional: buscar por nombre o ID
                int? supplierId = null;
                if (!string.IsNullOrWhiteSpace(supplierRaw))
                {
                    var proveedor = await _db.Suppliers
                        .FirstOrDefaultAsync(s => !s.IsDeleted && s.Name.ToLower() == supplierRaw.ToLower());
                    if (proveedor is null && int.TryParse(supplierRaw, out var sid))
                        proveedor = await _db.Suppliers
                            .FirstOrDefaultAsync(s => !s.IsDeleted && s.Id == sid);
                    supplierId = proveedor?.Id;
                }

                var producto = new Product
                {
                    Name          = nombre,
                    Description   = SafeStr(ws, row, 2).NullIfEmpty(),
                    Sku           = SafeStr(ws, row, 3).NullIfEmpty(),
                    PurchasePrice = costPrice,
                    SalePrice     = salePrice,
                    CurrentStock  = stock,
                    MinimumStock  = minStock,
                    CategoryId    = categoryId,
                    SupplierId    = supplierId,
                    IsActive      = true
                };

                await _db.Products.AddAsync(producto);
                importados++;
            }
            catch (Exception ex)
            {
                errores.Add($"Fila {row}: error inesperado — {ex.Message}");
            }
        }

        if (importados > 0)
            await _db.SaveChangesAsync();

        return new ExcelImportResultDto(lastRow - 1, importados, errores.Count, errores);
    }

    // ── Vista previa de importación (sin guardar) ─────────────────────────────
    public async Task<List<ImportPreviewRowDto>> PreviewImportAsync(Stream excelStream)
    {
        using var wb = new XLWorkbook(excelStream);
        var ws = wb.Worksheets.FirstOrDefault(w => w.Name == "Productos")
                 ?? wb.Worksheets.First(w => w.Visibility == XLWorksheetVisibility.Visible);
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        var preview = new List<ImportPreviewRowDto>();

        for (var row = 2; row <= lastRow; row++)
        {
            string nombre;
            try { nombre = ws.Cell(row, 1).GetValue<string>().Trim(); }
            catch { nombre = string.Empty; }
            if (string.IsNullOrEmpty(nombre)) continue;

            var sku         = SafeStr(ws, row, 3).NullIfEmpty();
            var descripcion = SafeStr(ws, row, 2).NullIfEmpty();
            var costPrice   = SafeDecimal(ws, row, 4);
            var salePrice   = SafeDecimal(ws, row, 5);
            var stock       = SafeInt(ws, row, 6);
            var minStock    = SafeInt(ws, row, 7);
            var categoryRaw = SafeStr(ws, row, 8);
            var supplierRaw = SafeStr(ws, row, 9);

            string? error = null;
            string? categoriaNombre = null;
            int     categoryId      = 0;
            int?    supplierId      = null;
            string? proveedorNombre = null;

            if (costPrice < 0 || salePrice <= 0)
            {
                error = costPrice < 0 ? "Precio de costo inválido."
                      : "Precio de venta debe ser mayor a 0.";
            }
            else if (string.IsNullOrWhiteSpace(categoryRaw))
            {
                error = "Categoría es requerida.";
            }
            else
            {
                // Buscar categoría por nombre; si no coincide intentar por ID numérico
                var categoria = await _db.Categories
                    .FirstOrDefaultAsync(c => !c.IsDeleted &&
                        c.Name.ToLower() == categoryRaw.ToLower());
                if (categoria is null && int.TryParse(categoryRaw, out var cid))
                    categoria = await _db.Categories
                        .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == cid);

                if (categoria is null)
                    error = $"Categoría '{categoryRaw}' no encontrada.";
                else
                {
                    categoryId      = categoria.Id;
                    categoriaNombre = categoria.Name;
                }

                // Proveedor es opcional: buscar por nombre o ID
                if (!string.IsNullOrWhiteSpace(supplierRaw))
                {
                    var proveedor = await _db.Suppliers
                        .FirstOrDefaultAsync(s => !s.IsDeleted &&
                            s.Name.ToLower() == supplierRaw.ToLower());
                    if (proveedor is null && int.TryParse(supplierRaw, out var sid))
                        proveedor = await _db.Suppliers
                            .FirstOrDefaultAsync(s => !s.IsDeleted && s.Id == sid);
                    if (proveedor is not null)
                    {
                        supplierId      = proveedor.Id;
                        proveedorNombre = proveedor.Name;
                    }
                }
            }

            preview.Add(new ImportPreviewRowDto(
                row,
                nombre,
                sku,
                descripcion,
                costPrice,
                salePrice,
                stock,
                minStock,
                categoryId,
                categoriaNombre,
                supplierId,
                proveedorNombre,
                Valido: error is null,
                MensajeError: error
            ));
        }

        return preview;
    }

    // ── Helpers de lectura segura de celdas ───────────────────────────────────
    private static string SafeStr(IXLWorksheet ws, int row, int col)
    {
        try { return ws.Cell(row, col).GetValue<string>()?.Trim() ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static decimal SafeDecimal(IXLWorksheet ws, int row, int col)
    {
        try
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return 0m;
            // Intentar como double primero (formato nativo de Excel) y luego como string
            if (cell.TryGetValue<double>(out var d)) return (decimal)d;
            var s = cell.GetValue<string>().Replace(",", ".").Trim();
            return decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;
        }
        catch { return 0m; }
    }

    private static int SafeInt(IXLWorksheet ws, int row, int col)
    {
        try
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return 0;
            if (cell.TryGetValue<int>(out var i)) return i;
            var s = cell.GetValue<string>().Trim();
            return int.TryParse(s, out var v) ? v : 0;
        }
        catch { return 0; }
    }

    // ── Importar desde vista previa confirmada (sin re-leer el Excel) ─────────
    public async Task<ExcelImportResultDto> ImportFromPreviewAsync(List<ImportPreviewRowDto> filas)
    {
        var validas  = filas.Where(f => f.Valido).ToList();
        var errores  = filas.Where(f => !f.Valido)
                            .Select(f => $"Fila {f.Fila}: {f.MensajeError}").ToList();
        var importados = 0;

        foreach (var fila in validas)
        {
            try
            {
                var producto = new Product
                {
                    Name          = fila.Nombre ?? string.Empty,
                    Description   = fila.Descripcion,
                    Sku           = fila.Sku,
                    PurchasePrice = fila.PrecioCosto,
                    SalePrice     = fila.PrecioVenta,
                    CurrentStock  = fila.StockInicial,
                    MinimumStock  = fila.StockMinimo,
                    CategoryId    = fila.CategoriaId,
                    SupplierId    = fila.ProveedorId,
                    IsActive      = true
                };
                await _db.Products.AddAsync(producto);
                importados++;
            }
            catch (Exception ex)
            {
                errores.Add($"Fila {fila.Fila}: error inesperado — {ex.Message}");
            }
        }

        if (importados > 0)
            await _db.SaveChangesAsync();

        return new ExcelImportResultDto(filas.Count, importados, errores.Count, errores);
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}

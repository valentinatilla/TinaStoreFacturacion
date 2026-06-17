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
            "ID", "Nombre", "Descripción", "SKU", "Código Interno",
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
            ws.Cell(row, 5).Value = p.InternalCode ?? string.Empty;
            ws.Cell(row, 6).Value = (double)p.PurchasePrice;
            ws.Cell(row, 7).Value = (double)p.SalePrice;
            ws.Cell(row, 8).Value = p.CurrentStock;
            ws.Cell(row, 9).Value = p.MinimumStock;
            ws.Cell(row, 10).Value = p.Category?.Name ?? string.Empty;
            ws.Cell(row, 11).Value = p.Supplier?.Name ?? string.Empty;
            ws.Cell(row, 12).Value = p.IsActive ? "Sí" : "No";

            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Plantilla para importación ────────────────────────────────────────────
    public Task<byte[]> GetProductTemplateAsync()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Productos");

        var headers = new[]
        {
            "Nombre*", "Descripción", "SKU", "Código Interno",
            "Precio Costo*", "Precio Venta*", "Stock Inicial*", "Stock Mínimo",
            "ID Categoría*", "ID Proveedor"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Fila de ejemplo
        ws.Cell(2, 1).Value = "Producto Ejemplo";
        ws.Cell(2, 2).Value = "Descripción opcional";
        ws.Cell(2, 3).Value = "SKU-001";
        ws.Cell(2, 4).Value = "COD-001";
        ws.Cell(2, 5).Value = 5000;
        ws.Cell(2, 6).Value = 8000;
        ws.Cell(2, 7).Value = 10;
        ws.Cell(2, 8).Value = 2;
        ws.Cell(2, 9).Value = 1;
        ws.Cell(2, 10).Value = "";
        ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }

    // ── Importar productos desde Excel ────────────────────────────────────────
    public async Task<ExcelImportResultDto> ImportProductsAsync(Stream excelStream)
    {
        using var wb = new XLWorkbook(excelStream);
        var ws = wb.Worksheet(1);
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        var importados = 0;
        var errores = new List<string>();

        for (var row = 2; row <= lastRow; row++)
        {
            try
            {
                var nombre = ws.Cell(row, 1).GetValue<string>().Trim();
                if (string.IsNullOrEmpty(nombre)) continue;

                var costPrice = ws.Cell(row, 5).GetValue<decimal>();
                var salePrice = ws.Cell(row, 6).GetValue<decimal>();
                var stock = ws.Cell(row, 7).GetValue<int>();
                var minStock = ws.Cell(row, 8).GetValue<int>();
                var categoryId = ws.Cell(row, 9).GetValue<int>();

                if (costPrice < 0 || salePrice <= 0 || categoryId <= 0)
                {
                    errores.Add($"Fila {row}: datos inválidos (precio o categoría).");
                    continue;
                }

                var categoriaExiste = await _db.Categories.AnyAsync(c => c.Id == categoryId && !c.IsDeleted);
                if (!categoriaExiste)
                {
                    errores.Add($"Fila {row}: categoría ID {categoryId} no existe.");
                    continue;
                }

                int? supplierId = null;
                var supplierRaw = ws.Cell(row, 10).GetValue<string>().Trim();
                if (int.TryParse(supplierRaw, out var sid))
                {
                    var supplierExiste = await _db.Suppliers.AnyAsync(s => s.Id == sid && !s.IsDeleted);
                    supplierId = supplierExiste ? sid : null;
                }

                var producto = new Product
                {
                    Name = nombre,
                    Description = ws.Cell(row, 2).GetValue<string>().Trim().NullIfEmpty(),
                    Sku = ws.Cell(row, 3).GetValue<string>().Trim().NullIfEmpty(),
                    InternalCode = ws.Cell(row, 4).GetValue<string>().Trim().NullIfEmpty(),
                    PurchasePrice = costPrice,
                    SalePrice = salePrice,
                    CurrentStock = stock,
                    MinimumStock = minStock,
                    CategoryId = categoryId,
                    SupplierId = supplierId,
                    IsActive = true
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
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}

using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Infrastructure.Data;

namespace TinaStore.Infrastructure.Services;

public sealed class ExcelService : IExcelService
{
    private readonly AppDbContext _db;

    public ExcelService(AppDbContext db) => _db = db;

    // ── Exportar clientes ─────────────────────────────────────────────────────
    public async Task<byte[]> ExportCustomersAsync()
    {
        var clientes = await _db.Customers
            .Include(c => c.AccountReceivable)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.FullName)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Clientes");

        var headers = new[]
        {
            "Nombre", "Tipo documento", "N° documento", "Teléfono",
            "Correo", "Dirección", "Estado", "Última compra",
            "Saldo pendiente", "Fecha de creación"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DB2777");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (var i = 0; i < clientes.Count; i++)
        {
            var c   = clientes[i];
            var row = i + 2;

            ws.Cell(row, 1).Value  = c.FullName;
            ws.Cell(row, 2).Value  = c.DocumentType ?? string.Empty;
            ws.Cell(row, 3).Value  = c.DocumentNumber ?? string.Empty;
            ws.Cell(row, 4).Value  = c.Phone ?? string.Empty;
            ws.Cell(row, 5).Value  = c.Email ?? string.Empty;
            ws.Cell(row, 6).Value  = c.Address ?? string.Empty;
            ws.Cell(row, 7).Value  = c.IsActive ? "Activo" : "Inactivo";

            // Última compra: máximo de InvoiceDate entre las facturas no anuladas del cliente
            var ultCompra = await _db.Invoices
                .Where(f => !f.IsDeleted && f.CustomerId == c.Id && f.Status != InvoiceStatus.Cancelled)
                .MaxAsync(f => (DateTime?)f.InvoiceDate);
            ws.Cell(row, 8).Value = ultCompra.HasValue
                ? ultCompra.Value.ToLocalTime().ToString("dd/MM/yyyy")
                : "—";

            var saldo = c.AccountReceivable?.TotalDebt - c.AccountReceivable?.TotalPaid ?? 0m;
            ws.Cell(row, 9).Value  = (double)(saldo > 0 ? saldo : 0m);
            ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

            ws.Cell(row, 10).Value = c.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy");

            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FDF2F8");
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Exportar ventas ───────────────────────────────────────────────────────
    public async Task<byte[]> ExportInvoicesAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _db.Invoices
            .Include(f => f.Customer)
            .Include(f => f.Details).ThenInclude(d => d.Product!).ThenInclude(p => p.Category)
            .Include(f => f.Payments).ThenInclude(p => p.PaymentMethod)
            .Where(f => !f.IsDeleted);

        if (desde.HasValue)  query = query.Where(f => f.InvoiceDate >= desde.Value.ToUniversalTime());
        if (hasta.HasValue)  query = query.Where(f => f.InvoiceDate <= hasta.Value.ToUniversalTime().AddDays(1).AddSeconds(-1));

        var facturas = await query.OrderByDescending(f => f.InvoiceDate).ToListAsync();

        using var wb = new XLWorkbook();

        // ── Hoja 1: Resumen ───────────────────────────────────────────────────
        var wsRes = wb.Worksheets.Add("Resumen ventas");

        var hdrsRes = new[]
        {
            "N° Venta", "Fecha", "Cliente", "Tipo", "Total",
            "Pagado", "Saldo", "Estado", "Método de pago", "Observaciones"
        };
        for (var i = 0; i < hdrsRes.Length; i++)
        {
            var cell = wsRes.Cell(1, i + 1);
            cell.Value = hdrsRes[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DB2777");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (var i = 0; i < facturas.Count; i++)
        {
            var f   = facturas[i];
            var row = i + 2;

            var tieneProductos = f.Details.Any(d => d.ProductId is not null);
            var tipo           = tieneProductos ? "Con productos" : "Venta libre";
            var estado         = f.Status switch
            {
                InvoiceStatus.Paid      => "Pagada",
                InvoiceStatus.Pending   => "Pendiente",
                InvoiceStatus.Partial   => "Parcial",
                InvoiceStatus.Cancelled => "ANULADA",
                _                       => f.Status.ToString()
            };
            var metodoPago = f.Payments.Any()
                ? string.Join(", ", f.Payments
                    .GroupBy(p => p.PaymentMethod?.Name ?? "—")
                    .Select(g => g.Key))
                : "—";

            wsRes.Cell(row, 1).Value  = f.InvoiceNumber;
            wsRes.Cell(row, 2).Value  = f.InvoiceDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            wsRes.Cell(row, 3).Value  = f.Customer?.FullName ?? "—";
            wsRes.Cell(row, 4).Value  = tipo;
            wsRes.Cell(row, 5).Value  = (double)f.Total;
            wsRes.Cell(row, 6).Value  = (double)f.AmountPaid;
            wsRes.Cell(row, 7).Value  = (double)f.Balance;
            wsRes.Cell(row, 8).Value  = estado;
            wsRes.Cell(row, 9).Value  = metodoPago;
            wsRes.Cell(row, 10).Value = f.Notes ?? string.Empty;

            foreach (var col in new[] { 5, 6, 7 })
                wsRes.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";

            if (f.Status == InvoiceStatus.Cancelled)
                wsRes.Row(row).Style.Font.FontColor = XLColor.Red;
            else if (i % 2 == 1)
                wsRes.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FDF2F8");
        }
        wsRes.Columns().AdjustToContents();

        // ── Hoja 2: Detalle de productos ──────────────────────────────────────
        var wsDet = wb.Worksheets.Add("Detalle de productos");

        var hdrsDet = new[]
        {
            "N° Venta", "SKU", "Producto", "Categoría",
            "Cantidad", "Precio unitario", "Descuento", "Subtotal"
        };
        for (var i = 0; i < hdrsDet.Length; i++)
        {
            var cell = wsDet.Cell(1, i + 1);
            cell.Value = hdrsDet[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#9333EA");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var detRow = 2;
        foreach (var f in facturas)
        {
            foreach (var d in f.Details)
            {
                wsDet.Cell(detRow, 1).Value = f.InvoiceNumber;
                wsDet.Cell(detRow, 2).Value = d.Product?.Sku ?? "—";
                wsDet.Cell(detRow, 3).Value = d.ProductName.Length > 0 ? d.ProductName : "—";
                wsDet.Cell(detRow, 4).Value = d.Product?.Category?.Name ?? "—";
                wsDet.Cell(detRow, 5).Value = d.Quantity;
                wsDet.Cell(detRow, 6).Value = (double)d.UnitPrice;
                wsDet.Cell(detRow, 7).Value = (double)d.DiscountAmount;
                wsDet.Cell(detRow, 8).Value = (double)d.Subtotal;

                foreach (var col in new[] { 6, 7, 8 })
                    wsDet.Cell(detRow, col).Style.NumberFormat.Format = "#,##0.00";

                if (detRow % 2 == 1)
                    wsDet.Row(detRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#F3E8FF");

                detRow++;
            }
        }
        wsDet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

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

        // Encabezados — misma estructura que la plantilla de importación
        var headers = new[]
        {
            "SKU", "Nombre", "Descripción", "Categoría", "Proveedor",
            "Costo", "Precio de venta", "Stock", "Stock mínimo", "Unidad de medida"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DB2777");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Datos
        for (var i = 0; i < productos.Count; i++)
        {
            var p = productos[i];
            var row = i + 2;
            ws.Cell(row, 1).Value  = p.Sku ?? string.Empty;
            ws.Cell(row, 2).Value  = p.Name;
            ws.Cell(row, 3).Value  = p.Description ?? string.Empty;
            ws.Cell(row, 4).Value  = p.Category?.Name ?? string.Empty;
            ws.Cell(row, 5).Value  = p.Supplier?.Name ?? string.Empty;
            ws.Cell(row, 6).Value  = (double)p.PurchasePrice;
            ws.Cell(row, 7).Value  = (double)p.SalePrice;
            ws.Cell(row, 8).Value  = p.CurrentStock;
            ws.Cell(row, 9).Value  = p.MinimumStock;
            ws.Cell(row, 10).Value = p.Unit ?? string.Empty;

            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FDF2F8");
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
            wb.DefinedNames.Add("ListaCategorias",
                wsListas.Range(1, 1, categorias.Count, 1));

        if (proveedores.Count > 0)
            wb.DefinedNames.Add("ListaProveedores",
                wsListas.Range(1, 2, proveedores.Count, 2));

        // ── Hoja principal — misma estructura que exportación ─────────────────
        var ws = wb.Worksheets.Add("Productos");

        // Columnas: SKU(1), Nombre(2), Descripción(3), Categoría(4), Proveedor(5),
        //           Costo(6), Precio de venta(7), Stock(8), Stock mínimo(9), Unidad(10)
        var headers = new[]
        {
            "SKU", "Nombre*", "Descripción", "Categoría*", "Proveedor",
            "Costo*", "Precio de venta*", "Stock inicial*", "Stock mínimo", "Unidad de medida"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DB2777");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Validación desplegable en filas 2-500 para Categoría (col 4) y Proveedor (col 5)
        const int maxDataRows = 500;
        if (categorias.Count > 0)
        {
            var validCat = ws.Range(2, 4, maxDataRows, 4);
            validCat.CreateDataValidation().List(wsListas.Range(1, 1, categorias.Count, 1), true);
        }
        if (proveedores.Count > 0)
        {
            var validProv = ws.Range(2, 5, maxDataRows, 5);
            validProv.CreateDataValidation().List(wsListas.Range(1, 2, proveedores.Count, 2), true);
        }

        // Fila de ejemplo
        ws.Cell(2, 1).Value  = "SKU-001";
        ws.Cell(2, 2).Value  = "Producto Ejemplo";
        ws.Cell(2, 3).Value  = "Descripción opcional";
        ws.Cell(2, 4).Value  = categorias.Count  > 0 ? categorias[0].Name  : "General";
        ws.Cell(2, 5).Value  = proveedores.Count > 0 ? proveedores[0].Name : string.Empty;
        ws.Cell(2, 6).Value  = 5000;
        ws.Cell(2, 7).Value  = 8000;
        ws.Cell(2, 8).Value  = 10;
        ws.Cell(2, 9).Value  = 2;
        ws.Cell(2, 10).Value = "Unidad";
        ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#FCE7F3");

        // Nota informativa
        ws.Cell(1, 12).Value = "⚠ Los campos con * son obligatorios. Selecciona Categoría y Proveedor desde el desplegable.";
        ws.Cell(1, 12).Style.Font.Italic = true;
        ws.Cell(1, 12).Style.Font.FontColor = XLColor.DarkRed;

        ws.Columns(1, 10).AdjustToContents();

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
                // Nueva estructura: SKU(1) Nombre(2) Desc(3) Cat(4) Prov(5) Costo(6) Venta(7) Stock(8) Min(9) Unidad(10)
                var nombre = SafeStr(ws, row, 2);
                if (string.IsNullOrEmpty(nombre)) continue;

                var costPrice    = SafeDecimal(ws, row, 6);
                var salePrice    = SafeDecimal(ws, row, 7);
                var stock        = SafeInt(ws, row, 8);
                var minStock     = SafeInt(ws, row, 9);
                var categoryRaw  = SafeStr(ws, row, 4);
                var supplierRaw  = SafeStr(ws, row, 5);

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
                    Sku           = SafeStr(ws, row, 1).NullIfEmpty(),
                    Name          = nombre,
                    Description   = SafeStr(ws, row, 3).NullIfEmpty(),
                    Unit          = SafeStr(ws, row, 10).NullIfEmpty(),
                    PurchasePrice = costPrice,
                    SalePrice     = salePrice,
                    CurrentStock  = stock,
                    MinimumStock  = minStock,
                    CategoryId    = categoria.Id,
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
            // Nueva estructura: SKU(1) Nombre(2) Desc(3) Cat(4) Prov(5) Costo(6) Venta(7) Stock(8) Min(9) Unidad(10)
            string nombre;
            try { nombre = ws.Cell(row, 2).GetValue<string>().Trim(); }
            catch { nombre = string.Empty; }
            if (string.IsNullOrEmpty(nombre)) continue;

            var sku         = SafeStr(ws, row, 1).NullIfEmpty();
            var descripcion = SafeStr(ws, row, 3).NullIfEmpty();
            var costPrice   = SafeDecimal(ws, row, 6);
            var salePrice   = SafeDecimal(ws, row, 7);
            var stock       = SafeInt(ws, row, 8);
            var minStock    = SafeInt(ws, row, 9);
            var categoryRaw = SafeStr(ws, row, 4);
            var supplierRaw = SafeStr(ws, row, 5);

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

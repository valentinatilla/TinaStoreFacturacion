using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TinaStore.Application.Interfaces;
using TinaStore.Infrastructure.Data;

namespace TinaStore.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    private readonly AppDbContext _db;

    public PdfService(AppDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Details).ThenInclude(d => d.Product)
            .Include(i => i.Payments).ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(i => i.Id == invoiceId)
            ?? throw new ArgumentException($"Factura {invoiceId} no encontrada.");

        var settings = await _db.StoreSettings.FirstOrDefaultAsync();
        var storeName = settings?.StoreName ?? "Tina Store";
        var currency = settings?.Currency ?? "COP";

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(storeName).Bold().FontSize(18);
                            c.Item().Text("Factura de Venta").FontSize(12).FontColor("#555555");
                        });
                        row.ConstantItem(160).Column(c =>
                        {
                            c.Item().AlignRight().Text("N° Factura").FontSize(8).FontColor("#888888");
                            c.Item().AlignRight().Text(invoice.InvoiceNumber).Bold().FontSize(14);
                            c.Item().AlignRight().Text(invoice.InvoiceDate.ToString("dd/MM/yyyy HH:mm")).FontColor("#555555");
                            c.Item().AlignRight().Text($"Moneda: {currency}").FontSize(8).FontColor("#888888");
                        });
                    });
                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor("#CCCCCC");
                });

                // Contenido
                page.Content().Column(col =>
                {
                    // Cliente
                    col.Item().PaddingVertical(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("CLIENTE").Bold().FontColor("#333333");
                            c.Item().Text(invoice.Customer?.FullName ?? "--");
                            if (!string.IsNullOrEmpty(invoice.Customer?.DocumentNumber))
                                c.Item().Text($"Doc: {invoice.Customer.DocumentNumber}").FontColor("#666666");
                            if (!string.IsNullOrEmpty(invoice.Customer?.Phone))
                                c.Item().Text($"Tel: {invoice.Customer.Phone}").FontColor("#666666");
                            if (!string.IsNullOrEmpty(invoice.Customer?.Email))
                                c.Item().Text($"Email: {invoice.Customer.Email}").FontColor("#666666");
                        });
                    });

                    // Tabla de productos
                    var esVentaLibre = invoice.Details.Any() && invoice.Details.All(d => d.ProductId == null);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        // Cabecera
                        static IContainer HeaderCell(IContainer c) => c
                            .DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                            .Background("#2563EB").PaddingVertical(5).PaddingHorizontal(4);

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text(esVentaLibre ? "Descripción" : "Producto");
                            h.Cell().Element(HeaderCell).AlignCenter().Text("Cant.");
                            h.Cell().Element(HeaderCell).AlignRight().Text("Precio");
                            h.Cell().Element(HeaderCell).AlignRight().Text("Desc.");
                            h.Cell().Element(HeaderCell).AlignRight().Text("Subtotal");
                        });

                        // Filas
                        var rowIndex = 0;
                        foreach (var d in invoice.Details)
                        {
                            var bg = rowIndex % 2 == 0 ? "#FFFFFF" : "#F3F4F6";
                            static IContainer Cell(IContainer c, string bg) =>
                                c.Background(bg).PaddingVertical(4).PaddingHorizontal(4);

                            table.Cell().Element(c => Cell(c, bg)).Text(d.ProductName);
                            table.Cell().Element(c => Cell(c, bg)).AlignCenter().Text(d.Quantity.ToString());
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"${d.UnitPrice:N0}");
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text(d.DiscountAmount > 0 ? $"${d.DiscountAmount:N0}" : "--");
                            table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"${d.Subtotal:N0}");
                            rowIndex++;
                        }
                    });

                    // Totales
                    col.Item().PaddingTop(10).AlignRight().Column(totales =>
                    {
                        void Fila(string label, decimal valor, bool bold = false)
                        {
                            totales.Item().Row(r =>
                            {
                                r.RelativeItem().AlignRight().Text(t =>
                                {
                                    var span = t.Span(label).FontColor(bold ? Colors.Black.ToString() : "#444444");
                                    if (bold) span.Bold();
                                });
                                r.ConstantItem(120).AlignRight().Text(t =>
                                {
                                    var span = t.Span($"${valor:N0}");
                                    if (bold) span.Bold();
                                });
                            });
                        }

                        Fila("Subtotal:", invoice.Subtotal);
                        if (invoice.DiscountAmount > 0) Fila("Descuento:", invoice.DiscountAmount);
                        if (invoice.TaxAmount > 0) Fila($"Impuesto ({currency}):", invoice.TaxAmount);
                        Fila("TOTAL:", invoice.Total, true);
                        Fila("Pagado:", invoice.AmountPaid);
                        Fila("Saldo pendiente:", invoice.Balance, invoice.Balance > 0);
                    });

                    // Pagos
                    if (invoice.Payments.Any())
                    {
                        col.Item().PaddingTop(12).Column(pagos =>
                        {
                            pagos.Item().Text("Pagos registrados").Bold().FontColor("#333333");
                            foreach (var p in invoice.Payments)
                            {
                                pagos.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"{p.PaymentDate:dd/MM/yyyy} -- {p.PaymentMethod?.Name ?? "--"}").FontColor("#555555");
                                        if (!string.IsNullOrWhiteSpace(p.Reference))
                                            c.Item().Text($"Ref: {p.Reference}").FontSize(9).FontColor("#888888");
                                        if (!string.IsNullOrWhiteSpace(p.Notes))
                                            c.Item().Text($"Nota: {p.Notes}").FontSize(9).FontColor("#888888");
                                    });
                                    r.ConstantItem(120).AlignRight().Text($"${p.Amount:N0}");
                                });
                            }
                        });
                    }

                    // Estado
                    col.Item().PaddingTop(12).Row(r =>
                    {
                        var (color, etiqueta) = invoice.Status switch
                        {
                            Domain.Enums.InvoiceStatus.Paid      => ("#16A34A", "PAGADA"),
                            Domain.Enums.InvoiceStatus.Cancelled => ("#DC2626", "ANULADA"),
                            Domain.Enums.InvoiceStatus.Partial   => ("#D97706", "PARCIAL"),
                            _                                    => ("#2563EB", "PENDIENTE")
                        };
                        r.AutoItem().Background(color).Padding(6)
                            .Text(etiqueta)
                            .Bold().FontColor(Colors.White);
                    });

                    // Nota de creación
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        col.Item().PaddingTop(8).Row(r =>
                        {
                            r.AutoItem().Text("Nota: ").Bold().FontSize(9).FontColor("#555555");
                            r.RelativeItem().Text(invoice.Notes).FontSize(9).FontColor("#555555");
                        });
                    }

                    // Motivo de anulación
                    if (!string.IsNullOrWhiteSpace(invoice.CancellationReason))
                    {
                        col.Item().PaddingTop(6).Background("#FEE2E2").Padding(6).Row(r =>
                        {
                            r.AutoItem().Text("Motivo anulación: ").Bold().FontSize(9).FontColor("#991B1B");
                            r.RelativeItem().Text(invoice.CancellationReason).FontSize(9).FontColor("#991B1B");
                        });
                    }
                });

                // Pie
                page.Footer().AlignCenter()
                    .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} -- {storeName}")
                    .FontSize(8).FontColor("#999999");
            });
        });

        return pdf.GeneratePdf();
    }
}

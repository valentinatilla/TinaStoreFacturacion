using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Exceptions;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoices;
    private readonly IProductRepository _products;
    private readonly IAccountReceivableRepository _receivables;
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<InventoryMovement> _movements;
    private readonly IRepository<StoreSettings> _settings;

    public InvoiceService(
        IInvoiceRepository invoices,
        IProductRepository products,
        IAccountReceivableRepository receivables,
        IRepository<Payment> payments,
        IRepository<InventoryMovement> movements,
        IRepository<StoreSettings> settings)
    {
        _invoices = invoices;
        _products = products;
        _receivables = receivables;
        _payments = payments;
        _movements = movements;
        _settings = settings;
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync()
    {
        var lista = await _invoices.GetAllWithCustomerAsync();
        return lista.OrderByDescending(i => i.InvoiceDate).Select(ToSummaryDto);
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetByCustomerAsync(int customerId)
    {
        var lista = await _invoices.GetByCustomerAsync(customerId);
        return lista.Select(ToSummaryDto);
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var lista = await _invoices.GetByDateRangeAsync(from, to.AddDays(1).AddTicks(-1));
        return lista.Select(ToSummaryDto);
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id)
    {
        var entity = await _invoices.GetWithDetailsAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto)
    {
        if (!dto.Details.Any())
            throw new DomainException("La factura debe tener al menos un producto.");

        // ── Configuración de la tienda (consecutivo y stock) ──────────────────
        var settings = await _settings.GetByIdAsync(1)
            ?? throw new DomainException("No se encontró la configuración de la tienda.");

        // ── Validar productos y calcular subtotal ─────────────────────────────
        var lineas = new List<(Product producto, CreateInvoiceDetailDto linea)>();
        decimal subtotal = 0;

        foreach (var linea in dto.Details)
        {
            var producto = await _products.GetByIdAsync(linea.ProductId)
                ?? throw new EntityNotFoundException(nameof(Product), linea.ProductId);

            if (!settings.AllowNegativeStock && producto.CurrentStock < linea.Quantity)
                throw new InsufficientStockException(producto.Name, linea.Quantity, producto.CurrentStock);

            subtotal += (linea.UnitPrice * linea.Quantity) - linea.DiscountAmount;
            lineas.Add((producto, linea));
        }

        var total = subtotal - dto.DiscountAmount + dto.TaxAmount;

        // ── Crear factura ─────────────────────────────────────────────────────
        var invoiceNumber = $"TIN-{settings.InvoiceConsecutive:D6}";
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            CustomerId = dto.CustomerId,
            Subtotal = subtotal,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            Total = total,
            Notes = dto.Notes,
            Status = InvoiceStatus.Pending
        };

        // ── Líneas de detalle + movimientos de inventario ─────────────────────
        foreach (var (producto, linea) in lineas)
        {
            invoice.Details.Add(new InvoiceDetail
            {
                ProductId = producto.Id,
                ProductName = producto.Name,
                Quantity = linea.Quantity,
                UnitPrice = linea.UnitPrice,
                DiscountAmount = linea.DiscountAmount
            });

            var stockAntes = producto.CurrentStock;
            producto.CurrentStock -= linea.Quantity;

            invoice.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = producto.Id,
                MovementType = InventoryMovementType.Exit,
                Quantity = linea.Quantity,
                StockBefore = stockAntes,
                StockAfter = producto.CurrentStock,
                Reference = invoiceNumber,
                Notes = "Salida por venta"
            });

            await _products.UpdateAsync(producto);
        }

        // ── Pago inicial opcional ─────────────────────────────────────────────
        if (dto.PagoInicial is not null)
        {
            var pago = dto.PagoInicial;
            invoice.AmountPaid = pago.Amount;
            invoice.Payments.Add(new Payment
            {
                PaymentMethodId = pago.PaymentMethodId,
                PaymentDate = DateTime.UtcNow,
                Amount = pago.Amount,
                Reference = pago.Reference,
                Notes = pago.Notes
            });
        }

        // ── Actualizar estado según saldo ─────────────────────────────────────
        invoice.Status = invoice.AmountPaid >= total
            ? InvoiceStatus.Paid
            : invoice.AmountPaid > 0
                ? InvoiceStatus.Partial
                : InvoiceStatus.Pending;

        await _invoices.AddAsync(invoice);

        // ── Incrementar consecutivo ───────────────────────────────────────────
        settings.InvoiceConsecutive++;
        await _settings.UpdateAsync(settings);

        // ── Cuenta por cobrar si queda saldo ──────────────────────────────────
        if (invoice.Balance > 0)
        {
            var cxc = await _receivables.GetByCustomerAsync(dto.CustomerId);
            if (cxc is null)
            {
                cxc = new AccountReceivable
                {
                    CustomerId = dto.CustomerId,
                    TotalDebt = invoice.Balance,
                    TotalPaid = 0,
                    LastPaymentDate = DateTime.UtcNow
                };
                await _receivables.AddAsync(cxc);
            }
            else
            {
                cxc.TotalDebt += invoice.Balance;
                await _receivables.UpdateAsync(cxc);
            }
        }

        await _invoices.SaveChangesAsync();

        var result = await _invoices.GetWithDetailsAsync(invoice.Id);
        return ToDto(result!);
    }

    public async Task<InvoiceDto?> RegisterPaymentAsync(int invoiceId, RegisterPaymentDto dto)
    {
        var invoice = await _invoices.GetWithDetailsAsync(invoiceId);
        if (invoice is null) return null;
        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvoiceCancelledException(invoice.Id);

        var montoPendiente = invoice.Balance;
        var montoAplicado = Math.Min(dto.Amount, montoPendiente);

        var pago = new Payment
        {
            InvoiceId = invoiceId,
            PaymentMethodId = dto.PaymentMethodId,
            PaymentDate = DateTime.UtcNow,
            Amount = montoAplicado,
            Reference = dto.Reference,
            Notes = dto.Notes
        };

        await _payments.AddAsync(pago);

        invoice.AmountPaid += montoAplicado;
        invoice.Status = invoice.AmountPaid >= invoice.Total
            ? InvoiceStatus.Paid
            : InvoiceStatus.Partial;

        await _invoices.UpdateAsync(invoice);

        // ── Actualizar cuenta por cobrar ──────────────────────────────────────
        var cxc = await _receivables.GetByCustomerAsync(invoice.CustomerId);
        if (cxc is not null)
        {
            cxc.TotalPaid += montoAplicado;
            cxc.LastPaymentDate = DateTime.UtcNow;
            await _receivables.UpdateAsync(cxc);
        }

        await _invoices.SaveChangesAsync();

        var result = await _invoices.GetWithDetailsAsync(invoiceId);
        return ToDto(result!);
    }

    public async Task<InvoiceDto?> CancelAsync(int invoiceId, CancelInvoiceDto dto)
    {
        var invoice = await _invoices.GetWithDetailsAsync(invoiceId);
        if (invoice is null) return null;
        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvoiceCancelledException(invoice.Id);

        // ── Revertir stock
        foreach (var detalle in invoice.Details)
        {
            var producto = await _products.GetByIdAsync(detalle.ProductId);
            if (producto is null) continue;

            var stockAntes = producto.CurrentStock;
            producto.CurrentStock += detalle.Quantity;

            await _movements.AddAsync(new InventoryMovement
            {
                ProductId = producto.Id,
                MovementType = InventoryMovementType.ReturnFromSale,
                Quantity = detalle.Quantity,
                StockBefore = stockAntes,
                StockAfter = producto.CurrentStock,
                Reference = invoice.InvoiceNumber,
                Notes = $"Reversión por anulación: {dto.Reason}"
            });

            await _products.UpdateAsync(producto);
        }

        // ── Revertir cuenta por cobrar ────────────────────────────────────────
        if (invoice.Balance > 0)
        {
            var cxc = await _receivables.GetByCustomerAsync(invoice.CustomerId);
            if (cxc is not null)
            {
                // Restar sólo el balance pendiente; nunca dejar TotalDebt por debajo de TotalPaid
                cxc.TotalDebt = Math.Max(cxc.TotalPaid, cxc.TotalDebt - invoice.Balance);
                await _receivables.UpdateAsync(cxc);
            }
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancellationReason = dto.Reason;
        await _invoices.UpdateAsync(invoice);
        await _invoices.SaveChangesAsync();

        var result = await _invoices.GetWithDetailsAsync(invoiceId);
        return ToDto(result!);
    }

    // ── Mapeos ────────────────────────────────────────────────────────────────

    private static string StatusEnEspanol(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Paid      => "Pagada",
        InvoiceStatus.Partial   => "Parcial",
        InvoiceStatus.Cancelled => "Anulada",
        _                       => "Pendiente"
    };

    private static InvoiceSummaryDto ToSummaryDto(Invoice i) => new(
        i.Id,
        i.InvoiceNumber,
        i.InvoiceDate,
        i.Customer?.FullName ?? string.Empty,
        i.Total,
        i.Balance,
        i.Status,
        StatusEnEspanol(i.Status),
        i.AmountPaid
    );

    private static InvoiceDto ToDto(Invoice i) => new(
        i.Id,
        i.InvoiceNumber,
        i.InvoiceDate,
        i.CustomerId,
        i.Customer?.FullName ?? string.Empty,
        i.Subtotal,
        i.DiscountAmount,
        i.TaxAmount,
        i.Total,
        i.AmountPaid,
        i.Balance,
        i.Status,
        StatusEnEspanol(i.Status),
        i.Notes,
        i.Details.Select(d => new InvoiceDetailDto(
            d.Id, d.ProductId, d.ProductName,
            d.Quantity, d.UnitPrice, d.DiscountAmount, d.Subtotal
        )).ToList(),
        i.Payments.Select(p => new PaymentDto(
            p.Id, p.PaymentMethodId,
            p.PaymentMethod?.Name ?? string.Empty,
            p.PaymentDate, p.Amount, p.Reference, p.Notes
        )).ToList()
    );
}

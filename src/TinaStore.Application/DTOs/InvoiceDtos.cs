using TinaStore.Domain.Enums;

namespace TinaStore.Application.DTOs;

// ─── Detalle de línea ────────────────────────────────────────────────────────

/// <summary>Línea de producto al crear una factura.</summary>
public record CreateInvoiceDetailDto(
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount = 0
);

/// <summary>Línea de detalle en la respuesta de una factura.</summary>
public record InvoiceDetailDto(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal Subtotal
);

// ─── Pago / Abono ────────────────────────────────────────────────────────────

/// <summary>Registrar un pago o abono sobre una factura.</summary>
public record RegisterPaymentDto(
    int PaymentMethodId,
    decimal Amount,
    string? Reference,
    string? Notes
);

/// <summary>Pago registrado en la respuesta.</summary>
public record PaymentDto(
    int Id,
    int PaymentMethodId,
    string PaymentMethodName,
    DateTime PaymentDate,
    decimal Amount,
    string? Reference,
    string? Notes
);

// ─── Factura ─────────────────────────────────────────────────────────────────

/// <summary>Datos para crear una nueva factura de venta.</summary>
public record CreateInvoiceDto(
    int CustomerId,
    decimal DiscountAmount,
    decimal TaxAmount,
    string? Notes,
    List<CreateInvoiceDetailDto> Details,
    RegisterPaymentDto? PagoInicial
);

/// <summary>Respuesta completa de una factura con sus detalles y pagos.</summary>
public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    int CustomerId,
    string CustomerName,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    decimal AmountPaid,
    decimal Balance,
    InvoiceStatus Status,
    string StatusName,
    string? Notes,
    List<InvoiceDetailDto> Details,
    List<PaymentDto> Payments
);

/// <summary>Resumen de factura para listados.</summary>
public record InvoiceSummaryDto(
    int Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string CustomerName,
    decimal Total,
    decimal Balance,
    InvoiceStatus Status,
    string StatusName,
    decimal AmountPaid = 0
);

/// <summary>Datos para anular una factura.</summary>
public record CancelInvoiceDto(string Reason);

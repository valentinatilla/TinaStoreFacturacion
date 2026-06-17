using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public class CreateInvoiceDetailDtoValidator : AbstractValidator<CreateInvoiceDetailDto>
{
    public CreateInvoiceDetailDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("El producto es requerido.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");
    }
}

public class RegisterPaymentDtoValidator : AbstractValidator<RegisterPaymentDto>
{
    public RegisterPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("El método de pago es requerido.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
        RuleFor(x => x.Reference).MaximumLength(100).When(x => x.Reference != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("El cliente es requerido.");
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0).WithMessage("El impuesto no puede ser negativo.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.Details).NotEmpty().WithMessage("La factura debe tener al menos un producto.");
        RuleForEach(x => x.Details).SetValidator(new CreateInvoiceDetailDtoValidator());
        When(x => x.PagoInicial != null, () =>
            RuleFor(x => x.PagoInicial!).SetValidator(new RegisterPaymentDtoValidator()));
    }
}

public class CancelInvoiceDtoValidator : AbstractValidator<CancelInvoiceDto>
{
    public CancelInvoiceDtoValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("El motivo de anulación es requerido.")
            .MaximumLength(500);
    }
}

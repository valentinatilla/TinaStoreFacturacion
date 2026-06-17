using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodDto>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del método de pago es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de método de pago no es válido.");
    }
}

public sealed class UpdatePaymentMethodValidator : AbstractValidator<UpdatePaymentMethodDto>
{
    public UpdatePaymentMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del método de pago es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de método de pago no es válido.");
    }
}

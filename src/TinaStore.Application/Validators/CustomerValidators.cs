using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(20).WithMessage("El número de documento no puede superar 20 caracteres.")
            .When(x => x.DocumentNumber is not null);

        RuleFor(x => x.Phone)
            .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener exactamente 10 dígitos (sin espacios ni guiones).")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Ingresa un correo válido, por ejemplo: cliente@correo.com.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Phone)
            .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener exactamente 10 dígitos (sin espacios ni guiones).")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Ingresa un correo válido, por ejemplo: cliente@correo.com.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(20).WithMessage("El número de documento no puede superar 20 caracteres.")
            .When(x => x.DocumentNumber is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres.")
            .Matches(@"^\+?[\d\s\-]{7,20}$").WithMessage("Ingresa un número de teléfono válido. Para Colombia debe tener 10 dígitos.")
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
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres.")
            .Matches(@"^\+?[\d\s\-]{7,20}$").WithMessage("Ingresa un número de teléfono válido. Para Colombia debe tener 10 dígitos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Ingresa un correo válido, por ejemplo: cliente@correo.com.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

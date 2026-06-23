using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreateSupplierValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del proveedor es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.TaxId)
            .Matches(@"^\d+$").WithMessage("El NIT debe contener solo números.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));

        RuleFor(x => x.Phone)
            .Matches(@"^\d+$").WithMessage("El teléfono debe contener solo números.")
            .Length(10, 10).WithMessage("El teléfono debe tener exactamente 10 dígitos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

public sealed class UpdateSupplierValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del proveedor es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.TaxId)
            .Matches(@"^\d+$").WithMessage("El NIT debe contener solo números.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));

        RuleFor(x => x.Phone)
            .Matches(@"^\d+$").WithMessage("El teléfono debe contener solo números.")
            .Length(10, 10).WithMessage("El teléfono debe tener exactamente 10 dígitos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

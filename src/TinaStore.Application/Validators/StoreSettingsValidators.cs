using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public class UpdateStoreSettingsDtoValidator : AbstractValidator<UpdateStoreSettingsDto>
{
    public UpdateStoreSettingsDtoValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty().WithMessage("El nombre de la tienda es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(10);

        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("El IVA no puede ser negativo.")
            .LessThanOrEqualTo(100).WithMessage("El IVA no puede superar el 100%.");
    }
}

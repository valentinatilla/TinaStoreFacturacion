using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(30).WithMessage("El nombre no puede superar 30 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("La descripción no puede superar 200 caracteres.")
            .When(x => x.Description is not null);
    }
}

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(30).WithMessage("El nombre no puede superar 30 caracteres.");
    }
}

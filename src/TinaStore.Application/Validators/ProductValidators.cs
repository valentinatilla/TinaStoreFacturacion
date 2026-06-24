using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Sku)
            .MaximumLength(30).WithMessage("El SKU no puede superar 30 caracteres.")
            .When(x => x.Sku is not null);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del producto permite máximo 50 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("La descripción no puede superar 300 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Unit)
            .MaximumLength(30).WithMessage("La unidad de medida no puede superar 30 caracteres.")
            .Matches(@"^[a-záéíóúÁÉÍÓÚñÑüÜ\s]+$")
            .WithMessage("La unidad de medida no debe contener números.")
            .When(x => !string.IsNullOrWhiteSpace(x.Unit));

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.")
            .LessThanOrEqualTo(999_999.99m).WithMessage("El precio de compra no puede superar $999.999,99.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero.")
            .LessThanOrEqualTo(999_999.99m).WithMessage("El precio de venta no puede superar $999.999,99.");

        RuleFor(x => x.CurrentStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock inicial no puede ser negativo.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida.");
    }
}

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del producto permite máximo 50 caracteres.");

        RuleFor(x => x.Unit)
            .MaximumLength(30).WithMessage("La unidad de medida no puede superar 30 caracteres.")
            .Matches(@"^[a-záéíóúÁÉÍÓÚñÑüÜ\s]+$")
            .WithMessage("La unidad de medida no debe contener números.")
            .When(x => !string.IsNullOrWhiteSpace(x.Unit));

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.")
            .LessThanOrEqualTo(999_999.99m).WithMessage("El precio de compra no puede superar $999.999,99.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero.")
            .LessThanOrEqualTo(999_999.99m).WithMessage("El precio de venta no puede superar $999.999,99.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría válida.");
    }
}

using FluentValidation;
using TinaStore.Application.DTOs;

namespace TinaStore.Application.Validators;

/// <summary>Validador compartido para crear y actualizar categorías de egreso.</summary>
public class ExpenseCategoryDtoValidator : AbstractValidator<CreateExpenseCategoryDto>
{
    public ExpenseCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

// Alias para mantener compatibilidad con los registros en DependencyInjection
public class CreateExpenseCategoryDtoValidator : ExpenseCategoryDtoValidator { }
public class UpdateExpenseCategoryDtoValidator : AbstractValidator<UpdateExpenseCategoryDto>
{
    public UpdateExpenseCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
    }
}

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("La descripción es requerida.").MaximumLength(300);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
        RuleFor(x => x.ExpenseCategoryId).GreaterThan(0).WithMessage("La categoría de gasto es requerida.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.ExpenseDate).NotEmpty().WithMessage("La fecha es requerida.");
    }
}

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("La descripción es requerida.").MaximumLength(300);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");
        RuleFor(x => x.ExpenseCategoryId).GreaterThan(0).WithMessage("La categoría de gasto es requerida.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.ExpenseDate).NotEmpty().WithMessage("La fecha es requerida.");
    }
}

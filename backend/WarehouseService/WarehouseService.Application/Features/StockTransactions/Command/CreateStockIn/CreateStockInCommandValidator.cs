using FluentValidation;

namespace WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;

public class CreateStockInCommandValidator : AbstractValidator<CreateStockInCommand>
{
    public CreateStockInCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Stock in must contain at least one item.")
            .Must(items => items.Count <= 100).WithMessage("Stock in cannot contain more than 100 items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.IngredientId)
                .NotEmpty().WithMessage("Ingredient ID must not be empty.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");
        });

        RuleFor(x => x.ReferenceCode)
            .MaximumLength(100).WithMessage("Reference code must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceCode));

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy must not be empty.");
    }
}
using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseItems.Command.CreateWarehouseItem;

public class CreateWarehouseItemCommandValidator : AbstractValidator<CreateWarehouseItemCommand>
{
    public CreateWarehouseItemCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");

        RuleFor(x => x.IngredientId)
            .NotEmpty().WithMessage("Ingredient ID must not be empty.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit must not be empty.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to 0.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.")
            .When(x => x.ExpiryDate.HasValue);
    }
}
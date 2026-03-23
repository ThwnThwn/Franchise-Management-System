using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseItems.Command.UpdateWarehouseItem;

public class UpdateWarehouseItemCommandValidator : AbstractValidator<UpdateWarehouseItemCommand>
{
    public UpdateWarehouseItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse item ID must not be empty.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit must not be empty.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to 0.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.")
            .When(x => x.ExpiryDate.HasValue);
    }
}
using FluentValidation;

namespace WarehouseService.Application.Features.Warehouses.Command.UpdateWarehouse;

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Warehouse code must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

        RuleFor(x => x.Name)
            .MaximumLength(255).WithMessage("Warehouse name must not exceed 255 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Status)
            .Must(s => Enum.IsDefined(typeof(WarehouseService.Domain.Enums.WarehouseStatus), s!))
            .WithMessage("Warehouse status is invalid.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID must not be empty.")
            .When(x => x.StoreId.HasValue);
    }
}
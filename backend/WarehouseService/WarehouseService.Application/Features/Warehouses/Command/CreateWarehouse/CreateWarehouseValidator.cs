using FluentValidation;

namespace WarehouseService.Application.Features.Warehouses.Command.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code must not be empty.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name must not be empty.")
            .MaximumLength(255).WithMessage("Warehouse name must not exceed 255 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Warehouse address must not be empty.");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("StoreId must not be empty.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid warehouse status.");
    }
}
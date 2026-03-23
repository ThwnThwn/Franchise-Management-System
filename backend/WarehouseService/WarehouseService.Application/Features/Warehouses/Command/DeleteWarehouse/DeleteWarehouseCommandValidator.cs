using FluentValidation;

namespace WarehouseService.Application.Features.Warehouses.Command.DeleteWarehouse;

public class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");
    }
}
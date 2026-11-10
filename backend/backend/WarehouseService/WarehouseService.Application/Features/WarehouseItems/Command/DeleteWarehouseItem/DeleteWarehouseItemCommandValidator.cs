using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseItems.Command.DeleteWarehouseItem;

public class DeleteWarehouseItemCommandValidator : AbstractValidator<DeleteWarehouseItemCommand>
{
    public DeleteWarehouseItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse item ID must not be empty.");
    }
}
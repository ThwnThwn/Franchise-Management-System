using FluentValidation;
namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;

public class CreateWarehouseTransferCommandValidator : AbstractValidator<CreateWarehouseTransferCommand>
{
    public CreateWarehouseTransferCommandValidator()
    {
        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse ID must not be empty.");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse ID must not be empty.")
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouses must be different.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Transfer must contain at least one item.")
            .Must(items => items.Count <= 100).WithMessage("Transfer cannot contain more than 100 items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.WarehouseItemId)
                .NotEmpty().WithMessage("Warehouse item ID must not be empty.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Transfer quantity must be greater than 0.");
        });

    }
}
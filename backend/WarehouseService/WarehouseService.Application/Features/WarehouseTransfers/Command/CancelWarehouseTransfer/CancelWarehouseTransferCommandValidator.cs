using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;

public class CancelWarehouseTransferCommandValidator : AbstractValidator<CancelWarehouseTransferCommand>
{
    public CancelWarehouseTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse transfer ID must not be empty.");

    }
}
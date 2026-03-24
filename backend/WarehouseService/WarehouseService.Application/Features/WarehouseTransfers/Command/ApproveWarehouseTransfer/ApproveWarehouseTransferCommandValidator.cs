using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;

public class ApproveWarehouseTransferCommandValidator : AbstractValidator<ApproveWarehouseTransferCommand>
{
    public ApproveWarehouseTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse transfer ID must not be empty.");
    }
}
using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;

public class RejectWarehouseTransferCommandValidator : AbstractValidator<RejectWarehouseTransferCommand>
{
    public RejectWarehouseTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse transfer ID must not be empty.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason must not be empty.")
            .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");

    }
}
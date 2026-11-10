using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;

public class CompleteWarehouseTransferCommandValidator : AbstractValidator<CompleteWarehouseTransferCommand>
{
    public CompleteWarehouseTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse transfer ID must not be empty.");

        RuleFor(x => x.CompletedBy)
            .NotEmpty().WithMessage("CompletedBy must not be empty.");
    }
}
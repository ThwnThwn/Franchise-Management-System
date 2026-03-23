using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;

public class GetWarehouseTransferQueryValidator : AbstractValidator<GetWarehouseTransferQuery>
{
    public GetWarehouseTransferQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse transfer ID must not be empty.");
    }
}
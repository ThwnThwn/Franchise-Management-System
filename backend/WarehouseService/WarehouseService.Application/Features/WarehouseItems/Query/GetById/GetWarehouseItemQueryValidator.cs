using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetById;

public class GetWarehouseItemQueryValidator : AbstractValidator<GetWarehouseItemQuery>
{
    public GetWarehouseItemQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse item ID must not be empty.");
    }
}
using FluentValidation;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetById;

public class GetWarehouseQueryValidator : AbstractValidator<GetWarehouseQuery>
{
    public GetWarehouseQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");
    }
}
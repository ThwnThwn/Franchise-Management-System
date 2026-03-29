using FluentValidation;

namespace WarehouseService.Application.Features.WarehouseItems.Query.GetAll;

public class GetWarehouseItemsQueryValidator : AbstractValidator<GetWarehouseItemsQuery>
{
    public GetWarehouseItemsQueryValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID must not be empty.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
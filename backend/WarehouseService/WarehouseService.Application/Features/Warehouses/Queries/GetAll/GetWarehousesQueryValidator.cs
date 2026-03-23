using FluentValidation;

namespace WarehouseService.Application.Features.Warehouses.Queries.GetAll;

public class GetWarehousesQueryValidator : AbstractValidator<GetWarehousesQuery>
{
    public GetWarehousesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Warehouse status is invalid.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
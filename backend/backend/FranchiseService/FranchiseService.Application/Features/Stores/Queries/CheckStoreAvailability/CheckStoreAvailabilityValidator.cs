using FluentValidation;

namespace FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability;

public class CheckStoreAvailabilityValidator : AbstractValidator<CheckStoreAvailabilityQuery>
{
    public CheckStoreAvailabilityValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID không được để trống")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Store ID không đúng định dạng UUID");
    }
}
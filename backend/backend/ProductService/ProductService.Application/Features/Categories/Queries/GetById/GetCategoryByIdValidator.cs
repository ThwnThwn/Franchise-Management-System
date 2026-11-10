using FluentValidation;

namespace ProductService.Application.Features.Categories.Queries.GetById;

public sealed class GetCategoryByIdValidator :
    AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required")
            .Must(categoryId => categoryId != Guid.Empty)
            .WithMessage("Category Id must be a valid non-empty GUID.");
    }
}
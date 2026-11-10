using FluentValidation;

namespace ProductService.Application.Features.Categories.Queries.GetByCategoryName;

public sealed class GetCategoryByNameQueryValidator :
    AbstractValidator<GetCategoryByNameQuery>
{
    public GetCategoryByNameQueryValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .WithMessage("Category name is required");
    }
}
using FluentValidation;

namespace ProductService.Application.Features.Products.Queries.GetByProductName;

public class GetProductByNameQueryValidator :
    AbstractValidator<GetProductByNameQuery>
{
    public GetProductByNameQueryValidator()
    {
        RuleFor(x => x.ProductName)
            .NotNull()
            .WithMessage("Product name is required.");
    }
}
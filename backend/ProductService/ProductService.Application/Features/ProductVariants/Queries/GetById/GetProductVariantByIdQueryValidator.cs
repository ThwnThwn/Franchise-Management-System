using FluentValidation;

namespace ProductService.Application.Features.ProductVariants.Queries.GetById;

public sealed class GetProductVariantByIdQueryValidator:
    AbstractValidator<GetProductVariantByIdQuery>
{
    public GetProductVariantByIdQueryValidator() {
        RuleFor(x=>x.ProductVariantId).NotEmpty()
            .WithMessage("Product Variant Id is required");
}
}
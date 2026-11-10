using FluentValidation;

namespace ProductService.Application.Features.Products.Command.UpdateProduct;

public sealed class UpdateProductCommandValidator :
    AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty()
            .WithMessage("Product ID must be provided.");
        RuleFor(x => x.Request.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID must be provided.");
        RuleFor(x => x.Request.Name)
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");
        RuleFor(x => x.Request.Description)
            .MaximumLength(200)
            .WithMessage("Product description must not exceed 300 characters.");
    }
}
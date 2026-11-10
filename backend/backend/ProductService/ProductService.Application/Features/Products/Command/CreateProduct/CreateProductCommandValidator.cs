using FluentValidation;

namespace ProductService.Application.Features.Products.Command.CreateProduct;

public sealed class CreateProductCommandValidator :
    AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Product name must be provided.")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");
        RuleFor(x => x.Request.Description)
            .MaximumLength(200)
            .WithMessage("Product description must not exceed 300 characters.");
    }
}
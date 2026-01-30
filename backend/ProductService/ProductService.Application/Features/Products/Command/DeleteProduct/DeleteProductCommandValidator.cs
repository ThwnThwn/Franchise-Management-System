using FluentValidation;

namespace ProductService.Application.Features.Products.Command.DeleteProduct
{
    public sealed class DeleteProductCommandValidator :
        AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID must be provided.");
        }
    }
}

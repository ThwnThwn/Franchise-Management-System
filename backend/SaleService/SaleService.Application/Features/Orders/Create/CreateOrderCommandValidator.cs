using FluentValidation;

namespace SalesService.Application.Features.Orders.Create;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Request.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty");

        RuleForEach(x => x.Request.Items)
            .SetValidator(new CartItemDtoValidator());

        RuleFor(x => x.Request.Tax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.Tax.HasValue)
            .WithMessage("Tax must be greater than or equal to 0");
    }
}

public class CartItemDtoValidator : AbstractValidator<CartItemDto>
{
    public CartItemDtoValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}

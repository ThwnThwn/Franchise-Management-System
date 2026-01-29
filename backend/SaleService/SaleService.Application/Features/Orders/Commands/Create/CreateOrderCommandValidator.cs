using FluentValidation;

namespace SalesService.Application.Features.Orders.Commands.Create;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Request.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty");

        RuleForEach(x => x.Request.Items)
            .SetValidator(new CartItemDtoValidator());

        // Tax is always calculated as 10% in handler
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

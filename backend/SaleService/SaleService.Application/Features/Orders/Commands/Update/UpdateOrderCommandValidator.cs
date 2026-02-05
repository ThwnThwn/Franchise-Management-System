using FluentValidation;

namespace SalesService.Application.Features.Orders.Commands.Update;

public sealed class UpdateOrderCommandValidator
    : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        // OrderId must be provided
        RuleFor(x => x.Request.OrderId)
            .NotNull()
            .WithMessage("OrderId is required");

        // Items list must not be empty
        RuleFor(x => x.Request.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty");

        // Validate each item in the list
        RuleForEach(x => x.Request.Items)
            .SetValidator(new OrderItemValidator());
    }
    public sealed class OrderItemValidator : AbstractValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty()
                .WithMessage("SKU is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");
        }
    }
}

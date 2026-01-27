using FluentValidation;

namespace SalesService.Application.Features.Orders.GetById;

public sealed class GetOrderByIdQueryValidator
    : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}

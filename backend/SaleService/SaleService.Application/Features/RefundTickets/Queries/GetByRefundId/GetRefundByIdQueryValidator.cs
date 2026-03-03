using FluentValidation;

namespace SalesService.Application.Features.RefundTickets.Queries.GetById;

public sealed class GetRefundByIdQueryValidator
    : AbstractValidator<GetRefundByIdQuery>
{
    public GetRefundByIdQueryValidator()
    {
        RuleFor(x => x.RefundTicketId)
            .NotEmpty()
            .WithMessage("RefundTicketId is required");
    }
}

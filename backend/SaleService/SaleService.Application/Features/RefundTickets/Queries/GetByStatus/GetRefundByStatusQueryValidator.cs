using FluentValidation;
using SalesService.Application.Features.RefundTickets.Queries.GetByStatus;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.RefundTickets.Validators;

public sealed class GetRefundByStatusQueryValidator
    : AbstractValidator<GetRefundByStatusQuery>
{
    public GetRefundByStatusQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid refund ticket status");

        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("Page index must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");
    }
}
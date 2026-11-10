using FluentValidation;

namespace SalesService.Application.Features.RefundTickets.Queries.GetByUserId;

public sealed class GetRefundByUserIdValidator
    : AbstractValidator<GetRefundByUserIdQuery>
{
    public GetRefundByUserIdValidator()
    {
        RuleFor(x => x.UserIdToSearch)
            .NotEmpty()
            .WithMessage("UserId is required to search");
        RuleFor(x => x.LoggedUserId)
            .NotEmpty()
            .WithMessage("Logged UserId is required");
    }
}

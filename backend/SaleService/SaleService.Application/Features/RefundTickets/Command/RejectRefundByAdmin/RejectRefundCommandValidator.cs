
using FluentValidation;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;

namespace SalesService.Application.Features.RefundTickets.Command.RejectRefundCommandValidator
{
    internal sealed class RejectRefundCommandValidator
        : AbstractValidator<RejectRefundCommand>
    {
        public RejectRefundCommandValidator()
        {
            RuleFor(x => x.InternalUserId)
                .NotEmpty()
                .WithMessage("InternalUserId is required");

            RuleFor(x => x.RefundTicketId)
                .NotEmpty()
                .WithMessage("RefundTicketId is required");
        }
    }
}

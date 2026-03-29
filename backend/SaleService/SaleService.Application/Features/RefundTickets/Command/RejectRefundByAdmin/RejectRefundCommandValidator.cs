
using FluentValidation;
using SalesService.Application.Features.RefundTickets.Command.RejectRefundByAdmin;

namespace SalesService.Application.Features.RefundTickets.Command.RejectRefundCommandValidator
{
    public sealed class RejectRefundCommandValidator
        : AbstractValidator<RejectRefundCommand>
    {
        public RejectRefundCommandValidator()
        {

            RuleFor(x => x.RefundTicketId)
                .NotEmpty()
                .WithMessage("RefundTicketId is required");
        }
    }
}

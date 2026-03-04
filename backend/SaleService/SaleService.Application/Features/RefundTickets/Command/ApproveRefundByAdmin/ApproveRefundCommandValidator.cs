using FluentValidation;

namespace SalesService.Application.Features.RefundTickets.Command.ApproveRefund
{
    internal sealed class ApproveRefundCommandValidator
        : AbstractValidator<ApproveRefundCommand>
    {
        public ApproveRefundCommandValidator()
        {
           

            RuleFor(x => x.RefundTicketId)
                .NotEmpty()
                .WithMessage("RefundTicketId is required");
        }
    }
}

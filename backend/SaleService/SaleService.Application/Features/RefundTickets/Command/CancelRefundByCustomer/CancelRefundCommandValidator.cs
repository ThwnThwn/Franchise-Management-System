using FluentValidation;

namespace SalesService.Application.Features.RefundTickets.Command.CancelRefund
{
    public sealed class CancelRefundCommandValidator
        : AbstractValidator<CancelRefundCommand>
    {
        public CancelRefundCommandValidator()
        {

            RuleFor(x => x.RefundTicketId)
                .NotEmpty()
                .WithMessage("RefundTicketId is required.");

            When(x => x.Request != null, () =>
            {
                RuleFor(x => x.Request!.CancellationReason)
                    .MaximumLength(500)
                    .WithMessage("Cancellation reason cannot exceed 500 characters.")
                    .When(x => !string.IsNullOrWhiteSpace(x.Request!.CancellationReason));
            });
        }
    }
}

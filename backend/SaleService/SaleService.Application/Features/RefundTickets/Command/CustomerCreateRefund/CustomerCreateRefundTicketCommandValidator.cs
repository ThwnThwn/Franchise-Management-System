using FluentValidation;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;

namespace SalesService.Application.Features.RefundTickets.Command.CustomerCreateRefund
{
    public sealed class CustomerCreateRefundTicketCommandValidator
        : AbstractValidator<CustomerCreateRefundTicketCommand>
    {
        public CustomerCreateRefundTicketCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("CustomerId is required.");

            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("OrderId is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request body is required.");

            When(x => x.Request != null, () =>
            {
                RuleFor(x => x.Request.Amount)
                    .GreaterThan(0)
                    .WithMessage("Refund amount must be greater than 0.");

                RuleFor(x => x.Request.Reason)
                    .NotEmpty()
                    .WithMessage("Refund reason is required.")
                    .MaximumLength(300)
                    .WithMessage("Refund reason must not exceed 300 characters.");
            });
        }
    }
}

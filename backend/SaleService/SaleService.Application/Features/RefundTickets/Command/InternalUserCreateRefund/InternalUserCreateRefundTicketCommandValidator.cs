using FluentValidation;
using SalesService.Application.Features.RefundTickets.Command.InternalUserCreateRefund;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.RefundTickets.Validators;

public sealed class InternalUserCreateRefundTicketCommandValidator
    : AbstractValidator<InternalUserCreateRefundTicketCommand>
{
    public InternalUserCreateRefundTicketCommandValidator()
    {
        RuleFor(x => x.InternalUserOverride) 
            .NotNull()
            .WithMessage("InternalUserOverride is required for InternalUser refund creation.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId (target user) is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Refund request is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.WalletId)
                .NotEmpty()
                .WithMessage("WalletId is required.");

            RuleFor(x => x.Request.OriginalTransactionId)
                .NotEmpty()
                .WithMessage("OriginalTransactionId is required.");

            RuleFor(x => x.Request.Amount)
                .GreaterThan(0)
                .WithMessage("Refund amount must be greater than 0.")
                .LessThanOrEqualTo(1_000_000_000) 
                .WithMessage("Refund amount exceeds maximum allowed (1 billion).");

            RuleFor(x => x.Request.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters.");
        });

        // InternalUserOverride Validation
        When(x => x.InternalUserOverride != null, () =>
        {


            RuleFor(x => x.InternalUserOverride!.RefundReason)
            .IsInEnum()
            .WithMessage("Invalid refund reason.")
            .When(x => x.InternalUserOverride!.RefundReason.HasValue);

        //  InternalNotes required when using ANY bypass
        //RuleFor(x => x.InternalUserOverride!.InternalNotes)
        //    .NotEmpty()
        //    .WithMessage("InternalNotes is required when bypassing validations.")
        //    .When(x => x.InternalUserOverride!.BypassTimeLimit ||
        //              x.InternalUserOverride!.BypassAmountValidation);

        RuleFor(x => x.InternalUserOverride!.InternalNotes)
            .MaximumLength(1000)
            .WithMessage("InternalNotes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.InternalUserOverride!.InternalNotes));

        //  RefundReasonType required for non-normal refunds
        RuleFor(x => x.InternalUserOverride!.RefundReason)
            .NotNull()
            .NotEqual(RefundReasonType.NormalProcess)
            .WithMessage("Refund Reason Type must be specified when bypassing validations.")
            .When(x => x.InternalUserOverride!.BypassTimeLimit ||
                      x.InternalUserOverride!.BypassAmountValidation);

        //  AutoApprove logic validation
        //RuleFor(x => x.InternalUserOverride!.InternalNotes)
        //    .NotEmpty()
        //    .WithMessage("InternalNotes is required for auto-approved refunds.")
        //    .When(x => x.InternalUserOverride!.AutoApprove);
    });

        //  If bypassing amount, compensation type cannot be Normal
        RuleFor(x => x.InternalUserOverride!.RefundReason)
            .Must(type => type != RefundReasonType.NormalProcess)
            .WithMessage("Cannot use Normal compensation type when bypassing amount validation.")
            .When(x => x.InternalUserOverride != null &&
                      x.InternalUserOverride.BypassAmountValidation);

        //  If compensation type is SystemError/Complaint, bypass should be considered
        RuleFor(x => x.InternalUserOverride)
            .Must(o => o!.BypassTimeLimit || o.BypassAmountValidation)
            .WithMessage("SystemError or Complaint refunds should use bypass options.")
            .When(x => x.InternalUserOverride != null &&
                      x.InternalUserOverride.RefundReason.HasValue &&
                      (x.InternalUserOverride.RefundReason == RefundReasonType.SystemError ||
                       x.InternalUserOverride.RefundReason == RefundReasonType.CustomerComplaint))
            .WithSeverity(Severity.Warning); //  Warning, not error
    }
}
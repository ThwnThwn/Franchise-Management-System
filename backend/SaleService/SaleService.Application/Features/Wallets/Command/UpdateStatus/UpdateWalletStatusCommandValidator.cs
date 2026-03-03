using FluentValidation;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Wallets.Validators;

public sealed class UpdateWalletStatusCommandValidator
    : AbstractValidator<UpdateWalletStatusCommand>
{
    public UpdateWalletStatusCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("WalletId is required.");

        RuleFor(x => x.AdminId)
            .NotEmpty()
            .WithMessage("AdminId is required.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Request is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.NewStatus)
                .IsInEnum()
                .WithMessage("Invalid wallet status.");

            // ✅ Reason required for certain status changes
            RuleFor(x => x.Request.Reason)
                .NotEmpty()
                .When(x => x.Request.NewStatus is WalletStatus.Locked 
                                                 or WalletStatus.Suspended)
                .WithMessage("Reason is required when locking or suspending wallet.");

            RuleFor(x => x.Request.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Reason));

            RuleFor(x => x.Request.AdminNotes)
                .MaximumLength(1000)
                .WithMessage("AdminNotes cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.AdminNotes));
        });
    }
}
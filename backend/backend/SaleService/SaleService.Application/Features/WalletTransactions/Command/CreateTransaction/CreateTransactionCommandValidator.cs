using FluentValidation;
using SalesService.Application.Features.WalletTransactions.Commands.CreateTransaction;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WalletTransactions.Validators;

public sealed class CreateTransactionCommandValidator
    : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        // ===== UserId Validation =====
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        // ===== Request Validation =====
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Transaction request is required.");

        When(x => x.Request != null, () =>
        {
            // WalletId required
            RuleFor(x => x.Request.WalletId)
                .NotEmpty()
                .WithMessage("WalletId is required.");

            // Amount validation
            RuleFor(x => x.Request.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.")
                .LessThanOrEqualTo(10_000_000_000) // 10 billion VND max
                .WithMessage("Amount exceeds maximum allowed (10 billion VND).");

            // Transaction type required
            RuleFor(x => x.Request.TransactionType)
                .IsInEnum()
                .WithMessage("Invalid transaction type.");

            // Description length
            RuleFor(x => x.Request.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));

            // Notes length
            RuleFor(x => x.Request.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Notes));

            // External transaction ID
            RuleFor(x => x.Request.ExternalTransactionId)
                .MaximumLength(200)
                .WithMessage("ExternalTransactionId cannot exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.ExternalTransactionId));

            // ===== Type-Specific Validations =====

            // Payment: ReferenceId & ReferenceType required
            When(x => x.Request.TransactionType == TransactionType.Payment, () =>
            {
                RuleFor(x => x.Request.ReferenceId)
                    .NotEmpty()
                    .WithMessage("ReferenceId (OrderId) is required for Payment transactions.");

                RuleFor(x => x.Request.ReferenceType)
                    .Equal(WalletReferenceType.Order)
                    .WithMessage("ReferenceType must be 'Order' for Payment transactions.");
            });

            // Refund: ReferenceId & ReferenceType required
            When(x => x.Request.TransactionType == TransactionType.Refund, () =>
            {
                RuleFor(x => x.Request.ReferenceId)
                    .NotEmpty()
                    .WithMessage("ReferenceId (RefundTicketId) is required for Refund transactions.");

                RuleFor(x => x.Request.ReferenceType)
                    .Equal(WalletReferenceType.RefundTicket)
                    .WithMessage("ReferenceType must be 'RefundTicket' for Refund transactions.");
            });

            // Transfer: DestinationWalletId required
            When(x => x.Request.TransactionType == TransactionType.Transfer, () =>
            {
                RuleFor(x => x.Request.DestinationWalletId)
                    .NotEmpty()
                    .WithMessage("DestinationWalletId is required for Transfer transactions.");

                RuleFor(x => x.Request.DestinationWalletId)
                    .NotEqual(x => x.Request.WalletId)
                    .WithMessage("Cannot transfer to the same wallet.");
            });

            // Deposit: PaymentMethod recommended
            When(x => x.Request.TransactionType == TransactionType.Deposit, () =>
            {
                RuleFor(x => x.Request.PaymentMethod)
                    .NotEmpty()
                    .WithMessage("PaymentMethod is recommended for Deposit transactions.")
                    .WithSeverity(Severity.Warning);
            });
        });

        // ===== AdminOverride Validation =====
        When(x => x.AdminOverride != null, () =>
        {
            RuleFor(x => x.AdminOverride!.AdminId)
                .NotEmpty()
                .WithMessage("AdminId is required for admin override.");

            // InternalNotes required when bypassing
            RuleFor(x => x.AdminOverride!.InternalNotes)
                .NotEmpty()
                .WithMessage("InternalNotes is required when using admin override.")
                .When(x => x.AdminOverride!.BypassBalanceCheck ||
                          x.AdminOverride!.BypassTransactionLimits);

            RuleFor(x => x.AdminOverride!.InternalNotes)
                .MaximumLength(1000)
                .WithMessage("InternalNotes cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.AdminOverride!.InternalNotes));
        });
    }
}
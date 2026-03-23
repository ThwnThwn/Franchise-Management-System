using FluentValidation;

namespace WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;

public class CompleteStockTransactionCommandValidator : AbstractValidator<CompleteStockTransactionCommand>
{
    public CompleteStockTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionIds)
            .NotEmpty().WithMessage("Transaction IDs must not be empty.")
            .Must(ids => ids.Count <= 100).WithMessage("Cannot complete more than 100 transactions at once.");

        RuleForEach(x => x.TransactionIds)
            .NotEmpty().WithMessage("Transaction ID must not be empty.");

        RuleFor(x => x.CompletedBy)
            .NotEmpty().WithMessage("CompletedBy must not be empty.");
    }
}
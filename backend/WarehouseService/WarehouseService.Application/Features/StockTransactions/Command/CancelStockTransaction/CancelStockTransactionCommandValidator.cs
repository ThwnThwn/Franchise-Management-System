using FluentValidation;

namespace WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;

public class CancelStockTransactionCommandValidator : AbstractValidator<CancelStockTransactionCommand>
{
    public CancelStockTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stock transaction ID must not be empty.");

    }
}
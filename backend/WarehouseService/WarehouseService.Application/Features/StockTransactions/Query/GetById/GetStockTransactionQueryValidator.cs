using FluentValidation;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetById;

public class GetStockTransactionQueryValidator : AbstractValidator<GetStockTransactionQuery>
{
    public GetStockTransactionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stock transaction ID must not be empty.");
    }
}
using FluentValidation;
using WarehouseService.Application.Features.StockTransactions.Query.GetAll;
using WarehouseService.Domain.Enums;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetStockTransactions;

public class GetStockTransactionsQueryValidator : AbstractValidator<GetStockTransactionsQuery>
{
    public GetStockTransactionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Type)
            .Must(t => Enum.IsDefined(typeof(TransactionType), t!))
            .WithMessage("Transaction type is invalid.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Status)
            .Must(s => Enum.IsDefined(typeof(TransactionStatus), s!))
            .WithMessage("Transaction status is invalid.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.ToDate)
            .Must((query, toDate) => toDate >= query.FromDate)
            .WithMessage("ToDate must be greater than or equal to FromDate.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
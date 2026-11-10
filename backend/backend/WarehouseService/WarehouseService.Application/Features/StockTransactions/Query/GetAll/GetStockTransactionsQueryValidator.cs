using FluentValidation;

namespace WarehouseService.Application.Features.StockTransactions.Query.GetAll;

public class GetStockTransactionsQueryValidator : AbstractValidator<GetStockTransactionsQuery>
{
    public GetStockTransactionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Transaction type is invalid.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Transaction status is invalid.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("ToDate must be greater than or equal to FromDate.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
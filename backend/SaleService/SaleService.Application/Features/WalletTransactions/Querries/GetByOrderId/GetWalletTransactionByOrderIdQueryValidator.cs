using FluentValidation;
namespace SalesService.Application.Features.WalletTransactions.Querries.GetByOrderId;

public sealed class GetWalletTransactionByOrderIdQueryValidator :
    AbstractValidator<GetWalletTransactionByOrderIdQuery>
{
    public GetWalletTransactionByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
           .NotEmpty()
           .WithMessage("OrderId is required to search.");
    }
}

using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;

public record CreateWithdrawalCommand(
    Guid UserId,
    decimal Amount,
    string? BankCode,
    string BankName,
    string AccountName,
    string AccountNumber,
    string? Notes
) : IRequest<Guid>;
using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;

public record CompleteWithdrawalCommand(
    Guid RequestId,
    Guid AdminId,
    byte[] ReceiptFile,
    string BankTransactionId
) : IRequest<bool>;
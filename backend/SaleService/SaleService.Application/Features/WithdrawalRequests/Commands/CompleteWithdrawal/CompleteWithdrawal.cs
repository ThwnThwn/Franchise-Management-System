using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;

public record CompleteWithdrawalCommand(
    Guid RequestId,
    byte[]? ReceiptFile) : IRequest<bool>;
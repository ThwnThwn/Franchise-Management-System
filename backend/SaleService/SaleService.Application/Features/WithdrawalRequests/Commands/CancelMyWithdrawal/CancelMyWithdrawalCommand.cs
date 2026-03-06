using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CancelMyWithdrawal;

public record CancelMyWithdrawalCommand(
    Guid RequestId,
    Guid UserId, // Để check chủ sở hữu
    string Reason
) : IRequest<bool>;
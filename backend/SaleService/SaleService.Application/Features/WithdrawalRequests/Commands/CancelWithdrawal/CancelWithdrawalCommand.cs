using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;

public record CancelWithdrawalCommand(Guid RequestId, string Reason) : IRequest<bool>;
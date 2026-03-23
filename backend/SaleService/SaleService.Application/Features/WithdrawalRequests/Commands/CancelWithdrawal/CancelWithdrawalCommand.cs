using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;

public record CancelWithdrawalCommand(Guid RequestId, Guid AdminId, string Reason) : IRequest<bool>;
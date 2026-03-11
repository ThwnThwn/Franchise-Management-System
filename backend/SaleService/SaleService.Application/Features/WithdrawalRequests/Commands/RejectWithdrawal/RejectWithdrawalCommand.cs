using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;

public record RejectWithdrawalCommand(Guid RequestId, Guid AdminId, string RejectionReason) : IRequest<bool>;
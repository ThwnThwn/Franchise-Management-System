using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;

public record RejectWithdrawalCommand(Guid RequestId, string RejectionReason) : IRequest<bool>;
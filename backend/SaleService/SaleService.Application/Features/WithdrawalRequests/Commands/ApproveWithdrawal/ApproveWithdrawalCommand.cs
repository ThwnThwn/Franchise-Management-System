using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;

public record ApproveWithdrawalCommand(Guid RequestId, Guid AdminId, string ApprovalNotes) : IRequest<bool>;
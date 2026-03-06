using MediatR;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;

public record ApproveWithdrawalCommand(Guid RequestId, string ApprovalNotes) : IRequest<bool>;
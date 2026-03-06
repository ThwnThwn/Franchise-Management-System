using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;

public class ApproveWithdrawalHandler(IUnitOfWork uow, IWithdrawalRequestRepository repo)
    : IRequestHandler<ApproveWithdrawalCommand, bool>
{
    public async Task<bool> Handle(ApproveWithdrawalCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.RequestId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu rút tiền.");

        if (item.Status != WithdrawalStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể duyệt yêu cầu đang ở trạng thái chờ.");

        item.Status = WithdrawalStatus.Approved;
        item.ApprovalNotes = request.ApprovalNotes;

        await uow.CommitAsync(ct);
        return true;
    }
}
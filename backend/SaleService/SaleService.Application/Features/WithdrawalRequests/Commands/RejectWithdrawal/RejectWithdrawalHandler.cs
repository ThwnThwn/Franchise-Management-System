using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;

public class RejectWithdrawalHandler(
    IUnitOfWork uow,
    IWithdrawalRequestRepository repo,
    IWalletRepository walletRepo,
    IWalletTransactionRepository transactionRepo
) : IRequestHandler<RejectWithdrawalCommand, bool>
{
    public async Task<bool> Handle(RejectWithdrawalCommand request, CancellationToken ct)
    {
        await uow.BeginTransactionAsync(ct);
        try
        {
            var item = await repo.GetByIdAsync(request.RequestId, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu.");

            if (item.Status != WithdrawalStatus.Pending && item.Status != WithdrawalStatus.Approved)
                throw new InvalidOperationException("Không thể từ chối yêu cầu ở trạng thái này.");

            // 1. Cập nhật lệnh rút
            item.Status = WithdrawalStatus.Rejected;
            item.RejectionReason = request.RejectionReason;

            // 2. Hoàn tiền vào ví
            var wallet = await walletRepo.GetByUserIdAsync(item.UserId, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy ví.");

            if (wallet.RowVersion == null) throw new InvalidOperationException("Lỗi dữ liệu ví.");

            walletRepo.ApplyAddBalance(wallet, item.Amount, wallet.RowVersion);

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                UserId = item.UserId,
                Amount = item.Amount,
                TransactionType = TransactionType.Refund, // Loại: Hoàn tiền
                Status = TransactionStatus.Completed,
                Description = $"Hoàn tiền cho yêu cầu rút tiền bị từ chối: {item.RequestCode}",
                ReferenceId = item.Id, // Link tới lệnh rút
                BalanceBefore = wallet.Balance - item.Amount, // Balance cũ
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            };

            transactionRepo.Add(walletTransaction);

            await uow.CommitAsync(ct);
            return true;
        }
        catch
        {
            await uow.RollbackAsync(ct); throw;
        }
    }
}
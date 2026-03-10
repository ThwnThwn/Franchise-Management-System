using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;

public class CancelWithdrawalHandler(
    IUnitOfWork uow,
    IWithdrawalRequestRepository repo,
    IWalletRepository walletRepo,
    IWalletTransactionRepository transactionRepo
) : IRequestHandler<CancelWithdrawalCommand, bool>
{
    public async Task<bool> Handle(CancelWithdrawalCommand request, CancellationToken ct)
    {
        await uow.BeginTransactionAsync(ct);
        try
        {
            var item = await repo.GetByIdAsync(request.RequestId, ct) ?? throw new KeyNotFoundException("Không thấy yêu cầu.");

            if (item.Status != WithdrawalStatus.Approved && item.Status != WithdrawalStatus.Processing)
            {
                throw new InvalidOperationException("Chỉ có thể hủy yêu cầu rút tiền đang ở trạng thái Đã duyệt (Approved) hoặc Đang xử lý (Processing).");
            }

            repo.ApplyCancel(item, request.AdminId, request.Reason);

            var wallet = await walletRepo.GetByUserIdAsync(item.UserId, ct) ?? throw new KeyNotFoundException("Không thấy ví.");
            walletRepo.ApplyAddBalance(wallet, item.Amount, wallet.RowVersion!);
            await walletRepo.UpdateBalanceAsync(wallet.Id, wallet.Balance,
                wallet.RowVersion!, cancellationToken: ct);

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                TransactionCode = $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                UserId = item.UserId,
                Amount = item.Amount,
                TransactionType = TransactionType.Refund, // Loại: Hoàn tiền
                Status = TransactionStatus.Completed,
                Description = $"Hoàn tiền cho yêu cầu rút tiền bị hủy: {item.RequestCode}",
                ReferenceId = item.Id, // Link tới lệnh rút
                BalanceBefore = wallet.Balance - item.Amount, // Balance cũ
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            };

            transactionRepo.Add(walletTransaction);

            await uow.CommitAsync(ct);
            return true;
        }
        catch { await uow.RollbackAsync(ct); throw; }
    }
}
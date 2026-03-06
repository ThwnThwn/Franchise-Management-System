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

            if (item.Status == WithdrawalStatus.Completed || item.Status == WithdrawalStatus.Cancelled)
                throw new InvalidOperationException("Yêu cầu đã hoàn tất hoặc đã hủy trước đó.");

            item.Status = WithdrawalStatus.Cancelled;
            item.CancellationReason = request.Reason;

            var wallet = await walletRepo.GetByUserIdAsync(item.UserId, ct) ?? throw new KeyNotFoundException("Không thấy ví.");
            walletRepo.ApplyAddBalance(wallet, item.Amount, wallet.RowVersion!);

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
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
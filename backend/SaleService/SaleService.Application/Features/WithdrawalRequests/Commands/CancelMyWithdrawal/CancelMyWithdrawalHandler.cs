using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CancelMyWithdrawal;

public class CancelMyWithdrawalHandler(
    IUnitOfWork uow,
    IWalletRepository walletRepo,
    IWithdrawalRequestRepository withdrawalRepo,
    IWalletTransactionRepository transactionRepo
) : IRequestHandler<CancelMyWithdrawalCommand, bool>
{
    public async Task<bool> Handle(CancelMyWithdrawalCommand request, CancellationToken ct)
    {
        await uow.BeginTransactionAsync(ct);

        try
        {
            // 1. Lấy lệnh rút tiền và check chính chủ
            var withdrawal = await withdrawalRepo.GetByIdAsync(request.RequestId, ct);
            if (withdrawal == null || withdrawal.UserId != request.UserId)
                throw new KeyNotFoundException("Không tìm thấy yêu cầu rút tiền hoặc bạn không có quyền thực hiện.");

            if (withdrawal.Status != WithdrawalStatus.Pending)
                throw new InvalidOperationException("Chỉ có thể hủy yêu cầu đang ở trạng thái chờ xử lý.");

            // 2. Lấy ví
            var wallet = await walletRepo.GetByUserIdAsync(request.UserId, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy thông tin ví của người dùng.");

            if (wallet.RowVersion == null)
                throw new InvalidOperationException("Dữ liệu ví không hợp lệ (Thiếu Version).");

            // 3. Cập nhật trạng thái lệnh rút
            withdrawalRepo.ApplyCancel(withdrawal, request.UserId, request.Reason);

            // 4. Hoàn tiền vào ví
            walletRepo.ApplyAddBalance(wallet, withdrawal.Amount, wallet.RowVersion);
            walletRepo.ApplyUpdateBalance(wallet, wallet.Balance, wallet.RowVersion);

            // 5. TẠO BIẾN ĐỘNG SỐ DƯ (Transaction)
            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                WalletId = wallet.Id,
                UserId = request.UserId,
                Amount = withdrawal.Amount,
                TransactionType = TransactionType.Refund, // Loại: Hoàn tiền
                Status = TransactionStatus.Completed,
                Description = $"Hoàn tiền cho yêu cầu rút tiền bị hủy: {withdrawal.RequestCode}",
                ReferenceId = withdrawal.Id, // Link tới lệnh rút
                BalanceBefore = wallet.Balance - withdrawal.Amount, // Balance cũ
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            };

            transactionRepo.Add(walletTransaction); // Dùng hàm Add (không save) của Generic Repo

            // 6. Commit một phát ăn cả 3 bảng
            await uow.CommitAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await uow.RollbackAsync(ct);
            throw new Exception("Hệ thống đang bận xử lý giao dịch khác trên ví này, vui lòng thử lại sau.");
        }
        catch (InvalidOperationException)
        {
            await uow.RollbackAsync(ct);
            throw; // Re-throw các lỗi logic đã Việt hóa ở trên
        }
        catch (Exception)
        {
            await uow.RollbackAsync(ct);
            throw new Exception("Đã xảy ra lỗi không mong muốn khi hủy yêu cầu rút tiền.");
        }
    }
}
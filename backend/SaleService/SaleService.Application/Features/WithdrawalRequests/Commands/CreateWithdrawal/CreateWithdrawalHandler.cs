using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;

public class CreateWithdrawalHandler(
    IUnitOfWork uow,
    IWalletRepository walletRepo,
    IWithdrawalRequestRepository withdrawalRepo,
    IWalletTransactionRepository transactionRepo,
    IAesEncryptionService aesService
) : IRequestHandler<CreateWithdrawalCommand, Guid>
{
    public async Task<Guid> Handle(CreateWithdrawalCommand request, CancellationToken ct)
    {
        decimal amount = decimal.Parse(request.Amount);

        // 1. Kiểm tra spam (Nếu có lệnh Pending thì không cho tạo tiếp)
        var hasPending = await withdrawalRepo.HasPendingWithdrawalAsync(request.UserId, ct);
        if (hasPending)
            throw new InvalidOperationException("Bạn hiện đang có một yêu cầu rút tiền đang chờ xử lý.");

        // 2. Mở Transaction duy nhất
        await uow.BeginTransactionAsync(ct);

        try
        {
            // 3. Lấy ví và trừ tiền
            var wallet = await walletRepo.GetByUserIdAsync(request.UserId, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy thông tin ví người dùng.");

            // Kiểm tra RowVersion nullable để tránh NullReferenceException
            if (wallet.RowVersion == null)
                throw new InvalidOperationException("Dữ liệu ví không hợp lệ (Thiếu Version).");

            // 4. Trừ tiền (Sử dụng hàm Apply để batching lệnh SQL, check balance và concurrency bên trong)
            // Hàm này sẽ throw InvalidOperationException nếu không đủ tiền hoặc DbUpdateConcurrencyException nếu sai Version
            walletRepo.ApplyDeductBalance(wallet, amount, wallet.RowVersion);
            walletRepo.ApplyUpdateBalance(wallet, wallet.Balance, wallet.RowVersion);

            // 5. Mã hóa số tài khoản ngân hàng
            var encryptedAcc = aesService.Encrypt(request.AccountNumber);
            var maskedAcc = aesService.GenerateMaskedAccountNumber(request.AccountNumber);

            // 6. Tạo Entity rút tiền
            var withdrawal = new WithdrawalRequest
            {
                Id = Guid.NewGuid(),
                RequestCode = $"WR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                UserId = request.UserId,
                WalletId = wallet.Id,
                Amount = amount,
                FeeAmount = 0,
                BankCode = request.BankCode,
                BankName = request.BankName,
                AccountName = request.AccountName,
                MaskedAccountNumber = maskedAcc,
                EncryptedAccountNumber = encryptedAcc,
                Status = WithdrawalStatus.Pending,
                Notes = request.Notes,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // 7. Thêm vào Tracker (Chưa Save)
            withdrawalRepo.Add(withdrawal);

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                WalletId = wallet.Id,
                UserId = request.UserId,
                Amount = -amount,
                TransactionType = TransactionType.Withdrawal,
                Status = TransactionStatus.Completed,
                Description = $"Rút tiền về ngân hàng {request.BankName}: {withdrawal.RequestCode}",
                ReferenceId = withdrawal.Id,
                BalanceBefore = wallet.Balance + amount,
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            };

            withdrawal.TransactionId = walletTransaction.Id;

            transactionRepo.Add(walletTransaction);

            await uow.CommitAsync(ct);

            return withdrawal.Id;
        }
        catch (DbUpdateConcurrencyException)
        {
            await uow.RollbackAsync(ct);
            throw new Exception("Hệ thống bận do có giao dịch cùng lúc, vui lòng thử lại sau.");
        }
        catch (InvalidOperationException)
        {
            await uow.RollbackAsync(ct);
            // Re-throw để giữ message tiếng Việt từ hàm Apply (như "Số dư không đủ")
            throw;
        }
        catch (Exception)
        {
            await uow.RollbackAsync(ct);
            // Log ex ở đây nếu cần
            throw new Exception("Đã xảy ra lỗi trong quá trình tạo yêu cầu rút tiền.");
        }
    }
}
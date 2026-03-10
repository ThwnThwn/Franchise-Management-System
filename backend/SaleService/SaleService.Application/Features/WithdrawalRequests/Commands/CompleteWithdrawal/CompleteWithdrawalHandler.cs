using MediatR;
using SalesService.Application.Interfaces;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;

public class CompleteWithdrawalHandler(
    IUnitOfWork uow,
    IWithdrawalRequestRepository repo,
    IImageService imageService
) : IRequestHandler<CompleteWithdrawalCommand, bool>
{
    public async Task<bool> Handle(CompleteWithdrawalCommand request, CancellationToken ct)
    {
        // 1. Lấy dữ liệu
        var item = await repo.GetByIdAsync(request.RequestId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu rút tiền.");

        // 2. Kiểm tra trạng thái (Có thể đưa check này vào trong ApplyComplete luôn cho an toàn)
        if (item.Status != WithdrawalStatus.Approved && item.Status != WithdrawalStatus.Processing)
            throw new InvalidOperationException("Chỉ có thể hoàn tất khi lệnh đã được duyệt hoặc đang xử lý.");

        string? receiptUrl = item.ReceiptImageUrl;
        string? receiptPublicId = item.ReceiptImagePublicId;

        // 3. Xử lý Upload ảnh (nếu có file mới)
        if (request.ReceiptFile is { Length: > 0 })
        {
            var fileName = $"receipt_{item.RequestCode}_{DateTimeOffset.UtcNow.Ticks}";
            var uploadResult = await imageService.UploadImageFromBytesAsync(
                request.ReceiptFile,
                fileName,
                "image/jpeg");

            receiptUrl = uploadResult.Url;
            receiptPublicId = uploadResult.PublicId;
        }

        var walletTransactionId = item.TransactionId ?? Guid.Empty;

        repo.ApplyComplete(
            request: item,
            processedBy: request.AdminId,
            transactionId: walletTransactionId,
            bankTransactionId: request.BankTransactionId,
            receiptImageUrl: receiptUrl,
            receiptImagePublicId: receiptPublicId
        );

        // 5. Lưu xuống DB
        await uow.CommitAsync(ct);
        return true;
    }
}
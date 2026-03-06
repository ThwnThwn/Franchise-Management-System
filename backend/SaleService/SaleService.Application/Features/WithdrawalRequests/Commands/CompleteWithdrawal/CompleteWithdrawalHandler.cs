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
        var item = await repo.GetByIdAsync(request.RequestId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy yêu cầu rút tiền.");

        if (item.Status != WithdrawalStatus.Approved && item.Status != WithdrawalStatus.Processing)
            throw new InvalidOperationException("Yêu cầu phải được duyệt trước khi hoàn tất.");

        // Upload ảnh bill nếu có
        if (request.ReceiptFile is { Length: > 0 })
        {
            // fileName ní có thể đặt theo RequestCode để dễ quản lý trên Cloudinary
            var fileName = $"receipt_{item.RequestCode}_{DateTime.UtcNow.Ticks}";

            var uploadResult = await imageService.UploadImageFromBytesAsync(
                request.ReceiptFile,
                fileName,
                "image/jpeg");

            item.ReceiptImageUrl = uploadResult.Url;
            item.ReceiptImagePublicId = uploadResult.PublicId;
        }

        item.Status = WithdrawalStatus.Completed;
        item.CompletedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync(ct);
        return true;
    }
}
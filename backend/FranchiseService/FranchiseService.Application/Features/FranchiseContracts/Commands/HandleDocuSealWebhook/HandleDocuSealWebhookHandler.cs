using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FranchiseService.Application.Features.FranchiseContracts.Commands.HandleDocuSealWebhook;

/// <summary>
/// Command: Xử lý Webhook từ DocuSeal khi có sự kiện ký văn bản.
/// Chỉ quan tâm đến event "form.completed".
/// </summary>
public class HandleDocuSealWebhookCommand : IRequest<bool>
{
    public string EventType { get; set; } = string.Empty;
    public int SubmissionId { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class HandleDocuSealWebhookHandler(
    IFranchiseContractRepository contractRepo,
    IDocuSealService docuSeal,
    IImageService imageService,
    ILogger<HandleDocuSealWebhookHandler> logger
) : IRequestHandler<HandleDocuSealWebhookCommand, bool>
{
    public async Task<bool> Handle(HandleDocuSealWebhookCommand request, CancellationToken cancellationToken)
    {
        // Chỉ xử lý khi form.completed (đối tác đã ký xong)
        // Các event khác (form.viewed, form.started) chỉ log lại để debug
        if (!request.EventType.Equals("form.completed", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("DocuSeal Webhook event '{EventType}' ignored.", request.EventType);
            return true; // trả về 200 OK để DocuSeal không retry
        }

        logger.LogInformation(
            "DocuSeal Webhook: form.completed for submission {SubmissionId}", request.SubmissionId);

        // 1. Tìm Contract theo SubmissionId (nếu không thấy → có thể là test webhook, bỏ qua)
        var contract = await contractRepo.GetByDocusealSubmissionAsync(request.SubmissionId, cancellationToken);
        if (contract == null)
        {
            logger.LogWarning(
                "DocuSeal Webhook: No contract found for submission {SubmissionId}. Ignoring.",
                request.SubmissionId);
            return true;
        }

        // 2. Idempotency: Nếu hợp đồng đã vượt qua AwaitingSignature rồi, bỏ qua
        if (contract.Status > ContractStatus.AwaitingSignature)
        {
            logger.LogInformation(
                "DocuSeal Webhook: Contract {ContractId} already past AwaitingSignature ({Status}). Skipping.",
                contract.Id, contract.Status);
            return true;
        }

        // 3. Lấy URL PDF đã ký
        //    Ưu tiên lấy từ Webhook payload trước, nếu không có thì gọi API lấy lại
        var pdfUrl = request.DocumentUrl
            ?? await docuSeal.GetSubmissionPdfUrlAsync(request.SubmissionId, cancellationToken);

        // 4. Download PDF từ DocuSeal và re-upload lên Cloudinary để có link ổn định (không expired)
        //    DocuSeal document_url thường là signed S3 URL có TTL ngắn
        if (!string.IsNullOrWhiteSpace(pdfUrl))
        {
            try
            {
                // Upload lên Cloudinary
                var uploadResult = await imageService.UploadFileFromUrlAsync(
                    url: pdfUrl,
                    fileName: $"Contract_{contract.Id}",
                    folder: "Franchise_Contracts");

                // Lưu permanent URL của Cloudinary thay vì temporary URL của DocuSeal
                contract.ContractPdfUrl = uploadResult.Url;
                logger.LogInformation(
                    "DocuSeal Webhook: Signed PDF uploaded to Cloudinary {Url} for contract {ContractId}",
                    uploadResult.Url, contract.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Could not re-upload contract PDF to permanent storage. Storing original DocuSeal URL instead.");
                contract.ContractPdfUrl = pdfUrl;
            }
        }

        // 5. Chuyển trạng thái: AwaitingSignature → AwaitingPayment
        //    Hợp đồng được ký nhưng chưa có hiệu lực cho đến khi thanh toán tiền cọc
        contract.Status = ContractStatus.AwaitingPayment;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await contractRepo.UpdateAsync(contract);

        logger.LogInformation(
            "Contract {ContractId} transitioned to AwaitingPayment after DocuSeal completion.",
            contract.Id);

        // TODO Phase 3: Gửi thông báo (Kafka/SignalR) cho Kế toán biết có hợp đồng chờ xác nhận thanh toán

        return true;
    }
}
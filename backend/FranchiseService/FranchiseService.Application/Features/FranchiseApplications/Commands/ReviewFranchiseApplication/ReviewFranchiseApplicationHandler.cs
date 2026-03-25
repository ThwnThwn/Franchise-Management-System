using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Application.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.ReviewFranchiseApplication;

public class ReviewFranchiseApplicationHandler(
    IFranchiseApplicationRepository appRepo,
    IFranchiseContractRepository contractRepo,
    IDocuSealService docuSeal,
    ILogger<ReviewFranchiseApplicationHandler> logger,
    IOptions<FranchiseSettings> franchiseSettings
) : IRequestHandler<ReviewFranchiseApplicationCommand, ReviewFranchiseApplicationResult>
{
    private readonly FranchiseSettings _franchiseSettings = franchiseSettings.Value;

    public async Task<ReviewFranchiseApplicationResult> Handle(
        ReviewFranchiseApplicationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Lấy hồ sơ
        var application = await appRepo.GetByIdAsync(request.ApplicationId)
            ?? throw new KeyNotFoundException($"Không tìm thấy hồ sơ với ID: {request.ApplicationId}");

        if (application.Status != FranchiseApplicationStatus.Pending)
            throw new InvalidOperationException($"Hồ sơ đang ở trạng thái '{application.Status}', không thể duyệt lại.");

        // ── TRƯỜNG HỢP: TỪ CHỐI ─────────────────────────────────────────────
        if (request.Decision == FranchiseApplicationStatus.Rejected)
        {
            application.Status = FranchiseApplicationStatus.Rejected;
            application.AdminNote = request.AdminNote;
            application.ReviewedAt = DateTimeOffset.UtcNow;
            application.ReviewedBy = request.ReviewedBy;
            application.UpdatedAt = DateTimeOffset.UtcNow;
            await appRepo.UpdateAsync(application);

            return new ReviewFranchiseApplicationResult
            {
                Success = true,
                Message = "Hồ sơ đã bị từ chối."
            };
        }

        // ── TRƯỜNG HỢP: DUYỆT ───────────────────────────────────────────────
        if (request.Decision != FranchiseApplicationStatus.Approved)
            throw new InvalidOperationException("Decision không hợp lệ. Chỉ chấp nhận Approved hoặc Rejected.");

        if (request.DocuSealTemplateId <= 0)
            throw new InvalidOperationException("DocuSealTemplateId chưa được cấu hình.");

        // 2. Cập nhật Application → Approved
        application.Status = FranchiseApplicationStatus.Approved;
        application.AdminNote = request.AdminNote;
        application.ReviewedAt = DateTimeOffset.UtcNow;
        application.ReviewedBy = request.ReviewedBy;
        application.UpdatedAt = DateTimeOffset.UtcNow;
        await appRepo.UpdateAsync(application);

        // 3. Tự động điền giá trị mặc định nếu Admin không cung cấp
        var validFrom = request.ValidFrom ?? DateTimeOffset.UtcNow;
        var validUntil = request.ValidUntil ?? validFrom.AddYears(_franchiseSettings.DefaultDurationYears);
        var revenueShare = request.RevenueSharePercentage ?? _franchiseSettings.DefaultRevenueSharePercentage;
        var depositAmount = request.DepositAmount ?? _franchiseSettings.DefaultDepositAmount;
        var franchiseFee = request.FranchiseFee ?? _franchiseSettings.DefaultFranchiseFee;

        logger.LogInformation(
            "Contract defaults applied — RevenueShare: {RS}%, Deposit: {Deposit}, Fee: {Fee}, Duration: {From} → {Until}",
            revenueShare, depositAmount, franchiseFee, validFrom, validUntil);

        // 4. Tạo FranchiseContract (Draft trước, chờ DocuSeal xác nhận)
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Code = $"FR-{DateTimeOffset.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            ApplicationId = request.ApplicationId,
            DepositAmount = depositAmount,
            FranchiseFee = franchiseFee,
            RevenueSharePercentage = revenueShare,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            Status = ContractStatus.Draft,
            DepositStatus = DepositStatus.AwaitingPayment,
            SetupStatus = ContractSetupStatus.Design,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await contractRepo.AddAsync(contract);

        // 4. Gọi DocuSeal API để tạo Submission và gửi email link ký
        int? submissionId = null;
        try
        {
            var metadata = BuildDocuSealMetadata(application, contract);

            var docuSealResult = await docuSeal.CreateSubmissionAsync(
                templateId: request.DocuSealTemplateId,
                signerEmail: application.Email,
                signerName: application.FullName,
                metadata: metadata,
                cancellationToken: cancellationToken);

            submissionId = docuSealResult.Id;

            // 5. Lưu DocuSeal Submission ID và chuyển trạng thái Contract
            contract.DocusealSubmissionId = submissionId;
            contract.Status = ContractStatus.AwaitingSignature;
            await appRepo.UpdateAsync(application);
            contract.UpdatedAt = DateTimeOffset.UtcNow;
            await contractRepo.UpdateAsync(contract);

            logger.LogInformation(
                "DocuSeal submission {SubmissionId} created for contract {ContractId}",
                submissionId, contract.Id);
        }
        catch (Exception ex)
        {
            // Trả về Success = false kèm lỗi chi tiết để dễ debug
            application.Status = FranchiseApplicationStatus.Pending;
            contract.Status = ContractStatus.Draft;
            await contractRepo.UpdateAsync(contract);
            logger.LogError(ex,
                "Failed to create DocuSeal submission for contract {ContractId}.",
                contract.Id);

            return new ReviewFranchiseApplicationResult
            {
                Success = false,
                Message = $"Lỗi kết nối DocuSeal: {ex.Message}"
            };
        }

        return new ReviewFranchiseApplicationResult
        {
            Success = true,
            ContractId = contract.Id,
            DocuSealSubmissionId = submissionId,
            Message = $"Hồ sơ đã duyệt. Hợp đồng đã được gửi đến {application.Email} để ký."
        };
    }

    private static Dictionary<string, string> BuildDocuSealMetadata(
        FranchiseApplication app,
        FranchiseContract contract)
    {
        return new Dictionary<string, string>
        {
            ["contract_number"] = contract.Code.ToString(),
            ["franchise_name"] = app.FullName,
            ["franchise_email"] = app.Email,
            ["franchise_phone"] = app.Phone,
            ["franchise_identity"] = app.IdentityNumber,
            ["store_address"] = app.StoreAddress,
            ["franchise_fee"] = contract.FranchiseFee.HasValue
                ? $"{contract.FranchiseFee.Value:N0} VNĐ"
                : "Theo thỏa thuận",
            ["deposit_amount"] = contract.DepositAmount.HasValue
                ? $"{contract.DepositAmount.Value:N0} VNĐ"
                : "Theo thỏa thuận",
            ["revenue_share"] = contract.RevenueSharePercentage.HasValue
                ? $"{contract.RevenueSharePercentage.Value}%"
                : "Theo thỏa thuận",
            ["valid_from"] = contract.ValidFrom?.ToString("dd/MM/yyyy") ?? "",
            ["valid_until"] = contract.ValidUntil?.ToString("dd/MM/yyyy") ?? "",
            ["sign_date"] = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy"),
        };
    }
}
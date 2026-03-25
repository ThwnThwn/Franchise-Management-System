using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.ReviewFranchiseApplication;

/// <summary>
/// Command: Admin/Giám đốc duyệt hoặc từ chối hồ sơ nhượng quyền.
///
/// Nếu Approve:
///   1. Cập nhật Application.Status = Approved
///   2. Tạo FranchiseContract mới với các thông số tài chính
///   3. Gọi DocuSeal → CreateSubmission → gửi email link ký cho đối tác
///   4. Contract.Status = AwaitingSignature
///
/// Nếu Reject:
///   1. Cập nhật Application.Status = Rejected + ghi lý do vào AdminNote
/// </summary>
public class ReviewFranchiseApplicationCommand : IRequest<ReviewFranchiseApplicationResult>
{
    public Guid ApplicationId { get; set; }
    public Guid ReviewedBy { get; set; }

    /// <summary>Approve hoặc Reject</summary>
    public FranchiseApplicationStatus Decision { get; set; }

    public string? AdminNote { get; set; }

    // ── Chỉ cần khi Decision = Approved ──────────────────────────────────────

    // Lấy config qua IOptions<DocuSealSettings> từ Controller truyền xuống
    public int DocuSealTemplateId { get; set; }

    public decimal? DepositAmount { get; set; }
    public decimal? FranchiseFee { get; set; }
    public decimal? RevenueSharePercentage { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
}

public class ReviewFranchiseApplicationResult
{
    public bool Success { get; set; }
    public Guid? ContractId { get; set; }
    /// <summary>DocuSeal Submission ID — lưu để track webhook.</summary>
    public int? DocuSealSubmissionId { get; set; }
    public string Message { get; set; } = string.Empty;
}

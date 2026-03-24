using FranchiseService.Domain.Commons;
using FranchiseService.Domain.Enums;

namespace FranchiseService.Domain.Models;

public partial class FranchiseContract : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    // --------------- Basic Info ---------------
    public string Code { get; set; } = null!;

    // --------------- Tích hợp DocuSeal ---------------

    /// <summary>
    /// Submission ID từ DocuSeal — dùng để xử lý Webhook khi nhận callback "Ký xong".
    /// </summary>
    public int? DocusealSubmissionId { get; set; }

    /// <summary>
    /// Signed URL file PDF hợp đồng đã ký (upload lên S3/Cloudinary sau khi DocuSeal completed).
    /// </summary>
    public string? ContractPdfUrl { get; set; }

    // --------------- Thông số tài chính ---------------

    public decimal? DepositAmount { get; set; }

    public DepositStatus DepositStatus { get; set; } = DepositStatus.AwaitingPayment;

    public decimal? FranchiseFee { get; set; }

    public decimal? RevenueSharePercentage { get; set; }

    // --------------- Trạng thái hợp đồng (State Machine) ---------------

    /// <summary>
    /// Draft → Approved → AwaitingSignature → AwaitingPayment → Active → (Suspended / Terminated / Expired)
    /// </summary>
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidUntil { get; set; }

    // --------------- Tiến độ thi công (Store Onboarding) ---------------

    /// <summary>
    /// Design → Construction → Inspection → Ready
    /// </summary>
    public ContractSetupStatus SetupStatus { get; set; } = ContractSetupStatus.Design;

    // --------------- Audit ---------------

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    // --------------- Navigation Properties ---------------

    public virtual FranchiseApplication Application { get; set; } = null!;
}

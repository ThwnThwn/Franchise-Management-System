using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public partial class WithdrawalRequest : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string RequestCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal FeeAmount { get; set; }

    public string? BankCode { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public string MaskedAccountNumber { get; set; } = null!;

    public string EncryptedAccountNumber { get; set; } = null!;

    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public Guid? TransactionId { get; set; }

    public WithdrawalStatus Status { get; set; }

    public string? Notes { get; set; }

    public string? InternalNotes { get; set; }

    public string? RejectionReason { get; set; }

    public string? ApprovalNotes { get; set; }

    public string? CancellationReason { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public Guid? CancelledBy { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? ReceiptImageUrl { get; set; }

    public string? ReceiptImagePublicId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual WalletTransaction? Transaction { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
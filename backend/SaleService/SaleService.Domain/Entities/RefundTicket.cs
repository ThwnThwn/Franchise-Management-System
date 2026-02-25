using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities
{
    public partial class RefundTicket : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid OriginalTransactionId { get; set; }
        public RefundTicketStatus Status { get; set; }
        public Guid? ProcessedBy { get; set; }
        public string? RejectionReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        // Fields needed for ApproveRefundAsync
        public Guid? ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        // Fields needed for RejectRefundAsync
        public Guid? RejectedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }
        public string? RejectionNotes { get; set; }
        
        public Guid? CancelledBy { get; set; }
        public string? CancellationReason { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
        // Additional useful fields
        public decimal RefundAmount { get; set; }
        public string? BankName { get; set; }
        public string? Notes { get; set; } 
        public RefundReasonType? RefundReason { get; set; } = null;
        public RefundTicketMethod? RefundTicketMethod { get; set; }

        public string? InternalNotes { get; set; }
        public Guid? CreatedByInternalUserId { get; set; }
    }
}
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
        public string? ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        // Fields needed for RejectRefundAsync
        public string? RejectedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }

        // Fields needed for CompleteRefundAsync
        public DateTimeOffset? ProcessedDate { get; set; }

        // Additional useful fields
        public decimal RefundAmount { get; set; } // Để phân biệt với Amount nếu có phí xử lý
        public string? RefundMethod { get; set; } // e.g., "Wallet", "BankTransfer", "Cash"
        public string? BankAccountNumber { get; set; } // Nếu hoàn tiền qua bank
        public string? BankName { get; set; }
        public string? Notes { get; set; } 
    }
}
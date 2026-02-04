using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities
{
    public partial class WalletTransaction : SoftDeletedEntity, IEntity
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public string? Notes { get; set; } 
        public decimal Amount { get; set; } 
        public decimal? BalanceBefore { get; set; }
        public decimal? BalanceAfter { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; }
        public Guid? ReferenceId { get; set; }
        public WalletReferenceType? ReferenceType { get; set; } 
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; } 
        public DateTimeOffset? FailedAt { get; set; }
        public string? FailureReason { get; set; }
        public string? Metadata { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public virtual Wallet? Wallet { get; set; }
        public virtual Order? Order { get; set; }
        public virtual WalletTransaction? ParentTransaction { get; set; }
        public virtual ICollection<WalletTransaction>? ChildTransactions { get; set; }
    }
}
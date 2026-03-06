using System.ComponentModel.DataAnnotations;
using SalesService.Domain.Commons;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public partial class Wallet : SoftDeletedEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public WalletStatus Status { get; set; }

    [ConcurrencyCheck]
    public byte[]? RowVersion { get; set; } = default;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    public virtual ICollection<RefundTicket> RefundTickets { get; set; } = new List<RefundTicket>();

    public virtual ICollection<WalletEvent> WalletEvents { get; set; } = new List<WalletEvent>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}

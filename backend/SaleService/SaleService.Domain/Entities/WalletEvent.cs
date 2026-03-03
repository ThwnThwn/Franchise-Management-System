using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public class WalletEvent
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public Guid? WalletTransactionId { get; set; }  
    public WalletEventType EventType { get; set; }   
    public decimal Amount { get; set; }
    public WalletEventStatus Status { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
    public string? AdminNote { get; set; }         
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }       
}
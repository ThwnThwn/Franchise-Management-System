using SalesService.Domain.Commons;
using SalesService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesService.Domain.Entities;

public partial class OrderTracking : IEntity
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    
    public OrderStatus? Status { get; set; }
    
    public string Action { get; set; } = string.Empty;
    
    public string? Note { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }
}

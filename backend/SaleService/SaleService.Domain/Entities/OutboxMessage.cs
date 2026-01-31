using SalesService.Domain.Commons;

namespace SalesService.Domain.Entities;

public class OutboxMessage : IEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!; // e.g., "OrderCreatedEvent"
    public string Content { get; set; } = default!; // JSON payload
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

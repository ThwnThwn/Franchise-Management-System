using SharedKernel.Abstractions;
using InventoryService.Domain.Commons;

namespace InventoryService.Domain.Entities;

public class OutboxMessage : IEntity, IOutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Event Type
    public string Content { get; set; } = string.Empty; // JSON Payload
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }

    // IEntity implementation
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Explicit implementation for IOutboxMessage to bridge type difference without DB migration
    DateTimeOffset IOutboxMessage.OccurredOn
    {
        get => string.IsNullOrEmpty(Content) ? DateTimeOffset.UtcNow : new DateTimeOffset(OccurredOn, TimeSpan.Zero);
        set => OccurredOn = value.UtcDateTime;
    }
}


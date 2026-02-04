using SharedKernel.Abstractions;
using System.Text.Json;

namespace SharedKernel.Infrastructure.Extensions;

public static class OutboxExtensions
{
    /// <summary>
    /// Extension method giúp thêm sự kiện vào Outbox một cách ngắn gọn.
    /// Tự động Serialize nội dung và khởi tạo các thông tin cơ bản.
    /// </summary>
    /// <typeparam name="TEvent">Loại sự kiện (Payload).</typeparam>
    /// <typeparam name="TOutboxMessage">Loại Entity Outbox (phải implement IOutboxMessage và có default constructor).</typeparam>
    /// <param name="repository">Repository Outbox.</param>
    /// <param name="event">Đối tượng sự kiện cần lưu.</param>
    /// <param name="eventType">Tên loại sự kiện (Optional - mặc định lấy Name của Class).</param>
    public static void AddEvent<TEvent, TOutboxMessage>(
        this IOutboxRepository<TOutboxMessage> repository,
        TEvent @event,
        string? eventType = null)
        where TOutboxMessage : IOutboxMessage, new()
    {
        var jsonContent = JsonSerializer.Serialize(@event);
        
        var message = new TOutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = eventType ?? typeof(TEvent).Name,
            Content = jsonContent,
            OccurredOn = DateTimeOffset.UtcNow
        };

        repository.Add(message);
    }
}

using SalesService.Domain.Entities;

namespace SalesService.Application.Persisitences.Outbox; // Updated namespace

/// <summary>
/// Interface Repository dùng để lưu trữ các Outbox Message.
/// Giúp tách biệt (decouple) tầng Application khỏi DbContext cụ thể.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Thêm một Outbox message mới vào DbContext.
    /// Lưu ý: Hàm này chưa lưu xuống DB (SaveChanges) ngay, mà sẽ lưu cùng Transaction của Handler.
    /// </summary>
    /// <param name="message">Tin nhắn cần lưu.</param>
    void Add(OutboxMessage message);
}

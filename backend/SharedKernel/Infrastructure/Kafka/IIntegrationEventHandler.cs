using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Interface định nghĩa contract cho các xử lý sự kiện tích hợp (Integration Events) từ Kafka.
/// </summary>
/// <typeparam name="TIntegrationEvent">Loại sự kiện cần xử lý.</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent>
{
    /// <summary>
    /// Xử lý sự kiện khi nhận được tin nhắn từ Kafka.
    /// </summary>
    /// <param name="event">Dữ liệu sự kiện.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task Handle(TIntegrationEvent @event);
}

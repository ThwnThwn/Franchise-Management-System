namespace ProductService.Application.Interfaces;

public interface IIntegrationEventBus
{
    // Topic: Tên hàng đợi (Kafka topic)
    // @event: Kiện hàng (Record/Class)
    Task PublishAsync<T>(string topic, T @event, CancellationToken ct = default) where T : class;
}
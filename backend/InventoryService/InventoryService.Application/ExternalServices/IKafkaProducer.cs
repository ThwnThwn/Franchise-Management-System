namespace InventoryService.Application.ExternalServices;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message) where T : class;
}

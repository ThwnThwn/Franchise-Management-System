using Confluent.Kafka;
using SaleService.Application.ExternalServices;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace SaleService.Infrastructure.ExternalServices;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var messageJson = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = messageJson
        });
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}

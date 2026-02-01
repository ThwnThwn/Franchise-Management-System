using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using InventoryService.Application.ExternalServices;

namespace InventoryService.Infrastructure.ExternalServices;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var messageValue = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = messageValue };

        try
        {
            var result = await _producer.ProduceAsync(topic, kafkaMessage);
            _logger.LogInformation("Message sent to topic {Topic}: {Message}", topic, messageValue);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error producing message to topic {Topic}", topic);
            throw;
        }
    }
}

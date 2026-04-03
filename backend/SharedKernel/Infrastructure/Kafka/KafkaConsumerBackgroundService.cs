using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Infrastructure.Kafka;

/// <summary>
/// Background Service chịu trách nhiệm subscribe và consume tin nhắn từ Kafka một cách generic.
/// - Tự động Subscribe các topic có trong Registry.
/// - Tự động Deserialize tin nhắn thành Event Type tương ứng.
/// - Tự động tạo Scope và gọi Handler xử lý logic nghiệp vụ.
/// - Hỗ trợ refresh metadata tự động (qua KAFKA_METADATA_REFRESH_MS) để handle topic recreation.
/// </summary>
public class KafkaConsumerBackgroundService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerBackgroundService> _logger;
    private readonly KafkaConsumerRegistry _registry;

    public KafkaConsumerBackgroundService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerBackgroundService> logger,
        KafkaConsumerRegistry registry)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _registry = registry;

        var configValue = configuration["KAFKA_METADATA_REFRESH_MS"]
            ?? throw new InvalidOperationException("KAFKA_METADATA_REFRESH_MS is not configured.");
        var refreshInterval = int.Parse(configValue);
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092",
            GroupId = configuration["KAFKA_GROUP_ID"] ?? "default-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            TopicMetadataRefreshIntervalMs = refreshInterval
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var topics = _registry.GetAllTopics();
        _consumer.Subscribe(topics);
        _logger.LogInformation("Kafka Consumer started. Listening to topics: {Topics}", string.Join(", ", topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                await ProcessMessageAsync(result.Topic, result.Message.Value);
                _consumer.Commit(result);
                _consumer.StoreOffset(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in consumer loop");
            }
        }

        _consumer.Close();
    }

    private async Task ProcessMessageAsync(string topic, string messageJson)
    {
        if (!_registry.TryGet(topic, out var eventType, out var handlerType))
        {
            _logger.LogWarning("No handler registered for topic {Topic}", topic);
            return;
        }
        _logger.LogWarning("RAW MESSAGE: {Json}", messageJson);
        try
        {
            var @event = JsonSerializer.Deserialize(messageJson, eventType);
            if (@event == null)
            {
                _logger.LogWarning("Failed to deserialize message from {Topic}", topic);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);

            // Invoke Handle method dynamically using reflection since types are known only at runtime
            var method = handlerType.GetMethod("Handle")!;
            var task = (Task)method.Invoke(handler, [@event])!;
            await task;

            _logger.LogInformation("Successfully processed message from {Topic} using {Handler}", topic, handlerType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing business logic for topic {Topic}", topic);
        }
    }
}
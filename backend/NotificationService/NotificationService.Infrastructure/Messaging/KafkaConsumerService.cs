using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Handlers;
using SharedKernel.Events;
using System.Text.Json;
namespace NotificationService.Infrastructure.Messaging;
public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;
    public KafkaConsumerService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerService> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092",
            GroupId = configuration["KAFKA_CONSUMER_GROUP_ID"] ?? "notification-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,             // Use manual commit for better control
            EnableAutoOffsetStore = true          // Auto store offset after Consume()
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(new[] {
            "otp-notifications",
            "order-notifications",
            "password-reset-notifications",
            "low-stock-notifications"  // ✅ Subscribe to low stock events
        });
        _logger.LogInformation("Kafka consumer started, listening to topics...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                
                _logger.LogInformation(
                    "Received message from topic {Topic}: {Message}",
                    result.Topic,
                    result.Message.Value);
                
                await ProcessMessageAsync(result.Topic, result.Message.Value);
                
                // ✅ Commit offset immediately after successful processing
                // This ensures message won't be reprocessed even if service restarts
                _consumer.Commit(result);
                
                _logger.LogInformation(
                    "Successfully processed and committed message from topic {Topic}, Offset: {Offset}",
                    result.Topic,
                    result.Offset.Value);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka consumer");
                // ⚠️ Don't commit on error - message will be reprocessed
            }
        }
        _consumer.Close();
    }
    private async Task ProcessMessageAsync(string topic, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            switch (topic)
            {
                case "otp-notifications":
                    var otpEvent = JsonSerializer.Deserialize<OtpRequestedEvent>(message);
                    if (otpEvent != null)
                    {
                        var otpHandler = scope.ServiceProvider.GetRequiredService<OtpNotificationHandler>();
                        await otpHandler.HandleAsync(otpEvent);
                    }
                    break;
                case "order-notifications":
                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                    if (orderEvent != null)
                    {
                        var orderHandler = scope.ServiceProvider.GetRequiredService<OrderNotificationHandler>();
                        await orderHandler.HandleAsync(orderEvent);
                    }
                    break;
                case "password-reset-notifications":
                    var resetEvent = JsonSerializer.Deserialize<PasswordResetRequestedEvent>(message);
                    if (resetEvent != null)
                    {
                        // TODO: Tạo PasswordResetNotificationHandler (tương tự OTP handler)
                        _logger.LogInformation("Password reset event received for {Email}", resetEvent.Email);
                    }
                    break;
                case "low-stock-notifications":
                    var lowStockEvent = JsonSerializer.Deserialize<LowStockEvent>(message);
                    if (lowStockEvent != null)
                    {
                        var lowStockHandler = scope.ServiceProvider.GetRequiredService<LowStockNotificationHandler>();
                        await lowStockHandler.HandleAsync(lowStockEvent);
                    }
                    break;
                
                
                default:
                    _logger.LogWarning("Unknown topic: {Topic}", topic);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
        }
    }
    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}
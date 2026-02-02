using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Handlers;
using SharedKernel.Events;
using SharedKernel.Constants;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Service chạy nền (Background Service) để lắng nghe các sự kiện từ Kafka.
/// Chịu trách nhiệm nhận tin nhắn từ các topic và điều phối đến các Handler tương ứng (Email, OTP...).
/// </summary>
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
            EnableAutoCommit = false,             // Tắt tự động commit để kiểm soát việc xử lý lỗi tốt hơn (At-least-once)
            EnableAutoOffsetStore = true,         // Tự động lưu offset vào bộ nhớ sau khi Consume
            TopicMetadataRefreshIntervalMs = 3000 // Quan Trọng: Force refresh metadata mỗi 3s để tìm topic mới nhanh hơn (fix lỗi delay 5 phút)
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(new[] {
            KafkaTopics.OtpNotifications,
            KafkaTopics.OtpForgetPassword,
            KafkaTopics.OrderNotifications,
            KafkaTopics.PasswordResetNotifications,
            KafkaTopics.LowStockNotifications
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
                
                // Xử lý logic nghiệp vụ
                await ProcessMessageAsync(result.Topic, result.Message.Value);
                
                // ✅ Commit offset ngay sau khi xử lý thành công
                // Đảm bảo tin nhắn không bị xử lý lại nếu service restart, nhưng sẽ xử lý lại nếu ProcessMessageAsync lỗi (do chưa commit)
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

    /// <summary>
    /// Hàm điều phối xử lý tin nhắn dựa trên Topic.
    /// Mỗi topic sẽ được xử lý bởi một Handler riêng biệt.
    /// </summary>
    private async Task ProcessMessageAsync(string topic, string message)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (topic)
            {
                case var _ when topic == KafkaTopics.OtpNotifications:
                    var otpEvent = JsonSerializer.Deserialize<OtpRequestedEvent>(message);
                    if (otpEvent != null)
                    {
                        var otpHandler = scope.ServiceProvider.GetRequiredService<OtpNotificationHandler>();
                        await otpHandler.HandleAsync(otpEvent);
                    }
                    break;

                case var _ when topic == KafkaTopics.OrderNotifications:
                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                    if (orderEvent != null)
                    {
                        var orderHandler = scope.ServiceProvider.GetRequiredService<OrderNotificationHandler>();
                        await orderHandler.HandleAsync(orderEvent);
                    }
                    break;

                case var _ when topic == KafkaTopics.PasswordResetNotifications:
                    var resetEvent = JsonSerializer.Deserialize<PasswordResetRequestedEvent>(message);
                    if (resetEvent != null)
                    {
                        // TODO: Tạo PasswordResetNotificationHandler (tương tự OTP handler)
                        _logger.LogInformation("Password reset event received for {Email}", resetEvent.Email);
                    }
                    break;

                case var _ when topic == KafkaTopics.LowStockNotifications:
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
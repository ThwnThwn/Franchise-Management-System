using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using SharedKernel.Constants;
using SharedKernel.Events;

namespace InventoryService.API.Consumers;

/// <summary>
/// Background Service lắng nghe các sự kiện từ Kafka liên quan đến Đơn hàng (Order).
/// Chịu trách nhiệm xử lý các nghiệp vụ kho sau khi đơn hàng thay đổi trạng thái (Thanh toán, Hủy).
/// </summary>
public class OrderEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<OrderEventConsumer> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092",
            GroupId = "inventory-service-group", // Group riêng cho Inventory
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = true,
            TopicMetadataRefreshIntervalMs = 3000
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Vòng lặp chính của Consumer, chạy liên tục để nhận và xử lý tin nhắn từ Kafka.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Đảm bảo việc khởi tạo consumer không chặn luồng chính khi khởi động (tránh deadlock).
        _consumer.Subscribe(new[] {
            KafkaTopics.OrderPaid,
            KafkaTopics.OrderCancelled
        });

        _logger.LogInformation("Inventory OrderEventConsumer started, listening to topics...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                _logger.LogInformation("Received message from topic {Topic}", result.Topic);

                await ProcessMessageAsync(result.Topic, result.Message.Value);

                _consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in consumer");
            }
        }

        _consumer.Close();
    }

    /// <summary>
    /// Xử lý tin nhắn cụ thể nhận được từ Kafka.
    /// Phân loại topic và gọi logic nghiệp vụ tương ứng (Trừ kho, Nhả hàng...).
    /// </summary>
    private async Task ProcessMessageAsync(string topic, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var storeIngredientRepo = scope.ServiceProvider.GetRequiredService<IStoreIngredientRepository>();

        try
        {
            if (topic == KafkaTopics.OrderPaid)
            {
                var paidEvent = JsonSerializer.Deserialize<OrderPaidEvent>(message);
                if (paidEvent != null)
                {
                    _logger.LogInformation("Processing OrderPaidEvent for Order {OrderId}", paidEvent.OrderId);

                    // Refactor: Chuyển logic sang Application Layer (ConfirmStockReservationHandler)
                    var command = new Application.Features.StoreIngredient.Commands.ConfirmStockReservation.ConfirmStockReservationCommand(
                        paidEvent.OrderId, 
                        paidEvent.StoreId, 
                        paidEvent.Items);
                    
                    await mediator.Send(command);
                }
            }
            else if (topic == KafkaTopics.OrderCancelled)
            {
                var cancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(message);
                if (cancelledEvent != null)
                {
                    _logger.LogInformation("Processing OrderCancelledEvent for Order {OrderId}", cancelledEvent.OrderId);
                    // Chỉ cần xóa khỏi hàng đợi (Nhả hàng)
                    await storeIngredientRepo.RemoveQueueByOrderIdAsync(cancelledEvent.OrderId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {Topic}", topic);
            throw; // Ném lại lỗi để tránh Commit offset nếu muốn implement cơ chế retry sau này
        }
    }
}

using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Application.Persistences.Repositories;
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
        var storeIngredientRepo = scope.ServiceProvider.GetRequiredService<IStoreIngredientRepository>();

        // Đối với OrderPaid, có thể cần IInventoryDbContext để dùng Transaction nếu muốn chuẩn ACID tuyệt đối.
        // Nhưng Repository đã implement DecreaseStockAtomicallyAsync dùng SQL nên đã atomic cho từng item.


        // xử lý từng line item một.
        try
        {
            if (topic == KafkaTopics.OrderPaid)
            {
                var paidEvent = JsonSerializer.Deserialize<OrderPaidEvent>(message);
                if (paidEvent != null)
                {
                    _logger.LogInformation("Processing OrderPaidEvent for Order {OrderId}", paidEvent.OrderId);

                    // 1. Xóa khỏi Hàng đợi (Giải phóng giữ chỗ)
                    await storeIngredientRepo.RemoveQueueByOrderIdAsync(paidEvent.OrderId);

                    // 2. Trừ tồn kho vật lý
                    foreach (var item in paidEvent.Items)
                    {
                        // Để trừ kho vật lý, ta cần IngredientId.
                        // Nhưng OrderPaidEvent (từ SaleService) thường chỉ có VariantId.
                        // Do đó cần map: VariantId -> IngredientId(s).
                        // Việc này cần IRecipeRepository.

                        var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
                        // Đoạn này có thể tối ưu: Lấy hết recipe 1 lần thay vì gọi loop nhiều lần.
                        var recipes = await recipeRepo.GetRecipesByVariantIdsAsync(new[] { item.VariantId }, CancellationToken.None);

                        foreach (var recipe in recipes)
                        {
                            var amountToDeduct = recipe.Quantity * item.Quantity;
                            await storeIngredientRepo.DecreaseStockAtomicallyAsync(
                                paidEvent.StoreId,
                                recipe.IngredientId,
                                amountToDeduct);
                        }
                    }
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

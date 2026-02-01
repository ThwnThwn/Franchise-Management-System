using InventoryService.Application.ClientInterface.Indentity;
using InventoryService.Application.ExternalServices;
using InventoryService.Application.Signals;
using InventoryService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Events;
using System.Text.Json;

using InventoryService.Application.Dtos.User; // Added

namespace InventoryService.API.BackgroundJobs;

/// <summary>
/// Worker xử lý Outbox của Inventory Service.
/// Nhiệm vụ: Đọc event "LowStockDetected" -> Lấy Email Manager (gRPC) -> Bắn Kafka "LowStockEvent".
/// </summary>
public class InventoryOutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<InventoryOutboxWorker> _logger;

    public InventoryOutboxWorker(
        IServiceProvider serviceProvider, 
        IOutboxSignal outboxSignal, 
        ILogger<InventoryOutboxWorker> logger)
    {
        _serviceProvider = serviceProvider ;
        _outboxSignal = outboxSignal;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InventoryOutboxWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Inventory Outbox messages.");
            }
            // Chờ đợt signal tiếp theo (không polling 2s nữa)
            await _outboxSignal.WaitForPulseAsync(stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IInventoryDbContext>();
        var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();
        var identityClient = scope.ServiceProvider.GetRequiredService<IIdentityServiceClient>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(10) // Batch size nhỏ để xử lý nhanh
            .ToListAsync(stoppingToken);

        if (!messages.Any()) return;

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == "LowStockDetected")
                {
                    // 1. Deserialize nội dung thô
                    var payload = JsonSerializer.Deserialize<LowStockEventPayload>(message.Content);
                    
                    if (payload != null)
                    {
                        // 2. Gọi gRPC lấy Email Manager (Có thể retry nếu lỗi mạng)
                        string managerEmail = string.Empty;
                        try
                        {
                             var manager = await identityClient.GetManager(new GetManagerReq { StoreId = payload.StoreId });
                             managerEmail = manager?.Email ?? string.Empty;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Failed to fetch manager email for Store {StoreId}: {Message}", payload.StoreId, ex.Message);
                            // Tùy logic: Có thể skip hoặc retry sau. Ở đây tạm thời log warning và vẫn bắn event (ko có email)
                        }

                        if (!string.IsNullOrEmpty(managerEmail))
                        {
                            // 3. Tạo Event Kafka chính thức
                            var lowStockEvent = new LowStockEvent
                            {
                                StoreId = payload.StoreId,
                                IngredientId = payload.IngredientId,
                                IngredientName = payload.IngredientName,
                                CurrentStock = payload.CurrentStock,
                                MinThreshold = payload.MinThreshold,
                                ManagerEmail = managerEmail
                            };

                            // 4. Bắn Kafka
                            await kafkaProducer.PublishAsync("low-stock-notifications", lowStockEvent);
                            _logger.LogInformation("Published LowStockEvent for Ingredient {Id} to Manager {Email}", payload.IngredientId, managerEmail);
                        }
                        else
                        {
                             _logger.LogWarning("Skipping Kafka publish because Manager Email not found for Store {StoreId}", payload.StoreId);
                        }
                    }
                }

                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to process OutboxMessage {Id}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    // Helper class để deserialize payload từ Outbox
    private class LowStockEventPayload
    {
        public Guid StoreId { get; set; }
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinThreshold { get; set; }
    }
}

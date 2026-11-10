using InventoryService.Application.ClientInterface;
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Constants;
using InventoryService.Application.Signals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Events;
using System.Text.Json;

using InventoryService.Application.Dtos.User;
using InventoryService.Application.Abstractions; // Added

namespace InventoryService.API.BackgroundJobs;

/// <summary>
/// Worker xử lý Outbox của Inventory Service.
/// Nhiệm vụ: Đọc event từ Outbox -> Dispatch generic qua KafkaProducerRegistry -> Kafka.
/// </summary>
public class InventoryOutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<InventoryOutboxWorker> _logger;
    private readonly KafkaProducerRegistry _registry;

    public InventoryOutboxWorker(
        IServiceProvider serviceProvider, 
        IOutboxSignal outboxSignal, 
        ILogger<InventoryOutboxWorker> logger,
        KafkaProducerRegistry registry)
    {
        _serviceProvider = serviceProvider ;
        _outboxSignal = outboxSignal;
        _logger = logger;
        _registry = registry;
    }

    /// <summary>
    /// Vòng lặp chính của Background Service.
    /// Chờ tín hiệu từ OutboxSignal để xử lý tin nhắn mới.
    /// </summary>
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
            // Chờ đợt signal tiếp theo
            await _outboxSignal.WaitForPulseAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Xử lý batch các tin nhắn trong bảng Outbox.
    /// - Lấy batch 10 tin chưa xử lý.
    /// - Dựa vào Registry để tìm ra Topic và EventType.
    /// - Deserialize tin nhắn và Publish lên Kafka.
    /// </summary>
    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IInventoryDbContext>();
        var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(10) // Batch size nhỏ
            .ToListAsync(stoppingToken);

        if (!messages.Any()) return;

        foreach (var message in messages)
        {
            try
            {
                // 1. Resolve Type and Topic from Registry
                if (!_registry.TryGet(message.Type, out var eventType, out var topic))
                {
                    _logger.LogWarning("Unknown message type: {Type}", message.Type);
                    // Có thể đánh dấu lỗi để không retry vô hạn? Hoặc log warning rồi skip
                }
                else
                {
                    // 2. Deserialize generic
                    var payload = JsonSerializer.Deserialize(message.Content, eventType);
                    
                    if (payload != null)
                    {
                        // 3. Publish generic
                        await kafkaProducer.PublishAsync(topic, payload);
                        _logger.LogInformation("OutboxWorker published {MessageType} {MessageId} to {Topic}", message.Type, message.Id, topic);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize message {Id} of type {Type}", message.Id, message.Type);
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
}

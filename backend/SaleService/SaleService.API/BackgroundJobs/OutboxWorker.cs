using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SalesService.Application.Signals;
using SalesService.Infrastructure.ApplicationDbContext;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.API.BackgroundJobs;

/// <summary>
/// Background Service chịu trách nhiệm xử lý bảng Outbox và gửi tin nhắn lên Kafka.
/// - Sử dụng Pattern: Transactional Outbox.
/// - Cơ chế Trigger: Pulse Signal (giúp gửi tin Real-time mà không cần Polling liên tục).
/// - Cơ chế Dispatch: Sử dụng KafkaProducerRegistry để map động từ Event Type -> Topic.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly KafkaProducerRegistry _registry;

    public OutboxWorker(
        IServiceProvider serviceProvider,
        IOutboxSignal outboxSignal,
        ILogger<OutboxWorker> logger,
        KafkaProducerRegistry registry)
    {
        _serviceProvider = serviceProvider;
        _outboxSignal = outboxSignal;
        _logger = logger;
        _registry = registry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages.");
            }
            await _outboxSignal.WaitForPulseAsync(stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ISalesDbContext>();
        var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                // 1. Resolve Type and Topic from Registry
                if (!_registry.TryGet(message.Type, out var eventType, out var topic))
                {
                    _logger.LogWarning("Unknown message type: {Type}", message.Type);
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
                }

                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
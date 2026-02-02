using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Kafka;
using SharedKernel.Constants;
using SalesService.Infrastructure.ApplicationDbContext;
using SharedKernel.Events;
using System.Text.Json;
using SalesService.Application.Signals;

namespace SalesService.API.BackgroundJobs;

/// <summary>
/// Background Service chịu trách nhiệm xử lý bảng Outbox.
/// Sử dụng IOutboxSignal để phản ứng ngay lập tức khi có tin nhắn mới, đảm bảo gửi tin gần như Real-time.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxSignal _outboxSignal;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceProvider serviceProvider, IOutboxSignal outboxSignal, ILogger<OutboxWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _outboxSignal = outboxSignal;
        _logger = logger;
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

        if (!messages.Any()) return;

        foreach (var message in messages)
        {
            try
            {
                string topic = message.Type switch
                {
                    "OrderCreatedEvent" => KafkaTopics.OrderNotifications,
                    _ => "default-topic"
                };

                if (message.Type == "OrderCreatedEvent")
                {
                     var payload = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Content);
                     if(payload != null)
                     {
                         await kafkaProducer.PublishAsync(topic, payload);
                         _logger.LogInformation("OutboxWorker published message {MessageId} to {Topic}", message.Id, topic);
                     }
                }
                else 
                {
                    _logger.LogWarning("Unknown message type: {Type}", message.Type);
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

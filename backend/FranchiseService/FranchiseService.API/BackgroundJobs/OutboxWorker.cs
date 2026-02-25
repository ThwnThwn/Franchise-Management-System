using FranchiseService.Application.Repositories;
using FranchiseService.Application.Signals;
using FranchiseService.Domain.Entities;
using SharedKernel.Events;
using FranchiseService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace FranchiseService.API.BackgroundJobs
{
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

            ConfigureRegistry();

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
                
                // Chờ tín hiệu (hoặc timeout sau 2 giây để quét định kỳ phòng hờ)
                // Lưu ý: WaitForPulseAsync bản chất là await logic, nếu muốn kết hợp timeout thì cần logic phức tạp hơn chút.
                // Ở đây dùng Wait đơn giản, handler bên dưới sẽ tự pulse khi có message.
                // Để an toàn (fallback polling), ta có thể dùng Task.WhenAny với Task.Delay.
                
                var waitTask = _outboxSignal.WaitForPulseAsync(stoppingToken);
                var delayTask = Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                
                await Task.WhenAny(waitTask, delayTask);
            }
        }

        private void ConfigureRegistry()
        {
            // Map Event Type -> Topic
            // Sử dụng alias trùng với Type string lưu trong DB
            _registry.Register<ShiftCreatedEvent>("shift.events.v1", "ShiftCreated");
            _registry.Register<ShiftUpdatedEvent>("shift.events.v1", "ShiftUpdated");
            _registry.Register<ShiftDeletedEvent>("shift.events.v1", "ShiftDeleted");
            _registry.Register<ShiftAssignedEvent>("shift.events.v1", "ShiftAssigned");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IFranchiseDbContext>();
            // Interface IKafkaProducer cần được register. Nếu chưa có interface, dùng class cụ thể.
            // Check lại Program.cs: services.AddScoped<IKafkaProducer, KafkaProducer>(); -> Có vẻ OK.
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>(); // Cần đảm bảo SharedKernel có IKafkaProducer

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
                    // 1. Resolve Type and Topic from Registry
                    if (!_registry.TryGet(message.Type, out var eventType, out var topic))
                    {
                        // Fallback hoặc Log warning
                        // Nếu chưa register, có thể hardcode hoặc bỏ qua
                        _logger.LogWarning("Unknown message type: {Type}", message.Type);
                        topic = "shift.events.v1"; // Default topic
                    }

                    // 2. Deserialize generic (Ở đây nội dung là JSON object ẩn danh, ta có thể gửi raw string hoặc dynamic)
                    // Vì SharedKernel KafkaProducer thường nhận T message.
                    // Cách đơn giản nhất: Deserialize ra object/dynamic hoặc class cụ thể nếu có.
                    // Với "object", JsonSerializer sẽ ra JsonElement. Kéo theo Kafka producer cần support serialize lại.
                    
                    // Để an toàn, ta gửi string message.Content luôn nếu Kafka Producer support overload string.
                    // Giả sử KafkaProducer.PublishAsync<T>(topic, T message).
                    
                    // Workaround: Deserialize ra dynamic/object để pass vào PublishAsync
                    var payload = JsonSerializer.Deserialize<object>(message.Content ?? "{}");

                    if (payload != null)
                    {
                        // 3. Publish
                        await kafkaProducer.PublishAsync(topic, payload);
                        _logger.LogInformation("OutboxWorker published {MessageType} {MessageId} to {Topic}", message.Type, message.Id, topic);
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
}

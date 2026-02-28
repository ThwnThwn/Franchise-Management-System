using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace NotificationService.Application.EventHandlers;

/// <summary>
/// Xử lý sự kiện "UserRegisteredEvent".
/// Nhiệm vụ:
/// - Gọi EngagementService để tạo Loyalty Account.
/// - Lưu log kết quả vào NotificationLog.
/// </summary>
public class CreateLoyaltyAccountHandler: IIntegrationEventHandler<UserRegisteredEvent>
{
    private readonly Application.ExternalServices.IEngagementServiceClient _engagementServiceClient;
    private readonly INotificationRepository _repository;
    private readonly ILogger<CreateLoyaltyAccountHandler> _logger;

    public CreateLoyaltyAccountHandler(
        Application.ExternalServices.IEngagementServiceClient engagementServiceClient,
        INotificationRepository repository,
        ILogger<CreateLoyaltyAccountHandler> logger)
    {
        _engagementServiceClient = engagementServiceClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent @event)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Loyalty, // or create Loyalty type
            Recipient = @event.CustomerId.ToString(),
            Subject = "Create Loyalty Account",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(log);

        try
        {
            await _engagementServiceClient.CreateLoyaltyAccount(@event.CustomerId);

            log.Status = NotificationStatus.Sent;
            log.SentAt = DateTime.UtcNow;

            _logger.LogInformation("Loyalty account created for UserId: {UserId}",@event.CustomerId);
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.RetryCount++;

            _logger.LogError(ex,"Failed to create loyalty account for UserId: {UserId}",@event.CustomerId);

            throw; // let Kafka retry
        }

        await _repository.UpdateAsync(log);
    }
}
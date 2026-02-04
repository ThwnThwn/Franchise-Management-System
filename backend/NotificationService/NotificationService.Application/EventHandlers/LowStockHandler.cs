using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace NotificationService.Application.EventHandlers;

/// <summary>
/// Xử lý sự kiện "Sắp hết hàng" (LowStockEvent).
/// Nhiệm vụ:
/// - Tìm email Manager của cửa hàng (nếu thiếu).
/// - Gửi email cảnh báo nhập hàng.
/// </summary>
public class LowStockHandler : IIntegrationEventHandler<LowStockEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly Application.ExternalServices.IIdentityServiceClient _identityServiceClient;
    private readonly ILogger<LowStockHandler> _logger;

    public LowStockHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        Application.ExternalServices.IIdentityServiceClient identityServiceClient,
        ILogger<LowStockHandler> logger)
    {
        _emailSender = emailSender;
        _repository = repository;
        _identityServiceClient = identityServiceClient;
        _logger = logger;
    }

    public async Task<string?> GetManagerEmail(Guid storeId)
    {
        var email = await _identityServiceClient.GetManagerEmailByStoreIdAsync(storeId);
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Manager email not found for StoreId: {StoreId}", storeId);
        }
        return email;
    }

    public async Task Handle(LowStockEvent @event)
    {
        // 1. Enrich: Fetch Manager Email
        if (string.IsNullOrEmpty(@event.ManagerEmail))
        {
             var email = await GetManagerEmail(@event.StoreId);
             if (!string.IsNullOrEmpty(email))
             {
                 @event.ManagerEmail = email;
             }
        }

        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.LowStock,
            Recipient = @event.ManagerEmail,
            Subject = "Low Stock Alert",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(log);

        try
        {
            await _emailSender.SendLowStockEmailAsync(@event.ManagerEmail, @event);
            
            log.Status = NotificationStatus.Sent;
            log.SentAt = DateTime.UtcNow;
            _logger.LogInformation("Low stock notification sent to {Email}", @event.ManagerEmail);
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.RetryCount++;
            _logger.LogError(ex, "Failed to send low stock notification to {Email}", @event.ManagerEmail);
        }

        await _repository.UpdateAsync(log);
    }
}

using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using System.Text.Json;

namespace NotificationService.Application.Handlers;

/// <summary>
/// Xử lý sự kiện "Sắp hết hàng" (LowStockEvent).
/// Nhiệm vụ: Gửi email cảnh báo cho quản lý kho.
/// </summary>
public class LowStockNotificationHandler
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly ILogger<LowStockNotificationHandler> _logger;

    public LowStockNotificationHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        ILogger<LowStockNotificationHandler> logger)
    {
        _emailSender = emailSender;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Log lại sự kiện và gửi email cảnh báo ngay lập tức.
    /// </summary>
    public async Task HandleAsync(LowStockEvent @event)
    {
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

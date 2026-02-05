using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System.Text.Json;

namespace NotificationService.Application.EventHandlers;

/// <summary>
/// Xử lý sự kiện "Đơn hàng mới" (OrderCreatedEvent).
/// Nhiệm vụ chính: Lấy email khách hàng (nếu thiếu) và gửi email xác nhận.
/// </summary>
public class OrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly Application.ExternalServices.IIdentityServiceClient _identityServiceClient;
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        Application.ExternalServices.IIdentityServiceClient identityServiceClient,
        ILogger<OrderCreatedHandler> logger)
    {
        _emailSender = emailSender;
        _repository = repository;
        _identityServiceClient = identityServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Xử lý sự kiên:
    /// 1. Kiểm tra Email trong event. Nếu có ID mà chưa có Email -> Gọi IdentityService lấy Email.
    /// 2. Lưu log vào DB.
    /// 3. Gửi Email.
    /// 4. Cập nhật trạng thái log.
    /// </summary>
    public async Task Handle(OrderCreatedEvent @event)
    {
        // 0. Idempotency Check (Chống trùng lặp)
        if (await _repository.HasProcessedAsync(@event.OrderId.ToString()))
        {
            _logger.LogWarning("Duplicate event detected for OrderId: {OrderId}. Skipping.", @event.OrderId);
            return;
        }

        // 1. Fetch Email if missing
        if (string.IsNullOrEmpty(@event.CustomerEmail) && @event.CustomerId.HasValue)
        {
            _logger.LogInformation("CustomerEmail is empty. Fetching from IdentityService for CustomerId: {CustomerId}", @event.CustomerId);
            var email = await _identityServiceClient.GetUserEmailByIdAsync(@event.CustomerId.Value.ToString());
            if (!string.IsNullOrEmpty(email))
            {
                @event.CustomerEmail = email;
            }
            else
            {
                _logger.LogWarning("Could not find email for CustomerId: {CustomerId}", @event.CustomerId);
            }
        }

        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.OrderCreated,
            Recipient = @event.CustomerEmail,
            Subject = "Order Confirmation",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
            ReferenceId = @event.OrderId.ToString(), // Lưu lại OrderId để check duplicate
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(log);
        try
        {
            await _emailSender.SendOrderConfirmationEmailAsync(@event.CustomerEmail, @event);
            
            log.Status = NotificationStatus.Sent;
            log.SentAt = DateTime.UtcNow;
            _logger.LogInformation("Order confirmation sent to {Email}", @event.CustomerEmail);
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.RetryCount++;
            _logger.LogError(ex, "Failed to send order confirmation to {Email}", @event.CustomerEmail);
        }
        await _repository.UpdateAsync(log);
    }
}

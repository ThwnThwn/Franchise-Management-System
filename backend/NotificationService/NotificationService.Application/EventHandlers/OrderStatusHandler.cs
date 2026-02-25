using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Model;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using static SharedKernel.Events.OrderStatusEvent;

namespace NotificationService.Application.EventHandlers;

/// <summary>
/// Xử lý sự kiện "Đơn hàng mới" (OrderCreatedEvent).
/// Nhiệm vụ chính: Lấy email khách hàng (nếu thiếu) và gửi email xác nhận.
/// </summary>
public class OrderStatusHandler : IIntegrationEventHandler<OrderStatusEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly Application.ExternalServices.IIdentityServiceClient _identityServiceClient;
    private readonly ILogger<OrderStatusHandler> _logger;

    public OrderStatusHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        Application.ExternalServices.IIdentityServiceClient identityServiceClient,
        ILogger<OrderStatusHandler> logger)
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
    public async Task Handle(OrderStatusEvent @event)
    {
        // Skip notification cho đơn hàng chưa thanh toán
        if (@event.NewOrderStatus == OrderStatus.PaymentPending)
        {
            _logger.LogInformation("Skip notification for PaymentPending order {OrderId}", @event.OrderId);
            return;
        }

        // Check dno resend same email (idempotency)
        var referenceId = @event.OrderId.ToString();
        var notificationType = MapNotificationType(@event.NewOrderStatus);

        // IDENTITY CHECK (chống resend khi restart)
        if (await _repository.HasProcessedAsync(referenceId, notificationType))
        {
            _logger.LogInformation(
                "Notification {Type} already sent for OrderId {OrderId}",
                notificationType,
                referenceId);
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
            Type = notificationType,
            Recipient = @event.CustomerEmail,
            Subject = "Order Status Updated",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
            ReferenceId = @event.OrderId.ToString(), // Lưu lại OrderId để check duplicate
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(log);
        if (string.IsNullOrEmpty(@event.CustomerEmail))
        {
            _logger.LogWarning("Skip sending email because CustomerEmail is empty");
            return;
        }
        try
        {
            switch (@event.NewOrderStatus)
            {
                case OrderStatus.ConfirmationPending:
                    var items = MapToOrderPlacedEmail(@event);
                    await _emailSender.SendOrderPlacedEmailAsync(@event.CustomerEmail, items);
                    break;

                case OrderStatus.Confirmed:
                case OrderStatus.Preparing:
                case OrderStatus.ReadyForPickup:
                case OrderStatus.Delivering:
                case OrderStatus.Completed:
                case OrderStatus.Cancelled:
                    await _emailSender.SendOrderStatusEmailAsync(@event.CustomerEmail, @event);
                    break;
            }
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
    private static OrderPlacedEmailModel MapToOrderPlacedEmail(OrderStatusEvent @event)
    {
        return new OrderPlacedEmailModel
        {
            OrderId = @event.OrderId,
            OrderCode = @event.OrderCode,
            TotalAmount = @event.TotalAmount,
            Tax = @event.Tax,
            Items = @event.Items == null
                ? new List<NotificationService.Application.Model.CartItemDto>()
                : MapCartItems(@event.Items)
        };
    }
    private static List<NotificationService.Application.Model.CartItemDto> MapCartItems(
    List<SharedKernel.Events.OrderStatusEvent.CartItemDto> items)
    {
        return items.Select(i => new NotificationService.Application.Model.CartItemDto
        {
            Sku = i.Sku,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
        }).ToList();
    }
    private static NotificationType MapNotificationType(OrderStatus status)
    {
        return status switch
        {
            // PaymentPending: đơn chưa thanh toán, không nên nhận event này
            // nhưng handle defensive để tránh poison message
            OrderStatus.PaymentPending => NotificationType.OrderCreated,
            OrderStatus.ConfirmationPending => NotificationType.OrderCreated,
            OrderStatus.Confirmed => NotificationType.OrderConfirmed,
            OrderStatus.Preparing => NotificationType.OrderPreparing,
            OrderStatus.ReadyForPickup => NotificationType.OrderReadyForPickup,
            OrderStatus.Delivering => NotificationType.OrderDelivering,
            OrderStatus.Completed => NotificationType.OrderCompleted,
            OrderStatus.Cancelled => NotificationType.OrderCancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}

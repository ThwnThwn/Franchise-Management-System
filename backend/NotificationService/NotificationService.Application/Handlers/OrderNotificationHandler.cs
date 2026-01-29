using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using System.Text.Json;
namespace NotificationService.Application.Handlers;
public class OrderNotificationHandler
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly ILogger<OrderNotificationHandler> _logger;
    public OrderNotificationHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        ILogger<OrderNotificationHandler> logger)
    {
        _emailSender = emailSender;
        _repository = repository;
        _logger = logger;
    }
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.OrderCreated,
            Recipient = @event.CustomerEmail,
            Subject = "Order Confirmation",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
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
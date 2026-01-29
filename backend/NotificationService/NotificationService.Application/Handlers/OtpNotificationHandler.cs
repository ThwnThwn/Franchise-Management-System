using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Events;
using System.Text.Json;
namespace NotificationService.Application.Handlers;
public class OtpNotificationHandler
{
    private readonly IEmailSender _emailSender;
    private readonly INotificationRepository _repository;
    private readonly ILogger<OtpNotificationHandler> _logger;
    public OtpNotificationHandler(
        IEmailSender emailSender,
        INotificationRepository repository,
        ILogger<OtpNotificationHandler> logger)
    {
        _emailSender = emailSender;
        _repository = repository;
        _logger = logger;
    }
    public async Task HandleAsync(OtpRequestedEvent @event)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Otp,
            Recipient = @event.Email,
            Subject = "OTP Verification Code",
            Status = NotificationStatus.Pending,
            Metadata = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(log);
        try
        {
            await _emailSender.SendOtpEmailAsync(@event.Email, @event.OtpCode);
            
            log.Status = NotificationStatus.Sent;
            log.SentAt = DateTime.UtcNow;
            _logger.LogInformation("OTP sent to {Email}", @event.Email);
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.RetryCount++;
            _logger.LogError(ex, "Failed to send OTP to {Email}", @event.Email);
        }
        await _repository.UpdateAsync(log);
    }
}
using SharedKernel.Events;
namespace NotificationService.Application.Abstractions;
public interface IEmailSender
{
    Task SendOtpEmailAsync(string toEmail, string otp);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendOrderConfirmationEmailAsync(string toEmail, OrderCreatedEvent orderEvent);
    Task SendLowStockEmailAsync(string toEmail, LowStockEvent lowStockEvent);
}